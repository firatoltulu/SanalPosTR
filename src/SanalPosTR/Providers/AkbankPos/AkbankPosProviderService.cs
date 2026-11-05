using Microsoft.AspNetCore.Http;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SanalPosTR.Providers.AkbankPos
{
    public class AkbankPosProviderService : BaseProviderService, IProviderService
    {
        private const string ApprovedResponseCode = "VPS-0000";
        private const string SaleTxnCode = "1000";
        private const string RefundTxnCode = "1002";
        private const string SecureSaleTxnCode = "3000";
        private const string FullSecureMdStatus = "1";
        private const int DefaultCurrencyCode = 949;

        // Bankanın form hash tanımındaki sıra; boş alanlar hash'e boş string olarak girer ama formda gönderilmez.
        private static readonly string[] SecureFormFields =
        {
            "paymentModel", "txnCode", "merchantSafeId", "terminalSafeId", "orderId", "lang", "amount",
            "ccbRewardAmount", "pcbRewardAmount", "xcbRewardAmount", "currencyCode", "installCount",
            "okUrl", "failUrl", "emailAddress", "mobilePhone", "homePhone", "workPhone", "subMerchantId",
            "creditCard", "expiredDate", "cvv", "cardHolderName", "randomNumber", "requestDateTime",
            "b2bIdentityNumber", "merchantData", "merchantBranchNo", "mobileEci", "walletProgramData",
            "mobileAssignedId", "mobileDeviceType"
        };

        // Test ortamında doğrulanan gövde biçimi: '+', '=' ve Türkçe karakterler kaçışlanmadan gidiyor.
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public AkbankPosProviderService(SanalPosHttpClient httpClient) : base(httpClient)
        {
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => Definition.BankConfiguration[CurrentBank];

        private AkbankPosConfiguration Configuration => (AkbankPosConfiguration)ProviderConfiguration;

        private AkbankPosEndPointConfiguration EndPoint => (AkbankPosEndPointConfiguration)EndPointConfiguration;

        public override string EmbededDirectory => string.Empty;

        public override PostForm GetPostForm()
        {
            PostForm postForm = new PostForm();
            postForm.ContentType = "application/json";
            postForm.RequestFormat = RequestDataFormat.Json;
            postForm.PreTag = string.Empty;
            postForm.SendParameterType = SendParameterType.RequestBody;
            return postForm;
        }

        #endregion Base

        #region Pay

        public override async Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            var order = paymentModel.Order;

            Log.Information("ProcessPayment started. Provider: {Provider}, Use3DSecure: {Use3DSecure}, OrderId: {OrderId}", GetType().Name, paymentModel.Use3DSecure, order.OrderId);

            if (paymentModel.Use3DSecure)
            {
                var redirectResult = new PaymentResult
                {
                    Status = true,
                    IsRedirectContent = true,
                    ServerResponseRaw = Build3DForm(paymentModel)
                };

                Log.Information("ProcessPayment completed (3DSecure redirect). Provider: {Provider}, OrderId: {OrderId}", GetType().Name, order.OrderId);

                return redirectResult;
            }

            var card = paymentModel.CreditCard;
            var request = new
            {
                version = "1.00",
                txnCode = SaleTxnCode,
                requestDateTime = CreateRequestDateTime(),
                randomNumber = CreateRandomNumber(),
                terminal = CreateTerminal(),
                order = order.OrderId.IsEmpty() ? null : new { orderId = order.OrderId },
                card = new { cardNumber = card.CardNumber, cvv2 = card.CVV2, expireDate = FormatExpireDate(card) },
                reward = new { ccbRewardAmount = "0.00", pcbRewardAmount = "0.00", xcbRewardAmount = "0.00" },
                transaction = CreateTransaction(order),
                customer = order.EMail.IsEmpty() && order.IP.IsEmpty()
                    ? null
                    : new { emailAddress = NullIfEmpty(order.EMail), ipAddress = NullIfEmpty(order.IP) }
            };

            var result = await SendRequest(request, EndPointConfiguration.ApiEndPoint);

            Log.Information("ProcessPayment completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}", GetType().Name, order.OrderId, result.Status);

            return result;
        }

        public override async Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            var orderId = paymentModel.Order.OrderId;

            Log.Information("VerifyPayment started. Provider: {Provider}, OrderId: {OrderId}", GetType().Name, orderId);

            if (!Validate3D(collection))
                return Reject(orderId, "HASH", "3D doğrulama yanıtının imzası geçersiz");

            var callbackOrderId = collection["orderId"].ToString();
            if (callbackOrderId != orderId)
                return Reject(orderId, "ORDER", $"3D doğrulama yanıtı başka bir siparişe ait: {callbackOrderId}");

            var responseCode = collection["responseCode"].ToString();
            if (responseCode != ApprovedResponseCode)
                return Reject(orderId, responseCode, collection["responseMessage"].ToString());

            // Yarım 3D (mdStatus 4) bilinçli olarak kabul edilmiyor; NestPay'deki 1-4 aralığından daha dar.
            var mdStatus = collection["mdStatus"].ToString();
            if (mdStatus != FullSecureMdStatus)
                return Reject(orderId, mdStatus, $"3D doğrulama tamamlanmadı (mdStatus: {mdStatus})");

            var request = new
            {
                version = "1.00",
                txnCode = SaleTxnCode,
                requestDateTime = CreateRequestDateTime(),
                randomNumber = CreateRandomNumber(),
                terminal = CreateTerminal(),
                order = new { orderId },
                transaction = CreateTransaction(paymentModel.Order),
                secureTransaction = new
                {
                    secureId = collection["secureId"].ToString(),
                    secureEcomInd = collection["secureEcomInd"].ToString(),
                    secureData = collection["secureData"].ToString(),
                    secureMd = collection["secureMd"].ToString()
                }
            };

            var result = await SendRequest(request, EndPointConfiguration.SecureReturnEndPoint);

            Log.Information("VerifyPayment completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}", GetType().Name, orderId, result.Status);

            return result;
        }

        /// <summary>
        /// 3D callback hash'ini, bankanın <c>hashParams</c> alanında bildirdiği alan sırasıyla doğrular.
        /// </summary>
        public bool Validate3D(IFormCollection collection)
        {
            var hashParams = collection["hashParams"].ToString();
            if (hashParams.IsEmpty())
                return false;

            var plain = string.Concat(hashParams
                .Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(key => collection[key].ToString()));

            return HashHelper.GetHMACSHA512Base64(plain, Configuration.SecretKey) == collection["hash"].ToString();
        }

        public override async Task<PaymentResult> ProcessRefound(Refund refund)
        {
            Log.Information("ProcessRefund started. Provider: {Provider}, OrderId: {OrderId}", GetType().Name, refund.OrderId);

            var request = new
            {
                version = "1.00",
                txnCode = RefundTxnCode,
                requestDateTime = CreateRequestDateTime(),
                randomNumber = CreateRandomNumber(),
                terminal = CreateTerminal(),
                order = new { orderId = refund.OrderId },
                transaction = refund.RefundAmount > 0
                    ? new { amount = FormatAmount(refund.RefundAmount), currencyCode = ParseCurrencyCode(refund.CurrencyCode) }
                    : null
            };

            var result = await SendRequest(request, EndPointConfiguration.RefundEndPoint);

            Log.Information("ProcessRefund completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}", GetType().Name, refund.OrderId, result.Status);

            return result;
        }

        private string Build3DForm(PaymentModel paymentModel)
        {
            var configuration = Configuration;
            var order = paymentModel.Order;
            var card = paymentModel.CreditCard;

            var values = new Dictionary<string, string>
            {
                { "paymentModel", "3D" },
                { "txnCode", SecureSaleTxnCode },
                { "merchantSafeId", configuration.MerchantSafeId },
                { "terminalSafeId", configuration.TerminalSafeId },
                { "orderId", order.OrderId },
                { "lang", "TR" },
                { "amount", FormatAmount(order.Total) },
                { "currencyCode", ParseCurrencyCode(order.CurrencyCode).ToString(CultureInfo.InvariantCulture) },
                { "installCount", GetInstallCount(order).ToString(CultureInfo.InvariantCulture) },
                { "okUrl", configuration.SiteSuccessUrl.CompileOrderLink(paymentModel) },
                { "failUrl", configuration.SiteFailUrl.CompileOrderLink(paymentModel) },
                { "emailAddress", order.EMail },
                { "creditCard", card.CardNumber },
                { "expiredDate", FormatExpireDate(card) },
                { "cvv", card.CVV2 },
                { "cardHolderName", card.CardHolderName },
                { "randomNumber", CreateRandomNumber() },
                { "requestDateTime", CreateRequestDateTime() }
            };

            var fields = SecureFormFields
                .Select(name => new KeyValuePair<string, string>(name, values.TryGetValue(name, out var value) ? value ?? string.Empty : string.Empty))
                .ToList();

            var hash = HashHelper.GetHMACSHA512Base64(string.Concat(fields.Select(entry => entry.Value)), configuration.SecretKey);

            // Sayfa charset belirtilmeden basılıyor; Türkçe karakterli alan UTF-8 dışında gönderilirse banka hash'i tutmaz.
            var html = new StringBuilder();
            html.Append("<form id=\"SanalPosTR\" name=\"SanalPosTR\" method=\"post\" accept-charset=\"UTF-8\" style=\"display:none;\" action=\"")
                .Append(WebUtility.HtmlEncode($"{EndPoint.GatewayBaseUrl.TrimEnd('/')}/{EndPoint.SecureEndPointApi}"))
                .Append("\">");

            foreach (var field in fields.Where(entry => !entry.Value.IsEmpty()))
                AppendHiddenInput(html, field.Key, field.Value);

            AppendHiddenInput(html, "hash", hash);

            return html.Append("</form>").ToString();
        }

        private static void AppendHiddenInput(StringBuilder html, string name, string value)
        {
            html.Append("<input type=\"hidden\" name=\"").Append(name)
                .Append("\" value=\"").Append(WebUtility.HtmlEncode(value)).Append("\" />");
        }

        private async Task<PaymentResult> SendRequest(object request, string endPoint)
        {
            var body = JsonSerializer.Serialize(request, JsonOptions);

            var form = GetPostForm();
            form.Content = body;
            form.Headers["auth-hash"] = HashHelper.GetHMACSHA512Base64(body, Configuration.SecretKey);

            var response = await HttpClient.Send(EndPointConfiguration.BaseUrl, endPoint, form);

            var result = response.IsSuccess ? SignedHandler(response) : HttpErrorResult(response.StatusCode);
            result.ServerResponseRaw = response.Body;
            result.OrderContentRaw = body;

            return result;
        }

        private static PaymentResult HttpErrorResult(int statusCode)
        {
            var error = statusCode == (int)HttpStatusCode.Unauthorized
                ? "Banka isteğin imzasını reddetti; Merchant/Terminal Safe Id ve Secret Key bilgilerini kontrol edin"
                : $"Banka HTTP {statusCode} döndü";

            return new PaymentResult(false, error) { ErrorCode = $"HTTP-{statusCode}" };
        }

        private object CreateTerminal()
        {
            return new { merchantSafeId = Configuration.MerchantSafeId, terminalSafeId = Configuration.TerminalSafeId };
        }

        private static object CreateTransaction(OrderInfo order)
        {
            return new
            {
                amount = FormatAmount(order.Total),
                currencyCode = ParseCurrencyCode(order.CurrencyCode),
                motoInd = 0,
                installCount = GetInstallCount(order)
            };
        }

        private PaymentResult Reject(string orderId, string errorCode, string error)
        {
            Log.Warning("VerifyPayment rejected. Provider: {Provider}, OrderId: {OrderId}, ErrorCode: {ErrorCode}, Error: {Error}", GetType().Name, orderId, errorCode, error);

            return new PaymentResult(false, error) { ErrorCode = errorCode };
        }

        private static string FormatAmount(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

        private static int ParseCurrencyCode(string currencyCode) =>
            currencyCode.IsEmpty() ? DefaultCurrencyCode : int.Parse(currencyCode, CultureInfo.InvariantCulture);

        private static int GetInstallCount(OrderInfo order) => order.Installment > 1 ? order.Installment.Value : 1;

        private static string FormatExpireDate(CreditCardInfo card)
        {
            var year = card.ExpireYear.Trim();
            return card.ExpireMonth.Trim().PadLeft(2, '0') + year.Substring(year.Length - 2);
        }

        // Türkiye 2016'dan beri sabit UTC+3; sunucunun saat dilimine bağlı kalmamak için.
        private static string CreateRequestDateTime() =>
            DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture);

        private static string CreateRandomNumber()
        {
            var bytes = new byte[64];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            return BitConverter.ToString(bytes).Replace("-", string.Empty);
        }

        private static string NullIfEmpty(string value) => value.IsEmpty() ? null : value;

        #endregion Pay

        #region Handler

        public PaymentResult SignedHandler(SanalPosHttpResponse response)
        {
            response.Headers.TryGetValue("auth-hash", out var authHash);

            if (!IsValidResponseHash(response.RawBody, authHash))
            {
                Log.Warning("Response auth-hash verification failed. Provider: {Provider}", GetType().Name);
                return new PaymentResult(false, "Banka yanıtının imzası doğrulanamadı") { ErrorCode = "AUTH-HASH" };
            }

            return Handler(response.Body);
        }

        public bool IsValidResponseHash(byte[] serverResponse, string authHash)
        {
            return !authHash.IsEmpty() && authHash == HashHelper.GetHMACSHA512Base64(serverResponse, Configuration.SecretKey);
        }

        public override PaymentResult Handler(string serverResponse)
        {
            PaymentResult result = new PaymentResult();

            try
            {
                using (var document = JsonDocument.Parse(serverResponse))
                {
                    var root = document.RootElement;
                    var responseCode = GetString(root, "responseCode");

                    if (responseCode == ApprovedResponseCode)
                    {
                        result.Status = true;

                        if (root.TryGetProperty("transaction", out var transaction))
                        {
                            result.ProvisionNumber = GetString(transaction, "authCode")?.Trim();
                            result.ReferanceNumber = GetString(transaction, "rrn");
                        }
                    }
                    else
                    {
                        // Banka (issuer) reddinde host kodu dolu gelir; kural hatalarında (VPS-10xx) boştur.
                        var hostResponseCode = GetString(root, "hostResponseCode");
                        var useHost = !hostResponseCode.IsEmpty();

                        result.ErrorCode = useHost ? hostResponseCode : responseCode;
                        result.Error = (useHost ? GetString(root, "hostMessage") : GetString(root, "responseMessage"))?.Trim();
                    }
                }
            }
            catch (JsonException ex)
            {
                Log.Error(ex, "Response could not be parsed. Provider: {Provider}", GetType().Name);
                result.Error = "Banka yanıtı okunamadı";
            }

            return result;
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
                return null;

            return value.ToString();
        }

        #endregion Handler
    }
}

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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SanalPosTR.Providers.VakifBank
{
    public class VakifBankProviderService : BaseProviderService, IProviderService
    {
        private const string ApprovedResultCode = "0000";
        private const string EnrolledStatus = "Y";
        private const string FullSecureMdStatus = "1";
        private const string EcommerceDeviceSource = "0";
        private const int DefaultCurrencyCode = 949;
        private const int NonceLength = 24;
        private const int OrderTagLength = 16;

        // Yarım 3D'de Visa 06, Mastercard 01 döner; banka half secure işleme izin vermiyor.
        private static readonly HashSet<string> FullSecureEciValues = new HashSet<string> { "02", "05" };

        private static readonly HashSet<string> SecretFields = new HashSet<string> { "Cvv", "Password", "MerchantPassword" };

        // MessageErrorCode 200 başarılı enrollment'ta da dönüyor; hata kodu sayılmaz.
        private static readonly HashSet<string> SuccessCodes = new HashSet<string> { "200", "00", "0000" };

        private static readonly Regex WafSupportId = new Regex(@"support ID is:\s*(\d+)", RegexOptions.IgnoreCase);

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public VakifBankProviderService(SanalPosHttpClient httpClient) : base(httpClient)
        {
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => Definition.BankConfiguration[CurrentBank];

        private VakifBankConfiguration Configuration => (VakifBankConfiguration)ProviderConfiguration;

        private VakifBankEndPointConfiguration EndPoint => (VakifBankEndPointConfiguration)EndPointConfiguration;

        public override string EmbededDirectory => string.Empty;

        public override PostForm GetPostForm()
        {
            PostForm postForm = new PostForm();
            postForm.ContentType = "application/x-www-form-urlencoded";
            postForm.RequestFormat = RequestDataFormat.None;
            postForm.SendParameterType = SendParameterType.QueryString;
            return postForm;
        }

        #endregion Base

        #region Pay

        public override async Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            var order = paymentModel.Order;

            Log.Information("ProcessPayment started. Provider: {Provider}, Use3DSecure: {Use3DSecure}, OrderId: {OrderId}", GetType().Name, paymentModel.Use3DSecure, order.OrderId);

            var result = paymentModel.Use3DSecure
                ? await Enroll(paymentModel)
                : await SendVposRequest(new[]
                {
                    ("TransactionType", "Sale"),
                    ("TransactionId", CreateTransactionId()),
                    ("OrderId", NullIfEmpty(order.OrderId)),
                    ("CurrencyAmount", FormatAmount(order.Total)),
                    ("CurrencyCode", ParseCurrencyCode(order.CurrencyCode).ToString(CultureInfo.InvariantCulture)),
                    ("Pan", paymentModel.CreditCard.CardNumber),
                    ("Expiry", FormatExpiry(paymentModel.CreditCard)),
                    ("Cvv", paymentModel.CreditCard.CVV2),
                    ("NumberOfInstallments", GetInstallmentCount(order)),
                    ("ClientIp", FormatClientIp(order.IP)),
                    ("TransactionDeviceSource", EcommerceDeviceSource)
                });

            Log.Information("ProcessPayment completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}, ErrorCode: {ErrorCode}", GetType().Name, order.OrderId, result.Status, result.ErrorCode);

            return result;
        }

        public override async Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            var order = paymentModel.Order;

            Log.Information("VerifyPayment started. Provider: {Provider}, OrderId: {OrderId}, VerifyEnrollmentRequestId: {VerifyEnrollmentRequestId}", GetType().Name, order.OrderId, Get(collection, "VerifyEnrollmentRequestId"));

            var rejection = Check3DCallback(order, collection);
            if (rejection != null)
                return rejection;

            // 3DS Metodu: kart, vade ve tutar banka tarafından MpiTransactionId ile eşleştirilir; gönderilirse 1127 döner.
            var result = await SendVposRequest(new[]
            {
                ("TransactionType", "Sale"),
                ("TransactionId", CreateTransactionId()),
                ("OrderId", NullIfEmpty(order.OrderId)),
                ("MpiTransactionId", Get(collection, "VerifyEnrollmentRequestId")),
                ("ECI", FormatEci(Get(collection, "Eci"))),
                ("CAVV", Get(collection, "Cavv")),
                ("NumberOfInstallments", GetInstallmentCount(order)),
                ("ClientIp", FormatClientIp(order.IP)),
                ("TransactionDeviceSource", EcommerceDeviceSource)
            });

            Log.Information("VerifyPayment completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}, ErrorCode: {ErrorCode}", GetType().Name, order.OrderId, result.Status, result.ErrorCode);

            return result;
        }

        public override async Task<PaymentResult> ProcessRefound(Refund refund)
        {
            Log.Information("ProcessRefund started. Provider: {Provider}, OrderId: {OrderId}, ReferenceTransactionId: {ReferenceTransactionId}", GetType().Name, refund.OrderId, refund.ReferanceNumber);

            if (refund.ReferanceNumber.IsEmpty())
                return new PaymentResult(false, "İade için satış işleminin TransactionId değeri (ReferanceNumber) gerekli") { ErrorCode = "REFERENCE" };

            // Refund'da CurrencyAmount zorunlu (v2.4 zorunluluk tablosu); tam iade de tutarla yapılır.
            if (refund.RefundAmount <= 0)
                return new PaymentResult(false, "İade tutarı sıfırdan büyük olmalı") { ErrorCode = "AMOUNT" };

            if (refund.ClientIp.IsEmpty())
                return new PaymentResult(false, "İade isteğinde ClientIp zorunlu") { ErrorCode = "CLIENTIP" };

            var result = await SendVposRequest(new[]
            {
                ("TransactionType", "Refund"),
                ("ReferenceTransactionId", refund.ReferanceNumber),
                ("CurrencyAmount", FormatAmount(refund.RefundAmount)),
                ("ClientIp", FormatClientIp(refund.ClientIp))
            });

            Log.Information("ProcessRefund completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}, ErrorCode: {ErrorCode}", GetType().Name, refund.OrderId, result.Status, result.ErrorCode);

            return result;
        }

        private async Task<PaymentResult> Enroll(PaymentModel paymentModel)
        {
            var configuration = Configuration;
            var order = paymentModel.Order;
            var card = paymentModel.CreditCard;

            // JSON enrollment'ta PurchaseAmount string değil sayı olmalı (sandbox: enrollment-json).
            var request = new (string Name, object Value)[]
            {
                ("Pan", card.CardNumber),
                ("ExpiryDate", FormatExpiry(card).Substring(2)),
                ("PurchaseAmount", decimal.Round(order.Total, 2, MidpointRounding.AwayFromZero)),
                ("Currency", ParseCurrencyCode(order.CurrencyCode).ToString(CultureInfo.InvariantCulture)),
                ("VerifyEnrollmentRequestId", CreateVerifyEnrollmentRequestId(order.OrderId)),
                ("MerchantId", configuration.MerchantId),
                ("MerchantPassword", configuration.Password),
                ("SuccessUrl", configuration.SiteSuccessUrl.CompileOrderLink(paymentModel)),
                ("FailureUrl", configuration.SiteFailUrl.CompileOrderLink(paymentModel)),
                ("InstallmentCount", order.Installment > 1 ? order.Installment : null),
                ("BrowserIpAddress", NullIfEmpty(FormatClientIp(order.IP))),
                ("Language", "tr-TR")
            }.Where(field => field.Value != null).ToList();

            var response = await HttpClient.Send(EndPoint.EnrollmentBaseUrl, EndPoint.EnrollmentEndPoint, new PostForm
            {
                ContentType = "application/json",
                RequestFormat = RequestDataFormat.Json,
                SendParameterType = SendParameterType.RequestBody,
                Content = ToJson(request, mask: false)
            });

            var result = response.IsSuccess ? EnrollmentHandler(response.Body) : HttpErrorResult(response.StatusCode);
            result.OrderContentRaw = ToJson(request, mask: true);

            // Yönlendirmede ServerResponseRaw tarayıcıya basılacak ACS formudur.
            if (!result.IsRedirectContent)
                result.ServerResponseRaw = response.Body;

            return result;
        }

        /// <summary>
        /// ProcessEnrollment yanıtını yorumlar; kart 3D'ye kayıtlıysa ACS'e otomatik post edilecek formu döndürür.
        /// </summary>
        public PaymentResult EnrollmentHandler(string serverResponse)
        {
            var fields = ReadJsonFields(serverResponse);
            var status = Field(fields, "Status");

            if (status == EnrolledStatus)
            {
                // Rehber cevabında TermUrl yok; PaReq formu PaReq ve MD ile yetiniyor.
                return new PaymentResult
                {
                    Status = true,
                    IsRedirectContent = true,
                    ServerResponseRaw = BuildAcsForm(Field(fields, "AcsUrl"), new[]
                    {
                        ("PaReq", Field(fields, "PaReq")),
                        ("TermUrl", Field(fields, "TermUrl")),
                        ("MD", Field(fields, "MD"))
                    })
                };
            }

            var result = fields == null
                ? UnreadableResult(serverResponse, "3D-EMPTY")
                : new PaymentResult(false, Field(fields, "ErrorMessage") ?? $"Kart 3D Secure doğrulamasına uygun değil (Status: {status})")
                {
                    ErrorCode = new[] { "MessageErrorCode", "ResultCode", "ResponseCode" }
                        .Select(name => Field(fields, name))
                        .FirstOrDefault(code => code != null && !SuccessCodes.Contains(code)) ?? $"3D-{status ?? "EMPTY"}"
                };

            Log.Warning("Enrollment rejected. Provider: {Provider}, Status: {Status}, ErrorCode: {ErrorCode}, Error: {Error}", GetType().Name, status, result.ErrorCode, result.Error);

            return result;
        }

        private PaymentResult Check3DCallback(OrderInfo order, IFormCollection collection)
        {
            var orderId = order.OrderId;
            var status = Get(collection, "Status");

            // Başarısız dönüşte imza aranmaz: provizyona çıkılmadığı için sahte bir red yalnız bu ödemeyi düşürür.
            if (status != EnrolledStatus)
            {
                var errorCode = Get(collection, "ErrorCode");
                var errorMessage = Get(collection, "ErrorMessage");

                return Reject(orderId,
                    errorCode.IsEmpty() ? $"3D-{(status.IsEmpty() ? "EMPTY" : status)}" : errorCode,
                    errorMessage.IsEmpty() ? $"3D Secure doğrulaması başarısız (Status: {status})" : errorMessage);
            }

            if (!IsValidCallbackHash(collection))
                return Reject(orderId, "HASH", "3D doğrulama yanıtının imzası geçersiz");

            if (Get(collection, "MerchantId") != Configuration.MerchantId)
                return Reject(orderId, "MERCHANT", "3D doğrulama yanıtı başka bir üye işyerine ait");

            if (!BelongsToOrder(Get(collection, "VerifyEnrollmentRequestId"), orderId))
                return Reject(orderId, "ORDER", "3D doğrulama yanıtı bu siparişe ait değil");

            var purchAmount = Get(collection, "PurchAmount").Replace(".", string.Empty);
            var purchCurrency = Get(collection, "PurchCurrency");
            if (purchAmount != ToMinorUnits(order.Total) || purchCurrency != ParseCurrencyCode(order.CurrencyCode).ToString(CultureInfo.InvariantCulture))
                return Reject(orderId, "AMOUNT", $"3D doğrulanan tutar sipariş tutarıyla uyuşmuyor ({purchAmount} {purchCurrency})");

            var mdStatus = Get(collection, "MdStatus");
            var eci = FormatEci(Get(collection, "Eci"));
            if (mdStatus != FullSecureMdStatus || !FullSecureEciValues.Contains(eci) || Get(collection, "Cavv").IsEmpty())
                return Reject(orderId, "3D-SECURE", $"3D doğrulama tam güvenli değil (MdStatus: {mdStatus}, ECI: {eci})");

            return null;
        }

        /// <summary>
        /// 3D dönüşündeki Hash'i bankanın tanımladığı alan sırasıyla doğrular (ISO-8859-9, SHA-256, Base64).
        /// </summary>
        public bool IsValidCallbackHash(IFormCollection collection)
        {
            var hash = Get(collection, "Hash");
            if (hash.IsEmpty())
                return false;

            var plain = string.Concat(
                Get(collection, "VerifyEnrollmentRequestId"),
                Get(collection, "MerchantId"),
                Get(collection, "PurchCurrency"),
                Get(collection, "PurchAmount").Replace(".", string.Empty),
                FormatEci(Get(collection, "Eci")),
                Get(collection, "Cavv"),
                Get(collection, "MdStatus"),
                Get(collection, "Status"),
                Configuration.Password);

            return FixedTimeEquals(HashHelper.GetSHA256WithISO88599(plain), hash);
        }

        // VerifyPaymentModel enrollment'ta üretilen değeri taşımıyor; siparişe bağlılık saklamadan, imzalı kimlikle doğrulanır.
        private string CreateVerifyEnrollmentRequestId(string orderId)
        {
            var nonce = ToHex(CreateRandomBytes(NonceLength / 2));
            return nonce + CreateOrderTag(nonce, orderId);
        }

        private bool BelongsToOrder(string verifyEnrollmentRequestId, string orderId)
        {
            if (verifyEnrollmentRequestId.Length != NonceLength + OrderTagLength)
                return false;

            var nonce = verifyEnrollmentRequestId.Substring(0, NonceLength);
            return FixedTimeEquals(CreateOrderTag(nonce, orderId), verifyEnrollmentRequestId.Substring(NonceLength));
        }

        private string CreateOrderTag(string nonce, string orderId)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Configuration.Password ?? string.Empty)))
            {
                return ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(nonce + orderId))).Substring(0, OrderTagLength);
            }
        }

        private async Task<PaymentResult> SendVposRequest(IEnumerable<(string Name, string Value)> fields)
        {
            var configuration = Configuration;
            var requestFields = WithValues(new[]
            {
                ("MerchantId", configuration.MerchantId),
                ("Password", configuration.Password),
                ("TerminalNo", configuration.TerminalNo)
            }.Concat(fields));

            var response = await Send(EndPointConfiguration.BaseUrl, EndPointConfiguration.ApiEndPoint, new[] { ("prmstr", ToXml(requestFields, mask: false)) });

            var result = response.IsSuccess ? Handler(response.Body) : HttpErrorResult(response.StatusCode);
            result.ServerResponseRaw = response.Body;
            result.OrderContentRaw = ToXml(requestFields, mask: true);

            return result;
        }

        private async Task<SanalPosHttpResponse> Send(string baseUrl, string endPoint, IEnumerable<(string Name, string Value)> fields)
        {
            var form = GetPostForm();
            form.Parameters = fields.Select(field => new SanalPosTRAttribute { Key = field.Name, Value = field.Value }).ToList();

            return await HttpClient.Send(baseUrl, endPoint, form);
        }

        private static List<(string Name, string Value)> WithValues(IEnumerable<(string Name, string Value)> fields) =>
            fields.Where(field => !field.Value.IsEmpty()).ToList();

        private static string ToJson(IEnumerable<(string Name, object Value)> fields, bool mask) =>
            JsonSerializer.Serialize(
                fields.ToDictionary(field => field.Name, field => mask && field.Value is string text ? Mask(field.Name, text) : field.Value),
                JsonOptions);

        private static string ToXml(IEnumerable<(string Name, string Value)> fields, bool mask) =>
            new XElement("VposRequest", fields.Select(field => new XElement(field.Name, mask ? Mask(field.Name, field.Value) : field.Value)))
                .ToString(SaveOptions.DisableFormatting);

        private static string Mask(string name, string value)
        {
            if (SecretFields.Contains(name))
                return "***";

            if (name == "Pan" && value.Length > 10)
                return value.Substring(0, 6) + new string('*', value.Length - 10) + value.Substring(value.Length - 4);

            return value;
        }

        private static string BuildAcsForm(string acsUrl, IEnumerable<(string Name, string Value)> fields)
        {
            var html = new StringBuilder();
            html.Append("<form id=\"SanalPosTR\" name=\"SanalPosTR\" method=\"post\" style=\"display:none;\" action=\"")
                .Append(WebUtility.HtmlEncode(acsUrl))
                .Append("\">");

            foreach (var field in fields.Where(entry => !entry.Value.IsEmpty()))
            {
                html.Append("<input type=\"hidden\" name=\"").Append(field.Name)
                    .Append("\" value=\"").Append(WebUtility.HtmlEncode(field.Value)).Append("\" />");
            }

            return html.Append("</form>").ToString();
        }

        private static PaymentResult UnreadableResult(string serverResponse, string errorCode)
        {
            var supportId = WafSupportId.Match(serverResponse ?? string.Empty);

            return supportId.Success
                ? new PaymentResult(false, $"Banka güvenlik duvarı isteği reddetti (Support ID: {supportId.Groups[1].Value})") { ErrorCode = "WAF" }
                : new PaymentResult(false, "Banka yanıtı okunamadı") { ErrorCode = errorCode };
        }

        private static PaymentResult HttpErrorResult(int statusCode) =>
            new PaymentResult(false, $"Banka HTTP {statusCode} döndü") { ErrorCode = $"HTTP-{statusCode}" };

        private PaymentResult Reject(string orderId, string errorCode, string error)
        {
            Log.Warning("VerifyPayment rejected. Provider: {Provider}, OrderId: {OrderId}, ErrorCode: {ErrorCode}, Error: {Error}", GetType().Name, orderId, errorCode, error);

            return new PaymentResult(false, error) { ErrorCode = errorCode };
        }

        private static string Get(IFormCollection collection, string key) => collection[key].ToString();

        private static string FormatAmount(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

        private static string ToMinorUnits(decimal amount) =>
            decimal.Round(amount * 100, MidpointRounding.AwayFromZero).ToString("0", CultureInfo.InvariantCulture);

        private static int ParseCurrencyCode(string currencyCode) =>
            currencyCode.IsEmpty() ? DefaultCurrencyCode : int.Parse(currencyCode, CultureInfo.InvariantCulture);

        // 0 veya 1 gönderilirse MPI işlemi reddediyor; tek çekimde alan hiç gönderilmez.
        private static string GetInstallmentCount(OrderInfo order) =>
            order.Installment > 1 ? order.Installment.Value.ToString(CultureInfo.InvariantCulture) : null;

        private static string FormatExpiry(CreditCardInfo card)
        {
            var year = card.ExpireYear.Trim();
            if (year.Length == 2)
                year = "20" + year;

            return year + card.ExpireMonth.Trim().PadLeft(2, '0');
        }

        private static string FormatEci(string eci) => eci.IsEmpty() ? string.Empty : eci.Trim().PadLeft(2, '0');

        private static string FormatClientIp(string ip)
        {
            const string ipv4MappedPrefix = "::ffff:";

            var value = ip?.Trim();
            return value != null && value.StartsWith(ipv4MappedPrefix, StringComparison.OrdinalIgnoreCase)
                ? value.Substring(ipv4MappedPrefix.Length)
                : value;
        }

        // Banka 40 karaktere izin veriyor; çağıran taraf ReferanceNumber'ı 30 karakterlik bir sütunda saklayabildiği için kısa tutuluyor.
        private static string CreateTransactionId() => ToHex(CreateRandomBytes(15));

        private static byte[] CreateRandomBytes(int length)
        {
            var bytes = new byte[length];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(bytes);
            }

            return bytes;
        }

        private static string ToHex(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();

        // netstandard2.0'da CryptographicOperations.FixedTimeEquals yok.
        private static bool FixedTimeEquals(string expected, string actual)
        {
            if (expected == null || actual == null)
                return false;

            var left = Encoding.UTF8.GetBytes(expected);
            var right = Encoding.UTF8.GetBytes(actual);
            var difference = left.Length ^ right.Length;

            for (var i = 0; i < Math.Min(left.Length, right.Length); i++)
                difference |= left[i] ^ right[i];

            return difference == 0;
        }

        private static string NullIfEmpty(string value) => value.IsEmpty() ? null : value;

        #endregion Pay

        #region Handler

        public override PaymentResult Handler(string serverResponse)
        {
            var xml = TryParseXml(serverResponse);
            var resultCode = Value(xml, "ResultCode");

            if (resultCode.IsEmpty())
            {
                Log.Error("Response could not be parsed. Provider: {Provider}", GetType().Name);
                return UnreadableResult(serverResponse, null);
            }

            if (resultCode == ApprovedResultCode)
            {
                return new PaymentResult
                {
                    Status = true,
                    ProvisionNumber = Value(xml, "AuthCode"),
                    ReferanceNumber = Value(xml, "TransactionId")
                };
            }

            return new PaymentResult(false, Value(xml, "ResultDetail")) { ErrorCode = resultCode };
        }

        private static XDocument TryParseXml(string value)
        {
            if (value.IsEmpty())
                return null;

            try
            {
                return XDocument.Parse(value);
            }
            catch (XmlException)
            {
                return null;
            }
        }

        private static Dictionary<string, string> ReadJsonFields(string value)
        {
            if (value.IsEmpty())
                return null;

            try
            {
                using (var document = JsonDocument.Parse(value))
                {
                    var root = document.RootElement;
                    if (root.ValueKind != JsonValueKind.Object)
                        return null;

                    // Rehber cevabı ProcessEnrollmentResult altına sarıyor, sandbox düz dönüyor; alan adlarının harfleri de farklı (AcsUrl/ACSUrl).
                    var wrapped = root.EnumerateObject()
                        .Where(property => property.Name.Equals("ProcessEnrollmentResult", StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind == JsonValueKind.Object)
                        .Select(property => (JsonElement?)property.Value)
                        .FirstOrDefault();

                    var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var property in (wrapped ?? root).EnumerateObject())
                    {
                        if (property.Value.ValueKind == JsonValueKind.String)
                            fields[property.Name] = property.Value.GetString().Trim();
                        else if (property.Value.ValueKind == JsonValueKind.Number || property.Value.ValueKind == JsonValueKind.True || property.Value.ValueKind == JsonValueKind.False)
                            fields[property.Name] = property.Value.GetRawText();
                    }

                    return fields;
                }
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string Field(Dictionary<string, string> fields, string name) =>
            fields != null && fields.TryGetValue(name, out var value) && !value.IsEmpty() ? value : null;

        // Bazı işlem tipleri cevabı <_items> altına sarıyor; alan yerinden bağımsız aranır.
        private static string Value(XDocument xml, string name)
        {
            var value = xml?.Descendants().FirstOrDefault(element => element.Name.LocalName == name)?.Value.Trim();
            return value.IsEmpty() ? null : value;
        }

        #endregion Handler
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using SanalPosTR.Configuration;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using SanalPosTR.Providers.AkbankPos;
using SanalPosTR.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanalPosTR.Test.Providers
{
    [TestFixture]
    public class AkbankPosServiceTest : Initialize
    {
        private const string SuccessOrderId = "LVTEST3D-1793010417692";
        private const string FailOrderId = "LVTEST3D-1793010283880";

        private AkbankPosProviderService GetProvider()
        {
            var providerService = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderService>>();
            return (AkbankPosProviderService)providerService(BankTypes.AkbankPos);
        }

        // Akbank test ortamında tarayıcıyla alınmış gerçek okUrl callback'i; hash'i banka üretti.
        private static Dictionary<string, StringValues> SuccessCallback() => new Dictionary<string, StringValues>
        {
            ["txnCode"] = "3001",
            ["responseCode"] = "VPS-0000",
            ["responseMessage"] = "BAŞARILI",
            ["txnDateTime"] = "2026-10-26T13:27:07.000",
            ["merchantSafeId"] = "2023090417500272654BD9A49CF07574",
            ["terminalSafeId"] = "2023090417500284633D137A249DBBEB",
            ["maskedCardNumber"] = "557829******1055",
            ["orderId"] = SuccessOrderId,
            ["amount"] = "1.00",
            ["installCount"] = "1",
            ["secureEcomInd"] = "02",
            ["mdStatus"] = "1",
            ["secureId"] = "ZTQwOHU1UVIwU0hNdHY2Z2pKRXY=",
            ["secureData"] = "kBM6SgmAAAAAAAAAAAAAAAAAAAAA",
            ["secureMd"] = "F34CC3A09A105540325226366434ABFFDCC3C67878A4870BF6C330D2E4E595CF",
            ["okUrl"] = "http://localhost:5055/ok",
            ["failUrl"] = "http://localhost:5055/fail",
            ["hashParams"] = "txnCode+responseCode+responseMessage+txnDateTime+merchantSafeId+terminalSafeId+orderId+secureId+secureEcomInd+mdStatus+secureData+secureMd+maskedCardNumber+amount+installCount+okUrl+failUrl",
            ["hash"] = "Pz6RF7S6plTsXWlRiqSudAQRHLe9zXbvCu2Vug16aJ/q9HAlnVaRy25DlopIxWQbaktFu2DaibwNnAIef8z0Xw=="
        };

        // Aynı ortamdan failUrl callback'i: hashParams farklı ve txnCode boş geliyor.
        private static Dictionary<string, StringValues> FailCallback() => new Dictionary<string, StringValues>
        {
            ["txnCode"] = "",
            ["responseCode"] = "VPS-1282",
            ["responseMessage"] = "BKM 3DS Server Doğrulama Yapılamadı",
            ["hostResponseCode"] = "",
            ["hostMessage"] = "",
            ["merchantSafeId"] = "2023090417500272654BD9A49CF07574",
            ["terminalSafeId"] = "2023090417500284633D137A249DBBEB",
            ["maskedCardNumber"] = "435509******7068",
            ["orderId"] = FailOrderId,
            ["mdStatus"] = "0",
            ["okUrl"] = "http://localhost:5055/ok",
            ["failUrl"] = "http://localhost:5055/fail",
            ["hashParams"] = "responseCode+responseMessage+merchantSafeId+terminalSafeId+orderId+mdStatus+maskedCardNumber+okUrl+failUrl",
            ["hash"] = "nJSQ9JKKf2aX3FVyjlUsh7fjTnlEWTR+3U3DUoI0jFas/haNuK8GvlxsRpDveXdLlGs0fNDSf9jecoR9mkdvcg=="
        };

        private static VerifyPaymentModel CreateVerifyModel(string orderId) => new VerifyPaymentModel
        {
            Order = new OrderInfo { OrderId = orderId, Total = 1M },
            SelectedBank = BankTypes.AkbankPos
        };

        private static PaymentModel CreatePaymentModel(int? installment) => new PaymentModel
        {
            CreditCard = new CreditCardInfo
            {
                CardNumber = "4355093000315232",
                ExpireMonth = "1",
                ExpireYear = "2028",
                CVV2 = "264",
                CardHolderName = "Ayşe Şahin"
            },
            Order = new OrderInfo { OrderId = "LV-1001", Total = 10.5M, Installment = installment },
            Use3DSecure = true,
            SessionId = "session-1"
        };

        private static Dictionary<string, string> ParseHiddenInputs(string html)
        {
            return Regex.Matches(html, "<input type=\"hidden\" name=\"([^\"]+)\" value=\"([^\"]*)\" />")
                .Cast<Match>()
                .ToDictionary(m => m.Groups[1].Value, m => WebUtility.HtmlDecode(m.Groups[2].Value));
        }

        #region Callback

        [Test]
        public void Validate3D_BankSuccessCallback_IsValid()
        {
            Assert.That(GetProvider().Validate3D(new FormCollection(SuccessCallback())), Is.True);
        }

        [Test]
        public void Validate3D_BankFailCallback_IsValid()
        {
            Assert.That(GetProvider().Validate3D(new FormCollection(FailCallback())), Is.True);
        }

        [Test]
        public void Validate3D_TamperedAmount_IsInvalid()
        {
            var callback = SuccessCallback();
            callback["amount"] = "100.00";

            Assert.That(GetProvider().Validate3D(new FormCollection(callback)), Is.False);
        }

        [Test]
        public void Validate3D_MissingHashParams_IsInvalid()
        {
            var callback = SuccessCallback();
            callback.Remove("hashParams");

            Assert.That(GetProvider().Validate3D(new FormCollection(callback)), Is.False);
        }

        [Test]
        public async Task VerifyPayment_FailCallback_ReturnsBankError()
        {
            var result = await GetProvider().VerifyPayment(CreateVerifyModel(FailOrderId), new FormCollection(FailCallback()));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("VPS-1282"));
            Assert.That(result.Error, Is.EqualTo("BKM 3DS Server Doğrulama Yapılamadı"));
        }

        [Test]
        public async Task VerifyPayment_TamperedCallback_IsRejected()
        {
            var callback = SuccessCallback();
            callback["amount"] = "100.00";

            var result = await GetProvider().VerifyPayment(CreateVerifyModel(SuccessOrderId), new FormCollection(callback));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("HASH"));
        }

        [Test]
        public async Task VerifyPayment_CallbackOfAnotherOrder_IsRejected()
        {
            var result = await GetProvider().VerifyPayment(CreateVerifyModel("LV-OTHER"), new FormCollection(SuccessCallback()));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("ORDER"));
        }

        #endregion

        #region 3D Form

        [Test]
        public async Task ProcessPayment_3D_ReturnsSignedFormForGateway()
        {
            var result = await GetProvider().ProcessPayment(CreatePaymentModel(null));
            var inputs = ParseHiddenInputs(result.ServerResponseRaw);

            Assert.That(result.Status, Is.True);
            Assert.That(result.IsRedirectContent, Is.True);
            Assert.That(result.ServerResponseRaw, Does.Contain("action=\"https://virtualpospaymentgatewaypre.akbank.com/securepay\""));
            Assert.That(result.ServerResponseRaw, Does.Contain("accept-charset=\"UTF-8\""));
            Assert.That(inputs["paymentModel"], Is.EqualTo("3D"));
            Assert.That(inputs["txnCode"], Is.EqualTo("3000"));
            Assert.That(inputs["amount"], Is.EqualTo("10.50"));
            Assert.That(inputs["currencyCode"], Is.EqualTo("949"));
            Assert.That(inputs["installCount"], Is.EqualTo("1"));
            Assert.That(inputs["expiredDate"], Is.EqualTo("0128"));
            Assert.That(inputs["cardHolderName"], Is.EqualTo("Ayşe Şahin"));
            Assert.That(inputs["okUrl"], Is.EqualTo("http://x.com/Payment/ValidatePayment/session-1"));
            Assert.That(inputs["randomNumber"], Does.Match("^[0-9A-F]{128}$"));
            Assert.That(inputs["requestDateTime"], Does.Match(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}$"));
            Assert.That(inputs.ContainsKey("emailAddress"), Is.False);
            Assert.That(inputs.ContainsKey("mobilePhone"), Is.False);
        }

        [Test]
        public async Task ProcessPayment_3D_HashFollowsBankFieldOrder()
        {
            var result = await GetProvider().ProcessPayment(CreatePaymentModel(3));
            var inputs = ParseHiddenInputs(result.ServerResponseRaw);
            var secretKey = ((AkbankPosConfiguration)Definition.BankConfiguration[BankTypes.AkbankPos]).SecretKey;

            var bankOrder = new[]
            {
                "paymentModel", "txnCode", "merchantSafeId", "terminalSafeId", "orderId", "lang", "amount",
                "ccbRewardAmount", "pcbRewardAmount", "xcbRewardAmount", "currencyCode", "installCount",
                "okUrl", "failUrl", "emailAddress", "mobilePhone", "homePhone", "workPhone", "subMerchantId",
                "creditCard", "expiredDate", "cvv", "cardHolderName", "randomNumber", "requestDateTime",
                "b2bIdentityNumber", "merchantData", "merchantBranchNo", "mobileEci", "walletProgramData",
                "mobileAssignedId", "mobileDeviceType"
            };
            var plain = string.Concat(bankOrder.Select(name => inputs.TryGetValue(name, out var value) ? value : string.Empty));

            Assert.That(inputs["installCount"], Is.EqualTo("3"));
            Assert.That(inputs["hash"], Is.EqualTo(HashHelper.GetHMACSHA512Base64(plain, secretKey)));
        }

        #endregion

        #region Handler

        [Test]
        public void Handler_ApprovedResponse_MapsAuthCodeAndRrn()
        {
            var json = "{\"txnCode\":\"1000\",\"responseCode\":\"VPS-0000\",\"responseMessage\":\"BAŞARILI\",\"hostResponseCode\":\"00\",\"hostMessage\":\"000 ONAY KODU XXXXXX\",\"transaction\":{\"authCode\":\"038850\",\"rrn\":\"630026390027\",\"batchNumber\":1465,\"stan\":812}}";

            var result = GetProvider().Handler(json);

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("038850"));
            Assert.That(result.ReferanceNumber, Is.EqualTo("630026390027"));
        }

        [Test]
        public void Handler_IssuerDecline_UsesHostCode()
        {
            var json = "{\"txnCode\":\"1000\",\"responseCode\":\"VPS-1005\",\"responseMessage\":\"Provizyon Hatası\",\"hostResponseCode\":\"51\",\"hostMessage\":\"051 YETERSIZ BAKIYE \",\"transaction\":{\"authCode\":\"      \",\"rrn\":\"630026389978\"}}";

            var result = GetProvider().Handler(json);

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("51"));
            Assert.That(result.Error, Is.EqualTo("051 YETERSIZ BAKIYE"));
        }

        [Test]
        public void Handler_RuleError_UsesResponseCode()
        {
            var json = "{\"txnCode\":\"1002\",\"responseCode\":\"VPS-1020\",\"responseMessage\":\"İade edilebilir tutar hatalı\"}";

            var result = GetProvider().Handler(json);

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("VPS-1020"));
            Assert.That(result.Error, Is.EqualTo("İade edilebilir tutar hatalı"));
        }

        [Test]
        public void Handler_NonJsonResponse_ReturnsFailedResult()
        {
            var result = GetProvider().Handler("<html>Service Unavailable</html>");

            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Is.Not.Empty);
        }

        private static SanalPosHttpResponse SignedResponse(byte[] rawBody, string body, string authHash)
        {
            var response = new SanalPosHttpResponse { IsSuccess = true, StatusCode = 200, Body = body, RawBody = rawBody };
            if (authHash != null)
                response.Headers["auth-hash"] = authHash;

            return response;
        }

        private static string AkbankSecretKey => ((AkbankPosConfiguration)Definition.BankConfiguration[BankTypes.AkbankPos]).SecretKey;

        private static (AkbankPosProviderService Provider, CaptureHttpHandler Handler) CreateWithFakeBank(HttpStatusCode statusCode, string responseBody, bool signResponse)
        {
            var rawBody = Encoding.UTF8.GetBytes(responseBody);
            var headers = signResponse
                ? new[] { ("auth-hash", HashHelper.GetHMACSHA512Base64(rawBody, AkbankSecretKey)) }
                : new (string, string)[0];

            var handler = new CaptureHttpHandler(_ => CaptureHttpHandler.Response(statusCode, rawBody, "application/json", headers));
            var provider = new AkbankPosProviderService(new SanalPosHttpClient(new HttpClient(handler))) { CurrentBank = BankTypes.AkbankPos };

            return (provider, handler);
        }

        [Test]
        public async Task ProcessRefound_SendsSignedJsonAndAcceptsSignedResponse()
        {
            var (provider, handler) = CreateWithFakeBank(HttpStatusCode.OK, "{\"txnCode\":\"1002\",\"responseCode\":\"VPS-0000\",\"responseMessage\":\"BAŞARILI\",\"transaction\":{\"authCode\":\"038872\",\"rrn\":\"630026390103\"}}", signResponse: true);

            var result = await provider.ProcessRefound(new Refund { OrderId = "LV-1001", RefundAmount = 2.5M });

            Assert.That(handler.RequestUri, Is.EqualTo("https://apipre.akbank.com/api/v1/payment/virtualpos/transaction/process"));
            Assert.That(handler.RequestContentTypes, Is.EqualTo(new[] { "application/json; charset=utf-8" }));
            Assert.That(handler.RequestHeaders["auth-hash"], Is.EqualTo(HashHelper.GetHMACSHA512Base64(handler.RequestBody, AkbankSecretKey)));
            Assert.That(handler.RequestBody, Does.Contain("\"txnCode\":\"1002\"").And.Contain("\"amount\":\"2.50\""));
            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("038872"));
            Assert.That(result.OrderContentRaw, Is.EqualTo(handler.RequestBody));
            Assert.That(result.ServerResponseRaw, Does.Contain("VPS-0000"));
        }

        [Test]
        public async Task ProcessRefound_UnsignedResponse_IsRejected()
        {
            var (provider, _) = CreateWithFakeBank(HttpStatusCode.OK, "{\"responseCode\":\"VPS-0000\"}", signResponse: false);

            var result = await provider.ProcessRefound(new Refund { OrderId = "LV-1001", RefundAmount = 1M });

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("AUTH-HASH"));
        }

        [Test]
        public async Task ProcessRefound_Unauthorized_ReportsSignatureProblemNotTimeout()
        {
            var (provider, _) = CreateWithFakeBank(HttpStatusCode.Unauthorized, "{\"code\":\"401\",\"message\":\"Unauthorized\"}", signResponse: false);

            var result = await provider.ProcessRefound(new Refund { OrderId = "LV-1001", RefundAmount = 1M });

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("HTTP-401"));
            Assert.That(result.Error, Does.Contain("Secret Key"));
            Assert.That(result.ServerResponseRaw, Does.Contain("Unauthorized"));
        }

        [Test]
        public void SignedHandler_ValidSignature_ParsesResponse()
        {
            var body = "{\"responseCode\":\"VPS-0000\",\"responseMessage\":\"BAŞARILI\",\"transaction\":{\"authCode\":\"038870\",\"rrn\":\"630026390084\"}}";
            var rawBody = Encoding.UTF8.GetBytes(body);

            var result = GetProvider().SignedHandler(SignedResponse(rawBody, body, HashHelper.GetHMACSHA512Base64(rawBody, AkbankSecretKey)));

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("038870"));
        }

        [Test]
        public void SignedHandler_TamperedOrMissingSignature_IsRejected()
        {
            var body = "{\"responseCode\":\"VPS-0000\"}";
            var rawBody = Encoding.UTF8.GetBytes(body);
            var tampered = Encoding.UTF8.GetBytes(body.Replace("0000", "1005"));
            var authHash = HashHelper.GetHMACSHA512Base64(tampered, AkbankSecretKey);

            Assert.That(GetProvider().SignedHandler(SignedResponse(rawBody, body, authHash)).ErrorCode, Is.EqualTo("AUTH-HASH"));
            Assert.That(GetProvider().SignedHandler(SignedResponse(rawBody, body, null)).ErrorCode, Is.EqualTo("AUTH-HASH"));
        }

        [Test]
        public void SignedHandler_VerifiesRawBytesNotDecodedText()
        {
            // Banka UTF-8 gönderip farklı bir charset bildirirse çözülen metin bozulur, imzalanan baytlar değişmez.
            var rawBody = Encoding.UTF8.GetBytes("{\"responseCode\":\"VPS-0000\",\"responseMessage\":\"BAŞARILI\"}");
            var misdecoded = Encoding.Latin1.GetString(rawBody);

            var result = GetProvider().SignedHandler(SignedResponse(rawBody, misdecoded, HashHelper.GetHMACSHA512Base64(rawBody, AkbankSecretKey)));

            Assert.That(result.Status, Is.True);
        }

        #endregion

        [Test]
        public void AkbankPos_IsRegisteredSeparatelyFromNestPayAkbank()
        {
            Assert.That(Definition.BankProviders[BankTypes.AkbankPos], Is.EqualTo(typeof(AkbankPosProviderService)));
            Assert.That(Definition.BankProviders[BankTypes.Akbank].Name, Is.EqualTo("NestPayProviderService"));
            Assert.That(Definition.BankConfiguration[BankTypes.AkbankPos], Is.TypeOf<AkbankPosConfiguration>());
        }
    }
}

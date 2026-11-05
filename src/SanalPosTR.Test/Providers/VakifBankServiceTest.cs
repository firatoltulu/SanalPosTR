using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using SanalPosTR.Configuration;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using SanalPosTR.Providers.VakifBank;
using SanalPosTR.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SanalPosTR.Test.Providers
{
    [TestFixture]
    public class VakifBankServiceTest : Initialize
    {
        private const string OrderId = "LV-2001";
        private const string AcsUrl = "https://inbound.apigatewaytest.vakifbank.com.tr/threeDGateway/startThreeDFlow";

        // Sandbox enrollment-json örnek cevabı
        private const string EnrolledResponse =
            "{\"Status\":\"Y\",\"ResponseCode\":\"00\",\"ResponseMessage\":\"Success\",\"IsEnrolled\":true," +
            "\"AcsUrl\":\"" + AcsUrl + "\",\"PaReq\":\"BASE64_PAREQ\"," +
            "\"TermUrl\":\"https://inbound.apigateway.vakifbank.com.tr:8443/threeDGateway/startThreeDFlow\"," +
            "\"MD\":\"VERES_MD&1\",\"VerifyEnrollmentRequestId\":\"x\"}";

        // Rehber §4.2 örnek cevabı: sarmalanmış, TermUrl yok, alan adları farklı harfli
        private const string WrappedEnrolledResponse =
            "{\"ProcessEnrollmentResult\":{\"Pareq\":\"BASE64_PAREQ\",\"MD\":\"eyJhbGci#https://x#47892\"," +
            "\"ACSUrl\":\"" + AcsUrl + "\",\"ResultCode\":\"0000\",\"Status\":\"Y\",\"Version\":\"0.03\"," +
            "\"VerifyEnrollmentRequestId\":\"x\",\"MessageId\":\"0015\",\"ErrorMessage\":\"\",\"MessageErrorCode\":\"200\",\"ActualBrand\":200}}";

        // Rehber §6.4 örnek cevabı
        private const string ApprovedResponse =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><VposResponse><TransactionType>Sale</TransactionType><AuthCode>519558</AuthCode>" +
            "<ResultDetail>İŞLEM BAŞARILI</ResultDetail><BatchNo>1314</BatchNo><MerchantId>000100000013506</MerchantId>" +
            "<CurrencyAmount>10.50</CurrencyAmount><TransactionId>d518bf07-d66d-4408-8dd5-9284d1ca121d</TransactionId>" +
            "<Rrn>620238935583</Rrn><CurrencyCode>949</CurrencyCode><ThreeDSecureType>2</ThreeDSecureType>" +
            "<ResultCode>0000</ResultCode><HostDate>20260721135142</HostDate></VposResponse>";

        private static VakifBankConfiguration VakifConfiguration => (VakifBankConfiguration)Definition.BankConfiguration[BankTypes.VakifBank];

        private static (VakifBankProviderService Provider, CaptureHttpHandler Handler) CreateWithFakeBank(string responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handler = new CaptureHttpHandler(_ => CaptureHttpHandler.Response(statusCode, Encoding.UTF8.GetBytes(responseBody), "text/xml; charset=utf-8"));
            var provider = new VakifBankProviderService(new SanalPosHttpClient(new HttpClient(handler))) { CurrentBank = BankTypes.VakifBank };

            return (provider, handler);
        }

        private static PaymentModel CreatePaymentModel(bool use3DSecure, int? installment = null, string cardNumber = "4938410157705591") => new PaymentModel
        {
            CreditCard = new CreditCardInfo
            {
                CardNumber = cardNumber,
                ExpireMonth = "12",
                ExpireYear = "2029",
                CVV2 = "453",
                CardHolderName = "Ayşe Şahin"
            },
            Order = new OrderInfo { OrderId = OrderId, Total = 10.5M, Installment = installment, IP = "::ffff:85.105.1.2" },
            Use3DSecure = use3DSecure,
            SessionId = "session-1"
        };

        private static VerifyPaymentModel CreateVerifyModel(int? installment = null) => new VerifyPaymentModel
        {
            Order = new OrderInfo { OrderId = OrderId, Total = 10.5M, Installment = installment, IP = "85.105.1.2" },
            SelectedBank = BankTypes.VakifBank
        };

        private static Dictionary<string, StringValues> ParseForm(string body) =>
            QueryHelpers.ParseQuery(body).ToDictionary(pair => pair.Key, pair => pair.Value);

        private static XElement ParsePrmstr(string body) => XElement.Parse(ParseForm(body)["prmstr"]);

        private static JsonElement ParseJson(string body) => JsonDocument.Parse(body).RootElement;

        private static Dictionary<string, string> ParseHiddenInputs(string html) =>
            Regex.Matches(html, "<input type=\"hidden\" name=\"([^\"]+)\" value=\"([^\"]*)\" />")
                .Cast<Match>()
                .ToDictionary(m => m.Groups[1].Value, m => WebUtility.HtmlDecode(m.Groups[2].Value));

        // Rehber §15'teki tanımın provider'dan bağımsız uygulaması
        private static string BankHash(IDictionary<string, StringValues> callback)
        {
            var plain = string.Concat(
                callback["VerifyEnrollmentRequestId"], callback["MerchantId"], callback["PurchCurrency"],
                callback["PurchAmount"].ToString().Replace(".", string.Empty), callback["Eci"].ToString().PadLeft(2, '0'),
                callback["Cavv"], callback["MdStatus"], callback["Status"], VakifConfiguration.Password);

            using (var sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.GetEncoding("ISO-8859-9").GetBytes(plain)));
            }
        }

        private static Dictionary<string, StringValues> SignedCallback(string verifyEnrollmentRequestId, Action<Dictionary<string, StringValues>> change = null)
        {
            var callback = new Dictionary<string, StringValues>
            {
                ["MerchantId"] = VakifConfiguration.MerchantId,
                ["Pan"] = "49384101****5591",
                ["Expiry"] = "2912",
                ["PurchAmount"] = "1050",
                ["PurchCurrency"] = "949",
                ["VerifyEnrollmentRequestId"] = verifyEnrollmentRequestId,
                ["SessionInfo"] = "",
                ["Status"] = "Y",
                ["Cavv"] = "ABIBBxYAAAwfAAABCQAAAAAAAAA=",
                ["Eci"] = "5",
                ["MdStatus"] = "1",
                ["ErrorCode"] = "",
                ["ErrorMessage"] = ""
            };

            change?.Invoke(callback);
            callback["Hash"] = BankHash(callback);

            return callback;
        }

        private static async Task<string> EnrollAndGetRequestId(string orderId = OrderId)
        {
            var (provider, handler) = CreateWithFakeBank(EnrolledResponse);
            var paymentModel = CreatePaymentModel(use3DSecure: true);
            paymentModel.Order.OrderId = orderId;

            await provider.ProcessPayment(paymentModel);

            return ParseJson(handler.RequestBody).GetProperty("VerifyEnrollmentRequestId").GetString();
        }

        #region 3D Enrollment

        [Test]
        public async Task ProcessPayment_3D_SendsEnrollmentAndReturnsAcsForm()
        {
            var (provider, handler) = CreateWithFakeBank(EnrolledResponse);

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true));
            var request = ParseJson(handler.RequestBody);
            var inputs = ParseHiddenInputs(result.ServerResponseRaw);

            Assert.That(handler.RequestUri, Is.EqualTo("https://inbound.apigatewaytest.vakifbank.com.tr:8443/threeDGateway/ProcessEnrollment"));
            Assert.That(handler.RequestContentTypes.Single(), Does.StartWith("application/json"));
            Assert.That(request.GetProperty("Pan").GetString(), Is.EqualTo("4938410157705591"));
            Assert.That(request.GetProperty("ExpiryDate").GetString(), Is.EqualTo("2912"));
            Assert.That(request.GetProperty("PurchaseAmount").ValueKind, Is.EqualTo(JsonValueKind.Number));
            Assert.That(request.GetProperty("PurchaseAmount").GetDecimal(), Is.EqualTo(10.5M));
            Assert.That(request.GetProperty("Currency").GetString(), Is.EqualTo("949"));
            Assert.That(request.GetProperty("MerchantId").GetString(), Is.EqualTo("000100000013506"));
            Assert.That(request.GetProperty("MerchantPassword").GetString(), Is.EqualTo("123456"));
            Assert.That(request.GetProperty("SuccessUrl").GetString(), Is.EqualTo("https://x.com/Payment/ValidatePayment/session-1"));
            Assert.That(request.GetProperty("FailureUrl").GetString(), Is.EqualTo("https://x.com/Payment/ValidatePayment/session-1"));
            Assert.That(request.GetProperty("BrowserIpAddress").GetString(), Is.EqualTo("85.105.1.2"));
            Assert.That(request.GetProperty("VerifyEnrollmentRequestId").GetString(), Does.Match("^[0-9a-f]{40}$"));
            Assert.That(request.TryGetProperty("BrandName", out _), Is.False);
            Assert.That(request.TryGetProperty("InstallmentCount", out _), Is.False);
            Assert.That(request.TryGetProperty("SessionInfo", out _), Is.False);

            Assert.That(result.Status, Is.True);
            Assert.That(result.IsRedirectContent, Is.True);
            Assert.That(result.ServerResponseRaw, Does.Contain($"action=\"{AcsUrl}\""));
            Assert.That(inputs["PaReq"], Is.EqualTo("BASE64_PAREQ"));
            Assert.That(inputs["TermUrl"], Is.EqualTo("https://inbound.apigateway.vakifbank.com.tr:8443/threeDGateway/startThreeDFlow"));
            Assert.That(inputs["MD"], Is.EqualTo("VERES_MD&1"));
        }

        [Test]
        public async Task ProcessPayment_3D_WrappedResponseWithoutTermUrl_ReturnsAcsForm()
        {
            var (provider, _) = CreateWithFakeBank(WrappedEnrolledResponse);

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true));
            var inputs = ParseHiddenInputs(result.ServerResponseRaw);

            Assert.That(result.Status, Is.True);
            Assert.That(result.ServerResponseRaw, Does.Contain($"action=\"{AcsUrl}\""));
            Assert.That(inputs["PaReq"], Is.EqualTo("BASE64_PAREQ"));
            Assert.That(inputs["MD"], Is.EqualTo("eyJhbGci#https://x#47892"));
            Assert.That(inputs.ContainsKey("TermUrl"), Is.False);
        }

        [Test]
        public async Task ProcessPayment_3D_EachAttemptGetsANewVerifyEnrollmentRequestId()
        {
            Assert.That(await EnrollAndGetRequestId(), Is.Not.EqualTo(await EnrollAndGetRequestId()));
        }

        [Test]
        public async Task ProcessPayment_3D_Installment_IsSentToEnrollment()
        {
            var (provider, handler) = CreateWithFakeBank(EnrolledResponse);

            await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true, installment: 3));

            Assert.That(ParseJson(handler.RequestBody).GetProperty("InstallmentCount").GetInt32(), Is.EqualTo(3));
        }

        [Test]
        public async Task ProcessPayment_3D_RequestLogIsMasked()
        {
            var (provider, _) = CreateWithFakeBank(EnrolledResponse);

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true));

            Assert.That(result.OrderContentRaw, Does.Contain("\"Pan\":\"493841******5591\""));
            Assert.That(result.OrderContentRaw, Does.Contain("\"MerchantPassword\":\"***\""));
            Assert.That(result.OrderContentRaw, Does.Not.Contain("4938410157705591"));
        }

        [Test]
        public async Task ProcessPayment_3D_NotEnrolledCard_ReturnsFailure()
        {
            var (provider, _) = CreateWithFakeBank("{\"ProcessEnrollmentResult\":{\"Status\":\"N\",\"ResultCode\":\"0000\",\"ErrorMessage\":\"\",\"MessageErrorCode\":\"200\"}}");

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true));

            Assert.That(result.Status, Is.False);
            Assert.That(result.IsRedirectContent, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("3D-N"));
        }

        [Test]
        public async Task ProcessPayment_3D_EnrollmentError_ReturnsBankCodeAndMessage()
        {
            var (provider, _) = CreateWithFakeBank("{\"ProcessEnrollmentResult\":{\"Status\":\"E\",\"ErrorMessage\":\"Merchant cannot be found for this bank\",\"MessageErrorCode\":\"2005\"}}");

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("2005"));
            Assert.That(result.Error, Is.EqualTo("Merchant cannot be found for this bank"));
            Assert.That(result.ServerResponseRaw, Does.Contain("2005"));
        }

        [Test]
        public async Task ProcessPayment_3D_WafPage_ReportsSupportId()
        {
            var (provider, _) = CreateWithFakeBank("<html><head><title>Request Rejected</title></head><body>The requested URL was rejected. Please consult with your administrator.<br><br>Your support ID is: 6784419160722176229<br><br></body></html>");

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("WAF"));
            Assert.That(result.Error, Does.Contain("6784419160722176229"));
        }

        [TestCase("")]
        [TestCase("Service Unavailable")]
        [TestCase("[]")]
        public async Task ProcessPayment_3D_UnreadableResponse_ReturnsFailure(string response)
        {
            var (provider, _) = CreateWithFakeBank(response);

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: true));

            Assert.That(result.Status, Is.False);
            Assert.That(result.IsRedirectContent, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("3D-EMPTY"));
            Assert.That(result.Error, Is.EqualTo("Banka yanıtı okunamadı"));
        }

        #endregion

        #region 3D Callback

        [Test]
        public async Task VerifyPayment_FullSecureCallback_SendsCardlessProvision()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.VerifyPayment(CreateVerifyModel(installment: 3), new FormCollection(SignedCallback(requestId)));
            var request = ParsePrmstr(handler.RequestBody);

            Assert.That(handler.RequestUri, Is.EqualTo("https://inbound.apigatewaytest.vakifbank.com.tr:8443/virtualPos/Vposreq"));
            Assert.That(request.Element("TransactionType")?.Value, Is.EqualTo("Sale"));
            Assert.That(request.Element("MpiTransactionId")?.Value, Is.EqualTo(requestId));
            Assert.That(request.Element("ECI")?.Value, Is.EqualTo("05"));
            Assert.That(request.Element("CAVV")?.Value, Is.EqualTo("ABIBBxYAAAwfAAABCQAAAAAAAAA="));
            Assert.That(request.Element("NumberOfInstallments")?.Value, Is.EqualTo("3"));
            Assert.That(request.Element("OrderId")?.Value, Is.EqualTo(OrderId));
            Assert.That(request.Element("ClientIp")?.Value, Is.EqualTo("85.105.1.2"));
            Assert.That(request.Element("TransactionDeviceSource")?.Value, Is.EqualTo("0"));
            Assert.That(request.Element("TerminalNo")?.Value, Is.EqualTo("VP000579"));
            Assert.That(request.Element("TransactionId")?.Value, Does.Match("^[0-9a-f]{30}$"));
            foreach (var cardOrAmountField in new[] { "Pan", "Expiry", "Cvv", "CurrencyAmount", "CurrencyCode" })
                Assert.That(request.Element(cardOrAmountField), Is.Null, cardOrAmountField);

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("519558"));
            Assert.That(result.ReferanceNumber, Is.EqualTo("d518bf07-d66d-4408-8dd5-9284d1ca121d"));
            Assert.That(result.OrderContentRaw, Does.Contain("<Password>***</Password>"));
        }

        [Test]
        public async Task VerifyPayment_SingleInstallment_OmitsNumberOfInstallments()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);

            await provider.VerifyPayment(CreateVerifyModel(installment: 1), new FormCollection(SignedCallback(requestId)));

            Assert.That(ParsePrmstr(handler.RequestBody).Element("NumberOfInstallments"), Is.Null);
        }

        [Test]
        public async Task VerifyPayment_FailedAuthentication_ReturnsBankErrorWithoutProvision()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);
            var callback = SignedCallback(requestId, c =>
            {
                c["Status"] = "N";
                c["ErrorCode"] = "400";
                c["ErrorMessage"] = "3D Şifre doğrulaması yapılamadı.";
            });
            callback.Remove("Hash");

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(callback));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("400"));
            Assert.That(result.Error, Is.EqualTo("3D Şifre doğrulaması yapılamadı."));
            Assert.That(handler.CallCount, Is.Zero);
        }

        [Test]
        public async Task VerifyPayment_TamperedCallback_IsRejected()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);
            var callback = SignedCallback(requestId);
            callback["Eci"] = "06";

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(callback));

            Assert.That(result.ErrorCode, Is.EqualTo("HASH"));
            Assert.That(handler.CallCount, Is.Zero);
        }

        [Test]
        public async Task VerifyPayment_CallbackOfAnotherOrder_IsRejected()
        {
            var requestIdOfOtherOrder = await EnrollAndGetRequestId("LV-OTHER");
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(SignedCallback(requestIdOfOtherOrder)));

            Assert.That(result.ErrorCode, Is.EqualTo("ORDER"));
            Assert.That(handler.CallCount, Is.Zero);
        }

        [Test]
        public async Task VerifyPayment_ForgedRequestId_IsRejected()
        {
            var (provider, _) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(SignedCallback(new string('a', 40))));

            Assert.That(result.ErrorCode, Is.EqualTo("ORDER"));
        }

        [Test]
        public async Task VerifyPayment_OtherMerchant_IsRejected()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, _) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(SignedCallback(requestId, c => c["MerchantId"] = "000000000007955")));

            Assert.That(result.ErrorCode, Is.EqualTo("MERCHANT"));
        }

        [TestCase("105", "949")]
        [TestCase("1050", "840")]
        public async Task VerifyPayment_AmountOrCurrencyMismatch_IsRejected(string purchAmount, string purchCurrency)
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, _) = CreateWithFakeBank(ApprovedResponse);
            var callback = SignedCallback(requestId, c =>
            {
                c["PurchAmount"] = purchAmount;
                c["PurchCurrency"] = purchCurrency;
            });

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(callback));

            Assert.That(result.ErrorCode, Is.EqualTo("AMOUNT"));
        }

        [Test]
        public async Task VerifyPayment_DottedPurchAmount_IsAccepted()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, _) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(SignedCallback(requestId, c => c["PurchAmount"] = "10.50")));

            Assert.That(result.Status, Is.True);
        }

        [TestCase("06", "1", "ABIBBxYAAAwfAAABCQAAAAAAAAA=")]
        [TestCase("01", "1", "ABIBBxYAAAwfAAABCQAAAAAAAAA=")]
        [TestCase("05", "0", "ABIBBxYAAAwfAAABCQAAAAAAAAA=")]
        [TestCase("05", "1", "")]
        public async Task VerifyPayment_NotFullSecure_IsRejected(string eci, string mdStatus, string cavv)
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);
            var callback = SignedCallback(requestId, c =>
            {
                c["Eci"] = eci;
                c["MdStatus"] = mdStatus;
                c["Cavv"] = cavv;
            });

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(callback));

            Assert.That(result.ErrorCode, Is.EqualTo("3D-SECURE"));
            Assert.That(handler.CallCount, Is.Zero);
        }

        [Test]
        public async Task VerifyPayment_HalfSecureStatus_IsRejected()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(SignedCallback(requestId, c => c["Status"] = "A")));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("3D-A"));
            Assert.That(handler.CallCount, Is.Zero);
        }

        [Test]
        public async Task VerifyPayment_ProvisionDecline_ReturnsBankCode()
        {
            var requestId = await EnrollAndGetRequestId();
            var (provider, _) = CreateWithFakeBank("<VposResponse><ResultCode>1127</ResultCode><ResultDetail>3Ds İşlemlerde Kart Bilgisi Veya Tutar Provizyon Mesajında Yer Almamalıdır</ResultDetail></VposResponse>");

            var result = await provider.VerifyPayment(CreateVerifyModel(), new FormCollection(SignedCallback(requestId)));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("1127"));
        }

        #endregion

        #region Non-secure

        [Test]
        public async Task ProcessPayment_NonSecure_SendsSaleWithCard()
        {
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: false, installment: 6));
            var request = ParsePrmstr(handler.RequestBody);

            Assert.That(handler.RequestUri, Is.EqualTo("https://inbound.apigatewaytest.vakifbank.com.tr:8443/virtualPos/Vposreq"));
            Assert.That(handler.RequestContentTypes.Single(), Does.StartWith("application/x-www-form-urlencoded"));
            Assert.That(request.Element("MerchantId")?.Value, Is.EqualTo("000100000013506"));
            Assert.That(request.Element("Password")?.Value, Is.EqualTo("123456"));
            Assert.That(request.Element("Pan")?.Value, Is.EqualTo("4938410157705591"));
            Assert.That(request.Element("Expiry")?.Value, Is.EqualTo("202912"));
            Assert.That(request.Element("Cvv")?.Value, Is.EqualTo("453"));
            Assert.That(request.Element("CurrencyAmount")?.Value, Is.EqualTo("10.50"));
            Assert.That(request.Element("CurrencyCode")?.Value, Is.EqualTo("949"));
            Assert.That(request.Element("NumberOfInstallments")?.Value, Is.EqualTo("6"));
            Assert.That(request.Element("ClientIp")?.Value, Is.EqualTo("85.105.1.2"));
            Assert.That(request.Element("ECI"), Is.Null);

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("519558"));
            Assert.That(result.ReferanceNumber, Is.EqualTo("d518bf07-d66d-4408-8dd5-9284d1ca121d"));
            Assert.That(result.OrderContentRaw, Does.Contain("<Pan>493841******5591</Pan>").And.Contain("<Cvv>***</Cvv>").And.Not.Contain("123456"));
        }

        [Test]
        public async Task ProcessPayment_NonSecure_TwoDigitYear_IsExpanded()
        {
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);
            var paymentModel = CreatePaymentModel(use3DSecure: false);
            paymentModel.CreditCard.ExpireYear = "29";
            paymentModel.CreditCard.ExpireMonth = "3";

            await provider.ProcessPayment(paymentModel);

            Assert.That(ParsePrmstr(handler.RequestBody).Element("Expiry")?.Value, Is.EqualTo("202903"));
        }

        [Test]
        public async Task ProcessPayment_NonSecure_EscapesXml()
        {
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);
            var paymentModel = CreatePaymentModel(use3DSecure: false);
            paymentModel.Order.OrderId = "LV<&>1";

            await provider.ProcessPayment(paymentModel);

            Assert.That(ParsePrmstr(handler.RequestBody).Element("OrderId")?.Value, Is.EqualTo("LV<&>1"));
        }

        [Test]
        public async Task ProcessPayment_HttpError_ReportsStatusCode()
        {
            var (provider, _) = CreateWithFakeBank("Service Unavailable", HttpStatusCode.ServiceUnavailable);

            var result = await provider.ProcessPayment(CreatePaymentModel(use3DSecure: false));

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("HTTP-503"));
        }

        #endregion

        #region Refund

        [Test]
        public async Task ProcessRefound_SendsReferenceAndAmount()
        {
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse.Replace("<TransactionType>Sale</TransactionType>", "<TransactionType>Refund</TransactionType>"));

            var result = await provider.ProcessRefound(new Refund
            {
                OrderId = OrderId,
                ReferanceNumber = "d518bf07-d66d-4408-8dd5-9284d1ca121d",
                RefundAmount = 2.5M,
                ClientIp = "10.0.0.5"
            });
            var request = ParsePrmstr(handler.RequestBody);

            Assert.That(request.Element("TransactionType")?.Value, Is.EqualTo("Refund"));
            Assert.That(request.Element("ReferenceTransactionId")?.Value, Is.EqualTo("d518bf07-d66d-4408-8dd5-9284d1ca121d"));
            Assert.That(request.Element("CurrencyAmount")?.Value, Is.EqualTo("2.50"));
            Assert.That(request.Element("ClientIp")?.Value, Is.EqualTo("10.0.0.5"));
            Assert.That(request.Element("CurrencyCode"), Is.Null);
            Assert.That(result.Status, Is.True);
        }

        [TestCase(null, 1, "10.0.0.5", "REFERENCE")]
        [TestCase("ref", 0, "10.0.0.5", "AMOUNT")]
        [TestCase("ref", 1, null, "CLIENTIP")]
        public async Task ProcessRefound_MissingField_IsRejectedWithoutCallingBank(string reference, decimal amount, string clientIp, string errorCode)
        {
            var (provider, handler) = CreateWithFakeBank(ApprovedResponse);

            var result = await provider.ProcessRefound(new Refund { ReferanceNumber = reference, RefundAmount = amount, ClientIp = clientIp });

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo(errorCode));
            Assert.That(handler.CallCount, Is.Zero);
        }

        #endregion

        #region Handler

        [Test]
        public void Handler_Decline_MapsResultCodeAndDetail()
        {
            var provider = CreateWithFakeBank(string.Empty).Provider;

            var result = provider.Handler("<?xml version=\"1.0\" encoding=\"UTF-8\"?><VposResponse><ResultCode>0312</ResultCode><ResultDetail>KARTIN CVV2 DEĞERİ HATALI</ResultDetail><TransactionId>t1</TransactionId></VposResponse>");

            Assert.That(result.Status, Is.False);
            Assert.That(result.ErrorCode, Is.EqualTo("0312"));
            Assert.That(result.Error, Is.EqualTo("KARTIN CVV2 DEĞERİ HATALI"));
        }

        [Test]
        public void Handler_WrappedItems_AreRead()
        {
            var provider = CreateWithFakeBank(string.Empty).Provider;

            var result = provider.Handler("<VposResponse><_items><DTOTkFlexSaleRes><AuthCode>477082</AuthCode><TransactionId>test07</TransactionId><ResultCode>0000</ResultCode></DTOTkFlexSaleRes></_items></VposResponse>");

            Assert.That(result.Status, Is.True);
            Assert.That(result.ReferanceNumber, Is.EqualTo("test07"));
        }

        [TestCase("")]
        [TestCase("<html>Service Unavailable<br></html>")]
        [TestCase("<VposResponse><ResultDetail>x</ResultDetail></VposResponse>")]
        public void Handler_UnreadableResponse_ReturnsFailure(string response)
        {
            var result = CreateWithFakeBank(string.Empty).Provider.Handler(response);

            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Is.EqualTo("Banka yanıtı okunamadı"));
        }

        #endregion

        [Test]
        public void VakifBank_IsRegistered()
        {
            var providerService = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderService>>();

            Assert.That(providerService(BankTypes.VakifBank), Is.TypeOf<VakifBankProviderService>());
            Assert.That(Definition.BankConfiguration[BankTypes.VakifBank], Is.TypeOf<VakifBankConfiguration>());
            Assert.That(Definition.BankProdUrls[BankTypes.VakifBank].BaseUrl, Is.EqualTo("https://apigw.vakifbank.com.tr:8443"));
        }
    }
}

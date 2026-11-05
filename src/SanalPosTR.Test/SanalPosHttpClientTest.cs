using NUnit.Framework;
using SanalPosTR.Test.Helpers;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SanalPosTR.Test
{
    [TestFixture]
    public class SanalPosHttpClientTest
    {
        private static (SanalPosHttpClient Client, CaptureHttpHandler Handler) Create(HttpStatusCode statusCode, string body, string contentType = "application/json", params (string Name, string Value)[] headers)
        {
            var handler = new CaptureHttpHandler(_ => CaptureHttpHandler.Response(statusCode, Encoding.UTF8.GetBytes(body), contentType, headers));
            return (new SanalPosHttpClient(new HttpClient(handler)), handler);
        }

        // NestPay'in gönderdiği biçim: form-urlencoded, DATA= ön eki, XML gövde
        private static PostForm NestPayForm() => new PostForm
        {
            ContentType = "application/x-www-form-urlencoded",
            PreTag = "DATA=",
            Content = "<CC5Request/>",
            SendParameterType = SendParameterType.RequestBody
        };

        private static PostForm SignedJsonForm()
        {
            var form = new PostForm { ContentType = "application/json", Content = "{\"a\":1}" };
            form.Headers["auth-hash"] = "request-signature";
            return form;
        }

        #region Send

        [Test]
        public async Task Send_ReturnsStatusBodyRawBytesAndHeaders()
        {
            var (client, _) = Create(HttpStatusCode.OK, "{\"m\":\"BAŞARILI\"}", "application/json", ("auth-hash", "response-signature"));

            var response = await client.Send("https://bank.test", "api", SignedJsonForm());

            Assert.That(response.IsSuccess, Is.True);
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.That(response.Body, Is.EqualTo("{\"m\":\"BAŞARILI\"}"));
            Assert.That(response.RawBody, Is.EqualTo(Encoding.UTF8.GetBytes("{\"m\":\"BAŞARILI\"}")));
            Assert.That(response.Headers["AUTH-HASH"], Is.EqualTo("response-signature"));
            Assert.That(response.Headers["Content-Type"], Is.EqualTo("application/json"));
        }

        [Test]
        public async Task Send_SendsCustomRequestHeaders()
        {
            var (client, handler) = Create(HttpStatusCode.OK, "{}");

            await client.Send("https://bank.test", "api", SignedJsonForm());

            Assert.That(handler.RequestHeaders["auth-hash"], Is.EqualTo("request-signature"));
            Assert.That(handler.RequestBody, Is.EqualTo("{\"a\":1}"));
        }

        [Test]
        public async Task Send_NonSuccess_ReturnsResponseInsteadOfThrowing()
        {
            var (client, _) = Create(HttpStatusCode.Unauthorized, "{\"code\":\"401\",\"message\":\"Unauthorized\"}");

            var response = await client.Send("https://bank.test", "api", SignedJsonForm());

            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(401));
            Assert.That(response.Body, Does.Contain("Unauthorized"));
        }

        [Test]
        public async Task Send_ContentHeaderInHeaders_ReplacesContentValue()
        {
            var (client, handler) = Create(HttpStatusCode.OK, "{}");
            var form = SignedJsonForm();
            form.Headers["Content-Type"] = "application/xml";

            await client.Send("https://bank.test", "api", form);

            Assert.That(handler.RequestContentTypes, Is.EqualTo(new[] { "application/xml" }));
            Assert.That(handler.RequestHeaders.ContainsKey("Content-Type"), Is.False);
        }

        #endregion

        #region Post (mevcut bankaların yolu)

        [Test]
        public async Task Post_Success_SendsSameRequestAsBefore()
        {
            var (client, handler) = Create(HttpStatusCode.OK, "<Response>Approved</Response>", "text/xml");

            var result = await client.Post("https://bank.test/", "/fim/api", NestPayForm(), response => new PaymentResult { Status = response.Contains("Approved") });

            Assert.That(result.Status, Is.True);
            Assert.That(handler.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(handler.RequestUri, Is.EqualTo("https://bank.test/fim/api"));
            Assert.That(handler.RequestBody, Is.EqualTo("DATA=<CC5Request/>"));
            Assert.That(handler.RequestContentTypes, Is.EqualTo(new[] { "application/x-www-form-urlencoded; charset=utf-8" }));
            Assert.That(handler.RequestHeaders, Is.Empty);
            Assert.That(result.ServerResponseRaw, Is.EqualTo("<Response>Approved</Response>"));
            Assert.That(result.OrderContentRaw, Is.EqualTo("<CC5Request/>"));
        }

        [Test]
        public async Task Post_Failure_ReturnsTimeoutResultWithoutCallingHandler()
        {
            var (client, _) = Create(HttpStatusCode.InternalServerError, "Service Unavailable", "text/plain");
            var handlerCalled = false;

            var result = await client.Post("https://bank.test", "fim/api", NestPayForm(), response =>
            {
                handlerCalled = true;
                return new PaymentResult { Status = true };
            });

            Assert.That(handlerCalled, Is.False);
            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Is.EqualTo("[TIMEOUT]"));
            Assert.That(result.ServerResponseRaw, Is.EqualTo("Service Unavailable"));
            Assert.That(result.OrderContentRaw, Is.EqualTo("<CC5Request/>"));
        }

        [Test]
        public async Task PostGeneric_Success_ReturnsHandlerResult()
        {
            var (client, handler) = Create(HttpStatusCode.OK, "<form id=\"downloadForm\"/>", "text/html");

            var result = await client.Post("https://bank.test", "gate", NestPayForm(), response => response.Replace("downloadForm", "SanalPosTR"));

            Assert.That(result, Is.EqualTo("<form id=\"SanalPosTR\"/>"));
            Assert.That(handler.RequestHeaders, Is.Empty);
        }

        [Test]
        public async Task PostGeneric_Failure_ReturnsDefaultWithoutCallingHandler()
        {
            var (client, _) = Create(HttpStatusCode.BadGateway, "bad gateway", "text/plain");
            var handlerCalled = false;

            var result = await client.Post<string>("https://bank.test", "gate", NestPayForm(), response =>
            {
                handlerCalled = true;
                return response;
            });

            Assert.That(handlerCalled, Is.False);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostGeneric_SendsCustomHeaders()
        {
            var (client, handler) = Create(HttpStatusCode.OK, "ok");

            await client.Post<string>("https://bank.test", "api", SignedJsonForm(), response => response);

            Assert.That(handler.RequestHeaders["auth-hash"], Is.EqualTo("request-signature"));
        }

        #endregion
    }
}

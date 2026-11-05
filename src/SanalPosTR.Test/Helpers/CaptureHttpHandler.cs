using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SanalPosTR.Test.Helpers
{
    internal class CaptureHttpHandler : HttpMessageHandler
    {
        private readonly Func<string, HttpResponseMessage> _respond;

        public CaptureHttpHandler(Func<string, HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        public int CallCount { get; private set; }

        public HttpMethod Method { get; private set; }

        public string RequestUri { get; private set; }

        public string RequestBody { get; private set; }

        public Dictionary<string, string> RequestHeaders { get; private set; }

        public List<string> RequestContentTypes { get; private set; }

        public static HttpResponseMessage Response(HttpStatusCode statusCode, byte[] body, string contentType, params (string Name, string Value)[] headers)
        {
            var response = new HttpResponseMessage(statusCode) { Content = new ByteArrayContent(body) };
            response.Content.Headers.TryAddWithoutValidation("Content-Type", contentType);

            foreach (var header in headers)
                response.Headers.TryAddWithoutValidation(header.Name, header.Value);

            return response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            Method = request.Method;
            RequestUri = request.RequestUri.ToString();
            RequestBody = await request.Content.ReadAsStringAsync();
            RequestHeaders = request.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value));
            RequestContentTypes = request.Content.Headers.GetValues("Content-Type").ToList();

            return _respond(RequestBody);
        }
    }
}

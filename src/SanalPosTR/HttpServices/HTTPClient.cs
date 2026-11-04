using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SanalPosTR
{
    public class SanalPosHttpClient
    {
        private readonly HttpClient _httpClient;

        public SanalPosHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> Post<T>(string baseUrl, string resource, PostForm post, Func<string, T> handler)
        {
            var response = await Send(baseUrl, resource, post);

            return response.IsSuccess ? handler(response.Body) : default;
        }

        public async Task<PaymentResult> Post(string baseUrl, string resource, PostForm post, Func<string, PaymentResult> handler)
        {
            var response = await Send(baseUrl, resource, post);

            var paymentResult = response.IsSuccess
                ? handler(response.Body)
                : new PaymentResult { Status = false, Error = "[TIMEOUT]" };

            paymentResult.ServerResponseRaw = response.Body;
            paymentResult.OrderContentRaw = post.Content;

            return paymentResult;
        }

        /// <summary>
        /// İsteği gönderir ve yanıtı yorumlamadan döndürür; başarı ve hata anlamını çağıran belirler.
        /// </summary>
        public async Task<SanalPosHttpResponse> Send(string baseUrl, string resource, PostForm post)
        {
            var requestUri = CombineUrl(baseUrl, resource);

            using (var request = BuildRequest(requestUri, post))
            using (var response = await _httpClient.SendAsync(request))
            {
                var result = new SanalPosHttpResponse
                {
                    StatusCode = (int)response.StatusCode,
                    IsSuccess = response.IsSuccessStatusCode,
                    Body = await response.Content.ReadAsStringAsync(),
                    RawBody = await response.Content.ReadAsByteArrayAsync()
                };

                foreach (var header in response.Headers.Concat(response.Content.Headers))
                    result.Headers[header.Key] = string.Join(", ", header.Value);

                if (!result.IsSuccess)
                    Log.Warning("HTTP POST failed. Uri: {RequestUri}, StatusCode: {StatusCode}, Response: {ResponseBody}", requestUri, result.StatusCode, result.Body);

                return result;
            }
        }

        private static HttpRequestMessage BuildRequest(string requestUri, PostForm post)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = BuildContent(post)
            };

            foreach (var header in post.Headers)
            {
                // Content-Type gibi gövde başlıkları request.Headers'a eklenemez; gövdedeki değeri ezer.
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    request.Content.Headers.Remove(header.Key);
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return request;
        }

        private static HttpContent BuildContent(PostForm post)
        {
            string body;

            if (post.SendParameterType == SendParameterType.RequestBody)
            {
                body = string.IsNullOrEmpty(post.PreTag)
                    ? post.Content
                    : post.PreTag + post.Content;
            }
            else
            {
                if (post.Parameters.Count == 0)
                    body = post.PreTag + post.Content;
                else
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < post.Parameters.Count; i++)
                    {
                        if (i > 0) sb.Append('&');
                        sb.Append(Uri.EscapeDataString(post.Parameters[i].Key));
                        sb.Append('=');
                        sb.Append(Uri.EscapeDataString(post.Parameters[i].Value?.ToString() ?? ""));
                    }
                    body = sb.ToString();
                }
            }

            var mediaType = string.IsNullOrEmpty(post.ContentType) ? "application/x-www-form-urlencoded" : post.ContentType;

            return new StringContent(body, Encoding.UTF8, mediaType);
        }

        private static string CombineUrl(string baseUrl, string resource)
        {
            if (string.IsNullOrEmpty(resource))
                return baseUrl;

            baseUrl = baseUrl.TrimEnd('/');
            resource = resource.TrimStart('/');
            return $"{baseUrl}/{resource}";
        }
    }
}

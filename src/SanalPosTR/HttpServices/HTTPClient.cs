using Serilog;
using System;
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
            var requestUri = CombineUrl(baseUrl, resource);

            var content = BuildContent(post);

            Log.Information($"HTTPPost:{requestUri}");

            var response = await _httpClient.PostAsync(requestUri, content);

            Log.Information($"HTTPPosted:{requestUri}, StatusCode = {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return handler(responseBody);
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                Log.Information(responseBody);
                return default;
            }
        }

        public async Task<PaymentResult> Post(string baseUrl, string resource, PostForm post, Func<string, PaymentResult> handler)
        {
            var requestUri = CombineUrl(baseUrl, resource);

            var content = BuildContent(post);

            Log.Information($"HTTPPost:{requestUri}");

            PaymentResult paymentResult;

            var response = await _httpClient.PostAsync(requestUri, content);

            Log.Information($"HTTPPosted:{requestUri}, StatusCode = {response.StatusCode}");

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                paymentResult = handler(responseBody);
            }
            else
            {
                Log.Information(responseBody);

                paymentResult = new PaymentResult();
                paymentResult.Status = false;
                paymentResult.Error = "[TIMEOUT]";
                paymentResult.ServerResponseRaw = responseBody;
            }

            paymentResult.ServerResponseRaw = responseBody;
            paymentResult.OrderContentRaw = post.Content;

            return paymentResult;
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

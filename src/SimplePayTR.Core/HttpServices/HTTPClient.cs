using RestSharp;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimplePayTR
{
    public class HTTPClient
    {
        public HTTPClient(string baseUrl)
        {
            this.BaseUrl = baseUrl;
        }

        public string BaseUrl
        {
            get;
            set;
        }

        public async Task<T> Post<T>(string resource, PostForm post, Func<string, T> handler)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            RestClient client = new RestClient(BaseUrl);
            RestRequest request = new RestRequest(resource, Method.POST);

            request.RequestFormat = post.RequestFormat;

            if (post.SendParameterType == SendParameterType.RequestBody)
            {
                if (string.IsNullOrEmpty(post.PreTag))
                    request.AddParameter(post.ContentType, post.Content, ParameterType.RequestBody);
                else
                    request.AddParameter(post.ContentType, post.PreTag + post.Content, ParameterType.RequestBody);
            }
            else
            {
                if (post.Parameters.Count == 0)
                    request.AddParameter(post.PreTag, post.Content);
                else
                    foreach (var item in post.Parameters)
                        request.AddParameter(item.Key, item.Value, ParameterType.QueryString);
            }

            Log.Information($"HTTPPost:{client.BaseUrl}/{request.Resource}");

            var serverResponse = client.Execute(request);

            Log.Information($"HTTPPosted:{client.BaseUrl}/{request.Resource}, StatusCode = {serverResponse.StatusCode}");

            if (serverResponse.StatusCode == HttpStatusCode.OK)
                return await Task.FromResult(handler(serverResponse.Content));
            else
            {
                Log.Information(serverResponse.Content);
                return default;
            }
        }

        public Task<PaymentResult> Post(string resource, PostForm post, Func<string, PaymentResult> handler)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            PaymentResult paymentResult;
            RestClient client = new RestClient(BaseUrl);
            RestRequest request = new RestRequest(resource, Method.POST);

            request.RequestFormat = post.RequestFormat;

            if (post.SendParameterType == SendParameterType.RequestBody)
            {
                if (string.IsNullOrEmpty(post.PreTag))
                    request.AddParameter(post.ContentType, post.Content, ParameterType.RequestBody);
                else
                    request.AddParameter(post.ContentType, post.PreTag + post.Content, ParameterType.RequestBody);
            }
            else
            {
                if (post.Parameters.Count == 0)
                    request.AddParameter(post.PreTag, post.Content);
                else
                    foreach (var item in post.Parameters)
                        request.AddParameter(item.Key, item.Value, ParameterType.QueryString);
            }

            Log.Information($"HTTPPost:{client.BaseUrl}/{request.Resource}");

            var serverResponse = client.Execute(request);

            Log.Information($"HTTPPosted:{client.BaseUrl}/{request.Resource}, StatusCode = {serverResponse.StatusCode}");

            if (serverResponse.StatusCode == HttpStatusCode.OK)
                paymentResult = handler(serverResponse.Content);
            else
            {
                Log.Information(serverResponse.Content);

                paymentResult = new PaymentResult();
                paymentResult.Status = false;
                paymentResult.Error = "[TIMEOUT]";
                paymentResult.ServerResponseRaw = serverResponse.Content;
            }

            paymentResult.ServerResponseRaw = serverResponse.Content;
            paymentResult.OrderContentRaw = post.Content;

            return Task.FromResult(paymentResult);
        }
    }
}
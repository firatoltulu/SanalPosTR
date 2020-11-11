using RestSharp;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimplePayTR
{
    public class HTTPClient
    {
        public HTTPClient(string url)
        {
            this.Url = url;
        }

        public string Url
        {
            get;
            set;
        }

        //public Task<PaymentResult> Get(PostForm post, Func<IRestResponse, PaymentResult> handler)
        //{
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

        //    Result result = new Result();

        //    StringBuilder spr = new StringBuilder();
        //    spr.Append(post.Request.Url);
        //    //spr.AppendFormat("/{0}?", post.ContentType);

        //    foreach (var item in post.Parameters)
        //    {
        //        spr.AppendFormat("{0}={1}&", item.Key, item.Value.ToString());
        //    }

        //    var baseUrl = spr.ToString();
        //    baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

        //    RestSharp.RestClient client = new RestSharp.RestClient(baseUrl);
        //    RestSharp.RestRequest request = new RestSharp.RestRequest(Method.GET);
        //    //request.Resource = post.ContentType;

        //    request.RequestFormat = DataFormat.Xml;
        //    request.Parameters.Clear();

        //    //foreach (var item in post.Parameters)
        //    //    request.AddParameter(item.Key, item.Value.ToString(), ParameterType.GetOrPost);

        //    //if (request.Parameters.LastOrDefault().Name == "Accept")
        //    //    request.Parameters.RemoveAt(request.Parameters.Count - 1);

        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

        //    var serverResponse = client.Execute(request);

        //    if (serverResponse.StatusCode == System.Net.HttpStatusCode.OK)
        //        result = handler(serverResponse);
        //    else
        //    {
        //        result = new Result();
        //        result.Status = false;
        //        result.Error = "[TIMEOUT]";
        //        result.RequestContent = serverResponse.Content;
        //    }

        //    result.RequestData = post.Request;

        //    return result;
        //}

        public async Task<T> Post<T>(PostForm post, Func<string, T> handler)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            RestClient client = new RestClient(Url);
            RestRequest request = new RestRequest(Method.POST);

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

            var serverResponse = client.Execute(request);

            if (serverResponse.StatusCode == HttpStatusCode.OK)
                return await Task.FromResult(handler(serverResponse.Content));
            else
                return default;
        }

        public Task<PaymentResult> Post(PostForm post, Func<string, PaymentResult> handler)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            PaymentResult paymentResult;
            RestClient client = new RestClient(Url);
            RestRequest request = new RestRequest(Method.POST);

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

            var serverResponse = client.Execute(request);

            if (serverResponse.StatusCode == HttpStatusCode.OK)
                paymentResult = handler(serverResponse.Content);
            else
            {
                paymentResult = new PaymentResult();
                paymentResult.Status = false;
                paymentResult.Error = "[TIMEOUT]";
                paymentResult.ServerResponseRaw = serverResponse.Content;
            }

            paymentResult.OrderContentRaw = post.Content;

            return Task.FromResult(paymentResult);
        }
    }
}
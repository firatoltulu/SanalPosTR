using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimplePayTR
{
     

    public class HTTPClient
    {
 
        public string Url
        {
            get;
            set;
        }
        public string TokenId { get; set; }

        public Result Get(Post post, Func<IRestResponse, Result> handler)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;


            Result result = new Result();

            StringBuilder spr = new StringBuilder();
            spr.Append(post.Request.Url);
            //spr.AppendFormat("/{0}?", post.ContentType);

            foreach (var item in post.Parameters)
            {
                spr.AppendFormat("{0}={1}&", item.Key, item.Value.ToString());
            }

            var baseUrl = spr.ToString();
            baseUrl = baseUrl.Substring(0, baseUrl.Length - 1);

            RestSharp.RestClient client = new RestSharp.RestClient(baseUrl);
            RestSharp.RestRequest request = new RestSharp.RestRequest(Method.GET);
            //request.Resource = post.ContentType;

            request.RequestFormat = DataFormat.Xml;
            request.Parameters.Clear();

            //foreach (var item in post.Parameters)
            //    request.AddParameter(item.Key, item.Value.ToString(), ParameterType.GetOrPost);

            //if (request.Parameters.LastOrDefault().Name == "Accept")
            //    request.Parameters.RemoveAt(request.Parameters.Count - 1);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            var serverResponse = client.Execute(request);

            if (serverResponse.StatusCode == System.Net.HttpStatusCode.OK)
                result = handler(serverResponse);
            else
            {
                result = new Result();
                result.Status = false;
                result.Error = "[TIMEOUT]";
                result.RequestContent = serverResponse.Content;
            }

            result.RequestData = post.Request;

            return result;
        }

        public Result Post(Post post,Func<IRestResponse,Result> handler)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;


            Result result = new Result();

            var content = post.Parameters.Count == 0 ? Model.ReadEmbedXML(post.ConfigName) : "";
            var postData =post.Parameters.Count == 0 ? Model.CreatePosXML(post.Request, content).Trim() : "";
            

            if (!post.Request.Is3D)
            {

                RestSharp.RestClient client = new RestSharp.RestClient(post.Request.Url);
                RestSharp.RestRequest request = new RestSharp.RestRequest(post.Method);
              
                if (post.Request.Accounts.ContainsKey("Method")) {
                    request.Resource = post.Request.Accounts["Method"].ToString();
                }

                request.RequestFormat = post.RequestFormat;

                if (!post.IsQueryParameter)
                    request.AddParameter(post.ContentType, post.PreTag + postData, RestSharp.ParameterType.RequestBody);
                else
                {
                    if (post.Parameters.Count == 0)
                    {
                        request.AddParameter(post.PreTag, postData);
                    }
                    else
                    {
                        request.Resource = post.ContentType;

                        foreach (var item in post.Parameters)
                        {
                            request.AddParameter(item.Key,item.Value, ParameterType.UrlSegment);
                        }
                    }
                }

                
                client.Timeout = Convert.ToInt32(20000);

                var serverResponse = client.Execute(request);

                if (serverResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    result = handler(serverResponse);
                else
                {
                    result = new Result();
                    result.Status = false;
                    result.Error = "[TIMEOUT]";
                    result.RequestContent = serverResponse.Content;
                }
            }
            else {
                result.Status = true;
                result.ResultContent = postData;
            }

            result.RequestData = post.Request;

            return result;
        }

        
        static HTTPClient _single = null;

        public static HTTPClient SingleInstance
        {
            get
            {
                if (_single == null)
                    _single = new HTTPClient();

                return _single;
            }
        }

    }
}

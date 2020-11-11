using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePayTR.Network
{
    public class YKB:IGate
    {
        public Result Pay(Request request)
        {
            Result rs = null;
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = !request.Is3D ? "YKB_Pay.xml" : "YKB_Pay_3D.xml";
            prepaire.ContentType = "application/x-www-form-urlencoded";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "xmldata=";

            if (request.Is3D)
            {
                request.Is3D = false;
                rs = HTTPClient.SingleInstance.Post(prepaire, Handler3D);

                if (rs.Status == true) {

                    prepaire.Request = new Request
                    {
                        Accounts = request.Accounts,
                        SuccessUrl = request.SuccessUrl,
                        ErrorUrl = request.ErrorUrl,
                        Pos = new RequestPos {
                            FullName = rs.ProvisionNumber,
                            EMail = rs.ReferanceNumber,
                            Ip = rs.Error
                        },
                        Is3D=true
                    };
                    prepaire.ConfigName = "YKB_Pay_3D.cshtml";
                    prepaire.ContentType = "application/x-www-form-urlencoded";
                    prepaire.RequestFormat = RestSharp.DataFormat.Xml;
                    prepaire.PreTag = "xmldata=";

                    request.Is3D = true;
                    rs = HTTPClient.SingleInstance.Post(prepaire, Handler3D);
                    rs.RequestData = request;
                }
            }
            else {
                rs = HTTPClient.SingleInstance.Post(prepaire, Handler);
            }

            return rs;
        }

        public Result Refound(Request request)
        {
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = "YKB_Refound.xml";
            prepaire.ContentType = "text/xml";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "";
            return HTTPClient.SingleInstance.Post(prepaire, Handler);
        }

        Result Handler(IRestResponse serverResponse)
        {
            Result result = new Result();

            result.ResultContent = serverResponse.Content;

            var hostResponse = Model.GetInlineContent(result.ResultContent, "approved");
            if (hostResponse == "1")
            {
                result.Status = true;
                result.ProvisionNumber = Model.GetInlineContent(result.ResultContent, "authCode");
                result.ReferanceNumber = Model.GetInlineContent(result.ResultContent, "hostlogkey");
                result.ProcessId = Model.GetInlineContent(result.ResultContent, "hostlogkey");
            }
            else
            {
                result.Error = Model.GetInlineContent(result.ResultContent, "respText");
                result.ErrorCode = Model.GetInlineContent(result.ResultContent, "respCode");
            }

            return result;
        }

        Result Handler3D(IRestResponse serverResponse)
        {
            Result result = new Result();

            result.ResultContent = serverResponse.Content;

            var hostResponse = Model.GetInlineContent(result.ResultContent, "approved");
            if (hostResponse == "1")
            {
                result.Status = true;
                result.ProvisionNumber = Model.GetInlineContent(result.ResultContent, "data1");
                result.ReferanceNumber = Model.GetInlineContent(result.ResultContent, "data2");
                result.Error = Model.GetInlineContent(result.ResultContent, "sign");
            }
            else
            {
                result.Error = Model.GetInlineContent(result.ResultContent, "respText");
                result.ErrorCode = Model.GetInlineContent(result.ResultContent, "respCode");
            }

            return result;
        }

        public List<NetworkConfigurationModel> GetNetworkConfiguration()
        {
            List<NetworkConfigurationModel> model = new List<NetworkConfigurationModel>();

            model.Add(new NetworkConfigurationModel
            {
                Key = "MId",
                Value = "",
                Description = "Mağaza No",
                iType = NetworkType.YKB
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "TId",
                Value = "",
                Description = "Terminal No",
                iType = NetworkType.YKB
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "Username",
                Value = "",
                Description = "Kullanıcı Adı",
                iType = NetworkType.YKB
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "Password",
                Value = "",
                Description = "Şifre",
                iType = NetworkType.YKB
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "PosnetId",
                Value = "",
                Description = "PosnetId",
                iType = NetworkType.YKB
            });

            
            return model;
        }






        public Result Pay3D(Request request, System.Collections.Specialized.NameValueCollection collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                request.Accounts.Add(collection.GetKey(i), collection[i]);
            }
            
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = "YKB_Pay_3DEnd.xml";
            prepaire.ContentType = "application/x-www-form-urlencoded";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "xmldata=";
            return HTTPClient.SingleInstance.Post(prepaire, Handler);
        }

        public bool Check3D(System.Collections.Specialized.NameValueCollection formCollection, Dictionary<string, object> accounts)
        {


            
        }
    }
}

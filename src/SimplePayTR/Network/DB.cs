using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SimplePayTR.Network
{
    public class DB : IGate
    {
        public Result Pay(Request request)
        {
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = !request.Is3D ? "GB_Pay.xml" : "DB_Pay_3D.cshtml";
            prepaire.ContentType = "text/xml";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "";

            return HTTPClient.SingleInstance.Post(prepaire, Handler);
        }

        public Result Refound(Request request)
        {
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = "DB_Refound.xml";
            prepaire.ContentType = "text/xml";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "";

            return HTTPClient.SingleInstance.Post(prepaire, Handler);
        }

        private Result Handler(IRestResponse serverResponse)
        {
            Result result = new Result();

            result.ResultContent = serverResponse.Content;

            var hostResponse = Model.GetInlineContent(result.ResultContent, "Message");
            if (hostResponse == "Approved")
            {
                result.Status = true;
                result.ProvisionNumber = Model.GetInlineContent(result.ResultContent, "AuthCode");
                result.ReferanceNumber = Model.GetInlineContent(result.ResultContent, "RetrefNum");
                result.ProcessId = Model.GetInlineContent(result.ResultContent, "OrderID");
            }
            else
            {
                result.Error = Model.GetInlineContent(result.ResultContent, "ReasonMessage");
                if (string.IsNullOrEmpty(result.Error))
                {
                    result.Error = Model.GetInlineContent(result.ResultContent, "ErrorMsg");
                }
                result.ErrorCode = Model.GetInlineContent(result.ResultContent, "ReasonCode");
            }

            return result;
        }

        public List<NetworkConfigurationModel> GetNetworkConfiguration()
        {
            List<NetworkConfigurationModel> model = new List<NetworkConfigurationModel>();

            model.Add(new NetworkConfigurationModel
            {
                Key = "MerchantId",
                Value = "",
                Description = "Mağaza No",
                iType = NetworkType.GB
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "Id",
                Value = "",
                Description = "Terminal Id",
                iType = NetworkType.GB
            });
            model.Add(new NetworkConfigurationModel
            {
                Key = "UserName",
                Value = "",
                Description = "İade Kullanıcı(PROVRFN) Id",
                iType = NetworkType.GB
            });
            model.Add(new NetworkConfigurationModel
            {
                Key = "Password",
                Value = "",
                Description = "Şifre",
                iType = NetworkType.GB
            });
            model.Add(new NetworkConfigurationModel
            {
                Key = "PROVSalesId",
                Value = "",
                Description = "Satış Kullanıcı",
                iType = NetworkType.GB
            });


            model.Add(new NetworkConfigurationModel
            {
                Key = "PROVPassword",
                Value = "",
                Description = "İade Kullanıcı Şifre(PROVRFN)",
                iType = NetworkType.GB
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "TerminalId",
                Value = "",
                Description = "3D - 8 Haneli Terminal No",
                iType = NetworkType.GB
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "StoreKey",
                Value = "",
                Description = "3D - Şifre",
                iType = NetworkType.GB
            });

            return model;
        }

        public Result Pay3D(Request request, System.Collections.Specialized.NameValueCollection collection)
        {
            var result = new Result(false, "");
            result.Error = collection["ErrorMessage"].ToString();
            result.ErrorCode = "XXX000";
            result.Status = collection["3DStatus"].StartsWith("-") == false;
            if (result.Status)
            {
                result.ReferanceNumber = collection["HostRefNum"];
                result.ProvisionNumber = collection["AuthCode"];
                result.Status = true;
            }

            return result;
        }

        public bool Check3D(System.Collections.Specialized.NameValueCollection formCollection, Dictionary<string, object> accounts)
        {
            string strMDStatus = formCollection.Get("mdstatus");
            return (strMDStatus == "1" | strMDStatus == "2" | strMDStatus == "3" | strMDStatus == "4");
        }
    }
}
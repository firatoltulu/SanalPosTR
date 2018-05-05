using RestSharp;
using System;
using System.Collections.Generic;

namespace SimplePayTR.Network
{
    public class GB : IGate
    {
        public Result Pay(Request request)
        {
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = !request.Is3D ? "GB_Pay.xml" : "GB_Pay_3D.cshtml";
            prepaire.ContentType = "text/xml";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "";

            return HTTPClient.SingleInstance.Post(prepaire, Handler);
        }

        public Result Refound(Request request)
        {
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = "GB_Refound.xml";
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
            result.Error = "XXX000";
            result.ErrorCode = "XXX000";

            if (true)
            {
                bool isValidHash = true;
                String responseHashparams = collection.Get("hashparams");
                String responseHashparamsval = collection.Get("hashparamsval");
                String responseHash = collection.Get("hash");
                String storekey = request.Accounts["StoreKey"].ToString();

                if (responseHashparams != null && !"".Equals(responseHashparams))
                {
                    String digestData = "";
                    char[] separator = new char[] { ':' };
                    String[] paramList = responseHashparams.Split(separator);

                    foreach (String param in paramList)
                    {
                        digestData += collection.Get(param) == null ? "" : collection.Get(param);
                    }

                    digestData += storekey;

                    System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                    byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(digestData);
                    byte[] inputbytes = sha.ComputeHash(hashbytes);
                    String hashCalculated = Convert.ToBase64String(inputbytes);

                    if (responseHash.Equals(hashCalculated))
                    {
                        isValidHash = true;
                    }
                    else
                    {
                        isValidHash = false;
                        result = new Result(false, "");
                        result.Error = "0";
                        result.ErrorCode = "";
                    }

                    if (isValidHash)
                    {
                        result = new Result(true,"");
                        result.ReferanceNumber = collection.Get("hostrefnum");
                        result.ProvisionNumber = collection.Get("authcode");
                      
                        /*request.Accounts.Add("AuthenticationCode ", collection.Get("authenticationcode"));
                        request.Accounts.Add("TxnID ", collection.Get("txnid"));
                        request.Accounts.Add("Md", collection.Get("md"));
                        request.Accounts.Add("Eci", collection.Get("eci"));
                        request.Accounts.Add("SuccessUrl", request.SuccessUrl);
                        request.Accounts.Add("ErrorUrl", request.ErrorUrl);

                        Post prepaire = new Post();
                        prepaire.Request = request;
                        prepaire.ConfigName = "GB_Pay_3DEnd.xml";
                        prepaire.ContentType = "text/xml";
                        prepaire.RequestFormat = RestSharp.DataFormat.Xml;
                        prepaire.PreTag = "";

                        return HTTPClient.SingleInstance.Post(prepaire, Handler);     */
                    }
                }
                else
                {
                    result = new Result(false, "");
                    result.Error = "0";
                    result.ErrorCode = "";
                }
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
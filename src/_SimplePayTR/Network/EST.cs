using RestSharp;
using System;
using System.Collections.Generic;

namespace SimplePayTR.Network
{
    internal class EST : IGate
    {
        public Result Pay(Request request)
        {
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = !request.Is3D ? "EST_Pay.xml" : "EST_Pay_3D.cshtml";
            prepaire.ContentType = "application/x-www-form-urlencoded";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "DATA=";

            return HTTPClient.SingleInstance.Post(prepaire, Handler);
        }

        private Result Handler(IRestResponse serverResponse)
        {
            Result result = new Result();

            result.ResultContent = serverResponse.Content;

            var hostResponse = Model.GetInlineContent(result.ResultContent, "Response");
            if (hostResponse == "Approved")
            {
                result.Status = true;
                result.ProvisionNumber = Model.GetInlineContent(result.ResultContent, "AuthCode");
                result.ReferanceNumber = Model.GetInlineContent(result.ResultContent, "HostRefNum");
                result.ProcessId = Model.GetInlineContent(result.ResultContent, "OrderId");
            }
            else
            {
                result.Error = Model.GetInlineContent(result.ResultContent, "ErrMsg");
                result.ErrorCode = Model.GetInlineContent(result.ResultContent, "ERRORCODE");
            }

            return result;
        }

        public Result Refound(Request request)
        {
            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = "EST_Refound.xml";
            prepaire.ContentType = "application/x-www-form-urlencoded";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.PreTag = "DATA=";

            return HTTPClient.SingleInstance.Post(prepaire, Handler);
        }

        public List<NetworkConfigurationModel> GetNetworkConfiguration()
        {
            List<NetworkConfigurationModel> model = new List<NetworkConfigurationModel>();

            model.Add(new NetworkConfigurationModel
            {
                Key = "ClientId",
                Value = "",
                Description = "Mağaza No",
                iType = NetworkType.EST
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "Name",
                Value = "",
                Description = "Kullanıcı Adı",
                iType = NetworkType.EST
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "Password",
                Value = "",
                Description = "Şifre",
                iType = NetworkType.EST
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "StoreKey",
                Value = "",
                Description = "3D Şifresi",
                iType = NetworkType.EST
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "Method",
                Value = "cc5ApiServer",
                Description = "Post Edilecek Metod",
                iType = NetworkType.EST
            });

            return model;
        }

        public Result Pay3D(Request request, System.Collections.Specialized.NameValueCollection collection)
        {
            Result result = new Result(false, "İmza Doğrulanamadı!");
            if (Check3D(collection, request.Accounts))
            {
                request.Accounts.Add("ProcessId", collection.Get("oid"));
                request.Accounts.Add("PayerTxnId", collection.Get("xid"));
                request.Accounts.Add("PayerSecurityLevel", collection.Get("eci"));
                request.Accounts.Add("PayerAuthenticationCode", collection.Get("cavv"));
                request.Accounts.Add("CardNumber", collection.Get("md"));

                Post prepaire = new Post();
                prepaire.Request = request;
                prepaire.ConfigName = "EST_Pay_3DEnd.xml";
                prepaire.ContentType = "application/x-www-form-urlencoded";
                prepaire.RequestFormat = RestSharp.DataFormat.Xml;
                prepaire.PreTag = "DATA=";

                return HTTPClient.SingleInstance.Post(prepaire, Handler);
            }
            return result;
        }

        public bool Check3D(System.Collections.Specialized.NameValueCollection formCollection, Dictionary<string, object> accounts)
        {
            bool result = false;

            String hashparams = formCollection.Get("HASHPARAMS");
            String hashparamsval = formCollection.Get("HASHPARAMSVAL");

            String paramsval = "";
            int index1 = 0, index2 = 0;

            do
            {
                index2 = hashparams.IndexOf(":", index1);
                String val = formCollection.Get(hashparams.Substring(index1, index2 - index1)) == null ? "" : formCollection.Get(hashparams.Substring(index1, index2 - index1));
                paramsval += val;
                index1 = index2 + 1;
            }
            while (index1 < hashparams.Length);

            String hashval = paramsval + accounts["StoreKey"];
            String hashparam = formCollection.Get("HASH");

            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] hashbytes = System.Text.Encoding.GetEncoding("ISO-8859-9").GetBytes(hashval);
            byte[] inputbytes = sha.ComputeHash(hashbytes);

            String hash = Convert.ToBase64String(inputbytes);

            if (!paramsval.Equals(hashparamsval) || !hash.Equals(hashparam))
            {
                return false;
            }

            var mdstatus = formCollection.Get("mdStatus");
            result = (mdstatus.Equals("1") || mdstatus.Equals("2") || mdstatus.Equals("3") || mdstatus.Equals("4"));
            return result;
        }
    }
}
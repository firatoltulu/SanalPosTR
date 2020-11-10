using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace SimplePayTR.Network
{
    internal class VPOS : IGate
    {

        public Result Pay(Request request)
        {

            Post prepaire = new Post();
            prepaire.Request = request;
            prepaire.ConfigName = request.Is3D ? "VPOS_Pay_3D.cshtml" : "";
            prepaire.ContentType = "vpos724v3";
            prepaire.RequestFormat = RestSharp.DataFormat.Xml;
            prepaire.IsQueryParameter = true;
            prepaire.PreTag = "";
            prepaire.Method = Method.GET;

            var expiredate = request.Pos.ExpireDate.Substring(2, 2) + request.Pos.ExpireDate.Substring(0, 2);
            var total = (request.Pos.Total * 100).ToString("N0").PadLeft(12, '0');
 

            if (request.Is3D)
            {
                return HTTPClient.SingleInstance.Post(prepaire, Handler);
            }
            else
            {
                prepaire.Parameters.Add("kullanici", request.Accounts["kullanici"]);
                prepaire.Parameters.Add("sifre", request.Accounts["sifre"]);
                prepaire.Parameters.Add("uyeno", request.Accounts["uyeno"]);
                prepaire.Parameters.Add("posno", request.Accounts["posno"]);
                prepaire.Parameters.Add("islem", "PRO");

                prepaire.Parameters.Add("kkno", request.Pos.CardNumber);
                prepaire.Parameters.Add("gectar", expiredate);
                prepaire.Parameters.Add("cvc", request.Pos.CVV2);
                prepaire.Parameters.Add("tutar", total);

                prepaire.Parameters.Add("provno", "000000");
                prepaire.Parameters.Add("taksits", (request.Pos.Installment == 1 || request.Pos.Installment == 0) ? "00" : request.Pos.Installment.ToString().PadLeft(2, '0'));
                prepaire.Parameters.Add("islemyeri", "I");
                prepaire.Parameters.Add("uyeref", request.Pos.ProcessId);
                prepaire.Parameters.Add("vbref", 0);
                prepaire.Parameters.Add("khip", request.Pos.Ip);
                prepaire.Parameters.Add("xcip", request.Accounts["xcip"]);
                return HTTPClient.SingleInstance.Get(prepaire, Handler);
            }

        }

        Result Handler(IRestResponse serverResponse)
        {
            Result result = new Result();


            result.ResultContent = serverResponse.Content;

            var hostResponse = Model.GetInlineContent(result.ResultContent, "Kod");

            if (hostResponse == "00")
            {
                result.Status = true;
                result.ProvisionNumber = Model.GetInlineContent(result.ResultContent, "Mesaj").Clean(true, false, true, "", true);
                result.ReferanceNumber = Model.GetInlineContent(result.ResultContent, "VBRef");
                result.ProcessId = Model.GetInlineContent(result.ResultContent, "UyeRef");
            }
            else
            {
                result.Error = Model.GetInlineContent(result.ResultContent, "Mesaj");
                result.ErrorCode = Model.GetInlineContent(result.ResultContent, "Kod");
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


            //var content = Model.ReadEmbedXML("EST_Refound.xml");
            //var postData = Model.CreatePosXML(request, content).Trim();

            //Result result = new Result();

            //result.iNetworkType = NetworkType.EST;

            //RestSharp.RestClient client = new RestSharp.RestClient(request.Url);
            //RestSharp.RestRequest response = new RestSharp.RestRequest(RestSharp.Method.POST);

            //response.RequestFormat = RestSharp.DataFormat.Xml;
            //response.AddParameter("application/x-www-form-urlencoded", string.Format("DATA={0}", postData), RestSharp.ParameterType.RequestBody);

            ////client.Timeout = 120 * 3;

            //var serverResponse = client.Execute(response);
            //if (serverResponse.StatusCode == System.Net.HttpStatusCode.OK)
            //{
            //    result.ResultContent = serverResponse.Content;

            //    var hostResponse = Model.GetInlineContent(result.ResultContent, "Response");
            //    if (hostResponse == "Approved")
            //    {
            //        result.ProvisionNumber = Model.GetInlineContent(result.ResultContent, "AuthCode");
            //        result.ReferanceNumber = Model.GetInlineContent(result.ResultContent, "HostRefNum");
            //    }
            //    else {
            //        result.Error = Model.GetInlineContent(result.ResultContent, "ErrMsg");
            //        result.ErrorCode = Model.GetInlineContent(result.ResultContent, "ERRORCODE");
            //    }
            //} return result;


        }

        public List<NetworkConfigurationModel> GetNetworkConfiguration()
        {
            List<NetworkConfigurationModel> model = new List<NetworkConfigurationModel>();

            model.Add(new NetworkConfigurationModel
            {
                Key = "kullanici",
                Value = "",
                Description = "kullanici",
                iType = NetworkType.VPOS
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "sifre",
                Value = "",
                Description = "sifre",
                iType = NetworkType.VPOS
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "uyeno",
                Value = "",
                Description = "uyeno",
                iType = NetworkType.VPOS
            });

            model.Add(new NetworkConfigurationModel
            {
                Key = "posno",
                Value = "",
                Description = "posno",
                iType = NetworkType.VPOS
            });
            model.Add(new NetworkConfigurationModel
            {
                Key = "xcip",
                Value = "",
                Description = "xcip",
                iType = NetworkType.VPOS
            });

            return model;
        }

        public Result Pay3D(Request request, System.Collections.Specialized.NameValueCollection collection)
        {
            throw new NotImplementedException();
        }

        public bool Check3D(System.Collections.Specialized.NameValueCollection formCollection, Dictionary<string, object> accounts)
        {
            var memberNumber = accounts["uyeno"].ToString().Trim();
            var memberRequestNumber = formCollection["uyeno"].ToString().Trim();
            var status = formCollection["status"].Trim();

            return (memberNumber == memberRequestNumber && status == "Y");

        }

    }
}

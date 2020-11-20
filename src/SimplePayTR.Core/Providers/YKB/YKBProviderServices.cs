using Microsoft.AspNetCore.Http;
using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Extensions;
using SimplePayTR.Core.Model;
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimplePayTR.Core.Providers.Ykb
{
    public class YKBProviderServices : BaseProviderService, IProviderService
    {
        public class YKB3DResult
        {
            public bool Status { get; set; }

            public string Data1 { get; set; }

            public string Data2 { get; set; }

            public string Sign { get; set; }

            public string Error { get; set; }

            public string ErrorCode { get; set; }
        }

        public YKBProviderServices(Func<BankTypes, IProviderConfiguration> ziraatConfiguration) : base()
        {
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => SimplePayGlobal.BankConfiguration[BankTypes.Ykb];
        public override string EmbededDirectory => "YKB.Resources";

        public override PostForm GetPostForm()
        {
            PostForm postForm = new PostForm();
            postForm.ContentType = "application/x-www-form-urlencoded";
            postForm.RequestFormat = RestSharp.DataFormat.Xml;
            postForm.PreTag = "xmldata=";
            postForm.SendParameterType = SendParameterType.RequestBody;
            return postForm;
        }

        #endregion Base

        #region Pay

        public async override Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            var cloneObj = paymentModel.Clone();

            string orderId = cloneObj.Order.OrderId.PadLeft(cloneObj.Use3DSecure ? 20 : 24, '0');

            if (cloneObj.Order.Installment.HasValue && (cloneObj.Order.Installment == 1 || cloneObj.Order.Installment == 0))
                cloneObj.Order.Installment = null;

            cloneObj.Order.Total *= 100;
            cloneObj.Order.OrderId = orderId;

            if (cloneObj.Order.CurrencyCode.IsEmpty())
                cloneObj.Order.CurrencyCode = "TL";

            if (cloneObj.Use3DSecure)
            {
                string template = StringHelper.ReadEmbedResource($"{EmbededDirectory}.3D_before.xml");
                string postData = StringHelper.PrepaireXML(new ViewModel
                {
                    CreditCard = cloneObj.CreditCard,
                    Order = cloneObj.Order,
                    Use3DSecure = true,
                    Configuration = ProviderConfiguration
                }, template);

                var post = GetPostForm();
                post.Content = postData;

                HTTPClient client = new HTTPClient(EndPointConfiguration.BaseUrl);
                var result = await client.Post(EndPointConfiguration.ApiEndPoint, post, Handler3D);

                if (result.Status)
                {
                    cloneObj.Attributes.Add(new SimplePayAttribute { Key = "Data1", Value = result.Data1 });
                    cloneObj.Attributes.Add(new SimplePayAttribute { Key = "Data2", Value = result.Data2 });
                    cloneObj.Attributes.Add(new SimplePayAttribute { Key = "Sign", Value = result.Sign });
                }
                else
                {
                    return new PaymentResult
                    {
                        Status = result.Status,
                        Error = result.Error,
                        ErrorCode = result.ErrorCode
                    };
                }
            }

            return await base.ProcessPayment(cloneObj);
        }

        public async override Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            foreach (var item in collection)
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = item.Key.ToLower(), Value = item.Value });

            var validateResult = await Validate3D(paymentModel, collection);
            if (validateResult.Status)
            {
                return await base.VerifyPayment(paymentModel, collection);
            }
            else
            {
                return new PaymentResult()
                {
                    Status = false,
                    Error = validateResult.Error,
                    ErrorCode = validateResult.ErrorCode
                };
            }
        }

        public override Task<PaymentResult> ProcessRefound(Refund refund)
        {
            var cloneObj = refund.Clone();

            cloneObj.RefundAmount = cloneObj.RefundAmount * 100;

            return base.ProcessRefound(cloneObj);
        }

        private async Task<PaymentResult> Validate3D(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            var config = (YKBConfiguration)ProviderConfiguration;

            string encKey = config.HashKey;

            string xid = collection["Xid"];
            var amount = (decimal.Parse(collection["Amount"], new CultureInfo("tr-TR")) * 100).ToString("0", new CultureInfo("en-US"));
            string firstHash = HashHelper.GetSHA256(encKey + ';' + config.TerminalId);
            string mac = HashHelper.GetSHA256(xid + ';' + amount + ';' + paymentModel.Order.CurrencyCode + ';' + config.MerchantId + ';' + firstHash);

            paymentModel.Attributes.Add(new SimplePayAttribute { Key = "mac", Value = mac });

            string template = StringHelper.ReadEmbedResource($"{EmbededDirectory}.3D_Resolve.xml");
            string postData = StringHelper.PrepaireXML(new ViewModel
            {
                Use3DSecure = true,
                Configuration = ProviderConfiguration,
                Attributes = paymentModel.Attributes
            }, template);

            var post = GetPostForm();
            post.Content = System.Net.WebUtility.UrlEncode(postData);

            HTTPClient client = new HTTPClient(EndPointConfiguration.BaseUrl);
            var result = await client.Post(EndPointConfiguration.ApiEndPoint, post, Handler);

            if (StringHelper.GetInlineContent(result.ServerResponseRaw, "mdStatus") != "1")
            {
                result.Status = false;
                result.ErrorCode = StringHelper.GetInlineContent("mdErrorMessage", result.ServerResponseRaw);
            }
                

            return result;
        }

        #endregion Pay

        #region Handler

        private YKB3DResult Handler3D(string serverResponse)
        {
            YKB3DResult result = new YKB3DResult();

            var hostResponse = StringHelper.GetInlineContent(serverResponse, "approved");
            if (hostResponse == "1")
            {
                result.Status = true;
                result.Data1 = StringHelper.GetInlineContent(serverResponse, "data1");
                result.Data2 = StringHelper.GetInlineContent(serverResponse, "data2");
                result.Sign = StringHelper.GetInlineContent(serverResponse, "sign");
            }
            else
            {
                result.Error = StringHelper.GetInlineContent(serverResponse, "respText");
                result.ErrorCode = StringHelper.GetInlineContent(serverResponse, "respCode");
            }

            return result;
        }

        public override PaymentResult Handler(string serverResponse)
        {
            PaymentResult result = new PaymentResult();

            result.ServerResponseRaw = serverResponse;

            var hostResponse = StringHelper.GetInlineContent(serverResponse, "approved");
            if (hostResponse == "1")
            {
                result.Status = true;
                result.ProvisionNumber = StringHelper.GetInlineContent(serverResponse, "authCode");
                result.ReferanceNumber = StringHelper.GetInlineContent(serverResponse, "hostlogkey");
            }
            else
            {
                result.Error = StringHelper.GetInlineContent(serverResponse, "respText");
                result.ErrorCode = StringHelper.GetInlineContent(serverResponse, "respCode");
            }

            return result;
        }

        #endregion Handler
    }
}
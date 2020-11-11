using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Model;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

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

        private YKBConfiguration _ykbConfiguration;

        public YKBProviderServices(Func<Banks, IProviderConfiguration> ziraatConfiguration) : base()
        {
            _ykbConfiguration = (YKBConfiguration)ziraatConfiguration(Banks.Ykb);
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => _ykbConfiguration;
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

        public override string GetUrl(bool use3DSecure)
        {
            if (use3DSecure == false)
            {
                if (_ykbConfiguration.UseTestEndPoint)
                    return $"{SimplePayGlobal.BankTestUrls[Banks.Ykb]}/PosnetWebService/XML";
                else
                    return $"{SimplePayGlobal.BankProdUrls[Banks.Ykb]}/3DSWebService/YKBPaymentService";
            }
            else
            {
                if (_ykbConfiguration.UseTestEndPoint)
                    return $"{SimplePayGlobal.BankTestUrls[Banks.Ykb]}/PosnetWebService/XML";
                else
                    return $"{SimplePayGlobal.BankProdUrls[Banks.Ykb]}/3DSWebService/YKBPaymentService";
            }
        }

        #endregion Base

        #region Pay

        public async override Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            var cloneObj = paymentModel.Clone();

            string orderId = paymentModel.Order.OrderId.PadLeft(paymentModel.Use3DSecure ? 20 : 24, '0');

            if (cloneObj.Order.Installment.HasValue && (cloneObj.Order.Installment == 1 || cloneObj.Order.Installment == 0))
                cloneObj.Order.Installment = null;

            cloneObj.Order.Total *= 100;
            cloneObj.Order.OrderId = orderId;

            if (paymentModel.Use3DSecure)
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

                HTTPClient client = new HTTPClient(GetUrl(true));
                var result = await client.Post(post, Handler3D);

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

        public async override Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, NameValueCollection collection)
        {
            if (Validate3D(paymentModel, collection))
            {
                for (int i = 0; i < collection.Count; i++)
                    paymentModel.Attributes.Add(new SimplePayAttribute { Key = collection.GetKey(i), Value = collection[i] });

                return await base.VerifyPayment(paymentModel, collection);
            }
            else
                throw new ApplicationException("İmza Doğrulanamadı");
        }

        public override Task<PaymentResult> ProcessRefound(Refund refund)
        {
            var cloneObj = refund.Clone();

            cloneObj.RefundAmount = cloneObj.RefundAmount * 100;

            return base.ProcessRefound(cloneObj);
        }

        private bool Validate3D(VerifyPaymentModel paymentModel, NameValueCollection collection)
        {
            string xid = collection.Get("Xid");
            var amount = Convert.ToDecimal(collection.Get("Amount")) / 100M;

            return paymentModel.Order.Total == amount && paymentModel.Order.OrderId == xid;
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
using Microsoft.AspNetCore.Http;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SanalPosTR.Providers.Est
{
    public class KuveytTurkProviderServices : BaseProviderService, IProviderService
    {
        public KuveytTurkProviderServices(Func<BankTypes, IProviderConfiguration> Configuration) : base()
        {
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => Definition.BankConfiguration[CurrentBank];

        public override string OnCompilingTemplate(PaymentModel paymentModel, string template)
        {
            if (paymentModel.Use3DSecure)
            {
                var nestConf = (KuveytTurkConfiguration)ProviderConfiguration;
                var rnd = Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
                var installment = (paymentModel.Order.Installment.HasValue && (paymentModel.Order.Installment == 1 || paymentModel.Order.Installment == 0)) ? "" : paymentModel.Order.Installment.ToString();
                var amount = paymentModel.Order.Total.ToString(new CultureInfo("en-US"));

                var hashStr = string.Concat(
                                nestConf.MerchantId,
                                paymentModel.Order.OrderId,
                                amount,
                                nestConf.SiteSuccessUrl.CompileOrderLink(paymentModel),
                                nestConf.SiteFailUrl.CompileOrderLink(paymentModel),
                                nestConf.UserName,
                                HashHelper.GetSHA1WithUTF8(nestConf.Password)
                            );

                paymentModel.Attributes.Add(new SanalPosTRAttribute()
                {
                    Key = "Hash",
                    Value = HashHelper.GetSHA1(hashStr)
                });
            }
            return base.OnCompilingTemplate(paymentModel, template);
        }

        public override string EmbededDirectory => "KuveytTurk.Resources";

        public override PostForm GetPostForm()
        {
            PostForm postForm = new PostForm();
            postForm.ContentType = "application/xml";
            postForm.RequestFormat = RestSharp.DataFormat.None;
            postForm.PreTag = String.Empty;
            postForm.SendParameterType = SendParameterType.RequestBody;
            return postForm;
        }

        #endregion Base

        #region Pay

        public override async Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {

            if (paymentModel.Use3DSecure == false)
            {
                return new PaymentResult() { Status = false, Error = "3d siz işlemi desteklemektedir" };
            }

            var cloneObj = paymentModel.Clone();

            if (cloneObj.Order.Installment.HasValue && (cloneObj.Order.Installment == 1 || cloneObj.Order.Installment == 0))
                cloneObj.Order.Installment = null;

            if (cloneObj.Order.CurrencyCode.IsEmpty())
                cloneObj.Order.CurrencyCode = "0949";

            HTTPClient client = new HTTPClient(EndPointConfiguration.BaseUrl);

            var xmlTemplate = await base.ProcessPayment(cloneObj);

            if (xmlTemplate.IsRedirectContent)
            {
                var post = GetPostForm();
                post.Content = xmlTemplate.ServerResponseRaw;

                xmlTemplate.ServerResponseRaw = (await client.Post(EndPointConfiguration.SecureEndPointApi, post, (result) =>
                {
                    return result.Replace("downloadForm", "SanalPosTR");
                }));
            }

            return xmlTemplate;
        }

        public override async Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            if (Validate3D(collection, paymentModel))
            {
                var vPosTransactionResponseContract = GetVPosTransactionResponseContract(collection["AuthenticationResponse"]);

                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "Installment", Value = vPosTransactionResponseContract.VPosMessage.InstallmentCount });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "OrderId", Value = vPosTransactionResponseContract.MerchantOrderId });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "CurrencyCode", Value = vPosTransactionResponseContract.VPosMessage.CurrencyCode });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "Total", Value = vPosTransactionResponseContract.VPosMessage.Amount });
                //  paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "HashData", Value = vPosTransactionResponseContract.HashData });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "MD", Value = vPosTransactionResponseContract.MD });

                var nestConf = (KuveytTurkConfiguration)ProviderConfiguration;

                var hashStr = string.Concat(
                               nestConf.MerchantId,
                               vPosTransactionResponseContract.MerchantOrderId,
                               vPosTransactionResponseContract.VPosMessage.Amount,
                               nestConf.UserName,
                               vPosTransactionResponseContract.VPosMessage.HashPassword
                           );
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "HashData", Value = HashHelper.GetSHA1(hashStr) });

                return await base.VerifyPayment(paymentModel, collection);
            }
            else
            {
                var vPosTransactionResponseContract = GetVPosTransactionResponseContract(collection["AuthenticationResponse"]);

                PaymentResult paymentResult = new PaymentResult();
                paymentResult.Error = vPosTransactionResponseContract.ResponseMessage;
                paymentResult.ErrorCode = vPosTransactionResponseContract.ResponseCode;
                paymentResult.Status = false;
                return paymentResult;
            }
        }

        public VPosTransactionResponseContract GetVPosTransactionResponseContract(string response)
        {
            var resp = System.Web.HttpUtility.UrlDecode(response);
            var x = new XmlSerializer(typeof(VPosTransactionResponseContract));
            var model = new VPosTransactionResponseContract();
            using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(resp)))
            {
                model = x.Deserialize(ms) as VPosTransactionResponseContract;
            }

            return model;
        }

        public bool Validate3D(IFormCollection formCollection, VerifyPaymentModel paymentModel)
        {
            if (formCollection.Keys.Contains("AuthenticationResponse"))
            {
                var vPosTransactionResponseContract = GetVPosTransactionResponseContract(formCollection["AuthenticationResponse"]);
                return vPosTransactionResponseContract.ResponseCode == "00" && vPosTransactionResponseContract.VPosMessage.Amount == paymentModel.Order.Total;
            }
            else
                return false;
        }

        public override Task<PaymentResult> ProcessRefound(Refund refund)
        {
            return Task.FromResult(new PaymentResult
            {
                Status = false,
                Error = "İade desteği henüz tamamlanmamıştır"
            });

            return base.ProcessRefound(refund);
        }

        #endregion Pay

        #region Handler

        public override PaymentResult Handler(string serverResponse)
        {
            PaymentResult result = new PaymentResult();

            var hostResponse = TemplateHelper.GetInlineContent(serverResponse, "ResponseCode");
            if (hostResponse == "00")
            {
                result.Status = true;
                result.ProvisionNumber = TemplateHelper.GetInlineContent(serverResponse, "ProvisionNumber");
                result.ReferanceNumber = TemplateHelper.GetInlineContent(serverResponse, "RRN");
            }
            else
            {
                result.Error = TemplateHelper.GetInlineContent(serverResponse, "ResponseMessage");
                result.ErrorCode = TemplateHelper.GetInlineContent(serverResponse, "ResponseCode");
            }

            return result;
        }

        #endregion Handler

        public class VPosTransactionResponseContract
        {
            public string ACSURL { get; set; }
            public string AuthenticationPacket { get; set; }
            public string HashData { get; set; }
            public bool IsEnrolled { get; set; }

            public bool IsVirtual { get; set; }
            public string MD { get; set; }
            public string MerchantOrderId { get; set; }
            public int OrderId { get; set; }
            public string PareqHtmlFormString { get; set; }
            public string Password { get; set; }
            public string ProvisionNumber { get; set; }
            public string ResponseCode { get; set; }
            public string ResponseMessage { get; set; }
            public string RRN { get; set; }
            public string SafeKey { get; set; }
            public string Stan { get; set; }
            public DateTime TransactionTime { get; set; }
            public string TransactionType { get; set; }
            public KuveytTurkVPosMessage VPosMessage { get; set; }
        }

        public class KuveytTurkVPosMessage
        {
            public decimal Amount { get; set; }
            public string OrderId { get; set; }
            public int InstallmentCount { get; set; }
            public string CurrencyCode { get; set; }
            public string HashPassword { get; set; }
        }
    }
}
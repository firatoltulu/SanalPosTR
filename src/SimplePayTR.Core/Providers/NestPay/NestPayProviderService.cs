using Microsoft.AspNetCore.Http;
using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Extensions;
using SimplePayTR.Core.Model;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SimplePayTR.Core.Providers.Est
{
    public class NestPayProviderService : BaseProviderService, IProviderService
    {
        public NestPayProviderService(Func<BankTypes, IProviderConfiguration> ziraatConfiguration) : base()
        {
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => SimplePayGlobal.BankConfiguration[CurrentBank];

        public override string OnCompilingTemplate(PaymentModel paymentModel, string template)
        {
            if (paymentModel.Use3DSecure)
            {
                var nestConf = (NestPayConfiguration)ProviderConfiguration;
                var rnd = Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
                var installment = (paymentModel.Order.Installment.HasValue && (paymentModel.Order.Installment == 1 || paymentModel.Order.Installment == 0)) ? "" : paymentModel.Order.Installment.ToString();
                var amount = paymentModel.Order.Total.ToString(new CultureInfo("en-US"));

                var hashStr = string.Concat(
                                nestConf.ClientId,
                                paymentModel.Order.OrderId,
                                amount,
                                nestConf.SiteSuccessUrl,
                                nestConf.SiteFailUrl,
                                nestConf.Type,
                                installment,
                                rnd,
                                nestConf.HashKey
                            );

                paymentModel.Attributes.Add(new SimplePayAttribute()
                {
                    Key = "Hash",
                    Value = HashHelper.GetSHA1(hashStr)
                });

                paymentModel.Attributes.Add(new SimplePayAttribute()
                {
                    Key = "Random",
                    Value = rnd.ToString()
                });
            }
            return base.OnCompilingTemplate(paymentModel, template);
        }

        public override string EmbededDirectory => "NestPay.Resources";

        public override PostForm GetPostForm()
        {
            PostForm postForm = new PostForm();
            postForm.ContentType = "application/x-www-form-urlencoded";
            postForm.RequestFormat = RestSharp.DataFormat.Xml;
            postForm.PreTag = "DATA=";
            postForm.SendParameterType = SendParameterType.RequestBody;
            return postForm;
        }

        #endregion Base

        #region Pay

        public override Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            var cloneObj = paymentModel.Clone();

            if (cloneObj.Order.Installment.HasValue && (cloneObj.Order.Installment == 1 || cloneObj.Order.Installment == 0))
                cloneObj.Order.Installment = null;

            if (cloneObj.Order.CurrencyCode.IsEmpty())
                cloneObj.Order.CurrencyCode = "949";

            return base.ProcessPayment(cloneObj);
        }

        public override async Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            if (Validate3D(collection))
            {
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "OrderId", Value = collection["oid"] });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "PayerTxnId", Value = collection["xid"] });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "PayerSecurityLevel", Value = collection["eci"] });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "PayerAuthenticationCode", Value = collection["cavv"] });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "CardNumber", Value = collection["md"] });

                return await base.VerifyPayment(paymentModel, collection);
            }
            else
            {
                PaymentResult paymentResult = new PaymentResult();
                paymentResult.Error = collection["ErrMsg"];
                paymentResult.ErrorCode = collection["ProcReturnCode"];
                paymentResult.Status = false;
                return paymentResult;
            }
        }

        public bool Validate3D(IFormCollection formCollection)
        {
            var nestPayConfiguration = ((NestPayConfiguration)ProviderConfiguration);

            string hashparams = formCollection["HASHPARAMS"];
            string hashparamsval = formCollection["HASHPARAMSVAL"];
            string paramsval = "";

            int index1 = 0;
            int index2 = 0;

            do
            {
                string val;
                index2 = hashparams.IndexOf(":", index1);
                string string1 = formCollection[hashparams.Substring(index1, index2 - index1)];

                if (string1 == null)
                    val = string1;
                else
                    val = formCollection[hashparams.Substring(index1, index2 - index1)];

                paramsval += val;
                index1 = index2 + 1;
            }
            while (index1 < hashparams.Length);

            string hashval = paramsval + nestPayConfiguration.HashKey;
            string hashparam = formCollection["HASH"];
            string hash = StringHelper.GetSHA1(hashval);
            if (!paramsval.Equals(hashparamsval) || !hash.Equals(hashparam))
            {
                return false;
            }

            var mdstatus = formCollection["mdStatus"];
            var result = (mdstatus.Equals("1") || mdstatus.Equals("2") || mdstatus.Equals("3") || mdstatus.Equals("4"));
            return result;
        }

        #endregion Pay

        #region Handler

        public override PaymentResult Handler(string serverResponse)
        {
            PaymentResult result = new PaymentResult();

            var hostResponse = StringHelper.GetInlineContent(serverResponse, "Response");
            if (hostResponse == "Approved")
            {
                result.Status = true;
                result.ProvisionNumber = StringHelper.GetInlineContent(serverResponse, "AuthCode");
                result.ReferanceNumber = StringHelper.GetInlineContent(serverResponse, "HostRefNum");
            }
            else
            {
                result.Error = StringHelper.GetInlineContent(serverResponse, "ErrMsg");
                result.ErrorCode = StringHelper.GetInlineContent(serverResponse, "ERRORCODE");
            }

            return result;
        }

        #endregion Handler
    }
}
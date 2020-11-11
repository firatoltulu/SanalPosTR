using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Model;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Threading.Tasks;

namespace SimplePayTR.Core.Providers.Est
{
    public class NestPayProviderService : BaseProviderService, IProviderService
    {
        private NestPayConfiguration _nestPayConfiguration;

        public NestPayProviderService(Func<Banks, IProviderConfiguration> ziraatConfiguration) : base()
        {
            _nestPayConfiguration = (NestPayConfiguration)ziraatConfiguration(CurrentBank);
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => _nestPayConfiguration;

        public override string OnCompilingTemplate(PaymentModel paymentModel, string template)
        {
            if (paymentModel.Use3DSecure)
            {
                var nestConf = (NestPayConfiguration)ProviderConfiguration;
                var rnd = new Random().Next(100000, 9999999);
                var hashStr = string.Concat(
                                nestConf.ClientId,
                                paymentModel.Order.OrderId,
                                paymentModel.Order.Total.ToString(new CultureInfo("en-US")),
                                nestConf.SiteSuccessUrl,
                                nestConf.SiteFailUrl,
                                nestConf.Type,
                                paymentModel.Order.Installment ?? null, rnd, nestConf.HashKey
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

                paymentModel.Attributes.Add(new SimplePayAttribute()
                {
                    Key = "Url",
                    Value = GetUrl(paymentModel.Use3DSecure)
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

        public override string GetUrl(bool use3DSecure)
        {
            if (use3DSecure == false)
            {
                if (_nestPayConfiguration.UseTestEndPoint)
                    return $"{SimplePayGlobal.BankTestUrls[CurrentBank]}/fim/api";
                else
                    return $"{SimplePayGlobal.BankTestUrls[CurrentBank]}/fim/api";
            }
            else
            {
                if (_nestPayConfiguration.UseTestEndPoint)
                    return $"{SimplePayGlobal.BankTestUrls[CurrentBank]}/fim";
                else
                    return $"{SimplePayGlobal.BankTestUrls[CurrentBank]}/fim";
            }
        }

        #endregion Base

        #region Pay

        public override Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            var cloneObj = paymentModel.Clone();

            if (cloneObj.Order.Installment.HasValue && (cloneObj.Order.Installment == 1 || cloneObj.Order.Installment == 0))
                cloneObj.Order.Installment = null;

            return base.ProcessPayment(cloneObj);
        }

        public override async Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, NameValueCollection collection)
        {
            var nestPayConfiguration = _nestPayConfiguration;

            if (Validate3D(collection, nestPayConfiguration))
            {
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "OrderId", Value = collection.Get("oid") });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "PayerTxnId", Value = collection.Get("xid") });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "PayerSecurityLevel", Value = collection.Get("eci") });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "PayerAuthenticationCode", Value = collection.Get("cavv") });
                paymentModel.Attributes.Add(new SimplePayAttribute { Key = "CardNumber", Value = collection.Get("md") });

                return await base.VerifyPayment(paymentModel, collection);
            }
            else
                throw new ApplicationException("İmza Doğrulanamadı");
        }

        public bool Validate3D(NameValueCollection formCollection, NestPayConfiguration nestPayConfiguration)
        {
            string hashparams = formCollection.Get("HASHPARAMS");
            string hashparamsval = formCollection.Get("HASHPARAMSVAL");
            string paramsval = "";

            int index1 = 0;
            int index2 = 0;

            do
            {
                index2 = hashparams.IndexOf(":", index1);
                string val = formCollection.Get(hashparams.Substring(index1, index2 - index1)) == null ? "" : formCollection.Get(hashparams.Substring(index1, index2 - index1));
                paramsval += val;
                index1 = index2 + 1;
            }
            while (index1 < hashparams.Length);

            string hashval = paramsval + nestPayConfiguration.HashKey;
            string hashparam = formCollection.Get("HASH");
            string hash = StringHelper.GetSHA1(hashval);
            if (!paramsval.Equals(hashparamsval) || !hash.Equals(hashparam))
            {
                return false;
            }

            var mdstatus = formCollection.Get("mdStatus");
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
using Microsoft.AspNetCore.Http;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanalPosTR.Providers.Est
{
    public class NestPayProviderService : BaseProviderService, IProviderService
    {
        public NestPayProviderService(Func<BankTypes, IProviderConfiguration> ziraatConfiguration, SanalPosHttpClient httpClient) : base(httpClient)
        {
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => Definition.BankConfiguration[CurrentBank];

        public override string OnCompilingTemplate(PaymentModel paymentModel, string template)
        {
            if (paymentModel.Use3DSecure)
            {
                var nestConf = (NestPayConfiguration)ProviderConfiguration;
                var rnd = Guid.NewGuid().ToString("N").Substring(0, 20).ToUpper();
                var installment = (paymentModel.Order.Installment.HasValue && (paymentModel.Order.Installment == 1 || paymentModel.Order.Installment == 0)) ? "" : paymentModel.Order.Installment.ToString();
                var amount = paymentModel.Order.Total.ToString(new CultureInfo("en-US"));

                var formParams = new Dictionary<string, string>
                {
                    { "pan", paymentModel.CreditCard.CardNumber },
                    { "cv2", paymentModel.CreditCard.CVV2 },
                    { "Ecom_Payment_Card_ExpDate_Year", paymentModel.CreditCard.ExpireYear },
                    { "Ecom_Payment_Card_ExpDate_Month", paymentModel.CreditCard.ExpireMonth },
                    { "currency", paymentModel.Order.CurrencyCode },
                    { "clientid", nestConf.ClientId },
                    { "amount", amount },
                    { "oid", paymentModel.Order.OrderId },
                    { "okUrl", nestConf.SiteSuccessUrl.CompileOrderLink(paymentModel) },
                    { "failUrl", nestConf.SiteFailUrl.CompileOrderLink(paymentModel) },
                    { "rnd", rnd },
                    { "islemtipi", nestConf.Type ?? "Auth" },
                    { "taksit", installment },
                    { "storetype", "3D" },
                    { "lang", "tr" },
                    { "hashAlgorithm", "ver3" }
                };

                var hashValue = ComputeVer3Hash(formParams, nestConf.HashKey);

                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "Hash", Value = hashValue });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "Random", Value = rnd });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "HashAlgorithm", Value = "ver3" });
            }
            return base.OnCompilingTemplate(paymentModel, template);
        }

        public static string ComputeVer3Hash(Dictionary<string, string> formParams, string storeKey)
        {
            var enUS = new CultureInfo("en-US");
            var sortedKeys = formParams.Keys
                .OrderBy(k => k.ToLower(enUS), StringComparer.Ordinal)
                .ToList();

            var hashBuilder = new StringBuilder();
            foreach (var key in sortedKeys)
            {
                var escapedValue = EscapeVer3Value(formParams[key]);
                hashBuilder.Append(escapedValue).Append('|');
            }
            hashBuilder.Append(EscapeVer3Value(storeKey));

            return HashHelper.GetSHA512Base64(hashBuilder.ToString());
        }

        private static string EscapeVer3Value(string value)
        {
            return (value ?? "").Replace("\\", "\\\\").Replace("|", "\\|");
        }

        public override string EmbededDirectory => "NestPay.Resources";

        public override PostForm GetPostForm()
        {
            PostForm postForm = new PostForm();
            postForm.ContentType = "application/x-www-form-urlencoded";
            postForm.RequestFormat = RequestDataFormat.Xml;
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
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "OrderId", Value = collection["oid"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "PayerTxnId", Value = collection["xid"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "PayerSecurityLevel", Value = collection["eci"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "PayerAuthenticationCode", Value = collection["cavv"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "CardNumber", Value = collection["md"] });

                if (paymentModel.Order.Installment.HasValue && (paymentModel.Order.Installment == 1 || paymentModel.Order.Installment == 0))
                    paymentModel.Order.Installment = null;

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

            var excludeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "hash", "encoding", "countdown" };

            var paramDict = new Dictionary<string, string>();
            foreach (var key in formCollection.Keys)
            {
                if (!excludeKeys.Contains(key))
                {
                    paramDict[key] = formCollection[key];
                }
            }

            var calculatedHash = ComputeVer3Hash(paramDict, nestPayConfiguration.HashKey);
            var receivedHash = formCollection["HASH"].ToString();

            if (!calculatedHash.Equals(receivedHash))
                return false;

            var mdstatus = formCollection["mdStatus"].ToString();
            return (mdstatus == "1" || mdstatus == "2" || mdstatus == "3" || mdstatus == "4");
        }

        #endregion Pay

        #region Handler

        public override PaymentResult Handler(string serverResponse)
        {
            PaymentResult result = new PaymentResult();

            var hostResponse = TemplateHelper.GetInlineContent(serverResponse, "Response");
            if (hostResponse == "Approved")
            {
                result.Status = true;
                result.ProvisionNumber = TemplateHelper.GetInlineContent(serverResponse, "AuthCode");
                result.ReferanceNumber = TemplateHelper.GetInlineContent(serverResponse, "HostRefNum");
            }
            else
            {
                result.Error = TemplateHelper.GetInlineContent(serverResponse, "ErrMsg");
                result.ErrorCode = TemplateHelper.GetInlineContent(serverResponse, "ERRORCODE");
            }

            return result;
        }

        #endregion Handler
    }
}
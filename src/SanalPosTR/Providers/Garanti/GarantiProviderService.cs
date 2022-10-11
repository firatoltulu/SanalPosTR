using Microsoft.AspNetCore.Http;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SanalPosTR.Providers.Est
{
    public class GarantiProviderService : BaseProviderService, IProviderService
    {
        public GarantiProviderService(Func<BankTypes, IProviderConfiguration> ziraatConfiguration) : base()
        {
        }

        #region Base

        public override IProviderConfiguration ProviderConfiguration => Definition.BankConfiguration[CurrentBank];

        public override string OnCompilingTemplate(PaymentModel paymentModel, string template)
        {
            if (paymentModel.Use3DSecure)
            {
                var garantiConfiguration = (GarantiConfiguration)ProviderConfiguration;
                var installment = (paymentModel.Order.Installment.HasValue && (paymentModel.Order.Installment == 1 || paymentModel.Order.Installment == 0)) ? "" : paymentModel.Order.Installment.ToString();
                var amount = paymentModel.Order.Total.ToString(new CultureInfo("en-US"));
                string securityData = HashHelper.GetSHA1(garantiConfiguration.ProvUserId + garantiConfiguration.TerminalId).ToUpper();

                var hashStr = string.Concat(
                                garantiConfiguration.TerminalId,
                                paymentModel.Order.OrderId,
                                amount,
                                garantiConfiguration.SiteSuccessUrl.CompileOrderLink(paymentModel),
                                garantiConfiguration.SiteFailUrl.CompileOrderLink(paymentModel),
                                garantiConfiguration.Type,
                                installment,
                                garantiConfiguration.SecureKey,
                                securityData
                            );

                paymentModel.Attributes.Add(new SanalPosTRAttribute()
                {
                    Key = "Hash",
                    Value = HashHelper.GetSHA1(hashStr)
                });
            }
            return base.OnCompilingTemplate(paymentModel, template);
        }

        public override string EmbededDirectory => "Garanti.Resources";

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
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "OrderId", Value = collection["oid"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "CustomerIpAddress", Value = collection["customeripaddress"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "CustomerEmailAddress", Value = collection["customeremailaddress"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "Installment", Value = collection["txninstallmentcount"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "Total", Value = collection["txnamount"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "CurrencyCode", Value = collection["txncurrencycode"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "AuthenticationCode", Value = collection["cavv"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "SecurityLevel", Value = collection["eci"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "TxnID", Value = collection["xid"] });
                paymentModel.Attributes.Add(new SanalPosTRAttribute { Key = "Md", Value = collection["md"] });

               
                return await base.VerifyPayment(paymentModel, collection);
            }
            else
            {
                string message = "3-D Secure doğrulanamadı";


                switch (collection["mdstatus"].ToString())
                {
                    case "0": message = "3-D doğrulama başarısız"; break;
                    case "1": message = "Doğrulama başarılı, işleme devam edebilirsiniz"; break;
                    case "2": message = "Kart sahibi veya bankası sisteme kayıtlı değil"; break;
                    case "3": message = "Kartın bankası sisteme kayıtlı değil"; break;
                    case "4": message = "Doğrulama denemesi, kart sahibi sisteme daha sonra kayıt olmayı seçmiş"; break;
                    case "5": message = "Doğrulama yapılamıyor"; break;
                    case "6": message = "3-D Secure hatası"; break;
                    case "7": message = "Sistem hatası"; break;
                    case "8": message = "Bilinmeyen kart no"; break;
                    case "9": message = "Üye İşyeri 3D-Secure sistemine kayıtlı değil (Bankada işyeri ve terminal numarası 3d olarak tanımlı değil)"; break;
                    default:
                        break;
                }

                if (collection["mderrormessage"].ToString().IsNullOrEmpty() == false)
                    message = collection["mderrormessage"];

                PaymentResult paymentResult = new PaymentResult();
                paymentResult.Error = message;
                paymentResult.ErrorCode = collection["mdstatus"];
                paymentResult.Status = false;
                return paymentResult;
            }
        }

        public bool Validate3D(IFormCollection formCollection)
        {
            var result = formCollection.ContainsKey("mdstatus") == false || formCollection["mdstatus"].ToString() == "1";
            if (result)
            {
                var garantiConfiguration = (GarantiConfiguration)ProviderConfiguration;

                bool isValidHash = false;
                String responseHashparams = formCollection["hashparams"];
                String responseHashparamsval = formCollection["hashparamsval"];
                String responseHash = formCollection["hash"];
                String storekey = garantiConfiguration.SecureKey;

                if (responseHashparams != null && !"".Equals(responseHashparams))
                {
                    String digestData = "";
                    char[] separator = new char[] { ':' };
                    String[] paramList = responseHashparams.Split(separator);

                    foreach (String param in paramList)
                    {
                        digestData += formCollection[param].ToString().IsNullOrEmpty() ? "" : formCollection[param];
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
                    }
                }

                result = isValidHash;
            }

            return result;
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
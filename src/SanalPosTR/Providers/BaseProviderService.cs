using Microsoft.AspNetCore.Http;
using Serilog;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using System;
using System.Threading.Tasks;

namespace SanalPosTR.Providers
{
    public abstract class BaseProviderService : IProviderService
    {
        public abstract string EmbededDirectory { get; }

        public virtual IProviderConfiguration ProviderConfiguration { get; }

        public virtual IEnvironmentConfiguration EndPointConfiguration =>
            ProviderConfiguration.UseTestEndPoint ? Definition.BankTestUrls[CurrentBank] : Definition.BankProdUrls[CurrentBank];

        public BankTypes CurrentBank { get; set; }

        public virtual string GetUrl(bool use3DSecure)
        {
            return string.Empty;
        }

        public virtual string OnCompilingTemplate(ViewModel viewModel, string template)
        {
            if (ProviderConfiguration.UseTestEndPoint)
                viewModel.Environment = Definition.BankTestUrls[CurrentBank];
            else
                viewModel.Environment = Definition.BankProdUrls[CurrentBank];

            return TemplateHelper.PrepaireXML(viewModel, template);
        }

        public virtual string OnCompilingTemplate(VerifyPaymentModel paymentModel, string template)
        {
            var viewModel = new ViewModel
            {
                Configuration = ProviderConfiguration,
                Order = paymentModel.Order,
                Attributes = paymentModel.Attributes
            };

            return OnCompilingTemplate(viewModel, template);
        }

        public virtual string OnCompilingTemplate(PaymentModel paymentModel, string template)
        {
            var viewModel = new ViewModel
            {
                CreditCard = paymentModel.CreditCard,
                Configuration = ProviderConfiguration,
                Order = paymentModel.Order,
                Attributes = paymentModel.Attributes,
                Use3DSecure = paymentModel.Use3DSecure,
                SessionId = paymentModel.SessionId
            };

            return OnCompilingTemplate(viewModel, template);
        }

        public virtual string OnCompilingTemplate(Refund refundModel, string template)
        {
            return OnCompilingTemplate(new ViewModel
            {
                Configuration = ProviderConfiguration,
                Refund = refundModel
            }, template);
        }

        public async virtual Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            Log.Information($"ProcessPayment-Start");

            //after may be fluent validation here
            string resource = string.Empty;

            if (paymentModel.Use3DSecure)
                resource = $"{EmbededDirectory}.3D.xml";
            else
                resource = $"{EmbededDirectory}.Pay.xml";

            string embededResource = TemplateHelper.ReadEmbedResource(resource);

            Log.Information($"ProcessPayment-ReadEmbedResource");

            string viewModel = OnCompilingTemplate(paymentModel, embededResource);

            Log.Information($"ProcessPayment-CompiledTemplate");

            if (viewModel.IsEmpty() == false)
            {
                var form = GetPostForm();
                form.Content = viewModel;

                if (paymentModel.Use3DSecure == false)
                {
                    Log.Information($"ProcessPayment-HttpPost - NonUse3DSecure");

                    HTTPClient httpClient = new HTTPClient(EndPointConfiguration.BaseUrl);
                    return await httpClient.Post(EndPointConfiguration.ApiEndPoint, form, Handler);
                }
                else
                {
                    Log.Information($"ProcessPayment-HttpPost - Use3DSecure");

                    PaymentResult paymentResult = new PaymentResult();
                    paymentResult.IsRedirectContent = true;
                    paymentResult.ServerResponseRaw = viewModel;
                    paymentResult.Status = true;

                    return paymentResult;
                }


            }
            else
                throw new ApplicationException($"{this.GetType().Name} template is empty");



        }

        public async virtual Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            Log.Information($"{paymentModel.Order.OrderId} - VerifyPayment Process");

            var postForm = GetPostForm();
            var resource = $"{EmbededDirectory}.3DEnd.xml";
            string embededResource = TemplateHelper.ReadEmbedResource(resource);
            string viewModel = OnCompilingTemplate(paymentModel, embededResource);
            postForm.Content = viewModel;

            Log.Information($"{paymentModel.Order.OrderId} - VerifyPayment Http Post");


            HTTPClient httpClient = new HTTPClient(EndPointConfiguration.BaseUrl);

            return await httpClient.Post(EndPointConfiguration.SecureReturnEndPoint, postForm, Handler);
        }

        public virtual async Task<PaymentResult> ProcessRefound(Refund refund)
        {
            var resource = $"{EmbededDirectory}.Refund.xml";

            string embededResource = TemplateHelper.ReadEmbedResource(resource);
            string viewModel = OnCompilingTemplate(refund, embededResource);

            var form = GetPostForm();
            form.Content = viewModel;

            HTTPClient httpClient = new HTTPClient(EndPointConfiguration.BaseUrl);
            return await httpClient.Post(EndPointConfiguration.RefundEndPoint, form, Handler);
        }

        public abstract PostForm GetPostForm();

        public abstract PaymentResult Handler(string serverResponse);
    }
}
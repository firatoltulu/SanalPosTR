using Microsoft.AspNetCore.Http;
using SanalPosTR.Configuration;
using SanalPosTR.Extensions;
using SanalPosTR.Model;
using Serilog;
using System;
using System.Threading.Tasks;

namespace SanalPosTR.Providers
{
    public abstract class BaseProviderService : IProviderService
    {
        protected readonly SanalPosHttpClient HttpClient;

        protected BaseProviderService(SanalPosHttpClient httpClient)
        {
            HttpClient = httpClient;
        }

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

        public virtual async Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            Log.Information("ProcessPayment started. Provider: {Provider}, Use3DSecure: {Use3DSecure}, OrderId: {OrderId}", GetType().Name, paymentModel.Use3DSecure, paymentModel.Order.OrderId);

            string resource = paymentModel.Use3DSecure
                ? $"{EmbededDirectory}.3D.xml"
                : $"{EmbededDirectory}.Pay.xml";

            string embededResource = TemplateHelper.ReadEmbedResource(resource);

            Log.Debug("ProcessPayment - EmbeddedResource loaded: {Resource}", resource);

            string viewModel = OnCompilingTemplate(paymentModel, embededResource);

            Log.Debug("ProcessPayment - Template compiled");

            if (viewModel.IsEmpty() == false)
            {
                var form = GetPostForm();
                form.Content = viewModel;

                if (paymentModel.Use3DSecure == false)
                {
                    Log.Debug("ProcessPayment - Posting to bank endpoint: {BaseUrl}{Endpoint}", EndPointConfiguration.BaseUrl, EndPointConfiguration.ApiEndPoint);

                    var result = await HttpClient.Post(EndPointConfiguration.BaseUrl, EndPointConfiguration.ApiEndPoint, form, Handler);

                    Log.Information("ProcessPayment completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}", GetType().Name, paymentModel.Order.OrderId, result.Status);

                    return result;
                }
                else
                {
                    Log.Debug("ProcessPayment - 3DSecure redirect content prepared");

                    PaymentResult paymentResult = new PaymentResult();
                    paymentResult.IsRedirectContent = true;
                    paymentResult.ServerResponseRaw = viewModel;
                    paymentResult.Status = true;

                    Log.Information("ProcessPayment completed (3DSecure redirect). Provider: {Provider}, OrderId: {OrderId}", GetType().Name, paymentModel.Order.OrderId);

                    return paymentResult;
                }
            }
            else
                throw new ApplicationException($"{this.GetType().Name} template is empty");
        }

        public virtual async Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection)
        {
            Log.Information("VerifyPayment started. Provider: {Provider}, OrderId: {OrderId}", GetType().Name, paymentModel.Order.OrderId);

            var postForm = GetPostForm();
            var resource = $"{EmbededDirectory}.3DEnd.xml";
            string embededResource = TemplateHelper.ReadEmbedResource(resource);
            string viewModel = OnCompilingTemplate(paymentModel, embededResource);
            postForm.Content = viewModel;

            Log.Debug("VerifyPayment - Template compiled. Posting to: {BaseUrl}{Endpoint}", EndPointConfiguration.BaseUrl, EndPointConfiguration.SecureReturnEndPoint);

            var result = await HttpClient.Post(EndPointConfiguration.BaseUrl, EndPointConfiguration.SecureReturnEndPoint, postForm, Handler);

            Log.Information("VerifyPayment completed. Provider: {Provider}, OrderId: {OrderId}, Status: {Status}", GetType().Name, paymentModel.Order.OrderId, result.Status);

            return result;
        }

        public virtual async Task<PaymentResult> ProcessRefound(Refund refund)
        {
            Log.Information("ProcessRefund started. Provider: {Provider}", GetType().Name);

            var resource = $"{EmbededDirectory}.Refund.xml";

            string embededResource = TemplateHelper.ReadEmbedResource(resource);
            string viewModel = OnCompilingTemplate(refund, embededResource);

            var form = GetPostForm();
            form.Content = viewModel;

            Log.Debug("ProcessRefund - Template compiled. Posting to: {BaseUrl}{Endpoint}", EndPointConfiguration.BaseUrl, EndPointConfiguration.RefundEndPoint);

            var result = await HttpClient.Post(EndPointConfiguration.BaseUrl, EndPointConfiguration.RefundEndPoint, form, Handler);

            Log.Information("ProcessRefund completed. Provider: {Provider}, Status: {Status}", GetType().Name, result.Status);

            return result;
        }

        public abstract PostForm GetPostForm();

        public abstract PaymentResult Handler(string serverResponse);
    }
}

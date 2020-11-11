using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Extensions;
using SimplePayTR.Core.Model;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SimplePayTR.Core.Providers
{
    public abstract class BaseProviderService : IProviderService
    {
        public abstract string EmbededDirectory { get; }

        public virtual IProviderConfiguration ProviderConfiguration { get; }

        public virtual string GetUrl(bool use3DSecure)
        {
            return string.Empty;
        }

        public virtual string OnCompilingTemplate(PaymentModel paymentModel, string template)
        {
            string viewModel = StringHelper.PrepaireXML(new ViewModel
            {
                CreditCard = paymentModel.CreditCard,
                Configuration = ProviderConfiguration,
                Order = paymentModel.Order,
                Attributes = paymentModel.Attributes
            }, template);

            return viewModel;
        }

        public async virtual Task<PaymentResult> ProcessPayment(PaymentModel paymentModel)
        {
            //after may be fluent validation here
            string resource = string.Empty;

            if (paymentModel.Use3DSecure)
                resource = $"{EmbededDirectory}.3D.html";
            else
                resource = $"{EmbededDirectory}.Pay.xml";

            string embededResource = StringHelper.ReadEmbedResource(resource);
            string viewModel = OnCompilingTemplate(paymentModel, embededResource);

            if (viewModel.IsEmpty() == false)
            {
                var form = GetPostForm();
                form.Content = viewModel;

                if (paymentModel.Use3DSecure == false)
                {
                    HTTPClient httpClient = new HTTPClient(GetUrl(paymentModel.Use3DSecure));
                    return await httpClient.Post(form, Handler);
                }
                else
                {
                    PaymentResult paymentResult = new PaymentResult();
                    paymentResult.IsRedirectContent = true;
                    paymentResult.ServerResponseRaw = viewModel;

                    return paymentResult;
                }
            }
            else
                throw new ApplicationException($"{this.GetType().Name} template is empty");
        }

        public virtual Task<PaymentResult> ProcessPayment3DSecure(PaymentModel paymentModel, NameValueCollection collection)
        {
            return default;
        }

        public virtual Task<PaymentResult> ProcessRefound(Refund refund)
        {
            return default;
        }

        public abstract PostForm GetPostForm();

        public abstract PaymentResult Handler(string serverResponse);
    }
}
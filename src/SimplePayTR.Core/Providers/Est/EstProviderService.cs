using SimplePayTR.Core.Model;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SimplePayTR.Core.Providers.Est
{
    public class EstProviderService : IProviderService
    {
        public EstProviderService(EstConfiguration estConfiguration)
        {

        }

        public virtual Task<PaymentResult> ProcessPayment(Order order)
        {
            throw new NotImplementedException();
        }

        public virtual Task<PaymentResult> ProcessPayment3DSecure(Order order, NameValueCollection collection)
        {
            throw new NotImplementedException();
        }

        public virtual Task<PaymentResult> ProcessRefound(Refund refund)
        {
            throw new NotImplementedException();
        }
    }
}
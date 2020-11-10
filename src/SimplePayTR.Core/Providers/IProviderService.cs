using SimplePayTR.Core.Model;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SimplePayTR.Core.Providers
{
    public interface IProviderService
    {
        /// <summary>
        /// Ödeme Al
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<PaymentResult> ProcessPayment(Order order);

        /// <summary>
        /// 3D ile Onaylanmış Ödemeyi Al
        /// </summary>
        /// <param name="order"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        Task<PaymentResult> ProcessPayment3DSecure(Order order, NameValueCollection collection);

        /// <summary>
        /// İade Yap
        /// </summary>
        /// <param name="refund"></param>
        /// <returns></returns>
        Task<PaymentResult> ProcessRefound(Refund refund);
    }
}
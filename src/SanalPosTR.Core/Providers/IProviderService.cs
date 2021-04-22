using Microsoft.AspNetCore.Http;
using SanalPosTR.Model;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace SanalPosTR.Providers
{
    public interface IProviderService
    {
        /// <summary>
        /// Ödeme Al
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<PaymentResult> ProcessPayment(PaymentModel paymentModel);

        /// <summary>
        /// 3D ile Onaylanmış Ödemeyi Al
        /// </summary>
        /// <param name="order"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        Task<PaymentResult> VerifyPayment(VerifyPaymentModel paymentModel, IFormCollection collection);

        /// <summary>
        /// İade Yap
        /// </summary>
        /// <param name="refund"></param>
        /// <returns></returns>
        Task<PaymentResult> ProcessRefound(Refund refund);

        BankTypes CurrentBank { get; set; }

    }
}
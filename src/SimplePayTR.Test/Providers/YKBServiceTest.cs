using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimplePayTR.Core;
using SimplePayTR.Core.Model;
using SimplePayTR.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimplePayTR.Test.Providers
{
    [TestFixture]
    public class YKBServiceTest : Initialize
    {
        private static readonly object[] _paymentModelData = new object[]{
           new List<PaymentModel>{
               new PaymentModel()
                    {
                        CreditCard = new CreditCardInfo()
                        {
                            CardNumber = "5400617020092306",
                            CVV2 = "000",
                            ExpireMonth = "10",
                            ExpireYear = "22",
                            CardHolderName = "FIRAT OLTULU",
                        },
                        Order = new OrderInfo
                        {
                            Installment = 5,
                            OrderId = "3",
                            Total = 900.95M,
                            UserId = "ECommerce3D",
                            CurrencyCode="TL"
                        }
                    }
           }
        }.ToArray();

        [TestCaseSource("_paymentModelData")]
        public async Task YKBServiceTest_ProcessPayment(IEnumerable<PaymentModel> paymentModels)
        {
            var providerService = ServiceProvider.GetRequiredService<Func<Banks, IProviderService>>();
            var _ykbService = providerService(Banks.Ykb);

            var serverResponse = await _ykbService.ProcessPayment(paymentModels.FirstOrDefault());

            Assert.IsTrue(serverResponse.Status, serverResponse.ErrorCode);
        }

        [TestCaseSource("_paymentModelData")]
        public async Task YKBServiceTest_ProcessPaymentWith3D(IEnumerable<PaymentModel> paymentModels)
        {
            var providerService = ServiceProvider.GetRequiredService<Func<Banks, IProviderService>>();
            var _ykbService = providerService(Banks.Ykb);
            var _paymentModel = paymentModels.FirstOrDefault();

            _paymentModel.Use3DSecure = true;

            var serverResponse = await _ykbService.ProcessPayment(_paymentModel);

            Assert.IsFalse(serverResponse.Status);
        }

        [Test]
        public async Task YKBServiceTest_Refund()
        {
            var providerService = ServiceProvider.GetRequiredService<Func<Banks, IProviderService>>();
            var _ykbService = providerService(Banks.Ykb);
            var serverResponse = await _ykbService.ProcessRefound(new Refund
            {
                OrderId = "1".PadLeft(24, '0'),
                RefundAmount = 5,
                CurrencyCode = "TL"
            });

            Assert.IsFalse(serverResponse.Status);
        }
    }
}
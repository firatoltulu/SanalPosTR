using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SimplePayTR.Core.Model;
using SimplePayTR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimplePayTR.Core.Providers;

namespace SimplePayTR.Test.Providers
{
    [TestFixture]
    public class ZiraatBankServiceTest : Initialize
    {
        private static readonly object[] _paymentModelData = new object[]{
           new List<PaymentModel>{
               new PaymentModel()
                    {
                        CreditCard = new CreditCardInfo()
                        {
                            CardNumber = "4546711234567894",
                            CVV2 = "000",
                            ExpireMonth = "12",
                            ExpireYear = "26",
                            CardHolderName = "FIRAT OLTULU",
                        },
                        Order = new OrderInfo
                        {
                            Installment = 0,
                            OrderId = "1",
                            Total = 9.95M,
                            UserId = "ECommerce3D",
                            CurrencyCode="949"
                        }
                    }
           }
        }.ToArray();



        [TestCaseSource("_paymentModelData")]
        public async Task EstProviderServiceTest_ProcessPayment(IEnumerable<PaymentModel> paymentModels)
        {
            var providerService = ServiceProvider.GetRequiredService<Func<Banks, IProviderService>>();
            var _estService = providerService(Banks.Ziraat);

            var serverResponse = await _estService.ProcessPayment(paymentModels.FirstOrDefault());

            Assert.IsFalse(serverResponse.Status);
        }

        [TestCaseSource("_paymentModelData")]
        public async Task EstProviderServiceTest_ProcessPaymentWith3D(IEnumerable<PaymentModel> paymentModels)
        {
            var providerService = ServiceProvider.GetRequiredService<Func<Banks, IProviderService>>();
            var _estService = providerService(Banks.Ziraat);
            var _paymentModel = paymentModels.FirstOrDefault();

            _paymentModel.Use3DSecure = true;

            var serverResponse = await _estService.ProcessPayment(_paymentModel);

            Assert.IsFalse(serverResponse.Status);
        }

        [Test]
        public async Task EstProviderServiceTest_Refund()
        {
            var providerService = ServiceProvider.GetRequiredService<Func<Banks, IProviderService>>();
            var _estService = providerService(Banks.Ziraat);
            var serverResponse = await _estService.ProcessRefound(new Refund
            {
                OrderId = "123123",
                RefundAmount = 5,
                CurrencyCode = "949"
            });

            Assert.IsFalse(serverResponse.Status);
        }
    }
}
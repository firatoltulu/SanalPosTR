using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SanalPosTR;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using System;
using System.Threading.Tasks;

namespace SanalPosTR.Test.Providers
{
    [TestFixture]
    public class KuveytTurkServiceTest : Initialize
    {
        private IProviderService GetKuveytTurkProvider()
        {
            var providerService = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderService>>();
            return providerService(BankTypes.KuveytTurk);
        }

        private PaymentModel CreatePaymentModel()
        {
            return new PaymentModel
            {
                CreditCard = new CreditCardInfo
                {
                    CardNumber = "4242424242424242",
                    CVV2 = "123",
                    ExpireMonth = "12",
                    ExpireYear = "2030",
                    CardHolderName = "TEST USER"
                },
                Order = new OrderInfo
                {
                    OrderId = new Random().Next(10000, 999999).ToString(),
                    Total = 1.00M,
                    Installment = 0,
                    CurrencyCode = "0949",
                    UserId = "TestUser"
                },
                Use3DSecure = true
            };
        }

        [Test]
        public void KuveytTurkProvider_CanBeResolved()
        {
            var provider = GetKuveytTurkProvider();

            Assert.That(provider, Is.Not.Null);
            Assert.That(provider.CurrentBank, Is.EqualTo(BankTypes.KuveytTurk));
        }

        [Test]
        public async Task KuveytTurkProvider_ProcessPayment_Non3D_ReturnsError()
        {
            var provider = GetKuveytTurkProvider();
            var model = CreatePaymentModel();
            model.Use3DSecure = false;

            var result = await provider.ProcessPayment(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Does.Contain("3d"));
        }

        [Test]
        public async Task KuveytTurkProvider_ProcessRefund_ReturnsNotSupported()
        {
            var provider = GetKuveytTurkProvider();
            var refund = new Refund
            {
                OrderId = "123456",
                RefundAmount = 1.00M,
                CurrencyCode = "0949"
            };

            var result = await provider.ProcessRefound(refund);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.False);
        }

        [Test]
        public void KuveytTurkProvider_Handler_ParsesSuccessResponse()
        {
            var provider = GetKuveytTurkProvider();
            var xml = @"<VPosTransactionResponseContract>
                <ResponseCode>00</ResponseCode>
                <ProvisionNumber>P12345</ProvisionNumber>
                <RRN>RRN001</RRN>
            </VPosTransactionResponseContract>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("P12345"));
            Assert.That(result.ReferanceNumber, Is.EqualTo("RRN001"));
        }

        [Test]
        public void KuveytTurkProvider_Handler_ParsesErrorResponse()
        {
            var provider = GetKuveytTurkProvider();
            var xml = @"<VPosTransactionResponseContract>
                <ResponseCode>99</ResponseCode>
                <ResponseMessage>Declined</ResponseMessage>
            </VPosTransactionResponseContract>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Is.EqualTo("Declined"));
            Assert.That(result.ErrorCode, Is.EqualTo("99"));
        }
    }
}

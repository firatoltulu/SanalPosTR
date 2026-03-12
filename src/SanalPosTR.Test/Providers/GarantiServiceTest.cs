using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SanalPosTR;
using SanalPosTR.Configuration;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using System;
using System.Threading.Tasks;

namespace SanalPosTR.Test.Providers
{
    [TestFixture]
    public class GarantiServiceTest : Initialize
    {
        private IProviderService GetGarantiProvider()
        {
            var providerService = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderService>>();
            return providerService(BankTypes.Garanti);
        }

        private PaymentModel CreatePaymentModel(bool use3D = false)
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
                    CurrencyCode = "949",
                    UserId = "TestUser"
                },
                Use3DSecure = use3D
            };
        }

        [Test]
        public void GarantiProvider_CanBeResolved()
        {
            var provider = GetGarantiProvider();

            Assert.That(provider, Is.Not.Null);
            Assert.That(provider.CurrentBank, Is.EqualTo(BankTypes.Garanti));
        }

        [Test]
        [Category("Integration")]
        public async Task GarantiProvider_ProcessPayment_ReturnsResult()
        {
            var provider = GetGarantiProvider();
            var model = CreatePaymentModel();

            try
            {
                var result = await provider.ProcessPayment(model);
                Assert.That(result, Is.Not.Null);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Assert.Ignore("Garanti test endpoint is not reachable");
            }
        }

        [Test]
        public async Task GarantiProvider_ProcessPayment3D_ReturnsRedirectContent()
        {
            var provider = GetGarantiProvider();
            var model = CreatePaymentModel(use3D: true);

            var result = await provider.ProcessPayment(model);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsRedirectContent, Is.True);
            Assert.That(result.ServerResponseRaw, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        [Category("Integration")]
        public async Task GarantiProvider_ProcessRefund_ReturnsResult()
        {
            var provider = GetGarantiProvider();
            var refund = new Refund
            {
                OrderId = "123456",
                RefundAmount = 1.00M,
                CurrencyCode = "949"
            };

            try
            {
                var result = await provider.ProcessRefound(refund);
                Assert.That(result, Is.Not.Null);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Assert.Ignore("Garanti test endpoint is not reachable");
            }
        }

        [Test]
        public void GarantiProvider_Handler_ParsesSuccessResponse()
        {
            var provider = GetGarantiProvider();
            var xml = @"<GVPSResponse>
                <Transaction>
                    <Response><Code>00</Code></Response>
                    <AuthCode>A12345</AuthCode>
                    <RetrefNum>REF001</RetrefNum>
                </Transaction>
            </GVPSResponse>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("A12345"));
            Assert.That(result.ReferanceNumber, Is.EqualTo("REF001"));
        }

        [Test]
        public void GarantiProvider_Handler_ParsesErrorResponse()
        {
            var provider = GetGarantiProvider();
            var xml = @"<GVPSResponse>
                <Transaction>
                    <Response><Code>99</Code></Response>
                    <ErrorMsg>Declined</ErrorMsg>
                    <ReasonCode>0051</ReasonCode>
                </Transaction>
            </GVPSResponse>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Is.EqualTo("Declined"));
            Assert.That(result.ErrorCode, Is.EqualTo("0051"));
        }
    }
}

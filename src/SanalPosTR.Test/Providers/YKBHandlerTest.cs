using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SanalPosTR;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using System;

namespace SanalPosTR.Test.Providers
{
    [TestFixture]
    public class YKBHandlerTest : Initialize
    {
        private IProviderService GetYKBProvider()
        {
            var providerService = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderService>>();
            return providerService(BankTypes.Ykb);
        }

        [Test]
        public void Handler_ApprovedResponse_ParsesSuccessfully()
        {
            var provider = GetYKBProvider();
            var xml = @"<posnetResponse>
                <approved>1</approved>
                <authCode>Y12345</authCode>
                <hostlogkey>HOST001</hostlogkey>
            </posnetResponse>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("Y12345"));
            Assert.That(result.ReferanceNumber, Is.EqualTo("HOST001"));
        }

        [Test]
        public void Handler_RejectedResponse_ParsesError()
        {
            var provider = GetYKBProvider();
            var xml = @"<posnetResponse>
                <approved>0</approved>
                <respText>Card declined</respText>
                <respCode>0051</respCode>
            </posnetResponse>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Is.EqualTo("Card declined"));
            Assert.That(result.ErrorCode, Is.EqualTo("0051"));
        }

        [Test]
        public void Handler_EmptyApproved_ReturnsFailedResult()
        {
            var provider = GetYKBProvider();
            var xml = @"<posnetResponse>
                <approved>0</approved>
                <respText></respText>
                <respCode></respCode>
            </posnetResponse>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.False);
        }
    }
}

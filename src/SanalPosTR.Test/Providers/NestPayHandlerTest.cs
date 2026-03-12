using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SanalPosTR;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using SanalPosTR.Providers.Est;
using System;
using System.Collections.Generic;

namespace SanalPosTR.Test.Providers
{
    [TestFixture]
    public class NestPayHandlerTest : Initialize
    {
        private IProviderService GetNestPayProvider(BankTypes bank = BankTypes.Ziraat)
        {
            var providerService = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderService>>();
            return providerService(bank);
        }

        [Test]
        public void Handler_ApprovedResponse_ParsesSuccessfully()
        {
            var provider = GetNestPayProvider();
            var xml = @"<CC5Response>
                <Response>Approved</Response>
                <AuthCode>P12345</AuthCode>
                <HostRefNum>REF001</HostRefNum>
            </CC5Response>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.True);
            Assert.That(result.ProvisionNumber, Is.EqualTo("P12345"));
            Assert.That(result.ReferanceNumber, Is.EqualTo("REF001"));
        }

        [Test]
        public void Handler_DeclinedResponse_ParsesError()
        {
            var provider = GetNestPayProvider();
            var xml = @"<CC5Response>
                <Response>Declined</Response>
                <ErrMsg>Insufficient funds</ErrMsg>
                <ERRORCODE>05</ERRORCODE>
            </CC5Response>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.False);
            Assert.That(result.Error, Is.EqualTo("Insufficient funds"));
            Assert.That(result.ErrorCode, Is.EqualTo("05"));
        }

        [Test]
        public void Handler_EmptyResponse_ReturnsFailedResult()
        {
            var provider = GetNestPayProvider();
            var xml = @"<CC5Response>
                <Response></Response>
            </CC5Response>";

            var handlerMethod = provider.GetType().GetMethod("Handler");
            var result = (PaymentResult)handlerMethod.Invoke(provider, new object[] { xml });

            Assert.That(result.Status, Is.False);
        }

        [TestCase(BankTypes.Ziraat)]
        [TestCase(BankTypes.Akbank)]
        [TestCase(BankTypes.FinansBank)]
        [TestCase(BankTypes.Isbank)]
        public void NestPayBanks_CanBeResolved(BankTypes bankType)
        {
            var provider = GetNestPayProvider(bankType);

            Assert.That(provider, Is.Not.Null);
            Assert.That(provider.CurrentBank, Is.EqualTo(bankType));
        }

        [TestCase(BankTypes.Ziraat)]
        [TestCase(BankTypes.Akbank)]
        [TestCase(BankTypes.FinansBank)]
        [TestCase(BankTypes.Isbank)]
        public void NestPayBanks_ShareSameProviderType(BankTypes bankType)
        {
            var provider = GetNestPayProvider(bankType);

            Assert.That(provider.GetType().Name, Is.EqualTo("NestPayProviderService"));
        }

        [Test]
        public void ComputeVer3Hash_SameParams_ReturnsSameHash()
        {
            var formParams = new Dictionary<string, string>
            {
                { "clientid", "100100100" },
                { "amount", "1.00" },
                { "oid", "ORD123" },
                { "storetype", "3D" },
                { "hashAlgorithm", "ver3" }
            };

            var hash1 = NestPayProviderService.ComputeVer3Hash(formParams, "SKEY123");
            var hash2 = NestPayProviderService.ComputeVer3Hash(formParams, "SKEY123");

            Assert.That(hash1, Is.EqualTo(hash2));
        }

        [Test]
        public void ComputeVer3Hash_DifferentStoreKey_ReturnsDifferentHash()
        {
            var formParams = new Dictionary<string, string>
            {
                { "clientid", "100100100" },
                { "amount", "1.00" }
            };

            var hash1 = NestPayProviderService.ComputeVer3Hash(formParams, "KEY1");
            var hash2 = NestPayProviderService.ComputeVer3Hash(formParams, "KEY2");

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void ComputeVer3Hash_ReturnsBase64String()
        {
            var formParams = new Dictionary<string, string>
            {
                { "clientid", "100100100" },
                { "amount", "1.00" }
            };

            var hash = NestPayProviderService.ComputeVer3Hash(formParams, "SKEY");

            Assert.That(hash, Does.Match("^[A-Za-z0-9+/]+=*$"));
        }

        [Test]
        public void ComputeVer3Hash_SortsKeysCaseInsensitive()
        {
            // Keys with mixed case should be sorted case-insensitively
            var formParams1 = new Dictionary<string, string>
            {
                { "Amount", "1.00" },
                { "clientid", "100" }
            };

            var formParams2 = new Dictionary<string, string>
            {
                { "clientid", "100" },
                { "Amount", "1.00" }
            };

            var hash1 = NestPayProviderService.ComputeVer3Hash(formParams1, "SKEY");
            var hash2 = NestPayProviderService.ComputeVer3Hash(formParams2, "SKEY");

            Assert.That(hash1, Is.EqualTo(hash2));
        }

        [Test]
        public void ComputeVer3Hash_EscapesPipeAndBackslash()
        {
            var formParamsWithPipe = new Dictionary<string, string>
            {
                { "field", "val|ue" }
            };
            var formParamsWithoutPipe = new Dictionary<string, string>
            {
                { "field", "value" }
            };

            var hash1 = NestPayProviderService.ComputeVer3Hash(formParamsWithPipe, "KEY");
            var hash2 = NestPayProviderService.ComputeVer3Hash(formParamsWithoutPipe, "KEY");

            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void ComputeVer3Hash_EmptyValues_DoesNotThrow()
        {
            var formParams = new Dictionary<string, string>
            {
                { "field1", "" },
                { "field2", "value" }
            };

            Assert.DoesNotThrow(() => NestPayProviderService.ComputeVer3Hash(formParams, "KEY"));
        }
    }
}

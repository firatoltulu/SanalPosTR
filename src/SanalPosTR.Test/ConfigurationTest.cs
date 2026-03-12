using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SanalPosTR;
using SanalPosTR.Configuration;
using SanalPosTR.Model;
using SanalPosTR.Providers;
using System;

namespace SanalPosTR.Test
{
    [TestFixture]
    public class ConfigurationTest : Initialize
    {
        #region DI Resolution

        [Test]
        public void AddSanalPosTR_RegistersProviderFactory()
        {
            var factory = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderService>>();

            Assert.That(factory, Is.Not.Null);
        }

        [Test]
        public void AddSanalPosTR_RegistersConfigurationFactory()
        {
            var factory = ServiceProvider.GetRequiredService<Func<BankTypes, IProviderConfiguration>>();

            Assert.That(factory, Is.Not.Null);
        }

        [Test]
        public void AddSanalPosTR_RegistersConfigurationService()
        {
            var service = ServiceProvider.GetRequiredService<IConfigurationService>();

            Assert.That(service, Is.Not.Null);
        }

        [Test]
        public void AddSanalPosTR_RegistersSanalPosHttpClient()
        {
            var client = ServiceProvider.GetRequiredService<SanalPosHttpClient>();

            Assert.That(client, Is.Not.Null);
        }

        #endregion

        #region Definition

        [Test]
        public void Definition_BankProviders_ContainsAllConfiguredBanks()
        {
            Assert.That(Definition.BankProviders.ContainsKey(BankTypes.Ziraat), Is.True);
            Assert.That(Definition.BankProviders.ContainsKey(BankTypes.Akbank), Is.True);
            Assert.That(Definition.BankProviders.ContainsKey(BankTypes.FinansBank), Is.True);
            Assert.That(Definition.BankProviders.ContainsKey(BankTypes.Isbank), Is.True);
            Assert.That(Definition.BankProviders.ContainsKey(BankTypes.Ykb), Is.True);
            Assert.That(Definition.BankProviders.ContainsKey(BankTypes.Garanti), Is.True);
            Assert.That(Definition.BankProviders.ContainsKey(BankTypes.KuveytTurk), Is.True);
        }

        [Test]
        public void Definition_BankConfiguration_ContainsAllBanks()
        {
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.Ziraat), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.Akbank), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.FinansBank), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.Isbank), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.Ykb), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.Garanti), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.KuveytTurk), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.HalkBank), Is.True);
            Assert.That(Definition.BankConfiguration.ContainsKey(BankTypes.Anadolubank), Is.True);
        }

        [Test]
        public void Definition_TestUrls_AllHaveBaseUrl()
        {
            foreach (var kvp in Definition.BankTestUrls)
            {
                Assert.That(kvp.Value.BaseUrl, Is.Not.Null.And.Not.Empty,
                    $"BankType {kvp.Key} test URL is empty");
            }
        }

        [Test]
        public void Definition_ProdUrls_AllHaveBaseUrl()
        {
            foreach (var kvp in Definition.BankProdUrls)
            {
                Assert.That(kvp.Value.BaseUrl, Is.Not.Null.And.Not.Empty,
                    $"BankType {kvp.Key} prod URL is empty");
            }
        }

        [Test]
        public void Definition_NestPayBanks_UseNestPayProvider()
        {
            var nestPayBanks = new[] { BankTypes.Ziraat, BankTypes.Akbank, BankTypes.FinansBank, BankTypes.Isbank };

            foreach (var bank in nestPayBanks)
            {
                Assert.That(Definition.BankProviders[bank].Name, Is.EqualTo("NestPayProviderService"),
                    $"{bank} should use NestPayProviderService");
            }
        }

        #endregion

        #region Configuration Service

        [Test]
        public void ConfigurationService_UseZiraat_SetsConfiguration()
        {
            var config = Definition.BankConfiguration[BankTypes.Ziraat] as NestPayConfiguration;

            Assert.That(config, Is.Not.Null);
            Assert.That(config.ClientId, Is.EqualTo("x"));
        }

        [Test]
        public void ConfigurationService_UseYKB_SetsConfiguration()
        {
            var config = Definition.BankConfiguration[BankTypes.Ykb] as YKBConfiguration;

            Assert.That(config, Is.Not.Null);
            Assert.That(config.MerchantId, Is.EqualTo("x"));
            Assert.That(config.TerminalId, Is.EqualTo("x"));
        }

        [Test]
        public void ConfigurationService_UseKuveytTurk_SetsConfiguration()
        {
            var config = Definition.BankConfiguration[BankTypes.KuveytTurk] as KuveytTurkConfiguration;

            Assert.That(config, Is.Not.Null);
            Assert.That(config.MerchantId, Is.EqualTo("x"));
        }

        [Test]
        public void ConfigurationService_SetBankEnvironment_SetsTestMode()
        {
            var configService = ServiceProvider.GetRequiredService<IConfigurationService>();
            configService.SetBankEnvironment(BankTypes.Ziraat, true);

            var config = Definition.BankConfiguration[BankTypes.Ziraat] as I3DConfiguration;

            Assert.That(config, Is.Not.Null);
            Assert.That(config.UseTestEndPoint, Is.True);
        }

        #endregion

        #region PaymentModel Clone

        [Test]
        public void PaymentModel_Clone_CreatesDeepCopy()
        {
            var original = new PaymentModel
            {
                CreditCard = new CreditCardInfo
                {
                    CardNumber = "4242424242424242",
                    CVV2 = "123",
                    ExpireMonth = "12",
                    ExpireYear = "2030"
                },
                Order = new OrderInfo
                {
                    OrderId = "1",
                    Total = 100M,
                    Installment = 3
                },
                Use3DSecure = true
            };

            var cloned = original.Clone();

            Assert.That(cloned, Is.Not.SameAs(original));
            Assert.That(cloned.CreditCard.CardNumber, Is.EqualTo(original.CreditCard.CardNumber));
            Assert.That(cloned.Order.Total, Is.EqualTo(original.Order.Total));
            Assert.That(cloned.Use3DSecure, Is.EqualTo(original.Use3DSecure));
        }

        [Test]
        public void PaymentModel_Clone_ModifyCloneDoesNotAffectOriginal()
        {
            var original = new PaymentModel
            {
                CreditCard = new CreditCardInfo { CardNumber = "4242424242424242" },
                Order = new OrderInfo { Total = 100M, OrderId = "1" }
            };

            var cloned = original.Clone();
            cloned.Order.Total = 999M;
            cloned.CreditCard.CardNumber = "0000000000000000";

            Assert.That(original.Order.Total, Is.EqualTo(100M));
            Assert.That(original.CreditCard.CardNumber, Is.EqualTo("4242424242424242"));
        }

        [Test]
        public void Refund_Clone_CreatesDeepCopy()
        {
            var original = new Refund
            {
                OrderId = "123",
                RefundAmount = 50M,
                CurrencyCode = "949"
            };

            var cloned = original.Clone();

            Assert.That(cloned, Is.Not.SameAs(original));
            Assert.That(cloned.OrderId, Is.EqualTo(original.OrderId));
            Assert.That(cloned.RefundAmount, Is.EqualTo(original.RefundAmount));
        }

        #endregion
    }
}

using Microsoft.Extensions.DependencyInjection;
using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Helper;
using System;

namespace SimplePayTR.Core
{
    internal class SimplePayConfiguration : ISimplePayConfiguration
    {
        private IServiceProvider _serviceProvider;

        public SimplePayConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #region NestPay 

        public ISimplePayConfiguration UseZiraat(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(Banks.Ziraat, configuration);
            return this;
        }

        public ISimplePayConfiguration UseAkbank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(Banks.Akbank, configuration);
            return this;
        }

        public ISimplePayConfiguration UseIsBank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(Banks.Isbank, configuration);
            return this;
        }

        public ISimplePayConfiguration UseFinansBank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(Banks.FinansBank, configuration);
            return this;
        }

        public ISimplePayConfiguration UseTEB(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(Banks.TEB, configuration);
            return this;
        }

        public ISimplePayConfiguration UseAnadolubank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(Banks.Anadolubank, configuration);
            return this;
        }

        #endregion

        public ISimplePayConfiguration UseYKB(YKBConfiguration configuration)
        {
            var yKBConfiguration = (YKBConfiguration)_serviceProvider.GetRequiredService<Func<Banks, IProviderConfiguration>>()(Banks.Ykb);
            new MapperOptimized().Copy(configuration, yKBConfiguration);
            return this;
        }

        private void mappingConfigurationNest(Banks nestBanks, NestPayConfiguration configuration)
        {
            var nestPayConfiguration = (NestPayConfiguration)_serviceProvider.GetRequiredService<Func<Banks, IProviderConfiguration>>()(Banks.Ziraat);
            new MapperOptimized().Copy(configuration, nestPayConfiguration);
        }
    }
}
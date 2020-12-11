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
            mappingConfigurationNest(BankTypes.Ziraat, configuration);
            return this;
        }

        public ISimplePayConfiguration UseAkbank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.Akbank, configuration);
            return this;
        }

        public ISimplePayConfiguration UseIsBank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.Isbank, configuration);
            return this;
        }

        public ISimplePayConfiguration UseFinansBank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.FinansBank, configuration);
            return this;
        }

        public ISimplePayConfiguration UseTEB(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.TEB, configuration);
            return this;
        }

        public ISimplePayConfiguration UseAnadolubank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.Anadolubank, configuration);
            return this;
        }

        #endregion NestPay

        public ISimplePayConfiguration UseYKB(YKBConfiguration configuration)
        {
            SimplePayGlobal.BankConfiguration[BankTypes.Ykb] = configuration;
            return this;
        }

        public ISimplePayConfiguration UseFromJSON(BankTypes bankTypes, string jsonValue)
        {
            var configuration = SimplePayGlobal.BankConfiguration[bankTypes];
            var deserializeObj = System.Text.Json.JsonSerializer.Deserialize(jsonValue, configuration.GetType());
            SimplePayGlobal.BankConfiguration[bankTypes] = (IProviderConfiguration)deserializeObj;
            return this;
        }

        private void mappingConfigurationNest(BankTypes nestBanks, NestPayConfiguration configuration)
        {
            SimplePayGlobal.BankConfiguration[nestBanks] = configuration;
        }

        public ISimplePayConfiguration SetSuccessReturnUrl(string url)
        {
            foreach (var item in SimplePayGlobal.BankConfiguration)
            {
                if(item.Value is I3DConfiguration)
                    (item.Value as I3DConfiguration).SiteSuccessUrl = url;
            }

            return this;
        }

        public ISimplePayConfiguration SetFailReturnUrl(string url)
        {
            foreach (var item in SimplePayGlobal.BankConfiguration)
            {
                if (item.Value is I3DConfiguration)
                    (item.Value as I3DConfiguration).SiteFailUrl = url;
            }

            return this;
        }
    }
}
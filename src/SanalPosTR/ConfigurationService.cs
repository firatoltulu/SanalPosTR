using SanalPosTR.Configuration;
using System;

namespace SanalPosTR
{
    internal class ConfigurationService : IConfigurationService
    {
        private IServiceProvider _serviceProvider;

        public ConfigurationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #region NestPay

        public IConfigurationService UseZiraat(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.Ziraat, configuration);
            return this;
        }

        public IConfigurationService UseAkbank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.Akbank, configuration);
            return this;
        }

        public IConfigurationService UseIsBank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.Isbank, configuration);
            return this;
        }

        public IConfigurationService UseFinansBank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.FinansBank, configuration);
            return this;
        }

        public IConfigurationService UseTEB(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.TEB, configuration);
            return this;
        }

        public IConfigurationService UseAnadolubank(NestPayConfiguration configuration)
        {
            mappingConfigurationNest(BankTypes.Anadolubank, configuration);
            return this;
        }

        #endregion NestPay

        public IConfigurationService UseYKB(YKBConfiguration configuration)
        {
            Definition.BankConfiguration[BankTypes.Ykb] = configuration;
            return this;
        }

        public IConfigurationService UseKuveytTurk(KuveytTurkConfiguration configuration)
        {
            Definition.BankConfiguration[BankTypes.KuveytTurk] = configuration;
            return this;
        }

        public IConfigurationService UseFromJSON(BankTypes bankTypes, string jsonValue)
        {
            var configuration = Definition.BankConfiguration[bankTypes];
            var deserializeObj = System.Text.Json.JsonSerializer.Deserialize(jsonValue, configuration.GetType());
            Definition.BankConfiguration[bankTypes] = (IProviderConfiguration)deserializeObj;
            return this;
        }

        private void mappingConfigurationNest(BankTypes nestBanks, NestPayConfiguration configuration)
        {
            Definition.BankConfiguration[nestBanks] = configuration;
        }

        public IConfigurationService SetBankEnvironment(BankTypes bankTypes, bool useTest)
        {
            foreach (var item in Definition.BankConfiguration)
            {
                if (item.Key == bankTypes)
                {
                    if (item.Value is I3DConfiguration)
                        (item.Value as I3DConfiguration).UseTestEndPoint = useTest;
                }
            }
            return this;
        }

        public IConfigurationService SetSuccessReturnUrl(string url)
        {
            foreach (var item in Definition.BankConfiguration)
            {
                if (item.Value is I3DConfiguration)
                    (item.Value as I3DConfiguration).SiteSuccessUrl = url;
            }

            return this;
        }

        public IConfigurationService SetFailReturnUrl(string url)
        {
            foreach (var item in Definition.BankConfiguration)
            {
                if (item.Value is I3DConfiguration)
                    (item.Value as I3DConfiguration).SiteFailUrl = url;
            }

            return this;
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Providers;
using SimplePayTR.Core.Providers.Est;
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

        public ISimplePayConfiguration UseZiraat(ZiraatBankConfiguration estConfiguration)
        {
            var estConf = (NestPayConfiguration)_serviceProvider.GetRequiredService<Func<Banks, IProviderConfiguration>>()(Banks.Ziraat);
            mappingConfigurationEst(estConfiguration, estConf);
            return this;
        }

        private static void mappingConfigurationEst(
            NestPayConfiguration estConfiguration,
            NestPayConfiguration estConf)
        {
            estConf.ClientId = estConfiguration.ClientId;
            estConf.Name = estConfiguration.Name;
            estConf.Password = estConfiguration.Password;
            estConf.SiteFailUrl = estConfiguration.SiteFailUrl;
            estConf.SiteSuccessUrl = estConfiguration.SiteSuccessUrl;
            estConf.UseTestEndPoint = estConfiguration.UseTestEndPoint;
        }
    }
}
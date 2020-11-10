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

        public ISimplePayConfiguration UseEst(EstConfiguration estConfiguration)
        {
            var estConf = (EstConfiguration)_serviceProvider.GetRequiredService<Func<ProviderTypes, IProviderConfiguration>>()(ProviderTypes.Est);

            estConf.ClientId = estConfiguration.ClientId;
            estConf.Endpoint = estConfiguration.Endpoint;
            estConf.Name = estConfiguration.Name;
            estConf.Password = estConfiguration.Password;
            estConf.SiteFailUrl = estConfiguration.SiteFailUrl;
            estConf.SiteSuccessUrl = estConfiguration.SiteSuccessUrl;

            return this;
        }
    }
}
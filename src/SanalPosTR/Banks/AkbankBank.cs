using Microsoft.Extensions.DependencyInjection;
using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Providers.Est;
using System;

namespace SimplePayTR.Core.Providers
{
    public class AkbankBankService : NestPayProviderService, IProviderService
    {
        private ZiraatBankConfiguration _ziraatConfiguration;

        public AkbankBankService(Func<Banks, IProviderConfiguration> ziraatConfiguration) : base()
        {
            _ziraatConfiguration = (ZiraatBankConfiguration)ziraatConfiguration(Banks.Ziraat);
        }

        public override IProviderConfiguration ProviderConfiguration => _ziraatConfiguration;

    }
}
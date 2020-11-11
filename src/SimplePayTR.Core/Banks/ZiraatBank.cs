using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Providers.Est;
using System;

namespace SimplePayTR.Core.Providers
{
    public class ZiraatBankService : NestPayProviderService, IProviderService
    {
        private ZiraatBankConfiguration _ziraatConfiguration;

        public ZiraatBankService(Func<Banks, IProviderConfiguration> ziraatConfiguration) : base()
        {
            _ziraatConfiguration = (ZiraatBankConfiguration)ziraatConfiguration(Banks.Ziraat);
        }

        public override IProviderConfiguration ProviderConfiguration => _ziraatConfiguration;

        public override string GetUrl(bool use3DSecure)
        {
            if (use3DSecure == false)
            {
                if (_ziraatConfiguration.UseTestEndPoint)
                    return $"{SimplePayGlobal.BankTestUrls[Banks.Ziraat]}/fim/api";
                else
                    return $"{SimplePayGlobal.BankTestUrls[Banks.Ziraat]}/fim/api";
            }
            else
            {
                if (_ziraatConfiguration.UseTestEndPoint)
                    return $"{SimplePayGlobal.BankTestUrls[Banks.Ziraat]}/fim";
                else
                    return $"{SimplePayGlobal.BankTestUrls[Banks.Ziraat]}/fim";
            }
        }
    }
}
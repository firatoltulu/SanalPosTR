using SimplePayTR.Core.Providers.Est;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimplePayTR.Core.Configuration
{
    public class ZiraatBankConfiguration : NestPayConfiguration, IProviderConfiguration, I3DConfiguration
    {
        public override string Endpoint => UseTestEndPoint ? "{0}/fim/api" : "{0}/fim/api";
    }
}

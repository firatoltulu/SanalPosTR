using SanalPosTR.Configuration;
using SanalPosTR.Providers;
using SanalPosTR.Providers.Est;
using SanalPosTR.Providers.Ykb;
using System;
using System.Collections.Generic;

namespace SanalPosTR
{
    public static class Definition
    {
        public static Dictionary<BankTypes, Type> BankProviders = new Dictionary<BankTypes, Type>
        {
            { BankTypes.Ziraat, typeof(NestPayProviderService)},
            { BankTypes.Akbank, typeof(NestPayProviderService) },
            { BankTypes.FinansBank, typeof(NestPayProviderService) },
            { BankTypes.Isbank, typeof(NestPayProviderService) },
            { BankTypes.Ykb, typeof(YKBProviderServices) },
        };

        public static Dictionary<BankTypes, IProviderConfiguration> BankConfiguration = new Dictionary<BankTypes, IProviderConfiguration>
        {
            { BankTypes.Ziraat, new NestPayConfiguration()},
            { BankTypes.Akbank,  new NestPayConfiguration() },
            { BankTypes.FinansBank,  new NestPayConfiguration() },
            { BankTypes.Isbank,  new NestPayConfiguration() },
            { BankTypes.Ykb,  new YKBConfiguration() },
        };

        public static Dictionary<BankTypes, IEnvironmentConfiguration> BankTestUrls = new Dictionary<BankTypes, IEnvironmentConfiguration>
        {
            {BankTypes.Ziraat, new NestPayEndPointConfiguration{ BaseUrl="https://entegrasyon.asseco-see.com.tr" } },
            {BankTypes.Ykb,new YKBEndPointConfiguration{ BaseUrl="https://setmpos.ykb.com" } }
        };

        public static Dictionary<BankTypes, IEnvironmentConfiguration> BankProdUrls = new Dictionary<BankTypes, IEnvironmentConfiguration>
        {
            {BankTypes.Ziraat, new NestPayEndPointConfiguration{ BaseUrl="https://sanalpos2.ziraatbank.com.tr" } },
            {BankTypes.Ykb,new YKBEndPointConfiguration{ BaseUrl="https://posnet.yapikredi.com.tr" } }
        };


    }
}
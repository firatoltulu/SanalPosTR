using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Providers;
using SimplePayTR.Core.Providers.Est;
using SimplePayTR.Core.Providers.Ykb;
using System;
using System.Collections.Generic;

namespace SimplePayTR.Core
{
    internal static class SimplePayGlobal
    {
        public static Dictionary<Banks, Type> BankProviders = new Dictionary<Banks, Type>
        {
            { Banks.Ziraat, typeof(NestPayProviderService)},
            { Banks.Akbank, typeof(NestPayProviderService) },
            { Banks.FinansBank, typeof(NestPayProviderService) },
            { Banks.Isbank, typeof(NestPayProviderService) },
            { Banks.Ykb, typeof(YKBProviderServices) },
        };

        public static Dictionary<Banks, Type> BankConfiguration = new Dictionary<Banks, Type>
        {
            { Banks.Ziraat, typeof(NestPayConfiguration)},
            { Banks.Akbank, typeof(NestPayConfiguration) },
            { Banks.FinansBank, typeof(NestPayConfiguration) },
            { Banks.Isbank, typeof(NestPayConfiguration) },
            { Banks.Ykb, typeof(YKBConfiguration) },

        };

        public static Dictionary<Banks, string> BankTestUrls = new Dictionary<Banks, string>
        {
            { Banks.Ziraat, "https://sanalpos2.ziraatbank.com.tr"},
            {Banks.Ykb,"https://setmpos.ykb.com" }
        };

        public static Dictionary<Banks, string> BankProdUrls = new Dictionary<Banks, string>
        {
            { Banks.Ziraat, "https://sanalpos.ziraatbank.com.tr"},
            { Banks.Ykb, "https://postnet.ykb.com.tr"},
        };


    }
}
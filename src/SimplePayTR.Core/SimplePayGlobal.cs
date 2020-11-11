using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Providers;
using SimplePayTR.Core.Providers.Est;
using System;
using System.Collections.Generic;

namespace SimplePayTR.Core
{
    internal static class SimplePayGlobal
    {
        public static Dictionary<Banks, Type> BankProviders = new Dictionary<Banks, Type>
        {
            { Banks.Ziraat, typeof(ZiraatBankService)},
            { Banks.Akbank, typeof(AkbankBankService) },
            { Banks.FinansBank, typeof(NestPayProviderService) },
            { Banks.Isbank, typeof(NestPayProviderService) },
        };

        public static Dictionary<Banks, Type> BankConfiguration = new Dictionary<Banks, Type>
        {
            { Banks.Ziraat, typeof(ZiraatBankConfiguration)},
            { Banks.Akbank, typeof(AkbankConfiguration) },
            { Banks.FinansBank, typeof(NestPayConfiguration) },
            { Banks.Isbank, typeof(NestPayConfiguration) },
        };

        public static Dictionary<Banks, string> BankTestUrls = new Dictionary<Banks, string>
        {
            { Banks.Ziraat, "https://sanalpos2.ziraatbank.com.tr/"},
        };

        public static Dictionary<Banks, string> BankProdUrls = new Dictionary<Banks, string>
        {
            { Banks.Ziraat, "https://sanalpos.ziraatbank.com.tr/"},
        };


    }
}
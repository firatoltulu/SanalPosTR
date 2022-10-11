using SanalPosTR.Configuration;
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
            { BankTypes.KuveytTurk, typeof(KuveytTurkProviderServices) },
            { BankTypes.Garanti, typeof(GarantiProviderService) }

        };

        public static Dictionary<BankTypes, IProviderConfiguration> BankConfiguration = new Dictionary<BankTypes, IProviderConfiguration>
        {
            { BankTypes.Ziraat, new NestPayConfiguration()},
            { BankTypes.Akbank,  new NestPayConfiguration() },
            { BankTypes.FinansBank,  new NestPayConfiguration() },
            { BankTypes.Isbank,  new NestPayConfiguration() },
            { BankTypes.HalkBank,  new NestPayConfiguration() },
            { BankTypes.Anadolubank,  new NestPayConfiguration() },
            { BankTypes.Ykb,  new YKBConfiguration() },
            { BankTypes.KuveytTurk, new KuveytTurkConfiguration() },
            { BankTypes.Garanti, new GarantiConfiguration() },

        };

        public static Dictionary<BankTypes, IEnvironmentConfiguration> BankTestUrls = new Dictionary<BankTypes, IEnvironmentConfiguration>
        {
            {BankTypes.Ziraat, new NestPayEndPointConfiguration{ BaseUrl="https://entegrasyon.asseco-see.com.tr" } },
            {BankTypes.Akbank, new NestPayEndPointConfiguration{ BaseUrl="https://entegrasyon.asseco-see.com.tr" } },
            {BankTypes.FinansBank, new NestPayEndPointConfiguration{ BaseUrl="https://entegrasyon.asseco-see.com.tr" } },
            {BankTypes.Isbank, new NestPayEndPointConfiguration{ BaseUrl="https://entegrasyon.asseco-see.com.tr" } },
            {BankTypes.HalkBank, new NestPayEndPointConfiguration{ BaseUrl="https://entegrasyon.asseco-see.com.tr" } },
            {BankTypes.Anadolubank, new NestPayEndPointConfiguration{ BaseUrl="https://entegrasyon.asseco-see.com.tr" } },
            {BankTypes.Garanti,new GarantiEndPointConfiguration{ BaseUrl="https://sanalposprovtest.garanti.com.tr" } },
            {BankTypes.Ykb,new YKBEndPointConfiguration{ BaseUrl="https://setmpos.ykb.com" } },
            {BankTypes.KuveytTurk, new KuveytTurkEndPointConfiguration
                {
                    BaseUrl= "https://boatest.kuveytturk.com.tr",
                    SecureEndPointApi="boa.virtualpos.services/Home/ThreeDModelPayGate",
                    SecureReturnEndPoint="boa.virtualpos.services/Home/ThreeDModelProvisionGate"
                }
            }
        };

        public static Dictionary<BankTypes, IEnvironmentConfiguration> BankProdUrls = new Dictionary<BankTypes, IEnvironmentConfiguration>
        {
            {BankTypes.Ziraat, new NestPayEndPointConfiguration{ BaseUrl="https://sanalpos2.ziraatbank.com.tr" } },
            {BankTypes.Akbank, new NestPayEndPointConfiguration{ BaseUrl="https://www.sanalakpos.com" } },
            {BankTypes.FinansBank, new NestPayEndPointConfiguration{ BaseUrl="https://www.fbwebpos.com" } },
            {BankTypes.Isbank, new NestPayEndPointConfiguration{ BaseUrl="https://sanalpos.isbank.com.tr" } },
            {BankTypes.HalkBank, new NestPayEndPointConfiguration{ BaseUrl="https://sanalpos.halkbank.com.tr" } },
            {BankTypes.Anadolubank, new NestPayEndPointConfiguration{ BaseUrl=" https://anadolusanalpos.est.com.tr" } },
            {BankTypes.Ykb, new YKBEndPointConfiguration{ BaseUrl="https://posnet.yapikredi.com.tr" } },
            {BankTypes.KuveytTurk, new KuveytTurkEndPointConfiguration{
                BaseUrl="https://sanalpos.kuveytturk.com.tr",
                SecureEndPointApi="ServiceGateWay/Home/ThreeDModelPayGate",
                SecureReturnEndPoint="ServiceGateWay/Home/ThreeDModelProvisionGate"
            } }
        };
    }
}
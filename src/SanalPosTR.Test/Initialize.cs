using Microsoft.AspNetCore.Builder;

using Microsoft.Extensions.DependencyInjection;
using SanalPosTR;
using SanalPosTR.Configuration;
using System;

namespace SanalPosTR.Test
{
    public class Initialize
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IApplicationBuilder ApplicationBuilder { get; private set; }

        public Initialize()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSanalPosTR();

            ServiceProvider = serviceCollection.BuildServiceProvider();
            ApplicationBuilder = new ApplicationBuilder(ServiceProvider);

            ApplicationBuilder.UseSanalPosTR(ops =>
            {
                ops.UseZiraat(new NestPayConfiguration()
                {
                    Name = "x",
                    ClientId = "x",
                    Password = "x",
                    UseTestEndPoint = true,
                    SiteFailUrl = "http://x.com/Fail/{{OrderId}}",
                    SiteSuccessUrl = "http://x.com/Success/{{OrderId}}"
                });

                ops.UseYKB(new YKBConfiguration()
                {
                    MerchantId = "x",
                    TerminalId = "x",
                    PosnetId = "x",
                    UserName = "5558",
                    Password = "123456",
                    UseTestEndPoint = true,
                    SiteFailUrl = "http://x.com/Fail/{{OrderId}}",
                    SiteSuccessUrl = "http://x.com/Success/{{OrderId}}"
                });

                ops.UseKuveytTurk(new KuveytTurkConfiguration()
                {
                    MerchantId = "x",
                    //TerminalId = "x",
                    //PosnetId = "x",
                    UserName = "5558",
                    Password = "123456",
                    UseTestEndPoint = true,
                    SiteFailUrl = "http://x.com/Fail/{{OrderId}}",
                    SiteSuccessUrl = "http://x.com/Success/{{OrderId}}"
                });

                // Akbank Test Store'da (sanalposteststore-prep.akbank.com) herkese açık yayınlanan ortak test işyeri
                ops.UseAkbankPos(new AkbankPosConfiguration()
                {
                    MerchantSafeId = "2023090417500272654BD9A49CF07574",
                    TerminalSafeId = "2023090417500284633D137A249DBBEB",
                    SecretKey = "3230323330393034313735303032363031353172675f357637355f3273387373745f7233725f73323333383737335f323272383774767276327672323531355f",
                    UseTestEndPoint = true,
                    SiteFailUrl = "http://x.com/Payment/ValidatePayment/{{SessionId}}",
                    SiteSuccessUrl = "http://x.com/Payment/ValidatePayment/{{SessionId}}"
                });

                // VakıfBank Standart API Entegrasyon Rehberi v1.4 §2.2'de herkese açık yayınlanan test işyeri
                ops.UseVakifBank(new VakifBankConfiguration()
                {
                    MerchantId = "000100000013506",
                    TerminalNo = "VP000579",
                    Password = "123456",
                    UseTestEndPoint = true,
                    SiteFailUrl = "https://x.com/Payment/ValidatePayment/{{SessionId}}",
                    SiteSuccessUrl = "https://x.com/Payment/ValidatePayment/{{SessionId}}"
                });
            });

            Definition.BankConfiguration[BankTypes.Garanti] = new GarantiConfiguration()
            {
                TerminalId = "x",
                UserId = "x",
                MerchantId = "x",
                SecureKey = "x",
                Password = "x",
                UseTestEndPoint = true,
                SiteFailUrl = "http://x.com/Fail/{{OrderId}}",
                SiteSuccessUrl = "http://x.com/Success/{{OrderId}}"
            };
        }
    }
}
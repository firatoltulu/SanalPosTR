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
            });
        }
    }
}
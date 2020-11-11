using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.Extensions.DependencyInjection;
using SimplePayTR.Core;
using SimplePayTR.Core.Configuration;
using System;

namespace SimplePayTR.Test
{
    public class Initialize
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public IApplicationBuilder ApplicationBuilder { get; private set; }

        public Initialize()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSimplePayTR();

            ServiceProvider = serviceCollection.BuildServiceProvider();
            ApplicationBuilder = new ApplicationBuilder(ServiceProvider);

            ApplicationBuilder.UseSimplePayTR(ops =>
            {
                ops.UseZiraat(new ZiraatBankConfiguration()
                {
                    Name = "testuser",
                    ClientId = "testclient",
                    Password = "g8YUjec4ZiEamMC",
                    UseTestEndPoint = true,
                    SiteFailUrl = "http://x.com/Fail",
                    SiteSuccessUrl = "http://x.com/Success"
                });
            });
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Providers;
using System;
using System.Text;

namespace SimplePayTR.Core
{
    public static class SimplePayServiceCollectionExtension
    {
        public static IServiceCollection AddSimplePayTR(this IServiceCollection services)
        {
            services.AddScoped<ZiraatBankService>();
            services.AddScoped<AkbankBankService>();

            services.AddScoped<ZiraatBankConfiguration>();
            services.AddScoped<AkbankConfiguration>();

            services.AddTransient<ISimplePayConfiguration, SimplePayConfiguration>();

            services.AddTransient<Func<Banks, IProviderService>>(serviceProvider => key =>
            {
                var provider = SimplePayGlobal.BankProviders[key];
                var instance = (IProviderService)serviceProvider.GetService(provider);
                return instance;
            });

            services.AddTransient<Func<Banks, IProviderConfiguration>>(serviceProvider => key =>
            {
                var provider = SimplePayGlobal.BankConfiguration[key];
                var instance = (IProviderConfiguration)serviceProvider.GetService(provider);

                return instance;
            });

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            return services;
        }
    }
}
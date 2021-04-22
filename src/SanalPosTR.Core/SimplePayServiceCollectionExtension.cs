using Microsoft.Extensions.DependencyInjection;
using SanalPosTR.Configuration;
using SanalPosTR.Providers;
using SanalPosTR.Providers.Est;
using SanalPosTR.Providers.Ykb;
using System;
using System.Text;

namespace SanalPosTR
{
    public static class SimplePayServiceCollectionExtension
    {
        public static IServiceCollection AddSanalPosTR(this IServiceCollection services)
        {

            services.AddScoped<NestPayProviderService>();
            services.AddScoped<YKBProviderServices>();

            services.AddScoped<NestPayConfiguration>();
            services.AddScoped<YKBConfiguration>();

            services.AddTransient<Func<BankTypes, IProviderService>>(serviceProvider => key =>
            {
                var provider = SimplePayGlobal.BankProviders[key];
               // var instance = (IProviderService)serviceProvider.GetRequiredService(provider);

                var instance = (IProviderService)ActivatorUtilities.CreateInstance(serviceProvider, provider);

                instance.CurrentBank = key;
                return instance;
            });

            services.AddTransient<Func<BankTypes, IProviderConfiguration>>(serviceProvider => key =>
            {
                var provider = SimplePayGlobal.BankConfiguration[key];
                return provider;
            });

            services.AddTransient<ISimplePayConfiguration, SimplePayConfiguration>();


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            
            return services;
        }
    }
}
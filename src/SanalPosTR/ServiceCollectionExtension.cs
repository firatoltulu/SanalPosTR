using Microsoft.Extensions.DependencyInjection;
using SanalPosTR.Configuration;
using SanalPosTR.Providers;
using SanalPosTR.Providers.Est;
using SanalPosTR.Providers.Ykb;
using System;
using System.Text;

namespace SanalPosTR
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddSanalPosTR(this IServiceCollection services)
        {

            services.AddScoped<GarantiProviderService>();
            services.AddScoped<YKBProviderServices>();
            services.AddScoped<KuveytTurkProviderServices>();

            services.AddScoped<NestPayConfiguration>();
            services.AddScoped<YKBConfiguration>();
            services.AddScoped<KuveytTurkProviderServices>();

            services.AddTransient<Func<BankTypes, IProviderService>>(serviceProvider => key =>
            {
                var provider = Definition.BankProviders[key];
               // var instance = (IProviderService)serviceProvider.GetRequiredService(provider);

                var instance = (IProviderService)ActivatorUtilities.CreateInstance(serviceProvider, provider);

                instance.CurrentBank = key;
                return instance;
            });

            services.AddTransient<Func<BankTypes, IProviderConfiguration>>(serviceProvider => key =>
            {
                var provider = Definition.BankConfiguration[key];
                return provider;
            });

            services.AddTransient<IConfigurationService, ConfigurationService>();


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            
            return services;
        }
    }
}
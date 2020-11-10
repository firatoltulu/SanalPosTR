using Microsoft.Extensions.DependencyInjection;
using SimplePayTR.Core.Configuration;
using SimplePayTR.Core.Providers;
using SimplePayTR.Core.Providers.Est;
using System;

namespace SimplePayTR.Core
{
    public static class SimplePayServiceCollectionExtension
    {
        public static IServiceCollection AddSimplePayTR(this IServiceCollection services)
        {
            services.AddScoped<EstProviderService>();
            services.AddScoped<EstConfiguration>();

            services.AddTransient<Func<ProviderTypes, IProviderService>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case ProviderTypes.Est:
                        return serviceProvider.GetService<EstProviderService>();

                    default:
                        return null;
                }
            });


            services.AddTransient<Func<ProviderTypes, IProviderConfiguration>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case ProviderTypes.Est:
                        return serviceProvider.GetService<EstConfiguration>();

                    default:
                        return null;
                }
            });

            return services;
        }
    }
}
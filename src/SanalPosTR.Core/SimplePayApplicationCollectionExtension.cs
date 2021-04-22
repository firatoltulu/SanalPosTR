using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SanalPosTR.Configuration;
using System;

namespace SanalPosTR
{
    public static class SimplePayApplicationCollectionExtension
    {
        public static IApplicationBuilder UseSanalPosTR(
                this IApplicationBuilder app,
                Action<ISimplePayConfiguration> options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var services = app.ApplicationServices;
            var configuration = services.GetRequiredService<ISimplePayConfiguration>();

            options(configuration);

            return app;
        }
    }
}
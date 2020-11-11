using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimplePayTR.Core.Configuration;
using System;

namespace SimplePayTR.Core
{
    public static class SimplePayApplicationCollectionExtension
    {
        public static IApplicationBuilder UseSimplePayTR(
                this IApplicationBuilder app,
                Action<ISimplePayConfiguration> options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var services = app.ApplicationServices;
            var configuration = services.GetService<ISimplePayConfiguration>();

            options(configuration);

            return app;
        }
    }
}
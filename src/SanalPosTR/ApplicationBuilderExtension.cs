using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SanalPosTR.Configuration;
using System;

namespace SanalPosTR
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseSanalPosTR(
                this IApplicationBuilder app,
                Action<IConfigurationService> options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var services = app.ApplicationServices;
            var configuration = services.GetRequiredService<IConfigurationService>();

            TemplateHelper.initializeTemplate();

            options(configuration);

            return app;
        }
    }
}
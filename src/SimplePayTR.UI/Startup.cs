using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimplePayTR.Core;
using SimplePayTR.UI.Caching;
using SimplePayTR.UI.Data;
using SimplePayTR.UI.Data.DB;
using SimplePayTR.UI.Data.Entities;
using SimplePayTR.UI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimplePayTR.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AppConfig appConfig = new AppConfig();
            var section = Configuration.GetSection("SimplePayTR");
            section.Bind(appConfig);

            services.AddSingleton(appConfig.GetType(), appConfig);

            services.AddSimplePayTR();

            services.AddLinqToDbContext<DatabaseConnection>((provider, options) =>
            {
                options
                .UseSqlServer(Configuration.GetConnectionString("Default"))
                .UseDefaultLogging(provider);
            });

            services.AddOptions();
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
            });

            services.AddSession(options =>
            {
                options.Cookie = new CookieBuilder()
                {
                    Name = "SimplePaySession",
                    HttpOnly = true,
                };
                // options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.IsEssential = true;
            });

            services.AddTransient<ICache, RedisCache>();
            services.AddTransient<IDataServices, DataServices>();
            services.AddControllersWithViews();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dataConnection = scope.ServiceProvider.GetService<DatabaseConnection>();
                try
                {
                    dataConnection.CreateTable<PaySession>();
                    dataConnection.CreateTable<PosConfiguration>();
                }
                catch (System.Exception)
                {
                }
            }

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                endpoints.MapPost("/fail", (v) =>
                {
                    v.Response.Redirect("/");

                    return Task.CompletedTask;
                });
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });

            app.UseSimplePayTR(async opts =>
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var dataServices = scope.ServiceProvider.GetService<IDataServices>();

                    try
                    {
                        var posConfigs = (await dataServices.GetPosConfigurationsAsync()).Where(x => x.Active == true).ToList();
                        posConfigs.ForEach(v =>
                        {
                            opts.UseFromJSON(v.BankType, v.Configuration);
                            Program.PosConfiguration.Add(v.BankType, v);
                        });
                    }
                    catch (System.Exception)
                    {
                    }
                }

                opts.SetSuccessReturnUrl("https://localhost:44301/api/simplePay/ValidatePayment");
                opts.SetFailReturnUrl("https://localhost:44301/api/simplePay/ValidatePayment");
 
            });
        }
    }
}
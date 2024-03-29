using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SanalPosTR.Playground.Caching;
using SanalPosTR.Playground.Data;
using SanalPosTR.Playground.Data.DB;
using SanalPosTR.Playground.Data.Entities;
using SanalPosTR.Playground.Models;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SanalPosTR.Playground
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
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            AppConfig appConfig = new AppConfig();
            var section = Configuration.GetSection("SanalPosTRTR");
            section.Bind(appConfig);

            services.AddSingleton(appConfig.GetType(), appConfig);
            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin", builder => builder
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
            });
            services.AddSanalPosTR();
            services.AddLogging();

            services.AddLinqToDbContext<DatabaseConnection>((provider, options) =>
            {
                options
                .UseConnectionString(new LinqToDB.DataProvider.PostgreSQL.PostgreSQLDataProvider("psql", LinqToDB.DataProvider.PostgreSQL.PostgreSQLVersion.v95), Configuration.GetConnectionString("Default"))
                .UseDefaultLogging(provider);
            });

            services.AddOptions();
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.None;
                options.Secure = CookieSecurePolicy.Always;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddTransient<ICache, RedisCache>();
            services.AddTransient<IDataServices, DataServices>();
            services.AddControllersWithViews();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        private bool TableExists(DatabaseConnection databaseConnection, string tableName)
        {
            var command = new CommandInfo(databaseConnection, @$" SELECT EXISTS (SELECT * FROM pg_tables WHERE schemaname = 'public' AND tablename = '{tableName}')");
            return command.Execute<bool>();
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
                    if (TableExists(dataConnection, dataConnection.PaySessions.TableName) == false)
                        dataConnection.CreateTable<PaySession>();

                    if (TableExists(dataConnection, dataConnection.PosConfigurations.TableName) == false)
                        dataConnection.CreateTable<PosConfiguration>();

                    if (TableExists(dataConnection, dataConnection.PosInstallments.TableName) == false)
                        dataConnection.CreateTable<PosInstallment>();

                    if (TableExists(dataConnection, dataConnection.PosBinNumbers.TableName) == false)
                        dataConnection.CreateTable<PosBinNumber>();
                }
                catch (System.Exception ex)
                {
                }
            }
            app.UseCors("AllowOrigin");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseCookiePolicy();
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

            app.UseSanalPosTR(async opts =>
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var dataServices = scope.ServiceProvider.GetService<IDataServices>();
                    var appConfig = scope.ServiceProvider.GetService<AppConfig>();

                    try
                    {
                        var posConfigs = (await dataServices.GetPosConfigurationsAsync()).Where(x => x.Active == true).ToList();
                        posConfigs.ForEach(v =>
                        {
                            Log.Information($"{Enum.GetName(v.BankType.GetType(), v.BankType)} Loaded");

                            opts.UseFromJSON(v.BankType, v.Configuration);
                            Program.PosConfiguration.Add(v.BankType, v);
                        });
                    }
                    catch (System.Exception)
                    {
                    }

                    opts.SetSuccessReturnUrl(appConfig.SuccessEndPoint);
                    opts.SetFailReturnUrl(appConfig.FailEndPoint);
                }
            });

            Log.Information($"SanalPosTR Running");
        }
    }
}
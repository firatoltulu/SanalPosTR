using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SimplePayTR.Core;
using System.Collections.Generic;
using System.IO;

namespace SimplePayTR.UI
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(Configuration)
               .Enrich.WithProperty("App Name", "Simple Pay")
               .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static Dictionary<BankTypes, Data.Entities.PosConfiguration> PosConfiguration { get; set; } = new Dictionary<BankTypes, Data.Entities.PosConfiguration>();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
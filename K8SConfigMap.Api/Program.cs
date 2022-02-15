using K8SConfigMap.Api.Framework.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace K8SConfigMap.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args);
            builder
                .Build()
                .Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    //config.AddJsonFile(
                    //    new ConfigMapFileProvider(Path.Combine(AppContext.BaseDirectory, "servicemap")),
                    //    "ServiceRouting.json",
                    //    false,
                    //    true);
                    config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "servicemap", "ServiceRouting.json"), false, true);
                })
                .ConfigureWebHostDefaults(wb =>
                {
                    wb.UseStartup<Startup>();
                });
    }
}

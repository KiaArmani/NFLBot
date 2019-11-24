using System;
using CommandLine;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using XurMongoBridge.Services;

namespace XurMongoBridge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    Console.WriteLine(o.Verbose
                        ? $"Verbose output enabled. Current Arguments: -v {o.Verbose}"
                        : $"Current Arguments: -v {o.Verbose}");
                });

            var host = CreateWebHostBuilder(args).Build();
            host.Services.GetRequiredService<MongoService>().InitializeMongoConnection();
            host.Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseUrls("https://0.0.0.0:6000").UseStartup<Startup>();
        }
    }
}
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using XurCollector.Services;

namespace XurCollector
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Print Copyright Notice
            await LogAsync(new LogMessage(LogSeverity.Info, "XurCollector",
                "Copyright 2019, Kia Armani <contact@xi4.me>")).ConfigureAwait(false);

            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.
            await using var services = ConfigureServices();
            services.GetRequiredService<BungieService>().Log += LogAsync;
            services.GetRequiredService<MongoService>().Log += LogAsync;

            // Start Mongo Service
            var mongoService = services.GetRequiredService<MongoService>();
            mongoService.Initialize();

            var activityCache = services.GetRequiredService<ActivityCacheService>();
            activityCache.FillActivityCache();

            var workerService = services.GetRequiredService<WorkerService>();
            await workerService.DoWork(new CancellationToken());

            await Task.Delay(-1);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException);
            Console.WriteLine(ex.StackTrace);
        }

        private static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private static ServiceProvider ConfigureServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton<HttpClient>()
                .AddSingleton<MongoService>()
                .AddSingleton<ActivityCacheService>()
                .AddSingleton<BungieService>()
                .AddSingleton<WorkerService>();

            return collection.BuildServiceProvider();
        }
    }
}
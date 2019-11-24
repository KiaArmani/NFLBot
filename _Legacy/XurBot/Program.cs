using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using XurBot.Services;

namespace XurBot
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
            await LogAsync(new LogMessage(LogSeverity.Info, "NFLBot",
                "Copyright 2017, Christopher F. <foxbot@protonmail.com>")).ConfigureAwait(false);
            await LogAsync(new LogMessage(LogSeverity.Info, "NFLBot",
                "Copyright (c) 2017-2018, Benn Benson All rights reserved.")).ConfigureAwait(false);
            await LogAsync(new LogMessage(LogSeverity.Info, "NFLBot", "Copyright 2019, Kia Armani <contact@xi4.me>"))
                .ConfigureAwait(false);

            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.
            using var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;

            // Tokens should be considered secret data and never hard-coded.
            // We can read from the environment variable to avoid hardcoding.
            await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("NFLBOT_DISCORDTOKEN"))
                .ConfigureAwait(false);
            await client.StartAsync().ConfigureAwait(false);

            // Here we initialize the logic required to register our commands.
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync().ConfigureAwait(false);

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
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>();

            return collection.BuildServiceProvider();
        }
    }
}
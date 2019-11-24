using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using XurBot.Services;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace XurBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            // Print Copyright Notice
            await LogAsync(new LogMessage(LogSeverity.Info, "NFLBot", "Copyright 2017, Christopher F. <foxbot@protonmail.com>")).ConfigureAwait(false);
            await LogAsync(new LogMessage(LogSeverity.Info, "NFLBot", "Copyright (c) 2017-2018, Benn Benson All rights reserved.")).ConfigureAwait(false);
            await LogAsync(new LogMessage(LogSeverity.Info, "NFLBot", "Copyright 2019, Kia Armani <contact@xi4.me>")).ConfigureAwait(false);

            // You should dispose a service provider created using ASP.NET
            // when you are finished using it, at the end of your app's lifetime.
            // If you use another dependency injection framework, you should inspect
            // its documentation for the best way to do this.
            NightfallService destinyService;
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();

                client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;
                services.GetRequiredService<NightfallService>().Log += LogAsync;

                // Start Destiny Service
                destinyService = services.GetRequiredService<NightfallService>();
                destinyService.Initialize();
                
                // Tokens should be considered secret data and never hard-coded.
                // We can read from the environment variable to avoid hardcoding.
                await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("NFLBOT_DISCORDTOKEN")).ConfigureAwait(false);
                await client.StartAsync().ConfigureAwait(false);

                // Here we initialize the logic required to register our commands.
                await services.GetRequiredService<CommandHandlingService>().InitializeAsync().ConfigureAwait(false);

                // Set a recurring job for updating the scores (every 15 minutes)
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                Task timerTask = CheckProgress(destinyService, TimeSpan.FromMinutes(5), tokenSource.Token);

                await Task.Delay(-1).ConfigureAwait(false);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<NightfallService>();

            return collection.BuildServiceProvider();
        }

        async Task CheckProgress(NightfallService destinyService, TimeSpan interval, CancellationToken token)
        {
            while (true)
            {
                await destinyService.GetPCGRs();
                await destinyService.CheckChallengeActivities();
                await destinyService.LoadScores(30, 2);
                await Task.Delay(interval, token);
            }
        }
    }
}

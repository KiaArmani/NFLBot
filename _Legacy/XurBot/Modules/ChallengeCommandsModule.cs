using BungieNet;
using Discord;
using Discord.Commands;
using XurBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using XurClassLibrary.Models;

namespace XurBot.Modules
{
    public class ChallengeCommandsModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public MongoService _MongoService { get; set; }

        

        /// <summary>
        /// Creates and sends a beautiful embed message for up to 3 ranks.
        /// </summary>
        /// <param name="scoreEntries"></param>
        /// <returns></returns>
        private async Task BuildTopEmbed(string playerName)
        {
            var completedEmoji = new Emoji("\u2705");

            var eb = new EmbedBuilder
            {
                ThumbnailUrl =
                    "https://cdn.vox-cdn.com/thumbor/5IO-uBZNuGOtKVDY3HprUJQ5iV8=/0x0:1920x1080/920x613/filters:focal(662x200:968x506):format(webp)/cdn.vox-cdn.com/uploads/chorus_image/image/65607794/Xur_Destiny_2_.0.jpg",
                Title = "Trials of the Xichedelic",
                Description = $"Week {Globals.CurrentWeek}",
                Fields = new List<EmbedFieldBuilder>()
            };

            foreach (WeeklyChallenge weekly in Globals.WeeklyChallenges)
            {
                if(weekly.Metadata.IsHidden)
                    continue;

                var hasCompletedChallenge = _MongoService.HasCompletedChallenge(playerName, weekly.Metadata.Week, weekly.Metadata.Tier, weekly.Metadata.Difficulty);
                var descriptionString = hasCompletedChallenge ? $"{completedEmoji} {weekly.Description}" : weekly.Description;

                var efb = new EmbedFieldBuilder
                {
                    Name = weekly.Name, Value = descriptionString, IsInline = false
                };

                eb.Fields.Add(efb);
            }

            await Context.Channel.SendMessageAsync("", false, eb.Build());

        }

        /// <summary>
        /// Posts a scoreboard with the top scores across all Nightfalls and difficulties
        /// </summary>
        /// <param name="objects">[0] amount of entries to post</param>
        /// <returns></returns>
        [Command("nine")]
        public async Task MainCommand(params string[] objects)
        {
            if (objects.Length == 0)
                return;

            string playerName = objects[0];            

            await BuildTopEmbed(playerName);
        }


        [Command("nine help")]
        public async Task PostHelp()
        {
            var eb = new EmbedBuilder
            {
                Title = "Nine Bot Help",
                Description = "Syntax for Nine Bot Commands",
                Fields = new List<EmbedFieldBuilder>()
            };

            var efb3 = new EmbedFieldBuilder {Name = "!nine challenges", Value = "Prints out this weeks challenges."};
            eb.Fields.Add(efb3);

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        /// <summary>
        /// Returns the Ordinal for the given Number
        /// Code from: https://stackoverflow.com/a/20175
        /// Author: samjudson - https://stackoverflow.com/users/1908/samjudson
        /// Code licensed under cc-by-sa as per https://stackoverflow.com/help/licensing
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string AddOrdinal(int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }
    }
}

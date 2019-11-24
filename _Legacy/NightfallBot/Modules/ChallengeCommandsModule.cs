using BungieNet;
using Discord;
using Discord.Commands;
using XurBot.Models;
using XurBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XurBot.Modules
{
    public class ChallengeCommandsModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public NightfallService DestinyService { get; set; }

        

        /// <summary>
        /// Creates and sends a beautiful embed message for up to 3 ranks.
        /// </summary>
        /// <param name="scoreEntries"></param>
        /// <returns></returns>
        private async Task BuildTopEmbed(string playerName)
        {
            var completedEmoji = new Emoji("\u2705");

            EmbedBuilder eb = new EmbedBuilder();
            eb.ThumbnailUrl = "https://cdn.vox-cdn.com/thumbor/5IO-uBZNuGOtKVDY3HprUJQ5iV8=/0x0:1920x1080/920x613/filters:focal(662x200:968x506):format(webp)/cdn.vox-cdn.com/uploads/chorus_image/image/65607794/Xur_Destiny_2_.0.jpg";
            eb.Title = "Trials of the Xichedelic";
            eb.Description = $"Week {Globals.Tier1.Week}";
            eb.Fields = new List<EmbedFieldBuilder>();

            foreach (WeeklyChallenge weekly in Globals.WeeklyChallenges)
            {
                bool hasCompletedChallenge = await DestinyService.GetChallengeCompletionStatus(weekly.Metadata, playerName);
                string descriptionString = hasCompletedChallenge ? $"{completedEmoji} {weekly.Description}" : weekly.Description;

                EmbedFieldBuilder efb = new EmbedFieldBuilder();
                efb.Name = weekly.Name;
                efb.Value = descriptionString;
                efb.IsInline = weekly.Metadata.Tier != 3;

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

        /// <summary>
        /// Loads the latest scores of the clan members.
        /// Given that people could be doing multiple strikes in a row, this should be called around every 15 - 30 minutes.
        /// Only executable by the Owner of the Bot (aka. owner of Token)
        /// </summary>
        /// <returns></returns>
        [RequireOwner]
        [Command("nine load")]
        public async Task LoadScores()
        {
            try
            {
                await DestinyService.LoadScores(50, 5).ConfigureAwait(false);
                await ReplyAsync(message: $"Successfully refreshed the Scores!").ConfigureAwait(false);
            }
            catch(BungieException e)
            {
                await ReplyAsync($"Exception while loading Scores: {e.Message}").ConfigureAwait(false);
            }            
        }

        /// <summary>
        /// Resets the MongoDB collection, therefore deleting all saved scores.
        /// Only executable by the Owner of the Bot (aka. owner of Token)
        /// </summary>
        /// <returns></returns>
        [RequireOwner]
        [Command("nine init")]
        public async Task InitDatabase()
        {
            DestinyService.ResetDatabase();
            await ReplyAsync(message: $"Successfully reset the Database!").ConfigureAwait(false);
        }

        [Command("nine help")]
        public async Task PostHelp()
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Title = "Nine Bot Help";
            eb.Description = "Syntax for Nine Bot Commands";
            eb.Fields = new List<EmbedFieldBuilder>();

            EmbedFieldBuilder efb3 = new EmbedFieldBuilder();
            efb3.Name = "!nine challenges";
            efb3.Value = "Prints out this weeks challenges.";
            eb.Fields.Add(efb3);

            EmbedFieldBuilder efb4 = new EmbedFieldBuilder();
            efb4.Name = "!nine init";
            efb4.Value = "Requires Bot Owner. Resets the Challenge database.";
            eb.Fields.Add(efb4);

            EmbedFieldBuilder efb5 = new EmbedFieldBuilder();
            efb5.Name = "!nine load";
            efb5.Value = "Requires Bot Owner. Forces loading of new activity data.";
            eb.Fields.Add(efb5);

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

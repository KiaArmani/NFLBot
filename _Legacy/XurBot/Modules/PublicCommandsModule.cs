using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleTables;
using Discord;
using Discord.Commands;
using XurClassLibrary.Extensions;
using XurClassLibrary.Models;

namespace XurBot.Modules
{
    public class PublicCommandsModule : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        ///     Creates and sends a beautiful embed message for up to 3 ranks.
        /// </summary>
        /// <param name="scoreEntries"></param>
        /// <returns></returns>
        private async Task BuildTopEmbed(string title, string description, List<ScoreEntry> scoreEntries)
        {
            var eb = new EmbedBuilder
            {
                Title = title, Description = description, Fields = new List<EmbedFieldBuilder>()
            };

            var i = 1;
            var distinctScores = scoreEntries.GroupBy(x => x.Score);
            foreach (var distinctScore in distinctScores)
            {
                var entriesWithScore = scoreEntries.Where(x => x.Score == distinctScore.Key);
                var withScore = entriesWithScore as ScoreEntry[] ?? entriesWithScore.ToArray();
                var playerNamesWithScore = withScore.Select(x => x.Name).ToList();

                var efb = new EmbedFieldBuilder
                {
                    Name = $"{AddOrdinal(i)} Place: {string.Join(',', playerNamesWithScore)}",
                    Value = $"{distinctScore.Key} points in {withScore.FirstOrDefault()?.ActivityName}"
                };

                eb.Fields.Add(efb);
                i++;
            }

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        /// <summary>
        ///     Creates and sends a ConsoleTable using a given List of ScoreEntries.
        ///     If the amount of characters exceed 1400, multiple messages are sent.
        /// </summary>
        /// <param name="scoreEntries">List of Scores</param>
        /// <param name="personal"></param>
        /// <returns></returns>
        private async Task BuildTable(List<ScoreEntry> scoreEntries, bool personal = false)
        {
            var reply = new StringBuilder();
            if (!personal)
            {
                // Create StringBuilder to hold Reply
                reply.Append("\n```");

                // Create new Table
                var table = new ConsoleTable("#", "Members", "Nightfall", "Score", "Date");
                var i = 1;

                var distinctScores = scoreEntries.GroupBy(x => x.Score);
                foreach (var distinctScore in distinctScores)
                {
                    var entriesWithScore = scoreEntries.Where(x => x.Score == distinctScore.Key);
                    var withScore = entriesWithScore as ScoreEntry[] ?? entriesWithScore.ToArray();
                    var playerNamesWithScore = withScore.Select(x => x.Name).ToList();

                    var first = withScore.FirstOrDefault();

                    if (first != null)
                        table.AddRow(AddOrdinal(i), string.Join(',', playerNamesWithScore), first.ActivityName,
                            distinctScore.Key, first.ActivityDate);
                    i++;

                    // Cut Table at 1400 chars, so longer lists get sent in multiple messages
                    if (table.ToMarkDownString().Length < 1400) continue;

                    // Send Table
                    await SendTable(reply, table).ConfigureAwait(false);

                    // Create New Table
                    table = new ConsoleTable("#", "Member", "Nightfall", "Score", "Date");
                    reply.Clear().Append("\n```");
                }

                // Send final, or only, Table
                if (table.ToMarkDownString().Length > 150)
                    await SendTable(reply, table).ConfigureAwait(false);
            }
            else
            {
                reply.Append("\n```");

                // Create new Table
                var table = new ConsoleTable("#", "Member", "Nightfall", "Score", "Date");
                decimal previousScore = 0;
                var i = 0;

                foreach (var entry in scoreEntries)
                {
                    var differentScore = entry.Score != previousScore;
                    if (differentScore)
                        i++;

                    var index = personal ? await ApiConnectorModule.GetPositionOfScoreAsync(entry.NightfallId) : i;

                    // Add Score to Table
                    table.AddRow(index == -1 ? "-" : index.ToString(), entry.Name.Truncate(20), entry.Score,
                        entry.ActivityName, entry.ActivityDate);

                    // Cut Table at 1400 chars, so longer lists get sent in multiple messages
                    if (table.ToMarkDownString().Length >= 1400)
                    {
                        // Send Table
                        await SendTable(reply, table).ConfigureAwait(false);

                        // Create New Table
                        table = new ConsoleTable("#", "Member", "Nightfall", "Score", "Date");
                        reply.Clear().Append("\n```");
                    }

                    previousScore = entry.Score;
                }

                // Send final, or only, Table
                if (table.ToMarkDownString().Length > 150)
                    await SendTable(reply, table).ConfigureAwait(false);
            }
        }

        /// <summary>
        ///     Sends the given ConsoleTable.
        ///     Requires the StringBuilder to include a backtick already to function.
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private async Task SendTable(StringBuilder reply, ConsoleTable table)
        {
            reply.Append(table.ToMarkDownString());
            reply.Append("```");
            await ReplyAsync(reply.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        ///     Posts a scoreboard with the top scores across all Nightfalls and difficulties
        /// </summary>
        /// <param name="objects">[0] amount of entries to post</param>
        /// <returns></returns>
        [Command("nfl top")]
        public async Task MainCommand(params string[] objects)
        {
            // If it isn't specified how many rows we want, don't do anything
            if (objects == null)
                return;

            // If the given parameter isn't a number, don't do anything
            if (!int.TryParse(objects[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var topX))
                return;

            if (topX <= 0)
                return;

            // Request Scores from Destiny Service
            var result = await ApiConnectorModule.GetTopOrdealScoresAsync(topX);

            if (topX > 3)
                // Build and send Response Table
                await BuildTable(result).ConfigureAwait(false);
            else
                await BuildTopEmbed("Top Nightfall Ranks", $"Listing the top {topX} groups of the current Season.",
                    result).ConfigureAwait(false);
        }

        [Command("nfl help")]
        public async Task PostHelp()
        {
            var eb = new EmbedBuilder
            {
                Title = "NFL Bot Help",
                Description = "Syntax for NFL Bot Commands",
                Fields = new List<EmbedFieldBuilder>()
            };

            var efb = new EmbedFieldBuilder
            {
                Name = "!nfl top {0} (e.g. !nfl top 10)",
                Value =
                    "Shows the overall top scores across all Activities, {0} being the amount of results you want"
            };
            eb.Fields.Add(efb);

            var efb2 = new EmbedFieldBuilder
            {
                Name = "!nfl score {0} {ADEPT/HERO/MASTER/LEGEND} (e.g. !nfl score \"The Pyramidion\")",
                Value = "Shows the Top10 of a given Nightfall. {0} being the Nightfall Name and {1} the difficulty."
            };
            eb.Fields.Add(efb2);

            var efb3 = new EmbedFieldBuilder
            {
                Name =
                    "!nfl player {0} {1} {2} {ADEPT/HERO/MASTER/LEGEND} (e.g. !nfl player Kia 10 \"The Pyramidion\" )",
                Value =
                    "Gets a players scores. {0} is the player name. {1} the amount of results. {2} the activity name. (Everything but the name is optional)"
            };
            eb.Fields.Add(efb3);

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }

        /// <summary>
        ///     Returns the Ordinal for the given Number
        ///     Code from: https://stackoverflow.com/a/20175
        ///     Author: samjudson - https://stackoverflow.com/users/1908/samjudson
        ///     Code licensed under cc-by-sa as per https://stackoverflow.com/help/licensing
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private static string AddOrdinal(int num)
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
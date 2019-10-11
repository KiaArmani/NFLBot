using BungieNet;
using ConsoleTables;
using Discord;
using Discord.Commands;
using NFLBot.Models;
using NFLBot.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NFLBot.Modules
{
    public class PublicCommandsModule : ModuleBase<SocketCommandContext>
    {
        // Dependency Injection will fill this value in for us
        public DestinyService DestinyService { get; set; }

        /// <summary>
        /// Creates and sends a ConsoleTable using a given List of ScoreEntries.
        /// If the amount of characters exceed 1400, multiple messages are sent.
        /// </summary>
        /// <param name="scoreEntries">List of Scores</param>
        /// <returns></returns>
        private async Task BuildTable(List<ScoreEntry> scoreEntries)
        {
            // Create StringBuilder to hold Reply
            StringBuilder reply = new StringBuilder();
            reply.Append("\n`");

            // Create new Table
            var table = new ConsoleTable("#", "Member", "Score", "Nightfall", "Date");
            var i = 1;

            foreach (var entry in scoreEntries)
            {
                // Add Score to Table
                table.AddRow(i, entry.Name, entry.Score, entry.ActivityName, entry.ActivityDate);

                // Cut Table at 1400 chars, so longer lists get sent in multiple messages
                if (table.ToMarkDownString().Length >= 1400)
                {
                    // Send Table
                    await SendTable(reply, table).ConfigureAwait(false);

                    // Create New Table
                    table = new ConsoleTable("#", "Member", "Score", "Nightfall", "Date");
                    reply.Clear().Append("\n`");
                }

                // Increase Counter for Rank in Table
                i++;
            }

            // Send final, or only, Table
            await SendTable(reply, table).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the given ConsoleTable.
        /// Requires the StringBuilder to include a backtick already to function.
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private async Task SendTable(StringBuilder reply, ConsoleTable table)
        {
            reply.Append(table.ToMarkDownString());
            reply.Append("`");
            await ReplyAsync(reply.ToString()).ConfigureAwait(false);
        }

        /// <summary>
        /// Posts a scoreboard with the top scores across all Nightfalls and difficulties
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
            if (!int.TryParse(objects[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int topX))
                return;

            if (topX <= 0)
                return;

            // Request Scores from Destiny Service
            var result = DestinyService.GetTopScores(topX);

            // Build and send Response Table
            await BuildTable(result).ConfigureAwait(false);
        }

        /// <summary>
        /// Posts the scores of the given nightfall and difficulty (Ordeal)
        /// </summary>
        /// <param name="objects">[0] is Nightfall name, [1] (optional) the difficulty</param>
        /// <returns></returns>
        [Command("nfl score")]
        public async Task GetScore(params string[] objects)
        {
            // Get Amount of Parameters
            int paramCount = objects.Length;
            if (paramCount == 0)
            {
                return;
            }

            // Get Location from first Parameter
            string location = objects[0];
            if (paramCount > 1)
            {
                // Check if the given Difficulty is valid
                string difficulty = objects[1].ToUpper();
                bool validDifficulty = OrdealDifficulties.ValidDifficulties.Contains(difficulty.ToUpper(CultureInfo.InvariantCulture));

                // If yes, get scores for the given Nightfall using the difficulty
                if (validDifficulty)
                    await GetLocationScores(location, difficulty).ConfigureAwait(false);
                else // just give out the scores for normal Nightfall
                    await GetLocationScores(location).ConfigureAwait(false);
            }
            else // if no difficulty was specified, return normal scores
            {
                await GetLocationScores(location).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns all Scores for a given player name
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        [Command("nfl player")]
        public async Task GetPlayerScore(params string[] objects)
        {
            // Get Amount of Parameters
            int paramCount = objects.Length;
            if (paramCount == 0)
            {
                return;
            }

            // Get Location from first Parameter
            string playerName = objects[0];
            if (paramCount > 1)
            {
                // If the given parameter isn't a number, don't do anything
                if (!int.TryParse(objects[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int topX))
                    return;

                if (paramCount > 2)
                {
                    string location = objects[2];
                    if(paramCount > 3)
                    {
                        // Check if the given Difficulty is valid
                        string difficulty = objects[3].ToUpper();
                        bool validDifficulty = OrdealDifficulties.ValidDifficulties.Contains(difficulty.ToUpper(CultureInfo.InvariantCulture));
                        if(validDifficulty)
                        {
                            await GetPlayerScores(playerName, topX, location, difficulty).ConfigureAwait(false);
                        }
                        else
                        {
                            await GetPlayerScores(playerName, topX, location).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await GetPlayerScores(playerName, topX, location).ConfigureAwait(false);
                    }                    
                }
                else
                {
                    await GetPlayerScores(playerName, topX).ConfigureAwait(false);
                }
            }
            else // if no difficulty was specified, return normal scores
            {
                await GetPlayerScores(playerName, 10).ConfigureAwait(false);
            }
        }

        private async Task GetPlayerScores(string playerName, int max, string location = "ANY", string difficulty = "NONE")
        {
            // Get Scores from DestinyService
            List<ScoreEntry> scores = DestinyService.GetPlayerScoresFromDatabase(playerName, max, location, difficulty);

            // If Scores are available, build the table. (Preventing replying with an empty table)
            if (scores.Count > 0)
                await BuildTable(scores).ConfigureAwait(false);
            else
                await ReplyAsync($"No scores for {playerName} on Location {location} available. Please try again later.").ConfigureAwait(false);
        }

        /// <summary>
        /// Loads Scores for a given Nightfall
        /// If the difficulty parameter is given, Ordeal scores will be loaded
        /// </summary>
        /// <param name="location">Name of the Nightfall as in the Manifest</param>
        /// <param name="difficulty">Element of OrdealDifficulties.ValidDifficulties</param>
        /// <returns></returns>
        private async Task GetLocationScores(string location, string difficulty = "NONE")
        {
            // Get Scores from DestinyService
            List<ScoreEntry> scores = DestinyService.GetTopScoresForLocation(location, 10, difficulty);

            // If Scores are available, build the table. (Preventing replying with an empty table)
            if (scores.Count > 0)
                await BuildTable(scores).ConfigureAwait(false);
            else
                await ReplyAsync($"No scores for {location} on Difficulty {difficulty} available. Please try again later.").ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the latest scores of the clan members.
        /// Given that people could be doing multiple strikes in a row, this should be called around every 15 - 30 minutes.
        /// Only executable by the Owner of the Bot (aka. owner of Token)
        /// </summary>
        /// <returns></returns>
        [RequireOwner]
        [Command("nfl load")]
        public async Task LoadScores()
        {
            try
            {
                await DestinyService.LoadScores().ConfigureAwait(false);
                await ReplyAsync(message: $"Successfully refreshed the Scores!").ConfigureAwait(false);
            }
            catch(BungieException e)
            {
                await ReplyAsync($"Exception while loading Scores: {e.Message}").ConfigureAwait(false);
            }            
        }

        [RequireOwner]
        [Command("nfl loadseason")]
        public async Task LoadSeasonScores()
        {
            try
            {
                await DestinyService.LoadScores(10000).ConfigureAwait(false);
                await ReplyAsync(message: $"Successfully refreshed the Scores for the Season!").ConfigureAwait(false);
            }
            catch (BungieException e)
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
        [Command("nfl init")]
        public async Task InitDatabase()
        {
            DestinyService.ResetDatabase();
            await ReplyAsync(message: $"Successfully reset the Database!").ConfigureAwait(false);
        }

        [Command("nfl help")]
        public async Task PostHelp()
        {
            EmbedBuilder eb = new EmbedBuilder();
            eb.Title = "NFL Bot Help";
            eb.Description = "Syntax for NFL Bot Commands";
            eb.Fields = new List<EmbedFieldBuilder>();

            EmbedFieldBuilder efb = new EmbedFieldBuilder();
            efb.Name = "!nfl top {0} (e.g. !nfl top 10)";
            efb.Value = "Shows the overall top scores across all Activities, {0} being the amount of results you want";
            eb.Fields.Add(efb);

            EmbedFieldBuilder efb2 = new EmbedFieldBuilder();
            efb2.Name = "!nfl score {0} {ADEPT/HERO/MASTER/LEGEND} (e.g. !nfl score \"The Pyramidion\")";
            efb2.Value = "Shows the Top10 of a given Nightfall. {0} being the Nightfall Name and {1} the difficulty.";
            eb.Fields.Add(efb2);


            EmbedFieldBuilder efb3 = new EmbedFieldBuilder();
            efb3.Name = "!nfl init";
            efb3.Value = "Requires Bot Owner. Resets the Score database.";
            eb.Fields.Add(efb3);

            EmbedFieldBuilder efb4 = new EmbedFieldBuilder();
            efb4.Name = "!nfl load";
            efb4.Value = "Requires Bot Owner. Forces loading of new scores.";
            eb.Fields.Add(efb4);

            EmbedFieldBuilder efb5 = new EmbedFieldBuilder();
            efb5.Name = "!nfl player {0} {1} {2} {ADEPT/HERO/MASTER/LEGEND} (e.g. !nfl player Kia 10 \"The Pyramidion\" )";
            efb5.Value = "Gets a players scores. {0} is the player name. {1} the amount of results. {2} the activity name. (Everything but the name is optional)";
            eb.Fields.Add(efb5);

            await Context.Channel.SendMessageAsync("", false, eb.Build());
        }
    }
}

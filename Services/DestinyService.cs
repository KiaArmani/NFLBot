using BungieNet.Api;
using MongoDB.Driver;
using NFLBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Text;
using BungieNet.GroupsV2;
using System.Globalization;
using BungieNet.Destiny.HistoricalStats;
using BungieNet;
using BungieNet.Destiny.Responses;
using Discord;
using Hangfire;

namespace NFLBot.Services
{
    public class DestinyService
    {
        private readonly BungieClient bungieClient;
        private IMongoCollection<ScoreEntry> collection;
        private SQLiteConnection sqlConnection;
        private MongoClient mongoClient;
        private DateTime currentSeasonStart;

        public event Func<LogMessage, Task> Log;

        public DestinyService(IServiceProvider services)
        {
            // Create new BungieClient
            bungieClient = new BungieClient(apiKey: new BungieApiKey(Environment.GetEnvironmentVariable("NFLBOT_BUNGIETOKEN")));

            // Set Season Date
            currentSeasonStart = Convert.ToDateTime("10.01.2019",
                                                    CultureInfo.InvariantCulture);
        }

        public void Initialize()
        {
            LoadManifest();
            InitializeSQLiteDatabase();
            InitializeMongoDatabase();            
        }

        /// <summary>
        /// Loads Manifest URLs using GetDestinyManifest, downloads the english MobileWorldContent Database and extracts it into \manifest.
        /// </summary>
        private void LoadManifest()
        {
            var manifestResult = bungieClient.Destiny2.GetDestinyManifest();
            var manifestURL = $"https://www.bungie.net{manifestResult.MobileWorldContentPaths["en"]}";

            using (var client = new WebClient())
            {
                WriteLog(LogSeverity.Info, "Downloading Manifest..");
                client.DownloadFile(manifestURL, "manifest.zip");
                WriteLog(LogSeverity.Info, "Downloading Manifest done!");

                WriteLog(LogSeverity.Info, "Extracting Manifest..");
                ZipFile.ExtractToDirectory("manifest.zip", "manifest", true);
                WriteLog(LogSeverity.Info, "Extracting Manifest done!");
            }
        }

        /// <summary>
        /// Initializes Connection to the Cloud Atlas Database and recieves all currently saved scores.
        /// </summary>
        private void InitializeMongoDatabase()
        {
            WriteLog(LogSeverity.Info, "Initializing Cloud Atlas Connection..");
            var atlasToken = Environment.GetEnvironmentVariable("NFLBOT_ATLASTOKEN");
            mongoClient = new MongoClient($"mongodb+srv://nflbot:{atlasToken}@d2tools-intaa.mongodb.net/admin?retryWrites=true&w=majority");
            WriteLog(LogSeverity.Info, "Cloud Atlas Connection established!");

            WriteLog(LogSeverity.Info, "Loading Collection..");
            var database = mongoClient.GetDatabase("d2tools");
            collection = database.GetCollection<ScoreEntry>("nfl");
            WriteLog(LogSeverity.Info, "Collection Loaded!");
        }

        /// <summary>
        /// Drops the nfl Collection in the Database and re-creates it
        /// </summary>
        public void ResetDatabase()
        {
            var database = mongoClient.GetDatabase("d2tools");
            database.DropCollection("nfl");
            database.CreateCollection("nfl");
        }

        /// <summary>
        /// Searches for the manifest file and opens it
        /// </summary>
        private void InitializeSQLiteDatabase()
        {
            WriteLog(LogSeverity.Info, "Loading Manifest Database..");
            var files = Directory.GetFiles("manifest", "*.content");
            var fileName = Path.GetFileName(files[0]);
            sqlConnection = new SQLiteConnection($"Data Source=manifest\\{fileName};Version=3;");
            sqlConnection.Open();
            WriteLog(LogSeverity.Info, " Manifest Database loaded!");
        }

        /// <summary>
        /// Calls GetBungieScores and loads the result in the global List.
        /// </summary>
        /// <returns></returns>
        ///     
        public async Task LoadScores(int count = 0)
        {
            WriteLog(LogSeverity.Info, "Loading Scores from Bungie..");
            await GetBungieScores(count).ConfigureAwait(true);
            var scoreEntries = collection.Find(Builders<ScoreEntry>.Filter.Empty).ToList();
            WriteLog(LogSeverity.Info, "Scores from Bungie loaded!");
        }

        /// <summary>
        /// Gets all current activities for all characters for all clan members.
        /// Only gets the latest page of activies for ScoredNightfall activities.
        /// Scores then get inserted into the collection.
        /// </summary>
        /// <returns></returns>
        private async Task GetBungieScores(int count)
        {
            // Get Clan ID from Environment Variables
            long clanID = Convert.ToInt64(Environment.GetEnvironmentVariable("NFLBOT_CLANID"), CultureInfo.InvariantCulture);

            WriteLog(LogSeverity.Info, "Loading Clan Information..");
            // Get Members of Clan from Bungie
            SearchResultOfGroupMember clanResult = await bungieClient.GroupV2.GetMembersOfGroupAsync(
                groupId: clanID,
                currentpage: 0,
                memberType: RuntimeGroupMemberType.Member,
                nameSearch: string.Empty).ConfigureAwait(false);
            WriteLog(LogSeverity.Info, $"Clan Information loaded! {clanResult.Results.Length} members found.");

            // Loop through Members
            foreach (GroupMember clanMember in clanResult.Results)
            {
                // Get their Membership ID required for getting their Profile & Activity Information
                long membershipId = clanMember.DestinyUserInfo.MembershipId;

                // Get their Profile Information
                DestinyProfileResponse memberdata = await bungieClient.Destiny2.GetProfileAsync(
                    membershipType: clanMember.DestinyUserInfo.MembershipType,
                    destinyMembershipId: membershipId,
                    components: new BungieNet.Destiny.DestinyComponentType[] { BungieNet.Destiny.DestinyComponentType.Characters }).ConfigureAwait(false);

                WriteLog(LogSeverity.Debug, $"Found {memberdata.Characters.Data.Count} characters for {clanMember.DestinyUserInfo.DisplayName}");

                // Loop through Characters of Member
                foreach (var playerCharacter in memberdata.Characters.Data)
                {
                    // Hold Character ID in seperate Variable for convienence
                    long characterId = playerCharacter.Key;

                    // Get Activity History of Character
                    // Using a Try-Catch Block as players might opt-in to refuse API calls to their history
                    DestinyActivityHistoryResults activityData = null;
                    try
                    {
                         activityData = await bungieClient.Destiny2.GetActivityHistoryAsync(
                            membershipType: playerCharacter.Value.MembershipType,
                            destinyMembershipId: membershipId,
                            characterId: characterId,
                            count: count,
                            mode: BungieNet.Destiny.HistoricalStats.Definitions.DestinyActivityModeType.ScoredNightfall,
                            page: 0).ConfigureAwait(false);                                                                   
                    }
                    catch (BungieException e) // Most likely to be privacy related exceptions. For now we just ignore those scores.
                    {
                        Console.WriteLine(e);
                        continue;
                    }

                    // If we didn't get any data, skip
                    if (activityData == null || activityData.Activities == null)
                        continue;

                    // Loop through Activities
                    foreach (var activity in activityData.Activities)
                    {
                        // If we already collected that Nightfall, skip it.
                        if (collection.Find(x => x.NightfallId == activity.ActivityDetails.InstanceId).Any())
                            continue;

                        // If the Activity is before the start of the ranking period (usually a Season), skip the Score.
                        var activityDate = activity.Period;
                        if (activityDate <= currentSeasonStart)
                            continue;

                        // API might return aborted Nightfalls, so their Score is 0. We want to ignore those.
                        var nightfallScore = activity.Values["teamScore"].Basic.Value;
                        if (nightfallScore <= 0)
                            continue;

                        // Get the Director Hash required for finding the Nightfall Name
                        uint directorHash = activity.ActivityDetails.DirectorActivityHash;

                        // Get the Nightfall Name
                        string activityName = GetNameFromHash(directorHash);
                        if (activityName.Equals("INVALID", StringComparison.InvariantCulture))
                            continue;

                        // Create new Score Entry
                        ScoreEntry entry = new ScoreEntry(
                            name: clanMember.DestinyUserInfo.DisplayName,
                            accountId: membershipId,
                            directorActivityHash: activity.ActivityDetails.DirectorActivityHash.ToString(),
                            nightfallId: activity.ActivityDetails.InstanceId,
                            activityName: activityName,
                            activityDate: activityDate,
                            score: nightfallScore);

                        // Insert into Collection
                        collection.InsertOne(entry);
                        WriteLog(LogSeverity.Info, $"Added Score {nightfallScore} for Player {clanMember.DestinyUserInfo.DisplayName} from {activityDate} in {activityName}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets all Scores from the Collection, Sorts them Descending, Groups them by Player Name and returns topX amount of them.
        /// </summary>
        /// <param name="topX">Amount of scores to return.</param>
        /// <returns></returns>
        public List<ScoreEntry> GetTopScores(int topX)
        {
            // Get Current Season scores, sorted by Score, ascending
            var mongoResult = collection.Find(x => x.ActivityDate >= currentSeasonStart).ToList();
            
            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        /// <summary>
        /// Gets the topX scores of the given location and difficulty.
        /// </summary>
        /// <param name="location">Nightfall Name</param>
        /// <param name="topX">Amount of scores to return</param>
        /// <param name="difficulty">(Optional) Difficulty / Ordeal to for the Nightfall.</param>
        /// <returns></returns>
        public List<ScoreEntry> GetTopScoresForLocation(string location, int topX, string difficulty = "NONE")
        {
            // Add Ordeal Suffix if a valid difficulty is given
            string locationQueryString = String.Empty;
            if(OrdealDifficulties.ValidDifficulties.Contains(difficulty))
                locationQueryString = $"{location} (Ordeal - {difficulty})";
            else
                locationQueryString = location;

            // Find all Scores for the given Location, sorted by Score, ascending
            List<ScoreEntry> mongoResult = collection.Find(x => x.ActivityName.Equals(locationQueryString)).ToList();

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        /// <summary>
        /// Gets the Scores for a given playerName for an optional location
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="max"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        internal List<ScoreEntry> GetPlayerScoresFromDatabase(string playerName, int topX, string location, string difficulty)
        {
            bool withLocation = !location.Equals("ANY");
            bool withDifficulty = !difficulty.Equals("NONE");

            List<ScoreEntry> mongoResult = new List<ScoreEntry>();
            if (!withLocation)
            {
                // Find all Scores for the given Name
                mongoResult = collection.Find(x => x.Name.Equals(playerName)).ToList();
            }
            else
            {
                if(!withDifficulty)
                    mongoResult = collection.Find(x => x.Name.Equals(playerName) && x.ActivityName.Equals(location)).ToList();
                else
                {
                    // Add Ordeal Suffix if a valid difficulty is given
                    string locationQueryString = String.Empty;
                    if (OrdealDifficulties.ValidDifficulties.Contains(difficulty))
                        locationQueryString = $"{location} (Ordeal - {difficulty})";
                    else
                        locationQueryString = location;
                    mongoResult = collection.Find(x => x.Name.Equals(playerName) && x.ActivityName.Equals(locationQueryString)).ToList();
                }
            }

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.DirectorActivityHash).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        /// <summary>
        /// Converts the given unsigned int hash into a signed int hash and looks up the Activity name in the manifest Database.
        /// Returns "Unknown" if no entry in the Manifest could be found for the given Hash
        /// </summary>
        /// <param name="hash">Activity Hash</param>
        /// <returns></returns>
        public string GetNameFromHash(uint hash)
        {
            // Convert unsigned Hash into signed Hash
            int signedHash = (int)hash;

            // Prepare SQL Query
            SQLiteCommand query = new SQLiteCommand(sqlConnection)
            {
                CommandText = $"SELECT json FROM DestinyActivityDefinition WHERE ID = {signedHash}"
            };

            // Read result object into string variable
            String jsonString = String.Empty;
            var sqliteDataReader = query.ExecuteReader();
            while (sqliteDataReader.Read())
            {
                byte[] jsonData = (byte[])sqliteDataReader["json"];
                jsonString = Encoding.Default.GetString(jsonData);
            }

            // Dispose Command
            query.Dispose();

            // If no JSON was found, the activity ID doesn't exist in Manifest. Return "Unknown"
            if (String.IsNullOrEmpty(jsonString))
                return "INVALID";

            // Bungies naming scheme got inconsistent with Ordeals, so we have to do some string manipulation here to get proper results
            DestinyActivityDefinition dad = DestinyActivityDefinition.FromJson(jsonString);
            if (dad.DisplayProperties.Name.Contains("QUEST", StringComparison.InvariantCulture))
                return "INVALID";

            if (dad.DisplayProperties.Name.Contains("Nightfall: The Ordeal", StringComparison.InvariantCulture))
                return $"{dad.DisplayProperties.Description} (Ordeal - {dad.DisplayProperties.Name.Split("Ordeal: ")[1].ToUpper(CultureInfo.InvariantCulture)})";
            else
                return dad.DisplayProperties.Name.Split("Nightfall: ")[1];
        }

        /// <summary>
        /// Wrapper for Log
        /// </summary>
        /// <param name="logSeverity">Severity of Message</param>
        /// <param name="message">Message Text</param>
        private void WriteLog(LogSeverity logSeverity, string message)
        {
            Log(new LogMessage(logSeverity, "Destiny", message));
        }
    }
}

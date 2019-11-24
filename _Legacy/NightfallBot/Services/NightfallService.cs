using BungieNet.Api;
using MongoDB.Driver;
using XurBot.Models;
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
using BungieNet.Destiny.HistoricalStats.Definitions;
using BungieNet.Destiny.Definitions;
using XurBot.Models.NDestinyInventoryItemDefinition;
using BungieNet.Destiny;
using XurBot.Models.Destiny.NDestinyActivityDefinition;
using XurBot.Models.Destiny;
using XurBot.Extensions;
using System.Collections.Concurrent;

namespace XurBot.Services
{
    public class NightfallService
    {
        private readonly BungieClient bungieClient;
        private SQLiteConnection sqlConnection;
        private MongoClient mongoClient;

        private IMongoCollection<ScoreEntry> nflCollection;
        private IMongoCollection<NDestinyHistoricalStatsPeriodGroup> activityCollection;
        private IMongoCollection<ChallengeEntry> challengeCollection;

        public event Func<LogMessage, Task> Log;

        public NightfallService(IServiceProvider services)
        {
            // Create new BungieClient
            bungieClient = new BungieClient(apiKey: new BungieApiKey(Environment.GetEnvironmentVariable("NFLBOT_BUNGIETOKEN")));
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
            var mongoConnectionString = Environment.GetEnvironmentVariable("NFLBOT_MONGOSTRING");
            mongoClient = new MongoClient(mongoConnectionString);
            WriteLog(LogSeverity.Info, "MongoDB Connection established!");

            WriteLog(LogSeverity.Info, "Loading Collection..");
            var database = mongoClient.GetDatabase("d2tools");

            nflCollection = database.GetCollection<ScoreEntry>("nfl");
            activityCollection = database.GetCollection<NDestinyHistoricalStatsPeriodGroup>("memberactivity");
            challengeCollection = database.GetCollection<ChallengeEntry>("confirmedchallenges");

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

            database.DropCollection("memberactivity");
            database.CreateCollection("memberactivity");

            database.DropCollection("confirmedchallenges");
            database.CreateCollection("confirmedchallenges");
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
        public async Task LoadScores(int count = 20, int pageCount = 1)
        {
            WriteLog(LogSeverity.Info, "Loading Scores from Bungie..");
            await GetNightfalls(count, pageCount).ConfigureAwait(true);
            WriteLog(LogSeverity.Info, "Scores from Bungie loaded!");
        }

        /// <summary>
        /// Gets all current activities for all characters for all clan members.
        /// Only gets the latest page of activies for ScoredNightfall activities.
        /// Scores then get inserted into the nflCollection.
        /// </summary>
        /// <returns></returns>
        private async Task GetNightfalls(int countPerPage, int pages)
        {
            // Get Clan ID from Environment Variables
            long clanID = Convert.ToInt64(Environment.GetEnvironmentVariable("NFLBOT_CLANID"), CultureInfo.InvariantCulture);

            WriteLog(LogSeverity.Info, "Loading Clan Information..", "Nightfalls");
            // Get Members of Clan from Bungie
            SearchResultOfGroupMember clanResult = await bungieClient.GroupV2.GetMembersOfGroupAsync(
                groupId: clanID,
                currentpage: 0,
                memberType: RuntimeGroupMemberType.Member,
                nameSearch: string.Empty).ConfigureAwait(false);
            WriteLog(LogSeverity.Info, $"Clan Information loaded! {clanResult.Results.Length} members found.", "Nightfalls");

            WriteLog(LogSeverity.Info, $"Saving Membership IDs..", "Nightfalls");
            List<long> clanMembershipIds = new List<long>();
            foreach (var clanMember in clanResult.Results)
            {
                // Get their Membership ID required for getting their Profile & Activity Information
                clanMembershipIds.Add(clanMember.DestinyUserInfo.MembershipId);
            }

            WriteLog(LogSeverity.Info, $"Saving Membership IDs done!", "Nightfalls");
            ConcurrentBag<ScoreEntry> addingList = new ConcurrentBag<ScoreEntry>();

            // Loop through Members
            WriteLog(LogSeverity.Info, $"Requesting Clan Activity..", "Nightfalls");
            await AsyncExtensions.ForEachAsync(clanResult.Results, 10, async clanMember =>
            {
                // Get their Membership ID required for getting their Profile & Activity Information
                long membershipId = clanMember.DestinyUserInfo.MembershipId;

                // Get their Profile Information
                DestinyProfileResponse memberdata = await bungieClient.Destiny2.GetProfileAsync(
                    membershipType: clanMember.DestinyUserInfo.MembershipType,
                    destinyMembershipId: membershipId,
                    components: new DestinyComponentType[] { DestinyComponentType.Characters }).ConfigureAwait(false);

                // Skip if no data was recieved. Usually only when Bungie has Server issues.
                if (memberdata == null)
                    return;

                WriteLog(LogSeverity.Debug, $"Found {memberdata.Characters.Data.Count} characters for {clanMember.DestinyUserInfo.DisplayName}");

                // Loop through Characters of Member
                await AsyncExtensions.ForEachAsync(memberdata.Characters.Data, 3, async playerCharacter =>
                {
                    // Hold Character ID in seperate Variable for convienence
                    long characterId = playerCharacter.Key;

                    var nightfalls = await GetNightfallsOfClanMember(clanMember, clanMembershipIds);
                    nightfalls.AsParallel().ForAll(x => addingList.Add(x));
                });
            });
            WriteLog(LogSeverity.Info, $"Requesting Clan Activity done!");

            WriteLog(LogSeverity.Info, $"Inserting {addingList.Count} Documents into Collection..");

            if(addingList.Count > 0)
                await nflCollection.InsertManyAsync(addingList);

            WriteLog(LogSeverity.Info, $"Inserting Documents into Collection done!");
        }

        private async Task<List<ScoreEntry>> GetNightfallsOfClanMember(GroupMember clanMember, List<long> clanMembershipIds)
        {
            List<ScoreEntry> returnList = new List<ScoreEntry>();

            // Get Nightfall Activity of Character
            var membershipId = clanMember.DestinyUserInfo.MembershipId;
            var characterActivityResult = await activityCollection.FindAsync(
                x => x.OriginMembershipId == membershipId
                && (x.Data.ActivityDetails.Mode == DestinyActivityModeType.Nightfall || x.Data.ActivityDetails.Mode == DestinyActivityModeType.HeroicNightfall) 
                && x.Data.Period >= Globals.CurrentSeasonStart);
            
            // If we didn't get any data, skip
            if (characterActivityResult == null || characterActivityResult.ToList().Count == 0)
                return returnList;

            // Loop through Activities
            await AsyncExtensions.ForEachAsync(characterActivityResult.ToList(), 10, async activity =>
            {
                // If we already collected that Nightfall, skip it.
                if (nflCollection.Find(x => x.NightfallId == activity.Data.ActivityDetails.InstanceId && x.AccountId == membershipId).Any())
                    return;

                // If the Activity is before the start of the ranking period (usually a Season), skip the Score.
                var activityDate = activity.Data.Period;
                if (activityDate <= Globals.CurrentSeasonStart)
                    return;

                // API might return aborted Nightfalls, so their Score is 0. We want to ignore those.
                var nightfallScore = activity.Data.Values["teamScore"].Basic.Value;
                if (nightfallScore <= 0)
                    return;

                // Get Post Game Carnage Report
                var pcgr = await bungieClient.Destiny2.GetPostGameCarnageReportAsync(activity.Data.ActivityDetails.InstanceId);
                bool validRun = true;

                // Check if all Players in the Fireteam are in the Clan
                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null)
                        continue;

                    var playerMembershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!clanMembershipIds.Contains(playerMembershipId))
                        validRun = false;
                }

                // If not, it's invalid
                if (!validRun)
                    return;

                // Get the Director Hash required for finding the Nightfall Name
                string directorHash = activity.Data.ActivityDetails.DirectorActivityHash;

                // Get the Nightfall Name
                string activityName = GetNameFromHash(directorHash);
                if (activityName.Equals("INVALID"))
                    return;

                // Create new Score Entry
                ScoreEntry entry = new ScoreEntry(
                    name: clanMember.DestinyUserInfo.DisplayName,
                    accountId: membershipId,
                    directorActivityHash: activity.Data.ActivityDetails.DirectorActivityHash,
                    nightfallId: activity.Data.ActivityDetails.InstanceId,
                    activityName: activityName,
                    activityDate: activityDate,
                    score: nightfallScore);

                // Insert into Collection
                WriteLog(LogSeverity.Info, $"Adding Score {nightfallScore} for Player {clanMember.DestinyUserInfo.DisplayName} from {activityDate} in {activityName}");
                returnList.Add(entry);
            });

            return returnList;
        }

        private async Task<List<NDestinyHistoricalStatsPeriodGroup>> GetActivityData(BungieMembershipType membershipType, long membershipId, long characterId, GroupMember clanMember, List<NDestinyHistoricalStatsPeriodGroup> pcgrs)
        {
            List<NDestinyHistoricalStatsPeriodGroup> returnData = new List<NDestinyHistoricalStatsPeriodGroup>();
            int pages = 2;
            for (int i = 0; i < pages; i++)
            {
                // Get Activity History of Character
                // Using a Try-Catch Block as players might opt-in to refuse API calls to their history
                DestinyActivityHistoryResults allData = null;
                try
                {
                    allData = await bungieClient.Destiny2.GetActivityHistoryAsync(
                        membershipType: membershipType,
                        destinyMembershipId: membershipId,
                        characterId: characterId,
                        count: 30,
                        mode: DestinyActivityModeType.None,
                        page: i).ConfigureAwait(false);
                }
                catch (BungieException e) // Most likely to be privacy related exceptions. For now we just ignore those scores.
                {
                    Console.WriteLine($"{membershipId} - {e}");
                    continue;
                }

                // If we didn't get any data, skip
                if (allData != null && allData.Activities != null)
                {
                    // Loop through Activities
                    foreach (var activity in allData.Activities)
                    {
                        // If the Activity is before the start of the ranking period (usually a Season), skip the Score.
                        var activityDate = activity.Period;
                        if (activityDate < Globals.CurrentChallengeWeek)
                            continue;

                        // If we already collected that PCGR
                        if (pcgrs.Any(x => x.Data.ActivityDetails.InstanceId == activity.ActivityDetails.InstanceId))
                            continue;

                        // Insert into Collection
                        WriteLog(LogSeverity.Info, $"Adding {Enum.GetName(typeof(DestinyActivityModeType), activity.ActivityDetails.Mode)} PCGR {activity.ActivityDetails.InstanceId} from Player {clanMember.DestinyUserInfo.DisplayName} from {activityDate}.");
                        returnData.Add(new NDestinyHistoricalStatsPeriodGroup(activity, membershipId));
                    }
                }

            }

            return returnData;
        }

        /// <summary>
        /// Gets all Scores from the Collection, Sorts them Descending, Groups them by Player Name and returns topX amount of them.
        /// </summary>
        /// <param name="topX">Amount of scores to return.</param>
        /// <returns></returns>
        public List<ScoreEntry> GetTopScores(int topX)
        {
            // Get Current Season scores, sorted by Score, ascending
            var mongoResult = nflCollection.Find(x => x.ActivityDate >= Globals.CurrentSeasonStart).ToList();
            
            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        /// <summary>
        /// Gets all Ordeal Scores from the Collection, Sorts them Descending, Groups them by Player Name and returns topX amount of them.
        /// </summary>
        /// <param name="topX">Amount of scores to return.</param>
        /// <returns></returns>
        public List<ScoreEntry> GetTopOrdealScores(int topX)
        {
            // Get Current Season scores, sorted by Score, ascending
            var mongoResult = nflCollection.Find(x => x.ActivityDate >= Globals.CurrentSeasonStart && x.ActivityName.Contains("(")).ToList();

            // Temp: Check unique scores and remove those that don't appear three times.
            var returnResult = mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score)));
            var singleScores = returnResult.GroupBy(x => x.Score).Where(y => y.Count() == 3).Select(z => z.Key);
            return returnResult.ToList().Where(x => singleScores.Contains(x.Score)).OrderByDescending(x => x.Score).Take(topX).ToList();

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            // return mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score).Take(topX).ToList();
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
                locationQueryString = $"{location} ({difficulty})";
            else
                locationQueryString = location;

            // Find all Scores for the given Location, sorted by Score, ascending
            List<ScoreEntry> mongoResult = nflCollection.Find(x => x.ActivityName.Equals(locationQueryString)).ToList();

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

            List<ScoreEntry> mongoResult;
            if (!withLocation)
            {
                // Find all Scores for the given Name
                mongoResult = nflCollection.Find(x => x.Name.Equals(playerName)).ToList();
            }
            else
            {
                if(!withDifficulty)
                    mongoResult = nflCollection.Find(x => x.Name.Equals(playerName) && x.ActivityName.Equals(location)).ToList();
                else
                {
                    // Add Ordeal Suffix if a valid difficulty is given
                    string locationQueryString = String.Empty;
                    if (OrdealDifficulties.ValidDifficulties.Contains(difficulty))
                        locationQueryString = $"{location} ({difficulty})";
                    else
                        locationQueryString = location;
                    mongoResult = nflCollection.Find(x => x.Name.Equals(playerName) && x.ActivityName.Equals(locationQueryString)).ToList();
                }
            }

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.DirectorActivityHash).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        public int GetPositionOfScore(ScoreEntry score)
        {
            // Get Current Season scores, sorted by Score, ascending
            var mongoResult = nflCollection.Find(x => x.ActivityDate >= Globals.CurrentSeasonStart && x.ActivityName.Contains("(")).ToList();

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            var x = mongoResult.OrderByDescending(x => x.Score).ToList();

            return x.FindIndex(x => x.NightfallId == score.NightfallId);
        }

        /// <summary>
        /// Converts the given unsigned int hash into a signed int hash and looks up the Activity name in the manifest Database.
        /// Returns "Unknown" if no entry in the Manifest could be found for the given Hash
        /// </summary>
        /// <param name="hash">Activity Hash</param>
        /// <returns></returns>
        public string GetNameFromHash(string hash, bool raw = false)
        {
            // Convert unsigned Hash into signed Hash
            int signedHash = (int)uint.Parse(hash);

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
            NDestinyActivityDefinition dad = NDestinyActivityDefinition.FromJson(jsonString);
            if (dad.DisplayProperties.Name.Contains("QUEST", StringComparison.InvariantCulture))
                return "INVALID";

            if (dad.DisplayProperties.Name.Contains("Nightfall: The Ordeal", StringComparison.InvariantCulture))
                return $"{dad.DisplayProperties.Description} ({dad.DisplayProperties.Name.Split("Ordeal: ")[1].ToUpper(CultureInfo.InvariantCulture)})";
            else
                return dad.DisplayProperties.Name.Split("Nightfall: ")[1];
        }

        public TierType GetWeaponQuality(uint hash)
        {
            // Convert unsigned Hash into signed Hash
            int signedHash = (int)hash;

            // Prepare SQL Query
            SQLiteCommand query = new SQLiteCommand(sqlConnection)
            {
                CommandText = $"SELECT json FROM DestinyInventoryItemDefinition WHERE ID = {signedHash}"
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
                return TierType.Unknown;

            NDestinyInventoryItemDefinition itemDefinition = NDestinyInventoryItemDefinition.FromJson(jsonString);
            return (TierType)itemDefinition.Inventory.TierType.Value;
        }

        /// <summary>
        /// Wrapper for Log
        /// </summary>
        /// <param name="logSeverity">Severity of Message</param>
        /// <param name="message">Message Text</param>
        private void WriteLog(LogSeverity logSeverity, string message, string component = "Destiny")
        {
            Log(new LogMessage(logSeverity, component, message));
        }

        #region Challenge Tracking
        public async Task CheckChallengeActivities()
        {
            // Get Clan ID from Environment Variables
            long clanID = Convert.ToInt64(Environment.GetEnvironmentVariable("NFLBOT_CLANID"), CultureInfo.InvariantCulture);

            // Get Members of Clan from Bungie
            SearchResultOfGroupMember clanResult = await bungieClient.GroupV2.GetMembersOfGroupAsync(
                groupId: clanID,
                currentpage: 0,
                memberType: RuntimeGroupMemberType.Member,
                nameSearch: string.Empty).ConfigureAwait(false);

            // Loop through Members
            List<long> clanMembershipIds = new List<long>();
            foreach (var clanMember in clanResult.Results)
            {
                // Get their Membership ID required for getting their Profile & Activity Information
                clanMembershipIds.Add(clanMember.DestinyUserInfo.MembershipId);
            }           

            // Check Challenge 1
            // Get 165 or more final blows on enemies in a single instance of Vex Offensive.
            var completedVexOffensives = await activityCollection.FindAsync
                (
                    x => x.Data.ActivityDetails.Mode.Equals(DestinyActivityModeType.VexOffensive)
                    && x.Data.Values["completionReason"].Basic.DisplayValue.Equals("Objective Completed")
                );

            await AsyncExtensions.ForEachAsync(completedVexOffensives.ToList(), 10, async completedVexOffensive =>
            {
                var pcgr = await bungieClient.Destiny2.GetPostGameCarnageReportAsync(completedVexOffensive.Data.ActivityDetails.InstanceId);

                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null)
                        continue;

                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!clanMembershipIds.Contains(membershipId))
                        continue;

                    bool validRun = playerEntry.Values["kills"].Basic.Value >= 165;
                    if (validRun)
                    {
                        var mongoResult = await challengeCollection.FindAsync(
                            x => x.Challenge.Week == Globals.Tier1.Week
                            && x.Challenge.Tier == Globals.Tier1.Tier
                            && x.AccountId == membershipId);

                        if (!mongoResult.Any())
                        {
                            ChallengeEntry newEntry = new ChallengeEntry(
                                playerEntry.Player.DestinyUserInfo.DisplayName,
                                membershipId,
                                completedVexOffensive.Data.ActivityDetails.InstanceId.ToString(),
                                Globals.Tier1);

                            await challengeCollection.InsertOneAsync(newEntry);
                            WriteLog(LogSeverity.Info, $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {Globals.Tier1.Week}, Tier: {Globals.Tier1.Tier})");
                        }
                    }
                }
            });


            // Check Challenge 2
            // Complete a forge using only the Nameless Midnight Scout Rifle.
            var completedForges = await activityCollection.FindAsync
                (
                    x => x.Data.ActivityDetails.Mode.Equals(DestinyActivityModeType.BlackArmoryRun)
                    && x.Data.Values["completionReason"].Basic.DisplayValue.Equals("Objective Completed")
                );

            await AsyncExtensions.ForEachAsync(completedForges.ToList(), 10, async completedForge =>
            {
                var pcgr = await bungieClient.Destiny2.GetPostGameCarnageReportAsync(completedForge.Data.ActivityDetails.InstanceId);

                var membershipIdsOfPlayers = pcgr.Entries.Select(x => x.Player.DestinyUserInfo.MembershipId).ToList();
                bool allClanMembers = true;
                foreach (var player in membershipIdsOfPlayers)
                {
                    if (!clanMembershipIds.Contains(player))
                        allClanMembers = false;
                }

                // Do not allow Forges where non-clan members were in
                if (!allClanMembers)
                    return;

                foreach (var playerEntry in pcgr.Entries)
                {
                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!clanMembershipIds.Contains(membershipId))
                        continue;

                    bool validRun = true;
                    uint[] validWeaponIds = new uint[] { 2589931339, 2606709051, 1960218487, 3040742682 };

                    foreach (var weaponUsed in playerEntry.Extended.Weapons)
                    {
                        // Nameless Midnight                        
                        if (!validWeaponIds.Contains(weaponUsed.ReferenceId))
                        {
                            validRun = false;
                        }
                    }

                    if (validRun)
                    {
                        var mongoResult = await challengeCollection.FindAsync(
                            x => x.Challenge.Week == Globals.Tier2.Week
                            && x.Challenge.Tier == Globals.Tier2.Tier
                            && x.AccountId == membershipId);

                        if (!mongoResult.Any())
                        {
                            ChallengeEntry newEntry = new ChallengeEntry(
                                playerEntry.Player.DestinyUserInfo.DisplayName,
                                membershipId,
                                completedForge.Data.ActivityDetails.InstanceId.ToString(),
                                Globals.Tier2);

                            await challengeCollection.InsertOneAsync(newEntry);
                            WriteLog(LogSeverity.Info, $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {Globals.Tier2.Week}, Tier: {Globals.Tier2.Tier})");
                        }
                    }
                }
            });

            // Check Challenge 3
            // Complete the Leviathan Raid using Blue Weapons only.
            var completedRaids = await activityCollection.FindAsync
                (
                    x => x.Data.ActivityDetails.Mode.Equals(DestinyActivityModeType.Raid)
                    && x.Data.Values["completionReason"].Basic.DisplayValue.Equals("Objective Completed")
                );

            await AsyncExtensions.ForEachAsync(completedRaids.ToList(), 10, async completedRaid =>
            {
                string[] validRaids = new string[] { "2693136600", "2693136601", "2693136602", "2693136603", "2693136604", "2693136605" };

                if (!validRaids.Contains(completedRaid.Data.ActivityDetails.DirectorActivityHash))
                    return;

                var pcgr = await bungieClient.Destiny2.GetPostGameCarnageReportAsync(completedRaid.Data.ActivityDetails.InstanceId);
                bool validRun = true;

                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null)
                        continue;

                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!clanMembershipIds.Contains(membershipId))
                        continue;

                    foreach (var weaponUsed in playerEntry.Extended.Weapons)
                    {
                        // No Non-Blue Weapons
                        if (GetWeaponQuality(weaponUsed.ReferenceId) != TierType.Rare)
                        {
                            validRun = false;
                        }
                    }
                }

                if (validRun)
                {
                    foreach (var playerEntry in pcgr.Entries)
                    {
                        if (playerEntry.Player.DestinyUserInfo == null)
                            continue;

                        var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                        if (!clanMembershipIds.Contains(membershipId))
                            continue;

                        var mongoResult = await challengeCollection.FindAsync(
                        x => x.Challenge.Week == Globals.Tier3.Week
                        && x.Challenge.Tier == Globals.Tier3.Tier
                        && x.AccountId == membershipId);

                        if (!mongoResult.Any())
                        {
                            ChallengeEntry newEntry = new ChallengeEntry(
                                playerEntry.Player.DestinyUserInfo.DisplayName,
                                membershipId,
                                completedRaid.Data.ActivityDetails.InstanceId.ToString(),
                                Globals.Tier3);

                            await challengeCollection.InsertOneAsync(newEntry);
                            WriteLog(LogSeverity.Info, $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {Globals.Tier3.Week}, Tier: {Globals.Tier3.Tier})");
                        }
                    }
                }
            });
        }

        public async Task GetPCGRs()
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

            List<NDestinyHistoricalStatsPeriodGroup> newActivity = new List<NDestinyHistoricalStatsPeriodGroup>();

            // Get Activity Data
            WriteLog(LogSeverity.Info, "Loading Activity Data..");
            var activityData = await activityCollection.Find(_ => true).ToListAsync();
            WriteLog(LogSeverity.Info, "Loading Activity Data done!");

            await AsyncExtensions.ForEachAsync(clanResult.Results, 10, async clanMember =>
            {
                // Get their Membership ID required for getting their Profile & Activity Information
                long membershipId = clanMember.DestinyUserInfo.MembershipId;

                // Get their Profile Information
                DestinyProfileResponse memberdata = await bungieClient.Destiny2.GetProfileAsync(
                    membershipType: clanMember.DestinyUserInfo.MembershipType,
                    destinyMembershipId: membershipId,
                    components: new DestinyComponentType[] { DestinyComponentType.Characters }).ConfigureAwait(false);

                // Skip if no data was recieved. Usually only when Bungie has Server issues.
                if (memberdata == null)
                    return;

                // Loop through Characters of Member
                await AsyncExtensions.ForEachAsync(memberdata.Characters.Data, 3, async playerCharacter =>
                {
                    // Hold Character ID in seperate Variable for convienence
                    long characterId = playerCharacter.Key;

                    WriteLog(LogSeverity.Info, $"Getting PCGRs of {characterId}");
                    newActivity.AddRange(await GetActivityData(playerCharacter.Value.MembershipType, membershipId, characterId, clanMember, activityData));
                });
            });

            if(newActivity.Count > 0)
                await activityCollection.InsertManyAsync(newActivity);

            WriteLog(LogSeverity.Info, $"New Activity Data from Bungie added!");
        }

        public async Task<bool> GetChallengeCompletionStatus(WeeklyChallengeDatabase challenge, string playerName)
        {
            var mongoResult = await challengeCollection.FindAsync(
                            x => x.Challenge.Week == challenge.Week
                            && x.Challenge.Tier == challenge.Tier
                            && x.Name == playerName);

            return mongoResult.Any();
        }
        #endregion
    }

}

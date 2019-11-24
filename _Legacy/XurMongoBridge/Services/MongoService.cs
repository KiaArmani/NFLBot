using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BungieNet.Destiny.HistoricalStats.Definitions;
using MongoDB.Bson;
using MongoDB.Driver;
using XurClassLibrary.Models;
using XurClassLibrary.Models.Destiny;

namespace XurMongoBridge.Services
{
    public class MongoService
    {
        private IMongoCollection<NDestinyHistoricalStatsPeriodGroup> _activityCollection;
        private IMongoCollection<BlitzMissionEntry> _blitzCollection;

        private IMongoCollection<ChallengeEntry> _challengeCollection;
        private MongoClient _mongoClient;
        private IMongoCollection<ScoreEntry> _nflCollection;

        public MongoService(IServiceProvider services)
        {
        }

        /// <summary>
        ///     Initializes Connection to the Cloud Atlas Database and receives all currently saved scores.
        /// </summary>
        public void InitializeMongoConnection()
        {
            WriteLog(LogSeverity.Info, "Initializing Cloud Atlas Connection..");
            var mongoConnectionString = Environment.GetEnvironmentVariable("NFLBOT_MONGOSTRING");
            _mongoClient = new MongoClient(mongoConnectionString);
            WriteLog(LogSeverity.Info, "MongoDB Connection established!");

            WriteLog(LogSeverity.Info, "Loading Collection..");
            var database = _mongoClient.GetDatabase("d2tools");
            _challengeCollection = database.GetCollection<ChallengeEntry>("confirmedchallenges");
            _activityCollection = database.GetCollection<NDestinyHistoricalStatsPeriodGroup>("memberactivity");
            _nflCollection = database.GetCollection<ScoreEntry>("nfl");
            _blitzCollection = database.GetCollection<BlitzMissionEntry>("blitzmissions");

            WriteLog(LogSeverity.Info, "Collection Loaded!");
        }


        /// <summary>
        ///     Wrapper for Log
        /// </summary>
        /// <param name="logSeverity">Severity of Message</param>
        /// <param name="message">Message Text</param>
        private void WriteLog(LogSeverity logSeverity, string message)
        {
            Console.WriteLine($"{logSeverity}\tMongo\t{message}");
        }

        /// <summary>
        /// </summary>
        /// <param name="membershipId"></param>
        /// <param name="week"></param>
        /// <param name="tier"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool HasCompletedChallenge(long membershipId, long week, long tier, ChallengeDifficulty difficulty)
        {
            var mongoResult = _challengeCollection.Find(
                x => x.Challenge.Week == week
                     && x.Challenge.Tier == tier
                     && x.Challenge.Difficulty == difficulty
                     && x.AccountId == membershipId);

            return mongoResult.Any();
        }

        /// <summary>
        ///     Returns the combined Score of all Clan Members
        /// </summary>
        /// <returns></returns>
        public long GetClanScore()
        {
            long clanScore = 0;
            var mongoResult = _challengeCollection.Find(
                x => true).ToList();
            foreach (var validChallenge in mongoResult)
            {
                var matchingScore =
                    ChallengeGlobals.WeeklyChallengeList.Single(x => x.Tier == validChallenge.Challenge.Tier);
                clanScore += matchingScore.Score;
            }

            return clanScore;
        }

        /// <summary>
        ///     Returns the score of the given Membership ID
        /// </summary>
        /// <param name="membershipId"></param>
        /// <returns></returns>
        public long GetPlayerScore(long membershipId)
        {
            long playerScore = 0;
            var mongoResult = _challengeCollection.Find(
                x => x.AccountId == membershipId).ToList();

            foreach (var validChallenge in mongoResult)
            {
                var matchingScore =
                    ChallengeGlobals.WeeklyChallengeList.Single(x => x.Tier == validChallenge.Challenge.Tier);
                playerScore += matchingScore.Score;
            }

            return playerScore;
        }

        /// <summary>
        ///     Returns the amount of Kills throughout all activities from all members
        /// </summary>
        /// <returns></returns>
        public long GetClanKills()
        {
            var result = _activityCollection.Aggregate()
                .Group(new BsonDocument
                {
                    {"_id", BsonNull.Value},
                    {
                        "totalAmount",
                        new BsonDocument("$sum",
                            new BsonDocument("$toDouble", "$Data.Values.kills.Basic.Value"))
                    }
                }).ToList();

            return Convert.ToInt64(result[0].AsBsonDocument["totalAmount"].RawValue);
        }

        /// <summary>
        ///     Gets all Scores from the Collection, Sorts them Descending, Groups them by Player Name and returns topX amount of
        ///     them.
        /// </summary>
        /// <param name="topX">Amount of scores to return.</param>
        /// <returns></returns>
        public List<ScoreEntry> GetTopScores(int topX)
        {
            // Get Current Season scores, sorted by Score, ascending
            var mongoResult = _nflCollection.Find(x => x.ActivityDate >= ChallengeGlobals.CurrentSeasonStart).ToList();

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score)))
                .OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        /// <summary>
        ///     Gets all Ordeal Scores from the Collection, Sorts them Descending, Groups them by Player Name and returns topX
        ///     amount of them.
        /// </summary>
        /// <param name="topX">Amount of scores to return.</param>
        /// <returns></returns>
        public List<ScoreEntry> GetTopOrdealScores(int topX)
        {
            // Get Current Season scores, sorted by Score, ascending
            var mongoResult = _nflCollection
                .Find(x => x.ActivityDate >= ChallengeGlobals.CurrentSeasonStart && x.ActivityName.Contains("("))
                .ToList();

            // Temp: Check unique scores and remove those that don't appear three times.
            var singleScores = mongoResult.GroupBy(x => x.Score).Where(y => y.Count() == 3).Select(z => z.Key);
            //var returnResult = mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score)));
            return mongoResult.Where(x => singleScores.Contains(x.Score)).OrderByDescending(x => x.Score).Take(topX * 3)
                .ToList();

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            // return mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        /// <summary>
        ///     Gets the topX scores of the given location and difficulty.
        /// </summary>
        /// <param name="location">Nightfall Name</param>
        /// <param name="topX">Amount of scores to return</param>
        /// <param name="difficulty">(Optional) Difficulty / Ordeal to for the Nightfall.</param>
        /// <returns></returns>
        public List<ScoreEntry> GetTopScoresForLocation(string location, int topX, string difficulty = "NONE")
        {
            // Add Ordeal Suffix if a valid difficulty is given
            var locationQueryString = string.Empty;
            locationQueryString = OrdealDifficulties.ValidDifficulties.Contains(difficulty)
                ? $"{location} ({difficulty})"
                : location;

            // Find all Scores for the given Location, sorted by Score, ascending
            var mongoResult = _nflCollection.Find(x => x.ActivityName.Equals(locationQueryString)).ToList();

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.AccountId).SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score)))
                .OrderByDescending(x => x.Score).Take(topX).ToList();
        }

        /// <summary>
        ///     Gets the Scores for a given playerName for an optional location
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="max"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<ScoreEntry> GetPlayerScoresFromDatabase(string playerName, int topX, string location,
            string difficulty)
        {
            var withLocation = !location.Equals("ANY");
            var withDifficulty = !difficulty.Equals("NONE");

            List<ScoreEntry> mongoResult;
            if (!withLocation)
            {
                // Find all Scores for the given Name
                mongoResult = _nflCollection.Find(x => x.Name.Equals(playerName)).ToList();
            }
            else
            {
                if (!withDifficulty)
                {
                    mongoResult = _nflCollection.Find(x => x.Name.Equals(playerName) && x.ActivityName.Equals(location))
                        .ToList();
                }
                else
                {
                    // Add Ordeal Suffix if a valid difficulty is given
                    string locationQueryString;
                    locationQueryString = OrdealDifficulties.ValidDifficulties.Contains(difficulty)
                        ? $"{location} ({difficulty})"
                        : location;
                    mongoResult = _nflCollection
                        .Find(x => x.Name.Equals(playerName) && x.ActivityName.Equals(locationQueryString)).ToList();
                }
            }

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            return mongoResult.GroupBy(x => x.DirectorActivityHash)
                .SelectMany(g => g.Where(p => p.Score == g.Max(h => h.Score))).OrderByDescending(x => x.Score)
                .Take(topX).ToList();
        }

        /// <summary>
        ///     Returns the Position of the given ScoreEntry.
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public int GetPositionOfScore(long nightfallId)
        {
            // Get Current Season scores, sorted by Score, ascending
            var mongoResult = _nflCollection
                .Find(x => x.ActivityDate >= ChallengeGlobals.CurrentSeasonStart && x.ActivityName.Contains("("))
                .ToList();

            // Group by Name, take only the highest score of each player, then only return a List of topX scores.
            var x = mongoResult.OrderByDescending(x => x.Score).ToList();

            return x.FindIndex(x => x.NightfallId == nightfallId);
        }

        /// <summary>
        ///     Returns the Activity of the given ID
        /// </summary>
        /// <param name="instanceId"></param>
        /// <returns></returns>
        public NDestinyHistoricalStatsPeriodGroup GetActivityByInstanceId(long instanceId)
        {
            var mongodbResult = _activityCollection.Find(x => x.Data.ActivityDetails.InstanceId == instanceId);
            if (mongodbResult.Any())
                return mongodbResult.First();
            return null;
        }

        /// <summary>
        ///     Returns all Activities of the given type that are marked as completed
        /// </summary>
        /// <param name="modeType"></param>
        /// <returns></returns>
        public List<NDestinyHistoricalStatsPeriodGroup> GetCompletedActivitiesOfType(DestinyActivityModeType modeType)
        {
            var completedActivities = _activityCollection.Find
            (
                x => x.Data.ActivityDetails.Mode.Equals(modeType)
                     && x.Data.Values["completionReason"].Basic.DisplayValue.Equals("Objective Completed")
                     && x.Data.Period >= ChallengeGlobals.CurrentChallengeWeek
            );

            return completedActivities.ToList();
        }

        /// <summary>
        ///     Returns whether or not a challenge has been done already
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="week"></param>
        /// <param name="tier"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public bool HasCompletedChallenge(string memberName, long week, long tier, ChallengeDifficulty difficulty)
        {
            var mongoResult = _challengeCollection.Find(
                x => x.Challenge.Week == week
                     && x.Challenge.Tier == tier
                     && x.Challenge.Difficulty == difficulty
                     && x.Name == memberName);

            return mongoResult.Any();
        }

        /// <summary>
        ///     Returns all Nightfall and HeroicNightfall Activities for a given Membership ID
        /// </summary>
        /// <param name="membershipId"></param>
        /// <returns></returns>
        public async Task<List<NDestinyHistoricalStatsPeriodGroup>> GetNightfallActivityOfMembershipId(
            long membershipId)
        {
            var characterActivityResult = await _activityCollection.FindAsync(
                x => x.OriginMembershipId == membershipId
                     && (x.Data.ActivityDetails.Mode == DestinyActivityModeType.Nightfall ||
                         x.Data.ActivityDetails.Mode == DestinyActivityModeType.HeroicNightfall)
                     && x.Data.Period >= ChallengeGlobals.CurrentSeasonStart);

            return characterActivityResult.ToList();
        }

        /// <summary>
        ///     Checks if a given Instance ID for a member already exists
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="membershipId"></param>
        /// <returns></returns>
        public async Task<bool> CheckIfPlayerNightfallExistsInDatabase(long instanceId, long membershipId)
        {
            var mongoResult =
                await _nflCollection.FindAsync(x => x.NightfallId == instanceId && x.AccountId == membershipId);
            return await mongoResult.AnyAsync();
        }

        /// <summary>
        ///     Checks if a given Player has completed the current blitz mission
        /// </summary>
        /// <param name="membershipId"></param>
        /// <param name="missionStartTime"></param>
        /// <param name="mission"></param>
        /// <returns></returns>
        public bool HasCompletedBlitzMission(long membershipId, DateTime missionStartTime, BlitzMission mission)
        {
            var mongoResult = _blitzCollection.Find(
                x => x.MissionStartTime == missionStartTime
                     && x.AccountId == membershipId
                     && x.Challenge.ModeType == mission.Metadata.ModeType
                     && x.Challenge.ValueField == mission.Metadata.ValueField
                     && x.Challenge.Value == mission.Metadata.Value);
            return mongoResult.Any();
        }
    }
}
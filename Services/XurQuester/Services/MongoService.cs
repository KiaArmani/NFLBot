using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BungieNet.Destiny.HistoricalStats;
using BungieNet.Destiny.HistoricalStats.Definitions;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using XurClassLibrary.Models;

namespace XurQuester.Services
{
    public class MongoService
    {
        private readonly ILogger<MongoService> _logger;
        private IMongoCollection<DestinyHistoricalStatsPeriodGroup> _activityCollection;
        private IMongoCollection<ChallengeEntry> _challengeCollection;

        public MongoService(ILogger<MongoService> logger, IServiceProvider services)
        {
            _logger = logger;
            InitializeMongoDatabase();
        }

        /// <summary>
        ///     Initializes Connection to the Cloud Atlas Database and receives all currently saved scores.
        /// </summary>
        private void InitializeMongoDatabase()
        {
            _logger.LogInformation("Initializing Cloud Atlas Connection..");
            var mongoConnectionString = Environment.GetEnvironmentVariable("XUR_QUESTER_MONGOSTRING");
            var mongoClient = new MongoClient(mongoConnectionString);
            _logger.LogInformation("MongoDB Connection established!");

            _logger.LogInformation("Loading Collection..");
            var database = mongoClient.GetDatabase("d2tools");

            _activityCollection = database.GetCollection<DestinyHistoricalStatsPeriodGroup>("memberactivity");
            _challengeCollection = database.GetCollection<ChallengeEntry>("confirmedchallenges");

            _logger.LogInformation("Collection Loaded!");
        }

        public List<DestinyHistoricalStatsPeriodGroup> GetCompletedActivitiesOfType(DestinyActivityModeType modeType)
        {
            var completedActivities = _activityCollection.Find
            (
                x => x.ActivityDetails.Mode.Equals(modeType)
                     && x.Values["completionReason"].Basic.DisplayValue.Equals("Objective Completed")
                     && x.Period >= ChallengeGlobals.CurrentChallengeWeek
            );

            return completedActivities.ToList();
        }

        public bool HasCompletedChallenge(long membershipId, long week, long tier, ChallengeDifficulty difficulty)
        {
            var mongoResult = _challengeCollection.Find(
                x => x.Challenge.Week == week
                     && x.Challenge.Tier == tier
                     && x.Challenge.Difficulty == difficulty
                     && x.AccountId == membershipId);
            return mongoResult.Any();
        }

        public async Task AddFinishedChallenge(ChallengeEntry newEntry)
        {
            await _challengeCollection.InsertOneAsync(newEntry);
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
    }
}
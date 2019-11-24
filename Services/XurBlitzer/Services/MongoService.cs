using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BungieNet.Destiny.HistoricalStats.Definitions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using XurClassLibrary.Models;
using XurClassLibrary.Models.Destiny;

namespace XurBlitzer.Services
{
    public class MongoService
    {
        private readonly ILogger<MongoService> _logger;
        private IMongoCollection<NDestinyHistoricalStatsPeriodGroup> _activityCollection;
        private IMongoCollection<BlitzMissionEntry> _blitzCollection;

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
            var mongoConnectionString = Environment.GetEnvironmentVariable("XUR_BLITZER_MONGOSTRING");
            var mongoClient = new MongoClient(mongoConnectionString);
            _logger.LogInformation("MongoDB Connection established!");

            _logger.LogInformation("Loading Collection..");
            var database = mongoClient.GetDatabase("d2tools");

            _activityCollection = database.GetCollection<NDestinyHistoricalStatsPeriodGroup>("memberactivity");
            _blitzCollection = database.GetCollection<BlitzMissionEntry>("blitzmissions");

            _logger.LogInformation("Collection Loaded!");
        }

        public List<NDestinyHistoricalStatsPeriodGroup> GetBlitzActivitiesOfType(DestinyActivityModeType modeType,
            DateTime startTime, DateTime endTime)
        {
            var completedActivities = _activityCollection.Find
            (
                x => x.Data.ActivityDetails.Mode.Equals(modeType)
                     && x.Data.Period >= startTime
                     && x.Data.Period <= endTime
            );

            return completedActivities.ToList();
        }

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

        public async Task AddFinishedBlitzMission(BlitzMissionEntry newEntry)
        {
            await _blitzCollection.InsertOneAsync(newEntry);
        }
    }
}
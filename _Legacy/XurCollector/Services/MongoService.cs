using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BungieNet.Destiny.HistoricalStats.Definitions;
using Discord;
using MongoDB.Driver;
using XurClassLibrary.Models;
using XurClassLibrary.Models.Destiny;
using LogSeverity = Discord.LogSeverity;

namespace XurCollector.Services
{
    public class MongoService
    {
        private IMongoCollection<NDestinyHistoricalStatsPeriodGroup> _activityCollection;
        private MongoClient _mongoClient;

        public MongoService(IServiceProvider services)
        {
        }

        public event Func<LogMessage, Task> Log;

        public void Initialize()
        {
            InitializeMongoDatabase();
        }

        /// <summary>
        ///     Initializes Connection to the Cloud Atlas Database and receives all currently saved scores.
        /// </summary>
        private void InitializeMongoDatabase()
        {
            WriteLog(LogSeverity.Info, "Initializing Cloud Atlas Connection..");
            var mongoConnectionString = Environment.GetEnvironmentVariable("XUR_COLLECTOR_MONGOSTRING");
            _mongoClient = new MongoClient(mongoConnectionString);
            WriteLog(LogSeverity.Info, "MongoDB Connection established!");

            WriteLog(LogSeverity.Info, "Loading Collection..");
            var database = _mongoClient.GetDatabase("d2tools");
            _activityCollection = database.GetCollection<NDestinyHistoricalStatsPeriodGroup>("memberactivity");

            WriteLog(LogSeverity.Info, "Collection Loaded!");
        }

        /// <summary>
        ///     Wrapper for Log
        /// </summary>
        /// <param name="Discord.LogSeverity">Severity of Message</param>
        /// <param name="message">Message Text</param>
        private void WriteLog(LogSeverity logSeverity, string message)
        {
            Log?.Invoke(new LogMessage(logSeverity, "MongoService", message));
        }

        public NDestinyHistoricalStatsPeriodGroup GetActivityByInstanceId(long instanceId)
        {
            var mongodbResult = _activityCollection.Find(x => x.Data.ActivityDetails.InstanceId == instanceId);
            if (mongodbResult.Any())
                return mongodbResult.First();
            return null;
        }

        public List<NDestinyHistoricalStatsPeriodGroup> GetAllActivities()
        {
            return _activityCollection.Find(x => true).ToList();
        }

        public async Task AddNewActivities(List<NDestinyHistoricalStatsPeriodGroup> newActivityData)
        {
            await _activityCollection.InsertManyAsync(newActivityData);
        }
    }
}
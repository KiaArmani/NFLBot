using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using XurClassLibrary.Models.Destiny;

namespace XurCollector.Services
{
    public class MongoService
    {
        private readonly ILogger<MongoService> _logger;
        private IMongoCollection<NDestinyHistoricalStatsPeriodGroup> _activityCollection;
        private MongoClient _mongoClient;

        public MongoService(ILogger<MongoService> logger, IServiceProvider services)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Initializes Connection to the Cloud Atlas Database and receives all currently saved scores.
        /// </summary>
        public void InitializeMongoDatabase()
        {
            _logger.LogInformation("Initializing Cloud Atlas Connection..");
            var mongoConnectionString = Environment.GetEnvironmentVariable("XUR_COLLECTOR_MONGOSTRING");
            _mongoClient = new MongoClient(mongoConnectionString);
            _logger.LogInformation("MongoDB Connection established!");

            _logger.LogInformation("Loading Collection..");
            var database = _mongoClient.GetDatabase("d2tools");
            _activityCollection = database.GetCollection<NDestinyHistoricalStatsPeriodGroup>("memberactivity");

            _logger.LogInformation("Collection Loaded!");
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
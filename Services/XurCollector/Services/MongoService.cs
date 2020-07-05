using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using XurClassLibrary.Extensions;
using XurClassLibrary.Models.Destiny;

namespace XurCollector.Services
{
    public static class MongoService
    {
        private static IMongoCollection<NDestinyHistoricalStatsPeriodGroup> _activityCollection;
        private static MongoClient _mongoClient;

        /// <summary>
        ///     Initializes Connection to the Cloud Atlas Database and receives all currently saved scores.
        /// </summary>
        public static void InitializeMongoDatabase()
        {
            var alwaysAllowUInt32OverflowConvention = new AlwaysAllowUInt32OverflowConventionExtension();
            var conventionPack = new ConventionPack
            {
                alwaysAllowUInt32OverflowConvention
            };
            ConventionRegistry.Register("AlwaysAllowUInt32Overflow", conventionPack, t => true);

            Console.WriteLine("Initializing Cloud Atlas Connection..");
            var mongoConnectionString = Environment.GetEnvironmentVariable("XUR_COLLECTOR_MONGOSTRING");
            _mongoClient = new MongoClient(mongoConnectionString);
            Console.WriteLine("MongoDB Connection established!");

            Console.WriteLine("Loading Collection..");
            var database = _mongoClient.GetDatabase("xur");
            _activityCollection = database.GetCollection<NDestinyHistoricalStatsPeriodGroup>("memberactivity");

            Console.WriteLine("Collection Loaded!");
        }

        public static NDestinyHistoricalStatsPeriodGroup GetActivityByInstanceId(long instanceId)
        {
            var mongodbResult = _activityCollection.Find(x => x.Data.ActivityDetails.InstanceId == instanceId);
            if (mongodbResult.Any())
                return mongodbResult.First();
            return null;
        }

        public static List<NDestinyHistoricalStatsPeriodGroup> GetAllActivities()
        {
            return _activityCollection.Find(x => true).ToList();
        }

        public static async Task AddNewActivities(List<NDestinyHistoricalStatsPeriodGroup> newActivityData)
        {
            await _activityCollection.InsertManyAsync(newActivityData);
        }
    }
}
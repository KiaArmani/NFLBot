using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using XurClassLibrary.Models;
using XurClassLibrary.Models.Destiny;

namespace XurNightfaller.Services
{
    public class MongoService
    {
        private readonly ILogger<MongoService> _logger;
        private IMongoCollection<NDestinyHistoricalStatsPeriodGroup> _activityCollection;

        private IMongoCollection<ScoreEntry> _nflCollection;

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
            var mongoConnectionString = Environment.GetEnvironmentVariable("XUR_NIGHTFALLER_MONGOSTRING");
            var mongoClient = new MongoClient(mongoConnectionString);
            _logger.LogInformation("MongoDB Connection established!");

            _logger.LogInformation("Loading Collection..");
            var database = mongoClient.GetDatabase("d2tools");

            _nflCollection = database.GetCollection<ScoreEntry>("nfl");
            _activityCollection = database.GetCollection<NDestinyHistoricalStatsPeriodGroup>("memberactivity");

            _logger.LogInformation("Collection Loaded!");
        }

        public NDestinyHistoricalStatsPeriodGroup GetActivityByInstanceId(long instanceId)
        {
            var mongodbResult = _activityCollection.Find(x => x.Data.ActivityDetails.InstanceId == instanceId);
            return mongodbResult.Any() ? mongodbResult.First() : null;
        }

        public async Task<List<NDestinyHistoricalStatsPeriodGroup>> GetNightfallActivities()
        {
            // Get Current Season scores, sorted by Score, ascending
            var validActivityIds = new List<string>
            {
                "102545131", "282531137", "694558778", "966580527", "997759433", "1114928259", "1173782160",
                "1193451437", "1198226683",
                "1244305605", "1272746497",
                "1390900084", "1822476598", "1940967975", "2021103427", "2357524344", "2359276231", "2380555126",
                "3094633658",
                "3392133546", "3407296811", "3815447166", "4044386747", "4196546910"
            };

            var characterActivityResult = await _activityCollection.FindAsync(
                x => validActivityIds.Contains(x.Data.ActivityDetails.DirectorActivityHash)
                     && x.Data.Period >= ChallengeGlobals.CurrentSeasonStart);

            return characterActivityResult.ToList();
        }

        public async Task<bool> CheckIfPlayerNightfallExistsInDatabase(long instanceId, long membershipId)
        {
            var mongoResult =
                await _nflCollection.FindAsync(x => x.NightfallId == instanceId && x.AccountId == membershipId);
            return await mongoResult.AnyAsync();
        }

        public List<NDestinyHistoricalStatsPeriodGroup> GetAllActivities()
        {
            return _activityCollection.Find(x => true).ToList();
        }

        public async Task AddNewActivities(List<NDestinyHistoricalStatsPeriodGroup> newActivityData)
        {
            await _activityCollection.InsertManyAsync(newActivityData);
        }

        public async Task AddNewScores(List<ScoreEntry> newScores)
        {
            await _nflCollection.InsertManyAsync(newScores);
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
    }
}
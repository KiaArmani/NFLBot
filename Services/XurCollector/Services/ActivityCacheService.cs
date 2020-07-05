using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XurClassLibrary.Models.Destiny;

namespace XurCollector.Services
{
    public static class ActivityCacheService
    {
        private static ConcurrentDictionary<long, NDestinyHistoricalStatsPeriodGroup>
            _activityDataConcurrentDictionary =
                new ConcurrentDictionary<long, NDestinyHistoricalStatsPeriodGroup>();

        public static void FillActivityCache()
        {
            Console.WriteLine("Filling Activity Cache, please wait..");
            var allActivities = MongoService.GetAllActivities();
            foreach (var activity in allActivities)
                _activityDataConcurrentDictionary.TryAdd(activity.Data.ActivityDetails.InstanceId, activity);
            Console.WriteLine("Filling Activity Cache completed!");
        }

        public static NDestinyHistoricalStatsPeriodGroup TryGetActivityByInstanceId(long instanceId)
        {
            if (_activityDataConcurrentDictionary.TryGetValue(instanceId,
                out var activityObject))
                return activityObject;

            var result = MongoService.GetActivityByInstanceId(instanceId);
            if (result == null)
                return null;

            _activityDataConcurrentDictionary.TryAdd(result.Data.ActivityDetails.InstanceId, result);
            return result;
        }

        public static void AddActivityData(Tuple<long, NDestinyHistoricalStatsPeriodGroup> importData)
        {
            var (instanceId, activityData) = importData;
            _activityDataConcurrentDictionary.TryAdd(instanceId, activityData);
        }
    }
}
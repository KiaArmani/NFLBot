using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using XurClassLibrary.Models.Destiny;

namespace XurNightfaller.Services
{
    public class ActivityCacheService
    {
        private readonly ConcurrentDictionary<long, NDestinyHistoricalStatsPeriodGroup>
            _activityDataConcurrentDictionary =
                new ConcurrentDictionary<long, NDestinyHistoricalStatsPeriodGroup>();

        private readonly MongoService _mongoService;

        public ActivityCacheService(IServiceProvider services)
        {
            _mongoService = services.GetRequiredService<MongoService>();
            FillActivityCache();
        }

        public void FillActivityCache()
        {
            var allActivities = _mongoService.GetAllActivities();
            foreach (var activity in allActivities)
                _activityDataConcurrentDictionary.TryAdd(activity.Data.ActivityDetails.InstanceId, activity);
        }

        public NDestinyHistoricalStatsPeriodGroup TryGetActivityByInstanceId(long instanceId)
        {
            if (_activityDataConcurrentDictionary.TryGetValue(instanceId,
                out var activityObject))
                return activityObject;

            var result = _mongoService.GetActivityByInstanceId(instanceId);
            if (result == null)
                return null;

            _activityDataConcurrentDictionary.TryAdd(result.Data.ActivityDetails.InstanceId, result);
            return result;
        }

        public void AddActivityData(Tuple<long, NDestinyHistoricalStatsPeriodGroup> importData)
        {
            var (instanceId, activityData) = importData;
            _activityDataConcurrentDictionary.TryAdd(instanceId, activityData);
        }
    }
}
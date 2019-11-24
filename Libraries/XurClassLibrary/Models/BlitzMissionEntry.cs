using System;

namespace XurClassLibrary.Models
{
    public class BlitzMissionEntry
    {
        public BlitzMissionEntry(string name, long accountId, string instanceId, BlitzMissionDatabase challenge,
            DateTime missionStartTime)
        {
            _id = Guid.NewGuid().ToString();
            Name = name;
            AccountId = accountId;
            InstanceId = instanceId;
            Challenge = challenge;
            MissionStartTime = missionStartTime;
        }

        public string _id { get; }
        public string Name { get; set; }
        public long AccountId { get; set; }
        public string InstanceId { get; set; }
        public DateTime MissionStartTime { get; set; }
        public BlitzMissionDatabase Challenge { get; set; }
    }
}
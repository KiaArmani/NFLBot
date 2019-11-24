using System;

namespace XurClassLibrary.Models
{
    public class ChallengeEntry
    {
        public ChallengeEntry(string name, long accountId, string instanceId, WeeklyChallengeDatabase challenge)
        {
            _id = Guid.NewGuid().ToString();
            Name = name;
            AccountId = accountId;
            InstanceId = instanceId;
            Challenge = challenge;
        }

        public string _id { get; }
        public string Name { get; set; }
        public long AccountId { get; set; }
        public string InstanceId { get; set; }
        public WeeklyChallengeDatabase Challenge { get; set; }
    }
}
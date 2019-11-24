using System;

namespace XurClassLibrary.Models
{
    public class ScoreEntry
    {
        public ScoreEntry(string name, long accountId, string directorActivityHash, long nightfallId,
            string activityName, DateTime activityDate, decimal score)
        {
            _id = Guid.NewGuid().ToString();
            Name = name;
            AccountId = accountId;
            DirectorActivityHash = directorActivityHash;
            NightfallId = nightfallId;
            ActivityName = activityName;
            ActivityDate = activityDate;
            Score = score;
        }

        public string _id { get; }
        public string Name { get; set; }
        public long AccountId { get; set; }
        public string DirectorActivityHash { get; set; }
        public long NightfallId { get; set; }
        public string ActivityName { get; set; }
        public DateTime ActivityDate { get; set; }
        public decimal Score { get; set; }
    }
}
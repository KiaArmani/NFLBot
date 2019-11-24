using System;

namespace XurClassLibrary.Models
{
    public class WeeklyChallengeDatabase
    {
        public WeeklyChallengeDatabase(long week, long tier, ChallengeDifficulty difficulty, bool isHidden, long score)
        {
            _id = Guid.NewGuid().ToString();
            Week = week;
            Tier = tier;
            Difficulty = difficulty;
            IsHidden = isHidden;
            Score = score;
        }

        public string _id { get; }
        public long Week { get; set; }
        public long Tier { get; set; }
        public ChallengeDifficulty Difficulty { get; set; }
        public bool IsHidden { get; set; }
        public long Score { get; set; }
    }
}
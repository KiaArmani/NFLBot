using System;

namespace XurClassLibrary.Models
{
    public class WeeklyChallenge
    {
        public WeeklyChallenge(string name, string description, WeeklyChallengeDatabase metadata)
        {
            _id = Guid.NewGuid().ToString();
            Name = name;
            Description = description;
            Metadata = metadata;
        }

        public string _id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WeeklyChallengeDatabase Metadata { get; set; }
    }
}
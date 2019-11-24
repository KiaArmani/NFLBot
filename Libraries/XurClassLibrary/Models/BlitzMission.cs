using System;
using System.Globalization;

namespace XurClassLibrary.Models
{
    public class BlitzMission
    {
        public BlitzMission(string name, string description, BlitzMissionDatabase metadata)
        {
            _id = Guid.NewGuid().ToString();
            Name = name;
            Description = description;
            Metadata = metadata;
        }

        public string _id { get; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string DescriptionText => Description.Replace("%AMOUNT%", Metadata.Value.ToString(CultureInfo.InvariantCulture));
        public BlitzMissionDatabase Metadata { get; set; }
    }
}
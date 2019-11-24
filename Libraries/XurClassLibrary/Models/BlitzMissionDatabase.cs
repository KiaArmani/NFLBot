using System;
using BungieNet.Destiny.HistoricalStats.Definitions;

namespace XurClassLibrary.Models
{
    public class BlitzMissionDatabase
    {
        public BlitzMissionDatabase(DestinyActivityModeType modeType, string valueField, string valueFieldType,
            int[] range, bool isHidden, long score)
        {
            _id = Guid.NewGuid().ToString();
            ModeType = modeType;
            ValueField = valueField;
            ValueFieldType = valueFieldType;
            Range = range;
            IsHidden = isHidden;
            Score = score;
        }

        public string _id { get; }
        public DestinyActivityModeType ModeType { get; set; }
        public string ValueFieldType { get; set; }
        public string ValueField { get; set; }
        public decimal Value { get; set; }
        public int[] Range { get; set; }
        public bool IsHidden { get; set; }
        public long Score { get; set; }
    }
}
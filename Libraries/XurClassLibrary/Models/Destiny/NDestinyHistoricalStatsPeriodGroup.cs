using BungieNet.Destiny.HistoricalStats;
using System;

namespace XurClassLibrary.Models.Destiny
{
    public class NDestinyHistoricalStatsPeriodGroup
    {
        public NDestinyHistoricalStatsPeriodGroup(DestinyHistoricalStatsPeriodGroup data, long originMembershipId, string playerName)
        {
            _id = Guid.NewGuid().ToString();
            Data = data;
            OriginMembershipId = originMembershipId;
            PlayerName = playerName;
        }

        public string _id { get; }
        public DestinyHistoricalStatsPeriodGroup Data { get; set; }
        public long OriginMembershipId { get; set; }
        public string PlayerName { get; set; }
    }
}
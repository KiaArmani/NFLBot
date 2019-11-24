using System;
using BungieNet.Destiny.HistoricalStats;

namespace XurClassLibrary.Models.Destiny
{
    public class NDestinyHistoricalStatsPeriodGroup
    {
        public NDestinyHistoricalStatsPeriodGroup(DestinyHistoricalStatsPeriodGroup data, long originMembershipId)
        {
            _id = Guid.NewGuid().ToString();
            Data = data;
            OriginMembershipId = originMembershipId;
        }

        public string _id { get; }
        public DestinyHistoricalStatsPeriodGroup Data { get; set; }
        public long OriginMembershipId { get; set; }
    }
}
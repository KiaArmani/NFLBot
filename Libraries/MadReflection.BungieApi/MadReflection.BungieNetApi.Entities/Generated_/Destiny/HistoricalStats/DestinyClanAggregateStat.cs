﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace BungieNet.Destiny.HistoricalStats
{
	public partial class DestinyClanAggregateStat
	{
		[JsonProperty("mode")]
		public Destiny.HistoricalStats.Definitions.DestinyActivityModeType Mode { get; set; }

		[JsonProperty("statId")]
		public string StatId { get; set; }

		[JsonProperty("value")]
		public Destiny.HistoricalStats.DestinyHistoricalStatsValue Value { get; set; }
	}
}

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

namespace BungieNet.Destiny.Definitions
{
	public partial class DestinyProgressionRewardItemQuantity
	{
		[JsonProperty("rewardedAtProgressionLevel")]
		public int RewardedAtProgressionLevel { get; set; }

		[JsonProperty("acquisitionBehavior")]
		public Destiny.DestinyProgressionRewardItemAcquisitionBehavior AcquisitionBehavior { get; set; }

		[JsonProperty("uiDisplayStyle")]
		public string UIDisplayStyle { get; set; }

		[JsonProperty("claimUnlockDisplayStrings")]
		public string[] ClaimUnlockDisplayStrings { get; set; }

		[JsonProperty("itemHash")]
		public uint ItemHash { get; set; }

		[JsonProperty("itemInstanceId")]
		public long? ItemInstanceId { get; set; }

		[JsonProperty("quantity")]
		public int Quantity { get; set; }
	}
}
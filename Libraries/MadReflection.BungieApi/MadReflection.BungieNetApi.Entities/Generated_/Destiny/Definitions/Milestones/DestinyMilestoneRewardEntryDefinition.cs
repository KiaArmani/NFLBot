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

namespace BungieNet.Destiny.Definitions.Milestones
{
	/// <summary>
	/// The definition of a specific reward, which may be contained in a category of rewards and that has optional information about how it is obtained.
	/// </summary>
	public partial class DestinyMilestoneRewardEntryDefinition
	{
		[JsonProperty("rewardEntryHash")]
		public uint RewardEntryHash { get; set; }

		[JsonProperty("rewardEntryIdentifier")]
		public string RewardEntryIdentifier { get; set; }

		[JsonProperty("items")]
		public Destiny.DestinyItemQuantity[] Items { get; set; }

		[JsonProperty("vendorHash")]
		public uint? VendorHash { get; set; }

		[JsonProperty("displayProperties")]
		public Destiny.Definitions.Common.DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }

		[JsonProperty("order")]
		public int Order { get; set; }
	}
}
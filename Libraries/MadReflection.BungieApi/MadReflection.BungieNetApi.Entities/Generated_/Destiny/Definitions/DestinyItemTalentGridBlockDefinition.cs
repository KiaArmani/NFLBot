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
	/// <summary>
	/// This defines information that can only come from a talent grid on an item. Items mostly have negligible talent grid data these days, but instanced items still retain grids as a source for some of this common information.
	/// Builds/Subclasses are the only items left that still have talent grids with meaningful Nodes.
	/// </summary>
	public partial class DestinyItemTalentGridBlockDefinition
	{
		[JsonProperty("talentGridHash")]
		public uint TalentGridHash { get; set; }

		[JsonProperty("itemDetailString")]
		public string ItemDetailString { get; set; }

		[JsonProperty("buildName")]
		public string BuildName { get; set; }

		[JsonProperty("hudDamageType")]
		public Destiny.DamageType HudDamageType { get; set; }

		[JsonProperty("hudIcon")]
		public string HudIcon { get; set; }
	}
}

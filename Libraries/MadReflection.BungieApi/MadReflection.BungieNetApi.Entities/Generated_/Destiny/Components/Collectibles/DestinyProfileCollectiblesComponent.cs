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

namespace BungieNet.Destiny.Components.Collectibles
{
	public partial class DestinyProfileCollectiblesComponent
	{
		[JsonProperty("recentCollectibleHashes")]
		public uint[] RecentCollectibleHashes { get; set; }

		[JsonProperty("newnessFlaggedCollectibleHashes")]
		public uint[] NewnessFlaggedCollectibleHashes { get; set; }

		[JsonProperty("collectibles")]
		public System.Collections.Generic.Dictionary<uint, Destiny.Components.Collectibles.DestinyCollectibleComponent> Collectibles { get; set; }

		[JsonProperty("collectionCategoriesRootNodeHash")]
		public uint CollectionCategoriesRootNodeHash { get; set; }

		[JsonProperty("collectionBadgesRootNodeHash")]
		public uint CollectionBadgesRootNodeHash { get; set; }
	}
}

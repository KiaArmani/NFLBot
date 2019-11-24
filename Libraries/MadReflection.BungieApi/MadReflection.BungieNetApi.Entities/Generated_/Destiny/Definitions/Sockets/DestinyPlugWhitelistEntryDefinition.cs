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

namespace BungieNet.Destiny.Definitions.Sockets
{
	/// <summary>
	/// Defines a plug "Category" that is allowed to be plugged into a socket of this type.
	/// This should be compared against a given plug item's DestinyInventoryItemDefinition.plug.plugCategoryHash, which indicates the plug item's category.
	/// </summary>
	public partial class DestinyPlugWhitelistEntryDefinition
	{
		[JsonProperty("categoryHash")]
		public uint CategoryHash { get; set; }

		[JsonProperty("categoryIdentifier")]
		public string CategoryIdentifier { get; set; }

		[JsonProperty("reinitializationPossiblePlugHashes")]
		public uint[] ReinitializationPossiblePlugHashes { get; set; }
	}
}

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

namespace BungieNet.Destiny.Sockets
{
	public partial class DestinyItemPlugBase
	{
		[JsonProperty("plugItemHash")]
		public uint PlugItemHash { get; set; }

		[JsonProperty("canInsert")]
		public bool CanInsert { get; set; }

		[JsonProperty("enabled")]
		public bool Enabled { get; set; }

		[JsonProperty("insertFailIndexes")]
		public int[] InsertFailIndexes { get; set; }

		[JsonProperty("enableFailIndexes")]
		public int[] EnableFailIndexes { get; set; }
	}
}

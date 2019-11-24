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

namespace BungieNet.Destiny.Definitions.Director
{
	/// <summary>
	/// This is the position and other data related to nodes in the activity graph that you can click to launch activities. An Activity Graph node will only have one active Activity at a time, which will determine the activity to be launched (and, unless overrideDisplay information is provided, will also determine the tooltip and other UI related to the node)
	/// </summary>
	public partial class DestinyActivityGraphNodeDefinition
	{
		[JsonProperty("nodeId")]
		public uint NodeId { get; set; }

		[JsonProperty("overrideDisplay")]
		public Destiny.Definitions.Common.DestinyDisplayPropertiesDefinition OverrideDisplay { get; set; }

		[JsonProperty("position")]
		public Destiny.Definitions.Common.DestinyPositionDefinition Position { get; set; }

		[JsonProperty("featuringStates")]
		public Destiny.Definitions.Director.DestinyActivityGraphNodeFeaturingStateDefinition[] FeaturingStates { get; set; }

		[JsonProperty("activities")]
		public Destiny.Definitions.Director.DestinyActivityGraphNodeActivityDefinition[] Activities { get; set; }

		[JsonProperty("states")]
		public Destiny.Definitions.Director.DestinyActivityGraphNodeStateEntry[] States { get; set; }
	}
}

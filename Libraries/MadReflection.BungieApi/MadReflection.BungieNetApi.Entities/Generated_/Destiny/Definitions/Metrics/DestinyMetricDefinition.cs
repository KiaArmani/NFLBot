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

namespace BungieNet.Destiny.Definitions.Metrics
{
	public partial class DestinyMetricDefinition : Destiny.Definitions.DestinyDefinition
	{
		[JsonProperty("displayProperties")]
		public Destiny.Definitions.Common.DestinyDisplayPropertiesDefinition DisplayProperties { get; set; }

		[JsonProperty("trackingObjectiveHash")]
		public uint TrackingObjectiveHash { get; set; }

		[JsonProperty("lowerValueIsBetter")]
		public bool LowerValueIsBetter { get; set; }

		[JsonProperty("presentationNodeType")]
		public Destiny.DestinyPresentationNodeType PresentationNodeType { get; set; }

		[JsonProperty("traitIds")]
		public string[] TraitIds { get; set; }

		[JsonProperty("traitHashes")]
		public uint[] TraitHashes { get; set; }

		[JsonProperty("parentNodeHashes")]
		public uint[] ParentNodeHashes { get; set; }
	}
}
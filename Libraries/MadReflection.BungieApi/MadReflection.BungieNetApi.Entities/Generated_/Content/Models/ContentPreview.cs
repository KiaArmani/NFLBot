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

namespace BungieNet.Content.Models
{
	public partial class ContentPreview
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("itemInSet")]
		public bool ItemInSet { get; set; }

		[JsonProperty("setTag")]
		public string SetTag { get; set; }

		[JsonProperty("setNesting")]
		public int SetNesting { get; set; }

		[JsonProperty("useSetId")]
		public int UseSetId { get; set; }
	}
}
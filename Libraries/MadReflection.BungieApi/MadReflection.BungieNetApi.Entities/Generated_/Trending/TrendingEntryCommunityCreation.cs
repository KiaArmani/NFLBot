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

namespace BungieNet.Trending
{
	public partial class TrendingEntryCommunityCreation
	{
		[JsonProperty("media")]
		public string Media { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("author")]
		public string Author { get; set; }

		[JsonProperty("authorMembershipId")]
		public long AuthorMembershipId { get; set; }

		[JsonProperty("postId")]
		public long PostId { get; set; }

		[JsonProperty("body")]
		public string Body { get; set; }

		[JsonProperty("upvotes")]
		public int Upvotes { get; set; }
	}
}
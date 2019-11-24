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

namespace BungieNet.Community
{
	public partial class CommunityLiveStatus
	{
		[JsonProperty("dateStatusUpdated")]
		public DateTime DateStatusUpdated { get; set; }

		[JsonProperty("url")]
		public string Url { get; set; }

		[JsonProperty("partnershipIdentifier")]
		public string PartnershipIdentifier { get; set; }

		[JsonProperty("partnershipType")]
		public Partnerships.PartnershipType PartnershipType { get; set; }

		[JsonProperty("thumbnail")]
		public string Thumbnail { get; set; }

		[JsonProperty("thumbnailSmall")]
		public string ThumbnailSmall { get; set; }

		[JsonProperty("thumbnailLarge")]
		public string ThumbnailLarge { get; set; }

		[JsonProperty("destinyCharacterId")]
		public long DestinyCharacterId { get; set; }

		[JsonProperty("userInfo")]
		public User.UserInfoCard UserInfo { get; set; }

		[JsonProperty("currentActivityHash")]
		public uint CurrentActivityHash { get; set; }

		[JsonProperty("dateLastPlayed")]
		public DateTime DateLastPlayed { get; set; }

		[JsonProperty("dateStreamStarted")]
		public DateTime DateStreamStarted { get; set; }

		[JsonProperty("locale")]
		public string Locale { get; set; }

		[JsonProperty("currentViewers")]
		public int CurrentViewers { get; set; }

		[JsonProperty("followers")]
		public int Followers { get; set; }

		[JsonProperty("overallViewers")]
		public int OverallViewers { get; set; }

		[JsonProperty("isFeatured")]
		public bool IsFeatured { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("activityModeHash")]
		public uint ActivityModeHash { get; set; }

		[JsonProperty("dateFeatured")]
		public DateTime? DateFeatured { get; set; }

		[JsonProperty("trendingValue")]
		public decimal TrendingValue { get; set; }

		[JsonProperty("isSubscribable")]
		public bool IsSubscribable { get; set; }
	}
}

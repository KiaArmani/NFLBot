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

namespace BungieNet.Fireteam
{
	public partial class FireteamMember
	{
		[JsonProperty("destinyUserInfo")]
		public Fireteam.FireteamUserInfoCard DestinyUserInfo { get; set; }

		[JsonProperty("bungieNetUserInfo")]
		public User.UserInfoCard BungieNetUserInfo { get; set; }

		[JsonProperty("characterId")]
		public long CharacterId { get; set; }

		[JsonProperty("dateJoined")]
		public DateTime DateJoined { get; set; }

		[JsonProperty("hasMicrophone")]
		public bool HasMicrophone { get; set; }

		[JsonProperty("lastPlatformInviteAttemptDate")]
		public DateTime LastPlatformInviteAttemptDate { get; set; }

		[JsonProperty("lastPlatformInviteAttemptResult")]
		public Fireteam.FireteamPlatformInviteResult LastPlatformInviteAttemptResult { get; set; }
	}
}

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

namespace BungieNet.GroupsV2
{
	public partial class GroupMemberApplication
	{
		[JsonProperty("groupId")]
		public long GroupId { get; set; }

		[JsonProperty("creationDate")]
		public DateTime CreationDate { get; set; }

		[JsonProperty("resolveState")]
		public GroupsV2.GroupApplicationResolveState ResolveState { get; set; }

		[JsonProperty("resolveDate")]
		public DateTime? ResolveDate { get; set; }

		[JsonProperty("resolvedByMembershipId")]
		public long? ResolvedByMembershipId { get; set; }

		[JsonProperty("requestMessage")]
		public string RequestMessage { get; set; }

		[JsonProperty("resolveMessage")]
		public string ResolveMessage { get; set; }

		[JsonProperty("destinyUserInfo")]
		public User.UserInfoCard DestinyUserInfo { get; set; }

		[JsonProperty("bungieNetUserInfo")]
		public User.UserInfoCard BungieNetUserInfo { get; set; }
	}
}

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

namespace BungieNet.User
{
	public partial class HardLinkedUserMembership
	{
		[JsonProperty("membershipType")]
		public BungieMembershipType MembershipType { get; set; }

		[JsonProperty("membershipId")]
		public long MembershipId { get; set; }

		[JsonProperty("CrossSaveOverriddenType")]
		public BungieMembershipType CrossSaveOverriddenType { get; set; }

		[JsonProperty("CrossSaveOverriddenMembershipId")]
		public long? CrossSaveOverriddenMembershipId { get; set; }
	}
}

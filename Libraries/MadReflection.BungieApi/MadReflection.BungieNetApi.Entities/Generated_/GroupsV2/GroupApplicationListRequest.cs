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
	public partial class GroupApplicationListRequest
	{
		[JsonProperty("memberships")]
		public User.UserMembership[] Memberships { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}
}

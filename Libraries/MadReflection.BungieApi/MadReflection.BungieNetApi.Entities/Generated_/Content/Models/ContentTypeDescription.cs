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
	public partial class ContentTypeDescription
	{
		[JsonProperty("cType")]
		public string CType { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("contentDescription")]
		public string ContentDescription { get; set; }

		[JsonProperty("previewImage")]
		public string PreviewImage { get; set; }

		[JsonProperty("priority")]
		public int Priority { get; set; }

		[JsonProperty("reminder")]
		public string Reminder { get; set; }

		[JsonProperty("properties")]
		public Content.Models.ContentTypeProperty[] Properties { get; set; }

		[JsonProperty("tagMetadata")]
		public Content.Models.TagMetadataDefinition[] TagMetadata { get; set; }

		[JsonProperty("tagMetadataItems")]
		public System.Collections.Generic.Dictionary<string, Content.Models.TagMetadataItem> TagMetadataItems { get; set; }

		[JsonProperty("usageExamples")]
		public string[] UsageExamples { get; set; }

		[JsonProperty("showInContentEditor")]
		public bool ShowInContentEditor { get; set; }

		[JsonProperty("typeOf")]
		public string TypeOf { get; set; }

		[JsonProperty("bindIdentifierToProperty")]
		public string BindIdentifierToProperty { get; set; }

		[JsonProperty("boundRegex")]
		public string BoundRegex { get; set; }

		[JsonProperty("forceIdentifierBinding")]
		public bool ForceIdentifierBinding { get; set; }

		[JsonProperty("allowComments")]
		public bool AllowComments { get; set; }

		[JsonProperty("autoEnglishPropertyFallback")]
		public bool AutoEnglishPropertyFallback { get; set; }

		[JsonProperty("bulkUploadable")]
		public bool BulkUploadable { get; set; }

		[JsonProperty("previews")]
		public Content.Models.ContentPreview[] Previews { get; set; }

		[JsonProperty("suppressCmsPath")]
		public bool SuppressCmsPath { get; set; }

		[JsonProperty("propertySections")]
		public Content.Models.ContentTypePropertySection[] PropertySections { get; set; }
	}
}
using Newtonsoft.Json;

namespace BungieNet.Destiny.Definitions.Items
{
    partial class DestinyItemPlugDefinition
    {
        [JsonProperty("actionRewardSiteHash")] public uint ActionRewardSiteHash { get; set; }

        [JsonProperty("actionRewardItemOverrideHash")]
        public uint ActionRewardItemOverrideHash { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BungieNet.Api;
using BungieNet.Destiny.HistoricalStats;
using BungieNet.GroupsV2;
using Microsoft.Extensions.Logging;

namespace XurQuester.Services
{
    public class BungieService
    {
        private readonly BungieClient _bungieClient;

        public BungieService(ILogger<BungieService> logger, IServiceProvider services)
        {
            // Create new BungieClient
            _bungieClient =
                new BungieClient(new BungieApiKey(Environment.GetEnvironmentVariable("XUR_QUESTER_BUNGIETOKEN")));
        }


        /// <summary>
        ///     Returns the Download URL to the current Manifest
        /// </summary>
        /// <returns></returns>
        public string GetManifestUrl()
        {
            var manifestResult = _bungieClient.Destiny2.GetDestinyManifest();
            return $"https://www.bungie.net{manifestResult.MobileWorldContentPaths["en"]}";
        }

        /// <summary>
        ///     Returns a List of the Membership IDs of all clan members
        /// </summary>
        /// <returns></returns>
        public async Task<List<long>> GetMembershipIdListOfClanMembers()
        {
            // Get Clan ID from Environment Variables
            var clanId = Convert.ToInt64(Environment.GetEnvironmentVariable("XUR_QUESTER_CLANID"),
                CultureInfo.InvariantCulture);

            // Get Members of Clan from Bungie
            var clanResult = await _bungieClient.GroupV2.GetMembersOfGroupAsync(
                clanId,
                0,
                RuntimeGroupMemberType.Member,
                string.Empty).ConfigureAwait(false);

            // Loop through Members
            var clanMembershipIds = new List<long>();
            foreach (var clanMember in clanResult.Results)
                // Get their Membership ID required for getting their Profile & Activity Information
                clanMembershipIds.Add(clanMember.DestinyUserInfo.MembershipId);

            return clanMembershipIds;
        }

        public async Task<DestinyPostGameCarnageReportData> GetPostCarnageReport(long instanceId)
        {
            return await _bungieClient.Destiny2.GetPostGameCarnageReportAsync(instanceId);
        }
    }
}
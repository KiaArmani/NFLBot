using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BungieNet.Api;
using BungieNet.Destiny.HistoricalStats;
using BungieNet.GroupsV2;

namespace XurBlitzer.Services
{
    public class BungieService
    {
        private readonly BungieClient _bungieClient;

        public BungieService(IServiceProvider services)
        {
            // Create new BungieClient
            _bungieClient =
                new BungieClient(new BungieApiKey(Environment.GetEnvironmentVariable("XUR_BLITZER_BUNGIETOKEN")));
        }

        /// <summary>
        ///     Returns a List of the Membership IDs of all clan members
        /// </summary>
        /// <returns></returns>
        public async Task<List<long>> GetMembershipIdListOfClanMembers()
        {
            // Get Clan ID from Environment Variables
            var clanID = Convert.ToInt64(Environment.GetEnvironmentVariable("XUR_BLITZER_CLANID"),
                CultureInfo.InvariantCulture);

            // Get Members of Clan from Bungie
            var clanResult = await _bungieClient.GroupV2.GetMembersOfGroupAsync(
                clanID,
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
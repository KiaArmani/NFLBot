using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BungieNet;
using BungieNet.Api;
using BungieNet.Destiny;
using BungieNet.Destiny.HistoricalStats;
using BungieNet.Destiny.HistoricalStats.Definitions;
using BungieNet.GroupsV2;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using XurClassLibrary.Models;
using XurClassLibrary.Models.Destiny;
using LogSeverity = Discord.LogSeverity;

namespace XurCollector.Services
{
    public class BungieService
    {
        private const int PageCount = 2;
        private readonly ActivityCacheService _activityCacheService;
        private readonly BungieClient _bungieClient;
        private readonly MongoService _mongoService;

        public BungieService(IServiceProvider services)
        {
            // Create new BungieClient
            _bungieClient = new BungieClient(new BungieApiKey(Environment.GetEnvironmentVariable("XUR_COLLECTOR_BUNGIETOKEN")));
            _mongoService = services.GetRequiredService<MongoService>();
            _activityCacheService = services.GetRequiredService<ActivityCacheService>();
        }

        public event Func<LogMessage, Task> Log;

        public async Task GetActivityDataOfClan()
        {
            var clanMembers = await GetMembershipList();
            var newActivities = new List<NDestinyHistoricalStatsPeriodGroup>();
            // Loop through Members
            foreach (var clanMember in clanMembers)
            {
                // Get their Membership ID required for getting their Profile & Activity Information
                var membershipId = clanMember.DestinyUserInfo.MembershipId;

                // Get their Profile Information
                var memberdata = await _bungieClient.Destiny2.GetProfileAsync(
                    clanMember.DestinyUserInfo.MembershipType,
                    membershipId, DestinyComponentType.Characters).ConfigureAwait(false);

                // Skip if no data was received. Usually only when Bungie has Server issues.
                if (memberdata == null)
                    continue;

                WriteLog(LogSeverity.Debug,
                    $"Found {memberdata.Characters.Data.Count} characters for {clanMember.DestinyUserInfo.DisplayName}");

                // Loop through Characters of Member
                foreach (var playerCharacter in memberdata.Characters.Data)
                {
                    // Hold Character ID in separate Variable for convenience
                    var (characterId, value) = playerCharacter;
                    newActivities.AddRange(await GetActivityData(value.MembershipType, membershipId, characterId,
                        clanMember));
                }
            }

            if (newActivities.Count > 0)
                await _mongoService.AddNewActivities(newActivities);
        }

        /// <summary>
        ///     Scans through a players Activities returns a list of them
        /// </summary>
        /// <param name="membershipType">Membership Type of the given Account</param>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="characterId">Character ID</param>
        /// <param name="clanMember">GroupMember Object</param>
        /// <returns></returns>
        private async Task<List<NDestinyHistoricalStatsPeriodGroup>> GetActivityData(
            BungieMembershipType membershipType, long membershipId, long characterId, GroupMember clanMember)
        {
            var returnData = new List<NDestinyHistoricalStatsPeriodGroup>();
            for (var i = 0; i < PageCount; i++)
            {
                // Get Activity History of Character
                // Using a Try-Catch Block as players might opt-in to refuse API calls to their history
                DestinyActivityHistoryResults allData;
                try
                {
                    allData = await _bungieClient.Destiny2.GetActivityHistoryAsync(
                        membershipType,
                        membershipId,
                        characterId,
                        30,
                        DestinyActivityModeType.None,
                        i).ConfigureAwait(false);
                }
                catch (BungieException e
                ) // Most likely to be privacy related exceptions. For now we just ignore those scores.
                {
                    Console.WriteLine($"{membershipId} - {e}");
                    continue;
                }

                // If we didn't get any data, skip
                if (allData?.Activities == null) continue;

                // Loop through Activities
                foreach (var activity in allData.Activities)
                {
                    // If the Activity is before the start of the ranking period (usually a Season), skip the Score.
                    var activityDate = activity.Period;
                    if (activityDate < ChallengeGlobals.CurrentSeasonStart)
                        continue;

                    // If we already collected that Activity, stop here
                    if (_activityCacheService.TryGetActivityByInstanceId(activity.ActivityDetails.InstanceId) != null)
                        continue;

                    // Insert into Collection
                    WriteLog(LogSeverity.Info,
                        $"Adding {Enum.GetName(typeof(DestinyActivityModeType), activity.ActivityDetails.Mode)} Activity {activity.ActivityDetails.InstanceId} from Player {clanMember.DestinyUserInfo.DisplayName} from {activityDate}.");
                    returnData.Add(new NDestinyHistoricalStatsPeriodGroup(activity, membershipId));

                    // Add to Cache
                    _activityCacheService.AddActivityData(
                        new Tuple<long, NDestinyHistoricalStatsPeriodGroup>(
                            activity.ActivityDetails.InstanceId,
                            new NDestinyHistoricalStatsPeriodGroup(activity, membershipId
                            )));
                }
            }

            return returnData;
        }

        /// <summary>
        ///     Returns a List of all Clan Members
        /// </summary>
        /// <returns></returns>
        public async Task<List<GroupMember>> GetMembershipList()
        {
            // Get Clan ID from Environment Variables
            var clanID = Convert.ToInt64(Environment.GetEnvironmentVariable("NFLBOT_CLANID"),
                CultureInfo.InvariantCulture);

            // Get Members of Clan from Bungie
            var clanResult = await _bungieClient.GroupV2.GetMembersOfGroupAsync(
                clanID,
                0,
                RuntimeGroupMemberType.Member,
                string.Empty).ConfigureAwait(false);

            return clanResult.Results.ToList();
        }

        /// <summary>
        ///     Wrapper for Log
        /// </summary>
        /// <param name="Discord.LogSeverity">Severity of Message</param>
        /// <param name="message">Message Text</param>
        private void WriteLog(LogSeverity logSeverity, string message)
        {
            Log?.Invoke(new LogMessage(logSeverity, "Bungie", message));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BungieNet;
using BungieNet.Api;
using BungieNet.Destiny;
using BungieNet.Destiny.HistoricalStats;
using BungieNet.Destiny.HistoricalStats.Definitions;
using BungieNet.GroupsV2;
using XurClassLibrary.Models;
using XurClassLibrary.Models.Destiny;

namespace XurCollector.Services
{
    public static class BungieService
    {
        private const int PageCount = 2;
        private static BungieClient _bungieClient;
        private static Timer _timer;

        public static void Start()
        {
            _bungieClient =
                new BungieClient(new BungieApiKey(Environment.GetEnvironmentVariable("XUR_COLLECTOR_BUNGIETOKEN")));

            _timer = new Timer(GetActivityDataOfClan,
                               null,
                               TimeSpan.Zero,
                               TimeSpan.FromMinutes(15));
        }

        public static async void GetActivityDataOfClan(object state)
        {
            Console.WriteLine("Collection of Activity Data started.");

            // Get Memberlist
            var clanMembers = await GetMembershipList();

            // Loop through Members
            foreach (var clanMember in clanMembers)
            {
                // Create List of Activities
                var memberActivities = new List<NDestinyHistoricalStatsPeriodGroup>();

                // Get their Membership ID required for getting their Profile & Activity Information
                var membershipId = clanMember.DestinyUserInfo.MembershipId;

                // Get their Profile Information
                try
                {
                    var memberdata = _bungieClient.Destiny2.GetProfile(
                    clanMember.DestinyUserInfo.MembershipType,
                    membershipId, DestinyComponentType.Characters);

                    // Skip if no data was received. Usually only when Bungie has Server issues.
                    if (memberdata == null)
                        continue;

                    Console.WriteLine(
                        $"Found {memberdata.Characters.Data.Count} characters for {clanMember.DestinyUserInfo.DisplayName}");

                    // Loop through Characters of Member
                    foreach (var playerCharacter in memberdata.Characters.Data)
                    {
                        // Hold Character ID in separate Variable for convenience
                        var (characterId, value) = playerCharacter;
                        memberActivities.AddRange(GetActivityData(value.MembershipType, membershipId, characterId,
                            clanMember));
                    }

                    if (memberActivities.Count > 0)
                        await MongoService.AddNewActivities(memberActivities);
                }
                catch(BungieException)
                {
                    // Ignore Bungie Exceptions for now
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        /// <summary>
        ///     Scans through a players Activities returns a list of them
        /// </summary>
        /// <param name="membershipType">Membership Type of the given Account</param>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="characterId">Character ID</param>
        /// <param name="clanMember">GroupMember Object</param>
        /// <returns></returns>
        private static List<NDestinyHistoricalStatsPeriodGroup> GetActivityData(
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
                    allData = _bungieClient.Destiny2.GetActivityHistory(
                        membershipType,
                        membershipId,
                        characterId,
                        30,
                        DestinyActivityModeType.None,
                        i);
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
                    if (ActivityCacheService.TryGetActivityByInstanceId(activity.ActivityDetails.InstanceId) != null)
                        continue;

                    // Insert into Collection
                    Console.WriteLine(
                        $"Adding {Enum.GetName(typeof(DestinyActivityModeType), activity.ActivityDetails.Mode)} Activity {activity.ActivityDetails.InstanceId} from Player {clanMember.DestinyUserInfo.DisplayName} from {activityDate}.");
                    returnData.Add(new NDestinyHistoricalStatsPeriodGroup(activity, membershipId, clanMember.DestinyUserInfo.DisplayName));

                    // Add to Cache
                    ActivityCacheService.AddActivityData(
                        new Tuple<long, NDestinyHistoricalStatsPeriodGroup>(
                            activity.ActivityDetails.InstanceId,
                            new NDestinyHistoricalStatsPeriodGroup(activity, membershipId, clanMember.DestinyUserInfo.DisplayName
                            )));
                }
            }

            return returnData;
        }

        /// <summary>
        ///     Returns a List of all Clan Members
        /// </summary>
        /// <returns></returns>
        public static async Task<List<GroupMember>> GetMembershipList()
        {
            // Get Clan ID from Environment Variables
            var clanID = Convert.ToInt64(Environment.GetEnvironmentVariable("XUR_COLLECTOR_CLANID"),
                CultureInfo.InvariantCulture);

            // Use proper paging to get all members
            var clanMembers = new List<GroupMember>();
            bool haveAll = false;
            var page = 1;

            while(!haveAll)
            {
                // Get Members of Clan from Bungie
                var clanResult = await _bungieClient.GroupV2.GetMembersOfGroupAsync(
                    clanID,
                    page,
                    RuntimeGroupMemberType.None,
                    string.Empty);

                page++;
                clanMembers.AddRange(clanResult.Results);
                if (!clanResult.HasMore)
                    haveAll = true;
            }

            return clanMembers;
        }
    }
}
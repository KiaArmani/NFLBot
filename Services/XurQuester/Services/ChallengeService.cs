using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BungieNet.Destiny;
using BungieNet.Destiny.HistoricalStats.Definitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XurClassLibrary.Models;
using static System.Decimal;

namespace XurQuester.Services
{
    public class ChallengeService
    {
        private readonly BungieService _bungieService;
        private readonly ILogger<ChallengeService> _logger;
        private readonly ManifestService _manifestService;
        private readonly MongoService _mongoService;

        private List<long> _clanMembershipIds;

        public ChallengeService(ILogger<ChallengeService> logger, IServiceProvider services)
        {
            _logger = logger;
            _bungieService = services.GetRequiredService<BungieService>();
            _mongoService = services.GetRequiredService<MongoService>();
            _manifestService = services.GetRequiredService<ManifestService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = CheckChallengeActivities();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Checks for the completion status of the current challenges
        /// </summary>
        /// <returns></returns>
        public async Task CheckChallengeActivities()
        {
            _logger.LogInformation("Checking Challenge Activities..");
            // Get Membership List from Clan
            // Required for some challenges
            _clanMembershipIds = await _bungieService.GetMembershipIdListOfClanMembers();
            try
            {
                await Tier1Normal();
                //await Tier1Heroic();

                await Tier2Normal();
                //await Tier2Heroic();

                await Tier3Normal();
                //await Tier3Heroic();

                _logger.LogInformation("Checking Challenge Activities done!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await Task.Delay(new TimeSpan(0, 10, 0)).ContinueWith(async o => { await CheckChallengeActivities(); });
        }


        /// <summary>
        ///     Checks if players did the Tier 1 Normal Challenge
        /// </summary>
        /// <returns></returns>
        private async Task Tier1Normal()
        {
            // Check Tier 1 Normal
            // Send 3 Large Blockers in a single game of Gambit and win the match.
            _logger.LogInformation("Checking Activities for Tier 1 Normal..");
            var completedActivitiesOfType =
                _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.Gambit);

            foreach (var completedActivity in completedActivitiesOfType)
            {
                var pcgr = await _bungieService.GetPostCarnageReport(completedActivity.Data.ActivityDetails
                    .InstanceId);

                if (pcgr?.Entries == null)
                    continue;

                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null) continue;

                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!_clanMembershipIds.Contains(membershipId)) continue;

                    // If Defeated, abort
                    if (playerEntry.Values["standing"].Basic.Value == 1)
                        continue;

                    var validRun = playerEntry.Extended.Values["largeBlockersSent"].Basic.Value >= 3;
                    if (!validRun) continue;

                    var finishedChallengeAlready = _mongoService.HasCompletedChallenge
                    (membershipId, ChallengeGlobals.CurrentWeek, ChallengeGlobals.Tier1Normal.Tier,
                        ChallengeGlobals.Tier1Normal.Difficulty);

                    if (finishedChallengeAlready) continue;

                    var newEntry = new ChallengeEntry(
                        playerEntry.Player.DestinyUserInfo.DisplayName,
                        membershipId,
                        completedActivity.Data.ActivityDetails.InstanceId.ToString(),
                        ChallengeGlobals.Tier1Normal);

                    await _mongoService.AddFinishedChallenge(newEntry);
                    _logger.LogInformation(
                        $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {ChallengeGlobals.CurrentWeek}, Tier: {ChallengeGlobals.Tier1Normal.Tier})");
                }
            }

            _logger.LogInformation("Checking Activities for Tier 1 Normal done!");
        }

        /// <summary>
        ///     Checks if players did the Tier 1 Heroic Challenge
        /// </summary>
        /// <returns></returns>
        private async Task Tier1Heroic()
        {
            // Check Tier 1 Heroic
            // Get 165 or more final blows on enemies using a sidearm in a single instance of Vex Offensive.
            _logger.LogInformation("Checking Activities for Tier 1 Heroic..");
            var completedVexOffensives =
                _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.VexOffensive);

            foreach (var completedVexOffensive in completedVexOffensives)
            {
                var pcgr = await _bungieService.GetPostCarnageReport(completedVexOffensive.Data.ActivityDetails
                    .InstanceId);

                if (pcgr?.Entries == null)
                    continue;

                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null) continue;

                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!_clanMembershipIds.Contains(membershipId)) continue;

                    // Tier 1 Logic
                    var validNormalRun = playerEntry.Values["kills"].Basic.Value >= 165;
                    if (validNormalRun)
                    {
                        var finishedChallengeAlready = _mongoService.HasCompletedChallenge
                        (membershipId, ChallengeGlobals.CurrentWeek, ChallengeGlobals.Tier1Normal.Tier,
                            ChallengeGlobals.Tier1Normal.Difficulty);

                        if (!finishedChallengeAlready)
                        {
                            var newEntry = new ChallengeEntry(
                                playerEntry.Player.DestinyUserInfo.DisplayName,
                                membershipId,
                                completedVexOffensive.Data.ActivityDetails.InstanceId.ToString(),
                                ChallengeGlobals.Tier1Normal);

                            await _mongoService.AddFinishedChallenge(newEntry);
                            _logger.LogInformation(
                                $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {ChallengeGlobals.CurrentWeek}, Tier: {ChallengeGlobals.Tier1Normal.Tier})");
                        }
                    }

                    // Tier 1 Heroic Logic
                    if (playerEntry.Extended.Weapons == null)
                        continue;

                    var sidearmKills = playerEntry.Extended.Weapons
                        .Where(weapon => _manifestService.GetWeaponType(weapon.ReferenceId).Equals("Sidearm"))
                        .Sum(weapon => ToInt32(weapon.Values["uniqueWeaponKills"].Basic.Value));

                    if (sidearmKills >= 165)
                    {
                        var finishedChallengeAlready = _mongoService.HasCompletedChallenge
                        (membershipId, ChallengeGlobals.CurrentWeek, ChallengeGlobals.Tier1Heroic.Tier,
                            ChallengeGlobals.Tier1Heroic.Difficulty);

                        if (finishedChallengeAlready) continue;

                        var newEntry = new ChallengeEntry(
                            playerEntry.Player.DestinyUserInfo.DisplayName,
                            membershipId,
                            completedVexOffensive.Data.ActivityDetails.InstanceId.ToString(),
                            ChallengeGlobals.Tier1Heroic);

                        await _mongoService.AddFinishedChallenge(newEntry);
                        _logger.LogInformation(
                            $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {ChallengeGlobals.CurrentWeek}, Tier: {ChallengeGlobals.Tier1Heroic.Tier})");
                    }
                }
            }

            _logger.LogInformation("Checking Activities for Tier 1 Heroic done!");
        }

        /// <summary>
        ///     Checks if players did the Tier 2 Normal Challenge
        /// </summary>
        /// <returns></returns>
        private async Task Tier2Normal()
        {
            // Check Challenge 2
            // As a fireteam of Two: Complete the "Zero Hour" Story Mission in less than 15 Minutes.
            _logger.LogInformation("Checking Activities for Tier 2 Normal..");
            var completedActivitiesOfType = _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.Story);

            foreach (var completedActivity in completedActivitiesOfType)
            {
                // Get PCGR
                var pcgr = await _bungieService.GetPostCarnageReport(completedActivity.Data.ActivityDetails
                    .InstanceId);

                // Skip if no response
                if (pcgr?.Entries == null)
                    continue;

                // If it's not Zero Hour, skip
                if (pcgr.ActivityDetails.DirectorActivityHash != "3232506937")
                    continue;

                // If there are not two players, skip
                if (pcgr.Entries.Length != 2)
                    continue;

                // Check if all Members of the Fireteam are in the Clan
                var membershipIdsOfPlayers = pcgr.Entries.Select(x => x.Player.DestinyUserInfo.MembershipId).ToList();
                var allClanMembers = true;
                foreach (var _ in membershipIdsOfPlayers.Where(player => !_clanMembershipIds.Contains(player)))
                    allClanMembers = false;

                // Do not allow Forges where non-clan members were in
                if (!allClanMembers)
                    continue;

                // Save smallest reported activity time
                decimal activityTime = 99999999;
                foreach (var playerEntry in pcgr.Entries)
                    activityTime = playerEntry.Values["timePlayedSeconds"].Basic.Value < activityTime
                        ? playerEntry.Values["timePlayedSeconds"].Basic.Value
                        : activityTime;

                // If it's over 15 Minutes, skip
                if (activityTime > 900)
                    continue;

                // Write successful run for each member
                foreach (var playerEntry in pcgr.Entries)
                {
                    var finishedChallengeAlready = _mongoService.HasCompletedChallenge
                    (playerEntry.CharacterId, ChallengeGlobals.CurrentWeek, ChallengeGlobals.Tier2Normal.Tier,
                        ChallengeGlobals.Tier2Normal.Difficulty);

                    if (finishedChallengeAlready) continue;

                    var newEntry = new ChallengeEntry(
                        playerEntry.Player.DestinyUserInfo.DisplayName,
                        playerEntry.CharacterId,
                        completedActivity.Data.ActivityDetails.InstanceId.ToString(),
                        ChallengeGlobals.Tier2Normal);

                    await _mongoService.AddFinishedChallenge(newEntry);
                    _logger.LogInformation(
                        $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {ChallengeGlobals.CurrentWeek}, Tier: {ChallengeGlobals.Tier2Normal})");
                }
            }

            _logger.LogInformation("Checking Activities for Tier 2 Normal done!");
        }

        /// <summary>
        ///     Checks if players did the Tier 2 Heroic Challenge
        /// </summary>
        /// <returns></returns>
        private async Task Tier2Heroic()
        {
            // Check Tier 2 Heroic
            // As a fireteam of clan members, complete this week's Nightfall: The Ordeal flawlessly in 12 minutes or less. A higher difficulty rewards more points.
            _logger.LogInformation("Checking Activities for Tier 2 Heroic..");
            var completedNightfalls = _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.Nightfall);
            completedNightfalls.AddRange(
                _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.HeroicNightfall));
            completedNightfalls.AddRange(
                _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.ScoredNightfall));
            completedNightfalls.AddRange(
                _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.ScoredHeroicNightfall));

            foreach (var completedNightfall in completedNightfalls)
            {
                var pcgr = await _bungieService.GetPostCarnageReport(completedNightfall.Data.ActivityDetails
                    .InstanceId);

                if (pcgr?.Entries == null)
                    continue;

                // Check if all Members of the Fireteam are in the Clan
                var membershipIdsOfPlayers = pcgr.Entries.Select(x => x.Player.DestinyUserInfo.MembershipId).ToList();
                var allClanMembers = true;
                foreach (var player in membershipIdsOfPlayers.Where(player => !_clanMembershipIds.Contains(player)))
                    allClanMembers = false;

                // Do not allow Forges where non-clan members were in
                if (!allClanMembers)
                    continue;

                var validRun = true;
                decimal nightfallTime = 99999999;

                foreach (var playerEntry in pcgr.Entries)
                {
                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!_clanMembershipIds.Contains(membershipId))
                        continue;

                    if (playerEntry.Values["deaths"] == null || playerEntry.Values["timePlayedSeconds"] == null ||
                        playerEntry.Values["completionReason"] == null)
                        continue;

                    if (playerEntry.Values["deaths"].Basic.Value > 0)
                    {
                        validRun = false;
                        continue;
                    }

                    nightfallTime = playerEntry.Values["timePlayedSeconds"].Basic.Value < nightfallTime
                        ? playerEntry.Values["timePlayedSeconds"].Basic.Value
                        : nightfallTime;
                }

                if (!validRun || nightfallTime > 720) continue;

                foreach (var playerEntry in pcgr.Entries)
                {
                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    var finishedChallengeAlready = _mongoService.HasCompletedChallenge
                    (membershipId, ChallengeGlobals.CurrentWeek, ChallengeGlobals.Tier2Heroic.Tier,
                        ChallengeGlobals.Tier2Heroic.Difficulty);

                    if (finishedChallengeAlready) continue;

                    var newEntry = new ChallengeEntry(
                        playerEntry.Player.DestinyUserInfo.DisplayName,
                        membershipId,
                        completedNightfall.Data.ActivityDetails.InstanceId.ToString(),
                        ChallengeGlobals.Tier2Heroic);

                    await _mongoService.AddFinishedChallenge(newEntry);
                    _logger.LogInformation(
                        $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {ChallengeGlobals.CurrentWeek}, Tier: {ChallengeGlobals.Tier2Heroic})");
                }
            }

            _logger.LogInformation("Checking Activities for Tier 2 Heroic done!");
        }

        /// <summary>
        ///     Checks if players did the Tier 3 Normal Challenge
        /// </summary>
        /// <returns></returns>
        private async Task Tier3Normal()
        {
            // Check Challenge 3
            // Complete the Spire of Stars Raid having one Player never die and dealing no damage.
            WriteLog(LogSeverity.Info, "Checking Activities for Tier 3 Normal..");
            var completedRaids = _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.Raid);

            foreach (var completedRaid in completedRaids)
            { 
                // Get PCGR
                var pcgr = await _bungieService.GetPostCarnageReport(completedRaid.Data.ActivityDetails
                    .InstanceId);

                // If no data was returned, skip
                if (pcgr?.Entries == null)
                    continue;

                // Check if all Members of the Fireteam are in the Clan
                var membershipIdsOfPlayers = pcgr.Entries.Select(x => x.Player.DestinyUserInfo.MembershipId).ToList();
                var allClanMembers = true;
                foreach (var _ in membershipIdsOfPlayers.Where(player => !_clanMembershipIds.Contains(player)))
                    allClanMembers = false;

                // Do not allow Forges where non-clan members were in
                if (!allClanMembers)
                    continue;

                var validRun = false;
                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null)
                        continue;

                    var deaths = playerEntry.Values["deaths"].Basic.Value;
                    var kills = playerEntry.Values["kills"].Basic.Value;

                    if (deaths == 0 && kills == 0)
                        validRun = true;
                }

                if (!validRun) continue;

                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null)
                        continue;

                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;

                    var finishedChallengeAlready = _mongoService.HasCompletedChallenge
                    (membershipId, ChallengeGlobals.CurrentWeek, ChallengeGlobals.Tier3Normal.Tier,
                        ChallengeGlobals.Tier3Normal.Difficulty);

                    if (finishedChallengeAlready) continue;

                    var newEntry = new ChallengeEntry(
                        playerEntry.Player.DestinyUserInfo.DisplayName,
                        membershipId,
                        completedRaid.Data.ActivityDetails.InstanceId.ToString(),
                        ChallengeGlobals.Tier3Normal);

                    await _mongoService.AddFinishedChallenge(newEntry);
                    _logger.LogInformation(
                        $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a challenge! (Week: {ChallengeGlobals.CurrentWeek}, Tier: {ChallengeGlobals.Tier3Normal.Tier})");
                }
            }

            _logger.LogInformation("Checking Activities for Tier 3 Normal done!");
        }

        /// <summary>
        ///     Checks if players did the Tier 3 Heroic Challenge
        /// </summary>
        /// <returns></returns>
        private async Task Tier3Heroic()
        {
            // Check Challenge 3
            // Complete the Scourge of the Past Raid using Blue Weapons & Armor and a 150 Speed Sparrow, having each Character Class only twice in the Fireteam. 
            string[] validRaids = {"548750096", "2812525063"};
            _logger.LogInformation("Checking Activities for Tier 3 Heroic..");
            var completedRaids = _mongoService.GetCompletedActivitiesOfType(DestinyActivityModeType.Raid);

            foreach (var completedRaid in completedRaids)
            {
                if (!validRaids.Contains(completedRaid.Data.ActivityDetails.DirectorActivityHash))
                    continue;

                var pcgr = await _bungieService.GetPostCarnageReport(completedRaid.Data.ActivityDetails
                    .InstanceId);

                if (pcgr?.Entries == null)
                    continue;

                var validRun = true;

                var warlockCount = 0;
                var titanCount = 0;
                var hunterCount = 0;

                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null)
                        continue;

                    var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!_clanMembershipIds.Contains(membershipId)) validRun = false;

                    foreach (var weaponUsed in playerEntry.Extended.Weapons)
                        // No Non-Blue Weapons
                        if (_manifestService.GetWeaponQuality(weaponUsed.ReferenceId) != TierType.Rare)
                            validRun = false;

                    switch (playerEntry.Player.CharacterClass)
                    {
                        case "Hunter":
                            hunterCount++;
                            break;
                        case "Titan":
                            titanCount++;
                            break;
                        case "Warlock":
                            warlockCount++;
                            break;
                    }
                }

                if (validRun && warlockCount == 2 && titanCount == 2 && hunterCount == 2)
                    foreach (var playerEntry in pcgr.Entries)
                    {
                        if (playerEntry.Player.DestinyUserInfo == null)
                            continue;

                        var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                        if (!_clanMembershipIds.Contains(membershipId))
                            continue;

                        var finishedChallengeAlready = _mongoService.HasCompletedChallenge
                        (membershipId, ChallengeGlobals.CurrentWeek, ChallengeGlobals.Tier3Heroic.Tier,
                            ChallengeGlobals.Tier3Heroic.Difficulty);

                        if (finishedChallengeAlready) continue;

                        var newEntry = new ChallengeEntry(
                            playerEntry.Player.DestinyUserInfo.DisplayName,
                            membershipId,
                            completedRaid.Data.ActivityDetails.InstanceId.ToString(),
                            ChallengeGlobals.Tier3Heroic);

                        await _mongoService.AddFinishedChallenge(newEntry);
                    }
            }

            _logger.LogInformation("Checking Activities for Tier 3 Heroic done!");
        }
    }
}
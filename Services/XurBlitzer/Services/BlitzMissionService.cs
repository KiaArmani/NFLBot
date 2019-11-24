using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XurClassLibrary.Models;

namespace XurBlitzer.Services
{
    public class BlitzMissionService
    {
        private readonly BungieService _bungieService;
        private readonly ILogger<BlitzMissionService> _logger;
        private readonly MongoService _mongoService;

        public BlitzMissionService(ILogger<BlitzMissionService> logger, IServiceProvider services)
        {
            _logger = logger;
            _bungieService = services.GetRequiredService<BungieService>();
            _mongoService = services.GetRequiredService<MongoService>();
        }

        public BlitzMission CurrentActiveMission { get; private set; }
        public DateTime CurrentActiveMissionStart { get; private set; }

        public DateTime CurrentActiveMissionEnd => CurrentActiveMissionStart.AddHours(4);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = StartRandomBlitzMission();
            return Task.CompletedTask;
        }

        public async Task StartRandomBlitzMission()
        {
            // Select Random Blitz Mission
            var random = new Random();
            var indexOfNextMission = random.Next(BlitzMissionGlobals.BlitzMissionList.Count);
            var newBlitzMission = BlitzMissionGlobals.BlitzMissionList[indexOfNextMission];
            newBlitzMission.Metadata.Value =
                random.Next(newBlitzMission.Metadata.Range[0], newBlitzMission.Metadata.Range[1]);

            if (CurrentActiveMission != null && newBlitzMission.Name == CurrentActiveMission.Name)
            {
                await StartRandomBlitzMission();
            }
            else
            {
                CurrentActiveMission = newBlitzMission;
                CurrentActiveMissionStart = DateTime.Now;
                _logger.LogInformation(
                    $"New Blitz Mission Selected: {CurrentActiveMission.Name}, {CurrentActiveMission.Metadata.ValueField}, {CurrentActiveMission.Metadata.Value}");
                await Task.Delay(new TimeSpan(4, 0, 0)).ContinueWith(async o => { await StartRandomBlitzMission(); });
            }
        }

        /// <summary>
        ///     Checks for the completion status of the current challenges
        /// </summary>
        /// <returns></returns>
        public async Task CheckBlitzMissionProgress()
        {
            _logger.LogInformation("Checking Challenge Activities..");

            // Get Membership List from Clan. Required for some challenges
            var clanMembershipIds = await _bungieService.GetMembershipIdListOfClanMembers();

            try
            {
                _logger.LogInformation($"Checking Activities for {CurrentActiveMission.Name}");
                var activitiesOfType = _mongoService.GetBlitzActivitiesOfType(CurrentActiveMission.Metadata.ModeType,
                    CurrentActiveMissionStart, CurrentActiveMissionEnd);
                foreach (var activity in activitiesOfType)
                {
                    var pcgr = await _bungieService.GetPostCarnageReport(activity.Data.ActivityDetails.InstanceId);
                    if (pcgr?.Entries == null)
                        continue;

                    foreach (var playerEntry in pcgr.Entries)
                    {
                        // If the player is no member, skip
                        var membershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                        if (!clanMembershipIds.Contains(membershipId)) continue;

                        var finishedChallengeAlready = _mongoService.HasCompletedBlitzMission
                            (membershipId, CurrentActiveMissionStart, CurrentActiveMission);

                        if (finishedChallengeAlready)
                            continue;

                        decimal activityValue = 0;
                        switch (CurrentActiveMission.Metadata.ValueFieldType)
                        {
                            case "values":
                                activityValue = playerEntry.Values[CurrentActiveMission.Metadata.ValueFieldType].Basic
                                    .Value;
                                break;
                            case "extended.values":
                                activityValue = playerEntry.Extended
                                    .Values[CurrentActiveMission.Metadata.ValueFieldType].Basic.Value;
                                break;
                        }

                        if (activityValue < CurrentActiveMission.Metadata.Value) continue;

                        // Add Score
                        var newEntry = new BlitzMissionEntry(
                            playerEntry.Player.DestinyUserInfo.DisplayName,
                            membershipId,
                            activity.Data.ActivityDetails.InstanceId.ToString(),
                            CurrentActiveMission.Metadata,
                            CurrentActiveMissionStart);

                        await _mongoService.AddFinishedBlitzMission(newEntry);
                        _logger.LogInformation(
                            $"{playerEntry.Player.DestinyUserInfo.DisplayName} has successfully completed a blitz mission! ({CurrentActiveMission.Name}");
                    }
                }

                _logger.LogInformation("Checking Blitz Mission Activities done!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
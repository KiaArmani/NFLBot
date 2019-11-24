using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XurClassLibrary.Models;

namespace XurNightfaller.Services
{
    public class NightfallService
    {
        private readonly BungieService _bungieService;
        private readonly ILogger<NightfallService> _logger;
        private readonly ManifestService _manifestService;
        private readonly MongoService _mongoService;

        public NightfallService(ILogger<NightfallService> logger, IServiceProvider services)
        {
            _logger = logger;
            _mongoService = services.GetRequiredService<MongoService>();
            _bungieService = services.GetRequiredService<BungieService>();
            _manifestService = services.GetRequiredService<ManifestService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = GetNightfallsOfClanMembers();
            return Task.CompletedTask;
        }

        public async Task GetNightfallsOfClanMembers()
        {
            _logger.LogInformation("Checking Nightfalls..");

            var returnList = new List<ScoreEntry>();
            var membershipIdList = await _bungieService.GetMembershipIdListOfClanMembers();

            // Get Nightfall Activities
            var characterActivityResult = await _mongoService.GetNightfallActivities();

            foreach (var activity in characterActivityResult)
            {
                // Get Post Game Carnage Report
                var pcgr = await _bungieService.GetPostCarnageReport(activity.Data.ActivityDetails.InstanceId);
                var validRun = true;

                // API might return aborted Nightfalls, so their Score is 0. We want to ignore those.
                var nightfallScore = activity.Data.Values["teamScore"].Basic.Value;
                if (nightfallScore <= 0)
                    continue;

                foreach (var playerEntry in pcgr.Entries)
                {
                    if (playerEntry.Player.DestinyUserInfo == null)
                        continue;

                    var playerMembershipId = playerEntry.Player.DestinyUserInfo.MembershipId;
                    if (!membershipIdList.Contains(playerMembershipId))
                        validRun = false;
                }

                // If not, it's invalid
                if (!validRun)
                    continue;

                // Get the Director Hash required for finding the Nightfall Name
                var directorHash = activity.Data.ActivityDetails.DirectorActivityHash;

                // Get the Nightfall Name
                string databaseActivityName;
                var activityDisplayProperties = _manifestService.GetDisplayPropertiesForActivity(directorHash);

                if (activityDisplayProperties.Name.Contains("QUEST", StringComparison.InvariantCulture))
                    continue;

                if (activityDisplayProperties.Name.Contains("Nightfall: The Ordeal",
                    StringComparison.InvariantCulture))
                    databaseActivityName =
                        $"{activityDisplayProperties.Description} ({activityDisplayProperties.Name.Split("Ordeal: ")[1].ToUpper(CultureInfo.InvariantCulture)})";
                else
                    databaseActivityName = activityDisplayProperties.Name.Split("Nightfall: ")[1];

                var activityDate = activity.Data.Period;

                foreach (var playerEntry in pcgr.Entries)
                {
                    // If we already collected that Nightfall, skip it.
                    if (await _mongoService.CheckIfPlayerNightfallExistsInDatabase(
                        activity.Data.ActivityDetails.InstanceId, playerEntry.Player.DestinyUserInfo.MembershipId))
                        continue;

                    var name = playerEntry.Player.DestinyUserInfo.DisplayName;

                    // Create new Score Entry
                    var entry = new ScoreEntry(
                        name,
                        playerEntry.Player.DestinyUserInfo.MembershipId,
                        activity.Data.ActivityDetails.DirectorActivityHash,
                        activity.Data.ActivityDetails.InstanceId,
                        databaseActivityName,
                        activityDate,
                        nightfallScore);

                    // Insert into Collection
                    _logger.LogInformation(
                        $"Adding Score {nightfallScore} for Player {name} from {activityDate} in {databaseActivityName}");
                    returnList.Add(entry);
                }
            }

            if (returnList.Any())
                await _mongoService.AddNewScores(returnList);

            _logger.LogInformation("Checking Nightfalls done!");
            await Task.Delay(new TimeSpan(0, 10, 0)).ContinueWith(async o => { await GetNightfallsOfClanMembers(); });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BungieNet.Destiny.HistoricalStats.Definitions;

namespace XurClassLibrary.Models
{
    public static class BlitzMissionGlobals
    {
        // Blitz Missions
        public static readonly BlitzMission PvpKills = new BlitzMission(
            "PvP Kills",
            $"Land %AMOUNT% Final Blows against Guardians in any PvP Activity.",
            new BlitzMissionDatabase(
                DestinyActivityModeType.AllPvP,
                "kills",
                "values",
                new[] {6, 12},
                false,
                825));

        public static readonly BlitzMission StrikeKills = new BlitzMission(
            "Strike Kills",
            "Land %AMOUNT% Final Blows against Enemies in any Strike.",
            new BlitzMissionDatabase(
                DestinyActivityModeType.AllStrikes,
                "kills",
                "values",
                new[] {50, 85},
                false,
                825));

        public static readonly BlitzMission GambitPrimeMotes = new BlitzMission(
            "Deposit Motes",
            "Deposit %AMOUNT% Motes in Gambit Prime.",
            new BlitzMissionDatabase(
                DestinyActivityModeType.GambitPrime,
                "motesDeposited",
                "extended.values",
                new[] {10, 35},
                false,
                925));

        // Quest List
        public static readonly List<BlitzMission> BlitzMissionList = new List<BlitzMission>
        {
            PvpKills, StrikeKills, GambitPrimeMotes
        };

        // Dates
        public static DateTime GetBlitzMissionEnd(DateTime currentDate)
        {
            return currentDate.AddHours(4);
        }
    }
}
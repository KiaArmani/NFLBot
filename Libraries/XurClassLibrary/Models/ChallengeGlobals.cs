using System;
using System.Collections.Generic;
using System.Globalization;

namespace XurClassLibrary.Models
{
    public static class ChallengeGlobals
    {
        // Current Week
        public const long CurrentWeek = 1;

        // Dates
        public static readonly DateTime CurrentSeasonStart =
            Convert.ToDateTime("2020-06-09T18:00:00Z", CultureInfo.InvariantCulture);

        public static readonly DateTime CurrentChallengeWeek =
            Convert.ToDateTime("2020-06-09T18:00:00Z", CultureInfo.InvariantCulture);

        // Quests
        public static readonly WeeklyChallengeDatabase Tier1Normal =
            new WeeklyChallengeDatabase(CurrentWeek, 1, ChallengeDifficulty.Normal, false, 1635);

        public static readonly WeeklyChallengeDatabase Tier2Normal =
            new WeeklyChallengeDatabase(CurrentWeek, 2, ChallengeDifficulty.Normal, false, 2250);

        public static readonly WeeklyChallengeDatabase Tier3Normal =
            new WeeklyChallengeDatabase(CurrentWeek, 3, ChallengeDifficulty.Normal, false, 3795);

        public static readonly WeeklyChallengeDatabase Tier1Heroic =
            new WeeklyChallengeDatabase(CurrentWeek, 4, ChallengeDifficulty.Heroic, true, 2455);

        public static readonly WeeklyChallengeDatabase Tier2Heroic =
            new WeeklyChallengeDatabase(CurrentWeek, 5, ChallengeDifficulty.Heroic, true, 3679);

        public static readonly WeeklyChallengeDatabase Tier3Heroic =
            new WeeklyChallengeDatabase(CurrentWeek, 6, ChallengeDifficulty.Heroic, true, 6205);

        // Quest List
        public static readonly List<WeeklyChallengeDatabase> WeeklyChallengeList = new List<WeeklyChallengeDatabase>
        {
            Tier1Normal, Tier2Normal, Tier3Normal, Tier1Heroic, Tier2Heroic, Tier3Heroic
        };

        // Quest Details
        public static readonly List<WeeklyChallenge> WeeklyChallenges = new List<WeeklyChallenge>
        {
            new WeeklyChallenge("Alright, Alright, Alright",
                "Send 3 Large Blockers in a single game of Gambit and win the match.", Tier1Normal),
            new WeeklyChallenge("Negative Two Hour",
                "As a fireteam of two: Complete the \"Zero Hour\" Story Mission in less than 15 Minutes.",
                Tier2Normal),
            new WeeklyChallenge("Kinderguardian", "Complete any Raid with one player having zero Kills and Deaths.", Tier3Normal),
            new WeeklyChallenge("???",
                "REDACTED",
                Tier1Heroic),
            new WeeklyChallenge("???",
                "REDACTED",
                Tier2Heroic),
            new WeeklyChallenge("???",
                "REDACTED",
                Tier3Heroic)
        };
    }
}
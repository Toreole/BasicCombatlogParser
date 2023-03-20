using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;

namespace CombatlogParser
{
    public static partial class Queries
    {
        /// <summary>
        /// Attempts getting the combatlog metadata for a log based on its ID.
        /// </summary>
        /// <param name="logID"></param>
        /// <returns>null if no such log can be found.</returns>
        public static CombatlogMetadata? GetCombatlogMetadataByID(uint logId)
        {
            using CombatlogDBContext context = new();
            return context.Combatlogs.FirstOrDefault(x => x.Id == logId);
        }

        public static CombatlogMetadata?[] GetCombatlogMetadata(uint lastId, int pageSize = 10)
        {
            using CombatlogDBContext context = new();
            return context.Combatlogs
                .Where(c => c.Id > lastId)
                .OrderBy(x => x.Id)
                .Take(pageSize)
                .ToArray();
        }

        public static EncounterInfoMetadata?[] GetEncounterInfoMetadata(uint lastId, int pageSize = 10)
        {
            using CombatlogDBContext context = new();
            return context.Encounters
                .Where(c => c.Id > lastId)
                .OrderBy(x => x.Id)
                .Take(pageSize)
                .ToArray();
        }

        public static PerformanceMetadata?[] GetPerformanceMetadata(uint lastId, int pageSize = 10)
        {
            using CombatlogDBContext context = new();
            return context.Performances
                .Where(c => c.Id > lastId)
                .OrderBy(x => x.Id)
                .Take(pageSize)
                .ToArray();
        }

        public static PlayerMetadata?[] GetPlayerMetadata(uint lastId, int pageSize = 10)
        {
            using CombatlogDBContext context = new();
            return context.Players
                .Where(c => c.Id > lastId)
                .OrderBy(x => x.Id)
                .Take(pageSize)
                .ToArray();
        }

        public static PerformanceMetadata? GetHighestDpsOnEncounterForPlayer(
            uint playerId, 
            EncounterId encounter, 
            DifficultyId difficulty
            )
        {
            using CombatlogDBContext context = new();
            return context.Performances
                .Where(p => p.PlayerMetadataId == playerId 
                && p.EncounterInfoMetadata!.WowEncounterId == encounter //p.WowEncounterId == encounter ----> WOWENCOUNTERID IS UNKNOWN FOR SOME REASON!
                && p.EncounterInfoMetadata!.Success
                && p.EncounterInfoMetadata!.DifficultyId == difficulty)
                .OrderBy(p => p.Dps)
                .FirstOrDefault();
        }

        public static int GetKillCountOnEncounterForPlayer(
            uint playerId,
            EncounterId encounter,
            DifficultyId difficulty
            )
        {
            using CombatlogDBContext context = new();
            return context.Performances
                .Where(p => p.PlayerMetadataId == playerId &&
                p.EncounterInfoMetadata!.WowEncounterId == encounter
                && p.EncounterInfoMetadata!.Success
                && p.EncounterInfoMetadata!.DifficultyId == difficulty)
                .Count();
        }

        public static PerformanceMetadata? GetMedianDpsOnKill(
            uint playerId,
            EncounterId encounter,
            DifficultyId difficulty
            )
        {
            using CombatlogDBContext context = new();
            var matching = context.Performances.Where(p =>
                p.PlayerMetadataId == playerId
                && p.EncounterInfoMetadata!.WowEncounterId == encounter
                && p.EncounterInfoMetadata!.Success
                && p.EncounterInfoMetadata!.DifficultyId == difficulty
            );
            int count = matching.Count();
            var medianEntry = matching.OrderBy(p => p.Dps).Skip(count / 2).FirstOrDefault();
            return medianEntry;
        }

        public static PlayerEncounterPerformanceOverview GetPerformanceOverview(
            uint playerId,
            EncounterId encounter,
            DifficultyId difficulty
            )
        {
            using CombatlogDBContext context = new();
            var matching = context.Performances.Where(p =>
                p.PlayerMetadataId == playerId
                && p.EncounterInfoMetadata!.WowEncounterId == encounter
                && p.EncounterInfoMetadata!.Success
                && p.EncounterInfoMetadata!.DifficultyId == difficulty
            ).OrderBy(p => p.Dps);
            int count = matching.Count();
            if(count == 0)
            {
                return new()
                {
                    EncounterName = encounter.ToPrettyString() //only set encounter name, everything else is default.
                };
            }
            var medianEntry = matching.Skip(count >> 1).First();
            var bestEntry = matching.First();
            var fastest = matching.OrderBy(p => p.EncounterInfoMetadata!.EncounterDurationMS).First();
            //somehow fastest.EncounterInfoMetadata is null, so instead of bothering with figuring that out, just get it by id manually.
            var fastestEncounter = context.Encounters.Where(e => e.Id == fastest.EncounterInfoMetadataId).First();
            return new()
            {
                EncounterName = encounter.ToPrettyString(),
                FastestTime = ParsingUtil.MillisecondsToReadableTimeString((uint)fastestEncounter.EncounterDurationMS),
                HighestMetricValue = bestEntry.Dps.ToString("0.0"),
                MedianMetricValue = medianEntry.Dps.ToString("0.0"),
                KillCount = count.ToString()
            };
        }
    }
}

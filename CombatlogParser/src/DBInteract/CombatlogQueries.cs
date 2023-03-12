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
    }
}

using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;

namespace CombatlogParser
{
    /// <summary>
    /// Class that contains methods to store data in the database.
    /// </summary>
    public static class DBStore
    {
        /// <summary>
        /// Stores the combatlog, returns the assigned uID.
        /// </summary>
        public static uint StoreCombatlog(CombatlogMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            dbContext.Combatlogs.Add(data);
            dbContext.SaveChanges();
            return data.Id;
        }

        /// <summary>
        /// Stores an instance of EncounterInfoMetadata in the DB and returns the auto assigned ID
        /// </summary>
        public static uint StoreEncounter(EncounterInfoMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            dbContext.Encounters.Add(data);
            dbContext.SaveChanges();
            return data.Id;
        }

        public static uint StorePerformance(PerformanceMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            dbContext.Performances.Add(data);
            dbContext.SaveChanges();
            return data.Id;
        }

        public static void StorePlayer(PlayerMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            dbContext.Players.Add(data);
            dbContext.SaveChanges();
        }
    }
}

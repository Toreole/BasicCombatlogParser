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
        public static void StoreCombatlog(CombatlogMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            dbContext.Combatlogs.Add(data);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Stores an instance of EncounterInfoMetadata in the DB and returns the auto assigned ID
        /// </summary>
        public static void StoreEncounter(EncounterInfoMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            dbContext.Encounters.Add(data);
            dbContext.SaveChanges();
        }

        public static void StorePerformance(PerformanceMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            dbContext.Performances.Add(data);
            dbContext.SaveChanges();
        }

        public static void StorePlayer(PlayerMetadata player)
        {
            using CombatlogDBContext dbContext = new();
            PlayerMetadata? storedPlayer = dbContext.Players.FirstOrDefault(p => p.GUID == player.GUID);
            if(storedPlayer != null)
            {
                //if the player is already saved by GUID, just add missing fields if needed.
                if(string.IsNullOrEmpty(storedPlayer.Name) && !string.IsNullOrEmpty(player.Name))
                {
                    dbContext.Players.Update(storedPlayer);
                    storedPlayer.Name = player.Name;
                    storedPlayer.Realm = player.Realm;
                    dbContext.SaveChanges();
                }
            }
            else
            {
                dbContext.Players.Add(player);
                dbContext.SaveChanges();
            }

        }
    }
}

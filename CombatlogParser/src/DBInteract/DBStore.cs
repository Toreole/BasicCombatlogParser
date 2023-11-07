using CombatlogParser.Data;
using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;
using Microsoft.EntityFrameworkCore;

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
        //async for such small operations may not be necessary.
        public static async Task StoreCombatlogAsync(CombatlogMetadata data)
        {
            using CombatlogDBContext dbContext = new();
            await dbContext.Combatlogs.AddAsync(data);
            await dbContext.SaveChangesAsync();
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
			//avoid double saving the playermetadata thats referenced in here.
			dbContext.Entry(data.PlayerMetadata!).State = EntityState.Unchanged;
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Gets a PlayerMetadata by the players GUID or creates a new entity for it.
        /// </summary>
        public static PlayerMetadata GetOrCreatePlayerMetadata(PlayerInfo player)
        {
            using CombatlogDBContext dbContext = new();
            PlayerMetadata? playerMetadata = dbContext.Players.FirstOrDefault(p => p.GUID == player.GUID);
            if (playerMetadata != null)
            {
                if (playerMetadata.Name == string.Empty && player.Name != string.Empty)
                {
                    dbContext.Update(playerMetadata);
                    playerMetadata.Name = player.Name;
                    playerMetadata.Realm = player.Realm;
                    dbContext.SaveChanges();
                }
                return playerMetadata;
            }
            playerMetadata = PlayerMetadata.From(player);
            dbContext.Players.Add(playerMetadata);
            dbContext.SaveChanges();
            return playerMetadata;
        }

		/// <summary>
		/// Gets a PlayerMetadata by the players GUID or creates a new entity for it.
		/// </summary>
		public static async Task<PlayerMetadata> GetOrCreatePlayerMetadataAsync(CombatlogDBContext dbContext, PlayerInfo player)
		{
			PlayerMetadata? playerMetadata = await dbContext.Players.FirstOrDefaultAsync(p => p.GUID == player.GUID);
			if (playerMetadata != null)
			{
				if (playerMetadata.Name == string.Empty && player.Name != string.Empty)
				{
					dbContext.Update(playerMetadata);
					playerMetadata.Name = player.Name;
					playerMetadata.Realm = player.Realm;
				}
				return playerMetadata;
			}
			playerMetadata = PlayerMetadata.From(player);
			await dbContext.Players.AddAsync(playerMetadata);
			return playerMetadata;
		}

		/// <summary>
		/// Tries to store a player. If one with the same GUID already exists, returns the existing players Id.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static uint StorePlayer(PlayerMetadata player)
        {
            using CombatlogDBContext dbContext = new();
            PlayerMetadata? storedPlayer = dbContext.Players.FirstOrDefault(p => p.GUID == player.GUID);
            if (storedPlayer != null)
            {
                //if the player is already saved by GUID, just add missing fields if needed.
                if (string.IsNullOrEmpty(storedPlayer.Name) && !string.IsNullOrEmpty(player.Name))
                {
                    dbContext.Players.Update(storedPlayer);
                    storedPlayer.Name = player.Name;
                    storedPlayer.Realm = player.Realm;
                    dbContext.SaveChanges();
                }
                return storedPlayer.Id;
            }
            else
            {
                dbContext.Players.Add(player);
                dbContext.SaveChanges();
                return player.Id;
            }

        }
    }
}

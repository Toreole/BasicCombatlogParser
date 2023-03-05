using CombatlogParser.Data.Metadata;

namespace CombatlogParser
{
    public static partial class Queries
    {
        /// <summary>
        /// Gets the stored Metadata for all players whose names start with the provided string. This should be case-insenstive.
        /// </summary>
        public static PlayerMetadata[] AllPlayersWithNameLike(string start)
        {
            using var command = DB.CreateCommand();
            if (command == null)
                return Array.Empty<PlayerMetadata>();
            command.CommandText = "SELECT playerGUID, name, realm, classID FROM Player_Metadata WHERE name LIKE $s%";
            command.Parameters.AddWithValue("$s", start);

            List<PlayerMetadata> results = new(10);

            using var reader = command.ExecuteReader();
            while(reader.Read())
            {
                results.Add(new PlayerMetadata()
                {
                    GUID = reader.GetString(0),
                    Name = reader.GetString(1),
                    Realm = reader.GetString(2),
                    ClassID = reader.GetByte(3)
                });
            }
            return results.ToArray();
        }
    }
}

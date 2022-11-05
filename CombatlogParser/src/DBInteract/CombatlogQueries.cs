using CombatlogParser.Data.MetaData;

namespace CombatlogParser
{
    public static partial class Queries
    {
        /// <summary>
        /// Attempts getting the combatlog metadata for a log based on its ID.
        /// </summary>
        /// <param name="logID"></param>
        /// <returns>null if no such log can be found.</returns>
        public static CombatlogMetadata? GetCombatlogMetadataByID(uint logID)
        {
            using var command = DB.CreateCommand();
            if(command == null)
                return null;
            //very simple SELECT query
            command.CommandText = @"
                SELECT fileName, timestamp, isAdvanced, buildVersion, projectID 
                FROM Combatlog_Metadata
                WHERE log_ID = $id
                ";
            command.Parameters.AddWithValue("$id", logID);

            using var reader = command.ExecuteReader();
            if(reader.Read())
            {
                return new CombatlogMetadata()
                {
                    uID = logID,
                    fileName = reader.GetString(0),
                    msTimeStamp = reader.GetInt64(1),
                    isAdvanced = reader.GetBoolean(2),
                    buildVersion = reader.GetString(3),
                    projectID = (WowProjectID)reader.GetInt32(4)
                };
            }
            return null;
        }

        /// <summary>
        /// Gets the number of rows in the Combatlog_Metadata table.
        /// </summary>
        public static int GetCombatlogMetadataCount()
        {
            using var command = DB.CreateCommand();
            if (command == null)
                return 0;
            command.CommandText = "SELECT count(*) FROM Combatlog_Metadata";
            return (int)command.ExecuteScalar()!; //cant be null. if DB is initialized, the table will exist. worst case it returns 0.
        }
    }
}

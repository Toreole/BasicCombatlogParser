using CombatlogParser.Data.Metadata;

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
                    Id = logID,
                    FileName = reader.GetString(0),
                    MsTimeStamp = reader.GetInt64(1),
                    IsAdvanced = reader.GetBoolean(2),
                    BuildVersion = reader.GetString(3),
                    ProjectID = (WowProjectID)reader.GetInt32(4)
                };
            }
            return null;
        }

        /// <summary>
        /// Checks whether a combatlog has already been imported by confirming whether a log with the same fileName exists in the metadata table.
        /// </summary>
        public static bool CombatlogAlreadyImported(string fileName)
        {
            using var command = DB.CreateCommand();
            if (command == null)
                return false;
            command.CommandText = "SELECT count(*) FROM Combatlog_Metadata WHERE fileName = $fn";
            command.Parameters.AddWithValue("$fn", fileName);
            return (int)command.ExecuteScalar()! is 1;
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

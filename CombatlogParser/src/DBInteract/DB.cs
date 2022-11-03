using CombatlogParser.Data;
using Microsoft.Data.Sqlite;

namespace CombatlogParser
{
    /// <summary>
    /// DB (DataBase) is a static type used for accessing the SQLite database.
    /// </summary>
    public static class DB
    {
        private static SqliteConnection? connection = null;

        public static void InitializeConnection()
        {
            SQLitePCL.Batteries.Init();
            //the "arguments" for the connection string are seperated with ;
            connection = new("DataSource=hello.db; Mode=ReadWriteCreate");
            connection.Open();
        }

        public static void Shutdown()
        {
            connection?.Close();
            connection?.Dispose();
            connection = null;
        }

        public static SqliteCommand? CreateCommand()
        {
            return connection?.CreateCommand();
        }

        /// <summary>
        /// Creates any data tables that dont exist.
        /// </summary>
        public static void SetupDataTables()
        {
            if (connection == null)
                return;
            SqliteCommand command = connection.CreateCommand();

            //1. create CombatlogMetadata table
            command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Combatlog_Metadata (
                        log_ID INTEGER PRIMARY KEY AUTOINCREMENT,
                        fileName TEXT UNIQUE
                        timestamp INTEGER
                        isAdvanced INTEGER
                        buildVersion TEXT
                        projectID INTEGER
                        );
                ";
            command.ExecuteNonQuery();

            //2. create EncounterMetadata table
            //3. create PerformanceMetadata table
            //4. create PlayerMetadata table
        }

    }
}

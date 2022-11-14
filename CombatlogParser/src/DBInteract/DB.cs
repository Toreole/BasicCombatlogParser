using CombatlogParser.Data;
using Microsoft.Data.Sqlite;
using System.Text;

namespace CombatlogParser
{
    /// <summary>
    /// DB (DataBase) is a static type used for accessing the SQLite database.
    /// </summary>
    public static class DB
    {
        private static SqliteConnection? connection = null;
        private const string tableCreationCommandTemplate = "CREATE TABLE IF NOT EXISTS $table;";

        private static readonly DBSchema schema
            = new(
                new DBTable[]{
                    new DBTable("Combatlog_Metadata", new DBColumn[]
                    {
                        new("log_ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                        new("fileName", "TEXT UNIQUE"),
                        new("timestamp", "INTEGER"),
                        new("isAdvanced", "INTEGER"),
                        new("buildVersion", "TEXT"),
                        new("projectID", "INTEGER")
                    }),

                    new DBTable("Encounter_Metadata", new DBColumn[]
                    { 
                        new("encounter_ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                        new("sourceLog_ID", "INTEGER"),
                        new("startPosition", "INTEGER"),
                        new("wow_encounterID", "INTEGER"),
                        new("success", "INTEGER"),
                        new("difficultyID", "INTEGER"),
                        new("encounterLength", "INTEGER")
                    }),

                    new DBTable("Performance_Metadata", new DBColumn[]
                    { 
                        new("performance_ID", "INTEGER PRIMARY KEY AUTOINCREMENT"),
                        new("playerGUID", "TEXT"),
                        new("dps", "REAL"),
                        new("hps", "REAL"),
                        new("roleID", "INTEGER"),
                        new("specID", "INTEGER"),
                        new("encounterUID", "INTEGER")
                    }),

                    new DBTable("Player_Metadata", new DBColumn[]
                    {
                         new("playerGUID", "TEXT UNIQUE PRIMARY KEY"),
                         new("name", "TEXT"),
                         new("realm", "TEXT"),
                         new("classID", "INTEGER")
                    }),

                    new DBTable("Wow_Encounter_Info", new DBColumn[]
                    {
                        new("wowEncounterID", "INTEGER UNIQUE PRIMARY KEY"),
                        new("localName", "TEXT"),
                        new("zoneName", "TEXT")
                    })
                });

        public static void InitializeConnection()
        {
            SQLitePCL.Batteries.Init();
            //the "arguments" for the connection string are seperated with ;
            connection = new("DataSource=CombatlogMetadata.db; Mode=ReadWriteCreate");
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
            using SqliteCommand command = connection.CreateCommand();

            //create all tables
            foreach (var table in schema.tables)
            {
                command.CommandText = tableCreationCommandTemplate.Replace("$table", table.ToTableCreationString());
                command.ExecuteNonQuery();
            }

        }

        /// <summary>
        /// Is ran when a change in app version is detected. <br/>
        /// This will create any tables that dont yet exist, and add all required rows to those tables.
        /// </summary>
        public static void Upgrade()
        {
            using var command = CreateCommand();
            if (command is null)
                return;
            foreach(var table in schema.tables)
            {
                string template = $"ALTER TABLE {table.name} ADD COLUMN ";
                foreach(var column in table.columns)
                {
                    command.CommandText = template + column.ToString();
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch(SqliteException ex)
                    {

                    }
                    finally { } //dont really care about any exceptions that occur here.
                }
            }
        }

        public class DBSchema
        {
            public DBTable[] tables = Array.Empty<DBTable>();

            public DBSchema(DBTable[] tables)
                => this.tables = tables;
        }

        public class DBTable
        {
            public string name = "";
            public DBColumn[] columns = Array.Empty<DBColumn>();

            public DBTable(string name, DBColumn[] columns)
            {
                this.name = name;
                this.columns = columns;
            }

            /// <summary>
            /// Returns a string that follows the CREATE TABLE definition of a table, Example: <br/>
            /// tableName ( col1 TYPE CONSTRAINT, col2 TYPE CONSTRAINT ) <br/>
            /// this needs to be inserted into a "CREATE TABLE IF NOT EXISTS [...];" command.
            /// </summary>
            /// <returns></returns>
            public string ToTableCreationString()
            {
                StringBuilder strb = new();
                strb.Append(name);
                strb.Append(" ( ");
                if(columns.Length > 0)
                {
                    strb.Append(columns[0].ToString());
                    for (int i = 1; i < columns.Length; i++)
                        strb.Append($", {columns[i].ToString()}");
                }
                strb.Append(" )");
                return strb.ToString();
            }
        }

        public class DBColumn
        {
            public string name = "";
            public string typeAndConstraints = "";

            public DBColumn(string name, string typeAndConstraints)
            {
                this.name = name;
                this.typeAndConstraints = typeAndConstraints;
            }

            public override string ToString() => $"{name} {typeAndConstraints}";
        }

    }
}

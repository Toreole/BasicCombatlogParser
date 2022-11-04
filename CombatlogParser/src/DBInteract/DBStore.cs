using CombatlogParser.Data.MetaData;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace CombatlogParser
{
    public static class DBStore
    {
        /// <summary>
        /// Stores the combatlog, returns the assigned uID.
        /// </summary>
        /// <param name="data">The metadata you want to store.</param>
        /// <returns></returns>
        public static int StoreCombatlog(CombatlogMetadata data)
        {
            var command = DB.CreateCommand();
            if (command is null) //if the DB connection isnt established, this will be null. terminate before.
                return -1;

            command.CommandText =
                @" INSERT INTO Combatlog_Metadata 
                       ( fileName, timestamp, isAdvanced, buildVersion, projectID )
                   VALUES ( %fn, %time, %adv, %build, %project )
                   ON CONFLICT ABORT";
            //insert the parameters
            var args = command.Parameters;
            args.AddWithValue("%fn", data.fileName);
            args.AddWithValue("%time", data.msTimeStamp);
            args.AddWithValue("%adv", data.isAdvanced? 1 : 0);
            args.AddWithValue("%build", data.buildVersion);
            args.AddWithValue("%project", (int)data.projectID);
            //try because the insert command will raise an error given a duplicate fileName
            try
            {
                command.ExecuteNonQuery();
                var com2 = DB.CreateCommand()!; //is never null here
                //the INTEGER PRIMARY KEY log_ID is an alias for the ROWID, so I can use the SQL function.
                com2.CommandText = $"SELECT last_insert_rowid()"; 
                //given that the insert command executes without errors, this can never return null.
                return (int)com2.ExecuteScalar()!;
            }
            catch (SqliteException exception)
            {
                Debug.WriteLine(exception.Message);
                return -1;
            }
        }
    }
}

using CombatlogParser.Data.Metadata;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

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
        public static int StoreCombatlog(CombatlogMetadata data)
        {
            var command = DB.CreateCommand();
            if (command is null) //if the DB connection isnt established, this will be null. terminate before.
                return -1;

            command.CommandText =
                @" INSERT INTO Combatlog_Metadata 
                       ( fileName, timestamp, isAdvanced, buildVersion, projectID )
                   VALUES ( $fn, $time, $adv, $build, $project )
                   ON CONFLICT ABORT";
            //insert the parameters
            var args = command.Parameters;
            args.AddWithValue("$fn", data.fileName);
            args.AddWithValue("$time", data.msTimeStamp);
            args.AddWithValue("$adv", data.isAdvanced? 1 : 0);
            args.AddWithValue("$build", data.buildVersion);
            args.AddWithValue("$project", (int)data.projectID);
            //try because the insert command will raise an error given a duplicate fileName
            try
            {
                command.ExecuteNonQuery();
                var com2 = DB.CreateCommand()!; //is never null here
                //the INTEGER PRIMARY KEY log_ID is an alias for the ROWID, so I can use the SQL function.
                com2.CommandText = "SELECT last_insert_rowid()"; 
                //given that the insert command executes without errors, this can never return null.
                return (int)com2.ExecuteScalar()!;
            }
            catch (SqliteException exception)
            {
                Debug.WriteLine(exception.Message);
                return -1;
            }
        }

        /// <summary>
        /// Stores an instance of EncounterInfoMetadata in the DB and returns the auto assigned ID
        /// </summary>
        public static int StoreEncounter(EncounterInfoMetadata data)
        {
            var command = DB.CreateCommand();
            if (command is null) //if the DB connection isnt established, this will be null. terminate before.
                return -1;

            command.CommandText =
                @" INSERT INTO Combatlog_Metadata 
                       ( sourceLog_ID, startPosition, wow_encounterID, success, difficultyID )
                   VALUES ( $log, $start, $wowEncID, $success, $difficulty )
                   ON CONFLICT ABORT";
            //insert the parameters
            var args = command.Parameters;
            args.AddWithValue("$log", data.logID);
            args.AddWithValue("$start", data.encounterStartIndex);
            args.AddWithValue("$wowEncID", data.wowEncounterID);
            args.AddWithValue("$success", data.success);
            args.AddWithValue("$difficulty", data.difficultyID);
            //try because the insert command will raise an error given a duplicate fileName
            try
            {
                command.ExecuteNonQuery();
                var com2 = DB.CreateCommand()!; //is never null here
                //the INTEGER PRIMARY KEY log_ID is an alias for the ROWID, so I can use the SQL function.
                com2.CommandText = "SELECT last_insert_rowid()";
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

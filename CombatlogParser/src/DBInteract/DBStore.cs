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
            args.AddWithValue("$fn", data.FileName);
            args.AddWithValue("$time", data.MsTimeStamp);
            args.AddWithValue("$adv", data.IsAdvanced? 1 : 0);
            args.AddWithValue("$build", data.BuildVersion);
            args.AddWithValue("$project", (int)data.ProjectID);
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
                       ( sourceLog_ID, startPosition, wow_encounterID, success, difficultyID, encounterLength )
                   VALUES ( $log, $start, $wowEncID, $success, $difficulty, $length )
                   ON CONFLICT ABORT";
            //insert the parameters
            var args = command.Parameters;
            args.AddWithValue("$log", data.CombatlogMetadataId);
            args.AddWithValue("$start", data.EncounterStartIndex);
            args.AddWithValue("$wowEncID", data.WowEncounterID);
            args.AddWithValue("$success", data.Success);
            args.AddWithValue("$difficulty", data.DifficultyID);
            args.AddWithValue("$length", data.EncounterLengthInFile);
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

        public static int StorePerformance(PerformanceMetadata data)
        {
            var command = DB.CreateCommand();
            if (command is null) //if the DB connection isnt established, this will be null. terminate before.
                return -1;

            command.CommandText =
                @" INSERT INTO Performance_Metadata 
                       ( playerGUID, dps, hps, roleID, specID, encounterUID )
                   VALUES ( $playerGUID, $dps, $hps, $roleID, $specID, $encUID )";
            //insert the parameters
            var args = command.Parameters;
            args.AddWithValue("$playerGUID", data.PlayerGUID);
            args.AddWithValue("$dps", data.Dps);
            args.AddWithValue("$hps", data.Hps);
            args.AddWithValue("$roleID", data.RoleId);
            args.AddWithValue("$specID", data.SpecID);
            args.AddWithValue("encUID", data.EncounterID);
            //try because the insert command will raise an error given a duplicate
            try
            {
                command.ExecuteNonQuery();
                var com2 = DB.CreateCommand()!; //is never null here
                //the INTEGER PRIMARY KEY performance_ID is an alias for the ROWID, so I can use the SQL function.
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

        public static void StorePlayer(PlayerMetadata data)
        {
            var command = DB.CreateCommand();
            if (command is null) //if the DB connection isnt established, this will be null. terminate before.
                return;

            command.CommandText =
                @" INSERT INTO Player_Metadata 
                       ( playerGUID, name, realm, classID )
                   VALUES ( $playerGUID, $name, $realm, $classID )
                   ON CONFLICT ABORT";

            //insert the parameters
            var args = command.Parameters;
            args.AddWithValue("$playerGUID", data.GUID);
            args.AddWithValue("$name", data.Name);
            args.AddWithValue("$realm", data.Realm);
            args.AddWithValue("$classID", data.ClassID);
            //try because the insert command will raise an error given a duplicate GUID
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
    }
}

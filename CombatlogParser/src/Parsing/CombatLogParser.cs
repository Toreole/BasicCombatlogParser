using System.IO;
using System.Text.RegularExpressions;
using CombatlogParser.Data;
using CombatlogParser.Data.Metadata;

namespace CombatlogParser
{
    public static class CombatLogParser
    {
        public static void ImportCombatlog(string filePath)
        {
            System.Windows.Controls.ProgressBar progressBar = new();
            using FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long position = fileStream.Position; //should be 0 here. or well the start of the file at least.

            //just the name of the file.
            string fileName = Path.GetFileName(filePath);

            //create a text reader for the file
            using TextReader reader = new StreamReader(fileStream);

            string line = reader.ReadLine()!;

            //the metadata for the entire file
            CombatlogMetadata logMetadata = new();
            {
                Regex regex = new("([A-Z0-9._ /: ]+)");
                var matches = regex.Matches(line);
                //logMetadata.logVersion = int.Parse(matches[1].Value);
                logMetadata.isAdvanced   = matches[3].Value == "1";
                logMetadata.buildVersion = matches[5].Value;
                logMetadata.projectID    = (WowProjectID)int.Parse(matches[7].Value);
                logMetadata.fileName     = fileName;

                DateTime dtTimestamp = ParsingUtil.StringTimestampToDateTime(line[..18]);
                DateTimeOffset offset = new(dtTimestamp);
                logMetadata.msTimeStamp = offset.ToUnixTimeMilliseconds();
            }
            //store the log in the db and cache the ID
            int logID = DBStore.StoreCombatlog(logMetadata);

            //update the position
            position = fileStream.Position;
            string? next = reader.ReadLine();

            List<EncounterInfoMetadata> encounterMetadatas = new();
            //Step 1: Read the entire file, mark the start (and end) of encounters.
            {
                //single variable encounterMetadata is not needed outside of this scope.
                EncounterInfoMetadata? encounterMetadata = null;
                int length = 0;
                while (next != null)
                {
                    int i = 20;
                    if (next.ContainsSubstringAt("ENCOUNTER_START", 20))
                    {
                        length = 1;
                        ParsingUtil.MovePastNextDivisor(next, ref i); //move past ENCOUNTER_START,
                        encounterMetadata = new EncounterInfoMetadata()
                        {
                            logID = (uint)logID,
                            encounterStartIndex = (ulong)position
                        };
                        encounterMetadata.wowEncounterID = uint.Parse(ParsingUtil.NextSubstring(next, ref i));
                        ParsingUtil.MovePastNextDivisor(next, ref i); //skip past the name of the encounter.
                        encounterMetadata.difficultyID = int.Parse(ParsingUtil.NextSubstring(next, ref i));
                    }
                    else if (next.ContainsSubstringAt("ENCOUNTER_END", 20))
                    {
                        if (encounterMetadata != null)
                        {
                            for (int j = 0; j < 4; j++) //skip past encounterID, encounterName, difficultyID, groupSize
                                ParsingUtil.MovePastNextDivisor(line, ref i);
                            encounterMetadata.success = ParsingUtil.NextSubstring(line, ref i) == "1";
                            int encID = DBStore.StoreEncounter(encounterMetadata); //add it to the db
                            encounterMetadata.encounterInfoUID = (uint)encID;
                            encounterMetadata.encounterLength = length;
                            encounterMetadatas.Add(encounterMetadata); //also add it to the list.
                        }
                    }
                    else
                        length++;

                    next = reader.ReadLine();
                    //update the position
                    position = fileStream.Position;
                }
            }

            //Step 2: Create Tasks for all the seperate Encounters it found in the previous step
            //Process each one individually, wait for them all to finish, get the results.
            Task<EncounterInfo>[] parseTasks = new Task<EncounterInfo>[encounterMetadatas.Count];
            
            for(int i = 0; i < parseTasks.Length; i++)
                parseTasks[i] = Task.Run(() => ParseEncounter(encounterMetadatas[i], filePath, logMetadata.isAdvanced));
            //wait for all parses to finish.
            Task.WaitAll(parseTasks);

            //Step 3: Store relevant results in the DB, clean up, provide access to the results to the main window
            //so the app can display it all to the user.

        }

        //NOTE: I wonder if i should split each encounter into a seperate file, rather than keeping them all connected hmmm...
        /// <summary>
        /// Reads an Encounter from a combatlog file.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="filePath"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        private static EncounterInfo ParseEncounter(EncounterInfoMetadata metadata, string filePath, bool advanced)
        {
            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using TextReader reader = new StreamReader(fileStream);

            fileStream.Position = (long)metadata.encounterStartIndex;
            //encounterLength is guaranteed to be more or equal to the amount of actual combat events in the encounter.
            List<CombatlogEvent> events = new (metadata.encounterLength);

            string? line;
            while((line = reader.ReadLine()) != null)
            {
                int pos = 20;
                if (line.ContainsSubstringAt("ENCOUNTER_END", pos)) //the end of the encounter.
                    break;

                string subevent = ParsingUtil.NextSubstring(line, ref pos);
                //confirm that the subevent is a combatlog event (it has a prefix and a suffix)
                //other events are to be handled differently.
                if(ParsingUtil.TryParsePrefixAffixSubeventF(subevent, out var prefix, out var suffix))
                {
                    string sourceGUID = ParsingUtil.NextSubstring(line, ref pos);
                    string sourceName;
                    if (prefix == CombatlogEventPrefix.ENVIRONMENTAL || sourceGUID == "0000000000000000")
                    {
                        sourceName = "Environment";
                        ParsingUtil.MovePastNextDivisor(line, ref pos);
                    }
                    else
                    {
                        sourceName = ParsingUtil.NextSubstring(line, ref pos);
                    }

                    //source flags.
                    var sourceFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref pos));
                    var sourceRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref pos));

                    //targetGUID and name
                    var targetGUID = ParsingUtil.NextSubstring(line, ref pos);
                    var targetName = ParsingUtil.NextSubstring(line, ref pos);

                    //target flags
                    var targetFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref pos));
                    var targetRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref pos));

                    //at this point Prefix params can be handled (if any)
                    int prefixAmount = ParsingUtil.GetPrefixParamAmount(prefix);
                    string[] prefixData = new string[prefixAmount];

                    for (int j = 0; j < prefixAmount; j++)
                    {
                        prefixData[j] = ParsingUtil.NextSubstring(line, ref pos);
                    }

                    //then follow the advanced combatlog params
                    //watch out! not all events have the advanced params!
                    bool hasAdv = advanced && ParsingUtil.SubeventContainsAdvancedParams(suffix);
                    string[] advancedData = hasAdv ? new string[17] : Array.Empty<string>();
                    if (hasAdv)
                        for (int j = 0; j < 17; j++)
                            advancedData[j] = ParsingUtil.NextSubstring(line, ref pos);

                    //lastly, suffix event params.
                    int suffixAmount = ParsingUtil.GetSuffixParamAmount(suffix);
                    string[] suffixData = new string[suffixAmount];
                    for (int j = 0; j < suffixAmount; j++)
                    {
                        suffixData[j] = ParsingUtil.NextSubstring(line, ref pos);
                    }

                    CombatlogEvent clevent = new(prefix, prefixData, suffix, suffixData, advancedData)
                    {
                        SourceGUID = sourceGUID,
                        SourceName = sourceName,
                        SourceFlags = sourceFlags,
                        SourceRaidFlags = sourceRaidFlags,
                        TargetGUID = targetGUID,
                        TargetName = targetName,
                        TargetFlags = targetFlags,
                        TargetRaidFlags = targetRaidFlags
                    };
                    events.Add(clevent);
                }
            }

            return new EncounterInfo()
            {
                CombatlogEvents = events.ToArray(),
                EncounterStartTime = /*GET THIS FROM FIRST LINE*/,
                EncounterSuccess = metadata.success,
                DifficultyID = metadata.difficultyID,
                EncounterID = metadata.wowEncounterID,
                EncounterName = /*GET THIS FROM FIRST LINE*/,
                GroupSize = /*GET THIS FROM FIRST LINE*/,
                EncounterDuration = /*GET THIS FROM LAST LINE*/,
                EncounterEndTime = /*GET THIS FROM LAST LINE*/,
                Players = /*GET THIS FROM COMBATANT_INFO EVENTS*/
            };
        }

        [Obsolete("Should no longer use ReadCombatlogFile, use ImportCombatlog instead.")]
        public static Combatlog ReadCombatlogFile(string fileName)
        {
            if (File.Exists(fileName) == false)
                throw new FileNotFoundException(fileName);

            //init combatlog
            Combatlog combatlog = new();

            //simple using statements are new to me tbh, cool stuff.
            using FileStream file = File.OpenRead(fileName);
            using TextReader reader = new StreamReader(file);
            if (file.Length == 0)
                throw new EndOfStreamException($"File {fileName} was empty.");

            //the first line, and reused string var. if the file is not empty, the first line is guaranteed to exist.
            string? line = reader.ReadLine()!;
            try
            {
                combatlog.LogStartTimestamp = ParsingUtil.StringTimestampToDateTime(line[..18]);
                int i = 20;
                //skip COMBAT_LOG_VERSION
                ParsingUtil.MovePastNextDivisor(line, ref i);
                combatlog.CombatlogVersion = int.Parse(ParsingUtil.NextSubstring(line, ref i));
                //skip ADVANCED_LOG_ENABLED
                ParsingUtil.MovePastNextDivisor(line, ref i);
                combatlog.AdvancedLogEnabled = ParsingUtil.NextSubstring(line, ref i) == "1";
                //skip BUILD_VERSION
                ParsingUtil.MovePastNextDivisor(line, ref i);
                combatlog.BuildVersion = ParsingUtil.NextSubstring(line, ref i);
                //skip PROJECT_ID
                ParsingUtil.MovePastNextDivisor(line, ref i);
                combatlog.ProjectID = int.Parse(ParsingUtil.NextSubstring(line, ref i));
            }
            catch //IndexOutOfRange 
            {
                return combatlog;
            }

            //set up encounterInfo List.
            List<EncounterInfo> encounters = new(0);
            //blank encounterInfo to start with. may not be used.
            EncounterInfo? currentEncounter = null;
            //2000 is a reasonably small estimate of how many events are in a single encounter
            List<CombatlogEvent> encounterEvents = new(2000); 

            while ((line = reader.ReadLine()) != null)
            {
                int i = 20;
                string sub = ParsingUtil.NextSubstring(line, ref i);

                var timestamp = ParsingUtil.StringTimestampToDateTime(line[..18]);

                if (Enum.TryParse(sub, out CombatlogMiscEvents miscEvent))//should be a misc event.
                {
                    //encounter start.
                    if(miscEvent == CombatlogMiscEvents.ENCOUNTER_START)
                    {
                        //clear it in here aswell just to make sure.
                        encounterEvents.Clear();
                        currentEncounter = new()
                        {
                            EncounterStartTime = timestamp,
                            EncounterID = uint.Parse(ParsingUtil.NextSubstring(line, ref i)),
                            EncounterName = ParsingUtil.NextSubstring(line, ref i),
                            DifficultyID = int.Parse(ParsingUtil.NextSubstring(line, ref i)),
                            GroupSize = int.Parse(ParsingUtil.NextSubstring(line, ref i)),
                            InstanceID = uint.Parse(ParsingUtil.NextSubstring(line, ref i))
                        };
                    }
                    //encounter end.
                    else if(miscEvent == CombatlogMiscEvents.ENCOUNTER_END && currentEncounter != null)
                    {
                        for (int j = 0; j < 4; j++) //skip past encounterID, encounterName, difficultyID, groupSize
                            ParsingUtil.MovePastNextDivisor(line, ref i);
                        currentEncounter.EncounterSuccess = ParsingUtil.NextSubstring(line, ref i) == "1";
                        currentEncounter.EncounterDuration = uint.Parse(ParsingUtil.NextSubstring(line, ref i));
                        //trim the encounterEvents list, and assign it as array to the currentEncounter.
                        encounterEvents.TrimExcess();
                        currentEncounter.CombatlogEvents = encounterEvents.ToArray();
                        currentEncounter.EncounterEndTime = timestamp;

                        //double check events from the end of the list, to eliminate any that arent within the encounter.
                        for (int x = encounterEvents.Count - 1; x > 0; x--)
                        {
                            if (encounterEvents[x].Timestamp < currentEncounter.EncounterEndTime) //naive implementation
                            {
                                encounterEvents.RemoveRange(x + 1, encounterEvents.Count - x - 1);
                                break;
                            }
                        }
                        //clear it for re-use.
                        encounterEvents.Clear();
                        //add the finished encounter to the list of encounters.
                        encounters.Add(currentEncounter);
                    }
                    //other misc events. - currently discarded
                    //events.Add(clevent); 
                    //clevent.PrefixParams = new object[] { miscEvent };
                    
                    //PARTY_KILL:         guid, name, flag, flag, guid, name, flag, flag, unconscious? <- party killed an enemy?
                    //UNIT_DIED:      whatGUID, name, flag, flag, GUID, name, flag, flag, unconscious? <- regular creatures
                    //UNIT_DESTROYED: whatGUID, name, flag, flag, GUID, name, flag, flag, unconscious? <- for pets (ghouls for example)
                }
                //try parsing the substring to a CombatlogSubevent.
                else if (currentEncounter != null && 
                    timestamp > currentEncounter.EncounterStartTime && //only include events that happen *after* start of encounter.
                    ParsingUtil.TryParsePrefixAffixSubeventF(sub, out CombatlogEventPrefix cPrefix, out CombatlogEventSuffix cSuffix))
                {
                    
                    //sourceGUID and name
                    string sourceGUID = ParsingUtil.NextSubstring(line, ref i);
                    string sourceName;
                    if (cPrefix == CombatlogEventPrefix.ENVIRONMENTAL || sourceGUID == "0000000000000000")
                    {
                        sourceName = "Environment";
                        ParsingUtil.MovePastNextDivisor(line, ref i);
                    }
                    else
                    {
                        sourceName = ParsingUtil.NextSubstring(line, ref i);
                    }

                    //source flags.
                    var sourceFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));
                    var sourceRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));

                    //targetGUID and name
                    var targetGUID = ParsingUtil.NextSubstring(line, ref i);
                    var targetName = ParsingUtil.NextSubstring(line, ref i);

                    //target flags
                    var targetFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));
                    var targetRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));

                    //at this point Prefix params can be handled (if any)
                    int prefixAmount = ParsingUtil.GetPrefixParamAmount(cPrefix);
                    string[] prefixData = new string[prefixAmount];
                    
                    for (int j = 0; j < prefixAmount; j++)
                    {
                        prefixData[j] = ParsingUtil.NextSubstring(line, ref i);
                    }

                    //then follow the advanced combatlog params
                    //watch out! not all events have the advanced params!
                    bool hasAdv = combatlog.AdvancedLogEnabled && ParsingUtil.SubeventContainsAdvancedParams(cSuffix);
                    string[] advancedData = hasAdv ? new string[17] : Array.Empty<string>();
                    if (hasAdv)
                        for (int j = 0; j < 17; j++)
                            advancedData[j] = ParsingUtil.NextSubstring(line, ref i);

                    //lastly, suffix event params.
                    int suffixAmount = ParsingUtil.GetSuffixParamAmount(cSuffix);
                    string[] suffixData = new string[suffixAmount];
                    for (int j = 0; j < suffixAmount; j++)
                    {
                        suffixData[j] = ParsingUtil.NextSubstring(line, ref i);
                    }

                    CombatlogEvent clevent = new(cPrefix, prefixData, cSuffix, suffixData, advancedData)
                    { 
                        SourceGUID = sourceGUID,
                        SourceName = sourceName,
                        SourceFlags = sourceFlags,
                        SourceRaidFlags = sourceRaidFlags,
                        TargetGUID = targetGUID,
                        TargetName = targetName,
                        TargetFlags = targetFlags,
                        TargetRaidFlags = targetRaidFlags
                    };
                    encounterEvents.Add(clevent);
                    //SPELL_INSTAKILL is part of the regular combatlogevents.
                    //SPELL_INSTAKILL: guid, name, flag, flag, guid, name, flag, flag, [unknown], [unknown], unconscious? <- Unsure what this is.
                }
            }
            combatlog.Encounters = encounters.ToArray();
            return combatlog;
        }
    }
}

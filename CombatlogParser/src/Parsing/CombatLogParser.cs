using System.IO;
using System.Text.RegularExpressions;
using CombatlogParser.Data;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Metadata;
using CombatlogParser.Parsing;

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
                            length++;
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
            Task<EncounterInfo>[] parseTasks = new Task<EncounterInfo>[encounterMetadatas.Count];
            for (int i = 0; i < parseTasks.Length; i++)
                _ = ParseEncounter(encounterMetadatas[i], filePath, logMetadata.isAdvanced);
            for(int i = 0; i < parseTasks.Length; i++)
                parseTasks[i] = Task.Run(() => ParseEncounter(encounterMetadatas[i], filePath, logMetadata.isAdvanced));
            //wait for all parses to finish.
            Task.WaitAll(parseTasks);

            //NOTE: There is a potential out of memory issue in here if the log file is enormous
            //but i hope that an average file is at max 1 GB in memory.

            //Step 3: Process each encounter. This is for general HPS/DPS statistics
            Task<PerformanceMetadata[]>[] processTasks = new Task<PerformanceMetadata[]>[encounterMetadatas.Count];
            for (int i = 0; i < processTasks.Length; i++)
                processTasks[i] = Task.Run(() => ProcessPerformances(parseTasks[i].Result, encounterMetadatas[i].encounterInfoUID, logMetadata.isAdvanced));
            Task.WaitAll(processTasks);

            //Step 4: Store relevant results in the DB, clean up, provide access to the results to the main window
            //so the app can display it all to the user.
            foreach(var arrTask in processTasks)
            {
                foreach(var performance in arrTask.Result)
                {
                    performance.performanceUID = (uint)DBStore.StorePerformance(performance);
                }
            }
            var uniquePlayers = new List<PlayerMetadata>();
            foreach(var encounter in parseTasks)
            {
                foreach(var pi in encounter.Result.Players)
                    if(uniquePlayers.Exists(x => x.GUID == pi.GUID) == false)
                    {
                        uniquePlayers.Add(new()
                        {
                            GUID = pi.GUID,
                            name = pi.Name,
                            realm = pi.Realm,
                            //classID = ... //--TODO
                        });
                    }
            }
            foreach (var p in uniquePlayers)
                DBStore.StorePlayer(p);
        }

        //NOTE: I wonder if i should split each encounter into a seperate file, rather than keeping them all connected hmmm...
        /// <summary>
        /// Reads an Encounter from a combatlog file.
        /// </summary>
        /// <param name="metadata">metadata about the encounter.</param>
        /// <param name="filePath">The full file path to the combatlog.txt</param>
        /// <param name="advanced">Whether the log is using advanced parameters.</param>
        /// <returns></returns>
        private static EncounterInfo ParseEncounter(EncounterInfoMetadata metadata, string filePath, bool advanced)
        {
            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using TextReader reader = new StreamReader(fileStream);

            fileStream.Position = (long)metadata.encounterStartIndex;
            //encounterLength is guaranteed to be more or equal to the amount of actual combat events in the encounter.
            List<CombatlogEvent> events = new (metadata.encounterLength);
            List<MiscEvent> miscEvents = new(100); //not expecting a whole lot tbh.

            //data to be inserted into the resulting EncounterInfo later
            DateTime encStartT = DateTime.Now;
            //uint wowEncID = 0;
            string encName = "";
            //int diffID = 0;
            int grpSize = 0;
            uint wowInstID = 0;
            uint encDurationMS = 0;
            DateTime encEndT = DateTime.Now;


            string? line = reader.ReadLine()!;
            //double check the encounter start 
            if(line.ContainsSubstringAt("ENCOUNTER_START", 20))
            {
                encStartT = ParsingUtil.StringTimestampToDateTime(line[..18]);
                int i = 20;
                ParsingUtil.MovePastNextDivisor(line, ref i);
                //wowEncID = uint.Parse(ParsingUtil.NextSubstring(line, ref i));
                ParsingUtil.MovePastNextDivisor(line, ref i); //redundant, skip.
                encName = ParsingUtil.NextSubstring(line, ref i);
                //diffID = int.Parse(ParsingUtil.NextSubstring(line, ref i)); //redundant, skip.
                ParsingUtil.MovePastNextDivisor(line, ref i);
                grpSize = int.Parse(ParsingUtil.NextSubstring(line, ref i));
                wowInstID = uint.Parse(ParsingUtil.NextSubstring(line, ref i));
            }

            //TODO for the future: grab PlayerInfo[] from the COMBATANT_INFO events that are fired right after 
            //ENCOUNTER_START and their amount should equal the grpSize.
            
            //read all the events during the encounter.
            for(int l = 1; l < metadata.encounterLength-1; l++) 
            {
                line = reader.ReadLine()!;
                int pos = 20;
                var timestamp = ParsingUtil.StringTimestampToDateTime(line[..18]);

                string subevent = ParsingUtil.NextSubstring(line, ref pos);
                //confirm that the subevent is a combatlog event (it has a prefix and a suffix)
                //other events are to be handled differently.
                if(ParsingUtil.TryParsePrefixAffixSubeventF(subevent, out var prefix, out var suffix))
                {
                    CombatlogEvent? clevent = CombatlogEvent.Create(line);
                    if(clevent != null)
                        events.Add(clevent);
                }
                //TODO: Other events should also be handled.
                else if(ParsingUtil.TryParseMiscEventF(subevent, out var ev))
                {
                    switch(ev)
                    {
                        case CombatlogMiscEvents.COMBATANT_INFO:
                            break;
                        case CombatlogMiscEvents.WORLD_MARKER_PLACED:
                            break;
                        case CombatlogMiscEvents.WORLD_MARKER_REMOVED:
                            break;
                        case CombatlogMiscEvents.ZONE_CHANGE:
                            break;
                    }
                }
                else
                {
                    //TODO: Write to a Log that a subevent could not be recognized. Its probably new.
                }
            }
            //ENCOUNTER_EVENT should be exactly here.
            if((line = reader.ReadLine()) != null)
            {
                int pos = 20;
                var timestamp = ParsingUtil.StringTimestampToDateTime(line[..18]);
                if (line.ContainsSubstringAt("ENCOUNTER_END", pos))
                {
                    //the end of the encounter.
                    encEndT = timestamp;
                    for (int j = 0; j < 5; j++) //skip past encounterID, encounterName, difficultyID, groupSize, success
                        ParsingUtil.MovePastNextDivisor(line, ref pos);
                    encDurationMS = uint.Parse(ParsingUtil.NextSubstring(line, ref pos));
                }
            }

            return new EncounterInfo()
            {
                CombatlogEvents = events.ToArray(),
                EncounterStartTime = encStartT,
                EncounterSuccess = metadata.success,
                DifficultyID = metadata.difficultyID,
                EncounterID = metadata.wowEncounterID,
                EncounterName = encName,
                GroupSize = grpSize,
                EncounterDuration = encDurationMS,
                EncounterEndTime = encEndT,
                //Players = /*GET THIS FROM COMBATANT_INFO EVENTS*/
            };
        }

        /// <summary>
        /// Process the damage and healing data for all players in a log.
        /// </summary>
        /// <param name="encounterInfo"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        private static PerformanceMetadata[] ProcessPerformances(EncounterInfo encounterInfo, uint enc_UID, bool advanced)
        {
            PerformanceMetadata[] result = new PerformanceMetadata[encounterInfo.GroupSize];
            //1. Create metadata for every player.
            for(int i = 0; i < encounterInfo.GroupSize; i++)
            {
                result[i] = new(encounterInfo.Players[i].GUID)
                {
                    encounterUID = enc_UID
                };
            }
            //1.1 fetch the names for all players.
            foreach(var player in encounterInfo.Players)
            {
                var ev = encounterInfo.FirstEventForGUID(player.GUID);
                if (ev != null)
                    player.SetNameAndRealm(ev.SourceName);
            }

            var allySource = new AnyOfFilter(
                    new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY),
                    new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL)
                );
            var damageEvents = encounterInfo.AllEventsThatMatch(
                SubeventFilter.DamageEvents, //all _DAMAGE events
                allySource,                  //where the source is friendly or neutral (belongs to the raid/party)
                new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE) //and the target is hostile.
                );
            //the dictionary to look up the actual source GUID (pet->player, player->player, guardian->player, etc)
            var sourceToOwnerGUID = new Dictionary<string, string>();
            if (advanced)
            {
                foreach (var e in encounterInfo.CombatlogEvents)
                {
                    if (!ParsingUtil.SubeventContainsAdvancedParams(e.SubeventSuffix))
                        continue;
                    //if the source unit is the advanced info unit
                    if(e.SourceGUID == e.GetInfoGUID())
                    {
                        var owner = e.GetOwnerGUID();
                        //"000000000000" is the default GUID for "no owner".
                        //regular GUIDs start with Player/Creature prefixes.
                        //Since this event is confirmed to have advanced params, GetOwnerGUID will never return string.Empty.
                        if (owner[0] != '0') 
                        {
                            if (sourceToOwnerGUID.ContainsKey(e.SourceGUID) == false)
                                sourceToOwnerGUID.Add(e.SourceGUID, owner);
                        }
                        else if (sourceToOwnerGUID.ContainsKey(e.SourceGUID) == false)
                            sourceToOwnerGUID.Add(e.SourceGUID, e.SourceGUID);
                    }
                }
            }
            else
            {
                //TODO: Write to log that encounter cant be processed without advanced parameters.
                return Array.Empty<PerformanceMetadata>();
            }
            //add together all the damage events.
            foreach(var ev in encounterInfo.CombatlogEvents.GetEvents<DamageEvent>())
            {
                long damageDone = ev.amount;
                long damageAbsorbed = ev.absorbed;

                //try to add the damage done directly to the player by their GUID
                //by this point, the dictionary has ALL possible GUIDs of units in it. therefore this is OK!
                if(result.TryGetByGUID(sourceToOwnerGUID[ev.SourceGUID], out var perf))
                {
                    perf!.dps += (damageDone + damageAbsorbed);
                }
                else //source is not player, but a pet/guardian/npc summoned by a player that could not be identified to belong to a player.
                {

                }
            }
            foreach(var performance in result)
            {
                performance.dps /= encounterInfo.EncounterDuration;
                performance.hps /= encounterInfo.EncounterDuration;
                //other performance members need to be assigned toon before returned.
            }
            return result;
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
                    CombatlogEvent? clevent = CombatlogEvent.Create(line);
                    if (clevent != null)
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

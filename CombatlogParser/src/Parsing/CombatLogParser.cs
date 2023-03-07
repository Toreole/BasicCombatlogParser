using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CombatlogParser.Data;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;
using CombatlogParser.Parsing;
using static CombatlogParser.ParsingUtil;

namespace CombatlogParser
{
    public static class CombatLogParser
    {
        private const int MIN_ENCOUNTER_DURATION = 5000; //encounters that last less than 5 seconds will not be regarded.
        private readonly static FieldInfo charPosField;
        private readonly static FieldInfo charLenField;
        private readonly static FieldInfo charBufferField;

        static CombatLogParser()
        {
            BindingFlags privateMember = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            charBufferField = typeof(StreamReader).GetField("_charBuffer", privateMember)!;
            charLenField = typeof(StreamReader).GetField("_charLen", privateMember)!;
            charPosField = typeof(StreamReader).GetField("_charPos", privateMember)!;
        }

        private static bool DBContainsLog(string file)
        {
            using CombatlogDBContext context = new();
            return context.Combatlogs.Any(log => log.FileName == file);
        }

        private static string LocalPath(string file)
            => Path.Combine(Config.Default.Local_Log_Directory, file);

        public static void ImportCombatlog(string filePath)
        {
            //just the name of the file.
            string fileName = Path.GetFileName(filePath);
            if (DBContainsLog(fileName))
            {
                //Log has already been imported, dont do it again.
                return;
            }
            //Create a copy of the file in the current local log directory.
            string copiedLogPath = LocalPath(fileName);
            File.Copy(filePath, copiedLogPath);

            System.Windows.Controls.ProgressBar progressBar = new();
            using FileStream fileStream = File.Open(copiedLogPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            long position = fileStream.Position; //should be 0 here. or well the start of the file at least.


            //create a text reader for the file
            using StreamReader reader = new(fileStream);

            string line = reader.ReadLine()!;

            uint logId;
            bool advancedLogEnabled;
            //Create the CombatlogMetadata and save it to the DB.
            {
                //the metadata for the entire file
                CombatlogMetadata logMetadata = new();
                Regex regex = new("([A-Z0-9._ /: ]+)");
                var matches = regex.Matches(line);
                //logMetadata.logVersion = int.Parse(matches[1].Value);
                advancedLogEnabled = logMetadata.IsAdvanced = matches[3].Value == "1";
                logMetadata.BuildVersion = matches[5].Value;
                logMetadata.ProjectID = (WowProjectID)int.Parse(matches[7].Value);
                logMetadata.FileName = fileName;

                DateTime dtTimestamp = StringTimestampToDateTime(line[..18]);
                DateTimeOffset offset = new(dtTimestamp);
                logMetadata.MsTimeStamp = offset.ToUnixTimeMilliseconds();
                //store the log in the db and cache the ID
                DBStore.StoreCombatlog(logMetadata);
                logId = logMetadata.Id;
            }

            //update the position
            position = fileStream.Position;
            string? next = reader.ReadLine();

            List<EncounterInfoMetadata> encounterMetadatas = new();
            //Step 1: Read the entire file, mark the start (and end) of encounters.
            FetchEncounterInfoMetadata(ref position, reader, line, logId, ref next, encounterMetadatas);


            var uniquePlayers = new List<PlayerMetadata>();
            //Step 2: Parse the encounters individually
            for (int i = 0; i < encounterMetadatas.Count; i++)
            {
                var parsedEncounter = ParseEncounter(encounterMetadatas[i], advancedLogEnabled, copiedLogPath);
                foreach(var player in parsedEncounter.Players)
                {
                    if (uniquePlayers.Exists(other => other.GUID == player.GUID))
                        continue;
                    uniquePlayers.Add(new()
                    {
                        GUID = player.GUID,
                        Name = player.Name,
                        Realm = player.Realm,
                        ClassId = player.Class
                    });
                }
                //process performances inside of the current encounter immediately.
                var performances = ProcessPerformances(parsedEncounter, encounterMetadatas[i].Id, advancedLogEnabled);
            }

            //MainWindow.Encounters = encounterMetadatas.Take(encounterMetadatas.Count).ToArray();

            foreach (var p in uniquePlayers)
                DBStore.StorePlayer(p);
        }

        private static void FetchEncounterInfoMetadata(ref long position, StreamReader reader, string line, uint logId, ref string? next, List<EncounterInfoMetadata> encounterMetadatas)
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
                    MovePastNextDivisor(next, ref i); //move past ENCOUNTER_START,
                    encounterMetadata = new EncounterInfoMetadata()
                    {
                        CombatlogMetadataId = (uint)logId,
                        EncounterStartIndex = position
                    };
                    encounterMetadata.WowEncounterId = uint.Parse(NextSubstring(next, ref i));
                    MovePastNextDivisor(next, ref i); //skip past the name of the encounter.
                    encounterMetadata.DifficultyId = int.Parse(NextSubstring(next, ref i));
                }
                else if (next.ContainsSubstringAt("ENCOUNTER_END", 20))
                {
                    if (encounterMetadata != null)
                    {
                        length++;
                        for (int j = 0; j < 4; j++) //skip past encounterID, encounterName, difficultyID, groupSize
                            MovePastNextDivisor(line, ref i);
                        encounterMetadata.Success = NextSubstring(line, ref i) == "1";
                        
                        encounterMetadata.EncounterLengthInFile = length;
                        encounterMetadata.EncounterDurationMS = long.Parse(NextSubstring(line, ref i));
                        if(encounterMetadata.EncounterDurationMS > MIN_ENCOUNTER_DURATION)
                        {
                            DBStore.StoreEncounter(encounterMetadata); //add it to the db
                            encounterMetadatas.Add(encounterMetadata); //also add it to the list.
                        }
                    }
                }
                else
                    length++;

                var charBuffer = (char[])charBufferField.GetValue(reader)!;
                var charLen = (int)charLenField.GetValue(reader)!;
                var charPos = (int)charPosField.GetValue(reader)!;

                var actualPosition = reader.BaseStream.Position - reader.CurrentEncoding.GetByteCount(charBuffer, charPos, charLen - charPos);
                //update the position
                position = actualPosition;
                next = reader.ReadLine();
            }
        }

        //NOTE: I wonder if i should split each encounter into a seperate file, rather than keeping them all connected hmmm...
        /// <summary>
        /// Reads an Encounter from a combatlog file.
        /// </summary>
        /// <param name="metadata">metadata about the encounter.</param>
        /// <param name="filePath">The full file path to the combatlog.txt</param>
        /// <param name="advanced">Whether the log is using advanced parameters.</param>
        /// <returns></returns>
        public static EncounterInfo ParseEncounter(EncounterInfoMetadata metadata, bool advanced = true, string ? filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
                filePath = (metadata.CombatlogMetadata != null)?
                    LocalPath(metadata.CombatlogMetadata.FileName) : throw new FileNotFoundException("No filepath given.");
            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using TextReader reader = new StreamReader(fileStream);

            fileStream.Position = metadata.EncounterStartIndex;

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
                encStartT = StringTimestampToDateTime(line[..18]);
                int i = 20;
                MovePastNextDivisor(line, ref i);
                //wowEncID = uint.Parse(ParsingUtil.NextSubstring(line, ref i));
                MovePastNextDivisor(line, ref i); //redundant, skip.
                encName = NextSubstring(line, ref i);
                //diffID = int.Parse(ParsingUtil.NextSubstring(line, ref i)); //redundant, skip.
                MovePastNextDivisor(line, ref i);
                grpSize = int.Parse(NextSubstring(line, ref i));
                wowInstID = uint.Parse(NextSubstring(line, ref i));
            }

            //encounterLength is guaranteed to be more or equal to the amount of actual combat events in the encounter.
            List<CombatlogEvent> events = new(metadata.EncounterLengthInFile - grpSize);
            List<MiscEvent> miscEvents = new(100); //not expecting a whole lot tbh.

            //grab basic PlayerInfo[] from the COMBATANT_INFO events that are fired right after 
            //ENCOUNTER_START and their amount should equal the grpSize.
            PlayerInfo[] players = new PlayerInfo[grpSize];
            for(int i = 0; i < grpSize; i++)
            {
                line = reader.ReadLine()!;
                int pos = 20;
                if (NextSubstring(line, ref pos) != "COMBATANT_INFO")
                    break;
                //order of params:
                //(0)playerGUID,(1)Strength,(2)Agility,(3)Stamina,(4)Intelligence,(5)Dodge,(6)Parry,
                //(7)Block,(8)CritMelee,(9)CritRanged,(10)CritSpell,(11)Speed,
                //(12)Lifesteal,(13)HasteMelee,(14)HasteRanged,(15)HasteSpell,(16)Avoidance,(17)Mastery,
                //(18)VersatilityDamageDone,(19)VersatilityHealingDone,(20)VersatilityDamageTaken,
                //(21)Armor,(22)CurrentSpecID,
                PlayerInfo player = new()
                {
                    GUID = NextSubstring(line, ref pos),
                    Strength = int.Parse(NextSubstring(line, ref pos)),
                    Agility = int.Parse(NextSubstring(line, ref pos)),
                    Stamina = int.Parse(NextSubstring(line, ref pos)),
                    Intelligence = int.Parse(NextSubstring(line, ref pos))
                };
                for (int skip = 5; skip < 22; skip++) MovePastNextDivisor(line, ref pos);
                player.SpecId = (SpecId)int.Parse(NextSubstring(line, ref pos));
                player.Class = player.SpecId.GetClassId();
            }

            //read all the events during the encounter.
            for (int l = 1; l < metadata.EncounterLengthInFile-(1+grpSize); l++) 
            {
                line = reader.ReadLine()!;
                int pos = 20;
                var timestamp = StringTimestampToDateTime(line[..18]);

                string subevent = NextSubstring(line, ref pos);
                //confirm that the subevent is a combatlog event (it has a prefix and a suffix)
                //other events are to be handled differently.
                if(TryParsePrefixAffixSubeventF(subevent, out var prefix, out var suffix))
                {
                    CombatlogEvent? clevent = CombatlogEvent.Create(line, prefix, suffix);
                    if(clevent != null)
                        events.Add(clevent);
                }
                //TODO: Other events should also be handled.
                else if(TryParseMiscEventF(subevent, out var ev))
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
                var timestamp = StringTimestampToDateTime(line[..18]);
                if (line.ContainsSubstringAt("ENCOUNTER_END", pos))
                {
                    //the end of the encounter.
                    encEndT = timestamp;
                    for (int skip = 0; skip < 6; skip++)
                        MovePastNextDivisor(line, ref pos);
                    //var encounterEnd = ParsingUtil.NextSubstring(line, ref pos);
                    //var encounterId = ParsingUtil.NextSubstring(line, ref pos);
                    //var encounterName = ParsingUtil.NextSubstring(line, ref pos);
                    //var difficultyId = ParsingUtil.NextSubstring(line, ref pos);
                    //var groupsize = ParsingUtil.NextSubstring(line, ref pos);
                    //var succ = ParsingUtil.NextSubstring(line, ref pos);
                    encDurationMS = uint.Parse(ParsingUtil.NextSubstring(line, ref pos));
                }
            }

            //try to fill in the name and realm of the players from events theyre involved in.
            foreach(var player in players)
            {
                var fullName = events.Find(e => e.SourceGUID == player.GUID)?.SourceName
                    ?? events.Find(e => e.TargetGUID == player.GUID)?.TargetName;
                if (string.IsNullOrEmpty(fullName))
                    continue;
                player.SetNameAndRealm(fullName);
            }

            return new EncounterInfo()
            {
                CombatlogEvents = events.ToArray(),
                EncounterStartTime = encStartT,
                EncounterSuccess = metadata.Success,
                DifficultyID = metadata.DifficultyId,
                EncounterID = metadata.WowEncounterId,
                EncounterName = encName,
                GroupSize = grpSize,
                EncounterDuration = encDurationMS,
                EncounterEndTime = encEndT,
                Players = players 
            };
        }

        /// <summary>
        /// Process the damage and healing data for all players in a log.
        /// </summary>
        /// <param name="encounterInfo"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        private static PerformanceMetadata[] ProcessPerformances(EncounterInfo encounterInfo, uint encounterMetadataId, bool advanced)
        {
            PerformanceMetadata[] result = new PerformanceMetadata[encounterInfo.GroupSize];
            //1. Create metadata for every player.
            for (int i = 0; i < encounterInfo.GroupSize && i < encounterInfo.Players.Length; i++)
            {
                result[i] = new()
                {
                    PlayerMetadataId = 0, //TODO; needs to be discovered in the DB or registered.
                    EncounterInfoMetadataId = encounterMetadataId
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
            var damageEvents = encounterInfo.CombatlogEvents.GetEvents<DamageEvent>().AllThatMatch(
                new AllOfFilter(
                    allySource,                  //where the source is friendly or neutral (belongs to the raid/party)
                    new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE) //and the target is hostile.
                )
                );
            //the dictionary to look up the actual source GUID (pet->player, player->player, guardian->player, etc)
            var sourceToOwnerGUID = new Dictionary<string, string>();
            if (advanced)
            {
                foreach (var e in encounterInfo.CombatlogEvents)
                {
                    if (!SubeventContainsAdvancedParams(e.SubeventSuffix))
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
            foreach(var ev in damageEvents)
            {
                //try to add the damage done directly to the player by their GUID
                //by this point, the dictionary has ALL possible GUIDs of units in it. therefore this is OK!
                if(result.TryGetByGUID(ev.SourceGUID, out var perf))
                {
                    perf!.Dps += (ev.Amount + ev.Absorbed);
                }
                else //source is not player, but a pet/guardian/npc summoned by a player that could not be identified to belong to a player.
                {

                }
            }
            //add together all the healing events.
            foreach(var ev in encounterInfo.CombatlogEvents.GetEvents<HealEvent>())
            {
                if (result.TryGetByGUID(ev.SourceGUID, out var perf))
                {
                    perf!.Hps += ev.Amount + ev.Absorbed;
                }
                else //source is not player, but a pet/guardian/npc summoned by a player that could not be identified to belong to a player.
                {

                }
            }
            foreach(var performance in result)
            {
                if (performance is null) continue;
                performance.Dps /= (encounterInfo.EncounterDuration / 1000);
                performance.Hps /= (encounterInfo.EncounterDuration / 1000);
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
                combatlog.LogStartTimestamp = StringTimestampToDateTime(line[..18]);
                int i = 20;
                //skip COMBAT_LOG_VERSION
                MovePastNextDivisor(line, ref i);
                combatlog.CombatlogVersion = int.Parse(NextSubstring(line, ref i));
                //skip ADVANCED_LOG_ENABLED
                MovePastNextDivisor(line, ref i);
                combatlog.AdvancedLogEnabled = NextSubstring(line, ref i) == "1";
                //skip BUILD_VERSION
                MovePastNextDivisor(line, ref i);
                combatlog.BuildVersion = NextSubstring(line, ref i);
                //skip PROJECT_ID
                MovePastNextDivisor(line, ref i);
                combatlog.ProjectID = int.Parse(NextSubstring(line, ref i));
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
                string sub = NextSubstring(line, ref i);

                var timestamp = StringTimestampToDateTime(line[..18]);

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
                            EncounterID = uint.Parse(NextSubstring(line, ref i)),
                            EncounterName = NextSubstring(line, ref i),
                            DifficultyID = int.Parse(NextSubstring(line, ref i)),
                            GroupSize = int.Parse(NextSubstring(line, ref i)),
                            InstanceID = uint.Parse(NextSubstring(line, ref i))
                        };
                    }
                    //encounter end.
                    else if(miscEvent == CombatlogMiscEvents.ENCOUNTER_END && currentEncounter != null)
                    {
                        for (int j = 0; j < 4; j++) //skip past encounterID, encounterName, difficultyID, groupSize
                            MovePastNextDivisor(line, ref i);
                        currentEncounter.EncounterSuccess = NextSubstring(line, ref i) == "1";
                        currentEncounter.EncounterDuration = uint.Parse(NextSubstring(line, ref i));
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
                    TryParsePrefixAffixSubeventF(sub, out CombatlogEventPrefix cPrefix, out CombatlogEventSuffix cSuffix))
                {
                    CombatlogEvent? clevent = CombatlogEvent.Create(line, cPrefix, cSuffix);
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

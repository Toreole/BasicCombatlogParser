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

        private static string LocalPath(string fileName)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string logDirectory = Path.Combine(currentDirectory, Config.Default.Local_Log_Directory);
            if (Directory.Exists(logDirectory) == false)
                Directory.CreateDirectory(logDirectory);
            return Path.Combine(logDirectory, fileName);
        }

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
            File.Copy(filePath, copiedLogPath, true);

            //System.Windows.Controls.ProgressBar progressBar = new();
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

                DateTime dtTimestamp = StringTimestampToDateTime(line[..18].TrimEnd());
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
            FetchEncounterInfoMetadata(ref position, reader, logId, ref next, encounterMetadatas);


            //Step 2: Parse the encounters individually
            for (int i = 0; i < encounterMetadatas.Count; i++)
            {
                var currentPlayers = new List<PlayerMetadata>();
                var parsedEncounter = ParseEncounter(encounterMetadatas[i], advancedLogEnabled, copiedLogPath);
                foreach(var player in parsedEncounter.Players)
                {
                    if (player == null)
                        continue;
                    var newPlayer = DBStore.GetOrCreatePlayerMetadata(player);
                    currentPlayers.Add(newPlayer);
                }
                //process performances inside of the current encounter immediately.
                var performances = ProcessPerformances(parsedEncounter, currentPlayers, encounterMetadatas[i].Id, advancedLogEnabled);
                foreach(var performance in performances)
                {
                    if (performance == null)
                        continue;
                    DBStore.StorePerformance(performance);
                }
            }
        }

        private static void FetchEncounterInfoMetadata(ref long position, StreamReader reader, uint logId, ref string? next, List<EncounterInfoMetadata> encounterMetadatas)
        {
            //single variable encounterMetadata is not needed outside of this scope.
            EncounterInfoMetadata? encounterMetadata = null;
            int length = 0;
            while (next != null)
            {
                int index = next.IndexOf(timestamp_end_seperator);
                index += 2;
                if (next.ContainsSubstringAt("ENCOUNTER_START", index))
                {
                    length = 1;
                    MovePastNextDivisor(next, ref index); //move past ENCOUNTER_START,
                    encounterMetadata = new EncounterInfoMetadata()
                    {
                        CombatlogMetadataId = (uint)logId,
                        EncounterStartIndex = position
                    };
                    encounterMetadata.WowEncounterId = (EncounterId)uint.Parse(NextSubstring(next, ref index));
                    MovePastNextDivisor(next, ref index); //skip past the name of the encounter.
                    string diff = NextSubstring(next, ref index);
                    encounterMetadata.DifficultyId = (DifficultyId)int.Parse(diff);
                }
                else if (next.ContainsSubstringAt("ENCOUNTER_END", index))
                {
                    if (encounterMetadata != null)
                    {
                        length++;
                        for (int j = 0; j < 5; j++) //skip past ENCOUNTER_END, encounterID, encounterName, difficultyID, groupSize
                            MovePastNextDivisor(next, ref index);
                        encounterMetadata.Success = NextSubstring(next, ref index) == "1";
                        
                        encounterMetadata.EncounterLengthInFile = length;
                        string durationString = NextSubstring(next, ref index);
                        encounterMetadata.EncounterDurationMS = long.Parse(durationString);
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
            int timestampEnd = line.IndexOf(timestamp_end_seperator);
            int index = timestampEnd + 2;
            if (line.ContainsSubstringAt("ENCOUNTER_START", index))
            {
                encStartT = StringTimestampToDateTime(line[..timestampEnd]);
                MovePastNextDivisor(line, ref index);
                //wowEncID = uint.Parse(ParsingUtil.NextSubstring(line, ref i));
                MovePastNextDivisor(line, ref index); //redundant, skip.
                encName = NextSubstring(line, ref index);
                //diffID = int.Parse(ParsingUtil.NextSubstring(line, ref i)); //redundant, skip.
                MovePastNextDivisor(line, ref index);
                grpSize = int.Parse(NextSubstring(line, ref index));
                wowInstID = uint.Parse(NextSubstring(line, ref index));
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
                int pos = line.IndexOf(timestamp_end_seperator) + 2;
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
                for (int skip = 5; skip < 23; skip++) MovePastNextDivisor(line, ref pos);
                player.SpecId = (SpecId)int.Parse(NextSubstring(line, ref pos));
                player.Class = player.SpecId.GetClassId();
                _ = NextArray(line, ref pos); // talents are skipped over for now.
                _ = NextItemGroup(line, ref pos); //PvP talents skipped
                string rawEquipmentInfo = NextArray(line, ref pos);
                player.ItemLevel = GetAverageItemLevel(rawEquipmentInfo);
                players[i] = player; //cant believe i forgot this lmfao.
            }

            CombatlogEventDictionaryBuilder eventDictBuilder = new();
            //read all the events during the encounter.
            for (int l = 1; l < metadata.EncounterLengthInFile-(1+grpSize); l++) 
            {
                line = reader.ReadLine()!;
                int pos = line.IndexOf(timestamp_end_seperator);
                var timestamp = StringTimestampToDateTime(line[..pos]);
                pos += 2;
                string subevent = NextSubstring(line, ref pos);
                //confirm that the subevent is a combatlog event (it has a prefix and a suffix)
                //other events are to be handled differently.
                if(TryParsePrefixAffixSubeventF(subevent, out var prefix, out var suffix))
                {
                    CombatlogEvent? clevent = CombatlogEvent.Create(line, prefix, suffix);
                    if (clevent != null)
                    {
                        events.Add(clevent);
                        eventDictBuilder.Add(clevent);
                    }
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
                int pos = line.IndexOf(timestamp_end_seperator);
                var timestamp = StringTimestampToDateTime(line[..pos]);
                pos += 2;
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
                if (player == null) continue;
                var fullName = events.Find(e => e.SourceGUID == player.GUID)?.SourceName
                    ?? events.Find(e => e.TargetGUID == player.GUID)?.TargetName ?? string.Empty;
                if (string.IsNullOrEmpty(fullName))
                    continue;
                player.SetNameAndRealm(fullName);
            }
            //try to find all npcs.
            HashSet<string> npcGuids = new();
            List<NpcInfo> npcs = new();
            foreach(var _event in events)
            {
                (string? guid, string npcName ) = _event.SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_CONTROL_NPC) ? 
                    (_event.SourceGUID, _event.SourceName) 
                    : _event.TargetFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_CONTROL_NPC)? 
                    (_event.TargetGUID, _event.TargetName) 
                    : (null, "");
                if (guid is null || npcGuids.Contains(guid) || !TryGetNpcId(guid, out uint npcId)) 
                    continue;
                npcGuids.Add(guid);
                NpcInfo? npcInfo = npcs.FirstOrDefault(x => x.NpcId == npcId);
                if (npcInfo == null)
                {
                    npcInfo = new(npcId, npcName, guid);
                    npcs.Add(npcInfo);
                }
                else
                    npcInfo.InstanceGuids.Add(guid);
            }

            return new EncounterInfo()
            {
                CombatlogEvents = events.ToArray(),
                CombatlogEventDictionary = eventDictBuilder.Build(),
                EncounterStartTime = encStartT,
                EncounterSuccess = metadata.Success,
                DifficultyID = metadata.DifficultyId,
                EncounterID = metadata.WowEncounterId,
                EncounterName = encName,
                GroupSize = grpSize,
                EncounterDuration = encDurationMS,
                EncounterEndTime = encEndT,
                Players = players,
                Npcs = npcs,
            };
        }

        /// <summary>
        /// Gets a rough (rounded) estimate of the equipped item level.
        /// Always assumes optimal weapons are used (one hand + off-hand / two hand)
        /// </summary>
        /// <param name="equipString">the unprocessed equipment string (everything within [ ])</param>
        public static int GetAverageItemLevel(string equipString)
        {
            int index = 0;
            int totalLevel = 0;
            for (int itemSlot = 0; itemSlot < 3; itemSlot++)
                totalLevel += GetItemLevel(NextItemGroup(equipString, ref index));
            _ = NextItemGroup(equipString, ref index); //discard shirt for now.
            for (int itemSlot = 4; itemSlot < 16; itemSlot++)
                totalLevel += GetItemLevel(NextItemGroup(equipString, ref index));
            int offHandLevel = GetItemLevel(NextItemGroup(equipString, ref index));
            if(offHandLevel == 0)
            {
                return (int)MathF.Round(totalLevel / 15f);
            }
            else
            {
                return (int)MathF.Round((totalLevel + offHandLevel) / 16f);
            }
        }

        private static int GetItemLevel(string itemString)
        {
            int index = 0;
            MovePastNextDivisor(itemString, ref index);
            return int.Parse(NextSubstring(itemString, ref index));
        }

        /// <summary>
        /// Process the damage and healing data for all players in a log.
        /// </summary>
        /// <param name="encounterInfo"></param>
        /// <param name="advanced"></param>
        /// <returns></returns>
        private static PerformanceMetadata[] ProcessPerformances(EncounterInfo encounterInfo, List<PlayerMetadata> playerMetadatas, uint encounterMetadataId, bool advanced)
        {
            if (!advanced || playerMetadatas.Count == 0)
                //TODO: Write to log that encounter cant be processed without advanced parameters.
                return Array.Empty<PerformanceMetadata>();
            PerformanceMetadata[] result = new PerformanceMetadata[encounterInfo.GroupSize];
            //1. Create metadata for every player.
            for (int i = 0; i < encounterInfo.GroupSize && i < playerMetadatas.Count; i++)
            {
                result[i] = new()
                {
                    PlayerMetadata = playerMetadatas[i],
                    SpecId = encounterInfo.Players[i].SpecId,
                    PlayerMetadataId = playerMetadatas.Find(x => x.GUID == encounterInfo.Players[i].GUID)?.Id ?? 0,
                    EncounterInfoMetadataId = encounterMetadataId,
                    WowEncounterId = encounterInfo.EncounterID, //duplicate data anyway.
                    ItemLevel = encounterInfo.Players[i].ItemLevel,
                };
            }
            //1.1 fetch the names for all players.
            foreach(var player in encounterInfo.Players)
            {
                if (player == null)
                    continue;
                var ev = encounterInfo.FirstEventForGUID(player.GUID);
                if (ev != null)
                    player.SetNameAndRealm(ev.SourceName);
            }

            //the dictionary to look up the actual source GUID (pet->player, player->player, guardian->player, etc)
            var sourceToOwnerGUID = new Dictionary<string, string>();
            foreach(var summon in encounterInfo.CombatlogEventDictionary.GetEvents<SummonEvent>())
            {
                //the summoned "pet" is the targetGUID of the event.
                if (sourceToOwnerGUID.ContainsKey(summon.TargetGUID) == false)
                    sourceToOwnerGUID.Add(summon.TargetGUID, summon.SourceGUID);
            }
            foreach (var e in encounterInfo.CombatlogEvents.GetAdvancedParamEvents()) 
            {
                var sourceGUID = e.SourceGUID;
                //if the source unit is the advanced info unit
                if (sourceGUID != e.AdvancedParams.infoGUID)
                    continue;
                var owner = e.AdvancedParams.ownerGUID;
                //"000000000000" is the default GUID for "no owner".
                //regular GUIDs start with "Player", "Creature" or "Pet".
                if (sourceToOwnerGUID.ContainsKey(sourceGUID) == false)
                    sourceToOwnerGUID.Add(sourceGUID, owner[0] == '0' ? sourceGUID : owner);
            }

            var allySource = new AnyOfFilter(
                    new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY),
                    new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL)
                );
            var damageEvents = encounterInfo.CombatlogEventDictionary.GetEvents<DamageEvent>().AllThatMatch(
                new AllOfFilter(
                    allySource,                  //where the source is friendly or neutral (belongs to the raid/party)
                    new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE) //and the target is hostile.
                )
                );
            //add together all the damage events.
            foreach (var ev in damageEvents)
            {
                //try to add the damage done directly to the player by their GUID
                //by this point, the dictionary has ALL possible GUIDs of units in it. therefore this is OK!
                bool sourceIsCorrect = !sourceToOwnerGUID.TryGetValue(ev.SourceGUID, out string? trueSourceGUID);
                if(result.TryGetByGUID(sourceIsCorrect? ev.SourceGUID : trueSourceGUID!, out var perf))
                {
                    perf!.Dps += (ev.Amount + ev.Absorbed);
                }
                else //source is not player, but a pet/guardian/npc summoned by a player that could not be identified to belong to a player.
                {

                }
            }
            //add together all the healing events.
            foreach(var ev in encounterInfo.CombatlogEventDictionary.GetEvents<HealEvent>())
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
                if (performance.Dps != 0) 
                    performance.Dps /= (encounterInfo.EncounterDuration / 1000);
                if(performance.Hps != 0)
                    performance.Hps /= (encounterInfo.EncounterDuration / 1000);
                //other performance members need to be assigned toon before returned.
            }
            return result;
        }


    }
}

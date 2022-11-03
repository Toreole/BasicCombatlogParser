using System.IO;
using System.Text.RegularExpressions;
using CombatlogParser.Data;
using CombatlogParser.Data.MetaData;

namespace CombatlogParser
{
    public static class CombatLogParser
    {
        public static void Import(string filePath)
        {
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




            Task<string> task = ReadFrom(0, filePath);
            task.Start();
            task.GetAwaiter().GetResult();
        }

        //NOTE: I wonder if i should split each encounter into a seperate file, rather than keeping them all connected hmmm...

        private async static Task<string> ReadFrom(long startPosition, string fileName)
        {
            //every reader needs its own filestream.
            using FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            //offset the stream
            fileStream.Position = startPosition;
            using TextReader reader = new StreamReader(fileStream);
            Task<string?> x = reader.ReadLineAsync();
            await x;
            return x.Result!;
        }

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

                CombatlogEvent clevent = new()
                {
                    Timestamp = ParsingUtil.StringTimestampToDateTime(line[..18]),
                    SubeventName = sub
                };

                if (Enum.TryParse(sub, out CombatlogMiscEvents miscEvent))//should be a misc event.
                {
                    //encounter start.
                    if(miscEvent == CombatlogMiscEvents.ENCOUNTER_START)
                    {
                        //clear it in here aswell just to make sure.
                        encounterEvents.Clear();
                        currentEncounter = new()
                        {
                            EncounterStartTime = clevent.Timestamp,
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
                        currentEncounter.EncounterEndTime = clevent.Timestamp;

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
                    clevent.Timestamp > currentEncounter.EncounterStartTime && //only include events that happen *after* start of encounter.
                    ParsingUtil.TryParsePrefixAffixSubevent(sub, out CombatlogEventPrefix cPrefix, out CombatlogEventSuffix cSuffix))
                {
                    clevent.SubeventPrefix = cPrefix;
                    clevent.SubeventSuffix = cSuffix;

                    //sourceGUID and name
                    clevent.SourceGUID = ParsingUtil.NextSubstring(line, ref i);
                    if (clevent.SubeventPrefix == CombatlogEventPrefix.ENVIRONMENTAL || clevent.SourceGUID == "0000000000000000")
                    {
                        clevent.SourceName = "Environment";
                        ParsingUtil.MovePastNextDivisor(line, ref i);
                    }
                    else
                    {
                        clevent.SourceName = ParsingUtil.NextSubstring(line, ref i);
                    }

                    //source flags.
                    clevent.SourceFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));
                    clevent.SourceRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));

                    //targetGUID and name
                    clevent.TargetGUID = ParsingUtil.NextSubstring(line, ref i);
                    clevent.TargetName = ParsingUtil.NextSubstring(line, ref i);

                    //target flags
                    clevent.TargetFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));
                    clevent.TargetRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));

                    //at this point Prefix params can be handled (if any)
                    int prefixAmount = ParsingUtil.GetPrefixParamAmount(cPrefix);
                    if (prefixAmount > 0)
                    {
                        var prefixParams = new object[prefixAmount];
                        for (int j = 0; j < prefixAmount; j++)
                        {
                            if (j == 2)
                                prefixParams[j] = (SpellSchool)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));
                            else
                                prefixParams[j] = ParsingUtil.NextSubstring(line, ref i);
                        }
                        clevent.PrefixParams = prefixParams;
                    }

                    //then follow the advanced combatlog params
                    //watch out! not all events have the advanced params!
                    if (combatlog.AdvancedLogEnabled && ParsingUtil.SubeventContainsAdvancedParams(cSuffix))
                    {
                        var advancedParams = new string[17];
                        for (int j = 0; j < 17; j++)
                        {
                            advancedParams[j] = ParsingUtil.NextSubstring(line, ref i);
                        }
                        clevent.AdvancedParams = advancedParams;
                    }

                    //lastly, suffix event params.
                    int suffixAmount = ParsingUtil.GetSuffixParamAmount(cSuffix);
                    if (suffixAmount > 0)
                    {
                        var suffixParams = new object[suffixAmount];
                        for (int j = 0; j < suffixAmount; j++)
                        {
                            suffixParams[j] = ParsingUtil.NextSubstring(line, ref i);
                        }
                        clevent.SuffixParams = suffixParams;
                    }

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

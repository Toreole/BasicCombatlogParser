using CombatlogParser.Controls;
using CombatlogParser.Data;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;
using CombatlogParser.Parsing;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using static CombatlogParser.ParsingUtil;

namespace CombatlogParser
{
    public static class CombatLogParser
    {
        private const int MIN_ENCOUNTER_DURATION = 5000; //encounters that last less than 5 seconds will not be regarded.
        private readonly static FieldInfo charPosField;
        private readonly static FieldInfo charLenField;
        private readonly static FieldInfo charBufferField;

        private readonly static Regex logInfoSeperationRegex = new("([A-Z0-9._ /: ]+)");

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
            using FileStream fileStream = new(copiedLogPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384);

            //create a text reader for the file
            using StreamReader reader = new(fileStream);

            uint logId;
            //Create the CombatlogMetadata and save it to the DB.
            {
                //the metadata for the entire file
                CombatlogMetadata logMetadata = CreateLogMetadata(fileName, reader.ReadLine()!);
                //store the log in the db and cache the ID
                DBStore.StoreCombatlog(logMetadata);
                logId = logMetadata.Id;
            }

            long position = fileStream.Position;

            List<EncounterInfoMetadata> encounterMetadatas = new();
            //Step 1: Read the entire file, mark the start (and end) of encounters.
            FetchEncounterInfoMetadata(reader, logId, encounterMetadatas);

            //Step 2: Parse the encounters individually
            for (int i = 0; i < encounterMetadatas.Count; i++)
            {
                var currentPlayers = new List<PlayerMetadata>();
                var parsedEncounter = ParseEncounter(encounterMetadatas[i], copiedLogPath);
                DBStore.StoreEncounter(encounterMetadatas[i]);
                foreach (var player in parsedEncounter.Players)
                {
                    if (player == null)
                        continue;
                    var newPlayer = DBStore.GetOrCreatePlayerMetadata(player);
                    currentPlayers.Add(newPlayer);
                }
                //process performances inside of the current encounter immediately.
                var performances = ProcessPerformances(parsedEncounter, currentPlayers, encounterMetadatas[i].Id);
                foreach (var performance in performances)
                {
                    if (performance == null)
                        continue;
                    DBStore.StorePerformance(performance);
                }
            }
        }

        public static async Task ImportCombatlogAsync(string filePath, LabelledProgressBar progressDisplay)
        {
            //just the name of the file.
            string fileName = Path.GetFileName(filePath);
            if (DBContainsLog(fileName))
            {
                //Log has already been imported, dont do it again.
                return;
            }
            progressDisplay.UpdateDisplay(0, "Copying log file...");
            //Create a copy of the file in the current local log directory.
            string copiedLogPath = LocalPath(fileName);
            await CopyFileAsync(filePath, copiedLogPath);

            progressDisplay.UpdateDisplay(20, "Fetching encounter metadata...");

            //System.Windows.Controls.ProgressBar progressBar = new();
            using FileStream fileStream = new(copiedLogPath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384);
            //create a text reader for the file
            using StreamReader reader = new(fileStream);

            uint logId;
            {
                CombatlogMetadata logMetadata = CreateLogMetadata(fileName, reader.ReadLine()!);
                await DBStore.StoreCombatlogAsync(logMetadata);
                logId = logMetadata.Id;
            }

            List<EncounterInfoMetadata> encounterMetadatas = new();
            //Step 1: Read the entire file, mark the start (and end) of encounters.
            await FetchEncounterInfoMetadataAsync(reader, logId, encounterMetadatas);

            //Step 2: Parse the encounters individually
            for (int i = 0; i < encounterMetadatas.Count; i++)
            {
                progressDisplay.UpdateDisplay(
                    progressPercent: (30 + (70 * ((double)i / encounterMetadatas.Count))),
                    description: $"Parsing Encounters... ({i}/{encounterMetadatas.Count})"
                    );
                var currentPlayers = new List<PlayerMetadata>();
                var parsedEncounter = await ParseEncounterAsync(encounterMetadatas[i], copiedLogPath);
                using CombatlogDBContext dbContext = new();
                await dbContext.Encounters.AddAsync(encounterMetadatas[i]);
                foreach (var player in parsedEncounter.Players)
                {
                    if (player == null)
                        continue;
                    var newPlayer = await DBStore.GetOrCreatePlayerMetadataAsync(dbContext, player);
                    currentPlayers.Add(newPlayer);
                }
                await dbContext.SaveChangesAsync(); //need to save here for player metadata changes
                //process performances inside of the current encounter immediately.
                var performances = ProcessPerformances(parsedEncounter, currentPlayers, encounterMetadatas[i].Id);
                foreach (var performance in performances)
                {
                    if (performance == null)
                        continue;
                    await dbContext.Performances.AddAsync(performance);
                }
                await dbContext.SaveChangesAsync();
            }
        }

        //TODO: needs speeding up. takes too long to read through the file.
        //possibilities: timestamp check for date, shouldnt change length unless its before midnight on a 9th of a month, or the last day of the 9th month.
        //that way it could avoid the next.IndexOf(timestamp_end_seperator) running every time.
        //maybe ReadLineAsync is also a bait.
        private static async Task FetchEncounterInfoMetadataAsync(StreamReader reader, uint logId, List<EncounterInfoMetadata> encounterMetadatas)
        {
            long position = 0;
            //single variable encounterMetadata is not needed outside of this scope.
            EncounterInfoMetadata? encounterMetadata = null;
            int length = 0;
            string? next = await reader.ReadLineAsync();
            while (next != null)
			{
				ReadEncounterStartEnd(reader, logId, encounterMetadatas, ref position, ref encounterMetadata, ref length, next);
				next = await reader.ReadLineAsync();
            }
        }

        //taken from https://stackoverflow.com/questions/882686/non-blocking-file-copy-in-c-sharp 
        //CancellationToken omitted for simplicity
        public static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var bufferSize = 81920;

            //TODO: this currently fails while WoW is still running.
            using var sourceStream =
                  new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions);

            using var destinationStream =
                  new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, fileOptions);

            await sourceStream.CopyToAsync(destinationStream, bufferSize);
        }

        private static CombatlogMetadata CreateLogMetadata(string fileName, string line)
        {
            CombatlogMetadata logMetadata = new();

            var matches = logInfoSeperationRegex.Matches(line);
            //logMetadata.logVersion = int.Parse(matches[1].Value);
            bool advancedLogEnabled = logMetadata.IsAdvanced = matches[3].Value == "1";
            if (!advancedLogEnabled)
                throw new Exception("Combatlogs without the 'Advanced' Setting enabled are not supported.");
            logMetadata.BuildVersion = matches[5].Value;
            logMetadata.ProjectID = (WowProjectID)int.Parse(matches[7].Value);
            logMetadata.FileName = fileName;

            DateTime dtTimestamp = StringTimestampToDateTime(line[..18].TrimEnd());
            DateTimeOffset offset = new(dtTimestamp);
            logMetadata.MsTimeStamp = offset.ToUnixTimeMilliseconds();
            return logMetadata;
        }

        private static void FetchEncounterInfoMetadata(StreamReader reader, uint logId, List<EncounterInfoMetadata> encounterMetadatas)
        {
            long position = 0;
            //single variable encounterMetadata is not needed outside of this scope.
            EncounterInfoMetadata? encounterMetadata = null;
            int length = 0;
            string? next = reader.ReadLine();
            while (next != null)
			{
				ReadEncounterStartEnd(reader, logId, encounterMetadatas, ref position, ref encounterMetadata, ref length, next);
				next = reader.ReadLine();
			}
		}

		private static void ReadEncounterStartEnd(StreamReader reader, uint logId, List<EncounterInfoMetadata> encounterMetadatas, ref long position, ref EncounterInfoMetadata? encounterMetadata, ref int length, string line)
		{
			int index = line.IndexOf(timestamp_end_seperator);
			index += 2;
			if (line.ContainsSubstringAt("ENCOUNTER_START", index))
			{
				length = 1;
				MovePastNextDivisor(line, ref index); //move past ENCOUNTER_START,
				encounterMetadata = new EncounterInfoMetadata()
				{
					CombatlogMetadataId = (uint)logId,
					EncounterStartIndex = position
				};
				encounterMetadata.WowEncounterId = (EncounterId)uint.Parse(NextSubstring(line, ref index));
				MovePastNextDivisor(line, ref index); //skip past the name of the encounter.
				string diff = NextSubstring(line, ref index);
				encounterMetadata.DifficultyId = (DifficultyId)int.Parse(diff);
			}
			else if (line.ContainsSubstringAt("ENCOUNTER_END", index) && encounterMetadata != null)
			{
				length++;
				for (int j = 0; j < 5; j++) //skip past ENCOUNTER_END, encounterID, encounterName, difficultyID, groupSize
					MovePastNextDivisor(line, ref index);
				encounterMetadata.Success = NextSubstring(line, ref index) == "1";

				encounterMetadata.EncounterLengthInFile = length;
				string durationString = NextSubstring(line, ref index);
				encounterMetadata.EncounterDurationMS = long.Parse(durationString);
				if (encounterMetadata.EncounterDurationMS > MIN_ENCOUNTER_DURATION)
				{
					encounterMetadatas.Add(encounterMetadata); //add it to the list.
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
		}

		//NOTE: I wonder if i should split each encounter into a seperate file, rather than keeping them all connected hmmm...
		/// <summary>
		/// Reads an Encounter from a combatlog file.
		/// </summary>
		/// <param name="metadata">metadata about the encounter.</param>
		/// <param name="filePath">The full file path to the combatlog.txt</param>
		/// <param name="advanced">Whether the log is using advanced parameters.</param>
		/// <returns></returns>
		public static EncounterInfo ParseEncounter(EncounterInfoMetadata metadata, string? filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                if (metadata.CombatlogMetadata == null)
                    metadata.CombatlogMetadata = Queries.GetCombatlogMetadataByID(metadata.CombatlogMetadataId);
                filePath = (string.IsNullOrEmpty(metadata.CombatlogMetadata!.FileName)) ?
                    throw new FileNotFoundException("No filepath given.") : LocalPath(metadata.CombatlogMetadata.FileName);
            }
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384);
            return ParseEncounterWork(metadata, fileStream);
        }

        public static EncounterInfo ParseEncounter(EncounterInfoMetadata metadata, FileStream fileStream)
        {
            return ParseEncounterWork(metadata, fileStream);
        }

        public static async Task<EncounterInfo> ParseEncounterAsync(EncounterInfoMetadata metadata, string? filePath = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                if (metadata.CombatlogMetadata == null)
                    metadata.CombatlogMetadata = Queries.GetCombatlogMetadataByID(metadata.CombatlogMetadataId);
                filePath = (string.IsNullOrEmpty(metadata.CombatlogMetadata!.FileName)) ?
                    throw new FileNotFoundException("No filepath given.") : LocalPath(metadata.CombatlogMetadata.FileName);
            }
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 16384);
            return await ParseEncounterWorkAsync(metadata, fileStream);
        }

        private static EncounterInfo ParseEncounterWork(EncounterInfoMetadata metadata, FileStream fileStream)
        {
            fileStream.Position = metadata.EncounterStartIndex;
            using TextReader reader = new StreamReader(fileStream);
            using ParsingContext parsingContext = new();

            string? line = reader.ReadLine()!;

            int timestampEnd = line.IndexOf(timestamp_end_seperator);
            int index = timestampEnd + 2;
            DateTime encStartT = StringTimestampToDateTime(line[..timestampEnd]);
            MovePastNextDivisor(line, ref index); //ENCOUNTER_END
            MovePastNextDivisor(line, ref index); //encounterID
            MovePastNextDivisor(line, ref index); //encounter name
            string encounterName = metadata.WowEncounterId.GetDisplayName();
            MovePastNextDivisor(line, ref index); //difficulty
            int groupSize = int.Parse(NextSubstring(line, ref index));
            uint wowInstanceId = uint.Parse(NextSubstring(line, ref index));

            //encounterLength is guaranteed to be more or equal to the amount of actual combat events in the encounter.
            List<CombatlogEvent> events = new(metadata.EncounterLengthInFile - groupSize);
            List<MiscEvent> miscEvents = new(100); //not expecting a whole lot tbh.

            //grab basic PlayerInfo[] from the COMBATANT_INFO events that are fired right after 
            //ENCOUNTER_START and their amount should equal the grpSize.
            PlayerInfo[] players = new PlayerInfo[groupSize];
            for (int i = 0; i < groupSize; i++)
            {
                line = reader.ReadLine()!;
                int pos = line.IndexOf(timestamp_end_seperator) + 2;
                if (NextSubstring(line, ref pos) != "COMBATANT_INFO")
                    break;
                players[i] = GetCombatantInfo(line, pos);
            }

            CombatlogEventDictionaryBuilder eventDictBuilder = new();
            //read all the events during the encounter.
            for (int l = 1; l < metadata.EncounterLengthInFile - (1 + groupSize); l++)
            {
                line = reader.ReadLine()!;
                ParseLineInContext(line, parsingContext, events, eventDictBuilder);
            }
            //ENCOUNTER_EVENT should be exactly here.
            line = reader.ReadLine();
			ProcessEncounterEnd(line!, out var encounterEndTime, out var encounterDurationInMS);
			
			SetPlayerNamesFromEvents(events, players);
			List<NpcInfo> npcs = FindNPCs(events);

			return new EncounterInfo(
                events.ToArray(),
                eventDictBuilder.Build(),
                encStartT,
                metadata.Success,
                metadata.DifficultyId,
                metadata.WowEncounterId,
                encounterName,
                groupSize,
                encounterDurationInMS,
                encounterEndTime,
                players,
                npcs
            );
        }

        private static PlayerInfo GetCombatantInfo(string line, int pos)
        {
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
            return player;
        }

        private static async Task<EncounterInfo> ParseEncounterWorkAsync(EncounterInfoMetadata metadata, FileStream fileStream)
		{
			fileStream.Position = metadata.EncounterStartIndex;
			using TextReader reader = new StreamReader(fileStream);
			using ParsingContext parsingContext = new();

			string? line = (await reader.ReadLineAsync())!;

			int timestampEnd = line.IndexOf(timestamp_end_seperator);
			int index = timestampEnd + 2;
			DateTime encStartT = StringTimestampToDateTime(line[..timestampEnd]);
			MovePastNextDivisor(line, ref index); //ENCOUNTER_END
			MovePastNextDivisor(line, ref index); //encounterID
			MovePastNextDivisor(line, ref index); //encounter name
			string encounterName = metadata.WowEncounterId.GetDisplayName();
			MovePastNextDivisor(line, ref index); //difficulty
			int groupSize = int.Parse(NextSubstring(line, ref index));
			uint wowInstanceId = uint.Parse(NextSubstring(line, ref index));

			//encounterLength is guaranteed to be more or equal to the amount of actual combat events in the encounter.
			List<CombatlogEvent> events = new(metadata.EncounterLengthInFile - groupSize);
			List<MiscEvent> miscEvents = new(100); //not expecting a whole lot tbh.

			//grab basic PlayerInfo[] from the COMBATANT_INFO events that are fired right after 
			//ENCOUNTER_START and their amount should equal the grpSize.
			PlayerInfo[] players = new PlayerInfo[groupSize];
			for (int i = 0; i < groupSize; i++)
			{
				line = (await reader.ReadLineAsync())!;
				int pos = line.IndexOf(timestamp_end_seperator) + 2;
				if (NextSubstring(line, ref pos) != "COMBATANT_INFO")
					break;
				players[i] = GetCombatantInfo(line, pos);
			}

			CombatlogEventDictionaryBuilder eventDictBuilder = new();
			//read all the events during the encounter.
			for (int l = 1; l < metadata.EncounterLengthInFile - (1 + groupSize); l++)
			{
				line = (await reader.ReadLineAsync())!;
				ParseLineInContext(line, parsingContext, events, eventDictBuilder);
			}
            //ENCOUNTER_EVENT should be exactly here.
            // there used to be an if null here. fuck it, we ball.
            line = await reader.ReadLineAsync();
            ProcessEncounterEnd(line!, out  var encounterEndTime, out var encounterDurationInMS);
            
			SetPlayerNamesFromEvents(events, players);
			List<NpcInfo> npcs = FindNPCs(events);

			return new EncounterInfo(
				events.ToArray(),
				eventDictBuilder.Build(),
				encStartT,
				metadata.Success,
				metadata.DifficultyId,
				metadata.WowEncounterId,
				encounterName,
				groupSize,
				encounterDurationInMS,
				encounterEndTime,
				players,
				npcs
			);
		}

		private static void ProcessEncounterEnd(string line, out DateTime encounterEndTime, out uint encounterDurationInMS)
		{
			int pos = line.IndexOf(timestamp_end_seperator);
			var timestamp = StringTimestampToDateTime(line[..pos]);
			pos += 2;
			if (line.ContainsSubstringAt("ENCOUNTER_END", pos))
			{
				//the end of the encounter.
				encounterEndTime = timestamp;
				for (int skip = 0; skip < 6; skip++)
					MovePastNextDivisor(line, ref pos);
				//var encounterEnd = ParsingUtil.NextSubstring(line, ref pos);
				//var encounterId = ParsingUtil.NextSubstring(line, ref pos);
				//var encounterName = ParsingUtil.NextSubstring(line, ref pos);
				//var difficultyId = ParsingUtil.NextSubstring(line, ref pos);
				//var groupsize = ParsingUtil.NextSubstring(line, ref pos);
				//var succ = ParsingUtil.NextSubstring(line, ref pos);
				encounterDurationInMS = uint.Parse(ParsingUtil.NextSubstring(line, ref pos));
			}
            else
            {
                encounterEndTime = timestamp;
                encounterDurationInMS = uint.MaxValue; //unsuccessfull parse
            }
		}

		private static List<NpcInfo> FindNPCs(List<CombatlogEvent> events)
		{
			//try to find all npcs.
			HashSet<string> npcGuids = new();
			List<NpcInfo> npcs = new();
			foreach (var _event in events)
			{
				(string? guid, string npcName) = _event.SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_CONTROL_NPC) ?
					(_event.SourceGUID, _event.SourceName)
					: _event.TargetFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_CONTROL_NPC) ?
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

			return npcs;
		}

		/// <summary>
		/// try to fill in the name and realm of the players from events theyre involved in.
		/// </summary>
		/// <param name="events"></param>
		/// <param name="players"></param>
		private static void SetPlayerNamesFromEvents(List<CombatlogEvent> events, PlayerInfo[] players)
		{
			foreach (var player in players)
			{
				if (player == null) continue;
				var fullName = events.Find(e => e.SourceGUID == player.GUID)?.SourceName
					?? events.Find(e => e.TargetGUID == player.GUID)?.TargetName ?? string.Empty;
				if (string.IsNullOrEmpty(fullName))
					continue;
				player.SetNameAndRealm(fullName);
			}
		}

		private static void ParseLineInContext(string? line, ParsingContext parsingContext, List<CombatlogEvent> events, CombatlogEventDictionaryBuilder eventDictBuilder)
		{
            if (line is null)
                return;
			int pos = line.IndexOf(timestamp_end_seperator);
			// var timestamp = StringTimestampToDateTime(line[..pos]);
			pos += 2;
			string subevent = NextSubstring(line, ref pos);
			//confirm that the subevent is a combatlog event (it has a prefix and a suffix)
			//other events are to be handled differently.
			if (TryParsePrefixSuffixSubeventF(subevent, out var prefix, out var suffix))
			{
				CombatlogEvent? clevent = CombatlogEvent.Create(line, prefix, suffix);
				if (clevent != null)
				{
					events.Add(clevent);
					eventDictBuilder.Add(clevent);
				}
			}
			//TODO: Other events should also be handled.
			else if (TryParseMiscEventF(subevent, out var ev))
			{
				switch (ev)
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
				parsingContext.RegisterUnhandledSubevent(subevent, line);
			}
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
            if (offHandLevel == 0)
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
        private static PerformanceMetadata[] ProcessPerformances(EncounterInfo encounterInfo, List<PlayerMetadata> playerMetadatas, uint encounterMetadataId)
        {
            if (playerMetadatas.Count == 0)
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
            foreach (var player in encounterInfo.Players)
            {
                if (player == null)
                    continue;
                var ev = encounterInfo.FirstEventForGUID(player.GUID);
                if (ev != null)
                    player.SetNameAndRealm(ev.SourceName);
            }

            //source is friendly or neutral (belongs to the raid/party)
            var filter = EventFilters.AllySourceEnemyTargetFilter;
            var damageEvents = encounterInfo.CombatlogEventDictionary.GetEvents<DamageEvent>()
                .Where(filter.Match);
            var damageSupportEvents = encounterInfo.CombatlogEventDictionary.GetEvents<DamageSupportEvent>();

            //the lookup for source=>actual (owner) is calculated on EncounterInfo, but cache it here for convencience.
            var sourceToOwnerGUID = encounterInfo.SourceToOwnerGuidLookup;

            //add together all the damage events.
            foreach (var ev in damageEvents)
            {
                //try to add the damage done directly to the player by their GUID
                //by this point, the dictionary has ALL possible GUIDs of units in it. therefore this is OK!
                bool sourceIsCorrect = !sourceToOwnerGUID.TryGetValue(ev.SourceGUID, out string? trueSourceGUID);
                if (result.TryGetByGUID(sourceIsCorrect ? ev.SourceGUID : trueSourceGUID!, out var perf))
                {
                    perf!.Dps += (ev.damageParams.amount + ev.damageParams.absorbed);
                }
                //else //source is not player, but a pet/guardian/npc summoned by a player that could not be identified to belong to a player.
                //{

                //}
            }
            //subtract support damage from supported player, add to evoker.
            foreach (var ev in damageSupportEvents)
            {
                bool sourceIsCorrect = !sourceToOwnerGUID.TryGetValue(ev.SourceGUID, out string? trueSourceGUID);
                if (result.TryGetByGUID(sourceIsCorrect ? ev.SourceGUID : trueSourceGUID!, out var perf))
                {
                    perf!.Dps -= (ev.damageParams.amount + ev.damageParams.absorbed);
                }
                if (result.TryGetByGUID(ev.supporterGUID, out perf))
                {
                    perf!.Dps += ev.damageParams.amount + ev.damageParams.absorbed;
                }
            }

            //HEALING 
            //add together all the healing events.
            foreach (var ev in encounterInfo.CombatlogEventDictionary.GetEvents<HealEvent>())
            {
                if (result.TryGetByGUID(ev.SourceGUID, out var perf))
                {
                    perf!.Hps += ev.Amount + ev.Absorbed - ev.Overheal;
                }
                //else //source is not player, but a pet/guardian/npc summoned by a player that could not be identified to belong to a player.
                //{

                //}
            }
            //add absorb
            foreach (var absorbEvent in encounterInfo.CombatlogEventDictionary.GetEvents<SpellAbsorbedEvent>())
            {
                if (result.TryGetByGUID(absorbEvent.AbsorbCasterGUID, out var performance))
                {
                    performance!.Hps += absorbEvent.AbsorbedAmount;
                }
            }
            //attribute healing support correctly
            var healingSupportEvents = encounterInfo.CombatlogEventDictionary.GetEvents<HealSupportEvent>();
            foreach (var ev in healingSupportEvents)
            {
                bool sourceIsCorrect = !sourceToOwnerGUID.TryGetValue(ev.SourceGUID, out string? trueSourceGUID);
                if (result.TryGetByGUID(sourceIsCorrect ? ev.SourceGUID : trueSourceGUID!, out var perf))
                {
                    perf!.Hps -= ev.healParams.amount + ev.healParams.absorbed - ev.healParams.overheal;
                }
                if (result.TryGetByGUID(ev.supporterGUID, out perf))
                {
                    perf!.Hps += ev.healParams.amount + ev.healParams.absorbed - ev.healParams.overheal;
                }
            }

            double encounterDuration = encounterInfo.EncounterDuration / 1000.0;
            foreach (var performance in result)
            {
                if (performance is null) continue;
                if (performance.Dps != 0)
                    performance.Dps /= encounterDuration;
                if (performance.Hps != 0)
                    performance.Hps /= encounterDuration;
                //other performance members need to be assigned toon before returned.
            }
            return result;
        }


    }
}

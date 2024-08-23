using CombatlogParser.Controls;
using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Events.EventData;
using CombatlogParser.Formatting;
using Microsoft.Win32;
using System.Drawing;

using Point = System.Drawing.Point;
using ScottPlot.WPF;
using ScottPlot.TickGenerators;
using CombatlogParser.Data.Events.Filters;

namespace CombatlogParser.Data
{
    public class EncounterInfo
    {
        /// <summary>
        /// The name of the encounter, provided by ENCOUNTER_START
        /// </summary>
        public string EncounterName { get; }

        /// <summary>
        /// the ID of the encounter, provided by ENCOUNTER_START
        /// </summary>
        public EncounterId EncounterID { get; }

        /// <summary>
        /// The difficulty of the encounter. see https://wowpedia.fandom.com/wiki/DifficultyID 
        /// </summary>
        public DifficultyId DifficultyID { get; }

        /// <summary>
        /// The size of the group involved in the encounter.
        /// </summary>
        public int GroupSize { get; }

        /// <summary>
        /// ID if the instance the encounter is in. https://wowpedia.fandom.com/wiki/InstanceID
        /// </summary>
        public uint InstanceID { get; }

        /// <summary>
        /// The timestamp of the ENCOUNTER_START event
        /// </summary>
        public DateTime EncounterStartTime { get; }

        /// <summary>
        /// The timestamp of ENCOUNTER_END
        /// </summary>
        public DateTime EncounterEndTime { get; }

        /// <summary>
        /// The duration of the encounter in milliseconds, provided by ENCOUNTER_END
        /// </summary>
        public uint EncounterDuration { get; }

        /// <summary>
        /// Whether the encounter ended successfully, provided by ENCOUNTER_END
        /// </summary>
        public bool EncounterSuccess { get; }

        /// <summary>
        /// Size of the PlayerInfo[] is given by ENCOUNTER_START, followed by the data from COMBATANT_INFO
        /// </summary>
        public PlayerInfo[] Players { get; }

        /// <summary>
        /// List of NPCs found in the events.
        /// </summary>
        public List<NpcInfo> Npcs { get; }

        /// <summary>
        /// All combatlog events during the encounter.
        /// </summary>
        public CombatlogEvent[] CombatlogEvents { get; }

        /// <summary>
        /// Combatlog Events sorted by data type.
        /// </summary>
        public CombatlogEventDictionary CombatlogEventDictionary { get; }

        public Dictionary<string, string> SourceToOwnerGuidLookup { get; }

        /// <summary>
        /// The duration of the encounter in seconds. Used for DPS calculation.
        /// </summary>
        public float LengthInSeconds
        {
            get => EncounterDuration / 1000;
        }

        public EncounterInfo(
            CombatlogEvent[] allEvents,
            CombatlogEventDictionary eventDictionary,
            DateTime startTime,
            bool success,
            DifficultyId difficultyId,
            EncounterId encounterId,
            string encounterName,
            int groupSize,
            uint encounterDurationInMS,
            DateTime endTime,
            PlayerInfo[] players,
            List<NpcInfo> npcs) //could be an array aswell 
        {
            CombatlogEvents = allEvents;
            CombatlogEventDictionary = eventDictionary;
            EncounterStartTime = startTime;
            EncounterEndTime = endTime;
            Players = players;
            EncounterSuccess = success;
            DifficultyID = difficultyId;
            EncounterName = encounterName;
            EncounterID = encounterId;
            GroupSize = groupSize;
            EncounterDuration = encounterDurationInMS;
            Npcs = npcs;

            //initialize the lookup table
            SourceToOwnerGuidLookup = new();
            foreach (var summon in CombatlogEventDictionary.GetEvents<SummonEvent>())
            {
                //the summoned "pet" is the targetGUID of the event.
                if (SourceToOwnerGuidLookup.ContainsKey(summon.TargetGUID) == false)
                    SourceToOwnerGuidLookup.Add(summon.TargetGUID, summon.SourceGUID);
            }
            foreach (var e in CombatlogEventDictionary.GetEvents<AdvancedParamEvent>())
            {
                var sourceGUID = e.SourceGUID;
                //if the source unit is the advanced info unit
                if (sourceGUID != e.AdvancedParams.infoGUID)
                    continue;
                var owner = e.AdvancedParams.ownerGUID;
                //"000000000000" is the default GUID for "no owner".
                //regular GUIDs start with "Player", "Creature" or "Pet".
                if (SourceToOwnerGuidLookup.ContainsKey(sourceGUID) == false)
                    SourceToOwnerGuidLookup.Add(sourceGUID, owner[0] == '0' ? sourceGUID : owner);
            }
        }

        public CombatlogEvent? FirstEventForGUID(string guid)
            => CombatlogEvents.FirstOrDefault(x => x.SourceGUID == guid);

        public PlayerInfo? FindPlayerInfoByGUID(string sourceGUID)
        {
            foreach (var p in Players)
            {
                if (p.GUID == sourceGUID)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="sourceGUID"></param>
        /// <returns>A dictionary of either a units GUID or a spell name mapped to </returns>
        public Dictionary<string, long> CalculateBreakdown(BreakdownMode mode, bool alliesAsSource)
        {
            if (mode == BreakdownMode.DamageDone)
            {
                return GenerateDamageDoneBreakdown(alliesAsSource);
            }
			if (mode == BreakdownMode.HealingDone)
			{
				return GenerateHealingDoneBreakdown(alliesAsSource);
			}
			if (mode == BreakdownMode.DamageTaken)
			{
				return GenerateDamageTakenBreakdown(alliesAsSource);
			}
			if (mode == BreakdownMode.Casts)
			{
				return CalculateCastsPerEntity(alliesAsSource);
			}
            return new();
        }
		public Dictionary<SpellData, long> CalculateBreakdownForEntity(BreakdownMode mode, string entityGUID)
		{
			return mode switch {
				BreakdownMode.DamageDone => GenerateDamageDoneBySource(entityGUID),
				BreakdownMode.DamageTaken => GenerateDamageTakenByEntityGUID(entityGUID),
				BreakdownMode.HealingDone => GenerateHealingDoneBySource(entityGUID),
				BreakdownMode.Casts => CalculateCastsForEntity(entityGUID),
				_ => new()
			};
		}

		//TODO: for damage, figure out whether Absorb events are relevant. they might be.
		private Dictionary<string, long> GenerateDamageDoneBreakdown(bool alliesAsSource)
		{
			Dictionary<string, long> damageBySource = new();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			EventFilter filter = alliesAsSource? EventFilters.AllySourceEnemyTargetFilter : EventFilters.EnemySourceFilter;
			SumAmountsForSources(
				damageEvents.Where(filter.Match),
				dmgEvent => dmgEvent.DamageParams.TotalAmount,
				damageBySource);
			//damage done via support events (augmentation evoker) needs to be attributed to the evoker
			//and subtracted from the buffed players damage.
			var damageSupportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var dmgEvent in damageSupportEvents)
			{
				var damageSupported = dmgEvent.DamageParams.TotalAmount;
				AddSum(damageBySource, dmgEvent.SourceGUID, -damageSupported);
				AddSum(damageBySource, dmgEvent.SupporterGUID, damageSupported);
			}
			return damageBySource;
		}

		private Dictionary<SpellData, long> GenerateDamageDoneBySource(string selectedEntityGUID)
		{
			Dictionary<SpellData, long> damageBySpell = new();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			foreach (var dmgEvent in damageEvents.Where(x => GetActualSourceGUID(x.SourceGUID) == selectedEntityGUID))
			{
				AddSum(damageBySpell, dmgEvent.SpellData, dmgEvent.DamageParams.TotalAmount);
			}
			//once again, subtract any support dmg
			var supportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var supportEvent in supportEvents.Where(x => x.SourceGUID == selectedEntityGUID))
			{
				AddSum(damageBySpell, supportEvent.SpellData, -supportEvent.DamageParams.TotalAmount);
			}
            return damageBySpell;
		}


		 //TODO: for healing, figure out whether Absorb events are relevant. they might be.
		private Dictionary<string, long> GenerateHealingDoneBreakdown(bool alliesAsSource)
		{
			Dictionary<string, long> healingBySource = new();
			var healEvents = CombatlogEventDictionary.GetEvents<HealEvent>();
			EventFilter filter = alliesAsSource ? EventFilters.AllySourceFilter : EventFilters.EnemySourceFilter;
			SumAmountsForSources(
				healEvents.Where(filter.Match),
				healEvent => healEvent.HealParams.amount + healEvent.HealParams.absorbed,
				healingBySource);
			//damage done via support events (augmentation evoker) needs to be attributed to the evoker
			//and subtracted from the buffed players damage.
			var damageSupportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var supportEvent in damageSupportEvents)
			{
				var damageSupported = supportEvent.DamageParams.amount + supportEvent.DamageParams.absorbed;
				if (healingBySource.ContainsKey(supportEvent.SourceGUID))
				{
					healingBySource[supportEvent.SourceGUID] -= damageSupported;
				}
				AddSum(healingBySource, supportEvent.SupporterGUID, damageSupported);
			}
			return healingBySource;
		}

		/// <summary>
		/// Calculates how much healing the selected entity did with each spell.
		/// </summary>
		/// <param name="selectedEntityGUID"></param>
		/// <returns></returns>
		private Dictionary<SpellData, long> GenerateHealingDoneBySource(string selectedEntityGUID)
		{
			Dictionary<SpellData, long> healingBySpell = new();
			var healingEvents = CombatlogEventDictionary.GetEvents<HealEvent>();
			foreach (var healEvent in healingEvents.Where(x => GetActualSourceGUID(x.SourceGUID) == selectedEntityGUID))
			{
				AddSum(healingBySpell, healEvent.SpellData, healEvent.HealParams.TotalAmount);
			}
			//once again, subtract any support dmg
			var supportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var supportEvent in supportEvents.Where(x => x.SourceGUID == selectedEntityGUID))
			{
				healingBySpell[supportEvent.SpellData] -= supportEvent.DamageParams.TotalAmount;
			}
			return healingBySpell;
		}

		/// <summary>
		/// Calculate and show the totals of each players damage taken.
		/// </summary>
		private Dictionary<string, long> GenerateDamageTakenBreakdown(bool byAlly)
		{
			Dictionary<string, long> dmgTakenByTarget = new();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			EventFilter filter = byAlly ? EventFilters.GroupMemberTargetFilter : EventFilters.EnemyTargetFilter;
			foreach (var dmgEvent in damageEvents.Where(filter.Match))
			{
				AddSum(dmgTakenByTarget, dmgEvent.TargetGUID, dmgEvent.DamageParams.TotalAmount);
			}
			return dmgTakenByTarget;
		}

		/// <summary>
		/// Calculate and show the totals of each players damage taken.
		/// </summary>
		private Dictionary<SpellData, long> GenerateDamageTakenByEntityGUID(string entityGUID)
		{
			Dictionary<SpellData, long> dmgTakenBySpell = new();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			EventFilter filter = new TargetGUIDFilter(entityGUID);
			foreach (var dmgEvent in damageEvents.Where(filter.Match))
			{
				AddSum(dmgTakenBySpell, dmgEvent.SpellData, dmgEvent.DamageParams.TotalAmount);
			}
			return dmgTakenBySpell;
		}

		/// <summary>
		/// Sums up all known player deaths alongside the time it took from the last time they were at 90% or more hp.
		/// </summary>
		/// <returns>All player deaths ordered by time in the encounter</returns>
		public PlayerDeathDataRow[] GetPlayerDeathInfo()
		{
			List<PlayerDeathDataRow> deathData = new();
			var startTime = EncounterStartTime;
			//yes, all of this data is required.
			var unitDiedEvents = CombatlogEventDictionary.GetEvents<UnitDiedEvent>();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			var advancedInfoEvents = CombatlogEventDictionary.GetEvents<AdvancedParamEvent>();
			//hard-coded to only show death info for players right now. parameterize UnitFlag at a later time.
			foreach (var deathEvent in unitDiedEvents.Where(x => x.TargetFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PLAYER)))
			{
				//playerInfo is pretty much only used for coloring via the class brush.
				PlayerInfo? player = FindPlayerInfoByGUID(deathEvent.TargetGUID);
				var offsetTime = (deathEvent.Timestamp - startTime);
				var formattedTimestamp = offsetTime.ToString(@"m\:ss\.fff");
				if (player is not null)
				{
					var beforeFilter = EventFilters.Before(deathEvent.Timestamp);
					var last3hits = damageEvents.Reverse().Where(x => x.TargetGUID == deathEvent.TargetGUID)
							.Where(beforeFilter.Match)
							.Take(3).ToArray();
					// the time since the last time someone was at above 90% hp should give a rough estimate
					// of how quickly they died.
					// WARNING: there is an important edge case here:
					// someone getting resurrected and dying again before they
					// ever reach a high health % again. It would take the last time they were at high health before the first death.
					var lastTimeFullHealth = advancedInfoEvents.Reverse().Where(beforeFilter.Match)
						.Where(x =>
						{
							var aparams = x.AdvancedParams;
							return aparams.infoGUID == player.GUID && aparams.currentHP >= 0.90 * aparams.maxHP;
						}
						).FirstOrDefault()?.Timestamp ?? deathEvent.Timestamp;
					var slowDeathTimeOffset = deathEvent.Timestamp - lastTimeFullHealth;
					//TODO: It should also read instant when the killing damage event does more damage than the players max health.
					var formattedDeathTime = slowDeathTimeOffset.TotalMilliseconds <= 100 ? 
						"Instant" : $"{slowDeathTimeOffset:ss\\.fff} seconds";

					deathData.Add(
						new(player.Name,
						player.Class.GetClassBrush(),
						formattedTimestamp,
						abilityName: last3hits[^1].SpellData.name,
						lastHits: last3hits.Select(x => x.SpellData.name).ToArray(), // last3hits.LastOrDefault()?.spellData.name ?? "unknown"
						formattedDeathTime
						)
					);
				}
			}
			return deathData.ToArray();
		}

		/// <summary>
		/// Sums up the total successful spell casts by every entity grouped by alignment. (ally or enemy)
		/// </summary>
		/// <param name="isAllies"></param>
		/// <returns></returns>
		private Dictionary<string, long> CalculateCastsPerEntity(bool isAllies)
		{
			Dictionary<string, long> results = new();
			EventFilter filter = isAllies ? EventFilters.AllySourceFilter : EventFilters.EnemySourceFilter;
			var events = CombatlogEventDictionary.GetEvents<CastSuccessEvent>();
			foreach(var castEvent in events.Where(filter.Match))
			{
				var sourceGuid = GetActualSourceGUID(castEvent.SourceGUID);
				if (results.ContainsKey(sourceGuid))
				{
					results[sourceGuid] += 1;
				}
				else
				{
					results[sourceGuid] = 1;
				}
			}
			return results;
		}

		/// <summary>
		/// Sums up how many times an entity casted each spell during the encounter.
		/// </summary>
		/// <param name="entityGUID"></param>
		/// <returns></returns>
		private Dictionary<SpellData, long> CalculateCastsForEntity(string entityGUID)
		{
			Dictionary<SpellData, long> results = new();
			var events = CombatlogEventDictionary.GetEvents<CastSuccessEvent>();
			foreach (var castEvent in events.Where(x => GetActualSourceGUID(x.SourceGUID) == entityGUID))
			{
				AddSum(results, castEvent.SpellData, 1);
			}
			return results;
		}

		/// <summary>
		/// Add an amount to a guid's sum in the dictionary. Small helper to avoid duplicate code.
		/// </summary>
		/// <param name="sums"></param>
		/// <param name="key"></param>
		/// <param name="amount"></param>
		private static void AddSum<TKey>(Dictionary<TKey, long> sums, TKey key, long amount) where TKey : notnull
		{
			if (sums.ContainsKey(key))
			{
				sums[key] += amount;
			}
			else
			{
				sums[key] = amount;
			}
		}

		/// <summary>
		/// Sums up the amounts in the given events by their Source
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="events"></param>
		/// <param name="amountGetter">Determines the amount. Different events have different ways of calculating it.</param>
		/// <param name="sumDictionary">The dictionary it outputs into.</param>
		private void SumAmountsForSources<T>(
			IEnumerable<T> events,
			Func<T, long> amountGetter,
			Dictionary<string, long> sumDictionary)
			where T : CombatlogEvent
		{
			foreach (var ev in events)
			{
				string sourceGuid = GetActualSourceGUID(ev.SourceGUID);
				long amount = amountGetter(ev);
				AddSum(sumDictionary, sourceGuid, amount);
			}
		}

		/// <summary>
		/// Looks up the given GUID in the encounters SourceToOwnerGuidLookup Dictionary.
		/// </summary>
		/// <param name="possibleSource"></param>
		/// <returns>The Value in the Lookup if the key exists</returns>
		private string GetActualSourceGUID(string possibleSource)
		{
			if (SourceToOwnerGuidLookup.TryGetValue(possibleSource, out string? value))
			{
				return value;
			}
			return possibleSource;
		}

		/// <summary>
		/// Available modes for breakdown generation via <see cref="CalculateBreakdown(BreakdownMode, bool)"/>.
		/// May be merged with <see cref="SingleEncounterViewMode"/> at a later time.
		/// </summary>
		public enum BreakdownMode
        {
            DamageDone, 
            HealingDone,
            DamageTaken,
            Casts,
            //Deaths, doesnt really fit in with the other ones.
		}

		/// <summary>
		/// Exports an image of the movement of all players as a png image to a specified location.
		/// </summary>
		public void ExportPlayerMovementAsImage()
		{
			const int resolution = 1024;
			Bitmap bitmap = new(resolution, resolution);
			using var graphics = Graphics.FromImage(bitmap);
			//set background
			graphics.FillRectangle(System.Drawing.Brushes.Black, new(0, 0, resolution, resolution));

			var events = CombatlogEventDictionary.GetEvents<AdvancedParamEvent>().Select(x => x.AdvancedParams);

			const float padding = 5f;
			float minX, maxX, minY, maxY;
			minX = events.Min(x => x.positionX);
			maxX = events.Max(x => x.positionX);
			minY = events.Min(x => x.positionY);
			maxY = events.Max(x => x.positionY);
			{ //make it a square.
				var width = maxX - minX;
				var height = maxY - minY;
				var halfExtents = Math.Max(width, height) * 0.5f + padding;
				var centerX = MathUtil.Average(minX, maxX);
				var centerY = MathUtil.Average(minY, maxY);
				minX = centerX - halfExtents;
				maxX = centerX + halfExtents;
				minY = centerY - halfExtents;
				maxY = centerY + halfExtents;
			}

			var lastPixelPosition = new Dictionary<string, Point>();

			foreach (var entry in events)
			{
				var unitGUID = entry.infoGUID;
				var player = FindPlayerInfoByGUID(unitGUID);
				if (player == null)
					continue;

				int x = (int)(MathUtil.InverseLerp(entry.positionX, minX, maxX) * resolution);
				int y = (int)(MathUtil.InverseLerp(entry.positionY, minY, maxY) * resolution);
				Point position = new(x, y);

				if (lastPixelPosition.TryGetValue(unitGUID, out Point value))
				{
					var color = player.Class.GetClassColor();
					graphics.DrawLine(new System.Drawing.Pen(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B), 2), value, position);
				}
				lastPixelPosition[unitGUID] = position;
			}

			var fileDialog = new SaveFileDialog
			{
				AddExtension = true,
				DefaultExt = "png",
				Filter = "PNG image|*.png",
				FileName = $"{EncounterName}_{DifficultyID}.png"
			};
			if (fileDialog.ShowDialog() == true)
				bitmap.Save(fileDialog.FileName);
		}

		/// <summary>
		/// Attempt to get the name of a unit through combatlog Player or Npc information.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public string GetUnitNameOrFallback(string guid)
		{
			return Players.FirstOrDefault(x => x.GUID == guid)?.Name
				?? Npcs.FirstOrDefault(x => x.InstanceGuids.Contains(guid))?.Name
				?? "Unknown";
		}

		/// <summary>
		/// Currently hardcoded to only ever 
		/// </summary>
		/// <param name="metricPlot"></param>
		/// <param name="graphWidth"></param>
		public void PlotGraph(WpfPlot metricPlot)
		{
			int seconds = (int)EncounterDuration / 1000;
			long[] damagePerSecond = new long[seconds];

			var dmgEvents = CombatlogEventDictionary.GetEvents<DamageEvent>().Where(EventFilters.AllySourceEnemyTargetFilter.Match);
			foreach (var dmgEvent in dmgEvents)
			{
				try
				{
					var second = (int)(dmgEvent.Timestamp - EncounterStartTime).TotalSeconds;
					damagePerSecond[second] += dmgEvent.DamageParams.TotalAmount;
				} 
				catch (IndexOutOfRangeException)
				{
					damagePerSecond[seconds-1] += dmgEvent.DamageParams.TotalAmount;
				}
			}
			var maxDps = damagePerSecond.Max();

			long[] timestamps = new long[seconds];
			for (int i = 0; i < seconds; i++)
			{
				timestamps[i] = i;
			}

			ScottPlot.Plot myPlot = metricPlot.Plot;
			myPlot.Clear();
			var dpsGraph = myPlot.Add.Signal(damagePerSecond);
			dpsGraph.LegendText = "DPS";
			myPlot.Axes.Bottom.TickGenerator = new NumericAutomatic() { LabelFormatter = NumberFormatting.SecondsToMinutesAndSeconds };
			myPlot.Axes.Left.TickGenerator = new NumericAutomatic() { LabelFormatter = NumberFormatting.ToShortFormString };
			myPlot.Axes.SetLimitsY(0, maxDps * 1.1d);
			myPlot.Axes.SetLimitsX(0, seconds + 1);
			myPlot.ShowLegend();
			metricPlot.Refresh();
		}
	}
}

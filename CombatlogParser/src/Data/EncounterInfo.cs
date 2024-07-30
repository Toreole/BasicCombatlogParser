using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Events;
using CombatlogParser.Formatting;
using System.Numerics;
using System.Windows.Controls;
using static CombatlogParser.Controls.SingleEncounterView;

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
        public Dictionary<string, long> CalculateBreakdown(BreakdownMode mode, bool alliesAsSource, string? sourceGUID)
        {
            if (mode == BreakdownMode.DamageDone)
            {
                return sourceGUID == null ? GenerateDamageDoneBreakdown(alliesAsSource) : GenerateDamageDoneBySource(sourceGUID);
            }
			if (mode == BreakdownMode.HealingDone)
			{
				return sourceGUID == null ? GenerateHealingDoneBreakdown(alliesAsSource) : GenerateHealingDoneBySource(sourceGUID);
			}
			if (mode == BreakdownMode.DamageTaken)
			{
				return sourceGUID == null ? GenerateDamageTakenBreakdown(alliesAsSource) : GenerateDamageTakenByEntityGUID(sourceGUID);
			}
            return new();
        }

		private Dictionary<string, long> GenerateDamageDoneBreakdown(bool alliesAsSource)
		{
			Dictionary<string, long> damageBySource = new();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			EventFilter filter = alliesAsSource? EventFilters.AllySourceEnemyTargetFilter : EventFilters.EnemySourceFilter;
			SumAmountsForSources(
				damageEvents.Where(filter.Match),
				dmgEvent => dmgEvent.damageParams.TotalAmount,
				damageBySource);
			//damage done via support events (augmentation evoker) needs to be attributed to the evoker
			//and subtracted from the buffed players damage.
			var damageSupportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var dmgEvent in damageSupportEvents)
			{
				var damageSupported = dmgEvent.damageParams.TotalAmount;
				if (damageBySource.ContainsKey(dmgEvent.SourceGUID))
				{
					damageBySource[dmgEvent.SourceGUID] -= damageSupported;
				}
				AddSum(damageBySource, dmgEvent.supporterGUID, damageSupported);
			}
			return damageBySource;
		}

		private Dictionary<string, long> GenerateDamageDoneBySource(string selectedEntityGUID)
		{
			Dictionary<string, long> damageBySpell = new();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			foreach (var dmgEvent in damageEvents.Where(x => GetActualSourceGUID(x.SourceGUID) == selectedEntityGUID))
			{
				if (damageBySpell.ContainsKey(dmgEvent.spellData.name))
				{
					damageBySpell[dmgEvent.spellData.name] += dmgEvent.damageParams.TotalAmount;
				}
				else
				{
					damageBySpell.Add(dmgEvent.spellData.name, dmgEvent.damageParams.TotalAmount);
				}
			}
			//once again, subtract any support dmg
			var supportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var supportEvent in supportEvents.Where(x => x.SourceGUID == selectedEntityGUID))
			{
				damageBySpell[supportEvent.spellData.name] -= supportEvent.damageParams.TotalAmount;
			}
            return damageBySpell;
		}

		private Dictionary<string, long> GenerateHealingDoneBreakdown(bool alliesAsSource)
		{
			Dictionary<string, long> healingBySource = new();
			var healEvents = CombatlogEventDictionary.GetEvents<HealEvent>();
			EventFilter filter = alliesAsSource ? EventFilters.AllySourceFilter : EventFilters.EnemySourceFilter;
			SumAmountsForSources(
				healEvents.Where(filter.Match),
				healEvent => healEvent.healParams.amount + healEvent.healParams.absorbed,
				healingBySource);
			//damage done via support events (augmentation evoker) needs to be attributed to the evoker
			//and subtracted from the buffed players damage.
			var damageSupportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var supportEvent in damageSupportEvents)
			{
				var damageSupported = supportEvent.damageParams.amount + supportEvent.damageParams.absorbed;
				if (healingBySource.ContainsKey(supportEvent.SourceGUID))
				{
					healingBySource[supportEvent.SourceGUID] -= damageSupported;
				}
				AddSum(healingBySource, supportEvent.supporterGUID, damageSupported);
			}
			return healingBySource;
		}

		private Dictionary<string, long> GenerateHealingDoneBySource(string selectedEntityGUID)
		{
			Dictionary<string, long> healingBySpell = new();
			var healingEvents = CombatlogEventDictionary.GetEvents<HealEvent>();
			foreach (var healEvent in healingEvents.Where(x => GetActualSourceGUID(x.SourceGUID) == selectedEntityGUID))
			{
				if (healingBySpell.ContainsKey(healEvent.spellData.name))
				{
					healingBySpell[healEvent.spellData.name] += healEvent.healParams.TotalAmount;
				}
				else
				{
					healingBySpell.Add(healEvent.spellData.name, healEvent.healParams.TotalAmount);
				}
			}
			//once again, subtract any support dmg
			var supportEvents = CombatlogEventDictionary.GetEvents<DamageSupportEvent>();
			foreach (var supportEvent in supportEvents.Where(x => x.SourceGUID == selectedEntityGUID))
			{
				healingBySpell[supportEvent.spellData.name] -= supportEvent.damageParams.TotalAmount;
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
				var target = dmgEvent.TargetGUID;
				var amount = dmgEvent.damageParams.TotalAmount;
				AddSum(dmgTakenByTarget, target, amount);
			}
			return dmgTakenByTarget;
		}
		/// <summary>
		/// Calculate and show the totals of each players damage taken.
		/// </summary>
		private Dictionary<string, long> GenerateDamageTakenByEntityGUID(string entityGUID)
		{
			Dictionary<string, long> dmgTakenBySpell = new();
			var damageEvents = CombatlogEventDictionary.GetEvents<DamageEvent>();
			EventFilter filter = new TargetGUIDFilter(entityGUID);
			foreach (var dmgEvent in damageEvents.Where(filter.Match))
			{
				var target = dmgEvent.spellData.name;
				var amount = dmgEvent.damageParams.TotalAmount;
				AddSum(dmgTakenBySpell, target, amount);
			}
			return dmgTakenBySpell;
		}

		/// <summary>
		/// Add an amount to a guid's sum in the dictionary. Small helper to avoid duplicate code.
		/// </summary>
		/// <param name="sums"></param>
		/// <param name="guid"></param>
		/// <param name="amount"></param>
		private static void AddSum(Dictionary<string, long> sums, string guid, long amount)
		{
			if (sums.ContainsKey(guid))
			{
				sums[guid] += amount;
			}
			else
			{
				sums[guid] = amount;
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

		public enum BreakdownMode
        {
            DamageDone, 
            HealingDone,
            DamageTaken,
            Casts,
            //Deaths, doesnt really fit in with the other ones.
		}
    }
}

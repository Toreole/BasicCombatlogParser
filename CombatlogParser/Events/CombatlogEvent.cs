using CombatlogParser.Data.WowEnums;
using CombatlogParser.Parsing;
using static CombatlogParser.Parsing.ParsingUtil;

namespace CombatlogParser.Events;

/// <summary>
/// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
/// </summary>
public abstract class CombatlogEvent : LogEntryBase
{
	public CombatlogEventPrefix SubeventPrefix { get; private set; } = CombatlogEventPrefix.UNDEFINED;
	public CombatlogEventSuffix SubeventSuffix { get; private set; } = CombatlogEventSuffix.UNDEFINED;

	//these 8 parameters are guaranteed to be included in combatlog events.
	public string SourceGUID { get; private set; }
	public string SourceName { get; private set; }
	public UnitFlag SourceFlags { get; private set; }
	public RaidFlag SourceRaidFlags { get; private set; }
	public string TargetGUID { get; private set; }
	public string TargetName { get; private set; }
	public UnitFlag TargetFlags { get; private set; }
	public RaidFlag TargetRaidFlags { get; private set; }

	public EventType EventType { get; private set; } = EventType.UNDEFINED;

	public bool IsSourcePet => SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PET);

	//inheriting types should use these params: CombatlogEventPrefix prefix, string entry, int dataIndex
	//EventType and Suffix are provided by the inheriting type itself. One of them is a duplicate tbh.
	protected CombatlogEvent(string entry, ref int dataIndex, EventType eventType, CombatlogEventPrefix prefix, CombatlogEventSuffix suffix)
	{
		Timestamp = StringTimestampToDateTime(entry[..entry.IndexOf(timestamp_end_seperator)]);
		SourceGUID = string.Intern(NextSubstring(entry, ref dataIndex));
		SourceName = string.Intern(NextSubstring(entry, ref dataIndex));
		SourceFlags = NextFlags(entry, ref dataIndex);
		SourceRaidFlags = NextRaidFlags(entry, ref dataIndex);
		TargetGUID = string.Intern(NextSubstring(entry, ref dataIndex));
		TargetName = string.Intern(NextSubstring(entry, ref dataIndex));
		TargetFlags = NextFlags(entry, ref dataIndex);
		TargetRaidFlags = NextRaidFlags(entry, ref dataIndex);

		EventType = eventType;
		SubeventPrefix = prefix;
		SubeventSuffix = suffix;
	}

	public static CombatlogEvent? Create(string combatlogEntry, CombatlogEventPrefix prefix, CombatlogEventSuffix suffix)
	{
		//start after the two empty spaces behind the timestamp.
		int index = combatlogEntry.IndexOf(timestamp_end_seperator);
		//make sure that its a valid combat event.
		//if (prefix is CombatlogEventPrefix.PARTY || 
		//    prefix is CombatlogEventPrefix.UNIT || 
		//    prefix is CombatlogEventPrefix.ENVIRONMENTAL)
		//    return null;
		MovePastNextDivisor(combatlogEntry, ref index); //subevent skipped
														//divide into basic 
		CombatlogEvent? ev = suffix switch
		{
			CombatlogEventSuffix._DAMAGE => new DamageEvent(prefix, combatlogEntry, index),
			//case CombatlogEventSuffix._DURABILITY_DAMAGE:
			//    break;
			//case CombatlogEventSuffix._DAMAGE_LANDED:
			//    break;
			//TODO: CombatlogEventSuffix._DAMAGE_LANDED_SUPPORT => new DamageSupportEvent(...)
			CombatlogEventSuffix._DAMAGE_SUPPORT => new DamageSupportEvent(prefix, combatlogEntry, index),
			//case CombatlogEventSuffix._MISSED:
			//    break;
			CombatlogEventSuffix._HEAL => new HealEvent(prefix, combatlogEntry, index),
			CombatlogEventSuffix._HEAL_SUPPORT => new HealSupportEvent(prefix, combatlogEntry, index),
			//case CombatlogEventSuffix._HEAL_ABSORBED:
			//    break;
			CombatlogEventSuffix._ABSORBED => new SpellAbsorbedEvent(combatlogEntry, index),
			//case CombatlogEventSuffix._ENERGIZE:
			//    break;
			//case CombatlogEventSuffix._DRAIN:
			//    break;
			//case CombatlogEventSuffix._LEECH:
			//    break;
			//case CombatlogEventSuffix._INTERRUPT:
			//    break;
			//case CombatlogEventSuffix._DISPEL:
			//    break;
			//case CombatlogEventSuffix._DISPEL_FAILED:
			//    break;
			//case CombatlogEventSuffix._STOLEN:
			//    break;
			//case CombatlogEventSuffix._EXTRA_ATTACKS:
			//    break;
			//case CombatlogEventSuffix._AURA_APPLIED:
			//    break;
			//case CombatlogEventSuffix._AURA_REMOVED:
			//    break;
			//case CombatlogEventSuffix._AURA_APPLIED_DOSE:
			//    break;
			//case CombatlogEventSuffix._AURA_REMOVED_DOSE:
			//    break;
			//case CombatlogEventSuffix._AURA_REFRESH:
			//    break;
			//case CombatlogEventSuffix._AURA_BROKEN:
			//    break;
			//case CombatlogEventSuffix._AURA_BROKEN_SPELL:
			//    break;
			//case CombatlogEventSuffix._CAST_START:
			//    break;
			CombatlogEventSuffix._CAST_SUCCESS => new CastSuccessEvent(combatlogEntry, index),
			//    break;
			//case CombatlogEventSuffix._CAST_FAILED:
			//    break;
			//case CombatlogEventSuffix._INSTAKILL:
			//    break;
			//case CombatlogEventSuffix._CREATE:
			//    break;
			//case CombatlogEventSuffix._DURABILITY_DAMAGE_ALL:
			//    break;
			CombatlogEventSuffix._SUMMON => new SummonEvent(prefix, combatlogEntry, index),
			//case CombatlogEventSuffix._RESURRECT:
			//    break;
			//case CombatlogEventSuffix._SPLIT:
			//    break;
			//case CombatlogEventSuffix._SHIELD:
			//    break;
			//case CombatlogEventSuffix._REMOVED:
			//    break;
			//case CombatlogEventSuffix._APPLIED:
			//    break;
			//case CombatlogEventSuffix._KILL:
			//    break;
			CombatlogEventSuffix._DIED => new UnitDiedEvent(prefix, combatlogEntry, index),
			//    break;
			//case CombatlogEventSuffix._DESTROYED:
			//    break;
			//case CombatlogEventSuffix._DISSIPATES:
			//    break;
			_ or CombatlogEventSuffix.UNDEFINED => null
		};
		return ev;
	}
}
using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events
{
    /// <summary>
    /// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
    /// </summary>
    public class CombatlogEvent : LogEntryBase
    {
        public CombatlogEventPrefix SubeventPrefix { get; private set; } = CombatlogEventPrefix.UNDEFINED;
        public CombatlogEventSuffix SubeventSuffix { get; private set; } = CombatlogEventSuffix.UNDEFINED;

        //these 8 parameters are guaranteed to be included in combatlog events.
        public string SourceGUID { get; private set; } = "";
        public string SourceName { get; private set; } = "";
        public UnitFlag SourceFlags { get; private set; } = 0x0;
        public RaidFlag SourceRaidFlags { get; private set; } = 0x0;
        public string TargetGUID { get; private set; } = "";
        public string TargetName { get; private set; } = "";
        public UnitFlag TargetFlags { get; private set; } = 0x0;
        public RaidFlag TargetRaidFlags { get; private set; } = 0x0;

        public readonly EventType eventType = EventType.UNDEFINED;

        public bool IsSourcePet => SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PET);

        //inheriting types should use these params: CombatlogEventPrefix prefix, string entry, int dataIndex
        //EventType and Suffix are provided by the inheriting type itself. One of them is a duplicate tbh.
        protected CombatlogEvent(string entry, ref int dataIndex, EventType eventType, CombatlogEventPrefix prefix, CombatlogEventSuffix suffix)
        {
            Timestamp = StringTimestampToDateTime(entry[..entry.IndexOf(timestamp_end_seperator)]);
            SourceGUID = NextSubstring(entry, ref dataIndex);
            SourceName = NextSubstring(entry, ref dataIndex);
            SourceFlags = NextFlags(entry, ref dataIndex);
            SourceRaidFlags = NextRaidFlags(entry, ref dataIndex);
            TargetGUID = NextSubstring(entry, ref dataIndex);
            TargetName = NextSubstring(entry, ref dataIndex);
            TargetFlags = NextFlags(entry, ref dataIndex);
            TargetRaidFlags = NextRaidFlags(entry, ref dataIndex);

            this.eventType = eventType;
            SubeventPrefix = prefix;
            SubeventSuffix = suffix;
        }

        public static CombatlogEvent? Create(string combatlogEntry, CombatlogEventPrefix prefix, CombatlogEventSuffix suffix)
        {
            //start after the two empty spaces behind the timestamp.
            int index = combatlogEntry.IndexOf(timestamp_end_seperator);
            //make sure that its a valid combat event.
            if (prefix is CombatlogEventPrefix.PARTY || 
                prefix is CombatlogEventPrefix.UNIT || 
                prefix is CombatlogEventPrefix.ENVIRONMENTAL)
                return null;
            MovePastNextDivisor(combatlogEntry, ref index); //subevent skipped
            //divide into basic 
            CombatlogEvent? ev = suffix switch
            {
                CombatlogEventSuffix._DAMAGE => new DamageEvent(prefix, combatlogEntry, index),
                //case CombatlogEventSuffix._DURABILITY_DAMAGE:
                //    break;
                //case CombatlogEventSuffix._DAMAGE_LANDED:
                //    break;
                //case CombatlogEventSuffix._MISSED:
                //    break;
                CombatlogEventSuffix._HEAL => new HealEvent(prefix, combatlogEntry, index),
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
                //case CombatlogEventSuffix._DIED:
                //    break;
                //case CombatlogEventSuffix._DESTROYED:
                //    break;
                //case CombatlogEventSuffix._DISSIPATES:
                //    break;
                _ or CombatlogEventSuffix.UNDEFINED => null
            };
            return ev;
        }

        protected void GetSpellPrefixData(string entry, ref int index, out int spellId, out string spellName, out SpellSchool spellSchool)
        {
            spellId = int.Parse(NextSubstring(entry, ref index));
            spellName = NextSubstring(entry, ref index);
            spellSchool = (SpellSchool)HexStringToUInt(NextSubstring(entry, ref index));
        }
    }

    public class AdvancedParams
    {
        public readonly string infoGUID;
        public readonly string ownerGUID;
        public readonly int currentHP;
        public readonly int maxHP;
        public readonly int attackPower;
        public readonly int spellPower;
        public readonly int armor;
        public readonly int absorb;
        public readonly PowerType powerType;
        public readonly int currentPower;
        public readonly int maxPower;
        public readonly int powerCost;
        public readonly float positionX;
        public readonly float positionY;
        public readonly int uiMapID;
        public readonly float facing;
        public readonly int level;

        public AdvancedParams(string data, ref int dataIndex)
        {
            infoGUID = NextSubstring(data, ref dataIndex);
            ownerGUID = NextSubstring(data, ref dataIndex);
            currentHP = int.Parse(NextSubstring(data, ref dataIndex));
            maxHP = int.Parse(NextSubstring(data, ref dataIndex));
            attackPower = int.Parse(NextSubstring(data, ref dataIndex));
            spellPower = int.Parse(NextSubstring(data, ref dataIndex));
            armor = int.Parse(NextSubstring(data, ref dataIndex));
            absorb = int.Parse(NextSubstring(data, ref dataIndex));
            powerType = (PowerType)int.Parse(NextSubstring(data, ref dataIndex));
            currentPower = int.Parse(NextSubstring(data, ref dataIndex));
            maxPower = int.Parse(NextSubstring(data, ref dataIndex));
            powerCost = int.Parse(NextSubstring(data, ref dataIndex));
            positionX = float.Parse(NextSubstring(data, ref dataIndex));
            positionY = float.Parse(NextSubstring(data, ref dataIndex));
            uiMapID = int.Parse(NextSubstring(data, ref dataIndex));
            facing = float.Parse(NextSubstring(data, ref dataIndex));
            level = int.Parse(NextSubstring(data, ref dataIndex));
        }
    }
}

using System.Windows.Navigation;
using static CombatlogParser.ParsingUtil;

namespace CombatlogParser.Data.Events
{
    /// <summary>
    /// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
    /// This is a somewhat polymorphic class.
    /// </summary>
    public class CombatlogEvent : LogEntryBase
    {
        /// <summary>
        /// The advanced combatlog parameters.
        /// </summary>
        private object[] advancedParams = Array.Empty<string>();

        public CombatlogEventPrefix SubeventPrefix { get; private set; } = CombatlogEventPrefix.UNDEFINED;
        public CombatlogEventSuffix SubeventSuffix { get; private set; } = CombatlogEventSuffix.UNDEFINED;

        public string SubEvent => Enum.GetName(SubeventPrefix) + Enum.GetName(SubeventSuffix); 

        //these 8 parameters are guaranteed to be included in combatlog events.
        public string SourceGUID { get; init; } = "";
        public string SourceName { get; init; } = "";
        public UnitFlag SourceFlags { get; init; } = 0x0;
        public RaidFlag SourceRaidFlags { get; init; } = 0x0;
        public string TargetGUID { get; init; } = "";
        public string TargetName { get; init; } = "";
        public UnitFlag TargetFlags { get; init; } = 0x0;
        public RaidFlag TargetRaidFlags { get; init; } = 0x0;

        public readonly EventType eventType = EventType.UNDEFINED;

        public bool IsSourcePet => SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PET);

        protected CombatlogEvent(EventType eventType, CombatlogEventPrefix prefix, CombatlogEventSuffix suffix)
        {
            this.eventType = eventType;
            this.SubeventPrefix = prefix;
            this.SubeventSuffix = suffix;
        }

        /// <summary>
        /// the GUID of the unit that is the source of the event. (advanced param)
        /// </summary>
        public string GetInfoGUID()
        {
            if (advancedParams.Length > 0)
                return (string)advancedParams[0];
            return string.Empty;
        }

        /// <summary>
        /// the GUID of the unit that is the owner of the infoGUID. (advanced param)
        /// </summary>
        public string GetOwnerGUID()
        {
            if (advancedParams.Length > 0)
                return (string)advancedParams[1];
            return string.Empty;
        }

        public static CombatlogEvent? Create(string combatlogEntry)
        {
            //1. timestamp parsed.
            var timestamp = StringTimestampToDateTime(combatlogEntry[..18]);
            //2. start after the two empty spaces behind the timestamp.
            int index = 20;
            string subEvent = NextSubstring(combatlogEntry, ref index);
            //make sure that its a valid combat event.
            if (!TryParsePrefixAffixSubeventF(subEvent, out var prefix, out var suffix))
                return null;
            if (prefix is CombatlogEventPrefix.PARTY || 
                prefix is CombatlogEventPrefix.UNIT || 
                prefix is CombatlogEventPrefix.ENVIRONMENTAL)
                return null;
            //basic data
            string sourceGUID = NextSubstring(combatlogEntry, ref index);
            string sourceName = NextSubstring(combatlogEntry, ref index);
            var sourceFlags = NextFlags(combatlogEntry, ref index);
            var sourceRFlags = NextRaidFlags(combatlogEntry, ref index);
            string destGUID = NextSubstring(combatlogEntry, ref index);
            string destName = NextSubstring(combatlogEntry, ref index);
            var destFlags = NextFlags(combatlogEntry, ref index);
            var destRFlags = NextRaidFlags(combatlogEntry, ref index);
            //divide into basic 
            switch (suffix)
            {
                case CombatlogEventSuffix._DAMAGE:
                    return new DamageEvent(prefix, combatlogEntry, index)
                    {
                        Timestamp = timestamp,
                        SourceGUID = sourceGUID,
                        SourceName = sourceName,
                        SourceFlags = sourceFlags,
                        SourceRaidFlags = sourceRFlags,
                        TargetGUID = destGUID,
                        TargetName = destName,
                        TargetFlags = destFlags,
                        TargetRaidFlags = destRFlags
                    };
                //case CombatlogEventSuffix._DURABILITY_DAMAGE:
                //    break;
                //case CombatlogEventSuffix._DAMAGE_LANDED:
                //    break;
                //case CombatlogEventSuffix._MISSED:
                //    break;
                //case CombatlogEventSuffix._HEAL:
                //    break;
                //case CombatlogEventSuffix._HEAL_ABSORBED:
                //    break;
                //case CombatlogEventSuffix._ABSORBED:
                //    break;
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
                //case CombatlogEventSuffix._CAST_SUCCESS:
                //    break;
                //case CombatlogEventSuffix._CAST_FAILED:
                //    break;
                //case CombatlogEventSuffix._INSTAKILL:
                //    break;
                //case CombatlogEventSuffix._CREATE:
                //    break;
                //case CombatlogEventSuffix._DURABILITY_DAMAGE_ALL:
                //    break;
                //case CombatlogEventSuffix._SUMMON:
                //    break;
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
                case CombatlogEventSuffix.UNDEFINED:
                default:
                    return null;
            }
        }
    }

    public class AdvancedParams
    {
        public readonly string infoGUID;
        public readonly string ownerGUID;
        public readonly int currentHP;
        public int maxHP;
        public int attackPower;
        public int spellPower;
        public int armor;
        public int absorb;
        public PowerType powerType;
        public int currentPower;
        public int maxPower;
        public int powerCost;
        public float positionX;
        public float positionY;
        public int uiMapID;
        public float facing;
        public int level;

        private AdvancedParams(
            string infoGUID, 
            string ownerGUID,
            int currentHP,
            int maxHP,
            int attackPower,
            int spellPower,
            int armor,
            int absorb,
            PowerType powerType,
            int currentPower,
            int maxPower,
            int powerCost,
            float positionX,
            float positionY,
            int uiMapID,
            float facing,
            int level)
        {
            this.infoGUID = infoGUID;
            this.ownerGUID = ownerGUID;
            this.currentHP = currentHP;
            this.maxHP = maxHP;
            this.attackPower = attackPower;
            this.spellPower = spellPower;
            this.armor = armor;
            this.absorb = absorb;
            this.powerType = powerType;
            this.currentPower = currentPower;
            this.maxPower = maxPower;
            this.powerCost = powerCost;
            this.positionX = positionX;
            this.positionY = positionY;
            this.uiMapID = uiMapID;
            this.facing = facing;
            this.level = level;
        }

        public static AdvancedParams Get(string data, ref int dataIndex)
        {
            int initialIndex = dataIndex;
            //god this is ugly wtf
            try
            {
                return new AdvancedParams(
                    infoGUID: NextSubstring(data, ref dataIndex),
                    ownerGUID: NextSubstring(data, ref dataIndex),
                    currentHP: int.Parse(NextSubstring(data, ref dataIndex)),
                    maxHP: int.Parse(NextSubstring(data, ref dataIndex)),
                    attackPower: int.Parse(NextSubstring(data, ref dataIndex)),
                    spellPower: int.Parse(NextSubstring(data, ref dataIndex)),
                    armor: int.Parse(NextSubstring(data, ref dataIndex)),
                    absorb: int.Parse(NextSubstring(data, ref dataIndex)),
                    powerType: (PowerType)int.Parse(NextSubstring(data, ref dataIndex)),
                    currentPower: int.Parse(NextSubstring(data, ref dataIndex)),
                    maxPower: int.Parse(NextSubstring(data, ref dataIndex)),
                    powerCost: int.Parse(NextSubstring(data, ref dataIndex)),
                    positionX: float.Parse(NextSubstring(data, ref dataIndex)),
                    positionY: float.Parse(NextSubstring(data, ref dataIndex)),
                    uiMapID: int.Parse(NextSubstring(data, ref dataIndex)),
                    facing: float.Parse(NextSubstring(data, ref dataIndex)),
                    level: int.Parse(NextSubstring(data, ref dataIndex))
                    );
            }
            catch (Exception fe)
            {
                Console.WriteLine(fe.Message);
                Console.WriteLine($"index: {initialIndex}, data=" + data);
                string sub = data[initialIndex..];
                Console.WriteLine($"data at index= {sub}");
            }
            return new AdvancedParams("", "", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}

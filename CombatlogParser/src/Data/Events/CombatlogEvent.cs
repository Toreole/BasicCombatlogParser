namespace CombatlogParser.Data
{
    /// <summary>
    /// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
    /// This is a somewhat polymorphic class.
    /// </summary>
    public class CombatlogEvent : LogEntryBase
    {
        //this dictionary includes both suffix and prefix data.
        private readonly Dictionary<EventData, object> data = new();
        private readonly string[] rawSuffixData;
        private readonly string[] rawPrefixData;

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

        public bool IsSourcePet => SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PET);

        public object PrefixParam0 => GetPrefixString(0);
        public object PrefixParam1 => GetPrefixString(1);
        public object PrefixParam2 => GetPrefixString(2);

        //These are all used for displaying them in plain text.
        public object SuffixParam0 => GetSuffixString(0);
        public object SuffixParam1 => GetSuffixString(1);
        public object SuffixParam2 => GetSuffixString(2);
        public object SuffixParam3 => GetSuffixString(3);
        public object SuffixParam4 => GetSuffixString(4);
        public object SuffixParam5 => GetSuffixString(5);
        public object SuffixParam6 => GetSuffixString(6);
        public object SuffixParam7 => GetSuffixString(7);
        public object SuffixParam8 => GetSuffixString(8);
        public object SuffixParam9 => GetSuffixString(9);
        public object SuffixParam10 => GetSuffixString(10);


        public CombatlogEvent(
            CombatlogEventPrefix prefix, string[] prefixData,
            CombatlogEventSuffix suffix, string[] suffixData,
            string[] advancedData
            )
        {
            if (prefixData.Length < ParsingUtil.GetPrefixParamAmount(prefix) || suffixData.Length < ParsingUtil.GetSuffixParamAmount(suffix))
                throw new ArgumentException("too few prefix/suffix data entries");
            this.SubeventSuffix = suffix;
            this.SubeventPrefix = prefix;

            InitPrefixParamsFromStrings(prefixData);
            rawPrefixData = prefixData;
            InitSuffixParamsFromStrings(suffixData);
            rawSuffixData = suffixData;

            if(advancedData.Length == 17)
            { 
                this.advancedParams = new object[advancedData.Length];
                advancedParams[(int)AdvancedParamID.InfoGUID]     = advancedData[0];
                advancedParams[(int)AdvancedParamID.OwnerGUID]    = advancedData[1];
                advancedParams[(int)AdvancedParamID.CurrentHP]    = long.Parse(advancedData[2]);
                advancedParams[(int)AdvancedParamID.MaxHP]        = long.Parse(advancedData[3]);
                advancedParams[(int)AdvancedParamID.AttackPower]  = long.Parse(advancedData[4]);
                advancedParams[(int)AdvancedParamID.SpellPower]   = long.Parse(advancedData[5]);
                advancedParams[(int)AdvancedParamID.Armor]        = long.Parse(advancedData[6]);
                advancedParams[(int)AdvancedParamID.Absorb]       = long.Parse(advancedData[7]);
                advancedParams[(int)AdvancedParamID.PowerType]    = ParsingUtil.AllPowerTypesIn(advancedData[8]);
                advancedParams[(int)AdvancedParamID.CurrentPower] = ParsingUtil.AllIntsIn(advancedData[9]);
                advancedParams[(int)AdvancedParamID.MaxPower]     = ParsingUtil.AllIntsIn(advancedData[10]);
                advancedParams[(int)AdvancedParamID.PowerCost]    = ParsingUtil.AllIntsIn(advancedData[11]);
                advancedParams[(int)AdvancedParamID.PositionX]    = double.Parse(advancedData[12]);
                advancedParams[(int)AdvancedParamID.PositionY]    = double.Parse(advancedData[13]);
                advancedParams[(int)AdvancedParamID.UiMapID]      = int.Parse(advancedData[14]);
                advancedParams[(int)AdvancedParamID.Facing]       = float.Parse(advancedData[15]);
                advancedParams[(int)AdvancedParamID.Level]        = int.Parse(advancedData[16]);
            }
        }

        /// <summary>
        /// Tries getting a T value for the EventData entry.
        /// </summary>
        /// <typeparam name="T">a value type, consider: long, int, SpellSchool, float, double, PowerType </typeparam>
        /// <param name="entry"></param>
        /// <param name="t"></param>
        /// <returns>false if the entry does not exist, or is of another type.</returns>
        public bool TryGet<T>(EventData entry, out T t) where T : struct
        {
            if (data.TryGetValue(entry, out object? value) && value is T x)
            {
                t = x;
                return true;
            }
            t = default;
            return false;
        }

        /// <summary>
        /// Get the value of a data entry, or the default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entry"></param>
        /// <returns></returns>
        public T Get<T>(EventData entry) where T : struct
        {
            if (data.TryGetValue(entry, out object? value) && value is T x)
                return x;
            return default;
        }

        /// <summary>
        /// See <see cref="Get{T}(EventData)"/>, just for strings instead of values.
        /// </summary>
        public string GetString(EventData entry)
        {
            if (data.TryGetValue(entry, out object? value) && value is string x)
                return x;
            return string.Empty;
        }

        /// <summary>
        /// Generic way to get an advanced param by its ID for readability. <br/>
        /// Returns default if the wrong type is provided or the event doesnt have advanced params.
        /// </summary>
        /// <returns></returns>
        public T GetAdvancedParam<T>(AdvancedParamID id) where T : struct
        {
            if (advancedParams.Length > 0 && advancedParams[(int)id] is T x)
                return x;
            return default;
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


        //Gets the raw data of the suffix at the given index.
        private string GetSuffixString(int index)
        {
            if (index >= rawSuffixData.Length)
                return string.Empty;
            return rawSuffixData[index];
        }

        //Gets the raw data of the prefix at the given index.
        private string GetPrefixString(int index)
        {
            if (index >= rawPrefixData.Length)
                return string.Empty;
            return rawPrefixData[index];
        }

        private void InitPrefixParamsFromStrings(string[] prefixData)
        {
            switch (SubeventPrefix)
            {
                case CombatlogEventPrefix.SPELL_PERIODIC:
                case CombatlogEventPrefix.SPELL_BUILDING:
                case CombatlogEventPrefix.RANGE:
                case CombatlogEventPrefix.DAMAGE:
                case CombatlogEventPrefix.SPELL:
                    data.Add(EventData.SpellID, int.Parse(prefixData[0]));
                    data.Add(EventData.SpellName, prefixData[1]);
                    data.Add(EventData.SpellSchool, int.Parse(prefixData[2]));
                    break;
                case CombatlogEventPrefix.ENVIRONMENTAL:
                    data.Add(EventData.EnvironmentalType, prefixData[0]);
                    break;
                case CombatlogEventPrefix.ENCHANT:
                    data.Add(EventData.SpellName, prefixData[0]);
                    data.Add(EventData.ItemID, int.Parse(prefixData[1]));
                    data.Add(EventData.ItemName, prefixData[2]);
                    break;
                case CombatlogEventPrefix.UNIT:
                    //UNIT technically has recapID and unconsciousOnDeath, but i dont care.
                case CombatlogEventPrefix.SWING:
                case CombatlogEventPrefix.UNDEFINED:
                    break;
            }
        }

        private void InitSuffixParamsFromStrings(string[] suffixData)
        {
            switch(SubeventSuffix)
            {
                case CombatlogEventSuffix._SPLIT:
                case CombatlogEventSuffix._SHIELD:
                case CombatlogEventSuffix._DAMAGE:
                    data.Add(EventData.Amount, long.Parse(suffixData[0]));
                    data.Add(EventData.BaseAmount, long.Parse(suffixData[1]));
                    data.Add(EventData.Overkill, long.Parse(suffixData[2]));
                    data.Add(EventData.School, (SpellSchool)ParsingUtil.HexStringToUint(suffixData[3]));
                    data.Add(EventData.Resisted, long.Parse(suffixData [4]));
                    data.Add(EventData.Blocked, long.Parse(suffixData[5]));
                    data.Add(EventData.Absorbed, long.Parse(suffixData[6]));
                    data.Add(EventData.Critical, suffixData[7] == "1");
                    //these arent all that important, so just discard all of them when theyre not all included.
                    if (suffixData.Length < 11)
                        break;
                    data.Add(EventData.Glancing, suffixData[8] == "1");
                    data.Add(EventData.Crushing, suffixData[9] == "1");
                    data.Add(EventData.IsOffHand, suffixData[10] == "1");
                    break;
                case CombatlogEventSuffix._MISSED:
                    data.Add(EventData.MissType, Enum.Parse<MissType>(suffixData[0]));
                    data.Add(EventData.IsOffHand, suffixData[1] == "1");
                    data.Add(EventData.AmountMissed, long.Parse(suffixData[2]));
                    data.Add(EventData.Critical, suffixData[3] = "1");
                    break;
                case CombatlogEventSuffix._HEAL:
                    data.Add(EventData.Amount, long.Parse(suffixData[0]));
                    data.Add(EventData.BaseAmount, long.Parse(suffixData[1]));
                    data.Add(EventData.Overhealing, long.Parse(suffixData[2]));
                    data.Add(EventData.Absorbed, long.Parse(suffixData[3]));
                    data.Add(EventData.Critical, suffixData[4] == "1");
                    break;
                case CombatlogEventSuffix._HEAL_ABSORBED:
                    data.Add(EventData.ExtraGUID, suffixData[0]);
                    data.Add(EventData.ExtraName, suffixData[1]);
                    data.Add(EventData.ExtraFlags, (UnitFlag)uint.Parse(suffixData[2]));
                    data.Add(EventData.ExtraRaidFlags, (UnitFlag)uint.Parse(suffixData[3]));
                    data.Add(EventData.ExtraSpellID, int.Parse(suffixData[4]));
                    data.Add(EventData.ExtraSpellName, suffixData[5]);
                    data.Add(EventData.ExtraSchool, (SpellSchool)ParsingUtil.HexStringToUint(suffixData[6]));
                    data.Add(EventData.Absorbed, long.Parse(suffixData[7]));
                    if (suffixData.Length > 8)
                        data.Add(EventData.TotalAmount, long.Parse(suffixData[8]));
                    break;
                case CombatlogEventSuffix._ENERGIZE:
                case CombatlogEventSuffix._DRAIN:
                    data.Add(EventData.Amount, ParsingUtil.AllIntsIn(suffixData[0]));
                    data.Add(EventData.OverEnergize, ParsingUtil.AllIntsIn(suffixData[1]));
                    data.Add(EventData.PowerType, ParsingUtil.AllPowerTypesIn(suffixData[2]));
                    if (suffixData.Length > 3)
                        data.Add(EventData.MaxPower, ParsingUtil.AllIntsIn(suffixData[3]));
                    break;
                case CombatlogEventSuffix._LEECH:
                    data.Add(EventData.Amount, ParsingUtil.AllIntsIn(suffixData[0]));
                    data.Add(EventData.OverEnergize, ParsingUtil.AllIntsIn(suffixData[1]));
                    data.Add(EventData.PowerType, ParsingUtil.AllPowerTypesIn(suffixData[2]));
                    break;
                case CombatlogEventSuffix._INTERRUPT:
                case CombatlogEventSuffix._DISPEL_FAILED:
                    data.Add(EventData.ExtraSpellID, int.Parse(suffixData[0]));
                    data.Add(EventData.SpellName, suffixData[1]);
                    data.Add(EventData.ExtraSchool, (SpellSchool)ParsingUtil.HexStringToUint(suffixData[2]));
                    break;
                case CombatlogEventSuffix._DISPEL:
                case CombatlogEventSuffix._STOLEN:
                    data.Add(EventData.ExtraSpellID, int.Parse(suffixData[0]));
                    data.Add(EventData.SpellName, suffixData[1]);
                    data.Add(EventData.ExtraSchool, (SpellSchool)ParsingUtil.HexStringToUint(suffixData[2]));
                    data.Add(EventData.AuraType, Enum.Parse<AuraType>(suffixData[3]));
                    break;
                case CombatlogEventSuffix._EXTRA_ATTACKS:
                    data.Add(EventData.Amount, int.Parse(suffixData[0]));
                    break;
                case CombatlogEventSuffix._AURA_APPLIED_DOSE:
                    data.Add(EventData.AuraType, Enum.Parse<AuraType>(suffixData[0]));
                    data.Add(EventData.Amount, int.Parse(suffixData[1]));
                    break;
                case CombatlogEventSuffix._AURA_REMOVED_DOSE:
                    data.Add(EventData.AuraType, Enum.Parse<AuraType>(suffixData[0]));
                    data.Add(EventData.Amount, int.Parse(suffixData[1]));
                    break;
                case CombatlogEventSuffix._AURA_APPLIED:
                case CombatlogEventSuffix._AURA_REMOVED:
                case CombatlogEventSuffix._AURA_REFRESH:
                case CombatlogEventSuffix._AURA_BROKEN:
                    data.Add(EventData.AuraType, Enum.Parse<AuraType>(suffixData[0]));
                    break;
                case CombatlogEventSuffix._AURA_BROKEN_SPELL:
                    data.Add(EventData.ExtraSpellID, int.Parse(suffixData[0]));
                    data.Add(EventData.ExtraSpellName, suffixData[1]);
                    data.Add(EventData.ExtraSchool, (SpellSchool)ParsingUtil.HexStringToUint(suffixData[2]));
                    data.Add(EventData.AuraType, Enum.Parse<AuraType>(suffixData[3]));
                    break;
                case CombatlogEventSuffix._CAST_FAILED:
                    data.Add(EventData.FailedType, suffixData[0]);
                    break;
                case CombatlogEventSuffix._INSTAKILL:
                    data.Add(EventData.UnconsciousOnDeath, suffixData[0]);
                    break;
                case CombatlogEventSuffix._CAST_START:
                case CombatlogEventSuffix._CAST_SUCCESS:
                case CombatlogEventSuffix._DURABILITY_DAMAGE:
                case CombatlogEventSuffix._DURABILITY_DAMAGE_ALL:
                case CombatlogEventSuffix._CREATE:
                case CombatlogEventSuffix._SUMMON:
                case CombatlogEventSuffix._RESURRECT:
                case CombatlogEventSuffix._DIED:
                case CombatlogEventSuffix._DESTROYED:
                case CombatlogEventSuffix._DISSIPATES:
                case CombatlogEventSuffix._REMOVED:
                case CombatlogEventSuffix._APPLIED:
                case CombatlogEventSuffix._KILL:
                default:
                    break;
            }
        }

        //I usually try to keep enums in seperate files, but these two are specifically for the CombatlogEvent stuff.
        public enum EventData
        {
            //Prefix data

            /// <summary> int </summary>
            SpellID,
            /// <summary> string </summary>
            SpellName,
            /// <summary> SpellSchool </summary>
            SpellSchool,
            /// <summary> string (WIP) </summary>
            EnvironmentalType,
            //Suffix data
            /// <summary> long </summary>
            Amount,
            /// <summary> long </summary>
            BaseAmount,
            /// <summary> long </summary>
            Overkill,
            /// <summary> SpellSchool </summary>
            School,
            /// <summary> long </summary>
            Resisted,
            /// <summary> long </summary>
            Blocked,
            /// <summary> long </summary>
            Absorbed,
            /// <summary> bool </summary>
            Critical,
            /// <summary> bool </summary>
            Glancing,
            /// <summary> bool </summary>
            Crushing,
            /// <summary> bool </summary>
            IsOffHand,
            /// <summary> MissType </summary>
            MissType,
            /// <summary> long </summary>
            AmountMissed,
            /// <summary> long </summary>
            Overhealing,
            /// <summary> string </summary>
            ExtraGUID,
            /// <summary> string </summary>
            ExtraName,
            /// <summary> UnitFlag </summary>
            ExtraFlags,
            /// <summary> RaidFlag </summary>
            ExtraRaidFlags,
            /// <summary> int </summary>
            ExtraSpellID,
            /// <summary> string </summary>
            ExtraSpellName,
            /// <summary> SpellSchool </summary>
            ExtraSchool,
            /// <summary> long </summary>
            TotalAmount,
            /// <summary> int[] </summary>
            OverEnergize,
            /// <summary> PowerType[] </summary>
            PowerType,
            /// <summary> int[] </summary>
            MaxPower,
            /// <summary> AuraType </summary>
            AuraType,
            /// <summary> string (WIP) </summary>
            FailedType,
            /// <summary> string (WIP) </summary>
            UnconsciousOnDeath,
            /// <summary> int </summary>
            ItemID,
            /// <summary> string </summary>
            ItemName,
            /// <summary> int, THIS CAN BE DISREGARDED </summary>
            RecapID,

        }
        public enum AdvancedParamID 
        {
            /// <summary> string </summary>
            InfoGUID = 0,
            /// <summary> string </summary>
            OwnerGUID = 1,
            /// <summary> long </summary>
            CurrentHP = 2,
            /// <summary> long </summary>
            MaxHP = 3,
            /// <summary> long </summary>
            AttackPower = 4,
            /// <summary> long </summary>
            SpellPower = 5,
            /// <summary> long </summary>
            Armor = 6,
            /// <summary> long </summary>
            Absorb = 7,
            /// <summary> PowerType[] </summary>
            PowerType = 8,
            /// <summary> int[] </summary>
            CurrentPower = 9,
            /// <summary> int[] </summary>
            MaxPower = 10,
            /// <summary> int[] </summary>
            PowerCost = 11,
            /// <summary> double </summary>
            PositionX = 12,
            /// <summary> double </summary>
            PositionY = 13,
            /// <summary> int </summary>
            UiMapID = 14,
            /// <summary> float </summary>
            Facing = 15,
            /// <summary> int </summary>
            Level = 16
        }
    }
}

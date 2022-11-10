namespace CombatlogParser.Data
{
    /// <summary>
    /// Represents a single event in a combatlog. There are many kinds of events that go into the specifics.
    /// This is a somewhat polymorphic class.
    /// </summary>
    public class CombatlogEvent : LogEntryBase
    {
        public CombatlogEventPrefix SubeventPrefix { get; set; } = CombatlogEventPrefix.UNDEFINED;
        public CombatlogEventSuffix SubeventSuffix { get; set; } = CombatlogEventSuffix.UNDEFINED;

        public string SubEvent => Enum.GetName(SubeventPrefix) + Enum.GetName(SubeventSuffix); 

        //these 8 parameters are guaranteed to be included in combatlog events.
        public string SourceGUID { get; set; } = "";
        public string SourceName { get; set; } = "";
        public UnitFlag SourceFlags { get; set; } = 0x0;
        public RaidFlag SourceRaidFlags { get; set; } = 0x0;
        public string TargetGUID { get; set; } = "";
        public string TargetName { get; set; } = "";
        public UnitFlag TargetFlags { get; set; } = 0x0;
        public RaidFlag TargetRaidFlags { get; set; } = 0x0;

        public bool IsSourcePet => SourceFlags.HasFlagf(UnitFlag.COMBATLOG_OBJECT_TYPE_PET);

        /// <summary>
        /// The parameters specific to the Subevents Prefix
        /// </summary>
        public object[] PrefixParams { get; set; } = Array.Empty<object>();

        public object PrefixParam0 => PrefixParams.Length >= 1 ? PrefixParams[0] : "";
        public object PrefixParam1 => PrefixParams.Length >= 2 ? PrefixParams[1] : "";
        public object PrefixParam2 => PrefixParams.Length >= 3 ? PrefixParams[2] : "";

        /// <summary>
        /// The parameters specific to the Subevents Suffix
        /// </summary>
        public object[] SuffixParams { get; set; } = Array.Empty<object>();

        //These are all used for displaying them in plain text.
        public object SuffixParam0 => SuffixParams.Length >= 1 ? SuffixParams[0] : "";
        public object SuffixParam1 => SuffixParams.Length >= 2 ? SuffixParams[1] : "";
        public object SuffixParam2 => SuffixParams.Length >= 3 ? SuffixParams[2] : "";
        public object SuffixParam3 => SuffixParams.Length >= 4 ? SuffixParams[3] : "";
        public object SuffixParam4 => SuffixParams.Length >= 5 ? SuffixParams[4] : "";
        public object SuffixParam5 => SuffixParams.Length >= 6 ? SuffixParams[5] : "";
        public object SuffixParam6 => SuffixParams.Length >= 7 ? SuffixParams[6] : "";
        public object SuffixParam7 => SuffixParams.Length >= 8 ? SuffixParams[7] : "";
        public object SuffixParam8 => SuffixParams.Length >= 9 ? SuffixParams[8] : "";
        public object SuffixParam9 => SuffixParams.Length >= 10 ? SuffixParams[9] : "";
        public object SuffixParam10 => SuffixParams.Length >= 11 ? SuffixParams[10] : "";

        private readonly Dictionary<EventData, object> data = new();

        /// <summary>
        /// The advanced combatlog parameters.
        /// </summary>
        public object[] AdvancedParams { get; private set; } = Array.Empty<string>();

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
            InitSuffixParamsFromStrings(suffixData);

            if(advancedData.Length == 17)
            { 
                this.AdvancedParams = new object[advancedData.Length];
                AdvancedParams[(int)AdvancedParamID.InfoGUID]     = advancedData[0];
                AdvancedParams[(int)AdvancedParamID.OwnerGUID]    = advancedData[1];
                AdvancedParams[(int)AdvancedParamID.CurrentHP]    = long.Parse(advancedData[2]);
                AdvancedParams[(int)AdvancedParamID.MaxHP]        = long.Parse(advancedData[3]);
                AdvancedParams[(int)AdvancedParamID.AttackPower]  = long.Parse(advancedData[4]);
                AdvancedParams[(int)AdvancedParamID.SpellPower]   = long.Parse(advancedData[5]);
                AdvancedParams[(int)AdvancedParamID.Armor]        = long.Parse(advancedData[6]);
                AdvancedParams[(int)AdvancedParamID.Absorb]       = long.Parse(advancedData[7]);
                AdvancedParams[(int)AdvancedParamID.PowerType]    = (PowerType)int.Parse(advancedData[8]);
                AdvancedParams[(int)AdvancedParamID.CurrentPower] = int.Parse(advancedData[9]);
                AdvancedParams[(int)AdvancedParamID.MaxPower]     = int.Parse(advancedData[10]);
                AdvancedParams[(int)AdvancedParamID.PowerCost]    = int.Parse(advancedData[11]);
                AdvancedParams[(int)AdvancedParamID.PositionX]    = double.Parse(advancedData[12]);
                AdvancedParams[(int)AdvancedParamID.PositionY]    = double.Parse(advancedData[13]);
                AdvancedParams[(int)AdvancedParamID.UiMapID]      = int.Parse(advancedData[14]);
                AdvancedParams[(int)AdvancedParamID.Facing]       = float.Parse(advancedData[15]);
                AdvancedParams[(int)AdvancedParamID.Level]        = int.Parse(advancedData[16]);
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
            return "";
        }

        private void InitPrefixParamsFromStrings(string[] prefixData)
        {
            switch (SubeventPrefix)
            {
                case CombatlogEventPrefix.SPELL_PERIODIC:
                case CombatlogEventPrefix.SPELL_BUILDING:
                case CombatlogEventPrefix.RANGE:
                case CombatlogEventPrefix.SPELL:
                    data.Add(EventData.SpellID, int.Parse(prefixData[0]));
                    data.Add(EventData.SpellName, prefixData[1]);
                    data.Add(EventData.SpellSchool, int.Parse(prefixData[2]));
                    break;
                case CombatlogEventPrefix.ENVIRONMENTAL:
                    data.Add(EventData.EnvironmentalType, prefixData[0]);
                    break;
                case CombatlogEventPrefix.SWING:
                case CombatlogEventPrefix.UNDEFINED:
                    break;
            }
        }

        private void InitSuffixParamsFromStrings(string[] suffixData)
        {
            switch(SubeventSuffix)
            {
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
                default:
                    break;
            }
        }

        //I usually try to keep enums in seperate files, but these two are specifically for the CombatlogEvent stuff.
        public enum EventData
        {
            //Prefix data
            SpellID,
            SpellName,
            SpellSchool,
            EnvironmentalType,
            //Suffix data
            Amount,
            BaseAmount,
            Overkill,
            School,
            Resisted,
            Blocked,
            Absorbed,
            Critical,
            Glancing,
            Crushing,
            IsOffHand,
            MissType,
            AmountMissed,
            Overhealing,
            ExtraGUID,
            ExtraName,
            ExtraFlags,
            ExtraRaidFlags,
            ExtraSpellID,
            ExtraSpellName,
            ExtraSchool,
            TotalAmount,
            OverEnergize,
            PowerType,
            MaxPower,
            AuraType,
            FailedType,
            UnconsciousOnDeath
        }
        public enum AdvancedParamID 
        {
            InfoGUID = 0,
            OwnerGUID = 1,
            CurrentHP = 2,
            MaxHP = 3,
            AttackPower = 4,
            SpellPower = 5,
            Armor = 6,
            Absorb = 7,
            PowerType = 8,
            CurrentPower = 9,
            MaxPower = 10,
            PowerCost = 11,
            PositionX = 12,
            PositionY = 13,
            UiMapID = 14,
            Facing = 15,
            Level = 16
        }
    }
}

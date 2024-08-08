﻿using CombatlogParser.Data.Events;
using System.Globalization;

namespace CombatlogParser;

public static class ParsingUtil
{
    private static readonly CultureInfo formatInfoProvider = CultureInfo.GetCultureInfo("en-US");
    private static readonly string[] acceptedTimestampFormats =
    {
        "MM/dd HH:mm:ss.fff",
        "M/dd HH:mm:ss.fff",
        "M/d HH:mm:ss.fff",
        "MM/d HH:mm:ss.fff"
    };

    private static readonly Dictionary<string, Subevent> subeventDictionary;

    private static readonly List<PowerType[]> knownPowerTypes = new();

    private static readonly CombatlogEventPrefix[] prefixes;
    private static readonly string[] prefixNames;

    private static readonly CombatlogEventSuffix[] suffixes;
    private static readonly string[] suffixNames;

    private static readonly CombatlogMiscEvents[] miscEvents;
    private static readonly string[] miscNames;

    public static readonly string timestamp_end_seperator = "  ";

    public static readonly IFormatProvider FloatNumberFormat;

    static ParsingUtil()
    {
        prefixes = Enum.GetValues<CombatlogEventPrefix>();
        prefixNames = Enum.GetNames<CombatlogEventPrefix>();

        suffixes = Enum.GetValues<CombatlogEventSuffix>();
        suffixNames = Enum.GetNames<CombatlogEventSuffix>();

        miscEvents = Enum.GetValues<CombatlogMiscEvents>();
        miscNames = Enum.GetNames<CombatlogMiscEvents>();

        //needs to included extra because otherwise it will default to german on my machine.
        FloatNumberFormat = new CultureInfo("en-US").NumberFormat;

        subeventDictionary = new();
        //this creates every theoretically possible combination.
        //which is kinda overkill, because not every combination actually exists as an event
        //but its easier than actually writing it all out individually.
        for (int i = 0; i < prefixes.Length; i++)
        {
            if (prefixes[i] == CombatlogEventPrefix.UNDEFINED)
                continue;
            for (int j = 0; j < suffixes.Length; j++)
            {
				if (suffixes[i] == CombatlogEventSuffix.UNDEFINED)
					continue;
				subeventDictionary.Add(prefixNames[i] + suffixNames[j], new(prefixes[i], suffixes[j]));
            }
        }
    }

    /// <summary>
    /// Converts the Timestamps that come with the combatlog file into a DateTime object.
    /// </summary>
    /// <param name="timestamp">The full timestamp without trailing whitespace.</param>
    /// <returns></returns>
    public static DateTime StringTimestampToDateTime(string timestamp)
    {
        return DateTime.ParseExact(timestamp, acceptedTimestampFormats, formatInfoProvider);
    }

    /// <summary>
    /// Converts a hex string in the "0x[0-F]+" format into a 32 unsigned integer.
    /// </summary>
    /// <returns></returns>
    public static uint HexStringToUInt(string hex)
    {
        return Convert.ToUInt32(hex, 16);
    }

    /// <summary>
    /// Quickly "parses" a subevent and divides it into its prefix and suffix, by looking it up in a dictionary.
    /// </summary>
    /// <param name="subevent">The full subevent string. e.g. SPELL_DAMAGE</param>
    /// <returns>false if the prefix and suffix are UNDEFINED</returns>
    public static bool TryParsePrefixSuffixSubeventF(string subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix)
    {
        if (subeventDictionary.TryGetValue(subevent, out var val))
        {
            prefix = val.prefix;
            suffix = val.suffix;
            return true;
        }
        prefix = CombatlogEventPrefix.UNDEFINED;
        suffix = CombatlogEventSuffix.UNDEFINED;
        return false;
    }

    /// <summary>
    /// A simple, culture-insensitive, check whether a string starts with a given substring.
    /// </summary>
    /// <returns>false if the strings differ, or other is longer than self. true if other is contained within self at [0].</returns>
    public static bool StartsWithF(this string self, string other)
    {
        if (other.Length > self.Length)
            return false;
        for (int i = 0; i < other.Length; i++)
            if (self[i] != other[i])
                return false;
        return true;
    }

    /// <summary>
    /// A simple, culture-insensitive, check whether a string ends with a given substring.
    /// </summary>
    /// <returns>false if other isnt contained in self, or is longer than it. true if self contains other at the end.</returns>
    public static bool EndsWithF(this string self, string other)
    {
        if (other.Length > self.Length)
            return false;
        for (int i = 1; i <= other.Length; i++)
            if (self[^i] != other[^i])
                return false;
        return true;
    }

    /// <summary>
    /// Gets the amount of parameters that are associated with the given prefix.
    /// </summary>
    public static int GetPrefixParamAmount(CombatlogEventPrefix prefix)
    {
        return prefix switch
        {
            CombatlogEventPrefix.SWING => 0,
            CombatlogEventPrefix.RANGE => 3,
            CombatlogEventPrefix.SPELL => 3, //(suffix == CombatlogEventSuffix._ABSORBED) ? 0 :
            CombatlogEventPrefix.SPELL_PERIODIC => 3,
            CombatlogEventPrefix.SPELL_BUILDING => 3,
            CombatlogEventPrefix.ENVIRONMENTAL => 1,
            _ => 0 //default
        };
    }

    /// <summary>
    /// Gets the amount of parameters that are associated with the given suffix.
    /// </summary>
    public static int GetSuffixParamAmount(CombatlogEventSuffix suffix)
    {
        return suffix switch
        {
            CombatlogEventSuffix._DAMAGE => 10,
            CombatlogEventSuffix._DAMAGE_LANDED => 10,
            CombatlogEventSuffix._MISSED => 5, //actually 5, since the logfiles also include baseDamage after damage
            CombatlogEventSuffix._HEAL => 5,
            CombatlogEventSuffix._HEAL_ABSORBED => 9,
            CombatlogEventSuffix._ABSORBED => 9,
            CombatlogEventSuffix._ENERGIZE => 4,
            CombatlogEventSuffix._DRAIN => 4,
            CombatlogEventSuffix._LEECH => 3,
            CombatlogEventSuffix._INTERRUPT => 3,
            CombatlogEventSuffix._DISPEL => 4,
            CombatlogEventSuffix._DISPEL_FAILED => 3,
            CombatlogEventSuffix._STOLEN => 4,
            CombatlogEventSuffix._EXTRA_ATTACKS => 1,
            CombatlogEventSuffix._AURA_APPLIED => 1,
            CombatlogEventSuffix._AURA_REMOVED => 1,
            CombatlogEventSuffix._AURA_APPLIED_DOSE => 2,
            CombatlogEventSuffix._AURA_REMOVED_DOSE => 2,
            CombatlogEventSuffix._AURA_REFRESH => 2,
            CombatlogEventSuffix._AURA_BROKEN => 3,
            CombatlogEventSuffix._AURA_BROKEN_SPELL => 4,
            CombatlogEventSuffix._CAST_START => 0,
            CombatlogEventSuffix._CAST_SUCCESS => 0,
            CombatlogEventSuffix._CAST_FAILED => 1,
            CombatlogEventSuffix._INSTAKILL => 1,
            CombatlogEventSuffix._DURABILITY_DAMAGE => 0,
            CombatlogEventSuffix._DURABILITY_DAMAGE_ALL => 0,
            CombatlogEventSuffix._CREATE => 0,
            CombatlogEventSuffix._SUMMON => 0,
            CombatlogEventSuffix._RESURRECT => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Parses "3|4" into { Energy, ComboPoints }. etc.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static PowerType[] AllPowerTypesIn(string str)
    {
        if (str.Contains('|'))
        {
            string[] vs = str.Split('|');
            PowerType[] powerTypes = new PowerType[vs.Length];
            for (int i = 0; i < vs.Length; i++)
                powerTypes[i] = (PowerType)int.Parse(vs[i], NumberStyles.Number);

            return PseudoIntern(powerTypes);
        }
        return PseudoIntern( new[]{ (PowerType)int.Parse(str, NumberStyles.Number) });
    }

    /// <summary>
    /// "interns" a PowerType[] array. frees up upwards of 3MB of memory given large enough encounters.
    /// actually that may be bullshit because they rarely ever take up more than 32b anyway, since its just 1. 
    /// but ay whatever, it feels nice.
    /// </summary>
    /// <param name="powerTypes"></param>
    /// <returns></returns>
    public static PowerType[] PseudoIntern(PowerType[] powerTypes)
    {
        //the LINQ SequenceEqual is acceptably slow here since arrays will rarely (possibly never) have more than 2 elements.
        var existing = knownPowerTypes.Find(x => x.SequenceEqual(powerTypes));
        if (existing is null)
        {
            knownPowerTypes.Add(powerTypes);
            return powerTypes;
        }
        return existing;
    }

    /// <summary>
    /// Gets an int array from a string where values are seperated with a '|' 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int[] AllIntsIn(string str)
    {
        if (string.IsNullOrEmpty(str)) return new int[] { 0 };
        if (str.Contains('|'))
        {
            string[] vs = str.Split('|');
            int[] ix = new int[vs.Length];
            for (int i = 0; i < vs.Length; i++)
                ix[i] = int.Parse(vs[i], NumberStyles.Number);
            return ix;
        }
        return new int[] { int.Parse(str, NumberStyles.Number) };
    }

    /// <summary>
    /// Acts as a sort of lookup table for which subevents (by suffix) contain advanced params.
    /// Lists only those that have been observed to have them, all others by default are false.
    /// </summary>
    /// <param name="suffix"></param>
    /// <returns></returns>
    public static bool SubeventContainsAdvancedParams(CombatlogEventSuffix suffix)
    {
        return suffix switch
        {
            CombatlogEventSuffix._DAMAGE => true,
            CombatlogEventSuffix._DAMAGE_LANDED => true,
            CombatlogEventSuffix._CAST_SUCCESS => true,
            CombatlogEventSuffix._HEAL => true,
            CombatlogEventSuffix._ENERGIZE => true,
            _ => false
        };
    }

    /// <summary>
    /// Marches ahead in the string, dividing it into several substrings on demand,
    /// very useful for seperating data for combatlog events. Extracts "Word" from "\"Word\""
    /// Always checks for linebreak character.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static string NextSubstring(string line, ref int startIndex, char divisor = ',')
    {
        string sub;
        bool insideQuotations = false;
        for (int i = startIndex; i < line.Length; i++)
        {
            char c = line[i];
            //check for quotation start/end
            //this allows names (which are always in quotations) to include commas (,)
            if (c == '"')
            {
                //quotations end. extract the string insde, move index past next divisor
                if (insideQuotations)
                {
                    sub = line[startIndex..i];
                    startIndex = i;
                    MovePastNextDivisor(line, ref startIndex, true, divisor);
                    return sub;
                }
                else //start quotations. adjust startIndex
                {
                    insideQuotations = true;
                    startIndex = i + 1;
                    continue;
                }
            }
            //check for divisor, linebreak. (only outside of quotations)
            if (!insideQuotations && c == divisor && i + 1 < line.Length || line[i] == '\n')
            {
                sub = line[startIndex..i];
                startIndex = i + 1;
                return sub;
            }
            //end of string
            if (i == line.Length - 1)
            {
                sub = line[startIndex..line.Length];
                startIndex = line.Length;
                return sub;
            }
        }
        sub = "";
        startIndex = line.Length;
        return sub;
    }

    /// <summary>
    /// Advances the index beyond the next divisor (divisorIndex+1)
    /// </summary>
    public static void MovePastNextDivisor(string line, ref int startIndex, bool inQuotations = false, char divisor = ',')
    {
        for (int i = startIndex; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                inQuotations = !inQuotations;
            }
            else if ((!inQuotations && c == divisor) && i + 1 < line.Length || line[i] == '\n')
            {
                startIndex = i + 1;
                return;
            }
        }
        startIndex = line.Length;
    }

    public static bool HasFlagf(this UnitFlag flags, UnitFlag comp)
    {
        return (flags & comp) == comp;
    }
    public static bool HasFlagf(this RaidFlag flags, RaidFlag comp)
    {
        return (flags & comp) == comp;
    }

    /// <summary>
    /// Check if a string contains a substring starting at a given index.
    /// </summary>
    public static bool ContainsSubstringAt(this string self, string other, int startIndex)
    {
        if (startIndex + other.Length > self.Length)
            return false; //other string would be too long
        for (int i = 0; i < other.Length; i++)
        {
            if (self[startIndex + i] != other[i])
                return false; //other string doesnt match a character.
        }
        return true;
    }

    /// <summary>
    /// Try to parse a miscellaneous event
    /// </summary>
    /// <param name="subevent"></param>
    /// <param name="ev"></param>
    /// <returns></returns>
    public static bool TryParseMiscEventF(string subevent, out object ev)
    {
        for (int i = 0; i < miscEvents.Length; i++)
            if (subevent.StartsWithF(miscNames[i]))
            {
                ev = miscEvents[i];
                return true;
            }
        ev = CombatlogMiscEvents.UNDEFINED;
        return false;
    }

    public static UnitFlag NextFlags(string data, ref int index)
    {
        return (UnitFlag)HexStringToUInt(NextSubstring(data, ref index));
    }
    public static RaidFlag NextRaidFlags(string data, ref int index)
    {
        return (RaidFlag)HexStringToUInt(NextSubstring(data, ref index));
    }

    public static bool StartsWithAnyOf(this string str, params string[] sub)
    {
        foreach (string s in sub)
            if (str.StartsWithF(s))
                return true;
        return false;
    }

    /// <summary>
    /// Attempts extracting the NpcId from a GUID. 
    /// GUIDs must start with "Creature" or "Vehicle" to be accepted. Pets and Players are excluded from this.
    /// </summary>
    public static bool TryGetNpcId(string guid, out uint npcId)
    {
        npcId = 0;
        if (!guid.StartsWithAnyOf("Creature", "Vehicle"))
            return false;
        int endIndex = guid.LastIndexOf('-');
        int startIndex = guid.LastIndexOf('-', endIndex - 1) + 1;
        npcId = uint.Parse(guid[startIndex..endIndex]);
        return true;
    }

    /// <summary>
    /// Extracts the item group from the data string at the given index.
    /// Item groups are anything surrounded by ( ).
    /// If there is no ( at the start index, it will simply call NextSubstring.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="index"></param>
    /// <param name="divisor"></param>
    /// <returns></returns>
    public static string NextItemGroup(string data, ref int index, char divisor = ',')
    {
        bool isGroup = data[index] == '(';
        if (!isGroup)
            return NextSubstring(data, ref index, divisor);
        int startIndex = index + 1;
        int groupDepth = 1;
        for (index = startIndex; index < data.Length; index++)
        {
            var cchar = data[index];
            groupDepth += cchar == '(' ? 1 :
                          cchar == ')' ? -1 : 0;
            if (groupDepth != 0)
                continue;
            int endIndex = index;
            index += 2;
            if (startIndex == endIndex)
            {
                return string.Empty;
            }
            return data[startIndex..endIndex];
        }
        return string.Empty;
    }

	/// <summary>
	/// Extracts an array from the data string at the given index.
	/// Arrays are anything surrounded by [ ].
	/// If there is no [ at the start index, it will simply call NextSubstring.
	/// </summary>
	/// <param name="data"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	public static string NextArray(string data, ref int index)
    {
        bool isArray = data[index] == '[';
        if (!isArray)
            return NextSubstring(data, ref index);
        int startIndex = index + 1;
        int endIndex = data.IndexOf("]", startIndex);
        index = endIndex + 2;
        return data[startIndex..endIndex];
    }

    /// <summary>
    /// Splits a string up at every ',' 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static string[] SplitArgumentString(string data, int startIndex)
    {
        List<string> result = new(30);
        while (startIndex < data.Length)
        {
            result.Add(NextSubstring(data, ref startIndex));
        }
        return result.ToArray();
    }

    /// <summary>
    /// Counts the number of arguments in a string, where arguments are separated with a comma
    /// and commas inside of quotations are not counted.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="startIndex"></param>
    /// <returns></returns>
    public static int CountArguments(string data, int startIndex)
    {
        if (startIndex >= data.Length)
            return 0;
        int count = 1;
        bool inQuotes = false;
        for (int i = startIndex; i < data.Length; i++)
        {
            char c = data[i];
            if (c == '\"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Helper struct for subevent parsing.
    /// </summary>
    public readonly struct Subevent
    {
        public readonly CombatlogEventPrefix prefix;
        public readonly CombatlogEventSuffix suffix;

        public Subevent(CombatlogEventPrefix prefix, CombatlogEventSuffix suffix)
        {
            this.prefix = prefix;
            this.suffix = suffix;
        }
    }
}

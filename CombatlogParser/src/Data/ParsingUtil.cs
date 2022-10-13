using System.Globalization;
using System.Runtime.CompilerServices;

namespace CombatlogParser.Data
{
    public static class ParsingUtil
    {
        static readonly CultureInfo formatInfoProvider = CultureInfo.GetCultureInfo("en-US");
        static readonly string timestampFormat = "MM/dd HH:mm:ss.fff";

        /// <summary>
        /// Converts the Timestamps that come with the combatlog file into a DateTime object.
        /// </summary>
        /// <param name="timestamp">The full timestamp without trailing whitespace.</param>
        /// <returns></returns>
        public static DateTime StringTimestampToDateTime(string timestamp)
        {
            return DateTime.ParseExact(timestamp, timestampFormat, formatInfoProvider);
        }

        /// <summary>
        /// Converts a hex string in the "0x[0-F]+" format into a 32 unsigned integer.
        /// </summary>
        /// <returns></returns>
        public static uint HexStringToUint(string hex)
        {
            return Convert.ToUInt32(hex, 16);
        }

        /// <summary>
        /// Attempts to seperate a subevent into prefix and affix.
        /// </summary>
        /// <param name="subevent">The full subevent string. e.g. SPELL_DAMAGE</param>
        /// <returns></returns>
        public static bool TryParsePrefixAffixSubevent(string subevent, out CombatlogEventPrefix prefix, out CombatlogEventSuffix suffix)
        {
            int seperatorCount = 0;
            int[] seperatorIndices = new int[4]; //there are a max of 4 _ in any possible given subevent.

            //count the words in the subevent.
            for(int i = 0; i < subevent.Length; i++)
            {
                if (subevent[i] == '_')
                {
                    seperatorIndices[seperatorCount] = i;
                    seperatorCount++; //increment seperator count
                }
            }

            string remainder;
            //try to parse the two-word prefixes (SPELL_PERIODIC / SPELL_BUIDLING) 
            //the number of params is the same as just SPELL, but there are fewer prefixes than suffixes
            //so this way should be faster. (as opposed to double checking all suffix)
            if (seperatorCount > 1)
            {
                string longWord = subevent[0..(seperatorIndices[1])];
                remainder = subevent[seperatorIndices[1]..^0];

                if (TryParsePrefix(longWord, out prefix))
                {
                    if (TryParseSuffix(remainder, out suffix))
                    {
                        return true;
                    }
                }
            }
            //try to parse one-word prefixes (SPELL / SWING / RANGE / ENVIRONMENTAL)
            
            string shortWord = subevent[0..(seperatorIndices[0])];
            remainder = subevent[seperatorIndices[0]..^0];
            if (TryParsePrefix(shortWord, out prefix))
            {
                if (TryParseSuffix(remainder, out suffix))
                {
                    return true;
                }
            }
            
            //default to UNDEFINED when parse fails.
            prefix = CombatlogEventPrefix.UNDEFINED;
            suffix = CombatlogEventSuffix.UNDEFINED;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParsePrefix(string sub, out CombatlogEventPrefix prefix)
        {
            return Enum.TryParse(sub, out prefix);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryParseSuffix(string sub, out CombatlogEventSuffix suffix)
        {
            return Enum.TryParse(sub, out suffix);
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
                CombatlogEventPrefix.SPELL => 3,
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
                CombatlogEventSuffix._MISSED => 4,
                CombatlogEventSuffix._HEAL => 4,
                CombatlogEventSuffix._HEAL_ABSORBED => 9,
                CombatlogEventSuffix._ABSORBED => 0,
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
                if(c == '"')
                {
                    //quotations end. extract the string insde, move index past next divisor
                    if (insideQuotations)
                    {
                        sub = line[startIndex..i];
                        startIndex = i;
                        MovePastNextDivisor(line, ref startIndex, divisor);
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
                if (!insideQuotations && c == divisor && i+1 < line.Length || line[i] == '\n') 
                {
                    sub = line[startIndex..i];
                    startIndex = i + 1;
                    return sub;
                }
                //end of string
                if(i == line.Length-1)
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
        public static void MovePastNextDivisor(string line, ref int startIndex, char divisor = ',')
        {
            for(int i = startIndex; i < line.Length; i++)
            {
                if (line[i]==divisor && i + 1 < line.Length || line[i] == '\n')
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
    }
}

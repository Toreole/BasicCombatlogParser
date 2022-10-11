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

            //try to parse the two-word prefixes (SPELL_PERIODIC / SPELL_BUIDLING) 
            //the number of params is the same as just SPELL, but there are fewer prefixes than suffixes
            //so this way should be faster. (as opposed to double checking all suffix)
            if (seperatorCount > 1)
            {
                string longWord = subevent[0..(seperatorIndices[1])];

                if (TryParsePrefix(longWord, out prefix))
                {
                    if (TryParseSuffix(subevent[seperatorIndices[1]..^0], out suffix))
                    {
                        return true;
                    }
                }
            }
            //try to parse one-word prefixes (SPELL / SWING / RANGE / ENVIRONMENTAL)
            else
            {
                string shortWord = subevent[0..(seperatorIndices[0])];
                if (TryParsePrefix(shortWord, out prefix))
                {
                    if (TryParseSuffix(subevent[seperatorIndices[0]..^0], out suffix))
                    {
                        return true;
                    }
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

        public static int GetSuffixParamAmount(CombatlogEventSuffix suffix)
        {
            return suffix switch
            {
                CombatlogEventSuffix._DAMAGE => 10,
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
    }
}

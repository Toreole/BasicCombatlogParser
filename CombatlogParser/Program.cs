using CombatlogParser.Data;
using System.Globalization;

namespace CombatlogParser
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            CombatlogEventPrefix prefix;
            CombatlogEventSuffix suffix;
            string ev = "SPELL_DAMAGE";

            bool ok = ParsingUtil.TryParsePrefixAffixSubevent(ev, out prefix, out suffix);

            int x = 1;

            MainWindow window = new();
            window.ShowDialog();
        }

        static int ParseFromHexString(string hex) // mental note.
        {
            //for some weird unknown reason, AllowHexSpecifier does not allow the "0x" hex specifier / prefix.
            //but Convert.ToInt32( base=16) allows it.
            return Convert.ToInt32(hex, 16);
        }
    }
}
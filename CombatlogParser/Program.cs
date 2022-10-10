using System.Globalization;

namespace CombatlogParser
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            int x = 0;
            string b = "0xff";

            x = ParseFromHexString(b);

            x++;

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
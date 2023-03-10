using Microsoft.EntityFrameworkCore.Update;

namespace CombatlogParser
{
    public class Program
    {
        private const string APP_VERSION = "0.1.1a";

        [STAThread]
        static void Main()
        {
            bool updated = Config.Default.AppVersion != APP_VERSION; //- could check for app version changed, but what do?
            if (updated)
            { 
                //update the AppVersion.
                Config.Default.AppVersion = APP_VERSION;
                Config.Default.Save();
            }
            string fallbackFile = "combatlogLarge.txt";
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = System.IO.Path.Combine(Config.Default.WoW_Log_Directories, "Combatlog"),
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            bool? result = dialog.ShowDialog();
            if(result == true)
                CombatLogParser.ImportCombatlog(dialog.FileName);

            //this is where the main application runs.
            MainWindow app = new();
            app.ShowDialog();
        }
    }
}
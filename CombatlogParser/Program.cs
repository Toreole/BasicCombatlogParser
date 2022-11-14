namespace CombatlogParser
{
    public class Program
    {
        private const string APP_VERSION = "0.1.1a";

        [STAThread]
        static void Main()
        {
            bool updated = Config.Default.AppVersion != APP_VERSION; //- could check for app version changed, but what do?
            DB.InitializeConnection();
            DB.SetupDataTables();
            if (updated)
            { 
                //when a change in version is detected, try to upgrade the DB schema to match the latest.
                DB.Upgrade();
                //update the AppVersion.
                Config.Default.AppVersion = APP_VERSION;
                Config.Default.Save();
            }
            CombatLogParser.ImportCombatlog("combatlogLarge.txt"); //test with a relative path.
            //this is where the main application runs.
            //MainWindow app = new();
            //app.ShowDialog();

            //propertly shut down the DB before exiting.
            DB.Shutdown();
        }
    }
}
namespace CombatlogParser
{
    public class Program
    {
        private static readonly string APP_VERSION = "0.1.0a";

        [STAThread]
        static void Main()
        {
            //bool updated = Config.Default.AppVersion != APP_VERSION; - could check for app version changed, but what do?
            DB.InitializeConnection();
            DB.SetupDataTables();

            DB.Shutdown();
            return;
            MainWindow window = new();
            window.ShowDialog();
        }
    }
}
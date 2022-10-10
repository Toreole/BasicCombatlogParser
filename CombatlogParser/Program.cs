namespace CombatlogParser
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            MainWindow window = new();
            window.ShowDialog();
        }
    }
}
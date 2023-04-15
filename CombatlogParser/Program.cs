namespace CombatlogParser;

public class Program
{
    [STAThread]
    static void Main()
    {
        MainWindow app = new();
        app.ShowDialog();
    }
}
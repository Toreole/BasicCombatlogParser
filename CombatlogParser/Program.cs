using System.Diagnostics;

namespace CombatlogParser;

public class Program
{
	[STAThread]
	static void Main()
	{
#if DEBUG
		Debug.AutoFlush = true;
#endif
		MainWindow app = new();
		app.ShowDialog();
	}
}
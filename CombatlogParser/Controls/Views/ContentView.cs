using System.Windows.Controls;

namespace CombatlogParser.Controls.Views;

public abstract class ContentView : UserControl
{
	protected MainWindow? MainWindow { get; private set; }

	internal void SetWindow(MainWindow window) => MainWindow = window;
}

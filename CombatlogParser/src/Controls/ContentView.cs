using System.Windows.Controls;

namespace CombatlogParser.Controls;

public abstract class ContentView : UserControl
{
    protected MainWindow? MainWindow { get; private set; }

    internal void SetWindow(MainWindow window) => MainWindow = window;

    protected void ChangeActiveView(ContentView targetView)
    {
        if (MainWindow == null)
            throw new InvalidOperationException("MainWindow must be set");
        MainWindow.ChangeContent(targetView);
    }
}

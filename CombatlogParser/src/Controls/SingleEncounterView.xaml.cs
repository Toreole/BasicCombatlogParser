using System.Windows;
using System.Windows.Controls;

namespace CombatlogParser.Controls
{
    /// <summary>
    /// Interaction logic for SingleEncounterView.xaml
    /// </summary>
    public partial class SingleEncounterView : UserControl
    {
        private Button highlightedButton;
        public SingleEncounterView()
        {
            InitializeComponent();
            highlightedButton = DamageButton;
        }

        private void TabButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            MyTestList.GetData();
            highlightedButton.Style = this.Resources["selection-button-spaced"] as Style;
            highlightedButton = (Button)sender;
            object tag = highlightedButton.Tag;
            highlightedButton.Style = this.Resources["selection-button-spaced-highlighted"] as Style;
        }
    }
}

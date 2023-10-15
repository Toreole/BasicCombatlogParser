using CombatlogParser.Data.Metadata;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace CombatlogParser.Controls
{
    /// <summary>
    /// Interaction logic for CombatlogOverviewView.xaml
    /// </summary>
    public partial class CombatlogOverviewView : ContentView
    {
        public CombatlogMetadata? CombatlogMetadata { get; set; }
        private readonly ObservableCollection<EncounterInfoMetadata> encounters = new();

        public CombatlogOverviewView()
        {
            InitializeComponent();

            var binding = new Binding
            {
                Source = encounters
            };
            TestGrid.SetBinding(DataGrid.ItemsSourceProperty, binding);
        }

        public void FromEncounterMetadata(EncounterInfoMetadata encounterMetadata)
        {
            //ensure the embedded view has the mainwindow reference
            EmbeddedEncounterView.SetWindow(this.MainWindow!);
            CombatlogMetadata = Queries.GetCombatlogMetadataByID(encounterMetadata.CombatlogMetadataId);
            encounters.Clear();
            var xx = Queries.GetEncountersByCombatlogId(encounterMetadata.CombatlogMetadataId);
            foreach (var x in xx)
                encounters.Add(x);
        }

        private void TestGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            EmbeddedEncounterView.EncounterMetadata = TestGrid.SelectedItem as EncounterInfoMetadata;
        }
    }
}

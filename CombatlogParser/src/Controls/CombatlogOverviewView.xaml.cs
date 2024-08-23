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
		private CombatlogMetadata? currentCombatlog = null;
		private readonly ObservableCollection<EncounterInfoMetadata> encounters = new();

		public CombatlogMetadata? CombatlogMetadata
		{
			get => currentCombatlog;
			set
			{
				if (value == currentCombatlog || value == null) return;
				currentCombatlog = value;
				FetchAndStoreEncounterMetadatas(currentCombatlog.Id);
			}
		}

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
			CombatlogMetadata = Queries.GetCombatlogMetadataByID(encounterMetadata.CombatlogMetadataId)!;
			FetchAndStoreEncounterMetadatas(currentCombatlog!.Id);
		}

		private void FetchAndStoreEncounterMetadatas(uint combatlogId)
		{
			EmbeddedEncounterView.SetWindow(this.MainWindow!);
			encounters.Clear();
			var encounterMetadatas = Queries.GetEncountersByCombatlogId(combatlogId);
			foreach (var encounter in encounterMetadatas)
			{
				encounters.Add(encounter);
			}
		}

		private void TestGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			e.Handled = true;
			EmbeddedEncounterView.EncounterMetadata = TestGrid.SelectedItem as EncounterInfoMetadata;
		}
	}
}

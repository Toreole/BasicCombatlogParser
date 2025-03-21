using CombatlogParser.Controls.Styles;
using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Metadata;
using CombatlogParser.Data.WowEnums;
using CombatlogParser.Database;
using System.Windows.Controls;
using System.Windows.Data;

namespace CombatlogParser.Controls.Views;

/// <summary>
/// Interaction logic for PlayerMetadataView.xaml
/// </summary>
public partial class PlayerMetadataView : ContentView
{
	static readonly InstanceId[] AllRaids = InstanceEncounterUtility.OrderedRaids;
	static readonly DifficultyId[] Difficulties = new[]
	{
		DifficultyId.LFR,
		DifficultyId.Normal_Raid,
		DifficultyId.Heroic_Raid,
		DifficultyId.Mythic_Raid
	};

	private readonly List<EncounterId> encounters = new();
	//private int selectedEncounterIndex = 0;
	private PlayerMetadata? targetPlayer;

	private EncounterId SelectedEncounter => encounters[Math.Max(BossSelectionComboBox.SelectedIndex, 0)];
	private DifficultyId SelectedDifficulty => Difficulties[Math.Max(DifficultySelectionComboBox.SelectedIndex, 0)];
	private MetricType SelectedMetric => (MetricType)MetricSelectionComboBox.SelectedIndex;

	public PlayerMetadataView()
	{
		InitializeComponent();
		InitializeRaidSelection();
		InitializeForCurrentRaid();
		DifficultySelectionComboBox.SelectedIndex = 0;
		MetricSelectionComboBox.SelectedIndex = 0;
		SetupPerformanceListView();
		UpdatePerformanceList();
	}

	public PlayerMetadataView(PlayerMetadata playerMetadata) : this()
	{
		this.SetPlayer(playerMetadata);
	}

	private void InitializeRaidSelection()
	{
		var items = RaidSelectionComboBox.Items;
		items.Clear();
		foreach (var raid in AllRaids)
			items.Add(raid.GetDisplayName());
		RaidSelectionComboBox.SelectedIndex = 0;
	}

	private void InitializeForCurrentRaid()
	{
		InitializeBossSelectionForRaid(AllRaids[0]);
	}

	private void InitializeBossSelectionForRaid(InstanceId instance)
	{
		encounters.Clear();
		//selectedEncounterIndex = 0;
		encounters.Add(EncounterId.All_Bosses);
		encounters.AddRange(instance.GetEncounters());

		BossSelectionComboBox.Items.Clear();
		foreach (var i in encounters)
			BossSelectionComboBox.Items.Add(i.GetDisplayName());
		BossSelectionComboBox.SelectedIndex = 0;
	}

	private void UpdatePerformanceList()
	{
		if (targetPlayer is null)
			return;
		var items = BestPerformanceBossList.Items;
		items.Clear();
		var difficulty = SelectedDifficulty;
		if (SelectedEncounter == EncounterId.All_Bosses)
		{
			foreach (var boss in encounters)
			{
				if (boss == EncounterId.All_Bosses)
					continue;
				var data = Queries.GetPerformanceOverview(targetPlayer.Id, boss, difficulty, SelectedMetric);
				items.Add(data);
			}
		}
		else
		{
			var data = Queries.GetPlayerPerformances(targetPlayer.Id, SelectedEncounter, difficulty, SelectedMetric);
			foreach (var playerPerformance in data)
				items.Add(playerPerformance);
		}
	}

	private void SetupPerformanceListView()
	{
		var columns = PerformanceGridView.Columns;
		columns.Clear();
		var metricString = SelectedMetric.ToString();
		if (SelectedEncounter == EncounterId.All_Bosses)
		{
			//see PlayerEncounterPerformanceOverview.cs
			BestPerformanceBossList.Items.Clear(); //clear items because they might be incompatible.
			columns.Add(
				new() { Header = "Boss", DisplayMemberBinding = new Binding("EncounterName") }
				);
			columns.Add(
				new() { Header = $"Highest {metricString}", DisplayMemberBinding = new Binding("HighestMetricValue") }
				);
			columns.Add(
				new() { Header = $"Median {metricString}", DisplayMemberBinding = new Binding("MedianMetricValue") }
				);
			columns.Add(
				new() { Header = "Fastest Kill", DisplayMemberBinding = new Binding("FastestTime") }
				);
			columns.Add(
				new() { Header = "Recorded Kills", DisplayMemberBinding = new Binding("KillCount") }
				);
		}
		else
		{
			BestPerformanceBossList.Items.Clear();
			columns.Add(
				new() { Header = "Date", DisplayMemberBinding = new Binding("Date") }
				);
			columns.Add(
				new() { Header = metricString, DisplayMemberBinding = new Binding("MetricValue") }
				);
			columns.Add(
				new() { Header = "Duration", DisplayMemberBinding = new Binding("Duration") }
				);
			columns.Add(
				new() { Header = "ilvl", DisplayMemberBinding = new Binding("ItemLevel") }
				);
		}
	}

	public void SetPlayer(PlayerMetadata player)
	{
		targetPlayer = player;
		PlayerName.Content = player.Name;
		PlayerName.Foreground = player.ClassId.GetClassBrush();
		InitializeForCurrentRaid();
		SetupPerformanceListView();
		UpdatePerformanceList();
	}

	private void OnRaidSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		e.Handled = true;
		InitializeBossSelectionForRaid(AllRaids[RaidSelectionComboBox.SelectedIndex]);
		UpdatePerformanceList();
	}

	private void OnDifficultyChanged(object sender, SelectionChangedEventArgs e)
	{
		UpdatePerformanceList();
	}

	private void OnBossChanged(object sender, SelectionChangedEventArgs e)
	{
		SetupPerformanceListView();
		UpdatePerformanceList();
	}

	private void OnMetricChanged(object sender, SelectionChangedEventArgs e)
	{
		SetupPerformanceListView();
		UpdatePerformanceList();
	}

	private void PerformanceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (SelectedEncounter == EncounterId.All_Bosses)
		{
			BossSelectionComboBox.SelectedIndex = BestPerformanceBossList.SelectedIndex + 1;
			//BestPerformanceBossList.SelectedIndex = -1;
			OnBossChanged(sender, e);
		}
		else
		{
			var selectedPerformance = (PlayerPerformance)BestPerformanceBossList.SelectedItem;
			if (selectedPerformance != null && MainWindow != null)
			{
				MainWindow.ChangeContent(new SingleEncounterView())
					.EncounterMetadataId = selectedPerformance.EncounterMetadataId;
			}
		}
	}
}

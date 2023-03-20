using CombatlogParser.Data.Metadata;
using CombatlogParser.DBInteract;
using System.Windows.Controls;
using System.Windows.Data;

namespace CombatlogParser.Controls
{
    /// <summary>
    /// Interaction logic for PlayerMetadataView.xaml
    /// </summary>
    public partial class PlayerMetadataView : UserControl
    {
        static readonly InstanceId[] AllRaids = new[]{
            InstanceId.Vault_of_the_Incarnates,
            InstanceId.Sepulcher_of_the_First_Ones,
            InstanceId.Sanctum_of_Domination,
            InstanceId.Castle_Nathria, 
        };
        static readonly DifficultyId[] Difficulties = new[]
        {
            DifficultyId.LFR,
            DifficultyId.Normal_Raid,
            DifficultyId.Heroic_Raid,
            DifficultyId.Mythic_Raid
        };

        private List<EncounterId> encounters = new();
        private int selectedEncounterIndex = 0;
        private PlayerMetadata? targetPlayer;

        public PlayerMetadataView()
        {
            InitializeComponent();
            targetPlayer = Queries.FindPlayersWithNameLike("Bäng").FirstOrDefault(); //should find Bänger
            PlayerName.Content = targetPlayer?.Name ?? "could not find player";
            InitializeRaidSelection();
            InitializeForCurrentRaid();
            SetupPerformanceListView();
            UpdatePerformanceList();
        }

        private void InitializeRaidSelection()
        {
            var items = RaidSelectionComboBox.Items;
            items.Clear();
            foreach (var raid in AllRaids)
                items.Add(raid.ToPrettyString());
            RaidSelectionComboBox.SelectedIndex = 0;
        }

        private void InitializeForCurrentRaid()
        {
            InitializeBossSelectionForRaid(AllRaids[0]);
        }

        private void InitializeBossSelectionForRaid(InstanceId instance)
        {
            encounters.Clear();
            selectedEncounterIndex = 0;
            encounters.Add(EncounterId.All_Bosses);
            encounters.AddRange(instance.GetEncounters());

            BossSelectionComboBox.Items.Clear();
            foreach (var i in encounters)
                BossSelectionComboBox.Items.Add(i.ToPrettyString());
            BossSelectionComboBox.SelectedIndex = 0;
        }

        private void UpdatePerformanceList()
        {
            if (targetPlayer is null) 
                return;
            var items = BestPerformanceBossList.Items;
            items.Clear();
            var difficulty = Difficulties[DifficultySelectionComboBox.SelectedIndex];
            foreach(var boss in encounters)
            {
                if (boss == EncounterId.All_Bosses)
                    continue;
                var data = Queries.GetPerformanceOverview(targetPlayer.Id, boss, difficulty);
                items.Add(data);
            }
        }

        private void SetupPerformanceListView()
        {
            var columns = PerformanceGridView.Columns;
            columns.Clear();
            if (encounters[selectedEncounterIndex] == EncounterId.All_Bosses)
            {
                //see PlayerEncounterPerformanceOverview.cs
                columns.Add(
                    new() { Header = "Boss", DisplayMemberBinding = new Binding("EncounterName") }
                    );
                columns.Add(
                    new() { Header = "Highest Dps", DisplayMemberBinding = new Binding("HighestMetricValue") }
                    );
                columns.Add(
                    new() { Header = "Median Dps", DisplayMemberBinding = new Binding("MedianMetricValue") }
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
                columns.Add(
                    new() { Header = "Dps", DisplayMemberBinding = new Binding("Dps") }
                    );
                //columns.Add( 
                //    new() { Header = "Date", DisplayMemberBinding = new Binding("Date") }
                //    );
                //columns.Add( 
                //    new() { Header = "ilvl", DisplayMemberBinding = new Binding("ItemLevel") }
                //    );
            }
        }

        public void SetPlayer(PlayerMetadata player)
        {
            targetPlayer = player;
            PlayerName.Content = player.Name;
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

        }
    }
}

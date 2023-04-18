using CombatlogParser.Data.DisplayReady;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Metadata;
using CombatlogParser.Data;
using CombatlogParser.DBInteract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CombatlogParser.Controls
{
    /// <summary>
    /// Interaction logic for SingleEncounterView.xaml
    /// </summary>
    public partial class SingleEncounterView : UserControl
    {
        private Button highlightedButton;

        private EncounterInfo? currentEncounter = null;

        public SingleEncounterView()
        {
            InitializeComponent();
            highlightedButton = DamageButton;
            highlightedButton.Style = this.Resources["selection-button-spaced-highlighted"] as Style;
        }

        private void TabButtonClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            highlightedButton.Style = this.Resources["selection-button-spaced"] as Style;
            highlightedButton = (Button)sender;
            object tag = highlightedButton.Tag;
            highlightedButton.Style = this.Resources["selection-button-spaced-highlighted"] as Style;
        }

        public void GetData(EncounterInfoMetadata encounterInfoMetadata)
        {
            DataGrid.Items.Clear();

            EncounterInfo encounterInfo = CombatLogParser.ParseEncounter(encounterInfoMetadata);

            Dictionary<string, long> damageBySource = new();
            var damageEvents = encounterInfo.CombatlogEventDictionary.GetEvents<DamageEvent>();
            var filter = EventFilters.AllySourceEnemyTargetFilter;
            foreach (var dmgEvent in damageEvents.Where(filter.Match))
            {
                string? actualSource;
                if (!encounterInfo.SourceToOwnerGuidLookup.TryGetValue(dmgEvent.SourceGUID, out actualSource))
                    actualSource = dmgEvent.SourceGUID;
                if (damageBySource.ContainsKey(actualSource))
                    damageBySource[actualSource] += dmgEvent.Amount;
                else
                    damageBySource[actualSource] = dmgEvent.Amount;
            }
            (string sourceGuid, string sourceName, long damage)[] results = new (string, string, long)[damageBySource.Count];
            int i = 0;
            long totalDamage = 0;
            foreach (var pair in damageBySource.OrderByDescending(x => x.Value))
            {
                results[i] = (
                    pair.Key,
                    encounterInfo.CombatlogEvents.First(x => x.SourceGUID == pair.Key).SourceName,
                    pair.Value
                );
                totalDamage += pair.Value;
                i++;
            }

            var encounterLength = encounterInfo.LengthInSeconds;
            NamedValueBarData[] displayData = new NamedValueBarData[results.Length];
            long maxDamage = results[0].damage;
            for (i = 0; i < displayData.Length; i++)
            {
                PlayerInfo? player = encounterInfo.FindPlayerInfoByGUID(results[i].sourceGuid);
                if (player != null)
                {
                    displayData[i] = new()
                    {
                        Name = player.Name,
                        Color = player.Class.GetClassBrush(),
                        Maximum = maxDamage,
                        Value = results[i].damage,
                        ValueString = (results[i].damage / encounterLength).ToString("N1")
                    };
                }
                else
                {
                    displayData[i] = new()
                    {
                        Name = results[i].sourceName,
                        Color = Brushes.Red,
                        Maximum = maxDamage,
                        Value = results[i].damage,
                        ValueString = (results[i].damage / encounterLength).ToString("N1")
                    };
                }
            }
            foreach (var entry in displayData)
                DataGrid.Items.Add(entry);
        }

        public void GetData()
        {
            using CombatlogDBContext dbContext = new();
            EncounterInfoMetadata encounterInfoMetadata = dbContext.Encounters
                .Where(x => x.Success).First();
            GetData(encounterInfoMetadata);
        }
    }
}

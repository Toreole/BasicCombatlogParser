using CombatlogParser.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CombatlogParser.Data.DisplayReady;
using CombatlogParser.DBInteract;
using CombatlogParser.Data.Metadata;

namespace CombatlogParser.src.Controls
{
    /// <summary>
    /// Interaction logic for TestList.xaml
    /// </summary>
    public partial class TestList : UserControl
    {
        public TestList()
        {
            InitializeComponent();

            GetData();
            //AddRandomSampleData();
        }

        private void GetData()
        {
            using CombatlogDBContext dbContext = new();
            EncounterInfoMetadata firstKill = dbContext.Encounters
                .Where(x => x.Success).First();
            PerformanceMetadata[] performances = dbContext.Performances
                .Where(x => x.EncounterInfoMetadataId == firstKill.Id).ToArray();
            PlayerMetadata[] players = new PlayerMetadata[performances.Length];
            for (int i = 0; i < players.Length; i++)
                players[i] = dbContext.Players.Where(x => x.Id == performances[i].PlayerMetadataId).First();

            var encounterLength = (firstKill.EncounterDurationMS / 1000.0);
            var maxDamage = performances.OrderByDescending(x => x.Dps).First().Dps * encounterLength;
            var displayedData = new NamedValueBarData[players.Length];
            for(int i = 0; i < players.Length; i++)
            {
                displayedData[i] = new()
                {
                    Name = players[i].Name,
                    Color = players[i].ClassId.GetClassBrush(),
                    Maximum = maxDamage,
                    Value = performances[i].Dps * encounterLength,
                    ValueString = performances[i].Dps.ToString("N1")
                };
            }
            foreach (var entry in displayedData.OrderByDescending(x => x.Value))
                DataGrid.Items.Add(entry);
        }

        private void AddRandomSampleData()
        {
            Random random = new();
            //warning: highly illegal code
            byte[] objects = new byte[10];
            var classes = Enum.GetValues<ClassId>();
            var playerPerformances = objects.Select(x =>
            {
                double num = random.NextDouble() * 10000.0d;
                return new NamedValueBarData()
                {
                    Name = "Player",
                    Value = num,
                    Maximum = 10000.0,
                    ValueString = num.ToString("0.0"),
                    Color = classes[random.Next(1, classes.Length)].GetClassBrush(),
                };
            }).OrderByDescending(x => x.Value).ToArray();
            var max = playerPerformances.Max(x => x.Value);

            foreach (var x in playerPerformances) {
                x.Maximum = max;
                DataGrid.Items.Add(x);
            }

        }

        void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if(sender is Button b)
            {
                object arg = b.Tag;
                e.Handled = true;
            }
        }

    }
}

namespace CombatlogParser.Data.DisplayReady
{
    public class NamedValueBarData
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double Maximum { get; set; }
        public string ValueString { get; set; }
        public Brush Color { get; set; }
    }
}
using CombatlogParser.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            AddRandomSampleData();
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
                return new TestData()
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

        public class TestData 
        {
            public string Name { get; set; }
            public double Value { get; set; }
            public double Maximum { get; set; }
            public string ValueString { get; set; }
            public Brush Color { get; set; }
        }
    }
}

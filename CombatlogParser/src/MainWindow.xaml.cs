using CombatlogParser.Data;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CombatlogParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //create a binding with the path pointing towards "Value" (which is a property of the ObservableString)
            var myBinding = new Binding("HeaderLabelText")
            {
                //the Source needs to implement INotifyPropertyChanged to notify of changes (its all event based)
                Source = this
            };

            //apply the binding to the Label.ContentProperty 
            HeaderLabel.SetBinding(Label.ContentProperty, myBinding);

            //try reading a large combatlog into a full Combatlog object.
            Combatlog combatlog = CombatLogParser.ReadCombatlogFile("combatlogLarge.txt");

            if(combatlog.Encounters.Length > 1)
            {
                //filter out damage done by players.
                damageEvents = new(combatlog.Encounters[1].AllEventsThatMatch(
                    SubeventFilter.DamageEvents,
                    new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_CONTROL_PLAYER)
                    ));
            }

            var dmgEventsBinding = new Binding()
            {
                Source = damageEvents
            };
            DamageEventsList.SetBinding(ListView.ItemsSourceProperty, dmgEventsBinding);

            var eventsBinding = new Binding()
            {
                Source = events
            };
            CombatLogEventsList.SetBinding(ListView.ItemsSourceProperty, eventsBinding);

            //This bit works for initializing the Content to "Test", but it will not receive updates, as
            //the class does not implement INotifyPropertyChanged 
            //var binding = new Binding("Message")
            //{
            //    Source = test
            //};
            //HeaderLabel.SetBinding(Label.ContentProperty, binding);
        }


        public string HeaderLabelText => "Hello World!";
        private ObservableCollection<CombatlogEvent> events = new();
        private ObservableCollection<CombatlogEvent> damageEvents = new();

        //private TestCl test = new() { Message = "Test" };

        private Random random = new();

        private void RandomizeLabelButton_Click(object sender, RoutedEventArgs e)
        {
            events.Add(new());
        }
    }
}

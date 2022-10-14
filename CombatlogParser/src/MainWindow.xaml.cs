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

            //apply the binding to the Label.ContentProperty 
            HeaderLabel.Content = "Combatlogs";

            //try reading a large combatlog into a full Combatlog object.
            currentCombatlog = CombatLogParser.ReadCombatlogFile("combatlogLarge.txt");

            for(int i = 0; i < currentCombatlog.Encounters.Length; i++)
            {
                var enc = currentCombatlog.Encounters[i];
                EncounterSelection.Items.Add($"{i}:{enc.EncounterName}: {(enc.EncounterSuccess? "Kill" : "Wipe")}  - ({ParsingUtil.MillisecondsToReadableTimeString(enc.EncounterDuration)})");
            }
            EncounterSelection.SelectionChanged += OnEncounterChanged;

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

            var dmgBreakdownBinding = new Binding()
            {
                Source = damageSummaries
            };
            DmgPerSourceList.SetBinding(ListView.ItemsSourceProperty, dmgBreakdownBinding);
            //This bit works for initializing the Content to "Test", but it will not receive updates, as
            //the class does not implement INotifyPropertyChanged 
            //var binding = new Binding("Message")
            //{
            //    Source = test
            //};
            //HeaderLabel.SetBinding(Label.ContentProperty, binding);
        }

        private void OnEncounterChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = EncounterSelection.SelectedIndex;
            damageEvents.Clear();

            var de = currentCombatlog.Encounters[index].AllEventsThatMatch(
                SubeventFilter.DamageEvents,
                new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE | UnitFlag.COMBATLOG_OBJECT_TYPE_NPC),
                new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID)
            );
            foreach (var d in de)
                damageEvents.Add(d);

            damageSumDict.Clear();
            damageSummaries.Clear();

            foreach(var dmgevent in de)
            {
                //add to existing data
                if (damageSumDict.TryGetValue(dmgevent.SourceGUID, out DamageSummary? sum))
                {
                    sum.TotalDamage += uint.Parse((string)dmgevent.SuffixParam0);
                }
                else //create new sum
                {
                    damageSumDict[dmgevent.SourceGUID] = new()
                    {
                        SourceName = dmgevent.SourceName,
                        TotalDamage = uint.Parse((string)dmgevent.SuffixParam0)
                    };
                }
            }
            //add up all damage done to absorb shields on enemies.
            foreach(CombatlogEvent absorbEvent in currentCombatlog.Encounters[index].AllEventsThatMatch(
                MissTypeFilter.Absorbed,
                new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE | UnitFlag.COMBATLOG_OBJECT_TYPE_NPC),
                new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID)
                ))
            {
                if (damageSumDict.TryGetValue(absorbEvent.SourceGUID, out DamageSummary? sum))
                    sum.TotalDamage += uint.Parse((string)absorbEvent.SuffixParam2);
                else
                    damageSumDict[absorbEvent.SourceGUID] = new()
                    {
                        SourceName = absorbEvent.SourceName,
                        TotalDamage = uint.Parse((string)absorbEvent.SuffixParam2)
                    };
            }
            //divide damage to calculate DPS across the encounter.
            float encounterSeconds = currentCombatlog.Encounters[index].LengthInSeconds;
            foreach(var dmgsum in damageSumDict.Values)
            {
                dmgsum.DPS = dmgsum.TotalDamage / encounterSeconds;
                damageSummaries.Add(dmgsum);
            }
        }

        private Combatlog currentCombatlog;
        private ObservableCollection<CombatlogEvent> events = new();
        private ObservableCollection<CombatlogEvent> damageEvents = new();

        //TODO: assign pets to owners, process damage accordingly
        private Dictionary<string, string> petToOwnerGUID = new();

        private Dictionary<string, DamageSummary> damageSumDict = new();
        private ObservableCollection<DamageSummary> damageSummaries = new();

        //private TestCl test = new() { Message = "Test" };

        private Random random = new();

        private void RandomizeLabelButton_Click(object sender, RoutedEventArgs e)
        {
            events.Add(new());
        }
    }
}

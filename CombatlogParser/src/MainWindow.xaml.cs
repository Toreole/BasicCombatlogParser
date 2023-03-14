using CombatlogParser.Data;
using CombatlogParser.Data.Events;
using CombatlogParser.Data.Metadata;
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
        //private Combatlog currentCombatlog = new();
        //private ObservableCollection<CombatlogEvent> events = new();
        //private ObservableCollection<DamageEvent> damageEvents = new();

        ////TODO: assign pets to owners, process damage accordingly
        //private Dictionary<string, string> petToOwnerGUID = new();

        //private Dictionary<string, DamageSummary> damageSumDict = new();
        //private ObservableCollection<DamageSummary> damageSummaries = new();

        private ObservableCollection<CombatlogMetadata> combatlogs = new();
        private uint lastLogId;
        private ObservableCollection<EncounterInfoMetadata> encounters = new();
        private uint lastEncounterId;
        private ObservableCollection<PerformanceMetadata> performances = new();
        private uint lastPerformanceId;
        private ObservableCollection<PlayerMetadata> players = new();
        private uint lastPlayerId;

        public MainWindow()
        {
            InitializeComponent();

            //apply the binding to the Label.ContentProperty 
            HeaderLabel.Content = "Combatlogs";

            ////try reading a large combatlog into a full Combatlog object.
            //currentCombatlog = CombatLogParser.ReadCombatlogFile("combatlogLarge.txt");

            //for (int i = 0; i < Encounters.Length; i++)
            //{
            //    var enc = Encounters[i];
            //    EncounterSelection.Items.Add($"{i}:{enc.WowEncounterId}: {(enc.Success ? "Kill" : "Wipe")}  - ({ParsingUtil.MillisecondsToReadableTimeString((uint)enc.EncounterDurationMS)})");
            //}
            //EncounterSelection.SelectionChanged += OnEncounterChanged;

            var combatlogsBinding = new Binding()
            {
                Source = combatlogs
            };
            CombatlogListView.SetBinding(ListView.ItemsSourceProperty, combatlogsBinding);

            var cls = Queries.GetCombatlogMetadata(0, 10);
            foreach (var c in cls)
                if (c != null)
                    combatlogs.Add(c);

            var encountersBinding = new Binding()
            {
                Source = encounters
            };
            EncounterInfoListView.SetBinding(ListView.ItemsSourceProperty, encountersBinding);

            var ens = Queries.GetEncounterInfoMetadata(0, 30);
            foreach (var e in ens)
                if (e != null)
                    encounters.Add(e);

            var performancesBinding = new Binding()
            {
                Source = performances
            };
            PerformancesListView.SetBinding(ListView.ItemsSourceProperty, performancesBinding);

            var perfs = Queries.GetPerformanceMetadata(0, 30);
            foreach (var p in perfs)
                if (p != null)
                    performances.Add(p);

            var playersBinding = new Binding()
            {
                Source = players
            };
            PlayersListView.SetBinding(ListView.ItemsSourceProperty, playersBinding);

            var pls = Queries.GetPlayerMetadata(0, 30);
            foreach (var p in pls)
                if (p != null)
                    players.Add(p);

            //var dmgEventsBinding = new Binding()
            //{
            //    Source = damageEvents
            //};
            //DamageEventsList.SetBinding(ListView.ItemsSourceProperty, dmgEventsBinding);

            //var eventsBinding = new Binding()
            //{
            //    Source = events
            //};
            //CombatLogEventsList.SetBinding(ListView.ItemsSourceProperty, eventsBinding); //the "All Events" list is disabled.

            //var dmgBreakdownBinding = new Binding()
            //{
            //    Source = damageSummaries
            //};
            //DmgPerSourceList.SetBinding(ListView.ItemsSourceProperty, dmgBreakdownBinding);
            //This bit works for initializing the Content to "Test", but it will not receive updates, as
            //the class does not implement INotifyPropertyChanged
            //var binding = new Binding("Message")
            //{
            //Source = test
            //};
            //HeaderLabel.SetBinding(Label.ContentProperty, binding);
        }

        private void OnEncounterChanged(object sender, SelectionChangedEventArgs e)
        {
            //int index = EncounterSelection.SelectedIndex;
            //read the encounter back into memory. at this point the encounter metadata and the combatlog metadata should be linked.
            //EncounterInfo encounter = CombatLogParser.ParseEncounter(Encounters[index]);

            //damageEvents.Clear();

            //IEventFilter filter = new AllOfFilter(
            //        new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE | UnitFlag.COMBATLOG_OBJECT_TYPE_NPC), //to hostile NPCs
            //        new AnyOfFilter(
            //            new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY), //by either friendly
            //            new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL) //or neutral sources
            //        )
            //    );

            //var allDamageEvents = encounter.CombatlogEvents.GetEvents<DamageEvent>();
            //var filteredDamageEvents = allDamageEvents.AllThatMatch(filter); //basically all damage to enemies.

            //foreach (var d in allDamageEvents)
            //    damageEvents.Add(d);

            //damageSumDict.Clear();
            //damageSummaries.Clear();
            //petToOwnerGUID.Clear();

            //1. Populate the damageSumDict with all unique players.
            //foreach(var player in encounter.Players)
            //{
            //    damageSumDict.Add(player.GUID, new() {
            //        SourceName = encounter.CombatlogEvents.First(x => x.SourceGUID == player.GUID)?.SourceName ?? "Unknown"
            //    });
                
            //}
            
            //2. register all pets that were summoned during the encounter.
            //foreach(SummonEvent summonEvent in encounter.CombatlogEvents.GetEvents<SummonEvent>())
            //{
            //    //pets are the target, source is the summoning player.
            //    //no check needed here, dictionary is guaranteed to be empty. 
            //    //summon events only happen once per unit summoned
            //    petToOwnerGUID.Add(summonEvent.TargetGUID, summonEvent.SourceGUID); 
            //}
            //3. accessing advanced params, therefore need to check if advanced logging is enabled.
            if (false) //--NOTE THIS NEEDS AN UPDATE.
            {
                //register all pets that had some form of cast_success
                //foreach (CombatlogEvent castEvent in encounter.AllEventsThatMatch(
                //    SubeventFilter.CastSuccessEvents, //SPELL_CAST_SUCCESS
                //    new NotFilter(new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_TYPE_PLAYER)), //Guardians / pets / NPCs 
                //    new NotFilter(new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE)) //allied guardians/pets are never hostile.
                //))
                //{
                //    if (petToOwnerGUID.ContainsKey(castEvent.SourceGUID) == false) //dont add duplicates of course.
                //        petToOwnerGUID.Add(castEvent.SourceGUID, castEvent.GetOwnerGUID());
                //}
            }

            //4. sort out the damage events.
            //foreach(var dmgevent in filteredDamageEvents)
            //{
            //    string sourceGUID = dmgevent.SourceGUID;
            //    //check for pet melee attacks
            //    //check if this is registered as a pet. warlock demons do not have the pet flag, therefore cant be easily checked outside of SPELL_SUMMON events.
            //    if (petToOwnerGUID.TryGetValue(sourceGUID, out string? ownerGUID))
            //    {
            //        sourceGUID = ownerGUID;
            //    }
            //    //else if (dmgevent.IsSourcePet && dmgevent.SubeventPrefix == CombatlogEventPrefix.SWING && false) //--WOAH
            //    //{
            //    //    //pet swing damage as the owner GUID as advanced param
            //    //    sourceGUID = dmgevent.GetOwnerGUID();
            //    //    if (petToOwnerGUID.ContainsKey(dmgevent.SourceGUID) == false)
            //    //        petToOwnerGUID[dmgevent.SourceGUID] = sourceGUID;
            //    //}

            //    //add to existing data
            //    //if (damageSumDict.TryGetValue(sourceGUID, out DamageSummary? sum))
            //    //{
            //    //    sum.TotalDamage += dmgevent.amount;
            //    //}
            //    //else //create new sum
            //    //{
            //    //    damageSumDict[sourceGUID] = new()
            //    //    {
            //    //        SourceName = dmgevent.SourceName, //--NOTE: This sometimes still adds a summary with the Pets name. Which is bad.
            //    //        TotalDamage = dmgevent.amount
            //    //    };
            //    //}
            //}

            /* skipping this bit for now.
            //add up all damage done to absorb shields on enemies.
            foreach(CombatlogEvent absorbEvent in currentCombatlog.Encounters[index].AllEventsThatMatch(
                MissTypeFilter.Absorbed,
                new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE | UnitFlag.COMBATLOG_OBJECT_TYPE_NPC),
                new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_AFFILIATION_RAID)
                ))
            {
                if (petToOwnerGUID.TryGetValue(absorbEvent.SourceGUID, out string? sourceGUID) == false)
                    sourceGUID = absorbEvent.SourceGUID;

                if (damageSumDict.TryGetValue(sourceGUID, out DamageSummary? sum))
                    sum.TotalDamage += uint.Parse((string)absorbEvent.SuffixParam2);
                else
                    damageSumDict[sourceGUID] = new()
                    {
                        SourceName = absorbEvent.SourceName,
                        TotalDamage = uint.Parse((string)absorbEvent.SuffixParam2)
                    };
            }*/


            //List<DamageSummary> sums = new List<DamageSummary>(damageSumDict.Values);
            //sums.Sort((x, y) => x.TotalDamage > y.TotalDamage ? -1 : 1);
            //divide damage to calculate DPS across the encounter.
            //float encounterSeconds = Encounters[index].EncounterDurationMS / 1000;
            //foreach(var dmgsum in sums)
            //{
            //    dmgsum.DPS = dmgsum.TotalDamage / encounterSeconds;
            //    damageSummaries.Add(dmgsum);
            //}
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs eargs)
        {
            //encounters.Clear();
            //var encounterInfos = Queries.GetEncounterInfoMetadata(lastEncounterId, 30);
            //foreach (var e in encounterInfos)
            //    if (e != null)
            //    {
            //        encounters.Add(e);
            //        lastEncounterId = e.Id;
            //    }

            performances.Clear();
            var performanceInfos = Queries.GetPerformanceMetadata(lastPerformanceId, 30);
            foreach (var e in performanceInfos)
                if (e != null)
                {
                    performances.Add(e);
                    lastPerformanceId = e.Id;
                }

            PerformancesListView.UpdateLayout();
        }

        private void PlayerSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(PlayerSearch.Text) || string.IsNullOrWhiteSpace(PlayerSearch.Text))
            {
                players.Clear();
                var pls = Queries.GetPlayerMetadata(0, 30);
                foreach (var p in pls)
                    if (p != null)
                        players.Add(p);
                return;
            }
            
            PlayerMetadata[] results = Queries.FindPlayersWithNameLike(PlayerSearch.Text);
            players.Clear();
            foreach (var p in results)
                players.Add(p);
        }
    }
}

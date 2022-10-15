﻿using CombatlogParser.Data;
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
        private Combatlog currentCombatlog;
        private ObservableCollection<CombatlogEvent> events = new();
        private ObservableCollection<CombatlogEvent> damageEvents = new();

        //TODO: assign pets to owners, process damage accordingly
        private Dictionary<string, string> petToOwnerGUID = new();

        private Dictionary<string, DamageSummary> damageSumDict = new();
        private ObservableCollection<DamageSummary> damageSummaries = new();

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
            EncounterInfo encounter = currentCombatlog.Encounters[index];
            damageEvents.Clear();

            var filteredDamageEvents = encounter.AllEventsThatMatch(
                SubeventFilter.DamageEvents, //Damage done
                new TargetFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_HOSTILE | UnitFlag.COMBATLOG_OBJECT_TYPE_NPC), //to hostile NPCs
                new AnyOfFilter(
                    new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_FRIENDLY), //by either friendly
                    new SourceFlagFilter(UnitFlag.COMBATLOG_OBJECT_REACTION_NEUTRAL) //or neutral sources
                    )
            );
            foreach (var d in filteredDamageEvents)
                damageEvents.Add(d);

            damageSumDict.Clear();
            damageSummaries.Clear();
            petToOwnerGUID.Clear();

            //register all pets in the events.
            foreach(CombatlogEvent summonEvent in encounter.AllEventsThatMatch(SubeventFilter.SummonEvents))
            {
                //pets are the target, source is the summoning player.
                petToOwnerGUID[summonEvent.TargetGUID] = summonEvent.SourceGUID;
            }

            //sort out the damage events.
            foreach(var dmgevent in filteredDamageEvents)
            {
                string sourceGUID = dmgevent.SourceGUID;
                //check for pet melee attacks
                //check if this is registered as a pet. warlock demons do not have the pet flag, therefore cant be easily checked outside of SPELL_SUMMON events.
                if(petToOwnerGUID.TryGetValue(sourceGUID, out string? ownerGUID))
                {
                    sourceGUID = ownerGUID;
                }
                else if(dmgevent.IsSourcePet && dmgevent.SubeventPrefix == CombatlogEventPrefix.SWING && currentCombatlog.AdvancedLogEnabled)
                {
                    //pet swing damage as the owner GUID as advanced param
                    sourceGUID = dmgevent.AdvancedParams[1];
                    if(petToOwnerGUID.ContainsKey(dmgevent.SourceGUID) == false)
                        petToOwnerGUID[dmgevent.SourceGUID] = sourceGUID;
                }
                
                //add to existing data
                if (damageSumDict.TryGetValue(sourceGUID, out DamageSummary? sum))
                {
                    sum.TotalDamage += uint.Parse((string)dmgevent.SuffixParam0);
                }
                else //create new sum
                {
                    damageSumDict[sourceGUID] = new()
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
                string sourceGUID = absorbEvent.SourceGUID;
                if (petToOwnerGUID.TryGetValue(sourceGUID, out string? ownerGUID))
                    sourceGUID = ownerGUID;

                if (damageSumDict.TryGetValue(absorbEvent.SourceGUID, out DamageSummary? sum))
                    sum.TotalDamage += uint.Parse((string)absorbEvent.SuffixParam2);
                else
                    damageSumDict[absorbEvent.SourceGUID] = new()
                    {
                        SourceName = absorbEvent.SourceName,
                        TotalDamage = uint.Parse((string)absorbEvent.SuffixParam2)
                    };
            }

            //NOTES FOR PETS:
            //
            // HUNTER pets have swing_damage events, that actually DO show the GUID of the hunter as the owner.
            // example:
            // 10/12 19:54:04.757  SWING_DAMAGE,
            // Pet-0-3061-2450-11519-165189-010195110A,"Unbekannt",0x1114,0x0,
            // Creature-0-3061-2450-11519-176523-000046FD80,"Leidensschmied Raznal",0x10a48,0x0,
            //
            // Pet-0-3061-2450-11519-165189-010195110A,Player-3391-068AB778, <- advancedParam[1]
            // 76609,76609,1791,1791,2788,0,2,120,120,0,390.53,-1226.02,2000,5.2754,301,
            //
            // 1047,678,-1,1,0,0,0,1,nil,nil <- _DAMAGE suffix params
            //
            // WARLOCK pets have spell_cast_success events that show the warlock GUID as owner.
            // example: 
            // 10/12 20:04:19.569  SPELL_CAST_SUCCESS,Creature-0-3061-2450-11519-135002-000047019D,"Dämonischer Tyrann",0x2114,0x0,
            // Creature-0-3061-2450-11519-176523-0000470046,"Leidensschmied Raznal",0xa48,0x0,
            //
            // 270481,"Dämonenfeuer",0x24, <- SPELL prefix params
            //
            // Creature-0-3061-2450-11519-135002-000047019D,Player-3391-094DCD7D, <- advancedParam[1]
            // 50950,50950,2958,2958,2368,0,3,100,100,0,404.75,-1234.59,2000,5.7194,300
            //
            // Demo / Enhance / Shadow "pets" are specified in SPELL_SUMMON
            // Permanent pets are the most difficult part of this (hunter pets, warlock perma pets)
            // in the case of SPELL_SUMMON targetGUID is the pet's GUID
            //
            // NEW PROBLEM
            // Hunter pets have the OBJECT_TYPE_PET flag, but warlock summons do not. they are treated as NPCs or Guardians


            //divide damage to calculate DPS across the encounter.
            float encounterSeconds = currentCombatlog.Encounters[index].LengthInSeconds;
            foreach(var dmgsum in damageSumDict.Values)
            {
                dmgsum.DPS = dmgsum.TotalDamage / encounterSeconds;
                damageSummaries.Add(dmgsum);
            }
        }
    }
}

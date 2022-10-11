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
            var myBinding = new Binding("Value")
            {
                //the Source needs to implement INotifyPropertyChanged to notify of changes (its all event based)
                Source = HeaderLabelText
            };

            //apply the binding to the Label.ContentProperty 
            HeaderLabel.SetBinding(Label.ContentProperty, myBinding);

            using(FileStream file = File.OpenRead("combatlog.txt"))
            {
                using (TextReader reader = new StreamReader(file))
                {
                    string? line = reader.ReadLine();

                    while ((line = reader.ReadLine()) != null)
                    {

                        int i = 20;
                        string sub = ParsingUtil.NextSubstring(line, ref i);

                        //try parsing the substring to a CombatlogSubevent.
                        if(ParsingUtil.TryParsePrefixAffixSubevent(sub, out CombatlogEventPrefix cPrefix, out CombatlogEventSuffix cSuffix))
                        {
                            CombatlogEvent clevent = new()
                            {
                                Timestamp = ParsingUtil.StringTimestampToDateTime(line[..18]),
                                SubeventPrefix = cPrefix,
                                SubeventSuffix = cSuffix
                            };

                            //sourceGUID and name
                            clevent.SourceUID = ParsingUtil.NextSubstring(line, ref i);
                            clevent.SourceName = ParsingUtil.NextSubstring(line, ref i);

                            //source flags.
                            clevent.SourceFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i)); 
                            clevent.SourceRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));

                            //targetGUID and name
                            clevent.TargetUID = ParsingUtil.NextSubstring(line, ref i);
                            clevent.TargetName = ParsingUtil.NextSubstring(line, ref i);

                            //target flags
                            clevent.TargetFlags = (UnitFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i)); 
                            clevent.TargetRaidFlags = (RaidFlag)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));

                            //at this point Prefix params can be handled (if any)
                            int prefixAmount = ParsingUtil.GetPrefixParamAmount(cPrefix);
                            if (prefixAmount > 0)
                            {
                                var prefixParams = new object[prefixAmount];
                                for (int j = 0; j < prefixAmount; j++)
                                {
                                    if (j == 2)
                                        prefixParams[j] = (SpellSchool)ParsingUtil.HexStringToUint(ParsingUtil.NextSubstring(line, ref i));
                                    else
                                        prefixParams[j] = ParsingUtil.NextSubstring(line, ref i);
                                }
                                clevent.PrefixParams = prefixParams;
                            }

                            //then follow the advanced combatlog params
                            //watch out! not all events have the advanced params!
                            if(ParsingUtil.SubeventContainsAdvancedParams(cSuffix))
                            {
                                var advancedParams = new string[17];
                                for(int j = 0; j < 17; j++)
                                {
                                    advancedParams[j] = ParsingUtil.NextSubstring(line, ref i);
                                }
                                clevent.AdvancedParams = advancedParams;
                            }

                            //lastly, suffix event params.
                            int suffixAmount = ParsingUtil.GetSuffixParamAmount(cSuffix);
                            if(suffixAmount > 0)
                            {
                                var suffixParams = new object[suffixAmount];
                                for(int j = 0; j < suffixAmount; j++)
                                {
                                    suffixParams[j] = ParsingUtil.NextSubstring(line, ref i);
                                }
                                clevent.SuffixParams = suffixParams;
                            }

                            events.Add(clevent);
                        }
                    }
                    //end of read loop
                }
            }

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

        private ObservableString HeaderLabelText = new("Hello World!");
        private ObservableCollection<CombatlogEvent> events = new();

        //private TestCl test = new() { Message = "Test" };

        private Random random = new();

        private void RandomizeLabelButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderLabelText.Value = random.Next(1000).ToString();
            //test.Message = "Two";
            events.Add(new());
        }
    }
}

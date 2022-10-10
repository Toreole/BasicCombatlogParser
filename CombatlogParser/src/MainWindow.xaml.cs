using CombatlogParser.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
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
                        CombatlogEvent clevent = new()
                        {
                            Timestamp = line[..20]
                        };

                        int i = 20;
                        string sub = NextSubstring(line, ref i);

                        //try parsing the substring to a CombatlogSubevent.
                        if (Enum.TryParse(typeof(CombatlogSubevent), sub, out object? se))
                        {
                            //if parse has succeeded, it can never be null.
                            clevent.SubEvent = (CombatlogSubevent)se!; 
                                      
                            //sourceGUID and name
                            clevent.SourceUID = NextSubstring(line, ref i);
                            clevent.SourceName = NextSubstring(line, ref i);

                            //skip over flags
                            NextSubstring(line, ref i); NextSubstring(line, ref i);

                            //targetGUID and name
                            clevent.TargetUID = NextSubstring(line, ref i);
                            clevent.TargetName = NextSubstring(line, ref i);

                            //skip over flags for now
                            NextSubstring(line, ref i); NextSubstring(line, ref i);

                            //at this point Prefix params can be handled (if any)

                            //then follow the advanced combatlog params

                            //lastly, suffix event params.

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

        //Returns the next substring in the line, and moves the index to the start of the next one.
        private string NextSubstring(string line, ref int startIndex)
        {
            string sub;
            for (int i = startIndex; i < line.Length; i++)
            {
                if (line[i] == ',' || line[i] == '\n')
                {
                    sub = line[startIndex..i];
                    startIndex = i + 1;
                    return sub;
                }
            }
            sub = "";
            startIndex = line.Length;
            return sub;
        }

        private ObservableString HeaderLabelText = new("Hello World!");
        private ObservableCollection<CombatlogEvent> events = new();

        //private TestCl test = new() { Message = "Test" };

        private Random random = new Random();

        private void RandomizeLabelButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderLabelText.Value = random.Next(1000).ToString();
            //test.Message = "Two";
            events.Add(new());
        }
    }
}

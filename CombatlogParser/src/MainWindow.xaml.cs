using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ObservableCollection<CombatEvent> events = new();

        //private TestCl test = new() { Message = "Test" };

        private Random random = new Random();

        private void RandomizeLabelButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderLabelText.Value = random.Next(1000).ToString();
            //test.Message = "Two";
            events.Add(new());
        }


        //public class TestCl
        //{
        //    private string message = "";

        //    public string Message
        //    {
        //        get => message;
        //        set => message = value;
        //    }
        //}
    }
    public class CombatEvent : INotifyPropertyChanged
    {
        private string subEvent = "SPELL_DAMAGE";
        private string sourceName = "Teherach";
        private string spellName = "Chain Lightning";
        private int damage = 3642;

        public string SubEvent
        {
            get => subEvent;
            set
            {
                subEvent = value;
                OnPropertyChanged();
            }
        }
        public string SourceName
        {
            get => sourceName;
            set
            {
                sourceName = value;
                OnPropertyChanged();
            }
        }
        public string SpellName
        {
            get => spellName;
            set
            {
                spellName = value;
                OnPropertyChanged();
            }
        }
        public int Damage
        {
            get => damage;
            set
            {
                damage = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

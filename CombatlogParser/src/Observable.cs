using System.ComponentModel;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;

namespace CombatlogParser
{
    public class ObservableString : INotifyPropertyChanged 
    {
        private string value;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableString(string val)
        {
            value = val;
        }

        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

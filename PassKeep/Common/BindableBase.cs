using System.ComponentModel;
using System.Runtime.CompilerServices;
using Livet;

namespace PassKeep.Common {
    public class BindableBase : ViewModel, INotifyPropertyChanged {
        public new event PropertyChangedEventHandler PropertyChanged;

        protected override void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
            if (Equals(storage, value)) {
                return false;
            }
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
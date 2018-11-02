using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.Model
{
    public class Category : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private int _id;
        public int ID {
            get {
                return _id;
            }
            set {
                if (_id == value) return;

                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        private string _name;
        public string Name {
            get {
                return _name;
            }
            set {
                if (_name == value) return;

                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool _isEdit;
        public bool IsEdit {
            get {
                return _isEdit;
            }
            set {
                if (_isEdit == value) return;

                _isEdit = value;
                OnPropertyChanged(nameof(IsEdit));
            }
        }
    }
}

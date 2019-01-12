using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.Model {
    public class Account : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _title = string.Empty;
        public string Title {
            get {
                return _title;
            }
            set {
                if(_title == value)
                {
                    return;
                }
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _id = string.Empty;
        public string ID {
            get {
                return _id;
            }
            set {
                if (_id == value)
                {
                    return;
                }
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        private string _password = string.Empty;
        public string Password {
            get {
                return _password;
            }
            set {
                if (_password == value)
                {
                    return;
                }
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private Uri _url;
        public Uri URL {
            get {
                return _url;
            }
            set {
                if (_url == value)
                {
                    return;
                }
                _url = value;
                OnPropertyChanged(nameof(URL));
            }
        }

        private string _mail = string.Empty;
        public string Mail {
            get {
                return _mail;
            }
            set {
                if (_mail == value)
                {
                    return;
                }
                _mail = value;
                OnPropertyChanged(nameof(Mail));
            }
        }

        private int _categoryID = 0;
        public int CategoryID {
            get {
                return _categoryID;
            }
            set {
                if(_categoryID == value)
                {
                    return;
                }
                _categoryID = value;
                OnPropertyChanged(nameof(CategoryID));
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

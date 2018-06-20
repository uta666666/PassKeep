using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.Model {
    public class Account {
        private bool _hasChange;

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
                _hasChange = true;
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
                _hasChange = true;
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
                _hasChange = true;
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
                _hasChange = true;

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
                _hasChange = true;
            }
        }

        public bool HasChange()
        {
            return _hasChange;
        }

        public void ResetChange()
        {
            _hasChange = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Livet.Messaging;
using Microsoft.Win32;
using Newtonsoft.Json;
using PassKeep.Common;
using PassKeep.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace PassKeep.ViewModels {
    public class MainViewModel : BindableBase {
        public MainViewModel() {
            Password = Identity.Current;

            Bitmap = Properties.Resources.clipboard;
            Bitmap1 = Properties.Resources.browser;

            if (File.Exists("passkeep")) {
                var decryptStr = FileManager.ReadWithDecrypt("passkeep", Password);
                Accounts = JsonConvert.DeserializeObject<ObservableCollection<Account>>(decryptStr);
            } else {
                Accounts = new ObservableCollection<Account>();
            }

            AddCommand = new RelayCommand(() => {
                if (Accounts == null) {
                    Accounts = new ObservableCollection<Account>();
                }
                var newAccount = new Account();
                Accounts.Add(newAccount);

                CurrentAccount = newAccount;
            });

            DeleteCommand = new RelayCommand<Account>(a => {
                _accounts.Remove(a);
            });

            SaveCommand = new RelayCommand(() => {
                var jsonStr = JsonConvert.SerializeObject(Accounts);
                FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                MessageBox.Show("保存しました");
            });

            CopyToClipBoardCommand = new RelayCommand<string>((str) => {
                if (string.IsNullOrEmpty(str)) {
                    return;
                }
                Clipboard.SetText(str);
            });

            OpenBrowserCommand = new RelayCommand<Uri>((url) => {
                Process.Start(url.ToString());
            });

            CloseWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.CloseWindow(w);
            });

            //最小化
            MinimizeWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.MinimizeWindow(w);
            });

            //復元
            RestoreWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.RestoreWindow(w);
                VisibilityForMaximize = Visibility.Visible;
                VisibilityForRestore = Visibility.Collapsed;
            });

            //最大化
            MaximizeWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.MaximizeWindow(w);
                VisibilityForMaximize = Visibility.Collapsed;
                VisibilityForRestore = Visibility.Visible;
            });

            ShowChangePasswordCommand = new RelayCommand(() => {
                using(ChangePasswordViewModel vm = new ChangePasswordViewModel()) {
                    Messenger.Raise(new TransitionMessage(vm, "ChangePassword"));

                    if (vm.IsChanged) {
                        Password = Identity.Current;
                        var jsonStr = JsonConvert.SerializeObject(Accounts);
                        FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);
                    }
                }
            });
        }

        public ICommand ShowChangePasswordCommand { get; set; }

        public ICommand AddCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand CopyToClipBoardCommand { get; set; }

        public ICommand OpenBrowserCommand { get; set; }
        /// <summary>
        /// 最小化
        /// </summary>
        public ICommand MinimizeWindowCommand { get; set; }

        /// <summary>
        /// 復元
        /// </summary>
        public ICommand RestoreWindowCommand { get; set; }

        /// <summary>
        /// 最大化
        /// </summary>
        public ICommand MaximizeWindowCommand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ICommand CloseWindowCommand { get; set; }

        internal string Password { get; set; }

        private string _result;
        internal string Result {
            get {
                return _result;
            }
            set {
                SetProperty(ref _result, value);
                if (!string.IsNullOrEmpty(value)) {
                    Accounts = JsonConvert.DeserializeObject<ObservableCollection<Account>>(value);
                }
            }
        }

        private ObservableCollection<Account> _accounts;
        public ObservableCollection<Account> Accounts {
            get {
                return _accounts;
            }
            set {
                SetProperty(ref _accounts, value);
            }
        }

        private Account _currentAccount;
        public Account CurrentAccount {
            get {
                return _currentAccount;
            }
            set {
                SetProperty(ref _currentAccount, value);
            }
        }

        private Visibility _visibilityForMaximize = Visibility.Visible;
        /// <summary>
        /// 最大化ボタンの表示非表示
        /// </summary>
        public Visibility VisibilityForMaximize {
            get {
                return _visibilityForMaximize;
            }
            set {
                SetProperty(ref _visibilityForMaximize, value);
            }
        }

        private Visibility _visibilityForRestore = Visibility.Collapsed;
        /// <summary>
        /// 復元ボタンの表示非表示
        /// </summary>
        public Visibility VisibilityForRestore {
            get {
                return _visibilityForRestore;
            }
            set {
                SetProperty(ref _visibilityForRestore, value);
            }
        }

        private Bitmap _bmpSrc;
        public Bitmap Bitmap {
            get {
                return _bmpSrc;
            }
            set {
                SetProperty(ref _bmpSrc, value);
            }
        }

        private Bitmap _bmpSrc1;
        public Bitmap Bitmap1 {
            get {
                return _bmpSrc1;
            }
            set {
                SetProperty(ref _bmpSrc1, value);
            }
        }
    }
}

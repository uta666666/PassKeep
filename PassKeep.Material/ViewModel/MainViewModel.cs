using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Livet.Messaging;
using Microsoft.Win32;
using Newtonsoft.Json;
using PassKeep.Material.Common;
using PassKeep.Material.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using Reactive.Bindings;

namespace PassKeep.Material.ViewModel {
    public class MainViewModel : Livet.ViewModel {
        public MainViewModel() {
            //Property初期化
            InitializeProperty();

            Password = Identity.Current;
            
            if (File.Exists("passkeep")) {
                var decryptStr = FileManager.ReadWithDecrypt("passkeep", Password);
                Accounts.Value = JsonConvert.DeserializeObject<ObservableCollection<Account>>(decryptStr);
            } else {
                Accounts = new ReactiveProperty<ObservableCollection<Account>>(new ObservableCollection<Account>());
            }

            AddCommand = new ReactiveCommand();
            AddCommand.Subscribe(() => {
                if (Accounts == null) {
                    Accounts = new ReactiveProperty<ObservableCollection<Account>>(new ObservableCollection<Account>());
                }
                var newAccount = new Account();
                Accounts.Value.Add(newAccount);

                CurrentAccount.Value = newAccount;
            });

            DeleteCommand = new ReactiveCommand<Account>();
            DeleteCommand.Subscribe(a => {
                Accounts.Value.Remove(a);
            });

            SaveCommand = new ReactiveCommand();
            SaveCommand.Subscribe(() => {
                var jsonStr = JsonConvert.SerializeObject(Accounts.Value);
                FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                MessageBox.Show("保存しました");
            });

            CopyToClipBoardCommand = new ReactiveCommand<string>();
            CopyToClipBoardCommand.Subscribe((str) => {
                if (string.IsNullOrEmpty(str)) {
                    return;
                }
                Clipboard.SetText(str);
            });

            OpenBrowserCommand = new ReactiveCommand<Uri>();
            OpenBrowserCommand.Subscribe((url) => {
                Process.Start(url.ToString());
            });

            CloseWindowCommand = new ReactiveCommand<Window>();
            CloseWindowCommand.Subscribe((w) => {
                SystemCommands.CloseWindow(w);
            });

            //最小化
            MinimizeWindowCommand = new ReactiveCommand<Window>();
            MinimizeWindowCommand.Subscribe((w) => {
                SystemCommands.MinimizeWindow(w);
            });

            //復元
            RestoreWindowCommand = new ReactiveCommand<Window>();
            RestoreWindowCommand.Subscribe((w) => {
                SystemCommands.RestoreWindow(w);
                VisibilityForMaximize.Value = Visibility.Visible;
                VisibilityForRestore.Value = Visibility.Collapsed;
            });

            //最大化
            MaximizeWindowCommand = new ReactiveCommand<Window>();
            MaximizeWindowCommand.Subscribe((w) => {
                SystemCommands.MaximizeWindow(w);
                VisibilityForMaximize.Value = Visibility.Collapsed;
                VisibilityForRestore.Value = Visibility.Visible;
            });

            ShowChangePasswordCommand = new ReactiveCommand();
            ShowChangePasswordCommand.Subscribe(() => {
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

        private void InitializeProperty()
        {
            Accounts = new ReactiveProperty<ObservableCollection<Account>>();
            CurrentAccount = new ReactiveProperty<Account>();
            VisibilityForMaximize = new ReactiveProperty<Visibility>(Visibility.Visible);
            VisibilityForRestore = new ReactiveProperty<Visibility>(Visibility.Visible);
        }

        public ReactiveCommand ShowChangePasswordCommand { get; set; }

        public ReactiveCommand AddCommand { get; set; }

        public ReactiveCommand<Account> DeleteCommand { get; set; }

        public ReactiveCommand SaveCommand { get; set; }

        public ReactiveCommand<string> CopyToClipBoardCommand { get; set; }

        public ReactiveCommand<Uri> OpenBrowserCommand { get; set; }
        /// <summary>
        /// 最小化
        /// </summary>
        public ReactiveCommand<Window> MinimizeWindowCommand { get; set; }

        /// <summary>
        /// 復元
        /// </summary>
        public ReactiveCommand<Window> RestoreWindowCommand { get; set; }

        /// <summary>
        /// 最大化
        /// </summary>
        public ReactiveCommand<Window> MaximizeWindowCommand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand<Window> CloseWindowCommand { get; set; }

        internal string Password { get; set; }

        public ReactiveProperty<ObservableCollection<Account>> Accounts { get; set; }

        public ReactiveProperty<Account> CurrentAccount { get; set; }

        public ReactiveProperty<Visibility> VisibilityForMaximize { get; set; }

        public ReactiveProperty<Visibility> VisibilityForRestore { get; set; }
    }
}

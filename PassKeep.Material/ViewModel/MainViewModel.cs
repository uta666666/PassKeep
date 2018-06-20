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
using System.Text.RegularExpressions;
using MaterialDesignThemes.Wpf;
using System.Reactive.Linq;
using MahApps.Metro.Controls.Dialogs;

namespace PassKeep.Material.ViewModel
{
    public class MainViewModel : Livet.ViewModel
    {
        public MainViewModel()
        {
            //Property初期化
            InitializeProperty();

            new ThemeHelper().SetLightDark(IsDark.Value);

            Password = Identity.Current;

            if (File.Exists("passkeep"))
            {
                var decryptStr = FileManager.ReadWithDecrypt("passkeep", Password);
                Accounts.Value = JsonConvert.DeserializeObject<ObservableCollection<Account>>(decryptStr);
            }
            else
            {
                Accounts = new ReactiveProperty<ObservableCollection<Account>>(new ObservableCollection<Account>());
            }

            foreach (var a in Accounts.Value)
            {
                a.ResetChange();
            }

            //追加
            AddCommand = new ReactiveCommand();
            AddCommand.Subscribe(() =>
            {
                if (Accounts == null)
                {
                    Accounts = new ReactiveProperty<ObservableCollection<Account>>(new ObservableCollection<Account>());
                }
                var newAccount = new Account();
                Accounts.Value.Add(newAccount);

                CurrentAccount.Value = newAccount;
            });

            //削除
            DeleteCommand = new ReactiveCommand<Account>();
            DeleteCommand.Subscribe(a =>
            {
                Accounts.Value.Remove(a);
            });

            //保存
            SaveCommand = new ReactiveCommand();
            SaveCommand.Subscribe(() =>
            {
                var jsonStr = JsonConvert.SerializeObject(Accounts.Value);
                FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                foreach (var a in Accounts.Value)
                {
                    a.ResetChange();
                }

                Task.Factory.StartNew(() => MessageQueue.Enqueue("保存しました。"));
            });

            //クリップボードにコピー
            CopyToClipBoardCommand = new ReactiveCommand<string>();
            CopyToClipBoardCommand.Subscribe((str) =>
            {
                if (string.IsNullOrEmpty(str))
                {
                    return;
                }
                Clipboard.SetText(str);
            });

            //ブラウザで開く
            OpenBrowserCommand = new ReactiveCommand<Uri>();
            OpenBrowserCommand.Subscribe((url) =>
            {
                if (url == null)
                {
                    return;
                }
                Process.Start(url.ToString());
            });

            //パスワード変更画面を開く
            ShowChangePasswordCommand = new ReactiveCommand();
            ShowChangePasswordCommand.Subscribe(() =>
            {
                var vm = new ChangePasswordViewModel();
                DialogVM.Value = vm;
                IsDialogOpen.Value = true;
            });

            //Light←→Dark
            ChangeLightDarkCommand = new ReactiveCommand<bool>();
            ChangeLightDarkCommand.Subscribe(isDark =>
            {
                new ThemeHelper().SetLightDark(isDark);

                Properties.Settings.Default.IsDark = isDark;
                Properties.Settings.Default.Save();
            });

            //
            ShowDialogMahappsCommand = new ReactiveCommand();
            var s = new MetroDialogSettings()
            {
                AffirmativeButtonText = "はい",
                NegativeButtonText = "いいえ"
            };
            ShowDialogMahappsCommand.Subscribe(async w =>
            {
                var result = await MahAppsDialogCoordinator.ShowMessageAsync(this, "保存されていない変更があります。", "保存しますか？", MessageDialogStyle.AffirmativeAndNegative, s);
                if (result == MessageDialogResult.Affirmative)
                {
                    SaveCommand.Execute();
                }
                else
                {
                    foreach (var a in Accounts.Value)
                    {
                        a.ResetChange();
                    }
                }
                (w as MahApps.Metro.Controls.MetroWindow)?.Close();
            });


            //閉じる
            CloseWindowCommand = new ReactiveCommand<MahApps.Metro.Controls.MetroWindow>();
            CloseWindowCommand.Subscribe((w) =>
            {
                w.Close();
            });

            //最小化
            MinimizeWindowCommand = new ReactiveCommand<Window>();
            MinimizeWindowCommand.Subscribe((w) =>
            {
                SystemCommands.MinimizeWindow(w);
            });

            //復元
            RestoreWindowCommand = new ReactiveCommand<Window>();
            RestoreWindowCommand.Subscribe((w) =>
            {
                SystemCommands.RestoreWindow(w);
                VisibilityForMaximize.Value = Visibility.Visible;
                VisibilityForRestore.Value = Visibility.Collapsed;
            });

            //最大化
            MaximizeWindowCommand = new ReactiveCommand<Window>();
            MaximizeWindowCommand.Subscribe((w) =>
            {
                SystemCommands.MaximizeWindow(w);
                VisibilityForMaximize.Value = Visibility.Collapsed;
                VisibilityForRestore.Value = Visibility.Visible;
            });

            //ダイアログを閉じたとき
            IsDialogOpen.Subscribe(n =>
            {
                if (n) return;

                var vm = DialogVM.Value as ChangePasswordViewModel;
                if (vm != null)
                {
                    if (vm.IsChanged)
                    {
                        Password = Identity.Current;

                        var jsonStr = JsonConvert.SerializeObject(Accounts.Value);
                        FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                        Task.Factory.StartNew(() => MessageQueue.Enqueue("パスワードを変更しました。"));
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
            DialogVM = new ReactiveProperty<Livet.ViewModel>();
            IsDialogOpen = new ReactiveProperty<bool>(false);
            IsEditable = CurrentAccount.Select(n => n != null).ToReactiveProperty(); //Accounts.Select(n => (n?.Count ?? 0) > 0).ToReactiveProperty();
            IsDark = new ReactiveProperty<bool>(Properties.Settings.Default.IsDark);

            MessageQueue = new SnackbarMessageQueue();
        }


        /// <summary>
        /// パスワード変更
        /// </summary>
        public ReactiveCommand ShowChangePasswordCommand { get; set; }

        public ReactiveCommand AddCommand { get; set; }

        public ReactiveCommand<Account> DeleteCommand { get; set; }

        public ReactiveCommand SaveCommand { get; set; }

        public ReactiveCommand<string> CopyToClipBoardCommand { get; set; }

        public ReactiveCommand<Uri> OpenBrowserCommand { get; set; }

        public ReactiveCommand<bool> ChangeLightDarkCommand { get; set; }

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
        /// 閉じる
        /// </summary>
        public ReactiveCommand<MahApps.Metro.Controls.MetroWindow> CloseWindowCommand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand ShowDialogMahappsCommand { get; set; }


        internal string Password { get; set; }

        public ReactiveProperty<ObservableCollection<Account>> Accounts { get; set; }

        public ReactiveProperty<Account> CurrentAccount { get; set; }

        public ReactiveProperty<Visibility> VisibilityForMaximize { get; set; }

        public ReactiveProperty<Visibility> VisibilityForRestore { get; set; }

        public ReactiveProperty<Livet.ViewModel> DialogVM { get; set; }

        public ReactiveProperty<bool> IsDialogOpen { get; set; }

        public SnackbarMessageQueue MessageQueue { get; private set; }

        public ReactiveProperty<bool> IsEditable { get; set; }

        public ReactiveProperty<bool> IsDark { get; set; }

        public IDialogCoordinator MahAppsDialogCoordinator { get; set; }
    }
}

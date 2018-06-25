using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassKeep.Material.Common;
using PassKeep.Material.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using System.IO;
using Reactive.Bindings;
using MaterialDesignThemes.Wpf;
using System.Reactive.Linq;
using MahApps.Metro.Controls.Dialogs;
using System.Windows.Media;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PassKeep.Material.ViewModel
{
    public class MainViewModel : Livet.ViewModel
    {
        public MainViewModel()
        {
            //Property初期化
            InitializeProperty();            

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

                HasChanges.Value = true;
            });

            //保存
            SaveCommand = new ReactiveCommand();
            SaveCommand.Subscribe(() =>
            {
                var jsonStr = JsonConvert.SerializeObject(Accounts.Value);
                FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                HasChanges.Value = false;

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
                NegativeButtonText = "いいえ",
                FirstAuxiliaryButtonText = "キャンセル",
                DialogTitleFontSize = 20,
                DialogResultOnCancel = MessageDialogResult.FirstAuxiliary
            };
            ShowDialogMahappsCommand.Subscribe(async w =>
            {
                HasChanges.Value = false;

                var result = await MahAppsDialogCoordinator.ShowMessageAsync(this, "保存されていない変更があります。", "保存しますか？", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, s);
                if (result == MessageDialogResult.Affirmative)
                {
                    SaveCommand.Execute();
                }
                else if (result == MessageDialogResult.FirstAuxiliary)
                {
                    HasChanges.Value = true;
                    return;
                }
                (w as MahApps.Metro.Controls.MetroWindow)?.Close();
            });


            //閉じる
            CloseWindowCommand = new ReactiveCommand<MahApps.Metro.Controls.MetroWindow>();
            CloseWindowCommand.Subscribe((w) =>
            {
                IsLogout = false;
                w.Close();
            });

            //ログアウト
            LogoutCommand = new ReactiveCommand<MahApps.Metro.Controls.MetroWindow>();
            LogoutCommand.Subscribe((w) =>
            {
                IsLogout = true;
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
            HasChanges = new ReactiveProperty<bool>(false);
            TitleBrush = new ReactiveProperty<SolidColorBrush>();

            //変更がある場合は文字の色を変える
            HasChanges.Subscribe(n =>
            {
                if (n)
                {
                    TitleBrush.Value = (SolidColorBrush)Application.Current.Resources["SecondaryAccentBrush"];
                }
                else
                {
                    TitleBrush.Value = (SolidColorBrush)Application.Current.Resources["PrimaryHueDarkForegroundBrush"];
                }
            });

            MessageQueue = new SnackbarMessageQueue();

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
            //各要素に対してイベントハンドラをセット
            foreach (var ac in Accounts.Value)
            {
                ac.PropertyChanged += Account_PropertyChanged;
            }
            //リストに対してイベントハンドラをセット
            Accounts.Value.CollectionChanged += Accounts_CollectionChanged;
        }

        private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Account item in e.NewItems)
                {
                    item.PropertyChanged += Account_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Account item in e.OldItems)
                {
                    item.PropertyChanged -= Account_PropertyChanged;
                }
            }
        }

        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HasChanges.Value = true;
        }

        /// <summary>
        /// パスワード変更
        /// </summary>
        public ReactiveCommand ShowChangePasswordCommand { get; set; }

        /// <summary>
        /// 追加
        /// </summary>
        public ReactiveCommand AddCommand { get; set; }

        /// <summary>
        /// 削除
        /// </summary>
        public ReactiveCommand<Account> DeleteCommand { get; set; }

        /// <summary>
        /// 保存
        /// </summary>
        public ReactiveCommand SaveCommand { get; set; }

        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        public ReactiveCommand<string> CopyToClipBoardCommand { get; set; }

        /// <summary>
        /// ブラウザで開く
        /// </summary>
        public ReactiveCommand<Uri> OpenBrowserCommand { get; set; }

        /// <summary>
        /// テーマ変更
        /// </summary>
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
        /// Logout
        /// </summary>
        public ReactiveCommand<MahApps.Metro.Controls.MetroWindow> LogoutCommand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand ShowDialogMahappsCommand { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
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

        public ReactiveProperty<bool> HasChanges { get; set; }

        public ReactiveProperty<SolidColorBrush> TitleBrush { get; set; }

        public bool IsLogout { get; set; }
    }
}

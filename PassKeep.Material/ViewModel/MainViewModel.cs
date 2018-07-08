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
using System.Collections;
using System.Collections.Generic;

namespace PassKeep.Material.ViewModel
{
    public class MainViewModel : Livet.ViewModel, INotifyDataErrorInfo
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
                    Accounts = new ReactiveProperty<AccountList>(new AccountList());
                }
                var newAccount = new Account() { Title = "New Item", CategoryID = CurrentCategory.Value.ID };
                Accounts.Value.Add(newAccount);

                CurrentAccount.Value = newAccount;
                IsTitleFocused.Value = true;
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

                var catJsonStr = JsonConvert.SerializeObject(Categories.Value.Where(n => n.ID != 0));
                FileManager.WriteWithEncrypt("categories", Password, catJsonStr);

                Accounts.Value.CopyTo(_accountsBackup);
                Categories.Value.CopyTo(_categoriesBackup);
                HasChanges.Value = false;

                Task.Factory.StartNew(() => MessageQueue.Enqueue("保存しました。"));
            });

            //クリップボードにコピー
            CopyToClipBoardCommand = new ReactiveCommand<string>();
            CopyToClipBoardCommand.Subscribe((str) =>
            {
                if (string.IsNullOrEmpty(str)) return;

                Clipboard.SetText(str);
            });

            //ブラウザで開く
            OpenBrowserCommand = new ReactiveCommand<Uri>();
            OpenBrowserCommand.Subscribe((url) =>
            {
                if (url == null) return;

                Process.Start(url.ToString());
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

                if (IsLogout) return;

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
            LogoutCommand.Subscribe(async (w) =>
            {
                if (HasChanges.Value)
                {
                    var result = await MahAppsDialogCoordinator.ShowMessageAsync(this, "保存されていない変更があります。", "保存しますか？", MessageDialogStyle.AffirmativeAndNegative, s);
                    if (result == MessageDialogResult.Affirmative)
                    {
                        SaveCommand.Execute();
                    }
                    else
                    {
                        _accountsBackup.CopyTo(Accounts.Value);
                        _categoriesBackup.CopyTo(Categories.Value);
                    }
                    HasChanges.Value = false;
                }
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

            //パスワード変更画面を開く
            ShowChangePasswordCommand = new ReactiveCommand();
            ShowChangePasswordCommand.Subscribe(async () =>
            {
                var vm = new ChangePasswordViewModel();
                var result = await DialogHost.Show(vm);
                if ((result as bool?) ?? false)
                {
                    Password = Identity.Current;

                    var jsonStr = JsonConvert.SerializeObject(Accounts.Value);
                    FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                    await Task.Factory.StartNew(() => MessageQueue.Enqueue("パスワードを変更しました。"));
                }
            });

            //カテゴリ追加
            AddCategoryCommand = new ReactiveCommand();
            AddCategoryCommand.Subscribe(async () =>
            {
                var maxId = Categories.Value.Max(n => n.ID);
                var categoryVm = new CategoryViewModel() { CategoryName = new ReactiveProperty<string>($"category{maxId + 1}") };

                var result = await DialogHost.Show(categoryVm);
                if ((result as bool?) ?? false)
                {
                    var category = new Category() { ID = maxId + 1, Name = categoryVm.CategoryName.Value };
                    Categories.Value.Add(category);

                    CurrentCategory.Value = category;
                }
                IsDispSidePanel.Value = false;
            });

            ChangeCategoryNameCommand = new ReactiveCommand<Category>();
            ChangeCategoryNameCommand.Subscribe(async n =>
            {
                var categoryVm = new CategoryViewModel() { CategoryName = new ReactiveProperty<string>(n.Name) };

                var result = await DialogHost.Show(categoryVm);
                if ((result as bool?) ?? false)
                {
                    n.Name = categoryVm.CategoryName.Value;
                }
                var a = Categories.Value;
            });

            DeleteCategoryCommand = new ReactiveCommand<Category>();
            DeleteCategoryCommand.Subscribe(async n =>
            {
                if (n.ID == 0)
                {
                    await MahAppsDialogCoordinator.ShowMessageAsync(this, "\"ALL\"は削除できません。", "", MessageDialogStyle.Affirmative, s);
                    return;
                }

                var result = await MahAppsDialogCoordinator.ShowMessageAsync(this, "カテゴリ内のアカウントも削除しますか？", "削除しない場合はALLに表示されます。", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, s);
                if (result == MessageDialogResult.FirstAuxiliary) return;

                if (result == MessageDialogResult.Affirmative)
                {
                    for (int i = Accounts.Value.Count - 1; i >= 0; i--)
                    {
                        if (Accounts.Value[i].CategoryID == n.ID)
                        {
                            Accounts.Value.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    foreach (var a in Accounts.Value.Where(a => a.CategoryID == n.ID))
                    {
                        a.CategoryID = 0;
                    }
                }
                Categories.Value.Remove(n);
                CurrentCategory.Value = Categories.Value.First();
            });
        }

        /// <summary>
        /// プロパティ初期化
        /// </summary>
        private void InitializeProperty()
        {
            CurrentAccount = new ReactiveProperty<Account>();
            VisibilityForMaximize = new ReactiveProperty<Visibility>(Visibility.Visible);
            VisibilityForRestore = new ReactiveProperty<Visibility>(Visibility.Visible);
            DialogVM = new ReactiveProperty<Livet.ViewModel>();
            IsDialogOpen = new ReactiveProperty<bool>(false);
            IsEditable = CurrentAccount.Select(n => n != null).ToReactiveProperty(); //Accounts.Select(n => (n?.Count ?? 0) > 0).ToReactiveProperty();
            IsDark = new ReactiveProperty<bool>(Properties.Settings.Default.IsDark);
            HasChanges = new ReactiveProperty<bool>(false);
            TitleBrush = new ReactiveProperty<SolidColorBrush>();
            MessageQueue = new SnackbarMessageQueue();
            IsTitleFocused = new ReactiveProperty<bool>();
            IsDispSidePanel = new ReactiveProperty<bool>(false);
            CategoryViewModel = new ReactiveProperty<CategoryViewModel>();
            CategoryContextMenuEnabled = new ReactiveProperty<bool>(true);

            //ドラッグでの並べ替え対応
            Description = CreateDragAcceptDescription();

            //設定読み込みとか
            Password = Identity.Current;
            Accounts = new ReactiveProperty<AccountList>(CreateAccountList());

            Categories = new ReactiveProperty<CategoryList>(CreateCategoryList());
            CurrentCategory = new ReactiveProperty<Category>(Categories.Value[0]);

            //プロパティの変更を購読する
            Subscribe();
        }

        /// <summary>
        /// プロパティをサブスクライブ
        /// </summary>
        private void Subscribe()
        {
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

            //カテゴリが変わったら
            CurrentCategory.Subscribe(c =>
            {
                //IsDispSidePanel.Value = false;
                if (c == null) return;

                var collectionView = System.Windows.Data.CollectionViewSource.GetDefaultView(Accounts.Value);
                collectionView.Filter = x =>
                {
                    if (c.ID == 0) return true;

                    var account = (Account)x;
                    return account.CategoryID == c.ID;
                };

                CurrentAccount.Value = Accounts.Value.Where(n => (c.ID == 0 || n.CategoryID == c.ID)).FirstOrDefault();
            });
        }

        /// <summary>
        /// ListBoxの並び替え用コントロール作成
        /// </summary>
        /// <returns></returns>
        private DragAcceptDescription CreateDragAcceptDescription()
        {
            var description = new DragAcceptDescription();
            description.DragOver += (e) =>
            {
                if (e.AllowedEffects.HasFlag(e.Effects))
                {
                    if (e.Data.GetDataPresent(typeof(Account)))
                    {
                        return;
                    }
                }
                e.Effects = DragDropEffects.None;
            };
            description.DragDrop += (e) =>
            {
                var oldIndex = Accounts.Value.Select((data, index) => new { Index = index, Data = data }).Where(n => n.Data == e.MovingData).Select(n => n.Index).First();
                Accounts.Value.Move(oldIndex, e.NewIndex);
            };
            return description;
        }

        /// <summary>
        /// データ読み込み
        /// </summary>
        /// <returns></returns>
        private AccountList CreateAccountList()
        {
            var accounts = new AccountList();
            if (File.Exists("passkeep"))
            {
                var decryptStr = FileManager.ReadWithDecrypt("passkeep", Password);
                accounts = JsonConvert.DeserializeObject<AccountList>(decryptStr);
                accounts.CopyTo(_accountsBackup);
            }
            //各要素に対してイベントハンドラをセット
            foreach (var ac in accounts)
            {
                ac.PropertyChanged += Account_PropertyChanged;
            }
            //リストに対してイベントハンドラをセット
            accounts.CollectionChanged += Accounts_CollectionChanged;
            return accounts;
        }

        /// <summary>
        /// データ読み込み（カテゴリ）
        /// </summary>
        /// <returns></returns>
        private CategoryList CreateCategoryList()
        {
            var categories = new CategoryList();
            if (File.Exists("categories"))
            {
                var decryptCatStr = FileManager.ReadWithDecrypt("categories", Password);
                categories = JsonConvert.DeserializeObject<CategoryList>(decryptCatStr);
                categories.CopyTo(_categoriesBackup);
            }
            foreach(var cat in categories)
            {
                cat.PropertyChanged += Cat_PropertyChanged;
            }
            categories.CollectionChanged += Categories_CollectionChanged;
            return categories;
        }

        /// <summary>
        /// リストに変更があったとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Account item in e.NewItems)
                {
                    item.PropertyChanged += Account_PropertyChanged;
                }
                HasChanges.Value = true;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Account item in e.OldItems)
                {
                    item.PropertyChanged -= Account_PropertyChanged;
                }
                HasChanges.Value = true;
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                HasChanges.Value = true;
            }
        }

        /// <summary>
        /// 各データに変更があったとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Account_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HasChanges.Value = true;
        }

        /// <summary>
        /// リストに変更があったとき カテゴリ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Categories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                HasChanges.Value = true;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                HasChanges.Value = true;
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                HasChanges.Value = true;
            }
        }

        private void Cat_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HasChanges.Value = true;
        }

        private AccountList _accountsBackup = new AccountList();
        private CategoryList _categoriesBackup = new CategoryList();

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
        /// カテゴリーを追加する
        /// </summary>
        public ReactiveCommand AddCategoryCommand { get; set; }

        public ReactiveCommand<Category> ChangeCategoryNameCommand { get; set; }

        public ReactiveCommand<Category> DeleteCategoryCommand { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        internal string Password { get; set; }

        /// <summary>
        /// アカウントデータ
        /// </summary>
        public ReactiveProperty<AccountList> Accounts { get; set; }

        /// <summary>
        /// 選択中のアカウント
        /// </summary>
        public ReactiveProperty<Account> CurrentAccount { get; set; }

        /// <summary>
        /// カテゴリ
        /// </summary>
        public ReactiveProperty<CategoryList> Categories { get; set; }

        /// <summary>
        /// 選択中のカテゴリ
        /// </summary>
        public ReactiveProperty<Category> CurrentCategory { get; set; }

        public ReactiveProperty<Visibility> VisibilityForMaximize { get; set; }

        public ReactiveProperty<Visibility> VisibilityForRestore { get; set; }

        public ReactiveProperty<Livet.ViewModel> DialogVM { get; set; }

        public ReactiveProperty<bool> IsDialogOpen { get; set; }

        public SnackbarMessageQueue MessageQueue { get; private set; }

        public ReactiveProperty<bool> IsEditable { get; set; }

        public ReactiveProperty<bool> IsDark { get; set; }

        public ReactiveProperty<bool> HasChanges { get; set; }

        public ReactiveProperty<SolidColorBrush> TitleBrush { get; set; }
        /// <summary>
        /// TitleTextBoxにフォーカスをあてる
        /// </summary>
        public ReactiveProperty<bool> IsTitleFocused { get; set; }
        /// <summary>
        /// サイドパネルを表示する（ハンバーガーボタン）
        /// </summary>
        public ReactiveProperty<bool> IsDispSidePanel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReactiveProperty<CategoryViewModel> CategoryViewModel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ReactiveProperty<bool> CategoryContextMenuEnabled { get; set; }

        /// <summary>
        /// ログアウト
        /// </summary>
        public bool IsLogout { get; set; }

        /// <summary>
        /// MahAPpsのダイアログ
        /// </summary>
        public IDialogCoordinator MahAppsDialogCoordinator { get; set; }

        /// <summary>
        /// ドラッグアンドドロップでの並び替え用
        /// </summary>
        public DragAcceptDescription Description { get; set; }



        private readonly Dictionary<string, string> _currentErrors = new Dictionary<string, string>();

        private void AddError(string propertyName, string error)
        {
            if (!_currentErrors.ContainsKey(propertyName))
            {
                _currentErrors[propertyName] = error;
            }
            OnErrorsChanged(propertyName);
        }

        private void RemoveError(string propertyName)
        {
            if (_currentErrors.ContainsKey(propertyName))
            {
                _currentErrors.Remove(propertyName);
            }
            OnErrorsChanged(propertyName);
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
               !_currentErrors.ContainsKey(propertyName))
            {
                return null;
            }
            return _currentErrors[propertyName];
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => _currentErrors.Count > 0;
    }
}

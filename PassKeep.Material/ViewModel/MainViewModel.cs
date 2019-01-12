using MahApps.Metro.Controls.Dialogs;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using PassKeep.Material.Common;
using PassKeep.Material.Model;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PassKeep.Material.ViewModel
{
    public class MainViewModel : Livet.ViewModel, INotifyDataErrorInfo
    {
        public MainViewModel()
        {
            //Property初期化
            InitializeProperty();

            //MahAppDialog設定
            var s = new MetroDialogSettings()
            {
                AffirmativeButtonText = "はい",
                NegativeButtonText = "いいえ",
                FirstAuxiliaryButtonText = "キャンセル",
                DialogTitleFontSize = 20,
                DialogResultOnCancel = MessageDialogResult.FirstAuxiliary
            };

            //追加
            AddCommand = new ReactiveCommand();
            AddCommand.Subscribe(() =>
            {
                if (Accounts == null)
                {
                    Accounts = new ReactiveProperty<AccountList>(new AccountList());
                }
                var newAccount = new Account() { Title = "New Item", CategoryID = CurrentCategory.Value.ID, IsEdit = true };
                Accounts.Value.Add(newAccount);

                CurrentAccount.Value = newAccount;
                IsTitleFocused.Value = true;
            });

            //修正
            EditCommand = new ReactiveCommand<Account>();
            EditCommand.Subscribe(a =>
            {
                //a.IsEdit = true;
                IsEditable.Value = true;
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
                IsEditable.Value = false;

                foreach (var c in Accounts.Value)
                {
                    c.IsEdit = false;
                }
                //var jsonStr = JsonConvert.SerializeObject(Accounts.Value);
                //FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                int order = 1;
                foreach (var c in Categories.Value)
                {
                    c.IsEdit = false;
                    c.Order = order++;
                }
                //var catJsonStr = JsonConvert.SerializeObject(Categories.Value);
                //FileManager.WriteWithEncrypt("categories", Password, catJsonStr);

                var jsonStr = JsonConvert.SerializeObject(new PassKeepData { Accounts = Accounts.Value, Categories = Categories.Value });
                FileManager.WriteWithEncrypt("passkeep", Password, jsonStr);

                Accounts.Value.CopyTo(_accountsBackup);
                Categories.Value.CopyTo(_categoriesBackup);
                HasChanges.Value = false;

                CollectionViewSource.GetDefaultView(Accounts.Value).Refresh();
                Task.Factory.StartNew(() => MessageQueue.Enqueue("保存しました。"));
            });

            //キャンセル
            CancelCommand = new ReactiveCommand();
            CancelCommand.Subscribe(() =>
            {
                IsEditable.Value = false;
                var currentIndex = Accounts.Value.Select((data, index) => new { data, index }).FirstOrDefault(n => n.data == CurrentAccount?.Value)?.index ?? -1;
                _accountsBackup.CopyTo(Accounts.Value);

                CurrentAccount.Value = Accounts.Value.Where((data, index) => index == currentIndex).FirstOrDefault() ?? Accounts.Value.First();
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
            ShowDialogMahappsCommand.Subscribe((w) =>
            {
                HasChanges.Value = false;

                if (IsLogout) return;

                //var result = await MahAppsDialogCoordinator.ShowMessageAsync(this, "保存されていない変更があります。", "保存しますか？", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, s);
                //if (result == MessageDialogResult.Affirmative)
                //{
                //    SaveCommand.Execute();
                //}
                //else if (result == MessageDialogResult.FirstAuxiliary)
                //{
                //    HasChanges.Value = true;
                //    return;
                //}
                SaveCommand.Execute();

                //(w as MahApps.Metro.Controls.MetroWindow)?.Close();
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
                if (HasChanges.Value)
                {
                    //var result = await MahAppsDialogCoordinator.ShowMessageAsync(this, "保存されていない変更があります。", "保存しますか？", MessageDialogStyle.AffirmativeAndNegative, s);
                    //if (result == MessageDialogResult.Affirmative)
                    //{
                    //    SaveCommand.Execute();
                    //}
                    //else
                    //{
                    //    _accountsBackup.CopyTo(Accounts.Value);
                    //    _categoriesBackup.CopyTo(Categories.Value);
                    //}
                    SaveCommand.Execute();
                    HasChanges.Value = false;
                }
                else
                {
                    CancelCommand.Execute();
                }
                IsDispSidePanel.Value = false;

                IsLogout = true;
                w.Close();
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

                    var catJsonStr = JsonConvert.SerializeObject(Categories.Value);
                    FileManager.WriteWithEncrypt("categories", Password, catJsonStr);

                    await Task.Factory.StartNew(() => MessageQueue.Enqueue("パスワードを変更しました。"));
                }
            });

            //カテゴリ追加
            AddCategoryCommand = new ReactiveCommand();
            AddCategoryCommand.Subscribe(() =>
            {
                //確認モード→追加モード
                var maxId = Categories.Value.Max(n => n.ID);
                var category = new Category() { ID = maxId + 1, Name = $"category{maxId + 1}", Name2 = $"category{maxId + 1}", IsEdit = true };
                Categories.Value.Add(category);
                CurrentCategory.Value = category;

                CategoryButtonText.Value = "OK";
                CategoryEditMode.Value = (int)EditMode.Add;

                //TODO
                CategoriesForAccount.Value.Add(category);
            });

            //カテゴリ保存
            SaveCategoryCommand = new ReactiveCommand();
            SaveCategoryCommand.Subscribe(() =>
            {
                //追加モード→確認モード
                //修正モード→確認モード
                //TODO
                if ((EditMode)CategoryEditMode.Value == EditMode.Modify)
                {
                    var cat = Categories.Value.First(n => n.IsEdit);
                    var destRow = CategoriesForAccount.Value.FirstOrDefault(c => c.ID == cat.ID);
                    if (destRow != null)
                    {
                        destRow.Name = cat.Name;
                    }
                }

                foreach (var cat in Categories.Value)
                {
                    cat.IsEdit = false;
                }

                CategoryButtonText.Value = "Add Category";
                CategoryEditMode.Value = (int)EditMode.Confirm;

                SaveCommand.Execute();

                HasChanges.Value = false;
            });

            //カテゴリ名変更
            ChangeCategoryNameCommand = new ReactiveCommand<Category>();
            ChangeCategoryNameCommand.Subscribe(n =>
            {
                n.IsEdit = true;
                CurrentCategory.Value = n;

                CategoryButtonText.Value = "OK";
                CategoryEditMode.Value = (int)EditMode.Modify;
            });

            //カテゴリ削除
            DeleteCategoryCommand = new ReactiveCommand<Category>();
            DeleteCategoryCommand.Subscribe(async n =>
            {
                if (n.ID == 0)
                {
                    await MahAppsDialogCoordinator.ShowMessageAsync(this, "\"ALL\"は削除できません。", "", MessageDialogStyle.Affirmative, s);
                    return;
                }

                MessageDialogResult result = MessageDialogResult.Canceled;
                if (Accounts.Value.Any(a => a.CategoryID == n.ID))
                {
                    result = await MahAppsDialogCoordinator.ShowMessageAsync(this, "カテゴリ内のアカウントも削除しますか？", "削除しない場合はALLに表示されます。", MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, s);
                }
                if (result == MessageDialogResult.FirstAuxiliary) return;

                if (result == MessageDialogResult.Affirmative)
                {
                    foreach (var act in Accounts.Value.Where(a => a.CategoryID == n.ID))
                    {
                        Accounts.Value.Remove(act);
                    }
                }
                else if (result == MessageDialogResult.Negative)
                {
                    foreach (var a in Accounts.Value.Where(a => a.CategoryID == n.ID))
                    {
                        a.CategoryID = 0;
                    }
                }
                Categories.Value.Remove(n);
                CurrentCategory.Value = Categories.Value.First();

                HasChanges.Value = false;
                await Task.Factory.StartNew(() => MessageQueue.Enqueue("削除しました。"));
            });

            CancelEditCategoryCommand = new ReactiveCommand();
            CancelEditCategoryCommand.Subscribe(() =>
            {
                IsDispSidePanel.Value = false;
            });
        }

        /// <summary>
        /// プロパティ初期化
        /// </summary>
        private void InitializeProperty()
        {
            CurrentAccount = new ReactiveProperty<Account>();
            DialogVM = new ReactiveProperty<Livet.ViewModel>();
            IsDialogOpen = new ReactiveProperty<bool>(false);
            IsEditable = CurrentAccount.Select(n => n != null && n.IsEdit).ToReactiveProperty(); //Accounts.Select(n => (n?.Count ?? 0) > 0).ToReactiveProperty();
            IsDark = new ReactiveProperty<bool>(Properties.Settings.Default.IsDark);
            HasChanges = new ReactiveProperty<bool>(false);
            TitleBrush = new ReactiveProperty<SolidColorBrush>();
            MessageQueue = new SnackbarMessageQueue();
            IsTitleFocused = new ReactiveProperty<bool>();
            IsDispSidePanel = new ReactiveProperty<bool>(false);
            CategoryViewModel = new ReactiveProperty<CategoryViewModel>();
            CategoryContextMenuEnabled = new ReactiveProperty<bool>(true);
            CategoryButtonText = new ReactiveProperty<string>("Add Category");
            CategoryEditMode = new ReactiveProperty<int>((int)EditMode.Confirm);

            //ドラッグでの並べ替え対応
            AccountsDescription = CreateAccountDragAcceptDescription();

            CategoriesDescription = CreateCategoryDragAcceptDescription();

            //設定読み込みとか
            Password = Identity.Current;
            Accounts = new ReactiveProperty<AccountList>(CreateAccountList());

            Categories = new ReactiveProperty<CategoryList>(CreateCategoryList());
            CurrentCategory = new ReactiveProperty<Category>(Categories.Value[0]);

            CategoriesForAccount = new ReactiveProperty<CategoryList>(new CategoryList(Categories.Value.OrderByDescending(n => n.ID))); //default(ReactiveCollection<Category>);
            //CategoriesForAccount = Categories.Value.ToObservable().ToReactiveCollection();
            var collectionView = CollectionViewSource.GetDefaultView(CategoriesForAccount.Value);
            //collectionView.SortDescriptions.Clear();
            //collectionView.SortDescriptions.Add(new SortDescription("ID", ListSortDirection.Ascending));
            collectionView.Filter = x => ((Category)x).ID != 0;

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

            //カテゴリパネルがとじたとき
            IsDispSidePanel.Subscribe(n =>
            {
                if (n) return;

                if ((EditMode)CategoryEditMode.Value == EditMode.Add)
                {
                    //変更中のものは破棄する
                    var targets = Categories.Value.Where(c => c.IsEdit).ToList();
                    for (int i = targets.Count() - 1; i >= 0; i--)
                    {
                        var target = targets[i];
                        Categories.Value.Remove(target);
                    }
                    CategoryButtonText.Value = "Add Category";
                    CategoryEditMode.Value = (int)EditMode.Confirm;

                    CurrentCategory.Value = Categories.Value.First();
                }
                else if ((EditMode)CategoryEditMode.Value == EditMode.Modify)
                {
                    foreach (var c in Categories.Value.Where(c => c.IsEdit))
                    {
                        c.IsEdit = false;
                    }
                    CategoryButtonText.Value = "Add Category";
                    CategoryEditMode.Value = (int)EditMode.Confirm;
                }
            });
        }

        /// <summary>
        /// ListBoxの並び替え用コントロール作成
        /// </summary>
        /// <returns></returns>
        private DragAcceptDescription<Account> CreateAccountDragAcceptDescription()
        {
            var description = new DragAcceptDescription<Account>();
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

        private DragAcceptDescription<Category> CreateCategoryDragAcceptDescription()
        {
            var description = new DragAcceptDescription<Category>();
            description.DragOver += (e) =>
            {
                if (e.AllowedEffects.HasFlag(e.Effects))
                {
                    if (e.Data.GetDataPresent(typeof(Category)))
                    {
                        return;
                    }
                }
                e.Effects = DragDropEffects.None;
            };
            description.DragDrop += (e) =>
            {
                var oldIndex = Categories.Value.Select((data, index) => new { Index = index, Data = data }).Where(n => n.Data == e.MovingData).Select(n => n.Index).First();
                Categories.Value.Move(oldIndex, e.NewIndex);
            };
            return description;
        }

        /// <summary>
        /// データ読み込み
        /// </summary>
        /// <returns></returns>
        private AccountList CreateAccountList()
        {
            const string FILENAME = "passkeep";
            var accounts = new AccountList();
            if (File.Exists(FILENAME))
            {
                var decryptStr = FileManager.ReadWithDecrypt(FILENAME, Password);
                try
                {
                    accounts = AccountList.MakeList(decryptStr);
                }
                catch
                {
                    var data = PassKeepData.MakeData(decryptStr);
                    accounts = data.Accounts;
                }
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
            const string FILENAME = "categories";
            CategoryList categories = null;
            if (File.Exists(FILENAME))
            {
                var decryptCatStr = FileManager.ReadWithDecrypt(FILENAME, Password);
                try
                {
                    categories = CategoryList.MakeList(decryptCatStr);
                }
                catch
                {
                    var data = PassKeepData.MakeData(decryptCatStr);
                    categories = data.Categories;
                }
            }
            else
            {
                categories = CategoryList.MakeList();
            }
            categories.CopyTo(_categoriesBackup);

            foreach (var cat in categories)
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
        /// 修正
        /// </summary>
        public ReactiveCommand<Account> EditCommand { get; set; }
        /// <summary>
        /// 保存
        /// </summary>
        public ReactiveCommand SaveCommand { get; set; }
        /// <summary>
        /// キャンセル
        /// </summary>
        public ReactiveCommand CancelCommand { get; set; }
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
        /// <summary>
        /// カテゴリ保存
        /// </summary>
        public ReactiveCommand SaveCategoryCommand { get; set; }
        /// <summary>
        /// カテゴリ名変更
        /// </summary>
        public ReactiveCommand<Category> ChangeCategoryNameCommand { get; set; }
        /// <summary>
        /// カテゴリ削除
        /// </summary>
        public ReactiveCommand<Category> DeleteCategoryCommand { get; set; }
        /// <summary>
        /// カテゴリ編集キャンセル
        /// </summary>
        public ReactiveCommand CancelEditCategoryCommand { get; set; }


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

        public ReactiveProperty<CategoryList> CategoriesForAccount { get; set; }
        //public ReactiveCollection<Category> CategoriesForAccount { get; set; }
        //public ReadOnlyReactiveCollection<Category> CategoriesForAccount { get; set; }

        /// <summary>
        /// 選択中のカテゴリ
        /// </summary>
        public ReactiveProperty<Category> CurrentCategory { get; set; }

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

        public ReactiveProperty<int> CategoryEditMode { get; set; }

        public ReactiveProperty<string> CategoryButtonText { get; set; }

        /// <summary>
        /// ログアウト
        /// </summary>
        public bool IsLogout { get; set; }

        /// <summary>
        /// MahAPpsのダイアログ
        /// </summary>
        public IDialogCoordinator MahAppsDialogCoordinator { get; set; }

        /// <summary>
        /// ドラッグアンドドロップでの並び替え用 Account
        /// </summary>
        public DragAcceptDescription<Account> AccountsDescription { get; set; }

        /// <summary>
        /// ドラッグアンドドロップでの並び替え用　Category
        /// </summary>
        public DragAcceptDescription<Category> CategoriesDescription { get; set; }



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

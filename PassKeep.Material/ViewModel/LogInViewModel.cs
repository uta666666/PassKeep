using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PassKeep.Material.Common;
using System.Threading;
using Reactive.Bindings;
using System.ComponentModel;
using System.Collections;

namespace PassKeep.Material.ViewModel {
    public class LogInViewModel : Livet.ViewModel, INotifyDataErrorInfo
    {
        public LogInViewModel() {
            InitializeProperty();

            var isDark = Properties.Settings.Default.IsDark;
            new ThemeHelper().SetLightDark(isDark);

            if (!File.Exists("passkeep")) {
                ConfirmPasswordVisibility.Value = Visibility.Visible;
                Message.Value = "パスワードを登録してください" + Environment.NewLine + "パスワードを忘れると復元不可能です";
            } else {
                ConfirmPasswordVisibility.Value = Visibility.Hidden;
                Message.Value = string.Empty;
            }

            LogInCommand = new ReactiveCommand<Window>();
            LogInCommand.Subscribe((w) => {
                if (string.IsNullOrEmpty(Password)) {
                    return;
                }

                //ファイルがないときは初回と判断し、パスワードに関係なくログインさせる
                if (!File.Exists("passkeep")) {
                    if (Password == ConfirmPassword) {
                        Identity.Current = Password;
                        w.DialogResult = true;
                        RemoveError(nameof(ConfirmPassword));
                    } else {
                        AddError(nameof(ConfirmPassword), "パスワードが一致しません");
                    }
                    return;
                }

                var result = FileManager.ReadWithDecrypt("passkeep", Password);
                if (!string.IsNullOrEmpty(result)) {
                    Identity.Current = Password;
                    w.DialogResult = true;
                    RemoveError(nameof(Password));
                } else {
                    //Message.Value = "パスワードが違います";
                    AddError(nameof(Password), "パスワードが違います");
                }
            });

            CloseWindowCommand = new ReactiveCommand<Window>();
            CloseWindowCommand.Subscribe((w) => {
                SystemCommands.CloseWindow(w);
            });

            MinimizeWindowCommand = new ReactiveCommand<Window>();
            MinimizeWindowCommand.Subscribe((w) => {
                SystemCommands.MinimizeWindow(w);
            });
        }

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


        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

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

        public bool HasErrors => _currentErrors.Count > 0;


        private void InitializeProperty()
        {
            ConfirmPasswordVisibility = new ReactiveProperty<Visibility>(Visibility.Hidden);
            Message = new ReactiveProperty<string>();
        }

        public ReactiveCommand<Window> LogInCommand { get; set; }

        public ReactiveCommand<Window> MinimizeWindowCommand { get; set; }

        public ReactiveCommand<Window> CloseWindowCommand { get; set; }


        public string Password { get; set; }

        public string ConfirmPassword { get; set; }


        public ReactiveProperty<string> Message { get; set; }

        public ReactiveProperty<Visibility> ConfirmPasswordVisibility { get; set; }
    }
}

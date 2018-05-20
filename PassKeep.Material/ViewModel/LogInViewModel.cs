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
    public class LogInViewModel : Livet.ViewModel
    {
        public LogInViewModel() {
            InitializeProperty();

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
                    }
                    return;
                }

                var result = FileManager.ReadWithDecrypt("passkeep", Password);
                if (!string.IsNullOrEmpty(result)) {
                    Identity.Current = Password;
                    w.DialogResult = true;
                } else {
                    Message.Value = "パスワードが違います";
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

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void InitializeProperty()
        {
            ConfirmPasswordVisibility = new ReactiveProperty<Visibility>(Visibility.Hidden);
            Message = new ReactiveProperty<string>();
        }

        public IEnumerable GetErrors(string propertyName)
        {
            throw new NotImplementedException();
        }

        public ReactiveCommand<Window> LogInCommand { get; set; }

        public ReactiveCommand<Window> MinimizeWindowCommand { get; set; }

        public ReactiveCommand<Window> CloseWindowCommand { get; set; }


        public string Password { get; set; }

        public string ConfirmPassword { get; set; }


        public ReactiveProperty<string> Message { get; set; }

        public ReactiveProperty<Visibility> ConfirmPasswordVisibility { get; set; }

        public bool HasErrors => false;
    }
}

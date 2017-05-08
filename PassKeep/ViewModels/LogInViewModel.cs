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
using PassKeep.Common;
using System.Threading;

namespace PassKeep.ViewModels {
    public class LogInViewModel : BindableBase {
        public LogInViewModel() {
            if (!File.Exists("passkeep")) {
                ConfirmPassowrdVisibility = Visibility.Visible;
                Message = "パスワードを登録してください" + Environment.NewLine + "パスワードを忘れると復元不可能です";
            }

            LogInCommand = new RelayCommand<Window>((w) => {
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
                    Message = "パスワードが違います";
                }
            });

            CloseWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.CloseWindow(w);
            });

            MinimizeWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.MinimizeWindow(w);
            });
        }

        public ICommand LogInCommand { get; set; }

        public ICommand MinimizeWindowCommand { get; set; }

        public ICommand CloseWindowCommand { get; set; }


        public string Password { get; set; }

        public string ConfirmPassword { get; set; }


        private string _message;
        public string Message {
            get {
                return _message;
            }
            set {
                SetProperty(ref _message, value);
            }
        }

        private Visibility _confirmPassowrdVisibility = Visibility.Collapsed;
        public Visibility ConfirmPassowrdVisibility {
            get {
                return _confirmPassowrdVisibility;
            }
            set {
                SetProperty(ref _confirmPassowrdVisibility, value);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PassKeep.Material.Common;
using Reactive.Bindings;

namespace PassKeep.Material.ViewModel {
    public class ChangePasswordViewModel : Livet.ViewModel {
        public ChangePasswordViewModel() {
            IsChanged = false;

            ChangePasswordCommand = new ReactiveCommand<Window>();
            ChangePasswordCommand.Subscribe((w) => {
                if (string.IsNullOrEmpty(PasswordOld) ||
                    string.IsNullOrEmpty(PasswordNew) ||
                    string.IsNullOrEmpty(PasswordNewConfirm)) {
                    return;
                }

                if (Identity.Current != PasswordOld) {
                    return;
                }

                if (PasswordNew != PasswordNewConfirm) {
                    return;
                }

                Identity.Current = PasswordNew;
                IsChanged = true;
                MessageBox.Show("パスワードを変更しました。");
                SystemCommands.CloseWindow(w);
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


        public ReactiveCommand<Window> CloseWindowCommand { get; set; }

        public ReactiveCommand<Window> MinimizeWindowCommand { get; set; }

        public ReactiveCommand<Window> ChangePasswordCommand { get; set; }

        public string PasswordOld { get; set; }

        public string PasswordNew { get; set; }

        public string PasswordNewConfirm { get; set; }

        public bool IsChanged { get; set; }
    }
}

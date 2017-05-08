using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PassKeep.Common;

namespace PassKeep.ViewModels {
    public class ChangePasswordViewModel : BindableBase {
        public ChangePasswordViewModel() {
            IsChanged = false;

            ChangePasswordCommand = new RelayCommand<Window>((w) => {
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

            CloseWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.CloseWindow(w);
            });
            
            MinimizeWindowCommand = new RelayCommand<Window>((w) => {
                SystemCommands.MinimizeWindow(w);
            });
        }


        public ICommand CloseWindowCommand { get; set; }

        public ICommand MinimizeWindowCommand { get; set; }

        public ICommand ChangePasswordCommand { get; set; }

        public string PasswordOld { get; set; }

        public string PasswordNew { get; set; }

        public string PasswordNewConfirm { get; set; }

        public bool IsChanged { get; set; }
    }
}

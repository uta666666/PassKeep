using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using PassKeep.Material.Common;
using Reactive.Bindings;

namespace PassKeep.Material.ViewModel {
    public class ChangePasswordViewModel : Livet.ViewModel {
        public ChangePasswordViewModel() {
            IsChanged = false;

            ChangePasswordCommand = new ReactiveCommand();
            ChangePasswordCommand.Subscribe(() => {
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
                
                DialogHost.CloseDialogCommand.Execute(true, null);
            });
        }

        public ReactiveCommand ChangePasswordCommand { get; set; }

        public string PasswordOld { get; set; }

        public string PasswordNew { get; set; }

        public string PasswordNewConfirm { get; set; }

        public bool IsChanged { get; set; }
    }
}

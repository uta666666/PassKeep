using MahApps.Metro.Controls;
using PassKeep.Material.Model;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace PassKeep.Material.View.Behavior
{
    public class WindowCloseCheckBehavior : Behavior<MetroWindow>
    {
        public ObservableCollection<Account> Accounts {
            get {
                return (ObservableCollection<Account>)GetValue(AccountsProperty);
            }
            set {
                SetValue(AccountsProperty, value);
            }
        }

        public ReactiveCommand ShowDialogCommand {
            get {
                return (ReactiveCommand)GetValue(ShowDialogCommandProperty);
            }
            set {
                SetValue(ShowDialogCommandProperty, value);
            }
        }

        public static readonly DependencyProperty AccountsProperty
            = DependencyProperty.Register(nameof(Accounts), typeof(ObservableCollection<Account>), typeof(WindowCloseCheckBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowDialogCommandProperty
            = DependencyProperty.Register(nameof(ShowDialogCommand), typeof(ReactiveCommand), typeof(WindowCloseCheckBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Closing += AssociatedObject_Closing;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Closing -= AssociatedObject_Closing;
        }

        private void AssociatedObject_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Accounts.Any(a => a.HasChange()))
            {
                ShowDialogCommand?.Execute(AssociatedObject);
                e.Cancel = true;
                return;
            }
        }
    }
}

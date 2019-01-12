using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PassKeep.Material.View.Behavior {
    public class ReadOnlyPasswordBoxBehavior : Behavior<PasswordBox> {
        public bool IsReadOnly {
            get {
                return (bool)GetValue(IsReadOnlyProperty);
            }
            set {
                SetValue(IsReadOnlyProperty, value);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ReadOnlyPasswordBoxBehavior), new PropertyMetadata(null));

        protected override void OnAttached() {
            AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;

            base.OnAttached();
        }

        protected override void OnDetaching() {
            AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;

            base.OnDetaching();
        }

        private void AssociatedObject_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            if (IsReadOnly) {
                e.Handled = true;
            }
        }
    }
}

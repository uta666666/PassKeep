using MahApps.Metro.Controls;
using PassKeep.Material.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace PassKeep.Material.View.Behavior
{
    public class WindowLoadBehavior : Behavior<MetroWindow>
    {
        public bool IsDark {
            get {
                return (bool)GetValue(IsDarkProperty);
            }
            set {
                SetValue(IsDarkProperty, value);
            }
        }

        public bool HasChanges {
            get {
                return (bool)GetValue(HasChangesProperty);
            }
            set {
                SetValue(HasChangesProperty, value);
            }
        }

        public static readonly DependencyProperty IsDarkProperty
            = DependencyProperty.Register(nameof(IsDark), typeof(bool), typeof(WindowLoadBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty HasChangesProperty
            = DependencyProperty.Register(nameof(HasChanges), typeof(bool), typeof(WindowLoadBehavior), new PropertyMetadata(false));


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            new ThemeHelper().SetLightDark(IsDark);

            HasChanges = false;
        }
    }
}

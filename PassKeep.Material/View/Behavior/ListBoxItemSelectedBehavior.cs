using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace PassKeep.Material.View.Behavior
{
    public class ListBoxItemSelectedBehavior : Behavior<ListBox>
    {
        public ICommand Command {
            get {
                return (ICommand)GetValue(CommandProperty);
            }
            set {
                SetValue(CommandProperty, value);
            }
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ListBoxItemSelectedBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            //AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            //AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Command?.Execute(AssociatedObject.SelectedValue);
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Command?.Execute(AssociatedObject.SelectedValue);
            }
        }
    }
}

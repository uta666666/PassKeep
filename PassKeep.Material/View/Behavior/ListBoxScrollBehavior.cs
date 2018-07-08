using PassKeep.Material.Common;
using PassKeep.Material.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PassKeep.Material.View.Behavior
{
    public class ListBoxScrollBehavior : Behavior<ListBox>
    {
        public Account Target {
            get {
                return (Account)GetValue(TargetProperty);
            }
            set {
                SetValue(TargetProperty, value);
            }
        }
        
        public static DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target), typeof(Account), typeof(ListBoxScrollBehavior), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(TargetChanged)));

        private static void TargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;
            var target = e.NewValue as Account;
            listBox.ScrollIntoView(target);
        }

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            base.OnAttached();
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GlobalExclusionInfo.IsDragDroping) return;

            if (e.AddedItems.Count == 0) return;
            
            AssociatedObject.ScrollIntoView(e.AddedItems[0]);
        }
    }
}

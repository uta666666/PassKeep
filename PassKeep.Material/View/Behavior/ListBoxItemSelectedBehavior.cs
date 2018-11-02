using PassKeep.Material.Model;
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
        public Category CurrentCategory {
            get {
                return (Category)GetValue(CurrentCategoryProperty);
            }
            set {
                SetValue(CurrentCategoryProperty, value);
            }
        }

        public int EditMode {
            get {
                return (int)GetValue(EditModeProperty);
            }
            set {
                SetValue(EditModeProperty, value);
            }
        }


        public static readonly DependencyProperty CurrentCategoryProperty = 
            DependencyProperty.Register(nameof(CurrentCategory), typeof(Category), typeof(ListBoxItemSelectedBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty EditModeProperty = 
            DependencyProperty.Register(nameof(EditMode), typeof(int), typeof(ListBoxItemSelectedBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((Common.EditMode)EditMode != Common.EditMode.Edit)
            {
                return;
            }

            var category = (Category)AssociatedObject.SelectedValue;
            if (category == null)
            {
                return;
            }
            if (CurrentCategory == category)
            {
                return;
            }
            if (!category.IsEdit)
            {
                //変更させない
                AssociatedObject.SelectedValue = CurrentCategory;
            }
        }
    }
}

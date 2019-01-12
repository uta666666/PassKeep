using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using PassKeep.Material.Common;
using Reactive.Bindings;

namespace PassKeep.Material.View.Behavior {
    public class EditCategoryBehavior : Behavior<Button> {
        public ReactiveCommand AddCategoryCommand {
            get {
                return (ReactiveCommand)GetValue(AddCategoryCommandProperty);
            }
            set {
                SetValue(AddCategoryCommandProperty, value);
            }
        }

        public ReactiveCommand ModifyCategoryCommand {
            get {
                return (ReactiveCommand)GetValue(ModifyCategoryCommandProperty);
            }
            set {
                SetValue(ModifyCategoryCommandProperty, value);
            }
        }

        public ReactiveCommand SaveCategoryCommand {
            get {
                return (ReactiveCommand)GetValue(SaveCategoryCommandProperty);
            }
            set {
                SetValue(SaveCategoryCommandProperty, value);
            }
        }

        public int CategoryEditMode {
            get {
                return (int)GetValue(CategoryEditModeProperty);
            }
            set {
                SetValue(CategoryEditModeProperty, value);
            }
        }

        public static readonly DependencyProperty AddCategoryCommandProperty = DependencyProperty.Register(nameof(AddCategoryCommand), typeof(ReactiveCommand), typeof(EditCategoryBehavior), null);
        public static readonly DependencyProperty ModifyCategoryCommandProperty = DependencyProperty.Register(nameof(ModifyCategoryCommand), typeof(ReactiveCommand), typeof(EditCategoryBehavior), null);
        public static readonly DependencyProperty SaveCategoryCommandProperty = DependencyProperty.Register(nameof(SaveCategoryCommand), typeof(ReactiveCommand), typeof(EditCategoryBehavior), null);
        public static readonly DependencyProperty CategoryEditModeProperty = DependencyProperty.Register(nameof(CategoryEditMode), typeof(int), typeof(EditCategoryBehavior), null);


        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.Click += AssociatedObject_Click;
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            AssociatedObject.Click -= AssociatedObject_Click;
        }

        private void AssociatedObject_Click(object sender, RoutedEventArgs e) {
            switch ((EditMode)CategoryEditMode) {
                case EditMode.Confirm:
                    AddCategoryCommand?.Execute();
                    break;
                case EditMode.Add:
                    SaveCategoryCommand?.Execute();
                    break;
                case EditMode.Modify:
                    SaveCategoryCommand?.Execute();
                    break;
                default:
                    break;
            }
        }
    }
}

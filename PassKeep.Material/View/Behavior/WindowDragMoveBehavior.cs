using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Interactivity;

namespace PassKeep.Material.View.Behavior
{
    public class WindowDragMoveBehavior : Behavior<MetroWindow>
    {
        public bool IsDisableDragMove {
            get { return (bool)GetValue(IsDisableDragMoveProperty); }
            set { SetValue(IsDisableDragMoveProperty, value); }
        }

        public static DependencyProperty IsDisableDragMoveProperty = DependencyProperty.Register(nameof(IsDisableDragMove), typeof(bool), typeof(WindowDragMoveBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsDisableDragMove) return;

            AssociatedObject.DragMove();
        }
    }
}

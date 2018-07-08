using MahApps.Metro.Controls;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace PassKeep.Material.View.Behavior
{
    public class ForceLogoutBehavior : Behavior<MetroWindow>
    {
        public ReactiveCommand<MetroWindow> LogoutCommand {
            get {
                return (ReactiveCommand<MetroWindow>)GetValue(LogoutCommandProperty);
            }
            set {
                SetValue(LogoutCommandProperty, value);
            }
        }

        public static readonly DependencyProperty LogoutCommandProperty
            = DependencyProperty.Register(nameof(LogoutCommand), typeof(ReactiveCommand<MetroWindow>), typeof(ForceLogoutBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.IsVisibleChanged += AssociatedObject_IsVisibleChanged;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.IsVisibleChanged -= AssociatedObject_IsVisibleChanged;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
        }


        private DispatcherTimer _timer;
        private DateTime _start;
        private int _timeout;

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += _timer_Tick;
                _timer.Interval = TimeSpan.FromMilliseconds(500);

                _timeout = Properties.Settings.Default.Timeout;
            }
            _start = DateTime.Now;
            _timer.Start();
        }

        private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_timer == null) return;

            if (_timer.IsEnabled) return;

            if (AssociatedObject.Visibility != Visibility.Visible) return;

            _start = DateTime.Now;
            _timer.Start();
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_timer == null) return;

            _start = DateTime.Now;
        }

        private void AssociatedObject_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_timer == null) return;

            _start = DateTime.Now;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var diff = DateTime.Now - _start;
            if (diff >= TimeSpan.FromSeconds(_timeout))
            {
                _timer.Stop();
                LogoutCommand?.Execute(AssociatedObject);
            }
        }
    }
}

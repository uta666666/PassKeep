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
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
        }

        private DispatcherTimer _timer;
        private DateTime _start;

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += _timer_Tick;
                _timer.Interval = TimeSpan.FromMilliseconds(500);
            }
            _start = DateTime.Now;
            _timer.Start();
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_timer == null) return;

            _start = DateTime.Now;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var diff = DateTime.Now - _start;
            if (diff >= TimeSpan.FromMinutes(1))
            {
                _timer.Stop();
                LogoutCommand?.Execute(AssociatedObject);
            }
        }
    }
}

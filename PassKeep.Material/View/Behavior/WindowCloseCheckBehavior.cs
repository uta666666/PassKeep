﻿using MahApps.Metro.Controls;
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
        public bool HasChanges {
            get {
                return (bool)GetValue(HasChangesProperty);
            }
            set {
                SetValue(HasChangesProperty, value);
            }
        }

        public ReactiveCommand SaveCommand {
            get {
                return (ReactiveCommand)GetValue(SaveCommandProperty);
            }
            set {
                SetValue(SaveCommandProperty, value);
            }
        }

        public static readonly DependencyProperty HasChangesProperty
            = DependencyProperty.Register(nameof(HasChanges), typeof(bool), typeof(WindowCloseCheckBehavior), new PropertyMetadata(false));

        public static readonly DependencyProperty SaveCommandProperty
            = DependencyProperty.Register(nameof(SaveCommand), typeof(ReactiveCommand), typeof(WindowCloseCheckBehavior), new PropertyMetadata(null));

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
            if (HasChanges)
            {
                //e.Cancel = true;
                SaveCommand?.Execute(AssociatedObject);
                return;
            }
        }
    }
}

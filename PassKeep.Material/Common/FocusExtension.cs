using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PassKeep.Material.Common
{
    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }


        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }


        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusExtension), new UIPropertyMetadata(false, IsFocusedChanged));

        
        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uie = (UIElement)d;


            uie.GotFocus -= FrameworkElement_GotFocus;
            uie.LostFocus -= FrameworkElement_LostFocus;
            uie.GotFocus += FrameworkElement_GotFocus;
            uie.LostFocus += FrameworkElement_LostFocus;
            

            if ((bool)e.NewValue)
            {
                uie.Focus();
                (uie as TextBox)?.SelectAll();
            }

            //if (!fe.IsVisible)
            //{
            //    fe.IsVisibleChanged += new DependencyPropertyChangedEventHandler(fe_IsVisibleChanged);
            //}

            //if ((bool)e.NewValue)
            //{
            //    fe.Focus();
            //}
        }

        //private static void fe_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var fe = (FrameworkElement)sender;
        //    if (fe.IsVisible && (bool)((FrameworkElement)sender).GetValue(IsFocusedProperty))
        //    {
        //        fe.IsVisibleChanged -= fe_IsVisibleChanged;
        //        fe.Focus();
        //    }
        //}

        private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        }

        private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }
    }
}

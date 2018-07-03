﻿using PassKeep.Material.Common;
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
using System.Windows.Media;

namespace PassKeep.Material.View.Behavior
{
    public class DragStartBehavior : Behavior<UIElement>
    {
        public bool IsAttached {
            get { return (bool)GetValue(IsAttachedProperty); }
            set { SetValue(IsAttachedProperty, value); }
        }

        public DragDropEffects AllowedEffects {
            get { return (DragDropEffects)GetValue(AllowedEffectsProperty); }
            set { SetValue(AllowedEffectsProperty, value); }
        }

        public object DragDropData {
            get { return GetValue(DragDropDataProperty); }
            set { SetValue(DragDropDataProperty, value); }
        }

        public static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(nameof(IsAttached), typeof(bool), typeof(DragStartBehavior), new FrameworkPropertyMetadata(false, OnIsAttachedChanged));

        public static readonly DependencyProperty AllowedEffectsProperty = DependencyProperty.Register(nameof(AllowedEffects), typeof(DragDropEffects), typeof(DragStartBehavior), new UIPropertyMetadata(DragDropEffects.All));
        public static readonly DependencyProperty DragDropDataProperty = DependencyProperty.Register(nameof(DragDropData), typeof(object), typeof(DragStartBehavior), new PropertyMetadata(null));


        private static void OnIsAttachedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var el = o as UIElement;
            if (el != null)
            {
                var behColl = Interaction.GetBehaviors(el);
                var existingBehavior = behColl.FirstOrDefault(b => b.GetType() == typeof(DragStartBehavior)) as DragStartBehavior;
                if ((bool)e.NewValue == false && existingBehavior != null)
                {
                    behColl.Remove(existingBehavior);
                }
                else if ((bool)e.NewValue == true && existingBehavior == null)
                {
                    behColl.Add(new DragStartBehavior());
                }
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
            //AssociatedObject.PreviewMouseMove += AssociatedObject_PreviewMouseMove;
            //AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp;
            //AssociatedObject.QueryContinueDrag += AssociatedObject_QueryContinueDrag;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
            //AssociatedObject.PreviewMouseMove -= AssociatedObject_PreviewMouseMove;
            //AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
            //AssociatedObject.QueryContinueDrag -= AssociatedObject_QueryContinueDrag;
        }

        private void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left &&
                e.LeftButton == MouseButtonState.Pressed)
            {
                // Begin the drag operation
                UIElement uie = (UIElement)sender;
                var currentDragInfo = new UIElementMouseDrag();
                currentDragInfo.OnMouseDown(uie, e);
            }
        }


        internal class UIElementMouseDrag
        {
            private Point _origin;
            private bool _isButtonDown;
            private DragGhost _dragGhost;

            private Account _src;

            public void OnMouseDown(UIElement uie, MouseButtonEventArgs e)
            {
                _origin = e.GetPosition(uie);

                _src = (uie as ListBoxItem)?.DataContext as Account;
                
                _isButtonDown = true;

                uie.PreviewMouseMove += AssociatedObject_PreviewMouseMove;
                uie.PreviewMouseUp += AssociatedObject_PreviewMouseUp;
                uie.QueryContinueDrag += AssociatedObject_QueryContinueDrag;

                uie.CaptureMouse();
            }

            private void AssociatedObject_PreviewMouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton != MouseButtonState.Pressed || !_isButtonDown)
                {
                    return;
                }

                var uie = (sender as UIElement);

                var point = e.GetPosition(uie);
                if (CheckDistance(point, _origin))
                {
                    GlobalExclusionInfo.IsDragDroping = true;
                    try
                    {
                        if (_dragGhost == null)
                        {
                            var lbi = sender as ListBoxItem;

                            _dragGhost = new DragGhost(sender as UIElement, 0.5, e.GetPosition(sender as UIElement), lbi);
                            _dragGhost.Show();
                        }
                        DragDrop.DoDragDrop(uie, _src, DragDropEffects.Move);
                    }
                    finally
                    {
                        GlobalExclusionInfo.IsDragDroping = false;
                    }
                    _isButtonDown = false;
                    e.Handled = true;

                    if (_dragGhost != null)
                    {
                        _dragGhost.Remove();
                        _dragGhost = null;
                    }
                }
            }

            private void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e)
            {
                _isButtonDown = false;
            }

            private void AssociatedObject_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
            {
                if (_dragGhost != null)
                {
                    var uie = (sender as UIElement);

                    // ゴーストの位置を更新
                    _dragGhost.Position = CursorInfo.GetNowPosition(uie);
                }
            }

            private bool CheckDistance(Point x, Point y)
            {
                return Math.Abs(x.X - y.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                       Math.Abs(x.Y - y.Y) >= SystemParameters.MinimumVerticalDragDistance;
            }
        }
    }
}

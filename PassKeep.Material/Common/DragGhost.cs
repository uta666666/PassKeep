using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace PassKeep.Material.Common
{
    public class DragGhost : Adorner
    {

        protected UIElement ghost;
        protected Vector movement;
        protected bool isRendering;

        public DragGhost(UIElement draggedElement, double opacity, UIElement dummyElement)
            : this(draggedElement, opacity, new Point(0, 0), dummyElement) { }

        public DragGhost(UIElement draggedElement, double opacity, Point clicked, UIElement dummyElement)
            : base(draggedElement)
        {
            isRendering = false;

            // 1. ゴーストを作成

            // draggedElementを囲むサイズの長方形領域を作成
            var b = VisualTreeHelper.GetDescendantBounds(draggedElement);
            var rect = new System.Windows.Shapes.Rectangle() { Width = b.Width, Height = b.Height };

            // rectを指定の透過度のdraggedElementで塗りつぶすブラシ
            var brush = new VisualBrush(dummyElement) { Opacity = opacity };

            // rectに　ブラシを適用
            rect.Fill = brush;

            // rectをghostとして覚えておく
            ghost = rect;

            // 2. ゴーストの描画位置を決定するための前準備

            // clickedのクライアント座標を求め反転させたものを移動量とする
            movement = (Vector)Window.GetWindow(draggedElement).PointFromScreen(draggedElement.PointToScreen(clicked));
            movement.Negate();
        }

        private Point _Position;
        public Point Position {
            get { return _Position; }
            set {
                // 現在のマウス位置（クライアント座標系）に移動量を加算したものがゴーストの描画位置
                _Position = (Point)(value + movement);
                UpdatePosition();
            }
        }

        /// <summary>
        /// ゴーストの描画を開始する。
        /// DragDrop.DoDragDropメソッドを呼び出す直前に実行する。
        /// </summary>
        public bool Show()
        {
            if (!isRendering)
            {
                var adorner = AdornerLayer.GetAdornerLayer(this.AdornedElement);
                if (adorner != null)
                {
                    isRendering = true;
                    adorner.Add(this);
                }
            }
            return isRendering;
        }

        /// <summary>
        /// ゴーストの描画を終了する。
        /// DragDrop.DoDragDropメソッドを呼び出した直後に実行する。
        /// </summary>
        public void Remove()
        {
            if (isRendering)
            {
                var adorner = this.Parent as AdornerLayer;
                if (adorner != null)
                    adorner.Remove(this);
                isRendering = false;
            }
        }

        protected void UpdatePosition()
        {
            // Adornerの親はAdornerLayer
            var adorner = this.Parent as AdornerLayer;
            if (adorner != null)
            {
                adorner.Update(this.AdornedElement);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return ghost;
        }

        protected override int VisualChildrenCount {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size finalSize)
        {
            ghost.Measure(finalSize);
            return ghost.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {

            ghost.Arrange(new Rect(ghost.DesiredSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(Position.X, Position.Y));
            return result;
        }
    }
}

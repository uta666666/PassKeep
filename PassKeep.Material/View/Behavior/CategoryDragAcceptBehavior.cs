using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using PassKeep.Material.Common;
using PassKeep.Material.Model;

namespace PassKeep.Material.View.Behavior {
    public class CategoryDragAcceptBehavior : Behavior<ListBox> {

        public DragAcceptDescription<Category> CategoryDescription {
            get { return (DragAcceptDescription<Category>)GetValue(CategoryDescriptionProperty); }
            set { SetValue(CategoryDescriptionProperty, value); }
        }

        public static readonly DependencyProperty CategoryDescriptionProperty = DependencyProperty.Register(nameof(CategoryDescription), typeof(DragAcceptDescription<Category>), typeof(CategoryDragAcceptBehavior), new PropertyMetadata(null));

        protected override void OnAttached() {
            AssociatedObject.PreviewDragOver += AssociatedObject_DragOver;
            AssociatedObject.PreviewDrop += AssociatedObject_Drop;
            base.OnAttached();
        }

        protected override void OnDetaching() {
            AssociatedObject.PreviewDragOver -= AssociatedObject_DragOver;
            AssociatedObject.PreviewDrop -= AssociatedObject_Drop;
            base.OnDetaching();
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.None;
                return;
            }
            var desc = CategoryDescription;
            if (desc == null) {
                e.Effects = DragDropEffects.None;
                return;
            }
            var cat = (Category)e.Data.GetData(typeof(Category));
            if (cat.ID == 0) {
                e.Effects = DragDropEffects.None;
                return;
            }
            desc.OnDragOver(e);
            e.Handled = true;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e) {
            if (!GlobalExclusionInfo.IsDragDroping) {
                e.Effects = DragDropEffects.None;
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effects = DragDropEffects.None;
                return;
            }
            var desc = CategoryDescription;
            if (desc == null) {
                e.Effects = DragDropEffects.None;
                return;
            }

            var dropPos = e.GetPosition(AssociatedObject);
            var dropItem = e.Data.GetData(typeof(Category)) as Category;

            for (int i = 0; i < AssociatedObject.Items.Count; i++) {
                var item = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item == null) continue;
                var itemPos = AssociatedObject.PointFromScreen(item.PointToScreen(new Point(0, item.ActualHeight)));

                if (dropPos.Y < itemPos.Y) {
                    desc.OnDrop(new DragControlEventArgs<Category>(dropItem, i));
                    e.Handled = true;
                    return;
                }
            }
            desc.OnDrop(new DragControlEventArgs<Category>(dropItem, AssociatedObject.Items.Count - 1));
            e.Handled = true;
            return;
        }
    }
}

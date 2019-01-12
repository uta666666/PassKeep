using PassKeep.Material.Common;
using PassKeep.Material.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace PassKeep.Material.View.Behavior
{
    public sealed class AccountDragAcceptBehavior : Behavior<ListBox>
    {
        public DragAcceptDescription<Account> AccountDescription {
            get { return (DragAcceptDescription<Account>)GetValue(AccountDescriptionProperty); }
            set { SetValue(AccountDescriptionProperty, value); }
        }

        public static readonly DependencyProperty AccountDescriptionProperty = DependencyProperty.Register(nameof(AccountDescription), typeof(DragAcceptDescription<Account>), typeof(AccountDragAcceptBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            AssociatedObject.PreviewDragOver += AssociatedObject_DragOver;
            AssociatedObject.PreviewDrop += AssociatedObject_Drop;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewDragOver -= AssociatedObject_DragOver;
            AssociatedObject.PreviewDrop -= AssociatedObject_Drop;
            base.OnDetaching();
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            var desc = AccountDescription;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            desc.OnDragOver(e);
            e.Handled = true;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (!GlobalExclusionInfo.IsDragDroping)
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            var desc = AccountDescription;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var dropPos = e.GetPosition(AssociatedObject);
            var dropItem = e.Data.GetData(typeof(Account)) as Account;            

            for (int i = 0; i < AssociatedObject.Items.Count; i++)
            {
                var item = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item == null) continue;
                var itemPos = AssociatedObject.PointFromScreen(item.PointToScreen(new Point(0, item.ActualHeight)));

                if(dropPos.Y < itemPos.Y)
                {
                    desc.OnDrop(new DragControlEventArgs<Account>(dropItem, i));
                    e.Handled = true;
                    return;
                }
            }
            desc.OnDrop(new DragControlEventArgs<Account>(dropItem, AssociatedObject.Items.Count - 1));
            e.Handled = true;
            return;
        }
    }
}

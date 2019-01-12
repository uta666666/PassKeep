using PassKeep.Material.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PassKeep.Material.Model
{
    public sealed class DragAcceptDescription<T>
    {
        public event Action<DragEventArgs> DragOver;

        public void OnDragOver(DragEventArgs dragEventArgs)
        {
            DragOver?.Invoke(dragEventArgs);
        }

        public event Action<DragControlEventArgs<T>> DragDrop;

        public void OnDrop(DragControlEventArgs<T> dragEventArgs)
        {
            DragDrop?.Invoke(dragEventArgs);
        }
    }
}

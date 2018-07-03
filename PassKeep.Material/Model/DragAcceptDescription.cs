using PassKeep.Material.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PassKeep.Material.Model
{
    public sealed class DragAcceptDescription
    {
        public event Action<DragEventArgs> DragOver;

        public void OnDragOver(DragEventArgs dragEventArgs)
        {
            DragOver?.Invoke(dragEventArgs);
        }

        public event Action<DragControlEventArgs> DragDrop;

        public void OnDrop(DragControlEventArgs dragEventArgs)
        {
            DragDrop?.Invoke(dragEventArgs);
        }
    }
}

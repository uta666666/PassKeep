using PassKeep.Material.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PassKeep.Material.Common
{
    public class DragControlEventArgs<T> : RoutedEventArgs
    {
        public DragControlEventArgs(T src, int newIndex)
        {
            MovingData = src;
            NewIndex = newIndex;
        }

        public T MovingData { get; set; }

        public int NewIndex { get; set; }
    }
}

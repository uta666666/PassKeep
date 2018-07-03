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
    public class DragControlEventArgs : RoutedEventArgs
    {
        public DragControlEventArgs(Account src, int newIndex)
        {
            MovingData = src;
            NewIndex = newIndex;
        }

        public Account MovingData { get; set; }

        public int NewIndex { get; set; }
    }
}

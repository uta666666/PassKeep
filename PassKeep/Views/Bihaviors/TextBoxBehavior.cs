using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace PassKeep.Views.Bihaviors {
    public class TextBoxBehavior : Behavior<TextBox> {
        public void Focus() {
            AssociatedObject.Focus();
        }

        public void SelectAll() {
            AssociatedObject.SelectAll();
        }
    }
}

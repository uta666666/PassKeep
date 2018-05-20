using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.ViewModel
{
    public class MessageViewModel : Livet.ViewModel
    {
        public MessageViewModel() {
            InitializeProperty();
        }

        private void InitializeProperty()
        {
            Message = new ReactiveProperty<string>(string.Empty);
        }

        public ReactiveProperty<string> Message { get; set; }
    }
}

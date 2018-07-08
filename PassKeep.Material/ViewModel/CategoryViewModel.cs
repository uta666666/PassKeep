using MaterialDesignThemes.Wpf;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.ViewModel
{
    public class CategoryViewModel : Livet.ViewModel
    {
        public CategoryViewModel()
        {
            CategoryName = new ReactiveProperty<string>(string.Empty);

            InputCategoryNameCommand = new ReactiveCommand();
            InputCategoryNameCommand.Subscribe(n =>
            {
                DialogHost.CloseDialogCommand.Execute(true, null);
            });
        }

        public ReactiveCommand InputCategoryNameCommand { get; set; }

        public ReactiveProperty<string> CategoryName { get; set; }
    }
}

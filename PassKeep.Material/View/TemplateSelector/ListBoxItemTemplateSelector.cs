using PassKeep.Material.Model;
using System.Windows;
using System.Windows.Controls;

namespace PassKeep.Material.View.TemplateSelector
{
    public class ListBoxItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TemplateConfirm { get; set; }

        public DataTemplate TemplateEdit { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var category = (Category)item;
            if (category.IsEdit)
            {
                return TemplateEdit;
            }
            else
            {
                return TemplateConfirm;
            }
        }
    }
}

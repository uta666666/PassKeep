using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.Model
{
    public class CategoryList : ObservableCollection<Category>
    {
        public CategoryList() {
            Add(new Category() { ID = 0, Name = "ALL" });
        }

        public void CopyTo(CategoryList dest)
        {
            dest.Clear();
            foreach (var cat in Items)
            {
                dest.Add(new Category()
                {
                    ID = cat.ID,
                    Name = cat.Name
                });
            }
        }
    }
}

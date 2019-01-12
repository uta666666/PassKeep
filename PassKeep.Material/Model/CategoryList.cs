using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassKeep.Material.Common;

namespace PassKeep.Material.Model
{
    public class CategoryList : ObservableCollection<Category>
    {
        public static CategoryList MakeList(string decryptCatStr) {
            var categories = JsonConvert.DeserializeObject<CategoryList>(decryptCatStr);
            if (!categories.Any(n => n.ID == 0)) {
                categories.Insert(0, new Category() { ID = 0, Name = "ALL", Name2 = "None", Order = 0 });
            }

            foreach (var cat in categories) {
                if (cat.ID == 0 && cat.Name2 != "None") {
                    cat.Name2 = "None";
                } else if (cat.Name != cat.Name2) {
                    cat.Name2 = cat.Name;
                }
            }
            return categories;
        }

        public static CategoryList MakeList() {
            return new CategoryList() { new Category() { ID = 0, Name = "ALL", Name2 = "None", Order = 0 } };
        }

        public CategoryList() { }

        public CategoryList(IEnumerable<Category> source) {
            foreach (var s in source) {
                Add(s);
            }
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

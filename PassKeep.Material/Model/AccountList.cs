using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.Model
{
    public class AccountList : ObservableCollection<Account>
    {
        public AccountList() { }

        public AccountList(IEnumerable<Account> src)
        {
            foreach(var account in src)
            {
                Add(account);
            }
        }

        public void CopyTo(AccountList dest)
        {
            dest.Clear();
            foreach (var account in Items)
            {
                dest.Add(new Account()
                {
                    ID = account.ID,
                    Mail = account.Mail,
                    Password = account.Password,
                    Title = account.Title,
                    URL = account.URL,
                    CategoryID = account.CategoryID
                });
            }
        }
    }
}

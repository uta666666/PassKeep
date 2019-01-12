using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.Model
{
    public class PassKeepData
    {
        public static PassKeepData MakeData(string decryptStr)
        {
            return JsonConvert.DeserializeObject<PassKeepData>(decryptStr);
        }

        public AccountList Accounts { get; set; }

        public CategoryList Categories { get; set; }
    }
}

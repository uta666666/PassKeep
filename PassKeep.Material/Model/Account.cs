using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeep.Material.Model {
    public class Account {
        public string Title { get; set; } = string.Empty;

        public string ID { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public Uri URL { get; set; }

        public string Mail { get; set; } = string.Empty;
    }
}

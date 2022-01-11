using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitTools.Test.Models {
    
    class User {
        public string Name { get; set; }
        public int Age { get; set; }
        public string IDCard { get; set; }
        public string Address { get; set; }
        public byte[] Avatar { get; set; }
    }

    class UserDTO {
        public string NA { get; set; }
        public string Address { get; set; }
        public byte[] Avatar { get; set; }
    }
}

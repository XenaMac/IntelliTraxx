using Base_AVL.LATAService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class ITUser
    {
        public User user { get; set; }
        public List<Role> Roles { get; set; }
        public List<Company> Companies { get; set; }
    }

}
using Base_AVL.LATAService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Base_AVL.Common
{
    public class UserProfile
    {
        public Guid UserID { get; set; }
        public string UserLastName { get; set; }
        public string UserFirstName { get; set; }
        public string UserEmail { get; set; }
        public string UserOffice { get; set; }
        public string UserPhone { get; set; }
        public List<Role> Roles { get; set; }
        public List<Company> Companies { get; set; }
    }
}
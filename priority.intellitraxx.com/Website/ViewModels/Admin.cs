using Base_AVL.LATAService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class Admin
    {
        public List<Role> Roles { get; set; }
        public List<Company> Companies { get; set; }
        public List<ITUser> Users { get; set; }
        public List<VehicleClass> VehicleClasses { get; set; }
        public List<VehicleVM> Vehicles { get; set; }
        public List<Vars> AppVariables { get; set; }
        public List<Vars> ServiceVariables { get; set; }
    }
}
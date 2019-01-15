using System;
using System.Collections.Generic;
using IntelliTraxx.TruckService;

namespace IntelliTraxx.Common
{
    public class AuthMgmt
    {
        private TruckServiceClient truckService = new TruckServiceClient();

        public Guid logonUser(string username, string password)
        {
            Guid userID = new Guid(truckService.logonUser(username, password).ToString());
            return userID;
        }

        public List<string> getUserRoles(Guid userID)
        {
            List<string> rolesList = new List<string>();
            List<Role> Roles = new List<Role>();
            var userRoles = truckService.getUserRolesGuids(userID);
            var roles = truckService.getRoles(new Guid());

            foreach (Guid ur in userRoles)
            {
                foreach (Role r in roles)
                {
                    if (r.RoleID == ur)
                    {
                        Roles.Add(r);
                        rolesList.Add(r.roleName);
                    }
                }
            }

            //Set Roles Session Object
            System.Web.HttpContext.Current.Session["IntelliTraxxUserRoles"] = Roles;

            return rolesList;
        }
    }
}
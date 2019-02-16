using System;
using System.Collections.Generic;
using System.Web;
using IntelliTraxx.Shared.TruckService;

namespace IntelliTraxx.Shared.Identity
{
    public class AuthMgmt
    {
        private readonly TruckServiceClient _truckService = new TruckServiceClient();

        public Guid LogonUser(string username, string password)
        {
            var userID = new Guid(_truckService.logonUser(username, password).ToString());
            return userID;
        }

        public List<string> GetUserRoles(Guid userId)
        {
            var rolesList = new List<string>();
            var returnList = new List<Role>();

            var userRoles = _truckService.getUserRolesGuids(userId);
            var roles = _truckService.getRoles(new Guid());

            foreach (var ur in userRoles)
                foreach (var r in roles)
                    if (r.RoleID == ur)
                    {
                        returnList.Add(r);
                        rolesList.Add(r.roleName);
                    }

            //Set Roles Session Object
            HttpContext.Current.Session["IntelliTraxxUserRoles"] = returnList;

            return rolesList;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.

    public class TruckService : ITruckService
    {
        #region  " Get Vehicle Information "

        /// <summary>
        /// Returns a comma-delimited string of all vehicles in service
        /// </summary>
        /// <returns>list of vehicles</returns>
        public string getVehicles()
        {
            string ret = string.Empty;
            if (GlobalData.GlobalData.vehicles.Count > 0)
            {
                foreach (Models.Vehicle veh in GlobalData.GlobalData.vehicles)
                {
                    ret += veh.VehicleID + " : " + veh.message + ",";
                }
                ret = ret.Substring(0, ret.Length - 1);
            }
            else
            {
                ret = "No Vehicles";
            }
            return ret;
        }

        /// <summary>
        /// returns a list of retVehicle objects for all vehicles in service
        /// </summary>
        /// <returns>list of vehicle objects</returns>
        public List<retVehicle> getVehicleList()
        {
            List<retVehicle> vehList = new List<retVehicle>();
            foreach (Models.Vehicle veh in GlobalData.GlobalData.vehicles)
            {
                retVehicle rv = new retVehicle();
                rv.VehicleID = veh.VehicleID;
                rv.message = veh.message;
                rv.gpsData = veh.gps;
                rv.status = veh.status;
                vehList.Add(rv);
            }
            return vehList;
        }

        /// <summary>
        /// returns a list of vehilce ids and vehilceids vehicles in the db
        /// </summary>
        /// <returns>list of vehicle objects</returns>
        public List<linkVehicle> getVehicleListBasic()
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<linkVehicle> lvList = sql.getLinkVehicles();
            return lvList;
        }

        /// <summary>
        /// returns a list of vehilce ids and macaddress from router in the db
        /// </summary>
        /// <returns>list of vehicle objects</returns>
        public List<macVehicle> getVehicleListMac()
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<macVehicle> macList = sql.getMacVehicles();
            return macList;
        }

        public List<Models.Vehicle> getAllVehicles(bool loadHistorical)
        {
            try {
                List<Models.Vehicle> allVehicles = new List<Models.Vehicle>();
                foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles) {
                    allVehicles.Add(v);
                }
                if (loadHistorical) {
                    GlobalData.SQLCode sql = new GlobalData.SQLCode();
                    List<Models.Vehicle> otherVehicles = sql.getAllVehicles(allVehicles);
                    foreach (Models.Vehicle v in otherVehicles)
                    {
                        List<Messages.Status> statuses = new List<Messages.Status>();
                        Messages.Status status = new Messages.Status();
                        status.messageType = "A/I";
                        status.statusName = "Active";
                        status.statusVal = "Inactive";
                        statuses.Add(status);
                        v.status = statuses;
                        allVehicles.Add(v);
                    }
                }

                return allVehicles;
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        public List<VehicleGPSRecord> getAllGPS() {
            List<VehicleGPSRecord> gpsList = new List<VehicleGPSRecord>();
            foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles) {
                VehicleGPSRecord g = new VehicleGPSRecord();
                g.Direction = (float)v.gps.dir;
                g.ID = v.extendedData.ID;
                g.InPolygon = v.isInside;
                g.lastMessageReceived = v.lastMessageReceived;
                g.Lat = (float)v.gps.lat;
                g.Lon = (float)v.gps.lon;
                g.PolyName = v.insidePolyName.polyName;
                g.runID = v.runID;
                g.Speed = (float)v.gps.spd;
                g.timestamp = DateTime.Now;
                g.VehicleID = v.VehicleID;
                g.status = "Normal";
                foreach(var a in v.alerts)
                {
                    if (a.alertActive == true)
                    {
                        g.status = "InAlert";
                    }
                }
                gpsList.Add(g);
            }
            return gpsList;

        }

        public List<VehicleGPSRecord> getGPS(Guid id)
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<VehicleGPSRecord> gpsList = new List<VehicleGPSRecord>();
            foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles.Where( v => v.extendedData.ID == id))
            {
                VehicleGPSRecord g = new VehicleGPSRecord();
                g.Direction = (float)v.gps.dir;
                g.ID = v.extendedData.ID;
                g.InPolygon = v.isInside;
                g.lastMessageReceived = v.lastMessageReceived;
                g.Lat = (float)v.gps.lat;
                g.Lon = (float)v.gps.lon;
                g.PolyName = v.insidePolyName.polyName;
                g.runID = v.runID;
                g.Speed = (float)v.gps.spd;
                g.timestamp = DateTime.Now;
                g.VehicleID = v.VehicleID;
                g.status = "Normal";
                foreach(var a in v.alerts)
                {
                    if(a.alertActive == true)
                    {
                        g.status = "InAlert";
                    }
                }
                g.ABI = v.ABI;
                gpsList.Add(g);
            }
            return gpsList;

        }

        public Models.Vehicle getVehicleData(Guid ID) {
            List<Models.Vehicle> vehics = getAllVehicles(true);
            Models.Vehicle v = vehics.Find(delegate (Models.Vehicle find) { return find.extendedData.ID == ID; });
            return v;
        }

        public List<VehicleGPSRecord> getLastTwoHours(string vehicleID)
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            sql.getLastTwoHours(vehicleID);

            List<VehicleGPSRecord> GPSList = new List<VehicleGPSRecord>();
            foreach (VehicleGPSRecord g in GlobalData.GlobalData.GPSRecords)
            {
                GPSList.Add(g);
            }
            return GPSList;
        }

        public List<VehicleGPSRecord> getLocationHistory(string vehicleID, DateTime from, DateTime to)
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            sql.getVehicleHistory(vehicleID, from.ToUniversalTime(), to.ToUniversalTime());

            List<VehicleGPSRecord> GPSList = new List<VehicleGPSRecord>();
            foreach (VehicleGPSRecord g in GlobalData.GlobalData.GPSRecords)
            {
                GPSList.Add(g);
            }
            return GPSList;
        }

        //remove a vehicle from the service. This comes in handy for testing and removing bad vehicles
        public string killVehicle(string vehicleID)
        {
            try
            {
                bool killed = false;
                for (int i = GlobalData.GlobalData.vehicles.Count() - 1; i >= 0; i--)
                {
                    if (GlobalData.GlobalData.vehicles[i].VehicleID == vehicleID)
                    {
                        GlobalData.GlobalData.vehicles.RemoveAt(i);
                        killed = true;
                    }
                }
                if (killed)
                {
                    return "Removed " + vehicleID;
                }
                else {
                    return "Couldn't find " + vehicleID;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<VehicleExtendedData> getAvailableVehicles()
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<VehicleExtendedData> aVehics = sql.getAvailableVehicles();
                return aVehics;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Get and set user/role/company objects, handle logons"

        #region " User Logon "
        /// <summary>
        /// validate a user, input email and password
        /// return valid guid if valid user, else return empty guid
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="userPassword"></param>
        /// <returns>empty guid if credentials are invalid, else user id</returns>
        public Guid logonUser(string userEmail, string userPassword)
        {
            try
            {
                Guid g = Guid.Empty;

                //grab userEmail first
                User userfound = Users.GlobalUserData.userList.Find(delegate (User find)
                {
                    return find.UserEmail == userEmail;
                });

                //Grab userid for Salt if user found
                if (userfound != null)
                {
                    User userSalt = Users.GlobalUserData.userList.Find(delegate (User user)
                    {
                        return user.UserEmail == userEmail;
                    });

                    string pwdToHash = userPassword + "^LA~IT"; // ^LA~IT is my hard-coded salt
                    string hashedPwd = DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(pwdToHash, userSalt.UserSalt);

                    User found = Users.GlobalUserData.userList.Find(delegate (User find)
                    {
                        return find.UserEmail == userEmail && find.UserPassword == hashedPwd;
                    });
                    if (found != null)
                    {
                        g = found.UserID;
                    }
                }
                else
                {
                    g = Guid.Empty;
                }

                return g;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " User information "

        /// <summary>
        /// return a list of user objects based on company affiliation
        /// if userid = guid.empty, return all user objects
        /// </summary>
        /// <param name="userID">optional, send empty guid for all users</param>
        /// <returns>list of user objects</returns>
        public User getUserProfile(Guid userID)
        {
            //get users for a given company
            User User = (from u in Users.GlobalUserData.userList
                           where u.UserID == userID
                           select u).FirstOrDefault();

            return User;
        }

        /// <summary>
        /// return a list of user objects based on company affiliation
        /// if companyid = guid.empty, return all user objects
        /// </summary>
        /// <param name="companyID">optional, send empty guid for all users</param>
        /// <returns>list of user objects</returns>
        public List<User> getUsers(Guid companyID)
        {
            List<User> userList = new List<User>();
            if (companyID == null || companyID == Guid.Empty)
            {
                //get all the users
                foreach (User u in Users.GlobalUserData.userList)
                {
                    userList.Add(u);
                }
            }
            else
            {
                //get users for a given company
                var uList = from u in Users.GlobalUserData.userList
                            join ur in Users.GlobalUserData.userCompanyList on u.UserID equals ur.UserID
                            where ur.CompanyID == companyID
                            select new { u };
                foreach (var u in uList)
                {
                    if(!userList.Contains(u.u))
                    { userList.Add(u.u); }
                }
            }
            return userList;
        }

        /// <summary>
        /// Get a list of User Object by selected role id
        /// </summary>
        /// <param name="roleID">the roleid to look for, returns all users if guid.empty</param>
        /// <returns>list of user objects</returns>
        public List<User> getUsersByRole(Guid roleID)
        {
            List<User> userList = new List<User>();

            if (roleID == null || roleID == Guid.Empty)
            {
                foreach (User u in Users.GlobalUserData.userList)
                {
                    userList.Add(u);
                }
            }
            else
            {
                var uList = from u in Users.GlobalUserData.userList
                            join ur in Users.GlobalUserData.userRoleList on u.UserID equals ur.UserID
                            where ur.RoleID == roleID
                            select new { u };
                foreach (var u in uList)
                {
                    userList.Add(u.u);
                }
            }

            return userList;
        }

        /// <summary>
        /// Get a list of User Object by selected company id
        /// </summary>
        /// <param name="companyID">the companyID to look for, returns all users if guid.empty</param>
        /// <returns>list of user objects</returns>
        public List<User> getUsersByCompany(Guid companyID)
        {
            List<User> userList = new List<User>();

            if (companyID == null || companyID == Guid.Empty)
            {
                foreach (User u in Users.GlobalUserData.userList)
                {
                    userList.Add(u);
                }
            }
            else
            {
                var uList = from u in Users.GlobalUserData.userList
                            join uc in Users.GlobalUserData.userCompanyList on u.UserID equals uc.UserID
                            where uc.CompanyID == companyID
                            select new { u };
                foreach (var u in uList)
                {
                    userList.Add(u.u);
                }
            }

            return userList;
        }

        /// <summary>
        /// just returns the complete list of user/role associations
        ///
        /// </summary>
        /// <returns></returns>
        public List<UserRole> getAllUserRoles()
        {
            return Users.GlobalUserData.userRoleList;
        }

        /// <summary>
        /// returns the complete list of user/company associations
        /// </summary>
        /// <returns></returns>
        public List<UserCompany> getAllUserCompanies()
        {
            return Users.GlobalUserData.userCompanyList;
        }

        /// <summary>
        /// Get a guid list of a user's roles
        /// </summary>
        /// <param name="userID">guid - UserID to be found</param>
        /// <returns></returns>
        public List<Guid> getUserRolesGuids(Guid userID)
        {
            try
            {
                List<Guid> userRoles = new List<Guid>();
                var lstRoles = from ur in Users.GlobalUserData.userRoleList
                               where ur.UserID == userID
                               select ur;
                foreach (UserRole ur in lstRoles)
                {
                    userRoles.Add(ur.RoleID);
                }
                return userRoles;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// get a guid list of companies a user is involved with
        /// </summary>
        /// <param name="userID">guid - user ID to be found</param>
        /// <returns></returns>
        public List<Guid> getUserCompanies(Guid userID)
        {
            try
            {
                List<Guid> userCompanies = new List<Guid>();
                var lstCompanies = from uc in Users.GlobalUserData.userCompanyList
                                   where uc.UserID == userID
                                   select uc;
                foreach (UserCompany uc in lstCompanies)
                {
                    userCompanies.Add(uc.CompanyID);
                }
                return userCompanies;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// get a guid list of companies a user is involved with
        /// </summary>
        /// <param name="userID">guid - user ID to be found</param>
        /// <returns></returns>
        public List<Company> getUserCompaniesFull(Guid userID)
        {
            try
            {
                List<Company> userCompanies = new List<Company>();
                var lstCompanies = from c in Users.GlobalUserData.companyList
                                   join uc in Users.GlobalUserData.userCompanyList on c.CompanyID equals uc.CompanyID
                                   where uc.UserID == userID
                                   select c;
                foreach (Company uc in lstCompanies)
                {
                    userCompanies.Add(uc);
                }
                return userCompanies;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// get a guid list of roles a user is with
        /// </summary>
        /// <param name="userID">guid - user ID to be found</param>
        /// <returns></returns>
        public List<Role> getUserRolesFull(Guid userID)
        {
            try
            {
                List<Role> userRoles = new List<Role>();
                var lstRoles = from r in Users.GlobalUserData.roleList
                                   join u in Users.GlobalUserData.userRoleList on r.RoleID equals u.RoleID
                                   where u.UserID == userID
                                   select r;
                foreach (Role r in lstRoles)
                {
                    userRoles.Add(r);
                }
                return userRoles;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Add or update a user account
        /// </summary>
        /// <param name="u">User account to be modified</param>
        /// <param name="operatorID">guid of the person doing to the operation</param>
        /// <returns>OK if ok, else error message</returns>
        public string setUser(User u, Guid operatorID)
        {
            string ret = "OK";

            try
            {
                if (string.IsNullOrEmpty(u.UserPassword))
                {
                    return "Password cannot be blank";
                }
                if (string.IsNullOrEmpty(u.UserSalt)) //this is probably a new user
                {
                    string pwdToHash = u.UserPassword + "^LA~IT"; // ^LA~IT is my hard-coded salt
                    string salt = DevOne.Security.Cryptography.BCrypt.BCryptHelper.GenerateSalt();
                    string hashedPwd = DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(pwdToHash, salt);
                    u.UserPassword = hashedPwd;
                    u.UserSalt = salt;
                }
                Users.GlobalUserData.addUser(u, operatorID);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return ret;
        }

        public string resetPassword(Guid userID, string newPassword, string oldPassword, Guid operatorID)
        {
            string ret = "OK";

            try
            {
                User found = Users.GlobalUserData.userList.Find(delegate (User find) { return find.UserID == userID; });
                if (found != null)
                {
                    //the password stored with the user object
                    string oldPWD = found.UserPassword;
                    //the password sent to the system
                    string sentPWD = oldPassword + "^LA~IT";
                    string hashedSentPWD = DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(sentPWD, found.UserSalt);
                    //compare the two passwords
                    if (oldPWD == hashedSentPWD)
                    {
                        //valid password, create a new one and mark it.
                        string newPWD = newPassword + "^LA~IT";
                        string hashedNewPWD = DevOne.Security.Cryptography.BCrypt.BCryptHelper.HashPassword(newPWD, found.UserSalt);
                        found.UserPassword = hashedNewPWD;
                        Users.GlobalUserData.addUser(found, operatorID);
                    }
                    else
                    {
                        return "Incorrect old password sent, please try again";
                    }
                }
                else
                {
                    ret = "Couldn't find user or incorrect password sent";
                }
                return ret;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Delete user from db & service
        /// </summary>
        /// <param name="u">User object</param>
        /// <param name="operatorID">guid of person running</param>
        /// <returns>OK if ok, else error message</returns>
        public string deleteUser(User u, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.deleteUser(u, operatorID);
                return "OK";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Company Information "

        /// <summary>
        /// returns company information, either all companies or a specific company
        /// </summary>
        /// <param name="companyID">id of company to be returned, all companies come back if guid.empty</param>
        /// <returns>company object or list of comapny objects</returns>
        public List<Company> getCompanies(Guid companyID)
        {
            List<Company> companyList = new List<Company>();
            if (companyID == null || companyID == Guid.Empty)
            {
                foreach (Company c in Users.GlobalUserData.companyList)
                {
                    companyList.Add(c);
                }
            }
            else
            {
                var cList = from c in Users.GlobalUserData.companyList
                            where c.CompanyID == companyID
                            select c;
                foreach (Company c in cList)
                {
                    companyList.Add(c);
                }
            }
            return companyList;
        }

        /// <summary>
        /// add or update a company
        /// </summary>
        /// <param name="c">company object</param>
        /// <param name="operatorID">user id of user performing operation</param>
        /// <returns>OK if ok, else error message</returns>
        public string setCompany(Company c, Guid operatorID)
        {
            string ret = "OK";
            try
            {
                Users.GlobalUserData.addCompany(c, operatorID);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return ret;
        }

        /// <summary>
        /// delete a company
        /// </summary>
        /// <param name="c">company to be deleted</param>
        /// <param name="operatorID">guid of operator</param>
        /// <returns></returns>
        public string deleteCompany(Company c, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.deleteCompany(c, operatorID);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #region " Role Information "

        /// <summary>
        /// Return a list of roles
        /// </summary>
        /// <returns>List of Role Objects</returns>
        public List<Role> getRoles(Guid roleID)
        {
            List<Role> roleList = new List<Role>();
            try
            {
                if (roleID == null || roleID == Guid.Empty)
                {
                    foreach (Role r in Users.GlobalUserData.roleList)
                    {
                        roleList.Add(r);
                    }
                }
                else
                {
                    foreach (Role r in Users.GlobalUserData.roleList)
                    {
                        if (r.RoleID == roleID)
                        {
                            roleList.Add(r);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return roleList;
        }

        /// <summary>
        /// add or update a role
        /// </summary>
        /// <param name="r">role object</param>
        /// <param name="operatorID">id of operator performing operation</param>
        /// <returns>OK if ok else error message</returns>
        public string setRole(Role r, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.addRole(r, operatorID);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// delete a role from teh service and database
        /// </summary>
        /// <param name="r">role to be removed</param>
        /// <param name="operatorID">id of operator</param>
        /// <returns></returns>
        public string deleteRole(Role r, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.deleteRole(r, operatorID);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #endregion

        #region " User -> Company||Role association "

        /// <summary>
        /// add user object to company
        /// </summary>
        /// <param name="userID">user id to be added</param>
        /// <param name="companyID">company id to add user to</param>
        /// <param name="operatorID">id of operator performation operation</param>
        /// <returns>OK if ok, else error message</returns>
        public string addUserToCompany(Guid userID, Guid companyID, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.addUserToCompany(userID, companyID, operatorID);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// remove user object from company
        /// </summary>
        /// <param name="userID">user to be removed</param>
        /// <param name="companyID">company to be removed from</param>
        /// <param name="operatorID">operator performing operation</param>
        /// <returns></returns>
        public string removeUserFromCompany(Guid userID, Guid companyID, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.removeUserFromCompany(userID, companyID, operatorID);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Add a user to a role
        /// </summary>
        /// <param name="userID">User ID to be added</param>
        /// <param name="roleID">Role ID to be added to</param>
        /// <param name="operatorID">operator performing operation</param>
        /// <returns></returns>
        public string addUserToRole(Guid userID, Guid roleID, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.addUserToRole(userID, roleID, operatorID);
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// remove a user from a role
        /// </summary>
        /// <param name="userID">userid to be removed</param>
        /// <param name="roleID">roleid to be removed from</param>
        /// <param name="operatorID">operator performing operation</param>
        /// <returns></returns>
        public string removeUserFromRole(Guid userID, Guid roleID, Guid operatorID)
        {
            try
            {
                Role r = Users.GlobalUserData.roleList.Find(delegate (Role find) { return find.RoleID == roleID; });
                if (userID == operatorID && r != null && r.isAdmin == true)
                {
                    return "Cannot remove current user from Admin role";
                }
                else
                {
                    Users.GlobalUserData.removeUserFromRole(userID, roleID, operatorID);
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #endregion

        #region " Vehicle Extended Data "

        public List<VehicleExtendedData> getExtendedData()
        {
            try
            {
                List<VehicleExtendedData> vList = new List<VehicleExtendedData>();
                foreach (VehicleExtendedData v in GlobalData.GlobalData.vehicleEDs)
                {
                    vList.Add(v);
                }
                return vList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void updateExtendedData(VehicleExtendedData v, Guid operatorID)
        {
            try
            {
                GlobalData.GlobalData.updateExtendedData(v, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void deleteExtendedData(VehicleExtendedData v, Guid operatorID)
        {
            try
            {
                GlobalData.GlobalData.deleteVehicleExtendedData(v, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Vehicle Classes "

        public List<VehicleClass> getVehicleClasses()
        {
            try
            {
                List<VehicleClass> vList = new List<VehicleClass>();
                foreach (VehicleClass vc in GlobalData.GlobalData.vehicleClasses)
                {
                    vList.Add(vc);
                }
                return vList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void updateVehicleClass(VehicleClass v, Guid operatorID)
        {
            try
            {
                GlobalData.GlobalData.addUpdateVehicleClass(v, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void deleteVehicleClass(VehicleClass v, Guid operatorID)
        {
            try
            {
                GlobalData.GlobalData.deleteVehicleClass(v.VehicleClassID, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Driver Data & Logons "

        public List<Driver> getDrivers()
        {
            try
            {
                List<Driver> dList = new List<Driver>();
                foreach (Driver d in Users.GlobalUserData.driverList)
                {
                    dList.Add(d);
                }
                return dList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public List<Driver> getAvailableDrivers()
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<Driver> aDList = sql.getAvailableDrivers();
                return aDList;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void updateDriver(Driver d, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.updateDriver(d, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void deleteDriver(Driver d, Guid operatorID)
        {
            try
            {
                Users.GlobalUserData.deleteDriver(d, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public string logoffDriver(string PIN, Guid operatorID) {
            try {
                string res = "OK";
                res = GlobalData.GlobalData.forceLogoff(PIN, operatorID);
                return res;
            }
            catch (Exception ex) {
                return ex.Message;
            }
        }

        public string changeDrivers(string from, string to, string vehicleID, string modifiedBy)
        {
            TabletInterface ti = new TabletInterface();

            try
            {
                string res = "OK";

                if(from == "null") {
                    //2. get driver PIN
                    TruckService ts = new TruckService();
                    Driver driver = ts.getDrivers().Where(d => d.DriverID == new Guid(to)).FirstOrDefault();

                    //Log On to
                    TabletDriver tbi = ti.DriverAutoLogon(driver.PIN, vehicleID);

                    //3. change VehicleDriver Table
                    GlobalData.SQLCode sql = new GlobalData.SQLCode();
                    sql.updateVehicleDriver(new Guid(to), new Guid(vehicleID), modifiedBy);

                    killVehicle(vehicleID);
                } else {
                    //1. Log out from
                    string tdo = ti.DriverAutoLogoff(from, vehicleID);
                    if (tdo == "LOGGEDOFF")
                    {
                        //2. get driver PIN
                        TruckService ts = new TruckService();
                        Driver driver = ts.getDrivers().Where(d => d.DriverID == new Guid(to)).FirstOrDefault();

                        //Log On to
                        TabletDriver tbi = ti.DriverAutoLogon(driver.PIN, vehicleID);

                        //3. change VehicleDriver Table
                        GlobalData.SQLCode sql = new GlobalData.SQLCode();
                        sql.updateVehicleDriver(new Guid(to), new Guid(vehicleID), modifiedBy);

                        killVehicle(vehicleID);
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string deleteVehicleDriver(string ID)
        {
            string ret = "OK";
            Guid DTVID = new Guid(ID);

            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteVehicleDriver(DTVID);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return ret;
        }

        public string removeDriver(string from, string vehicleID)
        {
            string ret = "OK";
            Guid VID = new Guid(vehicleID);
            TabletInterface ti = new TabletInterface();

            try
            {
                string tdo = ti.DriverAutoLogoff(from, vehicleID);
                if (tdo == "LOGGEDOFF")
                {
                    GlobalData.SQLCode sql = new GlobalData.SQLCode();
                    sql.removeDriver(VID);
                    killVehicle(vehicleID);
                } else {
                    ret = "Can't Log Off";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return ret;
        }

        #endregion

        #region  " Status Information "

        public List<customStatus> getCustomStatuses(Guid companyID) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<customStatus> statList = sql.getCustomStatus(companyID);
                return statList;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void updateCustomStatus(customStatus c) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.updateCustomStatus(c);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Get all status information
        /// </summary>
        /// <returns></returns>
        public List<statusObjectReturn> getAllStatus()
        {
            try
            {
                List<statusObjectReturn> statuses = new List<statusObjectReturn>();
                var statList = from s in GlobalData.GlobalData.statuses
                               join v in GlobalData.GlobalData.vehicles on s.vehicleID equals v.extendedData.ID
                               join d in Users.GlobalUserData.driverList on s.driverID equals d.DriverID
                               select new {
                                   d.DriverLastName,
                                   d.DriverFirstName,
                                   v.VehicleID,
                                   s.statusStart,
                                   s.statusEnd,
                                   s.statusName
                               };
                foreach (var v in statList)
                {
                    statusObjectReturn sr = new statusObjectReturn();
                    sr.DriverName = v.DriverLastName + ", " + v.DriverFirstName;
                    sr.VehicleNumber = v.VehicleID;
                    sr.StatusName = v.statusName;
                    sr.statusStart = v.statusStart;
                    sr.statusEnd = v.statusEnd;
                    statuses.Add(sr);
                }
                statuses = statuses.OrderBy(s => s.VehicleNumber).ThenBy(r => r.statusStart).ToList<statusObjectReturn>();
                return statuses;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// return the status list for a given truck
        /// </summary>
        /// <param name="truckID">Truck Number - NOT THE GUID</param>
        /// <returns></returns>
        public List<statusObjectReturn> getTruckStatus(string truckID)
        {
            try
            {
                List<statusObjectReturn> statuses = new List<statusObjectReturn>();
                var statList = from s in GlobalData.GlobalData.statuses
                               join v in GlobalData.GlobalData.vehicles on s.vehicleID equals v.extendedData.ID
                               join d in Users.GlobalUserData.driverList on s.driverID equals d.DriverID
                               where v.VehicleID == truckID
                               select new {
                                   d.DriverLastName,
                                   d.DriverFirstName,
                                   v.VehicleID,
                                   s.statusStart,
                                   s.statusEnd,
                                   s.statusName
                               };
                foreach (var v in statList)
                {
                    statusObjectReturn sr = new statusObjectReturn();
                    sr.DriverName = v.DriverLastName + ", " + v.DriverFirstName;
                    sr.VehicleNumber = v.VehicleID;
                    sr.StatusName = v.statusName;
                    sr.statusStart = v.statusStart;
                    sr.statusEnd = v.statusEnd;
                    statuses.Add(sr);
                }
                statuses = statuses.OrderBy(s => s.VehicleNumber).ThenBy(r => r.statusStart).ToList<statusObjectReturn>();
                return statuses;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Vars "

        /// <summary>
        /// Gets all types of vars in one big list
        /// </summary>
        /// <returns></returns>
        public List<systemvar> getVars()
        {
            List<systemvar> varList = new List<systemvar>();
            foreach (systemvar v in GlobalData.GlobalData.appVars)
            {
                varList.Add(v);
            }
            foreach (systemvar v in GlobalData.GlobalData.vars)
            {
                varList.Add(v);
            }
            return varList;
        }

        public void updateVar(systemvar v, Guid operatorID)
        {
            try
            {
                switch (v.varType)
                {
                    case 0:
                        GlobalData.GlobalData.updateServiceVar(v, operatorID);
                        break;
                    case 1:
                        GlobalData.GlobalData.updateAppVar(v, operatorID);
                        break;
                    default:
                        throw new Exception("No type specified");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void deleteVar(systemvar v, Guid operatorID)
        {
            try
            {
                switch (v.varType)
                {
                    case 0:
                        GlobalData.GlobalData.deleteServiceVar(v, operatorID);
                        break;
                    case 1:
                        GlobalData.GlobalData.deleteAppVar(v, operatorID);
                        break;
                    default:
                        throw new Exception("No type specified");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public systemvar getSpecificVar(string varName, int type)
        {
            systemvar v = new systemvar();
            systemvar found;
            try
            {
                switch (type)
                {
                    case 0:
                        found = GlobalData.GlobalData.vars.Find(delegate (systemvar find) { return find.varName == varName; });
                        if (found != null)
                        {
                            v.varID = found.varID;
                            v.varName = found.varName;
                            v.varType = found.varType;
                            v.varVal = found.varVal;
                            return v;
                        }
                        break;
                    case 1:
                        found = GlobalData.GlobalData.appVars.Find(delegate (systemvar find) { return find.varName == varName; });
                        if (found != null)
                        {
                            v.varID = found.varID;
                            v.varName = found.varName;
                            v.varType = found.varType;
                            v.varVal = found.varVal;
                            return v;
                        }
                        break;
                    default:
                        throw new Exception("Type not specified");
                }
                if (string.IsNullOrEmpty(v.varName))
                {
                    throw new Exception("Couldn't find requested var");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return v;
        }

        public List<systemvar> getAppVars()
        {
            List<systemvar> varList = new List<systemvar>();
            foreach (systemvar v in GlobalData.GlobalData.appVars)
            {
                varList.Add(v);
            }
            return varList;
        }

        public void updateAppVar(systemvar appVar, Guid operatorID)
        {
            try
            {
                GlobalData.GlobalData.updateAppVar(appVar, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void deleteAppVar(systemvar appVar, Guid operatorID)
        {
            try
            {
                GlobalData.GlobalData.deleteAppVar(appVar, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public List<systemvar> getServiceVars()
        {
            try
            {
                List<systemvar> vars = new List<systemvar>();
                foreach (systemvar v in GlobalData.GlobalData.vars)
                {
                    vars.Add(v);
                }
                return vars;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void updateServiceVar(systemvar v, Guid operatorid)
        {
            try
            {
                GlobalData.GlobalData.updateServiceVar(v, operatorid);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public void deleteServiceVar(systemvar v, Guid operatorID)
        {
            try
            {
                GlobalData.GlobalData.deleteServiceVar(v, operatorID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Dispatches "

        public string dispatchVehicle(dispatch d) {
            string ret = "OK";

            try {
                GlobalData.DispatchData.addDispatch(d);
            }
            catch (Exception ex) {
                ret = ex.ToString();
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        public List<dispatch> getAllDispatches() {
            try {
                return GlobalData.DispatchData.dispatches;
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        public List<dispatch> getDispatchesByVehicle(string vehicleID) {
            List<dispatch> dList = new List<dispatch>();
            try {
                dList = GlobalData.DispatchData.getDispatchesByVehicle(vehicleID);
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
            return dList;
        }

        public dispatch getDispatchesByID(Guid dispatchID)
        {
            dispatch disp = new dispatch();

            try
            {
                disp = GlobalData.DispatchData.getDispatchesByID(dispatchID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return disp;
        }

        public List<dispatch> getDispatchesByRange(DateTime start, DateTime end) {
            List<dispatch> dList = new List<dispatch>();
            try {
                dList = GlobalData.DispatchData.getDispatchesByRange(start, end);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return dList;
        }

        #endregion

        #region " Alarts "

        public List<alertReturn> getAllAlertsByRange(DateTime start, DateTime end) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<alertReturn> alerts = sql.getAlertsByRange(start, end);
                return alerts;
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        public List<alertReturn> getAllAlertsByRangeByType(DateTime start, DateTime end, string type) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<alertReturn> alerts = sql.getAlertsByRangeByType(start, end, type);
                return alerts;
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        public alertReturn getAllAlertByID(Guid ID)
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                alertReturn alert = sql.getAlertByID(ID);
                return alert;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public List<alertReturn> getAllAlertsByRangeByVehicle(DateTime start, DateTime end, string vehicleID) {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<alertReturn> alerts = sql.getAlertsByRangeByVehicle(start, end, vehicleID);
                return alerts;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public List<alertReturn> getAllAlerts() {
            try {
                List<alertReturn> alarts = new List<alertReturn>();
                foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles) {
                    if (v.alerts.Count > 0) {

                        foreach (Models.alert a in v.alerts) {
                            alertReturn ar = new alertReturn();
                            ar.vehicleID = v.VehicleID;
                            ar.alertID = a.alertID;
                            ar.alertType = a.alertType;
                            ar.alertName = a.alertName;
                            ar.alertActive = a.alertActive;
                            ar.alertStart = a.alertStart;
                            ar.alertEnd = a.alertEnd;
                            ar.latLonStart = a.latLonStart;
                            ar.latLonEnd = a.latLonEnd;
                            ar.maxVal = a.maxVal;
                            ar.runID = a.runID;
                            alarts.Add(ar);
                        }
                    }
                }
                return alarts;
            }
            catch (Exception ex) {
                //log the error
                throw new Exception(ex.ToString());
            }

        }

        public List<alertReturn> getAllAlertsByVehicle(string VehicleID)
        {
            try
            {
                List<alertReturn> alarts = new List<alertReturn>();
                var vList = from v in GlobalData.GlobalData.vehicles
                            where v.VehicleID == VehicleID
                            select v;

                foreach (Models.Vehicle v in vList)
                {
                    if (v.alerts.Count > 0)
                    {

                        foreach (Models.alert a in v.alerts)
                        {
                            alertReturn ar = new alertReturn();
                            ar.vehicleID = v.VehicleID;
                            ar.alertID = a.alertID;
                            ar.alertType = a.alertType;
                            ar.alertName = a.alertName;
                            ar.alertActive = a.alertActive;
                            ar.alertStart = a.alertStart;
                            ar.alertEnd = a.alertEnd;
                            ar.latLonStart = a.latLonStart;
                            ar.latLonEnd = a.latLonEnd;
                            ar.maxVal = a.maxVal;
                            ar.runID = a.runID;
                            alarts.Add(ar);
                        }
                    }
                }
                return alarts;
            }
            catch (Exception ex)
            {
                //log the error
                throw new Exception(ex.ToString());
            }

        }

        public List<VehicleGPSRecord> getGPSTracking(string vehicleID, string from, string to)
        {
            List<VehicleGPSRecord> AlertGPS = new List<VehicleGPSRecord>();

            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                AlertGPS = sql.getAlertGPS(vehicleID, from, to);
                return AlertGPS;
            }
            catch (Exception ex)
            {
                //log the error
                throw new Exception(ex.ToString());
            }
        }

        public string clearAlerts(bool clearAll, string vehicleID) {
            string ret = "OK";
            try {
                Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find)
                {
                    return find.VehicleID == vehicleID;
                });
                if (v != null) {
                    if (clearAll) {
                        v.alerts.Clear();
                    }
                    else {
                        for (int i = v.alerts.Count - 1; i >= 0; i--) {
                            if (v.alerts[i].alertActive == false) {
                                v.alerts.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                ret = ex.Message;
            }
            return ret;
        }

        #endregion

        #region " Force Reloads "

        public void reloadVehicles() {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            sql.loadVehicleExtendedData();
            foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles) {
                foreach (VehicleExtendedData ved in GlobalData.GlobalData.vehicleEDs) {
                    if (v.VehicleID == ved.vehicleID) {
                        v.extendedData = ved;
                    }
                }
            }
        }

        #endregion

        #region driverVehicles

        public List<driverVehicleReturn> driverVehicleReturn()
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<driverVehicleReturn> driverToVehicles = sql.getVehicleDrivers();
                foreach(driverVehicleReturn dvr in driverToVehicles)
                {
                    dvr.Vehicle = getAllVehicles(true).Where(v => v.extendedData.ID == dvr.VehicleID).Select(f => f.extendedData).FirstOrDefault();
                    dvr.driver = Users.GlobalUserData.driverList.Where(d => d.DriverID == dvr.DriverID).FirstOrDefault();
                }
                return driverToVehicles;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region OBD Data
        public List<OBDLog> getOBDDataReturnByDateRange(string VehicleID, DateTime from, DateTime to)
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<OBDLog> OBDData = sql.getOBDHistory(new Guid(VehicleID), from, to);
                return OBDData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        #endregion
    }
}

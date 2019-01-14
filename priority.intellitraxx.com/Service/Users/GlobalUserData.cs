using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.Users
{
    public static class GlobalUserData
    {
        public static List<User> userList = new List<User>();
        public static List<Company> companyList = new List<Company>();
        public static List<Role> roleList = new List<Role>();
        public static List<UserRole> userRoleList = new List<UserRole>();
        public static List<UserCompany> userCompanyList = new List<UserCompany>();
        public static List<Driver> driverList = new List<Driver>();

        #region " Add / Update Objects "

        /// <summary>
        /// Verify operator has appropriate permissions
        /// Check to see if a user exits based on userID. If exists, update user object
        /// and update database information
        /// Else add user object to the list and database
        /// </summary>
        /// <param name="u">User Object</param>
        /// <returns>OK if everything worked, else error message</returns>
        public static string addUser(User u, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.updateUser(u);
                User found = userList.Find(delegate (User find) { return find.UserID == u.UserID; });
                if (found == null)
                {
                    userList.Add(u);
                }
                else
                {
                    found.UserLastName = u.UserLastName;
                    found.UserFirstName = u.UserFirstName;
                    found.UserEmail = u.UserEmail;
                    found.UserOffice = u.UserOffice;
                    found.UserPhone = u.UserPhone;
                    found.UserPassword = u.UserPassword;
                    found.UserSalt = u.UserSalt;
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Delete UsersCompanies, Delete UsersRoles, Delete Users
        /// where id = u.UserID;
        /// </summary>
        /// <param name="u">User object</param>
        /// <param name="operatorID">sucker doing the job</param>
        /// <returns></returns>
        public static string deleteUser(User u, Guid operatorID)
        {
            try
            {
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteUser(u.UserID);
                for (int i = userCompanyList.Count - 1; i >= 0; i--)
                {
                    if (userCompanyList[i].UserID == u.UserID)
                    {
                        userCompanyList.RemoveAt(i);
                    }
                }
                for (int i = userRoleList.Count - 1; i >= 0; i--)
                {
                    if (userRoleList[i].UserID == u.UserID)
                    {
                        userRoleList.RemoveAt(i);
                    }
                }
                for (int i = userList.Count - 1; i >= 0; i--)
                {
                    if (userList[i].UserID == u.UserID)
                    {
                        userList.RemoveAt(i);
                    }
                }
                return "OK";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Verify operator has appropriate permissions
        /// Check to see if a company exists based on CompanyID. If exists, verify user object exists
        /// if all good update
        /// if no company exists, add new object
        /// If no user exists, add new object, then add new Company
        /// </summary>
        /// <param name="c">Company Object</param>
        /// <returns>OK if everything worked, else error message</returns>
        public static string addCompany(Company c, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.updateCompany(c);
                Company found = companyList.Find(delegate (Company find) { return find.CompanyID == c.CompanyID; });
                if (found == null)
                {
                    companyList.Add(c);
                }
                else
                {
                    found.CompanyName = c.CompanyName;
                    found.CompanyAddress = c.CompanyAddress;
                    found.CompanyCity = c.CompanyCity;
                    found.CompanyState = c.CompanyState;
                    found.CompanyCountry = c.CompanyCountry;
                    found.isParent = c.isParent;
                    found.CompanyContact = c.CompanyContact;
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// delete a company from the database and the service
        /// 
        /// start by removing relationships
        /// then delete compnay object
        /// 
        /// </summary>
        /// <param name="c">Company to be deleted</param>
        /// <param name="operatorID"></param>
        public static string deleteCompany(Company c, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteCompany(c.CompanyID);
                for (int i = userCompanyList.Count - 1; i >= 0; i--)
                {
                    if (userCompanyList[i].CompanyID == c.CompanyID)
                    {
                        userCompanyList.RemoveAt(i);
                    }
                }
                for (int i = companyList.Count - 1; i >= 0; i--)
                {
                    if (companyList[i].CompanyID == c.CompanyID)
                    {
                        companyList.RemoveAt(i);
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Verify operator permissions, add or update role information
        /// </summary>
        /// <param name="r">Role Object</param>
        /// <param name="operatorID"></param>
        /// <returns>OK if everything worked, else error message</returns>
        public static string addRole(Role r, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.updateRole(r);
                Role found = roleList.Find(delegate (Role find) { return find.RoleID == r.RoleID; });
                if (found == null)
                {
                    roleList.Add(r);
                }
                else
                {
                    found.roleName = r.roleName;
                    found.roleDescription = r.roleDescription;
                    found.isAdmin = r.isAdmin;
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// delete a role from database, then from the service
        /// </summary>
        /// <param name="r">role to be removed</param>
        /// <param name="operatorID">ID of person doing the operation</param>
        /// <returns></returns>
        public static string deleteRole(Role r, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteRole(r.RoleID);
                for (int i = userRoleList.Count - 1; i >= 0; i--)
                {
                    if (userRoleList[i].RoleID == r.RoleID)
                    {
                        userRoleList.RemoveAt(i);
                    }
                }
                for (int i = roleList.Count - 1; i >= 0; i--)
                {
                    if (roleList[i].RoleID == r.RoleID)
                    {
                        roleList.RemoveAt(i);
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Add a new driver to the system
        /// </summary>
        /// <param name="d">Driver object</param>
        /// <param name="operatorID">Person performing the operation needs admin rights</param>
        /// <returns></returns>
        public static string addDriver(Driver d, Guid operatorID)
        {
            string ret = "OK";
            try
            {
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                Driver found = driverList.Find(delegate (Driver find) { return find.DriverID == d.DriverID; });
                if (found == null)
                {
                    driverList.Add(d);
                }
                else
                {
                    found.DriverLastName = d.DriverLastName;
                    found.DriverFirstName = d.DriverFirstName;
                    found.CompanyID = d.CompanyID;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return ret;
        }

        /// <summary>
        /// remove a driver from the system
        /// </summary>
        /// <param name="driverID">driver id to be removed</param>
        /// <param name="operatorID">id of person requestion deletion, must be admin</param>
        /// <returns></returns>
        public static string deleteDriver(Guid driverID, Guid operatorID)
        {
            string ret = "OK";
            try
            {
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                for (int i = driverList.Count - 1; i >= 0; i--)
                {
                    if (driverList[i].DriverID == driverID)
                    {
                        driverList.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return ret;
        }

        #endregion

        #region " Associate Objects "

        /// <summary>
        /// Associate a uers to a role
        /// </summary>
        /// <param name="userID">User to associate</param>
        /// <param name="roleID">Role to assocaite</param>
        /// <param name="operatorID">User performing operation</param>
        /// <returns>OK if everything worked, else error messaage</returns>
        public static string addUserToRole(Guid userID, Guid roleID, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.setRoleMembership(userID, roleID, false);
                UserRole found = userRoleList.Find(delegate (UserRole find) { return find.UserID == userID && find.RoleID == roleID; });
                if (found == null)
                {
                    UserRole ur = new UserRole();
                    ur.UserID = userID;
                    ur.RoleID = roleID;
                    userRoleList.Add(ur);
                }
                else
                {
                    ret = "User is already a member";
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Remove a user from a role
        /// </summary>
        /// <param name="userID">User id of object to remove</param>
        /// <param name="roleID">role id of role to be removed from</param>
        /// <param name="operatorID">id of operator performing operation</param>
        /// <returns></returns>
        public static string removeUserFromRole(Guid userID, Guid roleID, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                for (int i = userRoleList.Count - 1; i >= 0; i--)
                {
                    if (userRoleList[i].UserID == userID && userRoleList[i].RoleID == roleID)
                    {
                        GlobalData.SQLCode sql = new GlobalData.SQLCode();
                        sql.setRoleMembership(userID, roleID, true);
                        userRoleList.RemoveAt(i);
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Associate user to company
        /// </summary>
        /// <param name="userID">User to associate</param>
        /// <param name="companyID">company to associate</param>
        /// <param name="operatorID">user performing operation</param>
        /// <returns>OK if everything worked, else error message</returns>
        public static string addUserToCompany(Guid userID, Guid companyID, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.setCompanyMembership(userID, companyID, false);
                UserCompany found = userCompanyList.Find(delegate (UserCompany find) { return find.UserID == userID && find.CompanyID == companyID; });
                if (found == null)
                {
                    UserCompany uc = new UserCompany();
                    uc.UserID = userID;
                    uc.CompanyID = companyID;
                    userCompanyList.Add(uc);
                }
                else
                {
                    throw new Exception("User is already a member of that company");
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// remove a user from a company
        /// </summary>
        /// <param name="userID">user Id to be removed</param>
        /// <param name="companyID">company id to remove from</param>
        /// <param name="oeratorID">operator performing operation</param>
        /// <returns>OK if ok, else error message</returns>
        public static string removeUserFromCompany(Guid userID, Guid companyID, Guid operatorID)
        {
            try
            {
                string ret = "OK";
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.setCompanyMembership(userID, companyID, true);
                for (int i = userCompanyList.Count - 1; i >= 0; i--)
                {
                    if (userCompanyList[i].UserID == userID && userCompanyList[i].CompanyID == companyID)
                    {
                        userCompanyList.RemoveAt(i);

                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " Drivers add/update/delete "

        public static string updateDriver(Driver d, Guid operatorID)
        {
            string ret = "OK";

            try
            {
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.updateDriver(d);
                Driver found = driverList.Find(delegate (Driver find) { return find.DriverID == d.DriverID; });
                if (found != null)
                {
                    found.DriverLastName = d.DriverLastName;
                    found.DriverFirstName = d.DriverFirstName;
                    found.CompanyID = d.CompanyID;
                    found.DriverPassword = d.DriverPassword;
                    found.DriverEmail = d.DriverEmail;
                    found.ProfilePic = d.ProfilePic;
                    found.DriverNumber = d.DriverNumber;
                    found.imageData = d.imageData;
                    found.imageType = d.imageType;
                    found.PIN = d.PIN;
                }
                else
                {
                    driverList.Add(d);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        public static string deleteDriver(Driver d, Guid operatorID)
        {
            string ret = "OK";

            try
            {
                bool admin = GlobalData.Helpers.isAdmin(operatorID);
                if (!admin)
                {
                    throw new Exception("User is not an administrator, operation cancelled");
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteDriver(d.DriverID);
                for (int i = driverList.Count - 1; i >= 0; i--)
                {
                    if (driverList[i].DriverID == d.DriverID)
                    {
                        driverList.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        #endregion

        #region " Force Driver Logoff "



        #endregion

    }
}
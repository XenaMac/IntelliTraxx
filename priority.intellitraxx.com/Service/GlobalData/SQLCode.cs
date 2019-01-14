using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace LATATrax.GlobalData
{
    public class SQLCode
    {
        #region  " Connection Strings "

        private string getConn()
        {
            return ConfigurationManager.AppSettings["db"].ToString();
        }

        #endregion

        #region " Data Updates "

        #region  " Geo Fences "
        public void setGeoFence(Models.polygonData pd, string userEmail)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    //First grab the user id based on the incoming email address
                    SqlCommand cmd = new SqlCommand("FindUserByEmail", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmailAddress", userEmail);
                    string UserID = cmd.ExecuteScalar().ToString();
                    cmd = null;
                    //now fire the geo fence update code. The database will take care of
                    //determining if this is an update or an addition
                    cmd = new SqlCommand("UpdateGeoFence", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    string geoFence = string.Empty;
                    foreach (Models.LatLon ll in pd.geoFence)
                    {
                        geoFence += ll.Lat.ToString() + "," + ll.Lon.ToString() + "," + ll.Alt.ToString() + "|";
                    }
                    geoFence = geoFence.Substring(0, geoFence.Length - 1);
                    if (string.IsNullOrEmpty(pd.geoType))
                    {
                        pd.geoType = "polygon";
                    }
                    cmd.Parameters.AddWithValue("@GeoFenceID", pd.geoFenceID);
                    cmd.Parameters.AddWithValue("@GeoName", pd.polyName);
                    cmd.Parameters.AddWithValue("@GeoNotes", pd.notes);
                    cmd.Parameters.AddWithValue("@GeoFence", geoFence);
                    cmd.Parameters.AddWithValue("@UserCreated", UserID);
                    cmd.Parameters.AddWithValue("@UserModified", UserID);
                    cmd.Parameters.AddWithValue("@GeoType", pd.geoType);
                    cmd.Parameters.AddWithValue("@Radius", pd.radius);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                /*
                Logger logger = new Logger("SQLERRORS.txt");
                logger.writeToLogFile("setGeoFence ERROR: " + ex.ToString());
                */
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// delete a polygon from the database
        /// </summary>
        /// <param name="geoFenceID"></param>
        public void deleteGeoFence(Guid geoFenceID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteGeoFence", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@GeoFenceID", geoFenceID);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " User/Company/Role "

        #region " Users Modification "

        /// <summary>
        /// Update the user information in the database based on UserID
        /// If the User does not exist it gets created, otherwise it will be updated
        /// </summary>
        /// <param name="u"></param>
        public void updateUser(User u)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "updateUser";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", u.UserID);
                    cmd.Parameters.AddWithValue("@UserLastName", u.UserLastName);
                    cmd.Parameters.AddWithValue("@UserFirstName", u.UserFirstName);
                    cmd.Parameters.AddWithValue("@UserEmail", u.UserEmail);
                    cmd.Parameters.AddWithValue("@UserOffice", u.UserOffice);
                    cmd.Parameters.AddWithValue("@UserPhone", u.UserPhone);
                    cmd.Parameters.AddWithValue("@UserPassword", u.UserPassword);
                    cmd.Parameters.AddWithValue("@UserSalt", u.UserSalt);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Delete UsersCompanies
        /// Delete UsersRoles
        /// Delete Users
        /// use UserID
        /// </summary>
        /// <param name="userID">UserID to be deleted</param>
        public void deleteUser(Guid userID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "DeleteUser";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Company Modifications "

        /// <summary>
        /// update the company information in the database based on CompanyId
        /// if the company does not exist it is created, else updated
        /// </summary>
        /// <param name="c"></param>
        public void updateCompany(Company c)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "updateCompany";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", c.CompanyID);
                    cmd.Parameters.AddWithValue("@CompanyName", c.CompanyName);
                    cmd.Parameters.AddWithValue("@CompanyAddress", c.CompanyAddress);
                    cmd.Parameters.AddWithValue("@CompanyCity", c.CompanyCity);
                    cmd.Parameters.AddWithValue("@CompanyState", c.CompanyState);
                    cmd.Parameters.AddWithValue("@CompanyCountry", c.CompanyCountry);
                    cmd.Parameters.AddWithValue("@IsParent", c.isParent);
                    cmd.Parameters.AddWithValue("@Contact", c.CompanyContact.UserID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Delete any User-Company associations in UsersCompany
        /// Delete comapny records
        /// all based on CompanyID
        /// </summary>
        /// <param name="companyID">Company object to be deleted</param>
        public void deleteCompany(Guid companyID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "DeleteCompany";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", companyID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// set or remove user->company relationship.
        /// if remove = true, the user is removed, else inserted
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="companyID"></param>
        /// <param name="Remove"></param>
        public void setCompanyMembership(Guid userID, Guid companyID, bool Remove)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "setCompanyMembership";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@CompanyID", companyID);
                    cmd.Parameters.AddWithValue("@Remove", Remove);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Roles "

        /// <summary>
        /// update the role information in the database,
        /// if the role does not exist it is created
        /// </summary>
        /// <param name="r"></param>
        public void updateRole(Role r)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "updateRole";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", r.RoleID);
                    cmd.Parameters.AddWithValue("@RoleName", r.roleName);
                    cmd.Parameters.AddWithValue("@RoleDescription", r.roleDescription);
                    cmd.Parameters.AddWithValue("@IsAdmin", r.isAdmin);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Delete users roles
        /// delete roles
        /// based on role id
        /// </summary>
        /// <param name="RoleID"></param>
        public void deleteRole(Guid roleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "DeleteRole";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RoleID", roleID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// Update the role memebership in the database
        /// if remove = true, the row is removed, else inserted
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="roleID"></param>
        /// <param name="Remove"></param>
        public void setRoleMembership(Guid userID, Guid roleID, bool Remove)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "setRoleMembership";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.Parameters.AddWithValue("@RoleID", roleID);
                    cmd.Parameters.AddWithValue("@Remove", Remove);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Vehicles "

        public void createVehicleFromTablet(string vehicleID, string MACAddress, string vehicleFriendlyName, string licensePlate)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "MakeVehicle";

                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                    cmd.Parameters.AddWithValue("@LicensePlate", licensePlate);
                    cmd.Parameters.AddWithValue("@VehicleFriendlyName", vehicleFriendlyName);
                    cmd.Parameters.AddWithValue("@RouterID", MACAddress);
                    cmd.Parameters.AddWithValue("@MACAddress", MACAddress);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void updateVehicleExtendedData(VehicleExtendedData v)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "UpdateVehicle";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", v.ID);
                    cmd.Parameters.AddWithValue("@VehicleClassID", v.vehicleClassID);
                    cmd.Parameters.AddWithValue("@LicensePlate", v.licensePlate);
                    cmd.Parameters.AddWithValue("@Make", v.Make);
                    cmd.Parameters.AddWithValue("@Model", v.Model);
                    cmd.Parameters.AddWithValue("@Year", v.Year);
                    cmd.Parameters.AddWithValue("@HaulLimit", v.haulLimit);
                    cmd.Parameters.AddWithValue("@VehicleFriendlyName", v.VehicleFriendlyName);
                    cmd.Parameters.AddWithValue("@VehicleID", v.vehicleID);
                    cmd.Parameters.AddWithValue("@RouterID", v.RouterID);
                    cmd.Parameters.AddWithValue("@AddMacAddress", v.MACAddress);
                    cmd.Parameters.AddWithValue("@CompanyID", v.companyID);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void deleteVehicleExtendedData(Guid id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL3 = "DELETE AlertsVehicles WHERE VehicleID = '" + id + "'";
                    SqlCommand cmd3 = new SqlCommand(SQL3, conn);
                    cmd3.ExecuteNonQuery();

                    string SQL2 = "DELETE VehiclesCompanies WHERE VehicleID = '" + id + "'";
                    SqlCommand cmd2 = new SqlCommand(SQL2, conn);
                    cmd2.ExecuteNonQuery();

                    string SQL = "DELETE Vehicles WHERE ID = '" + id + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Drivers Add/Update/Delete"

        public void updateDriver(Driver d)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    if (string.IsNullOrEmpty(d.imageType))
                    {
                        d.imageType = "NA";
                    }
                    if (d.imageData == null) //no image data but we still need to throw something in the db. At the stored proc if this is less than 10, it won't be added
                    {
                        d.imageData = new byte[1];
                    }
                    string SQL = "updateDriver";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DriverID", d.DriverID);
                    cmd.Parameters.AddWithValue("@DriverLastName", d.DriverLastName);
                    cmd.Parameters.AddWithValue("@DriverFirstName", d.DriverFirstName);
                    cmd.Parameters.AddWithValue("@CompanyID", d.CompanyID);
                    cmd.Parameters.AddWithValue("@DriverPassword", d.DriverPassword);
                    cmd.Parameters.AddWithValue("@ProfilePic", d.ProfilePic);
                    cmd.Parameters.AddWithValue("@DriverEmail", d.DriverEmail);
                    cmd.Parameters.AddWithValue("@DriverNumber", d.DriverNumber);
                    cmd.Parameters.AddWithValue("@ImageFile", d.imageData);
                    cmd.Parameters.AddWithValue("@ImageType", d.imageType);
                    cmd.Parameters.AddWithValue("@DriverPIN", d.PIN);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void deleteDriver(Guid id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "DELETE Drivers WHERE DriverID = '" + id + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Vehicle Classes Add/Update/Delete"

        public void updateVehicleClass(VehicleClass v)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "updateVehicleClass";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleClassID", v.VehicleClassID);
                    cmd.Parameters.AddWithValue("@VehicleClassName", v.VehicleClassName);
                    cmd.Parameters.AddWithValue("@VehicleClassDescription", v.VehicleClassDescription);
                    cmd.Parameters.AddWithValue("@VehicleClassImage", v.VehicleClassImage);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void deleteVehicleClass(Guid id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "DELETE VehicleClasses WHERE VehicleClassID = '" + id + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Vehicle Extended Data "

        public void loadVehicleExtendedData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    GlobalData.vehicleEDs.Clear();

                    string SQL = "loadVehicleExtendedData";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        VehicleExtendedData vd = new VehicleExtendedData();
                        vd.ID = new Guid(rdr["ID"].ToString());
                        vd.vehicleID = rdr["VehicleID"].ToString();
                        vd.vehicleClass = rdr["VehicleClassName"].ToString();
                        vd.vehicleClassImage = rdr["VehicleClassImage"].ToString();
                        vd.vehicleClassID = rdr.IsDBNull(2) ? new Guid() : new Guid(rdr["VehicleClassID"].ToString());
                        vd.companyID = rdr.IsDBNull(3) ? new Guid() : new Guid(rdr["CompanyID"].ToString());
                        vd.companyName = rdr.IsDBNull(4) ? "NA" : rdr["CompanyName"].ToString();
                        vd.licensePlate = rdr["LicensePlate"].ToString();
                        vd.Make = rdr["Make"].ToString();
                        vd.Model = rdr["VehicleModel"].ToString();
                        vd.Year = rdr.IsDBNull(10) ? 0 : Convert.ToInt32(rdr["Year"]);
                        vd.haulLimit = rdr.IsDBNull(11) ? 0 : Convert.ToInt32(rdr["HaulLimit"]);
                        vd.VehicleFriendlyName = rdr["VehicleFriendlyName"].ToString();
                        vd.RouterID = rdr["RouterID"].ToString();
                        vd.MACAddress = rdr["MACAddress"].ToString();
                        GlobalData.vehicleEDs.Add(vd);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        //specific to DispatchData.cd
        //creates a vehicle extended data object that's used to get the routerid and macaddress
        //for a dispatch object when the vehicle isn't currently connected to the system.
        public VehicleExtendedData loadVehicleExtendedData(string vehicleID)
        {
            try
            {
                VehicleExtendedData vd = null;
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    GlobalData.vehicleEDs.Clear();

                    string SQL = "loadVehicleExtendedDataByVehicle";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vehicleID", vehicleID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        vd = new VehicleExtendedData();
                        vd.ID = new Guid(rdr["ID"].ToString());
                        vd.vehicleID = rdr["VehicleID"].ToString();
                        vd.vehicleClass = rdr["VehicleClassName"].ToString();
                        vd.vehicleClassImage = rdr["VehicleClassImage"].ToString();
                        vd.vehicleClassID = rdr.IsDBNull(2) ? new Guid() : new Guid(rdr["VehicleClassID"].ToString());
                        vd.companyID = rdr.IsDBNull(3) ? new Guid() : new Guid(rdr["CompanyID"].ToString());
                        vd.companyName = rdr.IsDBNull(4) ? "NA" : rdr["CompanyName"].ToString();
                        vd.licensePlate = rdr["LicensePlate"].ToString();
                        vd.Make = rdr["Make"].ToString();
                        vd.Model = rdr["VehicleModel"].ToString();
                        vd.Year = Convert.ToInt32(rdr["Year"]);
                        vd.haulLimit = Convert.ToInt32(rdr["HaulLimit"]);
                        vd.VehicleFriendlyName = rdr["VehicleFriendlyName"].ToString();
                        vd.RouterID = rdr["RouterID"].ToString();
                        vd.VehicleFriendlyName = rdr["VehicleFriendlyName"].ToString();
                        vd.MACAddress = rdr["MACAddress"].ToString();
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
                return vd;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #endregion

        #region " Log GPS Data "

        public void logGPS(Models.Vehicle v)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "LogGPS";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", v.VehicleID);
                    cmd.Parameters.AddWithValue("@Direction", v.gps.dir);
                    cmd.Parameters.AddWithValue("@Speed", v.gps.spd);
                    cmd.Parameters.AddWithValue("@Lat", v.gps.lat);
                    cmd.Parameters.AddWithValue("@Lon", v.gps.lon);
                    cmd.Parameters.AddWithValue("@InPolygon", v.isInside);
                    if (v.isInside)
                    {
                        cmd.Parameters.AddWithValue("@PolyName", v.insidePolyName.polyName);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@PolyName", "N/A");
                    }
                    cmd.Parameters.AddWithValue("@runID", v.runID);
                    cmd.Parameters.AddWithValue("@lastMessageReceived", v.lastMessageReceived);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void logLKP(Models.Vehicle v)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "LogLKP";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", v.VehicleID);
                    cmd.Parameters.AddWithValue("@Direction", v.gps.dir);
                    cmd.Parameters.AddWithValue("@Speed", v.gps.spd);
                    cmd.Parameters.AddWithValue("@Lat", v.gps.lat);
                    cmd.Parameters.AddWithValue("@Lon", v.gps.lon);
                    cmd.Parameters.AddWithValue("@InPolygon", v.isInside);
                    if (v.isInside)
                    {
                        cmd.Parameters.AddWithValue("@PolyName", v.insidePolyName.polyName);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@PolyName", "N/A");
                    }
                    cmd.Parameters.AddWithValue("@runID", v.runID);
                    cmd.Parameters.AddWithValue("@lastMessageReceived", v.lastMessageReceived);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void logMissingGPS(Models.Vehicle v, DateTime dt, Guid loggedRunID, Messages.MissingGPSMessage mgps, bool isInside, string polyName)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "LogMissingGPS";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", v.VehicleID);
                    cmd.Parameters.AddWithValue("@Direction", mgps.dir);
                    cmd.Parameters.AddWithValue("@Speed", mgps.spd);
                    cmd.Parameters.AddWithValue("@Lat", mgps.lat);
                    cmd.Parameters.AddWithValue("@Lon", mgps.lon);
                    cmd.Parameters.AddWithValue("@InPolygon", isInside);
                    cmd.Parameters.AddWithValue("@PolyName", polyName);
                    cmd.Parameters.AddWithValue("@runID", loggedRunID);
                    cmd.Parameters.AddWithValue("@lastMessageReceived", dt.ToUniversalTime());
                    cmd.Parameters.AddWithValue("@messageTime", dt);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Log Alert "

        public void updateAlert(dbAlert am)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "updateAlert";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlertID", am.AlertID);
                    cmd.Parameters.AddWithValue("@AlertActive", am.AlertActive);
                    cmd.Parameters.AddWithValue("@AlertStartDate", am.AlertStartTime);
                    cmd.Parameters.AddWithValue("@AlertEndDate", am.AlertEndTime);
                    cmd.Parameters.AddWithValue("@AlertType", am.AlertType);
                    cmd.Parameters.AddWithValue("@AlertClassID", am.AlertClassID);
                    cmd.Parameters.AddWithValue("@AlertFriendlyName", am.AlertFriendlyName);
                    cmd.Parameters.AddWithValue("@minVal", am.minVal);
                    cmd.Parameters.AddWithValue("@NDB", am.NDB);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public void updateAlertsVehicles(dbAlert a, List<alertVehicle> avList)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "resetAlertsVehicles"; //this clears the linkup for any vehicles using this alert id
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlertID", a.AlertID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    foreach (alertVehicle av in avList)
                    { //this rebuilds the linkup with the new information
                        SQL = "updateAlertsVehicles";
                        cmd = new SqlCommand(SQL, conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AlertID", a.AlertID);
                        cmd.Parameters.AddWithValue("@vehicleID", av.VehicleID);
                        cmd.Parameters.AddWithValue("@alertAction", av.AlertAction);
                        cmd.ExecuteNonQuery();
                        cmd = null;
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public void updateAlertsGeoFences(dbAlert a, List<alertGeoFence> agfList)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "resetAlertsGeoFences";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlertID", a.AlertID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    foreach (alertGeoFence agf in agfList)
                    {
                        SQL = "updateAlertsGeoFences";
                        cmd = new SqlCommand(SQL, conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AlertID", agf.AlertID);
                        cmd.Parameters.AddWithValue("@GeoFenceID", agf.GeoFenceID);
                        cmd.ExecuteNonQuery();
                        cmd = null;
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public void deleteAlert(dbAlert a)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("deleteAlert", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlertID", a.AlertID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public void logAlert(Models.alert a, string vehicleID)
        {

            #region check for nulls

            if(a.alertEnd == null)
            {
                a.alertEnd = DateTime.Now;
            }

            if (a.alertID == null)
            {
                a.alertID = Guid.NewGuid();
            }

            if (a.alertName == null)
            {
                a.alertName = "NULL ALERT";
            }
            
            if (a.alertStart == null)
            {
                a.alertStart = DateTime.Now;
            }

            if (a.alertType == null)
            {
                a.alertType = "NULL";
            }

            if (a.latLonEnd == null)
            {
                a.latLonEnd = "NULL";
            }

            if (a.latLonStart == null)
            {
                a.latLonStart = "NULL";
            }

            if (a.maxVal == null)
            {
                a.maxVal = "NULL";
            }

            if (a.runID == null)
            {
                a.runID = Guid.NewGuid();
            }

            #endregion

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "logAlert";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlertID", a.alertID);
                    cmd.Parameters.AddWithValue("@AlertName", a.alertName);
                    cmd.Parameters.AddWithValue("@AlertType", a.alertType);
                    cmd.Parameters.AddWithValue("@AlertStart", a.alertStart);
                    cmd.Parameters.AddWithValue("@AlertEnd", a.alertEnd);
                    cmd.Parameters.AddWithValue("@VehicleID", vehicleID ?? "NULL");
                    cmd.Parameters.AddWithValue("@LatLonStart", a.latLonStart ?? "NULL");
                    cmd.Parameters.AddWithValue("@LatLonEnd", a.latLonEnd ?? "NULL");
                    cmd.Parameters.AddWithValue("@MaxVal", a.maxVal ?? "NULL");
                    cmd.Parameters.AddWithValue("@runID", a.runID);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                string errorString = null;
                foreach (var prop in a.GetType().GetProperties())
                {
                    errorString += prop.Name + " = " + prop.GetValue(a, null) + "\r\n";
                }

                logError(ex.ToString() + errorString + " CablePS");
            }
        }

        public void changeAlertStatus(List<Guid> aList, bool enable)
        { //enable/disable alerts on the fly
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd;
                    foreach (Guid g in aList)
                    {
                        cmd = new SqlCommand("updateAlertStatus", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AlertID", g);
                        cmd.Parameters.AddWithValue("@AlertActive", enable);
                        cmd.ExecuteNonQuery();
                        cmd = null;
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        #endregion

        #region  " Driver Behavior "

        public void logDriverBehavior(driverBehavior d, Guid vehicleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("logBehavior", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                    cmd.Parameters.AddWithValue("@DriverID", d.driverID);
                    cmd.Parameters.AddWithValue("@Behavior", d.behavior);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        #region " OBD Data "

        public void logOBDData(OBD2Data o, Guid vehicleID, Guid runID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("logOBD", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                    cmd.Parameters.AddWithValue("@RunID", runID);
                    cmd.Parameters.AddWithValue("@Name", o.name);
                    cmd.Parameters.AddWithValue("@Val", o.val);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        #endregion

        #endregion

        #region " Vars "

        public void updateVar(systemvar v)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "updateVar";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", v.varID);
                    cmd.Parameters.AddWithValue("@varName", v.varName);
                    cmd.Parameters.AddWithValue("@varValue", v.varVal);
                    cmd.Parameters.AddWithValue("@varType", v.varType);
                    cmd.Parameters.AddWithValue("@minValue", v.minValue);
                    cmd.Parameters.AddWithValue("@Email", v.Email);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void deleteVar(systemvar v)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "deleteVar";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", v.varID);
                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region  " Status Updates "

        public void updateCustomStatus(customStatus c) {
            try {
                using (SqlConnection conn = new SqlConnection(getConn())) {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("updateCustomStatus", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleStatusID", c.customStatusID);
                    cmd.Parameters.AddWithValue("@VehicleStatusName", c.vehicleStatusName);
                    cmd.Parameters.AddWithValue("@VehicleStatusDescription", c.vehicleStatusDescription);
                    cmd.Parameters.AddWithValue("@VehicleStatusColor", c.vehicleStatusColor);
                    cmd.Parameters.AddWithValue("@CompanyID", c.companyID);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void updateStatus(statusObject so)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("updateStatus", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", so.statusID);
                    cmd.Parameters.AddWithValue("@DriverID", so.driverID);
                    cmd.Parameters.AddWithValue("@VehicleID", so.vehicleID);
                    cmd.Parameters.AddWithValue("@StatusStart", so.statusStart.ToString());
                    cmd.Parameters.AddWithValue("@StatusName", so.statusName);
                    cmd.Parameters.AddWithValue("@RunID", so.runID);
                    if (so.statusEnd != null)
                    {
                        cmd.Parameters.AddWithValue("@StatusEnd", so.statusEnd.ToString());
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@StatusEnd", "NA"); //ths will null the status end field in teh db
                    }
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Dispatches "

        public void logDispatchData(dispatch d)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "LogDispatch";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", d.ID);
                    cmd.Parameters.AddWithValue("@timeStamp", d.timeStamp);
                    cmd.Parameters.AddWithValue("@userEmail", d.UserEmail);
                    cmd.Parameters.AddWithValue("@vehicleID", d.vehicleID);
                    cmd.Parameters.AddWithValue("@runID", d.runID);
                    cmd.Parameters.AddWithValue("@address", d.address);
                    cmd.Parameters.AddWithValue("@city", d.city);
                    cmd.Parameters.AddWithValue("@state", d.state);
                    cmd.Parameters.AddWithValue("@zip", d.zip);
                    cmd.Parameters.AddWithValue("@note", d.note);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void ackDispatch(dispatch d)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "ackDispatch";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", d.ID);
                    cmd.Parameters.AddWithValue("@ackTime", d.ackTime);
                    cmd.Parameters.AddWithValue("@ackMessage", d.ackMessage);
                    cmd.Parameters.AddWithValue("@DriverPIN", d.driverPIN);
                    cmd.Parameters.AddWithValue("@RunID", d.runID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void closeDispatch(dispatch d)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "closeDispatch";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", d.ID);
                    cmd.Parameters.AddWithValue("@completedMessage", d.completedMessage);
                    cmd.Parameters.AddWithValue("@completedTime", d.completedTime);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Schedules "

        //this is primarily to update a single vehicles, currently unused
        public void updateSchedule(string vehicleID, DateTime dtStart, DateTime dtEnd)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("updateSchedule", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                    cmd.Parameters.AddWithValue("@dtstartTime", dtStart);
                    cmd.Parameters.AddWithValue("@dtendTime", dtEnd);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        //get a complete list of all the schedules in the database. may need to be expanded to allow
        //only getting active schedules or only schedules for a particular company
        public List<schedule> getAllSchedules()
        {
            List<schedule> sList = new List<schedule>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("GetAllSchedules", conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        schedule s = new schedule();
                        s.scheduleID = new Guid(rdr["ScheduleID"].ToString());
                        s.scheduleName = rdr["ScheduleName"].ToString();
                        s.companyid = new Guid(rdr["CompanyID"].ToString());
                        s.startTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["StartTime"].ToString());
                        s.endTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["EndTime"].ToString());
                        s.createdBy = rdr["CreatedBy"].ToString();
                        s.createdOn = Convert.ToDateTime(rdr["CreatedOn"]);
                        s.modifiedBy = rdr["ModifiedBy"].ToString();
                        s.modifiedOn = Convert.ToDateTime(rdr["ModifiedOn"]);
                        s.DOW = Convert.ToInt32(rdr["DOW"]);
                        s.EffDtStart = Convert.ToDateTime(rdr["EffDtStart"]);
                        s.EffDtEnd = Convert.ToDateTime(rdr["EffDtEnd"]);
                        s.active = Convert.ToBoolean(rdr["Active"]);
                        sList.Add(s);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
                return sList;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        //completely remove a schedule from the system
        public void deleteSchedule(Guid scheduleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("deleteSchedule", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@scheduleID", scheduleID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        //remove links in VehiclesSchedules, do not delete schedules or vehicles
        //may or may not be useful in production, but I can see a case where it
        //might come in handy to completely remove all linked up schedules/vehicles
        //for a given schedule so it can all be rebuilt
        public void deleteSchedulesBySchedule(Guid scheduleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("deleteSchedulesBySchedule", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@scheduleID", scheduleID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }
        }

        //remove links in VehiclesSchedules, do not delete schedules or vehicles
        //may or may not be useful in production, but I can see a case where it
        //might come in handy to completely remove all linked up schedules/vehicles
        //for a given schedule so it can all be rebuilt
        public void deleteSchedulesByVehicle(string vehicleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("deleteSchedulesByVehicle", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vehicleID", vehicleID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.Message);
            }
        }

        public void updateSchedules(List<schedule> sList)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    foreach (schedule s in sList)
                    {
                        SqlCommand cmd = new SqlCommand("UpdateSchedules", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ScheduleID", s.scheduleID);
                        cmd.Parameters.AddWithValue("@ScheduleName", s.scheduleName);
                        cmd.Parameters.AddWithValue("@CompanyID", s.companyid);
                        cmd.Parameters.AddWithValue("@StartDt", s.startTime);
                        cmd.Parameters.AddWithValue("@EndDt", s.endTime);
                        cmd.Parameters.AddWithValue("@UserName", s.modifiedBy); //usually for a new schedule, createdby and modifiedby will be the same. may be diff for updates
                        cmd.Parameters.AddWithValue("@EffStartDt", s.EffDtStart);
                        cmd.Parameters.AddWithValue("@EffEndDt", s.EffDtEnd);
                        cmd.Parameters.AddWithValue("@Active", s.active);
                        cmd.Parameters.AddWithValue("@DOW", s.DOW);
                        cmd.ExecuteNonQuery();
                        cmd = null;
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public List<vsLink> getAllSchedulesByVehicle(string vehicleID)
        {
            try
            {
                List<vsLink> vsList = new List<vsLink>();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "GetAllSchedulesByVehicle";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        vsLink vl = new vsLink();
                        vl.scheduleName = rdr["ScheduleName"].ToString();
                        vl.startTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["StartTime"]);
                        vl.endTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["EndTime"]);
                        vl.vehicleID = rdr["VehicleID"].ToString();
                        vl.vID = new Guid(rdr["ID"].ToString());
                        vsList.Add(vl);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
                return vsList;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public List<vsLink> getAllVehiclesBySchedule(Guid scheduleID)
        {
            try
            {
                List<vsLink> vsList = new List<vsLink>();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "GetAllVehiclesBySchedule";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ScheduleID", scheduleID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        vsLink vl = new vsLink();
                        vl.scheduleName = rdr["ScheduleName"].ToString();
                        vl.startTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["StartTime"]);
                        vl.endTime = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["EndTime"]);
                        vl.vehicleID = rdr["VehicleID"].ToString();
                        vl.vID = new Guid(rdr["ID"].ToString());
                        vsList.Add(vl);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
                return vsList;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public void addVehicleToSchedule(Guid scheduleID, List<string> vehicleList)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("deleteSchedulesBySchedule", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ScheduleID", scheduleID);
                    cmd.ExecuteNonQuery();
                    cmd = null;
                    foreach (string s in vehicleList)
                    {
                        cmd = new SqlCommand("addVehicleToSchedule", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ScheduleID", scheduleID);
                        cmd.Parameters.AddWithValue("@VehicleID", s);
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public void addScheduleToVehicle(string vehicleID, List<Guid> sList)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("deleteSchedulesByVehicle", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    foreach (Guid g in sList)
                    {
                        cmd = new SqlCommand("addVehicleToSchedule", conn);
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ScheduleID", g);
                        cmd.Parameters.AddWithValue("@VehicleID", vehicleID);
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        #endregion

        #region " Drivers "

        public void updateVehicleDriver(Guid to, Guid vehicleID, string modifiedBy)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "updateVehicleDriver";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@driverID", to);
                    cmd.Parameters.AddWithValue("@vehicleID", vehicleID);
                    cmd.Parameters.AddWithValue("@modifiedBy", modifiedBy);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void deleteVehicleDriver(Guid ID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("DeleteVehicleDriver", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", ID);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void removeDriver(Guid VehicleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("RemoveVehicleDriver", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vehicleID", VehicleID);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }
        #endregion

        #region " Signal "
        public void logSignal(VehicleSignal signal)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "logSignal";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", signal.Name);
                    cmd.Parameters.AddWithValue("@MAC", signal.MAC);
                    cmd.Parameters.AddWithValue("@timestamp", signal.timestamp);
                    cmd.Parameters.AddWithValue("@Lat", signal.Lat);
                    cmd.Parameters.AddWithValue("@Lon", signal.Lon);
                    cmd.Parameters.AddWithValue("@dBm", signal.dBm);
                    cmd.Parameters.AddWithValue("@SINR", signal.SINR);
                    cmd.ExecuteNonQuery();
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }
        #endregion

        #endregion

        #region " Data Reads "

        #region " Geo Fences "

        /// <summary>
        /// Loads the stored polygon data from the database, fired automatically at service start
        /// </summary>
        /// <returns>List of polygons for GlobalGeo's polygon list</returns>
        public List<Models.polygonData> getGeoFences(string GeoFenceID = "NA")
        {
            try
            {
                List<Models.polygonData> pdList = new List<Models.polygonData>();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = string.Empty;
                    if (GeoFenceID == "NA")
                    {
                        SQL = "SELECT GeoFenceID, GeoName, GeoNotes, GeoFence, GeoType, Radius FROM GeoFences";
                    }
                    else
                    {
                        SQL = "SELECT GeoFenceID, GeoName, GeoNotes, GeoFence, GeoType, Radius FROM GeoFences where GeoFenceID = '" + GeoFenceID + "'";
                    }

                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Models.polygonData pd = new Models.polygonData();
                        List<Models.LatLon> llList = new List<Models.LatLon>();
                        pd.geoFenceID = new Guid(rdr["GeoFenceID"].ToString());
                        pd.polyName = rdr["GeoName"].ToString();
                        pd.notes = rdr["GeoNotes"].ToString();
                        string[] fenceData = rdr["GeoFence"].ToString().Split('|');
                        double minLat = 0.0;
                        double minLon = 0.0;
                        double maxLat = 0.0;
                        double maxLon = 0.0;
                        for (int i = 0; i < fenceData.Length; i++)
                        {
                            Models.LatLon ll = new Models.LatLon();
                            string[] llData = fenceData[i].Split(',');
                            ll.Lat = Convert.ToDouble(llData[0]);
                            ll.Lon = Convert.ToDouble(llData[1]);
                            ll.Alt = Convert.ToDouble(llData[2]);
                            llList.Add(ll);
                            if (i == 0)
                            {
                                minLat = ll.Lat;
                                maxLat = ll.Lat;
                                minLon = ll.Lon;
                                maxLon = ll.Lon;
                            }
                            if (ll.Lat != 0 || ll.Lon != 0)
                            {
                                if (ll.Lat < minLat)
                                {
                                    minLat = ll.Lat;
                                }
                                if (ll.Lat > maxLat)
                                {
                                    maxLat = ll.Lat;
                                }
                                if (ll.Lon < minLon)
                                {
                                    minLon = ll.Lon;
                                }
                                if (ll.Lon > maxLon)
                                {
                                    maxLon = ll.Lon;
                                }
                            }
                        }
                        pd.geoFence = llList;
                        pd.geoType = rdr["GeoType"].ToString();
                        pd.radius = Convert.ToDouble(rdr["Radius"]);
                        pd.maxLat = maxLat;
                        pd.maxLon = maxLon;
                        pd.minLat = minLat;
                        pd.minLon = minLon;
                        //pd.polyID = pd.polyName;
                        pdList.Add(pd);
                    }

                    conn.Close();
                }
                return pdList;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " User/Company/Role "

        /// <summary>
        /// Load the user list from the database
        /// </summary>
        public void loadUsers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    Users.GlobalUserData.userList.Clear();
                    string SQL = "SELECT UserID, UserLastName, UserFirstName, UserEmail, UserOffice, UserPhone, UserPassword, UserSalt FROM Users";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        User u = new User();
                        u.UserID = new Guid(rdr["UserID"].ToString());
                        u.UserLastName = rdr["UserLastName"].ToString();
                        u.UserFirstName = rdr["UserFirstName"].ToString();
                        u.UserEmail = rdr["UserEmail"].ToString();
                        u.UserOffice = rdr["UserOffice"].ToString();
                        u.UserPhone = rdr["UserPhone"].ToString();
                        u.UserPassword = rdr["UserPassword"].ToString();
                        u.UserSalt = rdr["UserSalt"].ToString();
                        Users.GlobalUserData.userList.Add(u);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// load company data from database
        /// </summary>
        public void loadCompanies()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    Users.GlobalUserData.companyList.Clear();
                    string SQL = "SELECT CompanyID, CompanyName, CompanyAddress, CompanyCity, CompanyState, CompanyCountry, IsParent, Contact FROM Companies";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Company c = new Company();
                        c.CompanyID = new Guid(rdr["CompanyID"].ToString());
                        c.CompanyName = rdr["CompanyName"].ToString();
                        c.CompanyAddress = rdr["CompanyAddress"].ToString();
                        c.CompanyCity = rdr["CompanyCity"].ToString();
                        c.CompanyState = rdr["CompanyState"].ToString();
                        c.CompanyCountry = rdr["CompanyCountry"].ToString();
                        c.isParent = Convert.ToBoolean(rdr["IsParent"]);
                        if (!rdr.IsDBNull(7))
                        {
                            Guid userID = new Guid(rdr["Contact"].ToString());
                            User u = Users.GlobalUserData.userList.Find(delegate (User find) { return find.UserID == userID; });
                            if (u != null)
                            {
                                c.CompanyContact = u;
                            }
                        }
                        Users.GlobalUserData.companyList.Add(c);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Load role data from database
        /// </summary>
        public void loadRoles()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    Users.GlobalUserData.roleList.Clear();
                    string SQL = "SELECT RoleID, RoleName, RoleDescription, IsAdmin FROM Roles";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Role r = new Role();
                        r.RoleID = new Guid(rdr["RoleID"].ToString());
                        r.roleName = rdr["RoleName"].ToString();
                        r.roleDescription = rdr["RoleDescription"].ToString();
                        r.isAdmin = Convert.ToBoolean(rdr["IsAdmin"]);
                        Users.GlobalUserData.roleList.Add(r);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Load User Role information from database
        /// </summary>
        public void loadUserRoles()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    Users.GlobalUserData.userRoleList.Clear();
                    string SQL = "SELECT UserID, RoleID FROM UsersRoles";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        UserRole ur = new UserRole();
                        ur.UserID = new Guid(rdr["UserID"].ToString());
                        ur.RoleID = new Guid(rdr["RoleID"].ToString());
                        Users.GlobalUserData.userRoleList.Add(ur);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public void loadUserCompanies()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    Users.GlobalUserData.userCompanyList.Clear();
                    string SQL = "SELECT UserID, CompanyID FROM UsersCompanies";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        UserCompany uc = new UserCompany();
                        uc.UserID = new Guid(rdr["UserID"].ToString());
                        uc.CompanyID = new Guid(rdr["CompanyID"].ToString());
                        Users.GlobalUserData.userCompanyList.Add(uc);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " Vars "

        public void getVars()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    GlobalData.vars.Clear();
                    conn.Open();

                    string SQL = "SELECT ID,varName, varValue, varType, minValue, Email, Editable, Required FROM Vars WHERE VarType = 0";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        systemvar sv = new systemvar();
                        sv.varID = new Guid(rdr["ID"].ToString());
                        sv.varName = rdr["varName"].ToString().ToUpper();
                        sv.varVal = rdr["varValue"].ToString();
                        sv.minValue = rdr["minValue"].ToString();
                        sv.varType = Convert.ToInt32(rdr["varType"]);
                        sv.Email = rdr["Email"].ToString();
                        sv.Email = rdr["Editable"].ToString();
                        sv.Email = rdr["Required"].ToString();
                        GlobalData.vars.Add(sv);
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void getAppVars()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    GlobalData.appVars.Clear();
                    conn.Open();
                    string SQL = "SELECT ID, varName, varValue, minValue, Email, Editable, Required FROM Vars WHERE VarType = 1";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        systemvar sv = new systemvar();
                        sv.varID = new Guid(rdr["ID"].ToString());
                        sv.varName = rdr["varName"].ToString().ToUpper();
                        sv.varVal = rdr["varValue"].ToString();
                        sv.minValue = rdr["minValue"].ToString();
                        sv.Email = rdr["Email"].ToString();
                        sv.Email = rdr["Editable"].ToString();
                        sv.Email = rdr["Required"].ToString();
                        GlobalData.appVars.Add(sv);
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Vehicle Information "

        public string checkMac(string MACAddress)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "SELECT COUNT(*) FROM Vehicles WHERE MACAddress = '" + MACAddress + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();
                    if (count == 0)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "ALREADYEXISTS";
                    }


                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                return ex.ToString();
            }
        }

        public VehicleExtendedData getExtendedData(string vehicleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    VehicleExtendedData v = new VehicleExtendedData();
                    string SQL = "loadVehicleExtendedDataByVehicle";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vehicleID", vehicleID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {

                        v.ID = new Guid(rdr["ID"].ToString());
                        v.vehicleID = rdr["vehicleID"].ToString();
                        v.vehicleClassID = new Guid(rdr["VehicleClassID"].ToString());
                        v.vehicleClass = rdr["VehicleClassName"].ToString();
                        v.vehicleClassImage = rdr["VehicleClassImage"].ToString();
                        v.companyID = new Guid(rdr["CompanyID"].ToString());
                        v.companyName = rdr.IsDBNull(4) ? "NA" : rdr["CompanyName"].ToString();
                        v.licensePlate = rdr["LicensePlate"].ToString();
                        v.Make = rdr["Make"].ToString();
                        v.Model = rdr["VehicleModel"].ToString();
                        v.Year = Convert.ToInt32(rdr["Year"]);
                        v.haulLimit = Convert.ToInt32(rdr["HaulLimit"]);
                        v.RouterID = rdr["RouterID"].ToString();
                        v.MACAddress = rdr["MACAddress"].ToString();
                        v.VehicleFriendlyName = rdr["VehicleFriendlyName"].ToString();
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                    return v;
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void getLastTwoHours(string VehicleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    GlobalData.GPSRecords.Clear();
                    conn.Open();
                    string SQL = "LastTwoHours";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", VehicleID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        VehicleGPSRecord g = new VehicleGPSRecord();
                        g.ID = new Guid(rdr["ID"].ToString());
                        g.VehicleID = rdr["VehicleID"].ToString();
                        g.Direction = Convert.ToInt32(rdr["Direction"]);
                        g.Speed = Convert.ToInt32(rdr["Speed"]);
                        g.Lat = (float)(double)rdr["Lat"];
                        g.Lon = (float)(double)rdr["Lon"];
                        g.InPolygon = Convert.ToBoolean(rdr["InPolygon"]);
                        g.PolyName = rdr["PolyName"].ToString();
                        g.timestamp = Convert.ToDateTime(rdr["timestamp"]);
                        g.runID = new Guid(rdr["runID"].ToString());
                        g.lastMessageReceived = Convert.ToDateTime(rdr["lastMessageReceived"]);

                        GlobalData.GPSRecords.Add(g);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public int getABI(string VehicleID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    int ABI = 30;
                    conn.Open();
                    string SQL = "getABI";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", VehicleID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        ABI = Convert.ToInt32(rdr["ABI"]);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                    return ABI;
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void getVehicleHistory(string VehicleID, DateTime start, DateTime end)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    GlobalData.GPSRecords.Clear();
                    conn.Open();
                    string SQL = "getVehicleHistory";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", VehicleID);
                    cmd.Parameters.AddWithValue("@from", start);
                    cmd.Parameters.AddWithValue("@to", end);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        VehicleGPSRecord g = new VehicleGPSRecord();
                        g.ID = new Guid(rdr["ID"].ToString());
                        g.VehicleID = rdr["VehicleID"].ToString();
                        g.Direction = Convert.ToInt32(rdr["Direction"]);
                        g.Speed = Convert.ToInt32(rdr["Speed"]);
                        g.Lat = (float)(double)rdr["Lat"];
                        g.Lon = (float)(double)rdr["Lon"];
                        g.InPolygon = Convert.ToBoolean(rdr["InPolygon"]);
                        g.PolyName = rdr["PolyName"].ToString();
                        g.timestamp = Convert.ToDateTime(rdr["timestamp"]);
                        g.runID = new Guid(rdr["runID"].ToString());
                        g.lastMessageReceived = Convert.ToDateTime(rdr["lastMessageReceived"]);

                        GlobalData.GPSRecords.Add(g);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        //this loads up all the alerts that have been defined for a vehicle
        //it's not alerts that have been triggered, it's alerts that can be triggered
        public List<Models.alertModel> getAlertsForVehicle(Guid vehicleID)
        {
            try
            {
                List<Models.alertModel> alerts = new List<Models.alertModel>();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("getAlertsForVehicle", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vehicleID", vehicleID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Models.alertModel a = new Models.alertModel();
                        a.AlertID = new Guid(rdr["AlertID"].ToString());
                        a.AlertClassName = rdr["AlertClassName"].ToString();
                        a.AlertType = rdr["AlertType"].ToString();
                        a.AlertActive = Convert.ToBoolean(rdr["AlertActive"]);
                        a.AlertStartTime = Convert.ToDateTime(rdr["AlertStartDate"]);
                        a.AlertEndTime = Convert.ToDateTime(rdr["AlertEndDate"]);
                        a.AlertAction = rdr["AlertAction"].ToString();
                        a.AlertFriendlyName = rdr["AlertFriendlyName"].ToString();
                        a.minVal = rdr["minVal"].ToString();
                        a.NDB = Convert.ToBoolean(rdr["NDB"]);
                        alerts.Add(a);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
                return alerts;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public void loadAlertModels()
        {
            try
            {
                GlobalData.alertModels.Clear();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Models.alertModel m = new Models.alertModel();

                        GlobalData.alertModels.Add(m);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public void loadAlertVehicles()
        {
            try
            {
                GlobalData.alertVehicles.Clear();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT AlertID, VehicleID FROM AlertsVehicles";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        alertVehicle av = new alertVehicle();
                        av.AlertID = new Guid(rdr["AlertID"].ToString());
                        av.VehicleID = new Guid(rdr["VehicleID"].ToString());
                        GlobalData.alertVehicles.Add(av);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
        }

        public void loadAlertsForGeoFences()
        {
            try
            {
                GlobalData.alertGeoFences.Clear();

                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT AlertID, GeoFenceID FROM AlertsGeoFences";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        alertGeoFence a = new alertGeoFence();
                        a.AlertID = new Guid(rdr["AlertID"].ToString());
                        a.GeoFenceID = new Guid(rdr["GeoFenceID"].ToString());
                        GlobalData.alertGeoFences.Add(a);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Vehicle Classes "

        public void loadVehicleClasses()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    GlobalData.vehicleClasses.Clear();
                    string SQL = "SELECT VehicleClassID, VehicleClassName, VehicleClassDescription, VehicleClassImage FROM VehicleClasses";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        VehicleClass vc = new VehicleClass();
                        vc.VehicleClassID = new Guid(rdr["VehicleClassID"].ToString());
                        vc.VehicleClassName = rdr["VehicleClassName"].ToString();
                        vc.VehicleClassDescription = rdr["VehicleClassDescription"].ToString();
                        vc.VehicleClassImage = rdr["VehicleClassImage"].ToString();
                        GlobalData.vehicleClasses.Add(vc);
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }


        public List<VehicleExtendedData> getAvailableVehicles()
        {
            List<VehicleExtendedData> availableVehics = new List<VehicleExtendedData>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "select * from Vehicles V left join VehicleDriver AS VD on V.ID = VD.VehicleID WHERE VD.VehicleID IS NULL ORDER BY V.VehicleID";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        VehicleExtendedData v = getExtendedData(rdr["vehicleID"].ToString());
                        availableVehics.Add(v);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }

            return availableVehics;
        }

        #endregion

        #region " Drivers "

        public void loadDrivers()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    Users.GlobalUserData.driverList.Clear();
                    string SQL = "SELECT DriverID, DriverLastName, DriverFirstName, DriverPassword, CompanyID, ProfilePic, DriverEmail, DriverNumber, imagetype, imagefile, DriverPIN FROM Drivers";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Driver d = new Driver();
                        d.DriverID = new Guid(rdr["DriverID"].ToString());
                        d.DriverLastName = rdr["DriverLastName"].ToString();
                        d.DriverFirstName = rdr["DriverFirstName"].ToString();
                        d.DriverPassword = rdr["DriverPassword"].ToString();
                        d.ProfilePic = rdr["ProfilePic"].ToString();
                        d.CompanyID = rdr.IsDBNull(4) ? new Guid() : new Guid(rdr["CompanyID"].ToString());
                        d.DriverEmail = rdr["DriverEmail"].ToString();
                        d.DriverNumber = rdr["DriverNumber"].ToString();
                        d.imageType = rdr.IsDBNull(8) ? "NA" : rdr["imagetype"].ToString();
                        if (!rdr.IsDBNull(9))
                        {
                            d.imageData = (byte[])rdr["imagefile"];
                        }
                        d.PIN = rdr.IsDBNull(10) ? "NA" : rdr["DriverPIN"].ToString();
                        Users.GlobalUserData.driverList.Add(d);
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public string checkForDriver(Guid VehicleID)
        {
            string driverID = "0";
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT DriverID FROM VehicleDriver WHERE VehicleID = '" + VehicleID.ToString() + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        driverID = rdr.IsDBNull(0) ? "0" : rdr["DriverID"].ToString();
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return driverID;
        }

        public List<Driver> getAvailableDrivers()
        {
            List<Driver> availableDrivers = new List<Driver>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "select * from Drivers D left join VehicleDriver AS VD on VD.DriverID = D.DriverID WHERE  VD.DriverID IS NULL";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Driver d = new Driver();
                        d.DriverID = new Guid(rdr["DriverID"].ToString());
                        d.DriverLastName = rdr["DriverLastName"].ToString();
                        d.DriverFirstName = rdr["DriverFirstName"].ToString();
                        d.DriverPassword = rdr["DriverPassword"].ToString();
                        d.ProfilePic = rdr["ProfilePic"].ToString();
                        d.CompanyID = rdr.IsDBNull(4) ? new Guid() : new Guid(rdr["CompanyID"].ToString());
                        d.DriverEmail = rdr["DriverEmail"].ToString();
                        d.DriverNumber = rdr["DriverNumber"].ToString();
                        d.imageType = rdr.IsDBNull(8) ? "NA" : rdr["imagetype"].ToString();
                        if (!rdr.IsDBNull(9))
                        {
                            d.imageData = (byte[])rdr["imagefile"];
                        }
                        d.PIN = rdr.IsDBNull(10) ? "NA" : rdr["DriverPIN"].ToString();
                        availableDrivers.Add(d);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }

            return availableDrivers;
        }

        public List<driverVehicleReturn> getVehicleDrivers()
        {
            List<driverVehicleReturn> DriversToVehicles = new List<driverVehicleReturn>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "select * from VehicleDriver order by DateModified DESC";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        driverVehicleReturn dr = new driverVehicleReturn();
                        dr.ID = new Guid(rdr["ID"].ToString());
                        dr.VehicleID = new Guid(rdr["VehicleID"].ToString());
                        dr.DriverID = new Guid(rdr["DriverID"].ToString());
                        dr.ModifiedDate = Convert.ToDateTime(rdr["DateModified"]);
                        dr.ModifiedBy = rdr["ModifiedBy"].ToString();
                        dr.CreatedDate = Convert.ToDateTime(rdr["DateCreated"]);
                        dr.CreatedBy = rdr["CreatedBy"].ToString();
                        DriversToVehicles.Add(dr);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }

            return DriversToVehicles;
        }

        #endregion

        #region " Dispatch Lookups "

        public string getRouterData(string vehicleID)
        {
            string ret = string.Empty;
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT RouterID, MACAddress FROM Vehicles WHERE VehicleID = '" + vehicleID + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        string routerID = rdr["RouterID"].ToString();
                        string MAC = rdr["MACAddress"].ToString();
                        ret = routerID + "|" + MAC;
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return ret;
        }

        public dispatch findDispatch(Guid ID)
        {
            dispatch d = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "SELECT ID FROM Dispatches WHERE ID = '" + ID + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        d = new dispatch();
                        d.ID = new Guid(rdr["ID"].ToString());
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return d;
        }

        public dispatch findDispatchByID(Guid ID)
        {
            dispatch d = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "SELECT * FROM Dispatches WHERE ID = '" + ID + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        DateTime? dtAck = null;
                        DateTime? dtComplete = null;
                        d = new dispatch();
                        d.ID = new Guid(rdr["ID"].ToString());
                        d.timeStamp = Convert.ToDateTime(rdr["timeStamp"]);
                        d.UserEmail = rdr["UserEmail"].ToString();
                        d.vehicleID = rdr["vehicleID"].ToString();
                        d.runID = new Guid(rdr["runID"].ToString());
                        d.address = rdr["address"].ToString();
                        d.city = rdr["city"].ToString();
                        d.state = rdr["state"].ToString();
                        d.zip = rdr["zip"].ToString();
                        d.note = rdr.IsDBNull(9) ? string.Empty : rdr["note"].ToString();
                        d.acked = Convert.ToBoolean(rdr["acked"]);
                        d.ackTime = rdr.IsDBNull(11) ? dtAck : Convert.ToDateTime(rdr["ackTime"]);
                        d.ackMessage = rdr.IsDBNull(12) ? string.Empty : rdr["ackMessage"].ToString();
                        d.driverPIN = rdr.IsDBNull(13) ? string.Empty : rdr["driverPIN"].ToString();
                        d.completedMessage = rdr.IsDBNull(14) ? string.Empty : rdr["completedMessage"].ToString();
                        d.completedTime = rdr.IsDBNull(15) ? dtComplete : Convert.ToDateTime(rdr["completedTime"]);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
            return d;
        }

        public List<dispatch> getDispatchesByRange(DateTime start, DateTime end)
        {
            List<dispatch> dList = new List<dispatch>();

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT ID, timeStamp, UserEmail, vehicleID, runID, address, city, state, zip, note," +
                        " acked, ackTime, ackMessage, driverPIN, completedMessage, completedTime" +
                        " FROM Dispatches WHERE timeStamp BETWEEN '" + start.ToString() + "' and '" + end.ToString() + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        DateTime? dtAck = null;
                        DateTime? dtComplete = null;
                        dispatch d = new dispatch();
                        d.ID = new Guid(rdr["ID"].ToString());
                        d.timeStamp = Convert.ToDateTime(rdr["timeStamp"]);
                        d.UserEmail = rdr["UserEmail"].ToString();
                        d.vehicleID = rdr["vehicleID"].ToString();
                        d.runID = rdr.IsDBNull(4) ? Guid.Empty : new Guid(rdr["runID"].ToString());
                        d.address = rdr["address"].ToString();
                        d.city = rdr["city"].ToString();
                        d.state = rdr["state"].ToString();
                        d.zip = rdr["zip"].ToString();
                        d.note = rdr.IsDBNull(9) ? string.Empty : rdr["note"].ToString();
                        d.acked = Convert.ToBoolean(rdr["acked"]);
                        d.ackTime = rdr.IsDBNull(11) ? dtAck : Convert.ToDateTime(rdr["ackTime"]);
                        d.ackMessage = rdr.IsDBNull(12) ? string.Empty : rdr["ackMessage"].ToString();
                        d.driverPIN = rdr.IsDBNull(13) ? string.Empty : rdr["driverPIN"].ToString();
                        d.completedMessage = rdr.IsDBNull(14) ? string.Empty : rdr["completedMessage"].ToString();
                        d.completedTime = rdr.IsDBNull(15) ? dtComplete : Convert.ToDateTime(rdr["completedTime"]);
                        dList.Add(d);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return dList;
        }

        #endregion

        #region " Schedules "

        public Models.dailySchedule getDailyScheduleForVehicle(string ID)
        {
            try
            {
                Models.dailySchedule ds = new Models.dailySchedule();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("GetDailyScheduleForVehicle", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", ID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        ds.scheduleID = new Guid(rdr["ScheduleID"].ToString());
                        ds.dtStart = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["StartTime"].ToString());
                        if (rdr["EndTime"].ToString() == "00:00:00")
                        {
                            ds.dtEnd = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["EndTime"].ToString()).AddDays(1);
                        }
                        else
                        {
                            ds.dtEnd = Convert.ToDateTime(DateTime.Now.ToShortDateString() + " " + rdr["EndTime"].ToString());
                        }
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
                return ds;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                return null;
            }
        }

        #endregion

        #region " Alerts "

        public List<string> getLinkedAlertsVehicles(string alertFriendlyName)
        {
            try
            {
                List<string> vList = new List<string>();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("GetLinkedAlertsVehicles", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlertFriendlyName", alertFriendlyName);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        vList.Add(rdr["VehicleID"].ToString());
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
                return vList;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public List<string> getLinkedAlertsGeoFences(string alertFriendlyName)
        {
            try
            {
                List<string> gList = new List<string>();
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("GetLinkedAlertsGeoFences", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AlertFriendlyName", alertFriendlyName);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        gList.Add(rdr["GeoName"].ToString());
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                    return gList;
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public Guid getAlertClassIDFromName(string name)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT AlertClassID FROM AlertClasses WHERE Name = '" + name + "'", conn);
                    Guid acID = new Guid(cmd.ExecuteScalar().ToString());
                    cmd = null;
                    conn.Close();
                    return acID;
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public string getAlertClassNameFromID(Guid classID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT AlertClassName FROM AlertClasses WHERE AlertClassID = '" + classID + "'", conn);
                    string name = cmd.ExecuteScalar().ToString();
                    cmd = null;
                    conn.Close();
                    return name;
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        public List<alertClass> getAlertClasses()
        {
            List<alertClass> aList = new List<alertClass>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT AlertClassID, AlertClassName FROM AlertClasses ORDER BY AlertClassName";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        alertClass a = new alertClass();
                        a.AlertClassID = new Guid(rdr["AlertClassID"].ToString());
                        a.AlertClassName = rdr["AlertClassName"].ToString();
                        aList.Add(a);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
            return aList;
        }

        public List<linkVehicle> getLinkVehicles()
        {
            try
            {
                List<linkVehicle> lvList = new List<linkVehicle>();

                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT ID, VehicleID FROM Vehicles ORDER BY VehicleID", conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        linkVehicle lv = new linkVehicle();
                        lv.ID = new Guid(rdr["ID"].ToString());
                        lv.vehicleID = rdr["VehicleID"].ToString();
                        lvList.Add(lv);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }

                return lvList;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public List<macVehicle> getMacVehicles()
        {
            try
            {
                List<macVehicle> macList = new List<macVehicle>();

                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT ID, VehicleID, MACAddress FROM Vehicles ORDER BY VehicleID", conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        macVehicle mv = new macVehicle();
                        mv.ID = new Guid(rdr["ID"].ToString());
                        mv.vehicleID = rdr["VehicleID"].ToString();
                        mv.macAddress = rdr["MACAddress"].ToString();
                        macList.Add(mv);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }

                return macList;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public List<alertVehicle> getVehicleAlerts()
        {
            List<alertVehicle> aList = new List<alertVehicle>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT AlertID, VehicleID, AlertAction FROM AlertsVehicles";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        alertVehicle a = new alertVehicle();
                        a.AlertID = new Guid(rdr["AlertID"].ToString());
                        a.VehicleID = new Guid(rdr["VehicleID"].ToString());
                        a.AlertAction = rdr.IsDBNull(2) ? string.Empty : rdr["AlertAction"].ToString();
                        aList.Add(a);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
            return aList;
        }

        public List<alertGeoFence> getGeoFenceAlerts()
        {
            List<alertGeoFence> aList = new List<alertGeoFence>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT AlertID, GeoFenceID FROM AlertsGeoFences";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        alertGeoFence a = new alertGeoFence();
                        a.AlertID = new Guid(rdr["AlertID"].ToString());
                        a.GeoFenceID = new Guid(rdr["GeoFenceID"].ToString());
                        aList.Add(a);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
            return aList;
        }

        public List<dbAlert> getdbAlerts()
        {
            List<dbAlert> aList = new List<dbAlert>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT A.AlertID, A.AlertActive, A.AlertStartDate, A.AlertEndDate, A.AlertType, A.AlertClassID, AC.AlertClassName, A.AlertFriendlyName, A.minVal, A.NDB FROM Alerts A JOIN AlertClasses AC ON A.AlertClassID = AC.AlertClassID";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        dbAlert a = new dbAlert();
                        a.AlertID = new Guid(rdr["AlertID"].ToString());
                        a.AlertActive = Convert.ToBoolean(rdr["AlertActive"]);
                        a.AlertStartTime = Convert.ToDateTime(rdr["AlertStartDate"]);
                        a.AlertEndTime = Convert.ToDateTime(rdr["AlertEndDate"]);
                        a.AlertType = rdr.IsDBNull(4) ? string.Empty : rdr["AlertType"].ToString();
                        a.AlertClassID = new Guid(rdr["AlertClassID"].ToString());
                        a.AlertClassName = rdr["AlertClassName"].ToString();
                        a.AlertFriendlyName = rdr.IsDBNull(6) ? string.Empty : rdr["AlertFriendlyName"].ToString();
                        a.minVal = rdr.IsDBNull(7) ? string.Empty : rdr["minVal"].ToString();
                        a.NDB = rdr.IsDBNull(9) ? false : Convert.ToBoolean(rdr["NDB"]);
                        aList.Add(a);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
            return aList;
        }

        public List<alertReturn> getAlertsByRangeByVehicle(DateTime start, DateTime end, string vehicleID)
        {
            List<alertReturn> alerts = new List<alertReturn>();

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "SELECT AlertID, VehicleID, AlertName, AlertStart, AlertEnd, LatLonStart, LatLonEnd, MaxVal" +
                        " FROM VehicleAlerts WHERE AlertStart Between '" + start.ToString() + "' and '" + end.ToString() + "'" +
                        " AND VehicleID = '" + vehicleID + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        //d.ackMessage = rdr.IsDBNull(12) ? string.Empty : rdr["ackMessage"].ToString();
                        alertReturn a = new alertReturn();
                        a.alertID = new Guid(rdr["AlertID"].ToString());
                        a.vehicleID = rdr["VehicleID"].ToString();
                        a.alertName = rdr["AlertName"].ToString();
                        a.alertStart = rdr.IsDBNull(3) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertStart"]);
                        a.alertEnd = rdr.IsDBNull(4) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertEnd"]);
                        a.latLonStart = rdr.IsDBNull(5) ? "NA" : rdr["LatLonStart"].ToString();
                        a.latLonEnd = rdr.IsDBNull(6) ? "NA" : rdr["LatLonEnd"].ToString();
                        a.maxVal = rdr["MaxVal"].ToString();
                        alerts.Add(a);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return alerts;
        }

        public List<alertReturn> getAlertsByRange(DateTime start, DateTime end)
        {
            List<alertReturn> alerts = new List<alertReturn>();

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "getAlertsByDate";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        alertReturn a = new alertReturn();
                        a.alertID = new Guid(rdr["AlertID"].ToString());
                        a.vehicleID = rdr["VehicleID"].ToString();
                        a.alertName = rdr["AlertName"].ToString();
                        a.alertStart = rdr.IsDBNull(3) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertStart"]);
                        a.alertEnd = rdr.IsDBNull(4) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertEnd"]);
                        a.latLonStart = rdr.IsDBNull(5) ? "NA" : rdr["LatLonStart"].ToString();
                        a.latLonEnd = rdr.IsDBNull(6) ? "NA" : rdr["LatLonEnd"].ToString();
                        a.maxVal = rdr["MaxVal"].ToString();
                        alerts.Add(a);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return alerts;
        }

        public List<alertReturn> getAlertsByRangeByType(DateTime start, DateTime end, string type)
        {
            List<alertReturn> alerts = new List<alertReturn>();

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "getAlertsByDateByType";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@start", start);
                    cmd.Parameters.AddWithValue("@end", end);
                    cmd.Parameters.AddWithValue("@type", type);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        alertReturn a = new alertReturn();
                        a.alertID = new Guid(rdr["AlertID"].ToString());
                        a.vehicleID = rdr["VehicleID"].ToString();
                        a.alertName = rdr["AlertName"].ToString();
                        a.alertStart = rdr.IsDBNull(3) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertStart"]);
                        a.alertEnd = rdr.IsDBNull(4) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertEnd"]);
                        a.latLonStart = rdr.IsDBNull(5) ? "NA" : rdr["LatLonStart"].ToString();
                        a.latLonEnd = rdr.IsDBNull(6) ? "NA" : rdr["LatLonEnd"].ToString();
                        a.maxVal = rdr["MaxVal"].ToString();
                        alerts.Add(a);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return alerts;
        }

        public alertReturn getAlertByID(Guid ID)
        {
            alertReturn a = new alertReturn();

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "SELECT AlertID, VehicleID, AlertName, AlertStart, AlertEnd, LatLonStart, LatLonEnd, MaxVal" +
                        " FROM VehicleAlerts WHERE AlertID = '" + ID.ToString() + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        a.alertID = new Guid(rdr["AlertID"].ToString());
                        a.vehicleID = rdr["VehicleID"].ToString();
                        a.alertName = rdr["AlertName"].ToString();
                        a.alertStart = rdr.IsDBNull(3) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertStart"]);
                        a.alertEnd = rdr.IsDBNull(4) ? Convert.ToDateTime("01/01/2001 00:00:00") : Convert.ToDateTime(rdr["AlertEnd"]);
                        a.latLonStart = rdr.IsDBNull(5) ? "NA" : rdr["LatLonStart"].ToString();
                        a.latLonEnd = rdr.IsDBNull(6) ? "NA" : rdr["LatLonEnd"].ToString();
                        a.maxVal = rdr["MaxVal"].ToString();
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return a;
        }

        public List<VehicleGPSRecord> getAlertGPS(string vehicleID, string from, string to)
        {
            List<VehicleGPSRecord> AlertGPS = new List<VehicleGPSRecord>();

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();

                    string SQL = "SELECT ID, VehicleID, Direction, Speed, Lat, Lon, InPolygon, PolyName, timestamp, runID, lastMessageReceived" +
                        " FROM GPSTracking WHERE VehicleID = '" + vehicleID.ToString() + "' AND lastMessageReceived Between '" + from + "' and '" + to + "'";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        VehicleGPSRecord gps = new VehicleGPSRecord();
                        gps.ID = new Guid(rdr["ID"].ToString());
                        gps.VehicleID = rdr["VehicleID"].ToString();
                        gps.Direction = Convert.ToInt32(rdr["Direction"].ToString());
                        gps.Speed = Convert.ToInt32(rdr["Speed"].ToString());
                        gps.Lat = (float)(double)rdr["Lat"];
                        gps.Lon = (float)(double)rdr["Lon"];
                        gps.InPolygon = Convert.ToBoolean(rdr["InPolygon"].ToString());
                        gps.PolyName = rdr["PolyName"].ToString();
                        gps.timestamp = Convert.ToDateTime(rdr["timestamp"].ToString());
                        gps.runID = new Guid(rdr["runID"].ToString());
                        gps.lastMessageReceived = Convert.ToDateTime(rdr["lastMessageReceived"].ToString());
                        AlertGPS.Add(gps);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }

            return AlertGPS;
        }

        #endregion

        #region " Custom Statuses "

        public List<customStatus> getCustomStatus(Guid CompanyID) {
            try {
                List<customStatus> stats = new List<customStatus>();
                using (SqlConnection conn = new SqlConnection(getConn())) {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("GetCustomStatuses", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", CompanyID);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read()) {
                        customStatus s = new customStatus();
                        s.companyID = new Guid(rdr["CompanyID"].ToString());
                        s.companyName = rdr["CompanyName"].ToString();
                        s.customStatusID = new Guid(rdr["VehicleStatusID"].ToString());
                        s.vehicleStatusColor = rdr["VehicleStatusColor"].ToString();
                        s.vehicleStatusName = rdr["VehicleStatusName"].ToString();
                        s.vehicleStatusDescription = rdr["VehicleStatusDescription"].ToString();
                        stats.Add(s);
                    }

                    conn.Close();
                }
                return stats;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " Signal "

        public VehicleSignal getVehicleSignal(string MAC)
        {
            VehicleSignal signal = new VehicleSignal();

            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT ID, Name, MAC, MAX(timestamp) as timestamp, Lat, Lon, dBm, SINR FROM VehicleSignal where MAC = '" + MAC + "' Group By ID, Name, MAC, Lat, lon, dBm, SINR";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        signal.ID = new Guid(rdr["ID"].ToString());
                        signal.Name = rdr["Name"].ToString();
                        signal.MAC = rdr["MAC"].ToString();
                        signal.timestamp = Convert.ToDateTime(rdr["timestamp"]);
                        signal.Lat = float.Parse(rdr["Lat"].ToString());
                        signal.Lon = float.Parse(rdr["Lon"].ToString());
                        signal.dBm = float.Parse(rdr["dBm"].ToString());
                        signal.SINR = float.Parse(rdr["SINR"].ToString());
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }

            return signal;
        }

        #endregion

        #endregion

        #region " Error Logger "

        private void logError(string error)
        {
            Logger l = new Logger("SQLERRORS.txt");
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            string name = sf.GetMethod().Name + " in " + sf.GetMethod().DeclaringType.FullName;
            string logData = DateTime.Now.ToString() + Environment.NewLine + "ERROR IN: " + name + Environment.NewLine + error;
            l.writeToLogFile(logData);
        }

    #endregion

        #region " get Historical Data "

        public Messages.GPSData getLastPostion(Models.Vehicle v)
        {
            Messages.GPSData d = new Messages.GPSData();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    string SQL = "SELECT TOP 1 * FROM GPSTracking WHERE VehicleID = '" + v.VehicleID + "' AND timestamp < GETDATE() ORDER BY timestamp desc";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        d.dir = Convert.ToDouble(rdr["Direction"]);
                        d.lat = Convert.ToDouble(rdr["Lat"]);
                        d.lon = Convert.ToDouble(rdr["Lon"]);
                        d.spd = Convert.ToDouble(rdr["Speed"]);
                        d.messageType = "DB";
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
            return d;
        }

        public string checkLastPoly(Models.Vehicle v)
        {
            string polyName = string.Empty;
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    //this gets a little weird. Since a message has to be parsed and therefore logged in order to see if it's in a polygon
                    //and we need the current polygon data and the previous polygon data, we have to take the top 2 GPS messages from
                    //the vehicle and ignore the first one. Ultimately, it was easier to just make it a stored proc that did all that.
                    string SQL = "getNextToLastPoly";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VehicleID", v.VehicleID);
                    polyName = cmd.ExecuteScalar().ToString();
                    /*
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        polyName = rdr["PolyName"].ToString();
                    }
                    */
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
            }
            return polyName;
        }

        public List<Models.Vehicle> getAllVehicles(List<Models.Vehicle> cVehicles)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    conn.Open();
                    List<Models.Vehicle> vList = new List<Models.Vehicle>();
                    string exclude = string.Empty;
                    foreach (Models.Vehicle v in cVehicles)
                    {
                        exclude += v.VehicleID + ",";
                    }
                    if (exclude.Length > 0)
                    {
                        exclude = exclude.Substring(0, exclude.Length - 1);
                    }

                    SqlCommand cmd = new SqlCommand("GetAllVehicles", conn);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vals", exclude);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Models.Vehicle v = new Models.Vehicle();
                        v.alerts = new List<Models.alert>();
                        v.availAlerts = new List<Models.alertModel>();
                        v.behaviors = new List<driverBehavior>();
                        v.companyAlerts = new List<CompanyAlerts>();
                        v.driver = null;
                        v.extendedData = new VehicleExtendedData();
                        v.extendedData.companyID = new Guid(rdr["CompanyID"].ToString());
                        v.extendedData.companyName = rdr["CompanyName"].ToString();
                        v.extendedData.haulLimit = Convert.ToInt32(rdr["HaulLimit"]);
                        v.extendedData.ID = new Guid(rdr["ID"].ToString());
                        v.extendedData.licensePlate = rdr["LicensePlate"].ToString();
                        v.extendedData.MACAddress = rdr["MACAddress"].ToString();
                        v.extendedData.Make = rdr["Make"].ToString();
                        v.extendedData.Model = rdr["VehicleModel"].ToString();
                        v.extendedData.RouterID = rdr["RouterID"].ToString();
                        v.extendedData.vehicleClass = rdr["VehicleClassName"].ToString();
                        v.extendedData.vehicleClassID = new Guid(rdr["VehicleClassID"].ToString());
                        v.extendedData.vehicleClassImage = rdr["VehicleClassImage"].ToString();
                        v.extendedData.VehicleFriendlyName = rdr["VehicleFriendlyName"].ToString();
                        v.extendedData.vehicleID = rdr["VehicleID"].ToString();
                        v.extendedData.Year = Convert.ToInt32(rdr["Year"]);
                        v.VehicleID = rdr["VehicleID"].ToString();
                        v.gps = new Messages.GPSData();
                        v.gps.messageType = "GPS";
                        v.gps.lat = rdr.IsDBNull(17) ? 0 : Convert.ToDouble(rdr["Lat"]);
                        v.gps.lon = rdr.IsDBNull(18) ? 0 : Convert.ToDouble(rdr["Lon"]);
                        v.gps.dir = rdr.IsDBNull(15) ? 0 : Convert.ToInt32(rdr["Direction"]);
                        v.gps.spd = rdr.IsDBNull(16) ? 0 : Convert.ToInt32(rdr["Speed"]);
                        v.lastMessageReceived = rdr.IsDBNull(21) ? DateTime.Now : Convert.ToDateTime(rdr["LastMessageReceived"]);
                        v.insidePolyName = new Models.polygonData();
                        v.insidePolyName.polyName = rdr.IsDBNull(20) ? "N/A" : rdr["PolyName"].ToString();
                        vList.Add(v);
                    }
                    rdr.Close();
                    rdr = null;
                    cmd = null;

                    conn.Close();
                    return vList;
                }
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        public List<OBDLog> getOBDHistory(Guid VehicleID, DateTime from, DateTime to)
        {
            List<OBDLog> OBDLogs = new List<OBDLog>();
            try
            {
                using (SqlConnection conn = new SqlConnection(getConn()))
                {
                    GlobalData.vars.Clear();
                    conn.Open();

                    string SQL = "select * from [dbo].[OBDLog] where vehicleID = '" + VehicleID + "'AND timestamp BETWEEN '" + from.ToString() + "' AND '" + to.ToString() + "' order by timestamp desc";
                    SqlCommand cmd = new SqlCommand(SQL, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        OBDLog OBDLog = new OBDLog();
                        OBDLog.OBDLogID = new Guid(rdr["OBDLogID"].ToString());
                        OBDLog.timestamp = Convert.ToDateTime(rdr["timestamp"].ToString());
                        OBDLog.VehicleID = new Guid(rdr["VehicleID"].ToString());
                        OBDLog.runID = new Guid(rdr["runID"].ToString());
                        OBDLog.name = rdr["name"].ToString();
                        OBDLog.val = rdr["val"].ToString();
                        OBDLogs.Add(OBDLog);
                    }

                    conn.Close();
                }

                return OBDLogs;
            }
            catch (Exception ex)
            {
                logError(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        #endregion
    }
}
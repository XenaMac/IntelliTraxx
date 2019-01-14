using System;
using System.Collections.Generic;
using System.Linq;

namespace LATATrax.GlobalData
{
    public static class GlobalData
    {
        public static List<Models.Vehicle> vehicles = new List<Models.Vehicle>();
        public static List<systemvar> vars = new List<systemvar>();
        public static List<systemvar> appVars = new List<systemvar>();
        public static List<VehicleClass> vehicleClasses = new List<VehicleClass>();
        public static List<VehicleExtendedData> vehicleEDs = new List<VehicleExtendedData>();
        public static List<statusObject> statuses = new List<statusObject>();
        public static List<VehicleGPSRecord> GPSRecords = new List<VehicleGPSRecord>();
        public static List<Messages.OBD2> obdList = new List<Messages.OBD2>();
        public static List<Models.alertModel> alertModels = new List<Models.alertModel>();
        public static List<alertGeoFence> alertGeoFences = new List<alertGeoFence>(); //this holds things a specific geo fence alert can do
        public static List<alertVehicle> alertVehicles = new List<alertVehicle>();

        #region  " Alert Models & Geo Alerts "

        public static alertGeoFence findGeoAlert(string alertClass, Guid vehicleID, Guid geoFenceID) {
            //this needs to be altered to use just an alertid. we're not loading all alert models, those are handled per-vehicle
            var gfList = from am in alertModels
                         join av in alertVehicles on am.AlertID equals av.AlertID
                         join agf in alertGeoFences on av.AlertID equals agf.AlertID
                         join g in GeoCode.GlobalGeo.polygons on agf.GeoFenceID equals g.geoFenceID
                         join v in vehicles on av.VehicleID equals v.extendedData.ID
                         where g.geoFenceID == geoFenceID && v.extendedData.ID == vehicleID && am.AlertClassName == alertClass
                         select agf;
            alertGeoFence found = null;
            foreach (alertGeoFence agf in gfList) {
                found = agf;
            }
            return found;
        }

        #endregion

        #region " Vehicle Extended Data "

        public static string updateExtendedData(VehicleExtendedData v, Guid operatorID)
        {
            string ret = "OK";
            bool admin = Helpers.isAdmin(operatorID);
            if (!admin)
            {
                throw new Exception("User is not an administrator, operation cancelled");
            }
            try
            {
                //vehicle extended data is a part of a vehicle object, it's the part that's stored in the database and added
                //onto a running vehicle by associating vehicle.vehicleid to vehicleextendeddata.vehicleid. The object is only created and used when a truck
                //is actually running.
                SQLCode sql = new SQLCode();
                sql.updateVehicleExtendedData(v);
                Models.Vehicle found = vehicles.Find(delegate (Models.Vehicle find) { return find.VehicleID == v.vehicleID; });
                if (found != null)
                {
                    found.extendedData.companyID = v.companyID;
                    found.extendedData.haulLimit = v.haulLimit;
                    found.extendedData.licensePlate = v.licensePlate;
                    found.extendedData.Make = v.Make;
                    found.extendedData.Model = v.Model;
                    found.extendedData.vehicleClassID = v.vehicleClassID;
                    found.extendedData.VehicleFriendlyName = v.VehicleFriendlyName;
                    found.extendedData.Year = v.Year;
                    found.extendedData.MACAddress = v.MACAddress;
                }
                VehicleExtendedData vd = vehicleEDs.Find(delegate (VehicleExtendedData find) { return find.ID == v.ID; });
                if (vd == null)
                {
                    vehicleEDs.Add(v);
                }
                else
                {
                    vd.vehicleID = v.vehicleID;
                    vd.vehicleClassID = v.vehicleClassID;
                    vd.companyID = v.companyID;
                    vd.licensePlate = v.licensePlate;
                    vd.Make = v.Make;
                    vd.Model = v.Model;
                    vd.Year = v.Year;
                    vd.haulLimit = v.haulLimit;
                    vd.VehicleFriendlyName = v.VehicleFriendlyName;
                    vd.MACAddress = v.MACAddress;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        public static string deleteVehicleExtendedData(VehicleExtendedData v, Guid operatorID)
        {
            string ret = "OK";
            bool admin = Helpers.isAdmin(operatorID);
            if (!admin)
            {
                throw new Exception("User is not an administrator, operation cancelled");
            }

            try
            {
                SQLCode sql = new SQLCode();
                sql.deleteVehicleExtendedData(v.ID);
                Models.Vehicle found = vehicles.Find(delegate (Models.Vehicle find) { return find.VehicleID == v.vehicleID; });
                if (found != null)
                {
                    found.extendedData = null;
                    found.extendedData = new VehicleExtendedData();
                }
                for (int i = vehicleEDs.Count - 1; i >= 0; i--)
                {
                    if (vehicleEDs[i].ID == v.ID)
                    {
                        vehicleEDs.RemoveAt(i);
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

        #region " Vehicle Classes "

        public static string addUpdateVehicleClass(VehicleClass v, Guid operatorID)
        {
            string ret = "OK";
            bool admin = Helpers.isAdmin(operatorID);
            if (!admin)
            {
                throw new Exception("User is not an administrator, operation cancelled");
            }
            try
            {
                VehicleClass found = vehicleClasses.Find(delegate (VehicleClass find) { return find.VehicleClassID == v.VehicleClassID; });
                if (found != null)
                {
                    found.VehicleClassName = v.VehicleClassName;
                    found.VehicleClassDescription = v.VehicleClassDescription;
                    found.VehicleClassImage = v.VehicleClassImage;
                }
                else
                {
                    vehicleClasses.Add(v);
                }
                SQLCode sql = new SQLCode();
                sql.updateVehicleClass(v);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        public static string deleteVehicleClass(Guid id, Guid operatorID)
        {
            string ret = "OK";
            bool admin = Helpers.isAdmin(operatorID);
            if (!admin)
            {
                throw new Exception("User is not an administrator, operation cancelled");
            }
            try
            {
                for (int i = vehicleClasses.Count - 1; i >= 0; i--)
                {
                    if (vehicleClasses[i].VehicleClassID == id)
                    {
                        vehicleClasses.RemoveAt(i);
                    }
                }
                SQLCode sql = new SQLCode();
                sql.deleteVehicleClass(id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return ret;
        }

        #endregion

        #region "Alert Checkers "

        public static bool isSpeeding(double speed)
        {
            systemvar found = vars.Find(delegate (systemvar find) { return find.varName.ToUpper() == "SPEEDING"; });
            if (found != null)
            {
                double spdVar = Convert.ToDouble(found.varVal);
                if (speed > spdVar)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string isSpeedingEmail()
        {
            systemvar found = vars.Find(delegate(systemvar find) { return find.varName.ToUpper() == "SPEEDING"; });
            if (found != null)
            {
                return found.Email;
            }
            else
            {
                return "";
            }
        }

        #endregion

        #region  " App Vars "

        public static void updateAppVar(systemvar appVar, Guid operatorID)
        {
            try
            {
                bool isAdmin = Helpers.isAdmin(operatorID);
                if (isAdmin == false)
                {
                    throw new Exception("User is not an administrator");
                }
                SQLCode sql = new SQLCode();
                sql.updateVar(appVar);
                systemvar found = appVars.Find(delegate (systemvar find) { return find.varID == appVar.varID; });
                if (found == null)
                {
                    appVars.Add(appVar);
                }
                else
                {
                    found.varName = appVar.varName;
                    found.varVal = appVar.varVal;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static void deleteAppVar(systemvar v, Guid operatorID)
        {
            try
            {
                bool isAdmin = Helpers.isAdmin(operatorID);
                if (isAdmin == false)
                {
                    throw new Exception("User is not an administrator");
                }
                SQLCode sql = new SQLCode();
                sql.deleteVar(v);
                for (int i = appVars.Count - 1; i >= 0; i--)
                {
                    if (appVars[i].varID == v.varID)
                    {
                        appVars.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Service vars "

        public static void updateServiceVar(systemvar var, Guid operatorID)
        {
            try
            {
                bool isAdmin = Helpers.isAdmin(operatorID);
                if (isAdmin == false)
                {
                    throw new Exception("User is not an administrator");
                }
                SQLCode sql = new SQLCode();
                sql.updateVar(var);
                systemvar found = vars.Find(delegate (systemvar find) { return find.varID == var.varID; });
                if (found == null)
                {
                    vars.Add(var);
                }
                else
                {
                    found.varVal = var.varVal;
                    found.varName = var.varName;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static void deleteServiceVar(systemvar v, Guid operatorID)
        {
            try
            {
                bool isAdmin = Helpers.isAdmin(operatorID);
                if (isAdmin == false)
                {
                    throw new Exception("User is not an administrator");
                }
                SQLCode sql = new SQLCode();
                sql.deleteVar(v);
                for (int i = vars.Count - 1; i >= 0; i--)
                {
                    if (vars[i].varID == v.varID)
                    {
                        vars.RemoveAt(i);
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Status Data "

        public static void updateStatus(statusObject so)
        {
            try
            {
                statusObject soFound = statuses.Find(delegate (statusObject soFind) { return soFind.statusID == so.statusID; });
                SQLCode sql = new SQLCode();
                if (soFound == null)
                {
                    statuses.Add(so);
                }
                else
                {
                    if (so.statusEnd != null)
                    {
                        soFound.statusEnd = so.statusEnd;
                    }
                }
                sql.updateStatus(so);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public static string setStatus(string statusID, string statusName, string vehicleNumber, string driverID, string statusStart, string statusEnd)
        {
            string okay = "OK";
            try
            {
                statusObject so = new statusObject();
                Guid _statusID = new Guid(statusID);
                Guid _driverID = new Guid(driverID);
                if (statusName == "LOGGEDOFF") {
                    statusEnd = statusStart;
                }
                DateTime _statusStart = Convert.ToDateTime(statusStart);
                DateTime? _statusEnd = null;
                if (!string.IsNullOrEmpty(statusEnd) && statusEnd != "(not set)")
                {
                    _statusEnd = Convert.ToDateTime(statusEnd);
                }
                Models.Vehicle v = vehicles.Find(delegate (Models.Vehicle find) { return find.extendedData.MACAddress == vehicleNumber; });
                if (v.driver.DriverID == Guid.Empty)
                {
                    return "NLO"; //not logged on
                }

                so.statusID = _statusID;
                so.statusName = statusName;
                so.runID = v.runID;
                if (v != null)
                {
                    so.vehicleID = v.extendedData.ID; //this is the vehicles.ID field.
                }

                so.driverID = _driverID;
                so.statusStart = _statusStart;
                if (!string.IsNullOrEmpty(statusEnd))
                {
                    so.statusEnd = _statusEnd;
                }
                updateStatus(so);
                if (v.driver.currentStatus == null)
                {
                    v.driver.currentStatus = new statusObject();
                }
                v.driver.currentStatus.driverID = v.driver.DriverID;
                v.driver.currentStatus.vehicleID = v.extendedData.ID; //vehicle id
                v.driver.currentStatus.statusName = so.statusName;
                v.driver.currentStatus.statusStart = so.statusStart;
                v.driver.currentStatus.statusEnd = so.statusEnd;
                v.driver.currentStatus.statusID = so.statusID;
            }
            catch (Exception ex)
            { okay = ex.ToString(); }
            return okay;
        }

        #endregion

        #region " Force Logoff "

        public static string forceLogoff(string PIN, Guid operatorID) {
            string ret = "OK";

            try {

                bool isAdmin = Helpers.isAdmin(operatorID);
                if (isAdmin == false)
                {
                    throw new Exception("User is not an administrator");
                }
                for (int i = vehicles.Count - 1; i >= 0; i--) {
                    if (vehicles[i].driver.PIN == PIN) {
                        setStatus(Guid.NewGuid().ToString(), "LOGGEDOFF", vehicles[i].VehicleID, vehicles[i].driver.DriverID.ToString(), DateTime.Now.ToUniversalTime().ToString(), DateTime.Now.ToUniversalTime().ToString());
                        vehicles[i].driver = new Driver();
                    }
                }

            }
            catch (Exception ex) {
                ret = ex.Message;
            }

            return ret;

        }

        #endregion

        #region " UNIX Time Conversions "

        public static DateTime getUTCDTFromUnix(long timeStamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(timeStamp));
            dt = dt.ToUniversalTime();
            return dt;
        }

        public static DateTime getDTFromUnix(long timeStamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(timeStamp));
            return dt;
        }

        public static DateTime getDTFromUnixUTC(long timestamp)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(Convert.ToDouble(timestamp));
            dt = dt.ToLocalTime();
            return dt;
        }

        public static long getUTCUnixFromDT(DateTime dt)
        {
            dt = dt.ToUniversalTime();
            Int32 unixTime = (Int32)(dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            return unixTime;
        }

        public static long getUnixFromDT(DateTime dt)
        {
            Int32 unixTime = (Int32)(dt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
            return unixTime;
        }

        public static DateTime getUTCTime(DateTime dt)
        {
            return dt.ToUniversalTime();
        }

        #endregion

        #region "Silent Check"
        public static void SilentCheck()
        {
            try
            {
                foreach (Models.Vehicle veh in GlobalData.vehicles)
                {
                    DateTime now = DateTime.Now;
                    DateTime timestamp = veh.lastMessageReceived;
                    TimeSpan ts = (timestamp - now);
                    if(ts.TotalMinutes > 10)
                    {
                        GlobalData.vehicles.Remove(veh);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        #endregion
    }


}
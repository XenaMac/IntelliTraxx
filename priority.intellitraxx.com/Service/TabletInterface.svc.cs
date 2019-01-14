using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace LATATrax
{
    /// <summary>
    /// This Service Interface is for the tablet client only. Other functionality is exposed through AJAXVehicles (for AJAX-enabled WCF services for JavaScript) or
    /// TruckService for regular MVC interfaces
    /// </summary>

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TabletInterface" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select TabletInterface.svc or TabletInterface.svc.cs at the Solution Explorer and start debugging.
    public class TabletInterface : ITabletInterface
    {
        #region  " Driver Logon / Logoff "

        public TabletDriver LogonDriverByPin(string PIN, string VehicleID) //this is actually sending the MAC Address now, not the VehicleID
        {
            try
            {
                Driver d = Users.GlobalUserData.driverList.Find(delegate (Driver find) { return find.PIN == PIN; });
                if (d == null)
                {
                    return null;
                }
                else
                {
                    //got a driver, find the vehicle. NOTE: THe vehicle must be running and connected to the system.
                    Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.extendedData.MACAddress == VehicleID; });
                    if (v == null)
                    {
                        return null;
                    }
                    else
                    {
                        //do a check to see if that pin is already in use, if not allow the driver to logong
                        Models.Vehicle vPINCheck = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.driver.PIN == PIN; });
                        if (vPINCheck != null) {
                            //someone is already using that PIN. Create a tablet driver object that reflects that information and return it
                            TabletDriver td = new TabletDriver();
                            td.LastName = "PIN ALREADY IN USE";
                            return td;
                        }
                        else {
                            //should be all good, return a tablet driver object and let the driver logon
                            v.driver = d;
                            //create an empty status object, it will be populated from the tablet
                            d.currentStatus = new statusObject();
                            Company c = Users.GlobalUserData.companyList.Find(delegate (Company find) { return find.CompanyID == d.CompanyID; });
                            TabletDriver td = new TabletDriver();
                            td.DriverID = d.DriverID.ToString();
                            td.LastName = d.DriverLastName;
                            td.FirstName = d.DriverFirstName;
                            if (c != null)
                            {
                                td.CompanyName = c.CompanyName;
                            }
                            else
                            {
                                td.CompanyName = "UNKNOWN";
                            }
                            td.Email = d.DriverEmail;
                            td.DriverNumber = d.DriverNumber;
                            td.DriverImage = d.imageData;
                            td.ImageType = d.imageType.Replace(".", "");
                            td.runID = v.runID;
                            GlobalData.GlobalData.setStatus(Guid.NewGuid().ToString(), "LOGGEDON", v.VehicleID, d.DriverID.ToString(), DateTime.Now.ToString(), string.Empty);
                            return td;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public TabletDriver DriverAutoLogon(string PIN, string VehicleID) //this is actually sending the VehicleID
        {
            Guid VEH = new Guid(VehicleID);

            try
            {
                Driver d = Users.GlobalUserData.driverList.Find(delegate (Driver find) { return find.PIN == PIN; });
                if (d == null)
                {
                    return null;
                }
                else
                {
                    //got a driver, find the vehicle. NOTE: The vehicle must be running and connected to the system.
                    Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.extendedData.ID == VEH; });
                    if (v == null)
                    {
                        return null;
                    }
                    else
                    {
                        //do a check to see if that pin is already in use, if not allow the driver to logong
                        Models.Vehicle vPINCheck = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.driver.PIN == PIN; });
                        if (vPINCheck != null)
                        {
                            //someone is already using that PIN. Create a tablet driver object that reflects that information and return it
                            TabletDriver td = new TabletDriver();
                            td.LastName = "PIN ALREADY IN USE";
                            return td;
                        }
                        else
                        {
                            //should be all good, return a tablet driver object and let the driver logon
                            v.driver = d;
                            //create an empty status object, it will be populated from the tablet
                            d.currentStatus = new statusObject();
                            Company c = Users.GlobalUserData.companyList.Find(delegate (Company find) { return find.CompanyID == d.CompanyID; });
                            TabletDriver td = new TabletDriver();
                            td.DriverID = d.DriverID.ToString();
                            td.LastName = d.DriverLastName;
                            td.FirstName = d.DriverFirstName;
                            if (c != null)
                            {
                                td.CompanyName = c.CompanyName;
                            }
                            else
                            {
                                td.CompanyName = "UNKNOWN";
                            }
                            td.Email = d.DriverEmail;
                            td.DriverNumber = d.DriverNumber;
                            td.DriverImage = d.imageData;
                            td.ImageType = d.imageType.Replace(".", "");
                            td.runID = v.runID;
                            GlobalData.GlobalData.setStatus(Guid.NewGuid().ToString(), "LOGGEDON", v.VehicleID, d.DriverID.ToString(), DateTime.Now.ToString(), string.Empty);
                            return td;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public TabletDriver LogonDriver(string DriverNumber, string Password, string VehicleID)
        {
            try
            {
                //step 1, find driver
                Driver d = Users.GlobalUserData.driverList.Find(delegate (Driver find) { return find.DriverNumber == DriverNumber && find.DriverPassword == Password; });
                if (d == null)
                {
                    //can't find you, Hoss. Return false
                    return null;
                }
                else
                {
                    //got a driver, find the vehicle. NOTE: THe vehicle must be running and connected to the system.
                    Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.extendedData.MACAddress == VehicleID; });
                    if (v == null)
                    {
                        return null;
                    }
                    else
                    {
                        v.driver = d;
                        d.currentStatus = new statusObject();
                        Company c = Users.GlobalUserData.companyList.Find(delegate (Company find) { return find.CompanyID == d.CompanyID; });
                        TabletDriver td = new TabletDriver();
                        td.DriverID = d.DriverID.ToString();
                        td.LastName = d.DriverLastName;
                        td.FirstName = d.DriverFirstName;
                        if (c != null)
                        {
                            td.CompanyName = c.CompanyName;
                        }
                        else
                        {
                            td.CompanyName = "UNKNOWN";
                        }
                        td.Email = d.DriverEmail;
                        td.DriverNumber = d.DriverNumber;
                        td.DriverImage = d.imageData;
                        td.ImageType = d.imageType.Replace(".", "");
                        return td;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string LogoffDriver(string DriverID, string VehicleID)
        {
            try
            {
                Guid driverID = new Guid(DriverID);
                Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.extendedData.MACAddress == VehicleID; });
                if (v != null)
                {
                    if (v.driver.DriverID == driverID)
                    {
                        v.driver = new Driver();
                        v.driver.currentStatus = new statusObject();
                        v.runID = Guid.Empty;
                        GlobalData.GlobalData.setStatus(Guid.NewGuid().ToString(), "LOGGEDOFF", v.VehicleID, DriverID, DateTime.Now.ToString(), DateTime.Now.ToString());
                        return "LOGGEDOFF";
                    }
                    else
                    {
                        return "BADLOGOFF:NODRIVER";
                    }
                }
                else
                {
                    return "BADLOGOFF:NOVEHICLE";
                }
            }
            catch (Exception ex)
            {
                return "ERROR:" + ex.Message;
            }
        }
        
        public string DriverAutoLogoff(string from, string VehicleID)
        {
            try
            {
                Guid driverIDFrom = new Guid(from);
                Guid vehicleID = new Guid(VehicleID);
                Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.extendedData.ID == vehicleID; });
                if (v != null)
                {
                    if (v.driver.DriverID == driverIDFrom)
                    {
                        v.driver = new Driver();
                        v.driver.currentStatus = new statusObject();
                        v.runID = Guid.Empty;
                        GlobalData.GlobalData.setStatus(Guid.NewGuid().ToString(), "LOGGEDOFF", v.VehicleID, from, DateTime.Now.ToString(), DateTime.Now.ToString());
                        return "LOGGEDOFF";
                    }
                    else
                    {
                        return "BADLOGOFF:NODRIVER";
                    }
                }
                else
                {
                    return "BADLOGOFF:NOVEHICLE";
                }
            }
            catch (Exception ex)
            {
                return "ERROR:" + ex.Message;
            }
        }
        #endregion

        #region  " GPS Data for Tablet "

        public tabletGPS getPosition(string VehicleID)
        {
            try
            {
                tabletGPS gps = new tabletGPS();
                Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.VehicleID == VehicleID; });
                if (v != null)
                {
                    gps.lat = v.gps.lat;
                    gps.lon = v.gps.lon;
                    gps.direction = Convert.ToInt32(v.gps.dir);
                    gps.speed = v.gps.spd;
                }
                return gps;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        #region " Custom Status for Tablet "

        public customStatus[] getCustomStatuses(Guid companyID)
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<customStatus> statList = sql.getCustomStatus(companyID);
                customStatus[] stats = new customStatus[statList.Count];
                for (int i = 0; i < statList.Count; i++) {
                    stats[i] = statList[i];
                    }
                return stats;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " Status Updates "


        public string setStatus(string statusID, string statusName, string vehicleNumber, string driverID, string statusStart, string statusEnd)
        {
            string ret = GlobalData.GlobalData.setStatus(statusID, statusName, vehicleNumber, driverID, statusStart, statusEnd);
            return ret;
            
        }

        #endregion

        #region " Dispatches "

        public dispatch getDispatches(string routerID) {
            try {
                dispatch d = new dispatch();
                d = GlobalData.DispatchData.getDispatchesForVehicle(routerID);
                return d;
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }

        }

        public void ackDispatch(Guid dispatchID, string note, string driverPIN ) {
            try {
                GlobalData.DispatchData.ackDispatch(dispatchID, note, driverPIN);
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        public void closeDispatch(Guid dispatchID, string note) {
            try {
                GlobalData.DispatchData.closeDispatch(dispatchID, note);
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Initialize new vehicle from tablet "

        public string createNewVehicle(string vehicleID, string MACAddress, string vehicleFriendlyName, string licensePlate) {
            string ok = "OK";
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                //verify we don't have an existing MAC address
                ok = sql.checkMac(MACAddress);
                if (ok == "OK")
                {
                    //Log it to SQL
                    sql.createVehicleFromTablet(vehicleID, MACAddress, vehicleFriendlyName, licensePlate);
                }
                else {
                    return ok;
                }
                //Create a new vehicle with this basic data for intitial testing
                Models.Vehicle v = new Models.Vehicle();
                v.VehicleID = vehicleID;
                VehicleExtendedData ved = new VehicleExtendedData();
                ved.RouterID = MACAddress;
                ved.MACAddress = MACAddress;
                ved.VehicleFriendlyName = vehicleFriendlyName;
                v.extendedData = ved;
                GlobalData.GlobalData.vehicles.Add(v);
                return ok;
            }
            catch (Exception ex) {
                return ex.ToString();
            }
        }

        #endregion

        #region  " Test Interfaces - Not intended for production use "

        //Test interface for parameterized requests, not intended for production use
        public TestPerson GetPerson(string FirstName, string LastName)
        {
            TestPerson testPerson = new TestPerson();
            testPerson.FirstName = FirstName;
            testPerson.LastName = LastName;
            return testPerson;
        }

        //Simple test interface for connectivity test, not intended for production use
        public string sayHello()
        {
            return "Hello from WCF. And your mom.";
        }

        #endregion
    }

    
}

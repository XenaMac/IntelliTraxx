using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UDPListener.Helpers;
using Newtonsoft.Json;
using System.Web.Configuration;
using System.Net.Mail;
using System.Net;
using LATATrax.GlobalData;

namespace LATATrax.Models
{
    public class Vehicle : IVehicle
    {
        public string VehicleID = string.Empty;
        public string message = string.Empty;
        public Messages.GPSData gps = new Messages.GPSData();
        public List<Messages.Status> status = new List<Messages.Status>();
        public bool isInside = false;
        public Models.polygonData insidePolyName = null;
        public VehicleExtendedData extendedData = null;
        public List<alert> alerts = new List<alert>();
        public Driver driver = new Driver();
        public Guid runID = new Guid();
        public DateTime lastMessageReceived = Convert.ToDateTime("01/01/2001 00:00:00");
        public List<driverBehavior> behaviors = new List<driverBehavior>();
        public List<OBD2Data> OBDVals = new List<OBD2Data>();
        public Messages.OBD2 obd2 = new Messages.OBD2();
        public static List<Models.polygonData> polygons = new List<polygonData>();
        public List<CompanyAlerts> companyAlerts = new List<CompanyAlerts>();
        public List<alertModel> availAlerts = new List<alertModel>();
        private bool overrideStationary = false;
        private polygonData oldPoly = null;
        public dailySchedule sched = null;
        public int ABI = 10;
        public VehicleSignal signal = new VehicleSignal();

        #region " OBD Information "

        public void addOBDData(OBD2Data o) {
            try {
                OBD2Data found = OBDVals.Find(delegate (OBD2Data find)
                {
                    return find.name == o.name;
                });
                if (found == null)
                {
                    OBDVals.Add(o);
                }
                else {
                    found.val = o.val;
                }
                //log it
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.logOBDData(o, this.extendedData.ID, this.runID);
            }
            catch (Exception ex) {
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in addOBDData: " + ex.ToString());
            }
        }

        #endregion

        #region " Messages & Helpers "

        public void readMessage(MessageData md)
        {
            try
            {
                //got the message
                VehicleID = md.VehicleID;
                message = md.Message;
                if (runID == Guid.Empty)
                {
                    runID = Guid.NewGuid();
                }
                //check message type
                if (message.Contains("GPSMESSAGE"))
                {
                    gps = (Messages.GPSData)_getData<Messages.GPSData>(message);
                    processGPS(gps);
                }
                else if (message.Contains("STATUSMESSAGE"))
                {
                    Messages.Status st = (Messages.Status)_getData<Messages.Status>(message);
                    processStatusMessage(st);
                }
                else if (message.Contains("GPSMISSINGMESSAGE"))
                {
                    Messages.MissingGPSMessage mg = (Messages.MissingGPSMessage)_getData<Messages.MissingGPSMessage>(message);
                    processMissingGPSMessage(mg);
                }
                else if (message.Contains(">RPV"))
                {
                    VehicleID = message.Substring(38, 4);
                    Messages.GPSData gpsOld = gps;
                    //gps = (Messages.GPSData)_getData<Messages.GPSData>(message);
                    gps = getGPSData(message);
                    if (gps.lat == 0 && gps.lon == 0)
                    {
                        gps = gpsOld;
                        // keep this old so we can know
                        //lastMessageReceived = DateTime.Now.ToUniversalTime();
                    }
                    else
                    {
                        processGPS(gps);
                    }

                }
                else if (message.Contains("CAN")) {
                    obd2 = JsonConvert.DeserializeObject<Messages.OBD2>(message);
                }

                Global.udp.removeMessage(md.messageID);
            }
            catch (Exception ex)
            {
                //add logger at some point
                //throw new Exception(ex.ToString());
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in readMessage: " +  Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
            }
        }

        private void processGPS(Messages.GPSData gps)
        {
            try
            {
                checkForDriver();
                checkGeoFences(gps);
                checkSpeeding(gps.spd);
                checkStationaryAlert(gps);
                checkOvertime(gps);
                getSignal();
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.logGPS(this);
            }
            catch (Exception ex) {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in processGPS: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
            }
        }

        private void checkForDriver()
        {
            if(this.driver == null || this.driver.DriverID.ToString() == "00000000-0000-0000-0000-000000000000")
            {
                //check for a driver in the VehiclesDriverTable
                //if there is one then log that guy in
                SQLCode sql = new SQLCode();
                string driverID = sql.checkForDriver(this.extendedData.ID);

                if (driverID != "0")
                {
                    //get driver PIN
                    TruckService ts = new TruckService();
                    Driver driver = ts.getDrivers().Where(d => d.DriverID == new Guid(driverID)).FirstOrDefault();

                    //Log in the driver
                    TabletInterface ti = new TabletInterface();
                    TabletDriver td = ti.DriverAutoLogon(driver.PIN, this.extendedData.ID.ToString());
                }
            }
        }

        private void processMissingGPSMessage(Messages.MissingGPSMessage g)
        {
            try
            {
                Messages.GPSData gps = new Messages.GPSData();
                gps.lat = g.lat;
                gps.lon = g.lon;
                gps.dir = g.dir;
                gps.spd = g.spd;
                string polyName = findGeoName(gps);
                bool isInside = false;
                if (polyName != "NA")
                {
                    isInside = true;
                }

                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                string dtString = g.tm;
                Guid loggedRunID = new Guid(g.runid);
                DateTime dt = GlobalData.Helpers.makeDTFromTablet(g.tm);
                if (dt != Convert.ToDateTime("01/01/2001 00:00:00"))
                {
                    sql.logMissingGPS(this, dt, loggedRunID, g, isInside, polyName);
                }
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in processMissingGPSMessage: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
            }

        }

        private string findGeoName(Messages.GPSData gps)
        {
            try
            {
                //GlobalData.SQLCode sql = new GlobalData.SQLCode();
                insidePolyName = GeoCode.GlobalGeo.findInside(gps.lat, gps.lon);
                return insidePolyName.polyName;
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in findGeoName: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
                return ex.ToString();
            }
        }

        private void processStatusMessage(Messages.Status st)
        {
            try
            {
                Messages.Status found = status.Find(delegate (Messages.Status find)
                {
                    return find.statusName == st.statusName;
                });
                if (found == null)
                {
                    //no current status/val pair for this, add it
                    status.Add(st);
                }
                else
                {
                    //got one, update the data in the status type
                    found.statusVal = st.statusVal;
                }
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in processStatusMessage: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
            }
        }

        private T _getData<T>(string message) where T : new()
        {
            return !string.IsNullOrEmpty(message) ? JsonConvert.DeserializeObject<T>(message) : new T();
        }

        private Messages.GPSData getGPSData(string message)
        {
            Messages.GPSData gpsN = new Messages.GPSData();
            try
            {
                string TAIPString = message;

                string GPSDT = getGPSTimeOfDay(Convert.ToInt32(TAIPString.Substring(4, 5)));
                string LAT = TAIPString.Substring(9, 8);
                string LON = TAIPString.Substring(17, 9);
                string SPEED = TAIPString.Substring(26, 3);
                string DIR = TAIPString.Substring(29, 3);
                string SOURCE = TAIPString.Substring(32, 1);
                string AGE = TAIPString.Substring(33, 1);
                string ID = TAIPString.Substring(38, 4);
                string CHKSUM = TAIPString.Substring(33, 3);

                string LAT1 = LAT.Substring(0, 3) + ".";
                string LAT2 = LAT1 + ((LAT.Length > 4) ? LAT.Substring(LAT.Length - 5, 5) : LAT);

                string LON1 = LON.Substring(0, 4) + ".";
                string LON2 = LON1 + ((LON.Length > 4) ? LON.Substring(LON.Length - 5, 5) : LON);


                gpsN.dir = Convert.ToDouble(DIR);
                gpsN.lat = Convert.ToDouble(LAT2);
                gpsN.lon = Convert.ToDouble(LON2);
                gpsN.messageType = "GPSMESSAGE";
                gpsN.spd = Convert.ToDouble(SPEED);
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in getGPSData: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
            }
            return gpsN;
        }

        private string getGPSTimeOfDay(int GPSSeconds)
        {
            try
            {
                /*
                int hours = (GPSSeconds / 3600);
                int minutes = (GPSSeconds - (hours * 3600)) / 60;
                int seconds = GPSSeconds - ((hours * 3600) + (minutes * 60));
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                int day = DateTime.Now.Day;

                DateTime utcConverted =
                    new DateTime(year, month, day, hours, minutes, seconds, DateTimeKind.Utc)
                    .ToLocalTime();

                return utcConverted.ToString();
                */
                string GPSTOD = Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy 00:00:00")).AddSeconds(GPSSeconds).ToString("MM/dd/yyyy HH:mm:ss");
                DateTime dt = Convert.ToDateTime(GPSTOD);
                if(lastMessageReceived != Convert.ToDateTime("01/01/2001 00:00:00"))
                {
                    ABI = Convert.ToInt32((dt - lastMessageReceived).TotalSeconds);
                }
                lastMessageReceived = dt;
                return dt.ToString();
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in getGPSTimeOfDay: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
                return "ERROR";
            }

        }

        private string decimalToLatLong(decimal dec)
        {
            try
            {
                int d = (int)dec;
                int m = (int)((dec - d) * 60);
                decimal s = ((((dec - d) * 60) - m) * 60);

                return d + "° " + m + "' " + s + "\"";
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in decimalToLatLong: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
                return "ERROR";
            }

        }

        private void getSignal()
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            signal = sql.getVehicleSignal(extendedData.MACAddress);
        }
        
        #endregion

        #region " Alert Processing "

        private void checkStationaryInGeoAlert(Messages.GPSData gps, polygonData insidePolyName, bool inside) {
            bool raiseAlert = false;
            if (gps.spd > 0) {
                //do a quick check to see if there's already an alert
                alert a = alerts.Find(delegate (alert find) { return find.alertName == "GEOSTATIONARY" && find.alertEnd == Convert.ToDateTime("01/01/2001 00:00:00"); });
                if (a != null) {
                    a.alertEnd = DateTime.Now.ToUniversalTime();
                    a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                    a.alertActive = false;
                    overrideStationary = false;
                    GlobalData.SQLCode sql = new GlobalData.SQLCode();
                    sql.logAlert(a, this.VehicleID);
                }
                return;
            }
            try {
                if (inside)
                {
                    alertGeoFence agf = findGeoAlert("STATIONARY", this.extendedData.ID, insidePolyName.geoFenceID);
                    Models.alertModel am = null;
                    if (agf != null)
                    {
                        am = availAlerts.Find(delegate (alertModel find) { return find.AlertID == agf.AlertID &&
                            find.AlertActive == true &&
                            find.AlertStartTime <= DateTime.Now.ToUniversalTime() &&
                            find.AlertEndTime >= DateTime.Now.ToUniversalTime(); }); //NOTE: Alerts are created using local time. We might need to rethink that
                    }
                    if (am != null)
                    {
                        GlobalData.SQLCode sql = new GlobalData.SQLCode();
                        int threshold = 0;
                        systemvar v = GlobalData.GlobalData.appVars.Find(delegate (systemvar find) { return find.varName == "STATIONARY"; });
                        if (v != null) //this is the global setting, it will be used if a particular alert doesn't have a minVal specified
                        {
                            threshold = Convert.ToInt32(v.varVal);
                        }
                        int alertThreshold = 0;
                        if (Int32.TryParse(am.minVal, out alertThreshold))
                        {
                            alertThreshold = Convert.ToInt32(alertThreshold);
                        }
                        if (alertThreshold > 0)
                        {
                            threshold = alertThreshold;
                        }
                        alert a = alerts.Find(delegate (alert find) { return find.alertName == "GEOSTATIONARY" && find.alertEnd == Convert.ToDateTime("01/01/2001 00:00:00"); });
                        if (a != null && gps.spd > 0)
                        {
                            //we have a currently active stationary alert AND we're moving, close the alert and send it on its way
                            a.alertActive = false;
                            a.alertEnd = DateTime.Now.ToUniversalTime();
                            a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                            overrideStationary = false;
                            sql.logAlert(a, this.VehicleID);
                        }
                        else if (a == null && gps.spd == 0)
                        {
                            //currently going 0 miles per hour, make the alert, but don't log it until the threshold time is met
                            alert alrt = new alert();
                            alrt.alertID = Guid.NewGuid();
                            alrt.alertType = "GEOSTATIONARY";
                            alrt.alertName = "GEOSTATIONARY";
                            alrt.alertStart = DateTime.Now.ToUniversalTime();
                            alrt.alertEnd = Convert.ToDateTime("01/01/2001 00:00:00");
                            alrt.alertActive = false;
                            alrt.latLonStart = gps.lat.ToString() + "|" + gps.lon.ToString();

                            alrt.runID = this.runID;
                            overrideStationary = true;
                            alerts.Add(alrt);

                            if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL")) {
                                string emls = am.AlertAction.Replace("EMAIL:", "");
                                if (!string.IsNullOrEmpty(emls))
                                {
                                    GlobalData.sendMail sm = new GlobalData.sendMail();
                                    sm.sendAlertEmail(emls, a, this, string.Empty);
                                }
                            }
                        }
                        else if (a != null && gps.spd == 0)
                        {
                            //we have a pre-existing alert that may not have been active. Check to see if it should now be active.
                            //if it's active, log it
                            if (a.alertStart.AddMinutes(threshold) < DateTime.Now.ToUniversalTime())
                            {
                                if (!a.alertActive)
                                {
                                    //now we can fire the alert, but we only do it once
                                    a.alertActive = true;
                                    overrideStationary = true;
                                    sql.logAlert(a, this.VehicleID);
                                    if (am.NDB == true && this.driver != null)
                                    {
                                        //we have a driver behavior issue, log it
                                        driverBehavior db = new driverBehavior();
                                        db.behavior = a.alertName;
                                        db.driverID = this.driver.DriverID;
                                        db.timeStamp = DateTime.Now.ToUniversalTime();
                                        sql.logDriverBehavior(db, this.extendedData.ID);
                                        this.behaviors.Add(db);
                                    }
                                }
                            }
                        }
                    }
                    /*
                    else {
                        raiseAlert = false;
                    }
                    if (raiseAlert)
                    {

                    }
                    */
                }
                else {
                    //we're outside the geofence, see if there's still an alert pending for this vehicle and kill it.
                    alert a = alerts.Find(delegate (alert find) { return find.alertName == "GEOSTATIONARY" && find.alertEnd == Convert.ToDateTime("01/01/2001 00:00:00"); });
                    if (a != null) {
                        a.alertEnd = DateTime.Now.ToUniversalTime();
                        a.alertActive = false;
                        a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                        overrideStationary = false;
                        GlobalData.SQLCode sql = new GlobalData.SQLCode();
                        sql.logAlert(a, this.VehicleID);
                    }
                }
            }
            catch (Exception ex) {
                string err = ex.ToString();
            }
        }

        private void checkStationaryAlert(Messages.GPSData gps) {
            try {
                //check for stationaryness.
                if (overrideStationary) {
                    //this means we've already got a stationary alert for inside a geo fence, no need to duplicate alerts
                    return;
                }
                Models.alertModel am = availAlerts.Find(delegate (alertModel find) { return find.AlertClassName == "STATIONARY" &&
                            find.AlertActive == true &&
                            find.AlertStartTime <= DateTime.Now.ToUniversalTime() &&
                            find.AlertEndTime >= DateTime.Now.ToUniversalTime(); });
                if (am != null)
                {
                    //this means we have an alart available for stationaryness
                    //the first thing is to check and see if we have a currently active stationary alert
                    GlobalData.SQLCode sql = new GlobalData.SQLCode();
                    int threshold = 0;
                    systemvar v = GlobalData.GlobalData.appVars.Find(delegate (systemvar find) { return find.varName == "STATIONARY"; });
                    if (v != null) //this is the global setting, it will be used if a particular alert doesn't have a minVal specified
                    {
                        threshold = Convert.ToInt32(v.varVal);
                    }
                    int alertThreshold = 0;
                    if (Int32.TryParse(am.minVal, out alertThreshold))
                    {
                        alertThreshold = Convert.ToInt32(alertThreshold);
                    }
                    if (alertThreshold > 0) {
                        threshold = alertThreshold;
                    }
                    alert a = alerts.Find(delegate (alert find) { return find.alertName == "STATIONARY" && find.alertEnd == Convert.ToDateTime("01/01/2001 00:00:00"); });
                    if (a != null && gps.spd > 0)
                    {
                        //we have a currently active stationary alert AND we're moving, close the alert and send it on its way
                        a.alertActive = false;
                        a.alertEnd = DateTime.Now.ToUniversalTime();
                        a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                        sql.logAlert(a, this.VehicleID);
                    }
                    else if (a == null && gps.spd == 0)
                    {
                        //currently going 0 miles per hour, make the alert, but don't log it until the threshold time is met
                        alert alrt = new alert();
                        alrt.alertID = Guid.NewGuid();
                        alrt.alertType = am.AlertFriendlyName;
                        alrt.alertName = "STATIONARY";
                        alrt.alertStart = DateTime.Now.ToUniversalTime();
                        alrt.alertEnd = Convert.ToDateTime("01/01/2001 00:00:00");
                        alrt.alertActive = false;
                        alrt.latLonStart = gps.lat.ToString() + "|" + gps.lon.ToString();
                        //alrt.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString(); //this will get updted when the alert ends
                        alrt.runID = this.runID;
                        alerts.Add(alrt);
                        sql.logAlert(alrt, this.VehicleID);
                    }
                    else if (a != null && gps.spd == 0)
                    {
                        //we have a pre-existing alert that may not have been active. Check to see if it should now be active.
                        //if it's active, log it
                        if (a.alertStart.AddMinutes(threshold) < DateTime.Now.ToUniversalTime())
                        {
                            if (!a.alertActive)
                            {
                                //now we can fire the alert, but we only do it once
                                a.alertActive = true;
                                //a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                                sql.logAlert(a, this.VehicleID);
                                if (am.NDB == true && this.driver != null)
                                {
                                    //we have a driver behavior issue, log it
                                    driverBehavior db = new driverBehavior();
                                    db.behavior = a.alertName;
                                    db.driverID = this.driver.DriverID;
                                    db.timeStamp = DateTime.Now.ToUniversalTime();
                                    sql.logDriverBehavior(db, this.extendedData.ID);
                                    this.behaviors.Add(db);
                                }
                                if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL"))
                                {
                                    string emls = am.AlertAction.Replace("EMAIL:", "");
                                    if (!string.IsNullOrEmpty(emls))
                                    {
                                        GlobalData.sendMail sm = new GlobalData.sendMail();
                                        sm.sendAlertEmail(emls, a, this, string.Empty);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) {
                string err = ex.ToString();
            }
            
        }

        private void checkOvertime(Messages.GPSData gps) {
            try {
                if (sched != null) {
                    systemvar sv = GlobalData.GlobalData.appVars.Find(delegate (systemvar find) { return find.varName == "SCHEDULE"; });
                    int leewayTime = 0;
                    if (sv != null) {
                        leewayTime = Convert.ToInt32(sv.minValue);
                    }

                    //we have the potential for an alert, see if we have one defined
                    Models.alertModel am = availAlerts.Find(delegate (alertModel find) {
                        return find.AlertClassName == "SCHEDULE" &&
                            find.AlertActive == true &&
                            find.AlertStartTime <= DateTime.Now.ToUniversalTime() &&
                            find.AlertEndTime >= DateTime.Now.ToUniversalTime();
                    });

                    if(am != null)
                    {
                        //Check if they are running without a schedule. The alert is supposed to tell them when any vehilce is running OFF schedule
                        if (this.sched.scheduleID.ToString() == "00000000-0000-0000-0000-000000000000")
                        {
                            //this vehicle is running off scheddule with a schedule alert active
                            alert a = alerts.Find(delegate (alert find) { return find.alertName == "SCHEDULE"; });
                            if (a == null)
                            {
                                // no alert has been triggered, create one and fire it
                                a = new alert();
                                a.alertID = Guid.NewGuid();
                                a.alertActive = false;
                                a.alertEnd = this.lastMessageReceived;
                                a.alertStart = this.lastMessageReceived;
                                a.alertName = "SCHEDULE";
                                a.alertType = am.AlertFriendlyName;
                                a.latLonStart = gps.lat.ToString() + " | " + gps.lon.ToString();
                                a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                                a.maxVal = "NA";
                                a.runID = this.runID;
                                alerts.Add(a);
                                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                                sql.logAlert(a, this.VehicleID);
                                if (am.NDB == true && this.driver != null)
                                {
                                    //we have a driver behavior issue, log it
                                    driverBehavior db = new driverBehavior();
                                    db.behavior = a.alertName;
                                    db.driverID = this.driver.DriverID;
                                    db.timeStamp = this.lastMessageReceived;
                                    sql.logDriverBehavior(db, this.extendedData.ID);
                                    this.behaviors.Add(db);
                                }
                                if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL"))
                                {
                                    string emls = am.AlertAction.Replace("EMAIL:", "");
                                    if (!string.IsNullOrEmpty(emls))
                                    {
                                        GlobalData.sendMail sm = new GlobalData.sendMail();
                                        sm.sendAlertEmail(emls, a, this, string.Empty);
                                    }
                                }
                            }
                        } else if (this.lastMessageReceived < sched.dtStart)
                        {
                            //this vehicle is running off scheddule with a schedule alert active
                            alert a = alerts.Find(delegate (alert find) { return find.alertName == "SCHEDULE"; });
                            if (a == null)
                            {
                                // no alert has been triggered, create one and fire it
                                a = new alert();
                                a.alertID = Guid.NewGuid();
                                a.alertActive = false;
                                a.alertEnd = this.lastMessageReceived;
                                a.alertStart = this.lastMessageReceived;
                                a.alertName = "SCHEDULE";
                                a.alertType = am.AlertFriendlyName + " - BEFORE";
                                a.latLonStart = gps.lat.ToString() + " | " + gps.lon.ToString();
                                a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                                a.maxVal = "NA";
                                a.runID = this.runID;
                                alerts.Add(a);
                                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                                sql.logAlert(a, this.VehicleID);
                                if (am.NDB == true && this.driver != null)
                                {
                                    //we have a driver behavior issue, log it
                                    driverBehavior db = new driverBehavior();
                                    db.behavior = a.alertName;
                                    db.driverID = this.driver.DriverID;
                                    db.timeStamp = this.lastMessageReceived;
                                    sql.logDriverBehavior(db, this.extendedData.ID);
                                    this.behaviors.Add(db);
                                }
                                if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL"))
                                {
                                    string emls = am.AlertAction.Replace("EMAIL:", "");
                                    if (!string.IsNullOrEmpty(emls))
                                    {
                                        GlobalData.sendMail sm = new GlobalData.sendMail();
                                        sm.sendAlertEmail(emls, a, this, string.Empty);
                                    }
                                }
                            }
                        }
                        else if (this.lastMessageReceived > sched.dtEnd)
                        {
                            //this vehicle is running off scheddule with a schedule alert active
                            alert a = alerts.Find(delegate (alert find) { return find.alertName == "SCHEDULE"; });
                            if (a == null)
                            {
                                // no alert has been triggered, create one and fire it
                                a = new alert();
                                a.alertID = Guid.NewGuid();
                                a.alertActive = false;
                                a.alertEnd = this.lastMessageReceived;
                                a.alertStart = this.lastMessageReceived;
                                a.alertName = "SCHEDULE";
                                a.alertType = am.AlertFriendlyName + " - AFTER"; ;
                                a.latLonStart = gps.lat.ToString() + " | " + gps.lon.ToString();
                                a.latLonEnd = gps.lat.ToString() + "|" + gps.lon.ToString();
                                a.maxVal = "NA";
                                a.runID = this.runID;
                                alerts.Add(a);
                                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                                sql.logAlert(a, this.VehicleID);
                                if (am.NDB == true && this.driver != null)
                                {
                                    //we have a driver behavior issue, log it
                                    driverBehavior db = new driverBehavior();
                                    db.behavior = a.alertName;
                                    db.driverID = this.driver.DriverID;
                                    db.timeStamp = this.lastMessageReceived;
                                    sql.logDriverBehavior(db, this.extendedData.ID);
                                    this.behaviors.Add(db);
                                }
                                if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL"))
                                {
                                    string emls = am.AlertAction.Replace("EMAIL:", "");
                                    if (!string.IsNullOrEmpty(emls))
                                    {
                                        GlobalData.sendMail sm = new GlobalData.sendMail();
                                        sm.sendAlertEmail(emls, a, this, string.Empty);
                                    }
                                }
                            }
                        }
                    }

                    
                }
            }
            catch (Exception ex) {
                string err = ex.ToString();
            }
        }

        #endregion

        #region " GeoFence Checks "

        /// <summary>
        /// Determine if vehicle is in a geo fence
        /// Set alert types 0 & 1 accordingly
        /// </summary>
        /// <param name="gps">GPS message</param>
        public void checkGeoFences(Messages.GPSData gps)
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                
                insidePolyName = GeoCode.GlobalGeo.findInside(gps.lat, gps.lon);
                //if (insidePolyName != null)
                if(!string.IsNullOrEmpty(insidePolyName.polyName))
                {
                    checkStationaryInGeoAlert(gps, insidePolyName, true);
                    
                    if (!isInside) //this gets reset at the end of the function
                    {
                        bool raiseAlert = false;
                        //this means we're entering a polygon. Check to see if this vehicle has an enter alert specified
                        //for this polygon
                        alertGeoFence agf = findGeoAlert("ENTER POLYGON", this.extendedData.ID, insidePolyName.geoFenceID);
                        Models.alertModel am = null;
                        if (agf != null) {
                            am = availAlerts.Find(delegate (alertModel find) { return find.AlertID == agf.AlertID &&
                            find.AlertActive == true &&
                            find.AlertStartTime <= DateTime.Now.ToUniversalTime() &&
                            find.AlertEndTime >= DateTime.Now.ToUniversalTime();
                            });
                            //theoretically, we should have an alert for this since it's part of the LINQ query to get the geoFence info, but
                            //it's best to be safe here.
                            if (am == null)
                            {
                                raiseAlert = false;
                            }
                            else {
                                raiseAlert = true;
                            }
                        }
                        if (raiseAlert)
                        {
                            //vehicle was not in polygon, now is. Create alert status change
                            Models.alert a = new alert();
                            a.alertName = am.AlertFriendlyName + ":" + insidePolyName.polyName;
                            a.alertActive = true;
                            //a.alertStart = DateTime.Now;
                            a.alertStart = DateTime.Now.ToUniversalTime();
                            a.alertEnd = Convert.ToDateTime("01/01/2001 00:00:00");
                            a.alertID = Guid.NewGuid();
                            //a.alertType = am.AlertFriendlyName;
                            a.alertType = am.AlertClassName;
                            a.latLonStart = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.latLonEnd = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.maxVal = "NA";
                            a.runID = runID;
                            alerts.Add(a);
                            sql.logAlert(a, this.VehicleID);
                            if (am.NDB == true && this.driver != null)
                            {
                                //we have a driver behavior issue, log it
                                driverBehavior db = new driverBehavior();
                                db.behavior = a.alertName;
                                db.driverID = this.driver.DriverID;
                                db.timeStamp = DateTime.Now.ToUniversalTime();
                                sql.logDriverBehavior(db, this.extendedData.ID);
                                this.behaviors.Add(db);
                            }

                            if (am != null && !string.IsNullOrEmpty(am.AlertAction)) {
                                //alert action is used to specify who we're emailing when things do stuff.
                                //You can make up your own mind if it's hot alert action
                                if (am.AlertAction.Contains("EMAIL:")) {
                                    string eml = am.AlertAction.Replace("EMAIL:", "");
                                    GlobalData.sendMail send = new GlobalData.sendMail();
                                    send.sendAlertEmail(eml, a, this, string.Empty);
                                }
                            }
                            
                        }
                        #region " Old "
                        /* ths should no longer be necessary. Uncomment if needed
                        else
                        {
                            //vehicle was not in polygon, now is. This is probably a dumped message, create the alert but don't raise it
                            //this should be the only place to worry about for dumped messages
                            Models.alert a = new alert();
                            a.alertName = "ENTER: " + insidePolyName.polyName;
                            a.alertActive = false;
                            //a.alertStart = DateTime.Now;
                            //a.alertEnd = DateTime.Now;
                            a.alertStart = DateTime.Now.ToUniversalTime();
                            a.alertEnd = DateTime.Now.ToUniversalTime();
                            a.alertID = Guid.NewGuid();
                            a.alertType = 0;
                            a.latLonStart = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.latLonEnd = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.maxVal = "NA";
                            a.runID = runID;
                            alerts.Add(a);
                            sql.logAlert(a, this.VehicleID);

                            //If Email then end
                            if (insidePolyName.actionIn == true)
                            {
                                GlobalData.sendMail send = new GlobalData.sendMail();
                                send.sendAlertEmail(insidePolyName.actionInEmail, a, this.VehicleID);
                            }
                        }
                        */
                        #endregion
                        isInside = true;
                        oldPoly = insidePolyName;
                    }
                }
                else{ //was in a polygon, not anymore
                    //insidePolyName = new polygonData();
                    if (isInside)
                    {
                        //vehicle was in polygon, now is not. Create alert status change
                        //Step 0, figure out if we need an alert for exiting this particular polygon
                        bool raiseAlert = false;

                        //this means we're entering a polygon. Check to see if this vehicle has an enter alert specified
                        //for this polygon
                        alertGeoFence agf = null;
                        if (oldPoly != null) {
                            agf = findGeoAlert("EXIT POLYGON", this.extendedData.ID, oldPoly.geoFenceID);
                            checkStationaryInGeoAlert(gps, oldPoly, false); //we're no longer in a polygon, but
                            //by exiting it allows us to shut down any stationary in geo alerts.
                        }

                        Models.alertModel am = null;
                        if (agf != null)
                        {
                            am = availAlerts.Find(delegate (alertModel find) { return find.AlertID == agf.AlertID &&
                            find.AlertActive == true &&
                            find.AlertStartTime <= DateTime.Now.ToUniversalTime() &&
                            find.AlertEndTime >= DateTime.Now.ToUniversalTime();
                            });
                            //theoretically, we should have an alert for this since it's part of the LINQ query to get the geoFence info, but
                            //it's best to be safe here.
                            if (am == null)
                            {
                                raiseAlert = false;
                            }
                            else {
                                raiseAlert = true;
                            }
                        }
                        //Step 1, find original alert
                        string alrtName = string.Empty;
                        Models.alert found = alerts.Find(delegate(Models.alert find)
                        {
                            return find.alertActive == true && find.alertType == "ENTER POLYGON";
                        });
                        //update the existing alert with an end time, set it to inactive, and log it
                        if (found != null)
                        {
                            found.alertActive = false;
                            //found.alertEnd = DateTime.Now;
                            found.alertEnd = DateTime.Now.ToUniversalTime();
                            found.latLonEnd = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            found.maxVal = "NA";
                            alrtName = found.alertName;
                            sql.logAlert(found, this.VehicleID);

                            if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL"))
                            {
                                string emls = am.AlertAction.Replace("EMAIL:", "");
                                if (!string.IsNullOrEmpty(emls))
                                {
                                    GlobalData.sendMail sm = new GlobalData.sendMail();
                                    sm.sendAlertEmail(emls, found, this, string.Empty);
                                }
                            }
                        }
                        //create the alert for leaving the geo fence
                        //this is a single-shot deal, so just log it
                        if (raiseAlert) {
                            //not all polygon exits require an alert
                            Models.alert a = new alert();
                            if (found != null)
                            {
                                a.alertName = alrtName.Replace("ENTER", "EXIT");
                            }
                            else
                            {
                                a.alertName = "EXIT POLYGON";
                            }
                            a.alertID = Guid.NewGuid();
                            a.alertActive = false;
                            //a.alertStart = DateTime.Now;
                            //a.alertEnd = DateTime.Now;
                            a.alertStart = DateTime.Now.ToUniversalTime();
                            a.alertEnd = DateTime.Now.ToUniversalTime();
                            a.alertType = am.AlertFriendlyName;
                            a.latLonStart = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.latLonEnd = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.runID = runID;
                            a.maxVal = "NA";
                            alerts.Add(a);
                            sql.logAlert(a, this.VehicleID);
                            if (am.NDB == true && this.driver != null)
                            {
                                //we have a driver behavior issue, log it
                                driverBehavior db = new driverBehavior();
                                db.behavior = a.alertName;
                                db.driverID = this.driver.DriverID;
                                db.timeStamp = DateTime.Now.ToUniversalTime();
                                sql.logDriverBehavior(db, this.extendedData.ID);
                                this.behaviors.Add(db);
                            }
                            if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL"))
                            {
                                string emls = am.AlertAction.Replace("EMAIL:", "");
                                if (!string.IsNullOrEmpty(emls))
                                {
                                    GlobalData.sendMail sm = new GlobalData.sendMail();
                                    sm.sendAlertEmail(emls, a, this, string.Empty);
                                }
                            }
                        }
                    }
                    isInside = false;
                    if (oldPoly != null) {
                        oldPoly = null; 
                    }
                    insidePolyName.polyName = "NA";
                }
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in checkGeoFences: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
            }
        }

        public alertGeoFence findGeoAlert(string alertClass, Guid vehicleID, Guid geoFenceID)
        {
            //this needs to be altered to use just an alertid. we're not loading all alert models, those are handled per-vehicle
            var gfList = from am in availAlerts
                         join av in GlobalData.GlobalData.alertVehicles on am.AlertID equals av.AlertID
                         join agf in GlobalData.GlobalData.alertGeoFences on av.AlertID equals agf.AlertID
                         join g in GeoCode.GlobalGeo.polygons on agf.GeoFenceID equals g.geoFenceID
                         join v in GlobalData.GlobalData.vehicles on av.VehicleID equals v.extendedData.ID
                         where g.geoFenceID == geoFenceID && v.extendedData.ID == vehicleID && am.AlertClassName == alertClass
                         select agf;
            alertGeoFence found = null;
            foreach (alertGeoFence agf in gfList)
            {
                found = agf;
            }
            return found;
        }

        #endregion

        #region " Speeding "

        private void checkSpeeding(double spd)
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                bool isSpeeding = GlobalData.GlobalData.isSpeeding(spd);
                alert found = alerts.Find(delegate (alert find) { return find.alertName == "SPEEDING" && find.alertActive == true; });
                bool raiseAlert = false;
                Models.alertModel am = availAlerts.Find(delegate (alertModel find) { return find.AlertClassName == "SPEEDING" &&
                            find.AlertActive == true &&
                            find.AlertStartTime <= DateTime.Now.ToUniversalTime() &&
                            find.AlertEndTime >= DateTime.Now.ToUniversalTime();
                });
                if (am == null) {
                    //no alert defined for speeding, kick out
                    return;
                }
                int alertThreshold = 0;
                int threshhold = 0;
                systemvar sv = GlobalData.GlobalData.appVars.Find(delegate (systemvar find) { return find.varName.ToUpper() == "SPEEDING"; });
                if (sv != null)
                {
                    threshhold = Convert.ToInt32(sv.varVal);
                }
                if (Int32.TryParse(am.minVal, out alertThreshold))
                {
                    alertThreshold = Convert.ToInt32(am.minVal);
                }
                if (alertThreshold > 0)
                {
                    threshhold = alertThreshold;
                }
                if (am == null)
                {
                    raiseAlert = false;
                }
                else {
                    raiseAlert = true;
                }

                if (raiseAlert)
                {
                    //check the alert threshold
                    
                    string Email = GlobalData.GlobalData.isSpeedingEmail();
                    //find speeding alerts where active = true
                    found = alerts.Find(delegate (alert find) { return find.alertType == "SPEEDING" && find.alertActive == true; });
                    if (found == null && gps.spd > threshhold) // this means there's currently no speeding alert, make a new one
                    {
                        if (raiseAlert)
                        {
                            alert a = new alert();
                            a.alertID = Guid.NewGuid();
                            a.alertName = am.AlertFriendlyName;
                            a.alertType = am.AlertFriendlyName;
                            //a.alertStart = DateTime.Now;
                            a.alertStart = DateTime.Now.ToUniversalTime();
                            a.latLonStart = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.latLonEnd = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                            a.alertEnd = Convert.ToDateTime("01/01/2001 00:00:00");
                            a.maxVal = this.gps.spd.ToString();
                            a.alertActive = true;
                            a.runID = runID;
                            alerts.Add(a);
                            sql.logAlert(a, this.VehicleID);
                            sql.logAlert(a, this.VehicleID);
                            if (am.NDB == true && this.driver != null)
                            {
                                //we have a driver behavior issue, log it
                                driverBehavior db = new driverBehavior();
                                db.behavior = a.alertName;
                                db.driverID = this.driver.DriverID;
                                db.timeStamp = DateTime.Now.ToUniversalTime();
                                sql.logDriverBehavior(db, this.extendedData.ID);
                                this.behaviors.Add(db);
                            }
                            if (!string.IsNullOrEmpty(am.AlertAction) && am.AlertAction.Contains("EMAIL:"))
                            {
                                string eml = am.AlertAction.Replace("EMAIL:", "");
                                GlobalData.sendMail send = new GlobalData.sendMail();
                                send.sendAlertEmail(eml, a, this, gps.spd.ToString());
                            }
                        }
                    }
                }

                if (found != null && gps.spd < threshhold)
                {
                    found.alertActive = false;
                    //found.alertEnd = DateTime.Now;
                    found.alertEnd = DateTime.Now.ToUniversalTime();
                    found.latLonEnd = this.gps.lat.ToString() + "|" + this.gps.lon.ToString();
                    sql.logAlert(found, this.VehicleID);
                }
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
                GlobalData.Logger logger = new GlobalData.Logger(this.VehicleID + ".txt");
                logger.writeToLogFile("ERROR in checkSpeeding: " + Environment.NewLine + DateTime.Now.ToString() + Environment.NewLine + err);
            }
        }

        #endregion

    }
}
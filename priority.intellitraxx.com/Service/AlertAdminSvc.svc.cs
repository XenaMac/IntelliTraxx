using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AlertAdminSvc" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AlertAdminSvc.svc or AlertAdminSvc.svc.cs at the Solution Explorer and start debugging.
    public class AlertAdminSvc : IAlertAdminSvc
    {

        #region " Get Data "

        #region " Alerts "

        public List<alertClass> getAlertClasses() {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<alertClass> aList = sql.getAlertClasses();
            return aList;
        }

        public List<alertVehicle> getAlertVehicles() {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<alertVehicle> aList = sql.getVehicleAlerts();
            return aList;
        }

        public List<alertGeoFence> getAlertGeoFences() {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<alertGeoFence> aList = sql.getGeoFenceAlerts();
            return aList;
        }

        public List<polyData> getPolygons() {
            try {
                List<polyData> polyList = new List<polyData>();
                foreach (Models.polygonData p in GeoCode.GlobalGeo.polygons)
                {
                    polyData pd = new polyData();
                    pd.polyID = p.geoFenceID;
                    pd.polyName = p.polyName;
                    polyList.Add(pd);
                }
                return polyList;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public List<dbAlert> getAlerts() {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<dbAlert> aList = sql.getdbAlerts();
            return aList;
        }

        public List<linkVehicle> getAllLinkVehicles() {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<linkVehicle> lvList = sql.getLinkVehicles();
            return lvList;
        }

        public List<string> getLinkedAlertsVehicles(string alertFriendlyName) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<string> vList = sql.getLinkedAlertsVehicles(alertFriendlyName);
                return vList;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public List<string> getLinkedAlertsGeoFences(string alertFriendlyName) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<string> gList = sql.getLinkedAlertsGeoFences(alertFriendlyName);
                return gList;
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public alertData getAlertData(Guid alertID)
        {
            alertData alert = new alertData();
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                alert.alert = sql.getdbAlerts().Where(a => a.AlertID == alertID).FirstOrDefault();
                alert.alertVehicles = sql.getVehicleAlerts().Where(a => a.AlertID == alertID).ToList();
                //GlobalData.GlobalData.alertVehicles.Where(v => v.AlertID == alertID).ToList();
                alert.alertGeoFences = GlobalData.GlobalData.alertGeoFences.Where(v => v.AlertID == alertID).ToList();
                return alert;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region " Schedules "

        public List<schedule> getAllSchedules()
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                List<schedule> ret = new List<schedule>();
                List<schedule> schedList = sql.getAllSchedules();
                foreach (schedule s in schedList)
                {
                    ret.Add(s);
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public List<vsLink> getAllSchedulesByVehicle(string VehicleID) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                return sql.getAllSchedulesByVehicle(VehicleID);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public List<vsLink> getAllVehiclesBySchedule(Guid scheduleID) {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                return sql.getAllVehiclesBySchedule(scheduleID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion

        #region " Update Data "

        #region " Alerts "

        public string updateAlertData(dbAlert a, List<alertGeoFence> agfList, List<alertVehicle> avList)
        {
            try
            {
                //This is a multistage process to update alerts for vehicles
                //step 1 is to examine the dbAlert. If it's missing information, drop this whole thing and return an error.
                //step 2 is to log the alert to the database - SQL scripts will create/update as necessary
                //step 3 is to examine the geo fence information. If it's null, no worries. If it's not,
                //update the GlobalData.GlobalData.alertGeoFences object.
                //step 4, log the geo data to the database
                //step 5, examine the avlist. If it's null, no worries. If not, we have to loop through all the vehicles in the system
                //at the moment and see if this alert pertains to them. Update each vehicle's alert as necessary
                //step 6, log the avlist to the database
                GlobalData.SQLCode sql = new GlobalData.SQLCode();

                //Step 1 & 2
                if (a != null)
                {
                    checkAlert(a);
                    sql.updateAlert(a);
                }
                //step 3 & 4
                if (agfList != null && agfList.Count > 0)
                {
                    updateGeoFences(a, agfList);
                }
                //step 5 & 6
                if (avList != null && avList.Count > 0)
                {
                    updateAlertsVehicles(avList, a);
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public string deleteAlert(dbAlert a)
        {
            string ret = "OK";
            try
            {
                for (int i = GlobalData.GlobalData.alertGeoFences.Count - 1; i > -0; i--)
                {
                    if (GlobalData.GlobalData.alertGeoFences[i].AlertID == a.AlertID)
                    {
                        GlobalData.GlobalData.alertGeoFences.RemoveAt(i);
                    }
                }
                for (int i = GlobalData.GlobalData.alertGeoFences.Count - 1; i > -0; i--)
                {
                    if (GlobalData.GlobalData.alertGeoFences[i].AlertID == a.AlertID)
                    {
                        GlobalData.GlobalData.alertGeoFences.RemoveAt(i);
                    }
                }
                foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles)
                {
                    for (int i = v.availAlerts.Count - 1; i >= 0; i--)
                    {
                        if (v.availAlerts[i].AlertID == a.AlertID)
                        {
                            v.availAlerts.RemoveAt(i);
                        }
                    }
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteAlert(a);
                return ret;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        private string checkAlert(dbAlert a)
        {
            try
            {
                string ret = "OK";
                if (a.AlertID == Guid.Empty)
                {
                    throw new Exception("BADGUID");
                }
                if (string.IsNullOrEmpty(a.AlertFriendlyName))
                {
                    throw new Exception("BADFRIENDLYNAME");
                }
                if (a.AlertEndTime < DateTime.Now)
                {
                    throw new Exception("BADENDDATE");
                }

                return ret;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void updateGeoFences(dbAlert a, List<alertGeoFence> agfList)
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            for (int i = GlobalData.GlobalData.alertGeoFences.Count - 1; i >= 0; i--)
            {
                if (GlobalData.GlobalData.alertGeoFences[i].AlertID == a.AlertID)
                {
                    GlobalData.GlobalData.alertGeoFences.RemoveAt(i);
                }
            }
            try
            {
                foreach (alertGeoFence agf in agfList)
                {
                    if (agf.AlertID == Guid.Empty)
                    {
                        throw new Exception("BADALERTID");
                    }
                    if (agf.GeoFenceID == Guid.Empty)
                    {
                        throw new Exception("BADGEOFENCEID");
                    }
                    GlobalData.GlobalData.alertGeoFences.Add(agf);
                }
                sql.updateAlertsGeoFences(a, agfList);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void updateAlertsVehicles(List<alertVehicle> avList, dbAlert a)
        {
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                for (int i = GlobalData.GlobalData.alertVehicles.Count - 1; i >= 0; i--)
                { //clean up the existing data
                    if (GlobalData.GlobalData.alertVehicles[i].AlertID == a.AlertID)
                    {
                        GlobalData.GlobalData.alertVehicles.RemoveAt(i);
                    }
                }
                foreach (alertVehicle av in avList)
                {
                    //do some checking on the list items
                    if (av.AlertID == Guid.Empty)
                    {
                        throw new Exception("BADALERTID");
                    }
                    if (av.VehicleID == Guid.Empty)
                    {
                        throw new Exception("BADVEHICLEID");
                    }
                    GlobalData.GlobalData.alertVehicles.Add(av);

                    //find the vehicle this thing pertains to
                    Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) {
                        return find.extendedData.ID == av.VehicleID;
                    });
                    if (v != null)
                    {
                        Models.alertModel am = v.availAlerts.Find(delegate (Models.alertModel find) { return find.AlertID == av.AlertID; });
                        if (am != null)
                        {
                            am.AlertAction = av.AlertAction;
                            am.AlertActive = a.AlertActive;
                            am.AlertClassName = sql.getAlertClassNameFromID(a.AlertClassID);
                            am.AlertEndTime = a.AlertEndTime;
                            am.AlertFriendlyName = a.AlertFriendlyName;
                            am.AlertStartTime = a.AlertStartTime;
                            am.AlertType = a.AlertType;
                        }
                        else
                        {
                            am = new Models.alertModel();
                            am.AlertAction = av.AlertAction;
                            am.AlertActive = a.AlertActive;
                            am.AlertClassName = sql.getAlertClassNameFromID(a.AlertClassID);
                            am.AlertEndTime = a.AlertEndTime;
                            am.AlertFriendlyName = a.AlertFriendlyName;
                            am.AlertStartTime = a.AlertStartTime;
                            am.AlertType = a.AlertType;
                            am.AlertID = a.AlertID;
                            v.availAlerts.Add(am);
                        }
                    }
                }
                //finally, update the database with the new information
                sql.updateAlertsVehicles(a, avList);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string changeAlertStatus(List<Guid> aList, bool enable, bool updatedb) {
            string ret = "OK";
            try {
                if (updatedb) {
                    GlobalData.SQLCode sql = new GlobalData.SQLCode();
                    sql.changeAlertStatus(aList, enable);
                }
                foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles) {
                    foreach (Models.alertModel am in v.availAlerts) {
                        for (int i = 0; i < aList.Count - 1; i++) {
                            if (am.AlertID == aList[i]) {
                                am.AlertActive = enable;
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

        #region " Schedules "

        public string changeSchedule(string VehicleID, DateTime start, DateTime end) {
            string ret = "OK";
            try {
                //this should only be a temporary modification for a single vehicle.
                //to completely update a schedule in the db for all vehicles, use updateSchedules
                Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) { return find.extendedData.vehicleID == VehicleID; });
                if (v != null) {
                    v.sched.dtStart = start;
                    v.sched.dtEnd = end;
                }
                return ret;
            }
            catch (Exception ex) {
                return ex.Message;
            }
        }

        public string updateSchedules(List<schedule> sList) {
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                string ret = "OK";

                //step 1 : look for trucks with new schedules and update accordingly
                foreach (schedule s in sList) {
                    foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles)
                    {
                        if(v.sched.scheduleID.ToString() == "00000000-0000-0000-0000-000000000000")
                        {
                            v.sched = sql.getDailyScheduleForVehicle(v.extendedData.vehicleID);
                        }
                        else if (v.sched != null && v.sched.scheduleID == s.scheduleID)
                        {
                            v.sched.dtEnd = s.endTime;
                            v.sched.dtStart = s.startTime;
                        }
                    }
                }
                //step 2 : pass the whole list to sql server for updating
                sql.updateSchedules(sList);
                return ret;
            }
            catch (Exception ex) {

                return ex.Message;
            }
        }

        public string deleteSchedules(List<schedule> dSchedules) {
            string ret = "OK";
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                foreach (schedule s in dSchedules) {
                    //step 1: find vehicles with those schedules
                    foreach (Models.Vehicle v in GlobalData.GlobalData.vehicles) {
                        if (v.sched.scheduleID == s.scheduleID) {
                            v.sched = null;
                        }
                    }
                    //step 2: remove the schedules from the database
                    sql.deleteSchedule(s.scheduleID);
                }
            }
            catch (Exception ex) {
                ret = ex.Message;
            }
            return ret;
        }

        public string deleteSchedulesBySchedule(Guid scheduleID) {
            string ret = "OK";
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteSchedulesBySchedule(scheduleID);
            }
            catch (Exception ex) {
                return ex.Message;
            }
            return ret;
        }

        public string deleteSchedulesByVehicle(string vehicleID) {
            string ret = "OK";
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.deleteSchedulesByVehicle(vehicleID);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return ret;
        }

        public string addVehicleToSchedule(Guid scheduleID, List<string> vehicleIDs) {
            string ret = "OK";
            try {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.addVehicleToSchedule(scheduleID, vehicleIDs);
                return ret;
            }
            catch (Exception ex) {
                return ex.Message;
            }
        }

        public string addSchedulesToVehicle(string vehicleID, List<Guid> scheduleList) {
            string ret = "OK";
            try
            {
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.addScheduleToVehicle(vehicleID, scheduleList);
                return ret;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #endregion
    }
}

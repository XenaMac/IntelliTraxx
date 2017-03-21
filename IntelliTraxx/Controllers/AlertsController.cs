using IntelliTraxx.TruckService;
using IntelliTraxx.AlertAdminService;
using IntelliTraxx.PolygonService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Serialization;

namespace IntelliTraxx.Controllers
{
    public class AlertsController : Controller
    {
        TruckServiceClient truckService = new TruckServiceClient();
        AlertAdminSvcClient alertService = new AlertAdminSvcClient();
        PolygonServiceClient polygonService = new PolygonServiceClient();

        // GET: Alerts
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Admin()
        {
            return View();
        }

        //GET: All Alerts
        public ActionResult getAllAlerts()
        {
            List<dbAlerts> Alerts = new List<dbAlerts>();
            List<dbAlert> alerts = alertService.getAlerts();
            List<alertClass> classes = alertService.getAlertClasses();

            foreach(dbAlert a in alerts)
            {
                dbAlerts newAlert = new dbAlerts();
                newAlert.AlertActive = a.AlertActive;
                newAlert.AlertClassID = a.AlertClassID;
                newAlert.AlertClassName = classes.Find(x => x.AlertClassID == a.AlertClassID).AlertClassName;
                newAlert.AlertEndTime = a.AlertEndTime;
                newAlert.AlertFriendlyName = a.AlertFriendlyName;
                newAlert.AlertID = a.AlertID;
                newAlert.AlertStartTime = a.AlertStartTime;
                newAlert.AlertType = a.AlertType;
                newAlert.ExtensionData = a.ExtensionData;
                newAlert.minVal = a.minVal;
                Alerts.Add(newAlert);
            }

            return Json(Alerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            var allVehicles = truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        //GET: All Alerts
        public ActionResult getAllAlertsByRangeByType(DateTime from, DateTime to, string type)
        {
            List<alertReturn> alerts = truckService.getAllAlertsByRangeByType(from, to, type);
            return Json(alerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllAlertsByRangeByVehicle(DateTime from, DateTime to, string vehicleid)
        {
            List<alertReturn> alerts = truckService.getAllAlertsByRangeByVehicle(from, to, vehicleid);
            return Json(alerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAlert(string alertID)
        {
            Guid id = new Guid(alertID);
            alertReturn alert = null;
            alert = truckService.getAllAlertByID(new Guid(alertID));

            return Json(alert, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAlertHistory(string alertID, string vehicleID)
        {
            AlertHistory AH = new AlertHistory();
            alertReturn alert = truckService.getAllAlertByID(new Guid(alertID));
            alert.alertStart = alert.alertStart.AddMinutes(-2);
            alert.alertEnd = alert.alertEnd.ToString() != "1/1/2001 12:00:00 AM" ? alert.alertEnd.AddMinutes(2) : alert.alertStart.AddMinutes(5);
            AH.Alert = alert;

            AH.Locations = truckService.getGPSTracking(vehicleID, alert.alertStart, alert.alertEnd);

            return Json(AH, JsonRequestBehavior.AllowGet);
        }

        public ActionResult changeAlertStatus(Guid[] aList, bool enabled, bool updatedb)
        {
            var ret = alertService.changeAlertStatus(aList.ToList<Guid>(), enabled, updatedb);

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAlertClasses()
        {
            var classes = alertService.getAlertClasses();

            return Json(classes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllFences()
        {
            var Fences = polygonService.getPolygons();

            return Json(Fences, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getLinkedAlertsVehicles(string alertFriendlyName)
        {
            List<Vehicle> vehicles = truckService.getAllVehicles(true);
            List<string> linkedVehicles = alertService.getLinkedAlertsVehicles(alertFriendlyName);
            List<Vehicle> linkedAlertVehicles = new List<Vehicle>();
            foreach(string id in linkedVehicles)
            {
                Guid Vid = new Guid(id);
                Vehicle v = vehicles.Where(veh => veh.extendedData.ID == Vid).FirstOrDefault();
                linkedAlertVehicles.Add(v);
            }
            return Json(linkedAlertVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getLinkedAlertsFences(string alertFriendlyName)
        {
            List<polyData> allFences = alertService.getPolygons();
            List<string> linkedFences = alertService.getLinkedAlertsGeoFences(alertFriendlyName);
            List<polyData> linkeAlertFences = new List<polyData>();
            foreach (string GeoName in linkedFences)
            {
                polyData p = allFences.Where(ply => ply.polyName == GeoName).FirstOrDefault();
                linkeAlertFences.Add(p);
            }
            return Json(linkeAlertFences, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAlertData(Guid ID)
        {
            alertData AD = alertService.getAlertData(ID);
            extendedAlertData EAD = new extendedAlertData();
            EAD.alert = AD.alert;
            EAD.alertGeoFences = AD.alertGeoFences;
            EAD.alertVehicles = AD.alertVehicles;
            EAD.extendedAlertFences = new List<PolygonService.polygonData>();
            EAD.extendedAlertVehicles = new List<Vehicle>();


            foreach(alertGeoFence gf in AD.alertGeoFences)
            {
                PolygonService.polygonData poly = new PolygonService.polygonData();
                poly = polygonService.getPolygons().Where(p => p.geoFenceID == gf.GeoFenceID).FirstOrDefault();
                EAD.extendedAlertFences.Add(poly);
            }

            foreach(alertVehicle av in AD.alertVehicles)
            {
                Vehicle v = new Vehicle();
                v = truckService.getAllVehicles(true).Where(vh => vh.extendedData.ID == av.VehicleID).FirstOrDefault();
                EAD.extendedAlertVehicles.Add(v);
            }

            return Json(EAD, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult updateAlertData(string alertClassID, string alertClassName, string alertName, string startDate, string endDate, List<polygonID> polygonIDs, List<polygonNames> polygonNames, List<AV> alertVehicles, string alertValue)
        {
            DateTime s = DateTime.Now;
            DateTime e = DateTime.Now.AddYears(5);

            if (startDate != "")
            {
                s = Convert.ToDateTime(startDate);
            }

            if (endDate != "")
            {
                e = Convert.ToDateTime(endDate);
            }

            //create alert class
            dbAlert alert = new dbAlert();
            alert.AlertID = Guid.NewGuid();
            alert.AlertActive = true;
            alert.AlertStartTime = s;
            alert.AlertEndTime = e;
            alert.AlertType = "N";
            alert.AlertClassID = new Guid(alertClassID);
            alert.AlertFriendlyName = alertName;
            alert.minVal = alertValue;

            //create polygon list
            List<alertGeoFence> fences = new List<alertGeoFence>();
            foreach (polygonID pg in polygonIDs)
            {
                alertGeoFence fence = new alertGeoFence();
                fence.AlertID = alert.AlertID;
                fence.GeoFenceID = new Guid(pg.id);
                fences.Add(fence);
            }

            //list of vehicles
            List<alertVehicle> vehicles = new List<alertVehicle>();
            foreach (AV veh in alertVehicles)
            {
                alertVehicle vehicle = new alertVehicle();
                vehicle.AlertAction = veh.email;
                vehicle.AlertID = alert.AlertID;
                vehicle.VehicleID = new Guid(veh.id);
                vehicles.Add(vehicle);
            }

            string results = alertService.updateAlertData(alert, fences, vehicles);

            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult deleteAlert(dbAlert alert)
        {
            string result = alertService.deleteAlert(alert);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class AlertHistory
        {
            public alertReturn Alert { get; set; }
            public List<VehicleGPSRecord> Locations { get; set; }
        }

        public class dbAlerts : dbAlert
        {
            public string AlertClassName { get; set; }
        }

        public class AV
        {
            public string id { get; set; }
            public string name { get; set; }
            public string email { get; set; }
        }

        public class polygonID
        {
            public string id { get; set; }
        }

        public class polygonNames
        {
            public string Name { get; set; }
        }

        public class extendedAlertData : alertData
        {
            public List<Vehicle> extendedAlertVehicles { get; set; }

            public List<PolygonService.polygonData> extendedAlertFences { get; set;}
        }
    }
}

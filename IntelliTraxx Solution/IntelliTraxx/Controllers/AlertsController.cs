using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IntelliTraxx.Shared.AlertAdminService;
using IntelliTraxx.Shared.PolygonService;
using IntelliTraxx.Shared.TruckService;
using polygonData = IntelliTraxx.Shared.PolygonService.polygonData;

namespace IntelliTraxx.Controllers
{
    public class AlertsController : Controller
    {
        private readonly AlertAdminSvcClient _alertService = new AlertAdminSvcClient();
        private readonly PolygonServiceClient _polygonService = new PolygonServiceClient();
        private readonly TruckServiceClient _truckService = new TruckServiceClient();

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
            var Alerts = new List<dbAlerts>();
            var alerts = _alertService.getAlerts();
            var classes = _alertService.getAlertClasses().ToList();

            foreach (var a in alerts)
            {
                var newAlert = new dbAlerts();
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
                newAlert.NDB = a.NDB;
                Alerts.Add(newAlert);
            }

            return Json(Alerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            var allVehicles = _truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        //GET: All Alerts
        public ActionResult getAllAlertsByRangeByType(DateTime from, DateTime to, string type)
        {
            var alerts = _truckService.getAllAlertsByRangeByType(from, to, type);
            return Json(alerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllAlertsByRangeByVehicle(DateTime from, DateTime to, string vehicleid)
        {
            var alerts = _truckService.getAllAlertsByRangeByVehicle(from, to, vehicleid);
            return Json(alerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAlert(string alertID)
        {
            var id = new Guid(alertID);
            alertReturn alert = null;
            alert = _truckService.getAllAlertByID(new Guid(alertID));

            return Json(alert, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAlertHistory(string alertID, string vehicleID)
        {
            var AH = new AlertHistory();
            var alert = _truckService.getAllAlertByID(new Guid(alertID));
            alert.alertStart = alert.alertStart.AddMinutes(-2);
            alert.alertEnd = alert.alertEnd.ToString() != "1/1/2001 12:00:00 AM"
                ? alert.alertEnd.AddMinutes(2)
                : alert.alertStart.AddMinutes(5);
            AH.Alert = alert;

            AH.Locations = _truckService
                .getGPSTracking(vehicleID, alert.alertStart.ToString(), alert.alertEnd.ToString()).ToList();

            return Json(AH, JsonRequestBehavior.AllowGet);
        }

        public ActionResult changeAlertStatus(Guid[] aList, bool enabled, bool updatedb)
        {
            var ret = _alertService.changeAlertStatus(aList, enabled, updatedb);

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAlertClasses()
        {
            var classes = _alertService.getAlertClasses();

            return Json(classes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllFences()
        {
            var Fences = _polygonService.getPolygons();

            return Json(Fences, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getLinkedAlertsVehicles(string alertFriendlyName)
        {
            var vehicles = _truckService.getAllVehicles(true);
            var linkedVehicles = _alertService.getLinkedAlertsVehicles(alertFriendlyName);
            var linkedAlertVehicles = new List<Vehicle>();
            foreach (var id in linkedVehicles)
            {
                var Vid = new Guid(id);
                var v = vehicles.FirstOrDefault(veh => veh.extendedData.ID == Vid);
                linkedAlertVehicles.Add(v);
            }

            return Json(linkedAlertVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getLinkedAlertsFences(string alertFriendlyName)
        {
            var allFences = _alertService.getPolygons();
            var linkedFences = _alertService.getLinkedAlertsGeoFences(alertFriendlyName);
            var linkeAlertFences = new List<polyData>();
            foreach (var GeoName in linkedFences)
            {
                var p = allFences.FirstOrDefault(ply => ply.polyName == GeoName);
                linkeAlertFences.Add(p);
            }

            return Json(linkeAlertFences, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAlertData(Guid ID)
        {
            var AD = _alertService.getAlertData(ID);
            var EAD = new extendedAlertData
            {
                alert = AD.alert,
                alertGeoFences = AD.alertGeoFences,
                alertVehicles = AD.alertVehicles,
                extendedAlertFences = new List<polygonData>(),
                extendedAlertVehicles = new List<Vehicle>()
            };


            foreach (var gf in AD.alertGeoFences)
            {
                var poly = new polygonData();
                poly = _polygonService.getPolygons().FirstOrDefault(p => p.geoFenceID == gf.GeoFenceID);
                EAD.extendedAlertFences.Add(poly);
            }

            foreach (var av in AD.alertVehicles)
            {
                var v = new Vehicle();
                v = _truckService.getAllVehicles(true).FirstOrDefault(vh => vh.extendedData.ID == av.VehicleID);
                EAD.extendedAlertVehicles.Add(v);
            }

            return Json(EAD, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult updateAlertData(string alertClassID, string alertClassName, string alertName,
            string editAlertID, string startDate, string endDate, List<polygonID> polygonIDs,
            List<polygonNames> polygonNames, List<AV> alertVehicles, string alertValue, bool TNDB)
        {
            var s = DateTime.Now;
            var e = DateTime.Now.AddYears(5);

            if (startDate != "") s = Convert.ToDateTime(startDate).ToUniversalTime();

            if (endDate != "") e = Convert.ToDateTime(endDate).ToUniversalTime();

            //create alert class
            var alert = new dbAlert();
            if (string.IsNullOrEmpty(editAlertID))
                alert.AlertID = Guid.NewGuid();
            else
                alert.AlertID = new Guid(editAlertID);
            alert.AlertActive = true;
            alert.AlertStartTime = s;
            alert.AlertEndTime = e;
            alert.AlertType = "N";
            alert.AlertClassID = new Guid(alertClassID);
            alert.AlertFriendlyName = alertName;
            alert.minVal = alertValue;
            alert.NDB = TNDB;

            //create polygon list
            var fences = new List<alertGeoFence>();
            if (polygonIDs != null)
                foreach (var pg in polygonIDs)
                {
                    var fence = new alertGeoFence {AlertID = alert.AlertID, GeoFenceID = new Guid(pg.id)};
                    fences.Add(fence);
                }

            //list of vehicles
            var vehicles = new List<alertVehicle>();
            foreach (var veh in alertVehicles)
            {
                var vehicle = new alertVehicle
                {
                    AlertAction = veh.email, AlertID = alert.AlertID, VehicleID = new Guid(veh.id)
                };
                vehicles.Add(vehicle);
            }

            var results = _alertService.updateAlertData(alert, fences.ToArray(), vehicles.ToArray());

            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult deleteAlert(dbAlert alert)
        {
            var result = _alertService.deleteAlert(alert);
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

            public List<polygonData> extendedAlertFences { get; set; }
        }
    }
}
using IntelliTraxx.TruckService;
using IntelliTraxx.AlertAdminService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace IntelliTraxx.Controllers
{
    public class AlertsController : Controller
    {
        TruckServiceClient truckService = new TruckServiceClient();
        AlertAdminSvcClient alertService = new AlertAdminSvcClient();

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

        public class AlertHistory
        {
            public alertReturn Alert { get; set; }
            public List<VehicleGPSRecord> Locations { get; set; }
        }

        public class dbAlerts : dbAlert
        {
            public string AlertClassName { get; set; }
        }
    }
}

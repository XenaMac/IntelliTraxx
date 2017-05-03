using IntelliTraxx.AlertAdminService;
using IntelliTraxx.PolygonService;
using IntelliTraxx.TruckService;
using IntelliTraxx.TabletService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace IntelliTraxx.Controllers
{
    public class FleetController : Controller
    {
        TruckServiceClient truckService = new TruckServiceClient();
        PolygonServiceClient polygonService = new PolygonServiceClient();
        AlertAdminSvcClient alertAdminService = new AlertAdminSvcClient();
        TabletInterfaceClient tabletService = new TabletInterfaceClient();

        // GET: Fleet
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetParentCompanyLocation()
        {
            Company parentCompany = new Company();
            List<Company> companies = truckService.getCompanies(new Guid());

            foreach (Company c in companies)
            {
                if (c.isParent == true)
                {
                    parentCompany = c;
                }
            }

            return Json(parentCompany.CompanyCity + ", " + parentCompany.CompanyState, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVariables()
        {
            var variables = truckService.getVarsAsync();

            return Json(variables, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVehicles()
        {
            var vehicles = truckService.getVehicles();

            return Json(vehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            List<Vehicle> allVehicles = truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVehicleList()
        {
            var vehicleList = truckService.getVehicleList();

            return Json(vehicleList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getGPS(Guid id)
        {
            var vehicleData = truckService.getGPS(id);

            return Json(vehicleData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVehicleData(string id)
        {

            var vehicleData = truckService.getVehicleData(new Guid(id));

            if (vehicleData == null)
            {
                var allVehicles = truckService.getAllVehicles(true);
                foreach (Vehicle v in allVehicles)
                {
                    if (v.extendedData.ID == new Guid(id))
                    {
                        vehicleData = v;
                    }
                }
            }

            return Json(vehicleData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAvailableDrivers()
        {
            List<Driver> availableDrivers = truckService.getAvailableDrivers();
            
            return Json(availableDrivers, JsonRequestBehavior.AllowGet);
        }

        public ActionResult changeDrivers(string from, string to, string vehicleID)
        {
            string result = "";

            result = truckService.changeDrivers(from, to, vehicleID, User.Identity.Name);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Polygon Functions

        [Authorize]
        public ActionResult GetAllFences()
        {
            var Fences = polygonService.getPolygons();

            return Json(Fences, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFence(string id)
        {
            var Fence = polygonService.getPolygon(id);

            return Json(Fence, JsonRequestBehavior.AllowGet);
        }

        public ActionResult addFence(string type, string polyName, string notes, string geofenceID, string geoFence, string radius)
        {
            PolygonService.polygonData polygon = new PolygonService.polygonData();
            List<PolygonService.LatLon> LatLongs = new List<PolygonService.LatLon>();

            polygon.geoType = type;
            polygon.polyName = polyName;
            polygon.notes = notes;
            polygon.geoFenceID = new Guid(geofenceID);

            string[] coords = geoFence.Split(',');
            foreach (string s in coords)
            {
                string[] latlong = s.Split('^');
                PolygonService.LatLon LL = new PolygonService.LatLon();
                LL.Lat = Convert.ToDouble(latlong[0]);
                LL.Lon = Convert.ToDouble(latlong[1]);
                LatLongs.Add(LL);
            }
            polygon.radius = (type == "circle") ? Convert.ToDouble(radius) : 0;
            polygon.geoFence = LatLongs;

            var success = polygonService.addPolygon(polygon);

            return Json(success, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFence(string id)
        {
            polygonService.deletePolygon(new Guid(id));

            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region  Vehicle Alert Functions

        public ActionResult getVehicleAlerts(Guid ID)
        {
            List<Vehicle> vehicles = truckService.getAllVehicles(false).Where(a => a.extendedData.ID == ID).ToList();
            List<alert> alerts = new List<alert>();
            foreach (Vehicle v in vehicles)
            {
                foreach (alert a in v.alerts)
                {
                    alerts.Add(a);
                }
            }

            return Json(alerts.OrderByDescending(o => o.alertStart), JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAlerts(string from, string to)
        {
            DateTime today = DateTime.Now.Date; //new DateTime(2016, 5, 16); //;
            DateTime Tomorrow = today.AddDays(1); //new DateTime(2016, 5, 17); //;

            if (from != null)
            {
                today = DateTime.Parse(from);
            }

            if (to != null)
            {
                Tomorrow = DateTime.Parse(to);
            }

            List<alertReturn> Todays = truckService.getAllAlertsByRange(today, Tomorrow);

            return Json(Todays.OrderByDescending(o => o.alertStart), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAlert(string alertID)
        {
            Guid id = new Guid(alertID);
            alertReturn alert = null;
            alert = truckService.getAllAlertByID(new Guid(alertID));

            return Json(alert, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CurrentMonthAlerts()
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            List<alertReturn> alerts = truckService.getAllAlertsByRange(startDate, endDate).OrderByDescending(a => a.alertStart).ToList();

            GridAlerts gridAlerts = new GridAlerts();
            gridAlerts.GridAlertList = new List<GridAlert>();

            List<string> alertTypes = alerts.Select(a => a.alertName).Distinct().ToList();

            foreach (string s in alertTypes)
            {
                GridAlert gridAlert = new GridAlert();
                gridAlert.AlertName = s;
                gridAlert.Alerts = alerts.Where(a => a.alertName == s).ToList();
                gridAlerts.GridAlertList.Add(gridAlert);
            }

            var jsonResult = Json(gridAlerts, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
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

        public ActionResult getGeoFenceAlerts(Guid id)
        {
            List<alertGeoFence> fences = alertAdminService.getAlertGeoFences().Where(gf => gf.GeoFenceID == id).ToList();
            List<dbAlert> alerts = alertAdminService.getAlerts().ToList();
            List<dbAlert> GFAlerts = new List<dbAlert>();

            foreach (alertGeoFence agf in fences)
            {
                foreach (dbAlert dbalert in alerts)
                {
                    if (dbalert.AlertID == agf.AlertID)
                    {
                        GFAlerts.Add(dbalert);
                    }
                }
            }

            return Json(GFAlerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult changeAlertStatus(Guid[] aList, bool enabled, bool updatedb)
        {
            var ret = alertAdminService.changeAlertStatus(aList.ToList<Guid>(), enabled, updatedb);

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult getHistory(string ID, DateTime start, DateTime end)
        {
            var tracking = truckService.getGPSTracking(ID, start, end);

            return Json(tracking, JsonRequestBehavior.AllowGet);
        }





        // ------------------------ Classes -------------------//


        public class AlertHistory
        {
            public alertReturn Alert { get; set; }
            public List<VehicleGPSRecord> Locations { get; set; }
        }

        public class GridAlert
        {
            public string AlertName { get; set; }
            public List<alertReturn> Alerts { get; set; }
        }

        public class GridAlerts
        {
            public List<GridAlert> GridAlertList { get; set; }
        }

        public class availableDriver
        {
            public Guid DriverID { get; set; }
            public string DriverName { get; set; }
        }
    }
}
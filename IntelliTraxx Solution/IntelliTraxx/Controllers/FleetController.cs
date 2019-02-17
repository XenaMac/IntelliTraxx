using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using IntelliTraxx.Shared.AlertAdminService;
using IntelliTraxx.Shared.PolygonService;
using IntelliTraxx.Shared.TabletService;
using IntelliTraxx.Shared.TruckService;
using RestSharp;

namespace IntelliTraxx.Controllers
{
    public class FleetController : Controller
    {
        readonly TruckServiceClient _truckService = new TruckServiceClient();
        readonly PolygonServiceClient _polygonService = new PolygonServiceClient();
        readonly AlertAdminSvcClient _alertAdminService = new AlertAdminSvcClient();
        TabletInterfaceClient _tabletService = new TabletInterfaceClient();

        // GET: Fleet
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetParentCompanyLocation()
        {
            Company parentCompany = new Company();
            var companies = _truckService.getCompanies(new Guid());

            foreach (Company c in companies)
            {
                if (c.isParent == true)
                {
                    parentCompany = c;
                }
            }

            return Json(parentCompany.CompanyAddress + "," + parentCompany.CompanyCity + ", " + parentCompany.CompanyState, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVariables()
        {
            var variables = _truckService.getVarsAsync();

            return Json(variables, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVehicles()
        {
            var vehicles = _truckService.getVehicles();

            return Json(vehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            var allVehicles = _truckService.getAllVehicles(loadHistorical);
            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVehicleList()
        {
            var vehicleList = _truckService.getVehicleList();

            return Json(vehicleList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getGPS(Guid id)
        {
            var vehicleData = _truckService.getGPS(id);

            return Json(vehicleData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVehicleData(string id)
        {

            var vehicleData = _truckService.getVehicleData(new Guid(id));

            if (vehicleData == null)
            {
                var allVehicles = _truckService.getAllVehicles(true);
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
            List<Driver> availableDrivers = _truckService.getAvailableDrivers().OrderBy(d => d.DriverFirstName).ToList();
            
            return Json(availableDrivers, JsonRequestBehavior.AllowGet);
        }

        public ActionResult changeDrivers(string from, string to, string vehicleID)
        {
            string result = "";

            result = _truckService.changeDrivers(from, to, vehicleID, User.Identity.Name);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult removeDriver(string from, string vehicleID)
        {
            string result = "";

            result = _truckService.removeDriver(from, vehicleID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult killVehilce(string from, string VehicleID)
        {
            var VehilceDead = _truckService.killVehicle(VehicleID);

            return Json(VehilceDead, JsonRequestBehavior.AllowGet);
        }

        #region Polygon Functions

        [Authorize]
        public ActionResult GetAllFences()
        {
            var Fences = _polygonService.getPolygons();

            return Json(Fences, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFence(string id)
        {
            var Fence = _polygonService.getPolygon(id);

            return Json(Fence, JsonRequestBehavior.AllowGet);
        }

        public ActionResult addFence(string type, string polyName, string notes, string geofenceID, string geoFence, string radius)
        {
            var polygon = new Shared.PolygonService.polygonData();
            var latLongs = new List<Shared.PolygonService.LatLon>();

            polygon.geoType = type;
            polygon.polyName = polyName;
            polygon.notes = notes;
            polygon.geoFenceID = new Guid(geofenceID);

            var coords = geoFence.Split(',');
            foreach (string s in coords)
            {
                var latlong = s.Split('^');
                var LL = new Shared.PolygonService.LatLon
                {
                    Lat = Convert.ToDouble(latlong[0]), Lon = Convert.ToDouble(latlong[1])
                };
                latLongs.Add(LL);
            }
            polygon.radius = (type == "circle") ? Convert.ToDouble(radius) : 0;
            polygon.geoFence = latLongs.ToArray();

            var success = _polygonService.addPolygon(polygon);

            return Json(success, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFence(string id)
        {
            _polygonService.deletePolygon(new Guid(id));

            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region  Vehicle Alert Functions

        public ActionResult getVehicleAlerts(Guid ID)
        {
            List<Vehicle> vehicles = _truckService.getAllVehicles(false).Where(a => a.extendedData.ID == ID).ToList();
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
            DateTime tomorrow = today.AddDays(1); //new DateTime(2016, 5, 17); //;

            if (from != null)
            {
                today = DateTime.Parse(from);
            }

            if (to != null)
            {
                tomorrow = DateTime.Parse(to);
            }

            var todays = _truckService.getAllAlertsByRange(today, tomorrow);

            return Json(todays.OrderByDescending(o => o.alertStart), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAlert(string alertID)
        {
            Guid id = new Guid(alertID);
            alertReturn alert = null;
            alert = _truckService.getAllAlertByID(new Guid(alertID));

            return Json(alert, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CurrentMonthAlerts()
        {
            DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            List<alertReturn> alerts = _truckService.getAllAlertsByRange(startDate, endDate).OrderByDescending(a => a.alertStart).ToList();

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
            alertReturn alert = _truckService.getAllAlertByID(new Guid(alertID));
            alert.alertStart = alert.alertStart.AddMinutes(-2);
            alert.alertEnd = alert.alertEnd.ToString() != "1/1/2001 12:00:00 AM" ? alert.alertEnd.AddMinutes(2) : alert.alertStart.AddMinutes(5);
            AH.Alert = alert;

            AH.Locations = _truckService.getGPSTracking(vehicleID, alert.alertStart.ToString(), alert.alertEnd.ToString()).ToList();

            return Json(AH, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getGeoFenceAlerts(Guid id)
        {
            List<alertGeoFence> fences = _alertAdminService.getAlertGeoFences().Where(gf => gf.GeoFenceID == id).ToList();
            List<dbAlert> alerts = _alertAdminService.getAlerts().ToList();
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
            var ret = _alertAdminService.changeAlertStatus(aList.ToArray(), enabled, updatedb);

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public ActionResult getHistory(string ID, string start, string end)
        {
            var tracking = _truckService.getGPSTracking(ID, start, end);

            return Json(tracking.OrderBy(t => t.timestamp), JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllRouters()
        {
            var client = new RestClient("https://www.cradlepointecm.com/api/v2/routers/");
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "a4586ada-91c0-b9a3-c93b-fba498301e98");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("x-ecm-api-key", "b9e2467d6913a936b59334c5c091cc17b349cbf1");
            request.AddHeader("x-ecm-api-id", "00218660-47fc-431e-a569-b49d61d7a7b9");
            request.AddHeader("x-cp-api-key", "8bbe2a520e19f8d28c668a32d47dd44c");
            request.AddHeader("x-cp-api-id", "a73571e3");
            request.AddHeader("content-type", "application/json");
            var response = client.Execute<JsonResult>(request);

            return Json(response.Data.Data, JsonRequestBehavior.AllowGet);
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
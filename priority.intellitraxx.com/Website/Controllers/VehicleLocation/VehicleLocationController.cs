using Base_AVL.LATAService;
using Base_AVL.PolygonService;
using Base_AVL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Base_AVL.Controllers.VehicleLocation
{
    public class VehicleLocationController : MessageControllerBase
    {
        TruckServiceClient truckService = new TruckServiceClient();
        PolygonServiceClient polygonService = new PolygonServiceClient();

        // GET: VehicleLocation
        [CheckSessionOut]
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [CheckSessionOut]
        [Authorize]
        public ActionResult GetVehicleAndDriver(string vehicleID)
        {
            Vehicle_Driver VD = new Vehicle_Driver();
            var vehicle = truckService.getAllVehicles().Where(v => v.VehicleID == vehicleID).SingleOrDefault();
            VD.Driver = vehicle.driver;
            if (vehicle.driver.imageData != null)
            {
                VD.DriverImage64String = Convert.ToBase64String(vehicle.driver.imageData);
            }
            VD.Vehicle = vehicle.extendedData;
            VD.Alerts = vehicle.alerts;
            VD.gps = vehicle.gps;
            VD.lastMessageReceived = vehicle.lastMessageReceived;
            
            return Json(VD, JsonRequestBehavior.AllowGet);
        }

        #region get vehicles calls
        [CheckSessionOut]
        [Authorize]
        public ActionResult GetAllVehicles()
        {
            var Vehicles = truckService.getAllVehiclesAsync();

            return Json(Vehicles, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [Authorize]
        public ActionResult GetVehicles()
        {
            var Vehicles = truckService.getVehiclesAsync();

            return Json(Vehicles, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [Authorize]
        public ActionResult GetVehicleList()
        {
            var Vehicles = truckService.getVehicleListAsync();

            return Json(Vehicles, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [Authorize]
        public ActionResult GetLastTwoHours(string vehicleID)
        {
            var tracking = truckService.getLastTwoHours(vehicleID);

            return Json(tracking, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region get variables
        [CheckSessionOut]
        [Authorize]
        public ActionResult getVariables()
        {
            var variables = truckService.getVarsAsync();

            return Json(variables, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Polygon Functions
        [CheckSessionOut]
        [Authorize]
        public ActionResult GetAllFences()
        {
            var Fences = polygonService.getPolygons();

            return Json(Fences, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [Authorize]
        public ActionResult GetFence(string id)
        {
            var Fence = polygonService.getPolygon(id);

            return Json(Fence, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [Authorize]
        public ActionResult addFence(string type, string polyName, string notes, string geofenceID, string geoFence, string radius, bool actionOut, string actionOutEmail, bool actionIn, string actionInEmail)
        {
            Base_AVL.PolygonService.polygonData polygon = new Base_AVL.PolygonService.polygonData();
            List<Base_AVL.PolygonService.LatLon> LatLongs = new List<Base_AVL.PolygonService.LatLon>();

            polygon.geoType = type;
            polygon.polyName = polyName;
            polygon.notes = notes;
            polygon.geoFenceID = new Guid(geofenceID);

            string[] coords = geoFence.Split(',');
            foreach (string s in coords)
            {
                string[] latlong = s.Split('^');
                Base_AVL.PolygonService.LatLon LL = new Base_AVL.PolygonService.LatLon();
                LL.Lat = Convert.ToDouble(latlong[0]);
                LL.Lon = Convert.ToDouble(latlong[1]);
                LatLongs.Add(LL);
            }
            polygon.radius = (type == "circle") ? Convert.ToDouble(radius) : 0;
            polygon.geoFence = LatLongs;
            polygon.actionOut = actionOut;
            polygon.actionIn = actionIn;
            polygon.actionInEmail = actionInEmail;
            polygon.actionOutEmail = actionOutEmail;

            var success = polygonService.addPolygon(polygon);

            return Json(success, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        [Authorize]
        public ActionResult DeleteFence(string id)
        {
            polygonService.deletePolygon(new Guid(id));

            return Json("OK", JsonRequestBehavior.AllowGet);
        }

        #endregion  
    }
}
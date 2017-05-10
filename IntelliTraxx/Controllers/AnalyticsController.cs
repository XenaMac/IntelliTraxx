using IntelliTraxx.TruckService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntelliTraxx.Controllers
{
    public class AnalyticsController : Controller
    {
        TruckServiceClient truckService = new TruckServiceClient();
        
        // GET: Analytics
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            List<Vehicle> allVehicles = truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getDriverAnalytics()
        {
            driverAnalytics da = new driverAnalytics();
            List<driverVehicleReturn> driversAssigned = truckService.driverVehicleReturn();
            List<Vehicle> Vehicles = truckService.getAllVehicles(true);
            foreach(Vehicle v in Vehicles)
            {
                if(v.driver != null && v.driver.DriverID.ToString() != "00000000-0000-0000-0000-000000000000")
                {
                    da.driving++;
                } 
            }
            da.notDriving = driversAssigned.Count() - da.driving;
            da.notAssigned = truckService.getAvailableDrivers().Count();
            da.vehiclesWithoutDrivers = truckService.getAvailableVehicles().Count();
            return Json(da, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAlertsByRange(DateTime start, DateTime end)
        {
            List<alertReturn> alerts = truckService.getAllAlertsByRange(start, end);
            return Json(alerts, JsonRequestBehavior.AllowGet);
        }
    }

    //------------------------------ Classes ----------------------------------------//

    public class driverAnalytics
    {
        public int driving { get; set; }
        public int notDriving { get; set; }
        public int notAssigned { get; set; }
        public int vehiclesWithoutDrivers { get; set; }
    }
}
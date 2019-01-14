using Base_AVL.LATAService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Base_AVL.Controllers.Alerts
{
    public class AlertsController : Controller
    {
        LATAService.TruckServiceClient truckService = new LATAService.TruckServiceClient();

        // GET: Alerts        
        public ActionResult Index(string from, string to)
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

            return View(Todays);
        }

        //GET: ReadAlert
        public ActionResult ViewAlert(string alertID)
        {
            Guid id = new Guid(alertID);
            alertReturn alert = null;
            alert = truckService.getAllAlertByID(new Guid(alertID));

            return View(alert);
        }

        [Authorize]
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

        public class AlertHistory
        {
            public alertReturn Alert { get; set; }
            public List<VehicleGPSRecord> Locations { get; set; }
        }
    }
}
using IntelliTraxx.AlertAdminService;
using IntelliTraxx.TruckService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace IntelliTraxx.Controllers
{
    public class SchedulingController : Controller
    {

        TruckServiceClient truckService = new TruckServiceClient();
        AlertAdminSvcClient alertService = new AlertAdminSvcClient();

        // GET: Scheduling
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult getAllSchedules()
        {
            List<schedule> schedules = alertService.getAllSchedules();
            return Json(schedules, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            var allVehicles = truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehiclesBySchedule(string scheduleID)
        {
            var scheduleVehicles = alertService.getAllVehiclesBySchedule(new Guid(scheduleID));
            return Json(scheduleVehicles, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult updateSchedules(List<schedule> schedules)
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var userID = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            Company company = truckService.getUserCompaniesFull(new Guid(userID)).FirstOrDefault();

            foreach (schedule s in schedules)
            {
                s.companyid = company.CompanyID;
                s.createdOn = DateTime.Now;
                s.createdBy = User.Identity.Name;
                s.modifiedOn = DateTime.Now;
                s.modifiedBy = User.Identity.Name;
            }

            string success = alertService.updateSchedules(schedules);
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult addVehicleToSchedule(string scheduleID, List<string> vehicleIDs)
        {
            string success = alertService.addVehicleToSchedule(new Guid(scheduleID), vehicleIDs);
            return Json(success, JsonRequestBehavior.AllowGet);
        }
    }
}
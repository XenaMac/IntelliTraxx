using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web.Mvc;
using IntelliTraxx.Shared.AlertAdminService;
using IntelliTraxx.Shared.TruckService;

namespace IntelliTraxx.Controllers
{
    public class SchedulingController : Controller
    {
        private readonly AlertAdminSvcClient _alertService = new AlertAdminSvcClient();
        private readonly TruckServiceClient _truckService = new TruckServiceClient();

        // GET: Scheduling
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult getAllSchedules()
        {
            var simpleschedules = new List<simpleschedule>();
            var schedules = _alertService.getAllSchedules();
            foreach (var s in schedules)
            {
                var ss = new simpleschedule();
                ss.active = s.active;
                ss.companyid = s.companyid;
                ss.createdBy = s.createdBy;
                ss.createdOn = s.createdOn.ToLongDateString();
                ss.DOW = s.DOW;
                ss.EffDtEnd = s.EffDtEnd.ToShortDateString();
                ss.EffDtStart = s.EffDtStart.ToShortDateString();
                ss.endTime = s.endTime.ToString("HH:mm");
                ss.modifiedBy = s.modifiedBy;
                ss.modifiedOn = s.modifiedOn.ToShortDateString();
                ss.scheduleID = s.scheduleID;
                ss.scheduleName = s.scheduleName;
                ss.startTime = s.startTime.ToString("HH:mm");
                simpleschedules.Add(ss);
            }

            return Json(simpleschedules.OrderBy(v => v.scheduleName), JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            var allVehicles = _truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehiclesBySchedule(string scheduleID)
        {
            var scheduleVehicles = _alertService.getAllVehiclesBySchedule(new Guid(scheduleID));
            return Json(scheduleVehicles, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult updateSchedules(List<schedule> schedules, bool knew)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var identity = (ClaimsPrincipal) Thread.CurrentPrincipal;
            var userID = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            var company = _truckService.getUserCompaniesFull(new Guid(userID)).FirstOrDefault();

            if (knew)
                foreach (var s in schedules)
                {
                    s.companyid = company.CompanyID;
                    s.createdOn = DateTime.Now;
                    s.createdBy = User.Identity.Name;
                    s.modifiedOn = DateTime.Now;
                    s.modifiedBy = User.Identity.Name;
                }
            else
                foreach (var s in schedules)
                {
                    s.companyid = company.CompanyID;
                    s.modifiedOn = DateTime.Now;
                    s.modifiedBy = User.Identity.Name;
                }

            var success = _alertService.updateSchedules(schedules.ToArray());
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult addVehicleToSchedule(string scheduleID, List<string> vehicleIDs)
        {
            var success = _alertService.addVehicleToSchedule(new Guid(scheduleID), vehicleIDs.ToArray());
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult deleteSchedules(List<schedule> dSchedules)
        {
            var success = _alertService.deleteSchedules(dSchedules.ToArray());
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        //---------------------------------------------------------------------------------------//

        public class simpleschedule
        {
            public Guid scheduleID { get; set; }

            public string scheduleName { get; set; }

            public Guid companyid { get; set; }

            public string startTime { get; set; }

            public string endTime { get; set; }

            public string createdBy { get; set; }

            public string createdOn { get; set; }

            public string modifiedBy { get; set; }

            public string modifiedOn { get; set; }

            public int DOW { get; set; }

            public string EffDtStart { get; set; }

            public string EffDtEnd { get; set; }

            public bool active { get; set; }
        }
    }
}
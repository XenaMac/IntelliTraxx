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
            List<simpleschedule> simpleschedules = new List<simpleschedule>();
            List<schedule> schedules = alertService.getAllSchedules();
            foreach(schedule s in schedules)
            {
                simpleschedule ss = new simpleschedule();
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
            var allVehicles = truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllVehiclesBySchedule(string scheduleID)
        {
            var scheduleVehicles = alertService.getAllVehiclesBySchedule(new Guid(scheduleID));
            return Json(scheduleVehicles, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult updateSchedules(List<schedule> schedules, bool knew)
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var userID = identity.Claims.Where(c => c.Type == ClaimTypes.Sid).Select(c => c.Value).SingleOrDefault();
            Company company = truckService.getUserCompaniesFull(new Guid(userID)).FirstOrDefault();

            if (knew)
            {
                foreach (schedule s in schedules)
                {
                    s.companyid = company.CompanyID;
                    s.createdOn = DateTime.Now;
                    s.createdBy = User.Identity.Name;
                    s.modifiedOn = DateTime.Now;
                    s.modifiedBy = User.Identity.Name;
                }
            } else {
                foreach (schedule s in schedules)
                {
                    s.companyid = company.CompanyID;
                    s.modifiedOn = DateTime.Now;
                    s.modifiedBy = User.Identity.Name;
                }
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

        [HttpPost]
        public  ActionResult deleteSchedules(List<schedule> dSchedules)
        {
            string success = alertService.deleteSchedules(dSchedules);
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
using IntelliTraxx.AlertAdminService;
using IntelliTraxx.TruckService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntelliTraxx.Controllers
{
    public class SchedulingController : Controller
    {

        TruckServiceClient truckService = new TruckServiceClient();
        AlertAdminSvcClient alertService = new AlertAdminSvcClient();

        // GET: Scheduling
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult getAllSchedules()
        {
            List<schedule> schedules = alertService.getAllSchedules();
            return Json(schedules, JsonRequestBehavior.AllowGet);
        }
    }
}
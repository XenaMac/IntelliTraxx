using System;
using System.Web.Mvc;

namespace IntelliTraxx.Controllers
{
    public class DispatchController : Controller
    {
        [AllowAnonymous]
        public ActionResult GetServerTime()
        {
            var data = DateTime.Now;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetServerTimeSafe()
        {
            var data = DateTime.Now;
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
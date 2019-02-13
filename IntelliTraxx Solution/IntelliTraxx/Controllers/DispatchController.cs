using System;
using System.Web.Mvc;
using IntelliTraxx.Common;

namespace IntelliTraxx.Controllers
{
   
    public class DispatchController : MobileBaseController
    {
        [AllowAnonymous]
        public ActionResult GetServerTime()
        {
            var data = DateTime.Now;
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetServerTimeSafe()
        {
            var request = HttpContext.Request;
            var user = HttpContext.User.Identity.Name;
            var name = this.GetUserName();
            var userId = this.GetUserId();

            var data = DateTime.Now;
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
using System;
using System.Web.Mvc;
using IntelliTraxx.Common;
using IntelliTraxx.Common.Jwt;

namespace IntelliTraxx.Controllers
{   
    [JwtAuthentication]
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
            var name = this.GetUserName();
            var userId = this.GetUserId();
            var data = $"{DateTime.Now}-{name}-{userId}" ;
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
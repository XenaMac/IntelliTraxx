using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Base_AVL.Controllers.Analytics
{
    public class AnalyticsController : MessageControllerBase
    {
        // GET: Analytics
        [CheckSessionOut]
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}
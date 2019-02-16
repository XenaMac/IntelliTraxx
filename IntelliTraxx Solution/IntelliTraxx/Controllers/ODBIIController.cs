using System.Web.Mvc;
using IntelliTraxx.AJAXVehiclesService;

namespace IntelliTraxx.Controllers
{
    public class OBDIIController : Controller
    {
        AJAXVehiclesClient AVS = new AJAXVehiclesClient();

        // GET: ODBII
        [HttpGet]
        public ActionResult OBDInit(string MAC)
        {
            var OBDInit = AVS.ODBInit(MAC);
            return Json(OBDInit, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult setOBDData(OBDUpdate ODBData)
        {
            var setOBDData = AVS.setODBData(ODBData);
            return Json(setOBDData, JsonRequestBehavior.AllowGet);
        }
    }
}
using Base_AVL.LATAService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Base_AVL.Controllers.Dispatch
{
    public class DispatchController : Controller
    {
        TruckServiceClient truckService = new TruckServiceClient();

        // GET: Dispatch
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

            List<dispatch> dispatches = truckService.getDispatchesByRange(today, Tomorrow);

            return View(dispatches);
        }

        [HttpPost]
        public ActionResult GetParentCompanyLocation()
        {
            Company parentCompany = new Company();
            List<Company> companies = truckService.getCompanies(new Guid());

            foreach (Company c in companies)
            {
                if (c.isParent == true)
                {
                    parentCompany = c;
                }
            }

            return Json(parentCompany.CompanyCity + ", " + parentCompany.CompanyState, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DispatchVehicle(string address, string city, string state, string vehicleID, string zip, string note)
        {
            string dispatchVehicle = null;
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            var roles = identity.Claims.Where(c => c.Type == "Roles").Select(c => c.Value).SingleOrDefault();

            dispatch disp = new dispatch();
            disp.address = address;
            disp.city = city;
            disp.state = state;
            disp.vehicleID = vehicleID;
            disp.zip = zip;
            disp.UserEmail = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.Email).Value.ToString();
            disp.note = note;

            dispatchVehicle = truckService.dispatchVehicle(disp);

            return Json(dispatchVehicle, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult GetDispatchByID(string ID)
        {
            Guid dispID = new Guid(ID);
            dispatch disp = new dispatch();

            disp = truckService.getDispatchesByID(dispID);

            return Json(disp, JsonRequestBehavior.AllowGet);
        }
    }
}
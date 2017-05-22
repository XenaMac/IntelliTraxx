using IntelliTraxx.TruckService;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntelliTraxx.Controllers
{
    public class AnalyticsController : Controller
    {
        TruckServiceClient truckService = new TruckServiceClient();

        // GET: Analytics
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult getAllVehicles(bool loadHistorical)
        {
            List<Vehicle> allVehicles = truckService.getAllVehicles(loadHistorical);

            return Json(allVehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getDriverAnalytics()
        {
            driverAnalytics da = new driverAnalytics();
            List<driverVehicleReturn> driversAssigned = truckService.driverVehicleReturn();
            List<Vehicle> Vehicles = truckService.getAllVehicles(true);
            foreach (Vehicle v in Vehicles)
            {
                if (v.driver != null && v.driver.DriverID.ToString() != "00000000-0000-0000-0000-000000000000")
                {
                    da.driving++;
                }
            }
            da.notDriving = driversAssigned.Count() - da.driving;
            da.notAssigned = truckService.getAvailableDrivers().Count();
            da.vehiclesWithoutDrivers = truckService.getAvailableVehicles().Count();
            return Json(da, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAlertsByRange(DateTime start, DateTime end)
        {
            List<alertReturn> alerts = truckService.getAllAlertsByRange(start, end);
            return Json(alerts, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getVehicleListMac()
        {
            List<macVehicle> vehicles = truckService.getVehicleListMac();
            return Json(vehicles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getECMRouter(string macAddress)
        {
            //get vars for api keys
            List<systemvar> vars = truckService.getAppVars();

            var client = new RestClient();
            client.BaseUrl = new Uri("https://www.cradlepointecm.com/");

            var request = new RestRequest();
            request.Resource = "api/v2/routers/";

            // easily add HTTP Headers
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-CP-API-ID", vars.Where(v => v.varName == "X-CP-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-CP-API-KEY", vars.Where(v => v.varName == "X-CP-API-KEY").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-ID", vars.Where(v => v.varName == "X-ECM-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-KEY", vars.Where(v => v.varName == "X-ECM-API-KEY").Select(x => x.varVal).FirstOrDefault());

            IRestResponse response = client.Execute(request);
            RoutersRootobject routers = JsonConvert.DeserializeObject<RoutersRootobject>(response.Content);
            var router = routers.data.Where(d => d.mac == macAddress.ToUpper()).FirstOrDefault();
            return Json(router, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getECMAccount(string accountNum)
        {
            //get vars for api keys
            List<systemvar> vars = truckService.getAppVars();

            var client = new RestClient();
            client.BaseUrl = new Uri("https://www.cradlepointecm.com/");

            var request = new RestRequest();
            request.Resource = "api/v2/accounts/" + accountNum + "/";

            // easily add HTTP Headers
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-CP-API-ID", vars.Where(v => v.varName == "X-CP-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-CP-API-KEY", vars.Where(v => v.varName == "X-CP-API-KEY").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-ID", vars.Where(v => v.varName == "X-ECM-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-KEY", vars.Where(v => v.varName == "X-ECM-API-KEY").Select(x => x.varVal).FirstOrDefault());

            IRestResponse response = client.Execute(request);
            AccountRootobject account = JsonConvert.DeserializeObject<AccountRootobject>(response.Content);
            return Json(account, JsonRequestBehavior.AllowGet);
        }
    }

    //------------------------------ Classes ----------------------------------------//

    public class driverAnalytics
    {
        public int driving { get; set; }
        public int notDriving { get; set; }
        public int notAssigned { get; set; }
        public int vehiclesWithoutDrivers { get; set; }
    }

    public class RoutersRootobject
    {
        public RoutersDatum[] data { get; set; }
        public RoutersMeta meta { get; set; }
    }

    public class RoutersMeta
    {
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
    }

    public class RoutersDatum
    {
        public string account { get; set; }
        public string actual_firmware { get; set; }
        public string asset_id { get; set; }
        public string config_status { get; set; }
        public DateTime created_at { get; set; }
        public object custom1 { get; set; }
        public object custom2 { get; set; }
        public string description { get; set; }
        public string full_product_name { get; set; }
        public string group { get; set; }
        public string id { get; set; }
        public string ipv4_address { get; set; }
        public string locality { get; set; }
        public string mac { get; set; }
        public string name { get; set; }
        public string product { get; set; }
        public string resource_url { get; set; }
        public string serial_number { get; set; }
        public string state { get; set; }
        public DateTime state_updated_at { get; set; }
        public string target_firmware { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class AccountRootobject
    {
        public string account { get; set; }
        public string id { get; set; }
        public bool is_disabled { get; set; }
        public string name { get; set; }
        public string resource_url { get; set; }
    }

}
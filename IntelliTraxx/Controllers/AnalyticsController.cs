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

        public ActionResult getECMSignalStrength(string routerID)
        {
            //initialize RouterSignal
            List<RouterSignal> RSList = new List<RouterSignal>();

            //get vars for api keys
            List<systemvar> vars = truckService.getAppVars();

            var client = new RestClient();
            client.BaseUrl = new Uri("https://www.cradlepointecm.com/");

            var request = new RestRequest();
            request.Resource = "api/v2/net_devices";
            request.AddParameter("router", routerID);

            // easily add HTTP Headers
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-CP-API-ID", vars.Where(v => v.varName == "X-CP-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-CP-API-KEY", vars.Where(v => v.varName == "X-CP-API-KEY").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-ID", vars.Where(v => v.varName == "X-ECM-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-KEY", vars.Where(v => v.varName == "X-ECM-API-KEY").Select(x => x.varVal).FirstOrDefault());

            IRestResponse response = client.Execute(request);
            NetDevicesRootobject devices = JsonConvert.DeserializeObject<NetDevicesRootobject>(response.Content);

            foreach (NetDevicesDatum device in devices.data)
            {
                if (device.router.Contains(routerID) && device.mode == "wan")
                {
                    RouterSignal rs = getRouterStrength(device);
                    RSList.Add(rs);
                }
            }

            return Json(RSList, JsonRequestBehavior.AllowGet);
        }

        public RouterSignal getRouterStrength(NetDevicesDatum device)
        {
            RouterSignal NewRS = new RouterSignal();
            List<systemvar> vars = truckService.getAppVars();

            var client = new RestClient();
            client.BaseUrl = new Uri("https://www.cradlepointecm.com/");

            var request = new RestRequest();
            request.Resource = "api/v2/net_device_signal_samples/";
            request.AddParameter("net_device", device.id);


            // easily add HTTP Headers
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-CP-API-ID", vars.Where(v => v.varName == "X-CP-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-CP-API-KEY", vars.Where(v => v.varName == "X-CP-API-KEY").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-ID", vars.Where(v => v.varName == "X-ECM-API-ID").Select(x => x.varVal).FirstOrDefault());
            request.AddHeader("X-ECM-API-KEY", vars.Where(v => v.varName == "X-ECM-API-KEY").Select(x => x.varVal).FirstOrDefault());

            IRestResponse response = client.Execute(request);
            DSSRootobject samples = JsonConvert.DeserializeObject<DSSRootobject>(response.Content);
            DSSDatum sample = samples.data.Where(s => s.net_device.Contains(device.id)).OrderByDescending(d => d.created_at).FirstOrDefault();
            if(sample != null)
            {
                NewRS.connection_state = device.connection_state;
                NewRS.name = device.name;
                NewRS.signal_percent = sample.signal_percent;
                NewRS.dbm = sample.dbm;
                NewRS.sinr = sample.sinr;
                NewRS.created_at = sample.created_at;
            } else
            {
                NewRS.connection_state = device.connection_state;
                NewRS.name = device.name;
                NewRS.signal_percent = 0;
                NewRS.dbm = 0;
                NewRS.sinr = 0;
                NewRS.created_at = Convert.ToDateTime("01/01/1959 00:00:00");
            }

            return NewRS;
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

    public class NetDevicesRootobject
    {
        public NetDevicesDatum[] data { get; set; }
        public NetDevicesMeta meta { get; set; }
    }

    public class NetDevicesMeta
    {
        public int limit { get; set; }
        public object next { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
    }

    public class NetDevicesDatum
    {
        public string account { get; set; }
        public object bsid { get; set; }
        public string carrier { get; set; }
        public string carrier_id { get; set; }
        public int? channel { get; set; }
        public string connection_state { get; set; }
        public string dns0 { get; set; }
        public string dns1 { get; set; }
        public object esn { get; set; }
        public string gateway { get; set; }
        public string gsn { get; set; }
        public string homecarrid { get; set; }
        public string hostname { get; set; }
        public string iccid { get; set; }
        public string id { get; set; }
        public string imei { get; set; }
        public string imsi { get; set; }
        public string ipv4_address { get; set; }
        public bool is_asset { get; set; }
        public bool is_gps_supported { get; set; }
        public bool is_upgrade_available { get; set; }
        public bool is_upgrade_supported { get; set; }
        public string ltebandwidth { get; set; }
        public object mac { get; set; }
        public string manufacturer { get; set; }
        public string mdn { get; set; }
        public string meid { get; set; }
        public string mfg_model { get; set; }
        public string mfg_product { get; set; }
        public string mn_ha_spi { get; set; }
        public string mn_ha_ss { get; set; }
        public string mode { get; set; }
        public string model { get; set; }
        public string modem_fw { get; set; }
        public int? mtu { get; set; }
        public string nai { get; set; }
        public string name { get; set; }
        public string netmask { get; set; }
        public string pin_status { get; set; }
        public string port { get; set; }
        public string prlv { get; set; }
        public string profile { get; set; }
        public string resource_url { get; set; }
        public string rfband { get; set; }
        public object rfchannel { get; set; }
        public string roam { get; set; }
        public string router { get; set; }
        public string rxchannel { get; set; }
        public string serial { get; set; }
        public string service_type { get; set; }
        public string ssid { get; set; }
        public string summary { get; set; }
        public string txchannel { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public DateTime updated_at { get; set; }
        public float? uptime { get; set; }
        public string ver_pkg { get; set; }
        public string version { get; set; }
        public string wimax_realm { get; set; }
    }

    public class RouterSignal
    {
        public DateTime created_at { get; set; }
        public string connection_state { get; set; }
        public string name { get; set; }
        public int signal_percent { get; set; }
        public int dbm { get; set; }
        public float sinr { get; set; }
    }
    
    public class DSSRootobject
    {
        public DSSDatum[] data { get; set; }
        public DSSMeta meta { get; set; }
    }

    public class DSSMeta
    {
        public int limit { get; set; }
        public string next { get; set; }
    }

    public class DSSDatum
    {
        public object cinr { get; set; }
        public DateTime created_at { get; set; }
        public string created_at_timeuuid { get; set; }
        public int dbm { get; set; }
        public object ecio { get; set; }
        public string net_device { get; set; }
        public float rsrp { get; set; }
        public float rsrq { get; set; }
        public object rssi { get; set; }
        public int signal_percent { get; set; }
        public float sinr { get; set; }
        public float uptime { get; set; }
    }

}
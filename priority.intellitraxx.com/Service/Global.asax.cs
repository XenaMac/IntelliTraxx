using System;
using System.Threading;
using System.Timers;
using System.Web;
using UDPListener;

namespace LATATrax
{
    public class Global : System.Web.HttpApplication
    {
        public static UDPListener.UDPListen udp;
        public GlobalData.Listener listen;
        GlobalData.SQLCode sql = new GlobalData.SQLCode();

        protected void Application_Start(object sender, EventArgs e)
        {
            //start the listener process
            udp = new UDPListen();
            listen = new GlobalData.Listener();
            //grab the stored polygon data from the database
            GeoCode.GlobalGeo.loadStoredPolygons();
            //Load user/role/company information from the database
            sql.loadUsers();
            sql.loadCompanies();
            sql.loadRoles();
            sql.loadUserRoles();
            sql.loadUserCompanies();
            sql.getVars();
            sql.getAppVars();
            sql.loadDrivers();
            sql.loadVehicleExtendedData();
            sql.loadVehicleClasses();
            sql.loadAlertsForGeoFences();
            //sql.loadAlertModels();
            sql.loadAlertVehicles();

            //KillTimer
            System.Timers.Timer SilentTimer = new System.Timers.Timer();
            SilentTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            SilentTimer.Interval = 600000;
            SilentTimer.Enabled = true;

        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            GlobalData.GlobalData.SilentCheck();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //Allow CORS connections
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods",
                              "GET, POST");
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers",
                              "Content-Type, Accept");
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age",
                              "1728000");
                HttpContext.Current.Response.End();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            udp.unBind();
        }
    }
}
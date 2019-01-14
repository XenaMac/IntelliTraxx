using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CANCodeInterface" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select CANCodeInterface.svc or CANCodeInterface.svc.cs at the Solution Explorer and start debugging.
    public class CANCodeInterface : ICANCodeInterface
    {
        public void addDriverBehavior(string MACAddress, string behavior) {
            //find the truck
            Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) {
                return find.extendedData.MACAddress == MACAddress;
            });
            if (v != null) {
                driverBehavior d = new driverBehavior();
                d.driverID = v.driver.DriverID;
                d.behavior = behavior;
                d.timeStamp = DateTime.Now.ToUniversalTime();
                v.behaviors.Add(d);
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.logDriverBehavior(d, v.extendedData.ID);
                Models.alert a = new Models.alert();
                a.alertActive = false;
                a.alertStart = DateTime.Now.ToUniversalTime();
                a.alertEnd = DateTime.Now.ToUniversalTime();
                a.alertName = "BEHAVIOR:" + behavior;
                a.alertType = "BEHAVIOR";
                a.latLonStart = v.gps.lat.ToString() + "|" + v.gps.lon.ToString();
                a.latLonEnd = v.gps.lat.ToString() + "|" + v.gps.lon.ToString();
                a.maxVal = "NA";
                a.runID = v.runID;
                a.alertID = Guid.NewGuid();
                v.alerts.Add(a);
                sql.logAlert(a, v.VehicleID);
            }
        }

        public void addODBData(string MACAddress, string name, string val) {
            try
            {
                //find the truck
                Models.Vehicle v = GlobalData.GlobalData.vehicles.Find(delegate (Models.Vehicle find) {
                    return find.extendedData.MACAddress == MACAddress;
                });
                if (v != null)
                {
                    OBD2Data o = new OBD2Data();
                    o.name = name;
                    o.val = val;
                    v.addOBDData(o);
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}

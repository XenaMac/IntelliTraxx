using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UDPListener;
using Newtonsoft.Json;

namespace LATATrax.GlobalData
{
    public class Listener
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public Listener()
        {
            Global.udp.handleNewMessage += Udp_handleNewMessage;
        }

        private void Udp_handleNewMessage(UDPListener.Helpers.MessageData md)
        {
            /* old - replaced 01/19/2017 - EL
            Models.Vehicle found = GlobalData.vehicles.Find(delegate (Models.Vehicle find) 
                {
                    return find.VehicleID == md.VehicleID;
                }
            );
            if (found == null)
            {
                Models.Vehicle veh = new Models.Vehicle();
                veh.VehicleID = md.VehicleID;
                SQLCode sql = new SQLCode();
                VehicleExtendedData ed = sql.getExtendedData(md.VehicleID);
                veh.extendedData = ed;
                GlobalData.vehicles.Add(veh);
                veh.readMessage(md);
            }
            else
            {
                found.readMessage(md);
            }
            */
            if (md.Message.Substring(2, 3) == "CAN")
            {
                //this is just a test section to attempt to read a CAN message without having a truck involved
                try
                {
                    JsonSerializer ser = new JsonSerializer();
                    Messages.OBD2 o = JsonConvert.DeserializeObject<Messages.OBD2>(md.Message);
                    o.timestamp = GlobalData.getDTFromUnixUTC(o.T);
                    o.timestampUTC = GlobalData.getDTFromUnix(o.T);
                    GlobalData.obdList.Add(o);
                }
                catch (Exception ex)
                {
                    //string err = ex.ToString();
                    string err = DateTime.Now.ToString() + Environment.NewLine + "Error processing OBD Message" + Environment.NewLine + md.Message;
                    err += Environment.NewLine + Environment.NewLine + ex.ToString() + Environment.NewLine + Environment.NewLine;
                    Logger logger = new Logger("Listener.txt");
                    logger.writeToLogFile("ERROR in CAN: " + ex.ToString());
                }
                string[] splitter = md.messageID.ToString().Split('|');
                Models.Vehicle found = GlobalData.vehicles.Find(delegate (Models.Vehicle find)
                {
                    return find.extendedData.MACAddress == md.VehicleID;
                });
                if (found == null)
                {
                    //not much we can do here because we don't know the vehicle id
                }
                else {
                    found.readMessage(md);
                }
            }
            else if (md.Message.Contains("<TR+"))
            {
                SQLCode sql = new SQLCode();
                string message = md.Message.Remove(0, 4);
                message = message.Remove(message.Length - 5, 5);
                string[] msg = message.Split(',');

                try
                {
                    VehicleSignal signal = new VehicleSignal();
                    signal.ID = Guid.NewGuid();
                    signal.Name = msg[0];
                    signal.MAC = msg[1];
                    signal.timestamp = epoch.AddSeconds(Convert.ToUInt32(msg[2]));
                    signal.Lat = float.Parse(msg[3]);
                    signal.Lon = float.Parse(msg[4]);
                    signal.dBm = float.Parse(msg[5]);
                    signal.SINR = float.Parse(msg[6]);
                    
                    sql.logSignal(signal);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
            else {
                Models.Vehicle found = GlobalData.vehicles.Find(delegate (Models.Vehicle find)
                {
                    return find.VehicleID == md.VehicleID;
                }
                );
                if (found == null) //this is initialization stuff. It should probably be moved to the vehicle class.
                {
                    try
                    {
                        Models.Vehicle veh = new Models.Vehicle();
                        veh.VehicleID = md.VehicleID;
                        SQLCode sql = new SQLCode();
                        VehicleExtendedData ed = sql.getExtendedData(md.VehicleID);
                        if (ed.ID == Guid.Empty)
                        {
                            //this is not a vehicle in the system. Log and alert
                            Logger logger = new Logger("Listener.txt");
                            logger.writeToLogFile("Vehicle not Found Error: A brodcast was captured with a TAIP ID not within the IntelliTRaxx System. Please add vehicle or fix TAIP within Router. ID was: " + md.VehicleID);
                        }
                        else
                        {
                            veh.extendedData = ed;
                            List<Models.alertModel> alertList = sql.getAlertsForVehicle(veh.extendedData.ID);
                            veh.availAlerts = alertList;
                            GlobalData.vehicles.Add(veh);
                            veh.readMessage(md);
                            Models.dailySchedule ds = sql.getDailyScheduleForVehicle(veh.extendedData.vehicleID);
                            veh.sched = ds;

                            //check for last position if that functionality is enabled
                            //string lp = System.Configuration.ConfigurationManager.AppSettings["checkLastPosition"].ToString();
                            //systemvar sv = GlobalData.vars.Find(delegate (systemvar find) { return find.varName.ToUpper() == "CHECKLASTPOSITION"; });
                            Models.alertModel afound = veh.availAlerts.Find(delegate (Models.alertModel find) { return find.AlertClassName == "LASTPOSITION"; });
                            if (afound != null && afound.AlertActive == true && afound.AlertStartTime <= DateTime.Now && afound.AlertEndTime >= DateTime.Now)
                            {
                                string lastPos = sql.checkLastPoly(veh);
                                if (!string.IsNullOrEmpty(lastPos) && lastPos != "NA" && lastPos != "N/A")
                                {
                                    Models.alert a = new Models.alert();
                                    a.alertActive = true;
                                    //a.alertEnd = DateTime.Now;
                                    a.alertEnd = DateTime.Now.ToUniversalTime();
                                    a.alertID = Guid.NewGuid();
                                    a.alertName = afound.AlertFriendlyName;
                                    //a.alertStart = DateTime.Now;
                                    a.alertStart = DateTime.Now.ToUniversalTime();
                                    a.alertType = afound.AlertFriendlyName;
                                    a.latLonStart = veh.gps.lat.ToString() + "|" + veh.gps.lon.ToString();
                                    a.latLonEnd = veh.gps.lat.ToString() + "|" + veh.gps.lon.ToString();
                                    a.maxVal = "NA";
                                    a.runID = veh.runID;
                                    if (!string.IsNullOrEmpty(veh.insidePolyName.polyName) && veh.insidePolyName.polyName != lastPos)
                                    {
                                        //this alert is contigent upon a vehicle previously being in a polygon and not being in one
                                        //now
                                        veh.alerts.Add(a);
                                        sql.logAlert(a, veh.VehicleID);
                                        if (!string.IsNullOrEmpty(afound.AlertAction))
                                        {
                                            if (afound.AlertAction.Contains("EMAIL:"))
                                            {
                                                string emls = afound.AlertAction.Replace("EMAIL:", "");
                                                if (!string.IsNullOrEmpty(emls))
                                                {
                                                    sendMail sm = new sendMail();
                                                    sm.sendAlertEmail(emls, a, veh, string.Empty);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            afound = veh.availAlerts.Find(delegate (Models.alertModel find) { return find.AlertClassName == "STARTUP"; });

                            if (afound != null && afound.AlertActive == true && afound.AlertStartTime <= DateTime.Now && afound.AlertEndTime >= DateTime.Now)
                            {
                                Models.alert a = new Models.alert();
                                a.alertActive = false;
                                a.alertEnd = DateTime.Now.ToUniversalTime();
                                a.alertStart = DateTime.Now.ToUniversalTime();
                                a.alertName = afound.AlertFriendlyName;
                                a.alertID = Guid.NewGuid();
                                a.alertType = afound.AlertFriendlyName;
                                a.latLonStart = veh.gps.lat.ToString() + "|" + veh.gps.lon.ToString();
                                a.latLonEnd = veh.gps.lat.ToString() + "|" + veh.gps.lon.ToString();
                                a.maxVal = "NA";
                                a.runID = veh.runID;
                                veh.alerts.Add(a);
                                sql.logAlert(a, veh.VehicleID);
                                if (!string.IsNullOrEmpty(afound.AlertAction))
                                {
                                    if (afound.AlertAction.Contains("EMAIL:"))
                                    {
                                        string emls = afound.AlertAction.Replace("EMAIL:", "");
                                        if (!string.IsNullOrEmpty(emls))
                                        {
                                            sendMail sm = new sendMail();
                                            sm.sendAlertEmail(emls, a, veh, string.Empty);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string err = ex.ToString();
                    }
                }
                else {
                    found.readMessage(md);
                }
            }
        }

    }
}
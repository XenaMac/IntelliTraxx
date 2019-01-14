using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;
using LATATrax.Models;

namespace LATATrax.GlobalData
{
    public class sendMail
    {
        public void sendAlertEmail(string emails, Models.alert a, Vehicle Vehicle, string maxVal)
        {
            try
            {
                String userName = "alerts@intellitraxx.com";
                String password = "aw3$ESse4#WA";
                string[] contactEmails = emails.Split(';');

                string subject = "IntelliTraxx: Alert Notification";
                string body = "The following alert has been triggered within the IntelliTraxx system. <br /><br />----------------<br />";
                body += "Alert Name: " + a.alertName + "<br />";
                body += "Alert Type: " + a.alertType + "<br />";
                body += "Alert Start: " + a.alertStart + "(UTC) <br />";
                body += "Alert End: " + a.alertEnd + "(UTC) <br />";
                body += "Lat/Lon Start: " + a.latLonStart + "<br />";
                body += "Lat/Lon End: " + a.latLonEnd + "<br />";

                if (!string.IsNullOrEmpty(maxVal))
                {
                    body += "Max Val: " + maxVal + "<br/>";
                }
                body += "Vehicle ID: " + Vehicle.VehicleID + "<br />";
                body += "Vehicle Friendly Name: " + Vehicle.extendedData.VehicleFriendlyName + "<br />";
                body += "Vehicle Class: " + Vehicle.extendedData.vehicleClass + "<br />";
                body += "Vehicle Make: " + Vehicle.extendedData.Make + "<br />";
                body += "Vehicle Model: " + Vehicle.extendedData.Model + "<br />";
                body += "Vehicle Year: " + Vehicle.extendedData.Year + "<br />";
                body += "Vehicle MAC Address: " + Vehicle.extendedData.MACAddress + "<br />";
                body += "Vehicle License Plate: " + Vehicle.extendedData.licensePlate + "<br />";
                body += "Vehicle Company Name: " + Vehicle.extendedData.companyName + "<br />";

                if(Vehicle.sched != null)
                {
                    if (Vehicle.sched.scheduleID.ToString() != "00000000-0000-0000-0000-000000000000")
                    {
                        body += "Schedule Start: " + Vehicle.sched.dtStart.ToLongDateString() + "<br />";
                        body += "Schedule End: " + Vehicle.sched.dtEnd.ToLongDateString() + "<br />";
                    }
                }

                if(Vehicle.driver != null)
                {
                    if (!string.IsNullOrEmpty(Vehicle.driver.DriverID.ToString()))
                    {
                        body += "Driver Number: " + Vehicle.driver.DriverNumber + "<br />";
                        body += "Driver Name: " + Vehicle.driver.DriverFirstName + " " + Vehicle.driver.DriverLastName + "<br />";
                    }
                }
                
                body += "----------------";

                MailMessage msg = new MailMessage();
                foreach (string email in contactEmails)
                {
                    msg.To.Add(email);
                    //msg.To.Add(new MailAddress("jmckinley@xenatech.com"));
                }

                msg.From = new MailAddress(userName);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.office365.com";
                client.Credentials = new System.Net.NetworkCredential(userName, password);
                client.Port = 587;
                client.EnableSsl = true;
                client.Send(msg);
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("IntelliTraxx Send Alert Email Error:" + ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace, EventLogEntryType.Error);
                }
            }

        }

        public void sendErrorEmail(string error)
        {
            try
            {
                String userName = "alerts@intellitraxx.com";
                String password = "aw3$ESse4#WA";

                string subject = "IntelliTraxx: Error Notification";
                string body = "Hey Doofus, The following error has been triggered within the CablePS IntelliTraxx system. Go Fix It!!<br /><br />----------------<br />";
                body += "Error: " + error + "<br />";
                body += "----------------";

                MailMessage msg = new MailMessage();
                msg.To.Add("jmckinley@xenatech.com");
                msg.From = new MailAddress(userName);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.Host = "smtp.office365.com";
                client.Credentials = new System.Net.NetworkCredential(userName, password);
                client.Port = 587;
                client.EnableSsl = true;
                client.Send(msg);
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "Application";
                    eventLog.WriteEntry("IntelliTraxx Send Alert Email Error:" + ex.Message + Environment.NewLine + ex.InnerException + Environment.NewLine + ex.StackTrace, EventLogEntryType.Error);
                }
            }

        }
    }
}
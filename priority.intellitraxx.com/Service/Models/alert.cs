using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.Models
{
    public class alert //this is a logged and returned alert
    {
        /*alertTypes (Tentative)
        0 - In Polygon
        1 - Out of polygon
        2 - Speeding
        3 - Mechanica
        */
        public Guid alertID { get; set; }
        public string alertType { get; set; }
        public string alertName { get; set; }
        public bool alertActive { get; set; }
        public DateTime alertStart { get; set; }
        public DateTime alertEnd { get; set; }
        public string latLonStart { get; set; }
        public string latLonEnd { get; set; }
        public string maxVal { get; set; }
        public Guid runID { get; set; }
    }

    public class alertModel { //this is the model of an alert that the database uses. It's used to hold types of alerts and what to do with them.
        public Guid AlertID { get; set; }
        public string AlertClassName { get; set; }
        public string AlertType { get; set; }
        public bool AlertActive { get; set; }
        public DateTime AlertStartTime { get; set; }
        public DateTime AlertEndTime { get; set; }
        public string AlertAction { get; set; }
        public string AlertFriendlyName { get; set; }
        public string minVal { get; set; }
        public bool NDB { get; set; }

    }

    public class dailySchedule {
        public Guid scheduleID { get; set; }
        public DateTime dtStart { get; set; }
        public DateTime dtEnd { get; set; }
    }
}
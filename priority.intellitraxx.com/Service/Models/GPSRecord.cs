using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.Models
{
    public class GPSRecord
    {
        public Guid ID { get; set; }
        public string VehicleID { get; set; }
        public float Direction { get; set; }
        public float Speed { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
        public bool InPolygon { get; set; }
        public string PolyName { get; set; }
        public DateTime timestamp { get; set; }
        public Guid runID { get; set; }
        public DateTime lastMessageReceived { get; set; }
    }
}
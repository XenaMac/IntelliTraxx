using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.Models
{
    public class LatLon
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double Alt { get; set; }
    }

    public class polygonData
    {
        public Guid geoFenceID { get; set; }
        //public string polyID { get; set; }
        public string notes { get; set; }
        public string polyName { get; set; }
        public List<LatLon> geoFence { get; set; }
        public string geoType { get; set; }
        public double radius { get; set; }
        public double minLat { get; set; }
        public double minLon { get; set; }
        public double maxLat { get; set; }
        public double maxLon { get; set; }
    }
}
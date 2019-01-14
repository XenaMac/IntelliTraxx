using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.Models
{
    /// <summary>
    /// This class is what gets sent back to the geofence designer. It's designed to work with blitz gmap editor
    /// </summary>
    public class polygonDesign
    {
        public string MapID { get; set; } //this should be a guid
        public int zoom { get; set; } //default to 19
        public int tilt { get; set; } //default to 0
        public string mapTypeId { get; set; } //will usually be hybrid
        public latLonPair center { get; set; }
        public List<layer> overlays { get; set; }
    }

    /// <summary>
    /// Each polygonDesign can have 1-n overlays. Each overlay is a geofence
    /// </summary>
    public class layer {
        public string type { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string fillColor { get; set; }
        public double fillOpacity { get; set; }
        public string strokeColor { get; set; }
        public double strokeOpacity { get; set; }
        public double strokeWeight { get; set; }
        public List<List<latLonPair>> paths { get; set; }
    }

    /// <summary>
    /// Each polygon design has a paths variable that's just a list of latLonPairs
    /// </summary>
    public class latLonPair
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
}
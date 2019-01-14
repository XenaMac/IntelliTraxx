using Base_AVL.LATAService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Base_AVL.ViewModels
{
    public class Vehicle_Driver
    {
        public Driver Driver { get; set; }
        public List<alert> Alerts { get; set; }
        public VehicleExtendedData Vehicle { get; set; }
        public GPSData gps { get; set; }
        public DateTime lastMessageReceived { get; set; }
        public string DriverImage64String { get; set; }
    }
}
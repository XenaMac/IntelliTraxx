using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliFleetKeepAlive
{
    class Program
    {
        static void Main(string[] args)
        {
            IntelliFleetVehicleService.VehicleServiceClient vsc = new IntelliFleetVehicleService.VehicleServiceClient();
            List<IntelliFleetVehicleService.Vehicle> vehicles = vsc.getAllVehicles().ToList();
            if (vehicles.Count <= 0)
            {
                //string badfooey = "badfooey";
            }
        }
    }
}

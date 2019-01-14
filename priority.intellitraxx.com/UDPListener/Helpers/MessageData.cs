using System;
namespace UDPListener.Helpers
{
    public class MessageData
    {
        public Guid messageID { get; set; }
        public string VehicleID { get; set; }
        public string Message { get; set; }
    }
}

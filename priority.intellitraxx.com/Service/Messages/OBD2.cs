using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.Messages
{
    public class OBD2
    {
        public List<kvPair> CAN { get; set; }
        public Int32 T { get; set; } //Timestamp
        public string M { get; set; } //MAC Address
        public DateTime timestampUTC { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class kvPair
    {
        public string K { get; set; }
        public string V { get; set; }
    }

}
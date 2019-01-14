using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDPListener.Helpers;

namespace LATATrax.Models
{
    public interface IVehicle
    {
        void readMessage(MessageData md);
    }
}

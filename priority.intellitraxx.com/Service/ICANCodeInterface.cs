using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICANCodeInterface" in both code and config file together.
    [ServiceContract]
    public interface ICANCodeInterface
    {
        [OperationContract]
        void addDriverBehavior(string MACAddress, string behavior);

        [OperationContract]
        void addODBData(string MACAddress, string name, string val);
    }
}

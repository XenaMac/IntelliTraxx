using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;

namespace LATATrax
{
    /// <summary>
    /// This Service Interface is for the tablet client only. Other functionality is exposed through AJAXVehicles (for AJAX-enabled WCF services for JavaScript) or
    /// TruckService for regular MVC interfaces
    /// </summary>

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITabletInterface" in both code and config file together.
    [ServiceContract(Namespace = "http://tabletinterface.com")]
    public interface ITabletInterface
    {

        #region  " Driver Logon / Logoff "

        [OperationContract]
        [WebInvoke(UriTemplate = "LogonDriver?DriverNumber={DriverNumber}&Password={Password}&VehicleID={VehicleID}")]
        TabletDriver LogonDriver(string DriverNumber, string Password, string VehicleID);

        [OperationContract]
        [WebInvoke(UriTemplate = "DriverAutoLogon?PIN={PIN}&VehicleID={VehicleID}")]
        TabletDriver DriverAutoLogon(string PIN, string VehicleID);

        [OperationContract]
        [WebInvoke(UriTemplate = "LogoffDriver?DriverID={DriverID}&VehicleID={VehicleID}")]
        string LogoffDriver(string DriverID, string VehicleID);

        [OperationContract]
        [WebInvoke(UriTemplate = "LogonDriverByPin?PIN={PIN}&VehicleID={VehicleID}")]
        TabletDriver LogonDriverByPin(string PIN, string VehicleID);

        #endregion

        #region  " GPS Data For Tablet "

        [OperationContract]
        [WebInvoke(UriTemplate = "getPosition?VehicleID={VehicleID}")]
        tabletGPS getPosition(string VehicleID);

        #endregion

        #region  " Status Updates "

        [OperationContract]
        [WebInvoke(UriTemplate="setStatus?StatusID={statusID}&statusName={statusName}&vehicleNumber={vehicleNumber}&driverID={driverID}&statusStart={statusStart}&statusEnd={statusEnd}")]
        string setStatus(string statusID, string statusName, string vehicleNumber, string driverID, string statusStart, string statusEnd);

        #endregion

        #region " Custom Status for Tablet "

        [OperationContract]
        [WebInvoke(UriTemplate = "getCustomStatuses?companyID={companyID}")]
        customStatus[] getCustomStatuses(Guid companyID);

        #endregion

        #region " Dispatches "

        [OperationContract]
        [WebInvoke(UriTemplate = "getDispatches?routerID={routerID}")]
        //NOTE: THIS IS ACTUALLY THE MACADDRESS, NOT THE ROUTERID
        dispatch getDispatches(string routerID);

        [OperationContract]
        [WebInvoke(UriTemplate = "ackDispatch?dispatchID={dispatchID}&note={note}&driverPIN={driverPIN}")]
        void ackDispatch(Guid dispatchID, string note, string driverPIN);

        [OperationContract]
        [WebInvoke(UriTemplate = "closeDispatch?dispatchID={dispatchID}&note={note}")]
        void closeDispatch(Guid dispatchID, string note);

        #endregion

        #region " Initialize new vehicle from tablet "

        [OperationContract]
        [WebInvoke(UriTemplate = "createNewVehicle?vehicleID={vehicleID}&MACAddress={MACAddress}&vehicleFriendlyName={vehicleFriendlyName}&licensePlate={licensePlate}")]
        string createNewVehicle(string vehicleID, string MACAddress, string vehicleFriendlyName, string licensePlate);

        #endregion
    }

    #region " Contracts "

    [DataContract]
    public class tabletGPS
    {
        [DataMember]
        public double lat { get; set; }
        [DataMember]
        public double lon { get; set; }
        [DataMember]
        public double speed { get; set; }
        [DataMember]
        public int direction { get; set;}
    }

    [DataContract]
    public class statusObject
    {
        [DataMember]
        public Guid statusID { get; set; }
        [DataMember]
        public string statusName { get; set; }
        [DataMember]
        public Guid vehicleID { get; set; }
        [DataMember]
        public Guid driverID { get; set; }
        [DataMember]
        public DateTime? statusStart { get; set; }
        [DataMember]
        public DateTime? statusEnd { get; set; }
        [DataMember]
        public Guid runID { get; set; }
    }

    [DataContract]
    public class tabletMessage
    {
        [DataMember]
        public Guid messageID { get; set; }
        [DataMember]
        public string messageText { get; set; }
        [DataMember]
        public string messageSender { get; set; }
        [DataMember]
        public string truckNumber { get; set; }
        [DataMember]
        public bool messageSent { get; set; }
    }


    #region " Unused "
    [DataContract] //Unused - This is part of a test process
    public class TestPerson
    {
        string firstName;
        string lastName;

        [DataMember]
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        [DataMember]
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }
    }
    #endregion

    #endregion
}

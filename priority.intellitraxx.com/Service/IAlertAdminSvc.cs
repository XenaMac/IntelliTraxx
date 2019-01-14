using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAlertAdminSvc" in both code and config file together.
    [ServiceContract]
    public interface IAlertAdminSvc
    {
        #region " Reads "

        #region " Alerts "

        [OperationContract]
        List<alertClass> getAlertClasses();

        [OperationContract]
        List<alertVehicle> getAlertVehicles();

        [OperationContract]
        List<alertGeoFence> getAlertGeoFences();

        [OperationContract]
        List<dbAlert> getAlerts();

        [OperationContract]
        List<linkVehicle> getAllLinkVehicles();

        [OperationContract]
        List<polyData> getPolygons();

        [OperationContract]
        List<string> getLinkedAlertsVehicles(string alertFriendlyName);

        [OperationContract]
        List<string> getLinkedAlertsGeoFences(string alertFriendlyName);

        [OperationContract]
        alertData getAlertData(Guid alertID);

        #endregion

        #region " Schedules "

        [OperationContract]
        List<schedule> getAllSchedules();

        [OperationContract]
        List<vsLink> getAllSchedulesByVehicle(string VehicleID);

        [OperationContract]
        List<vsLink> getAllVehiclesBySchedule(Guid scheduleID);

        #endregion

        #endregion

        #region " Writes "

        #region " Alerts "

        [OperationContract]
        string updateAlertData(dbAlert a, List<alertGeoFence> agfList, List<alertVehicle> avList);

        [OperationContract]
        string deleteAlert(dbAlert a);

        [OperationContract]
        string changeAlertStatus(List<Guid> aList, bool enable, bool updatedb);

        #endregion

        #region " Schedules "

        //NOTE: This is just to update a current, live schedule for a particular vehicle.
        //it does not update the database. To update the database, use updateSchedules
        [OperationContract]
        string changeSchedule(string VehicleID, DateTime start, DateTime end);

        [OperationContract]
        string deleteSchedules(List<schedule> dSchedules);

        [OperationContract]
        string deleteSchedulesBySchedule(Guid scheduleID);

        [OperationContract]
        string deleteSchedulesByVehicle(string vehicleID);

        [OperationContract]
        string updateSchedules(List<schedule> sList);

        [OperationContract]
        string addVehicleToSchedule(Guid scheduleID, List<string> vehicleIDs);

        [OperationContract]
        string addSchedulesToVehicle(string vehicleID, List<Guid> scheduleList);

        #endregion

        #endregion
    }

    #region " Alert Objects "

    [DataContract]
    public class alertVehicle
    {
        [DataMember]
        public Guid AlertID { get; set; }
        [DataMember]
        public Guid VehicleID { get; set; }
        [DataMember]
        public string AlertAction { get; set; }
    }

    [DataContract]
    public class polyData {
        [DataMember]
        public Guid polyID { get; set; }
        [DataMember]
        public string polyName { get; set; }
    }

    [DataContract]
    public class alertGeoFence
    {
        [DataMember]
        public Guid AlertID { get; set; }
        [DataMember]
        public Guid GeoFenceID { get; set; }
    }

    [DataContract]
    public class alertClass
    {
        [DataMember]
        public Guid AlertClassID { get; set; }
        [DataMember]
        public string AlertClassName { get; set; }
    }

    [DataContract]
    public class dbAlert
    {
        [DataMember]
        public Guid AlertID { get; set; }
        [DataMember]
        public bool AlertActive { get; set; }
        [DataMember]
        public DateTime AlertStartTime { get; set; }
        [DataMember]
        public DateTime AlertEndTime { get; set; }
        [DataMember]
        public string AlertType { get; set; }
        [DataMember]
        public Guid AlertClassID { get; set; }
        [DataMember]
        public string AlertClassName { get; set; }
        [DataMember]
        public string AlertFriendlyName { get; set; }
        [DataMember]
        public string minVal { get; set; }
        [DataMember]
        public bool NDB { get; set; }
    }

    [DataContract]
    public class alertData
    {
        [DataMember]
        public dbAlert alert { get; set; }
        [DataMember]
        public List<alertVehicle> alertVehicles { get; set; }
        [DataMember]
        public List<alertGeoFence> alertGeoFences { get; set; }
    }

    [DataContract]
    public class linkVehicle
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string vehicleID { get; set; }
    }

    [DataContract]
    public class macVehicle
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string vehicleID { get; set; }
        [DataMember]
        public string macAddress { get; set; }
    }

    #endregion

    #region " Schedule Objects "

    [DataContract]
    public class schedule {
        [DataMember]
        public Guid scheduleID { get; set; }
        [DataMember]
        public string scheduleName { get; set; }
        [DataMember]
        public Guid companyid { get; set; }
        [DataMember]
        public DateTime startTime { get; set; }
        [DataMember]
        public DateTime endTime { get; set; }
        [DataMember]
        public string createdBy { get; set; }
        [DataMember]
        public DateTime createdOn { get; set; }
        [DataMember]
        public string modifiedBy { get; set; }
        [DataMember]
        public DateTime modifiedOn { get; set; }
        [DataMember]
        public int DOW { get; set; }
        [DataMember]
        public DateTime EffDtStart { get; set; }
        [DataMember]
        public DateTime EffDtEnd { get; set; }
        [DataMember]
        public bool active { get; set; }
    }

    [DataContract]
    public class vsLink {
        [DataMember]
        public string scheduleName { get; set; }
        [DataMember]
        public DateTime startTime { get; set; }
        [DataMember]
        public DateTime endTime { get; set; }
        [DataMember]
        public string vehicleID { get; set; }
        [DataMember]
        public Guid vID { get; set; }
    }

    #endregion
}

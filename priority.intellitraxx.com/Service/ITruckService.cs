using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ITruckService
    {
        #region " Vehicle object interfaces "

        /// <summary>
        /// return a comma delimeted list of all vehicles in the service
        /// </summary>
        /// <returns>List of vehicles</returns>
        [OperationContract]
        string getVehicles();

        /// <summary>
        /// return a list of vehicle objects for all vehicles in service
        /// </summary>
        /// <returns>list of vehicle</returns>
        [OperationContract]
        List<retVehicle> getVehicleList();

        /// <summary>
        /// return a list of linkVehicle objects for all vehicles in a list
        /// </summary>
        /// <returns>list of vehicle</returns>
        [OperationContract]
        List<linkVehicle> getVehicleListBasic();

        /// <summary>
        /// return a list of linkVehicle objects for all vehicles in a list
        /// </summary>
        /// <returns>list of vehicle</returns>
        [OperationContract]
        List<macVehicle> getVehicleListMac();

        /// <summary>
        /// returns a list of vehicle objects for all vehicles in system
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<Models.Vehicle> getAllVehicles(bool loadHistorical);

        /// <summary>
        /// returns a list of vehicle GPDTracker objects for specific vehicle
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<VehicleGPSRecord> getLastTwoHours(string vehicleID);

        /// <summary>
        /// returns a list of vehicle GPDTracker objects for specific vehicle history range
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<VehicleGPSRecord> getLocationHistory(string vehicleID, DateTime start, DateTime end);

        /// <summary>
        /// returns list of basic information rather than entire vehicle object
        /// base on vehiclegpsrecord class
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<VehicleGPSRecord> getAllGPS();

        /// <summary>
        /// returns list of basic information rather than entire vehicle object of a specific vehicle id
        /// base on vehiclegpsrecord class
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<VehicleGPSRecord> getGPS(Guid id);

        /// <summary>
        /// returns extended data for a vehicle
        /// expect vehicle.extendeddata.id
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [OperationContract]
        Models.Vehicle getVehicleData(Guid ID);

        /// <summary>
        /// Primarily an admin thing, lets us remove a misbehaving vehicle from the service
        /// </summary>
        /// <param name="vehicleID"></param>
        [OperationContract]
        string killVehicle(string vehicleID);

        [OperationContract]
        List<VehicleExtendedData> getAvailableVehicles();

        #endregion

        #region " User Object interfaces "

        [OperationContract]
        Guid logonUser(string userEmail, string userPassword);

        #region  " User Data "

        [OperationContract]
        User getUserProfile(Guid userID);

        [OperationContract]
        List<User> getUsers(Guid companyID);

        [OperationContract]
        List<User> getUsersByRole(Guid roleID);

        [OperationContract]
        List<User> getUsersByCompany(Guid companyID);

        [OperationContract]
        string setUser(User u, Guid operatorID);

        [OperationContract]
        string resetPassword(Guid userID, string newPassword, string oldPassword, Guid operatorID);

        [OperationContract]
        string deleteUser(User u, Guid operatorID);

        [OperationContract]
        List<Guid> getUserRolesGuids(Guid userID);

        [OperationContract]
        List<Guid> getUserCompanies(Guid userID);

        [OperationContract]
        List<Company> getUserCompaniesFull(Guid userID);

        [OperationContract]
        List<Role> getUserRolesFull(Guid userID);

        #endregion

        #region " Company Information "

        [OperationContract]
        List<Company> getCompanies(Guid companyID);

        [OperationContract]
        string setCompany(Company c, Guid operatorID);

        [OperationContract]
        string deleteCompany(Company c, Guid operatorID);

        #endregion

        #region " Role Information "

        [OperationContract]
        List<Role> getRoles(Guid roleID);


        [OperationContract]
        string setRole(Role r, Guid operatorID);

        [OperationContract]
        string deleteRole(Role r, Guid operatorID);

        #endregion

        #region " User Role/User Company Association "

        [OperationContract]
        string addUserToCompany(Guid userID, Guid companyID, Guid operatorID);

        [OperationContract]
        string removeUserFromCompany(Guid userID, Guid companyID, Guid operatorID);

        [OperationContract]
        string addUserToRole(Guid userID, Guid roleID, Guid operatorID);

        [OperationContract]
        string removeUserFromRole(Guid userID, Guid roleID, Guid operatorID);

        [OperationContract]
        List<UserRole> getAllUserRoles();

        [OperationContract]
        List<UserCompany> getAllUserCompanies();

        #endregion

        #region " Vehicle Extended Data "

        [OperationContract]
        List<VehicleExtendedData> getExtendedData();

        [OperationContract]
        void updateExtendedData(VehicleExtendedData v, Guid operatorID);

        [OperationContract]
        void deleteExtendedData(VehicleExtendedData v, Guid operatorID);

        #endregion

        #region " Vehicle Classes "

        [OperationContract]
        List<VehicleClass> getVehicleClasses();

        [OperationContract]
        void updateVehicleClass(VehicleClass v, Guid operatorID);

        [OperationContract]
        void deleteVehicleClass(VehicleClass v, Guid operatorID);

        #endregion

        #region " Drivers, data and logon/logoff "

        [OperationContract]
        List<Driver> getDrivers();

        [OperationContract]
        List<Driver> getAvailableDrivers();

        [OperationContract]
        void updateDriver(Driver d, Guid operatorID);

        [OperationContract]
        string deleteVehicleDriver(string ID);

        [OperationContract]
        void deleteDriver(Driver d, Guid operatorID);

        [OperationContract]
        List<driverVehicleReturn> driverVehicleReturn();

        #endregion

        #endregion

        #region " Var Data "

        [OperationContract]
        List<systemvar> getAppVars();

        [OperationContract]
        void updateAppVar(systemvar appVar, Guid operatorID);

        [OperationContract]
        void deleteAppVar(systemvar appVar, Guid operatorID);

        [OperationContract]
        List<systemvar> getServiceVars();

        [OperationContract]
        void updateServiceVar(systemvar v, Guid operatorid);

        [OperationContract]
        void deleteServiceVar(systemvar v, Guid operatorID);

        [OperationContract]
        List<systemvar> getVars();

        [OperationContract]
        systemvar getSpecificVar(string varName, int type);

        [OperationContract]
        void updateVar(systemvar v, Guid operatorID);

        [OperationContract]
        void deleteVar(systemvar v, Guid operatorID);

        #endregion

        #region " Status information "

        [OperationContract]
        List<statusObjectReturn> getAllStatus();

        [OperationContract]
        List<statusObjectReturn> getTruckStatus(string truckID);

        [OperationContract]
        List<customStatus> getCustomStatuses(Guid companyID);

        [OperationContract]
        void updateCustomStatus(customStatus c);

        #endregion

        #region " Dispatches "

        [OperationContract]
        string dispatchVehicle(dispatch d);

        [OperationContract]
        List<dispatch> getAllDispatches();

        [OperationContract]
        List<dispatch> getDispatchesByVehicle(string vehicleID);

        [OperationContract]
        List<dispatch> getDispatchesByRange(DateTime start, DateTime end);

        [OperationContract]
        dispatch getDispatchesByID(Guid dispatchID);

        #endregion

        #region " Alarts "

        [OperationContract]
        List<alertReturn> getAllAlerts();

        [OperationContract]
        List<alertReturn> getAllAlertsByVehicle(string VehicleID);

        [OperationContract]
        List<alertReturn> getAllAlertsByRange(DateTime start, DateTime end);

        [OperationContract]
        List<alertReturn> getAllAlertsByRangeByType(DateTime start, DateTime end, string type);

        [OperationContract]
        alertReturn getAllAlertByID(Guid ID);

        [OperationContract]
        List<alertReturn> getAllAlertsByRangeByVehicle(DateTime start, DateTime end, string vehicleID);

        [OperationContract]
        List<VehicleGPSRecord> getGPSTracking(string vehicleID, string from, string to);

        [OperationContract]
        string clearAlerts(bool clearAll, string vehicleID);

        #endregion

        #region " Reload Data "

        [OperationContract]
        void reloadVehicles();

        #endregion

        #region " Force Driver Logoff "

        [OperationContract]
        string logoffDriver(string PIN, Guid operatorID);

        [OperationContract]
        string changeDrivers(string from, string to, string vehicleID, string modifiedBy);

        [OperationContract]
        string removeDriver(string from, string vehicleID);

        #endregion

        #region "OBDData Reads"

        [OperationContract]
        List<OBDLog> getOBDDataReturnByDateRange(string VehicleID, DateTime from, DateTime to);

        #endregion  
    }

    #region " Data Objects "

    #region " Vehicle Data "

    /// <summary>
    /// Abstracted vehicle data for consumption at Website
    /// </summary>
    [DataContract]
    public class retVehicle
    {
        [DataMember]
        public string VehicleID { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public Messages.GPSData gpsData { get; set; }
        [DataMember]
        public List<Messages.Status> status { get; set; }
    }
    [DataContract]
    public class driverBehavior
    {
        [DataMember]
        public string behavior { get; set; }
        [DataMember]
        public DateTime timeStamp { get; set; }
        [DataMember]
        public Guid driverID { get; set; }
    }

    [DataContract]
    public class OBD2Data {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string val { get; set; }
    }



    #endregion

    #region " User Data "

    /// <summary>
    /// User Object.
    /// </summary>
    [DataContract]
    public class User
    {
        [DataMember]
        public Guid UserID { get; set; }
        [DataMember]
        public string UserLastName { get; set; }
        [DataMember]
        public string UserFirstName { get; set; }
        [DataMember]
        public string UserEmail { get; set; }
        [DataMember]
        public string UserOffice { get; set; }
        [DataMember]
        public string UserPhone { get; set; }
        [DataMember]
        public string UserPassword { get; set; }
        [DataMember]
        public string UserSalt { get; set; }
    }

    /// <summary>
    /// Company object. Note: requires a user object for contact
    /// </summary>
    [DataContract]
    public class Company
    {
        [DataMember]
        public Guid CompanyID { get; set; }
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string CompanyAddress { get; set; }
        [DataMember]
        public string CompanyCity { get; set; }
        [DataMember]
        public string CompanyState { get; set; }
        [DataMember]
        public string CompanyCountry { get; set; }
        [DataMember]
        public User CompanyContact { get; set; }
        [DataMember]
        public bool isParent { get; set; }
    }

    /// <summary>
    /// Role Object
    /// </summary>
    [DataContract]
    public class Role
    {
        [DataMember]
        public Guid RoleID { get; set; }
        [DataMember]
        public string roleName { get; set; }
        [DataMember]
        public string roleDescription { get; set; }
        [DataMember]
        public bool isAdmin { get; set; }
    }

    /// <summary>
    /// relate users to roles
    /// </summary>
    [DataContract]
    public class UserRole
    {
        [DataMember]
        public Guid UserID { get; set; }
        [DataMember]
        public Guid RoleID { get; set; }
    }

    /// <summary>
    /// Relate users to companies
    /// </summary>
    [DataContract]
    public class UserCompany
    {
        [DataMember]
        public Guid UserID { get; set; }
        [DataMember]
        public Guid CompanyID { get; set; }
    }

    #endregion

    #region " Driver "

    [DataContract]
    public class Driver
    {
        [DataMember]
        public Guid DriverID { get; set; }
        [DataMember]
        public string DriverLastName { get; set; }
        [DataMember]
        public string DriverFirstName { get; set; }
        [DataMember]
        public Guid CompanyID { get; set; }
        [DataMember]
        public string DriverPassword { get; set; }
        [DataMember]
        public string ProfilePic { get; set; }
        [DataMember]
        public string DriverEmail { get; set; }
        [DataMember]
        public string DriverNumber { get; set; }
        [DataMember]
        public statusObject currentStatus { get; set; }
        [DataMember]
        public byte[] imageData { get; set; }
        [DataMember]
        public string imageType { get; set; }
        [DataMember]
        public string PIN { get; set; }
    }

    [DataContract]
    public class TabletDriver
    {
        [DataMember]
        public string DriverID { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string CompanyName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string DriverNumber { get; set; }
        [DataMember]
        public byte[] DriverImage { get; set; }
        [DataMember]
        public string ImageType { get; set; }
        [DataMember]
        public Guid runID { get; set; }
    }

    #endregion

    #region " Vehicle Extended Data "

    [DataContract]
    public class VehicleExtendedData
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string vehicleID { get; set; }
        [DataMember]
        public Guid vehicleClassID { get; set; }
        [DataMember]
        public string vehicleClass { get; set; }
        [DataMember]
        public string vehicleClassImage { get; set; }
        [DataMember]
        public Guid companyID { get; set; }
        [DataMember]
        public string companyName { get; set; }
        [DataMember]
        public string licensePlate { get; set; }
        [DataMember]
        public string Make { get; set; }
        [DataMember]
        public string Model { get; set; }
        [DataMember]
        public int Year { get; set; }
        [DataMember]
        public int haulLimit { get; set; }
        [DataMember]
        public string VehicleFriendlyName { get; set; }
        [DataMember]
        public string RouterID { get; set; }
        [DataMember]
        public string MACAddress { get; set; }

    }

    [DataContract]
    public class VehicleClass
    {
        [DataMember]
        public Guid VehicleClassID { get; set; }
        [DataMember]
        public string VehicleClassName { get; set; }
        [DataMember]
        public string VehicleClassDescription { get; set; }
        [DataMember]
        public string VehicleClassImage { get; set; }
    }

    [DataContract]
    public class VehicleGPSRecord
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string VehicleID { get; set; }
        [DataMember]
        public float Direction { get; set; }
        [DataMember]
        public float Speed { get; set; }
        [DataMember]
        public float Lat { get; set; }
        [DataMember]
        public float Lon { get; set; }
        [DataMember]
        public bool InPolygon { get; set; }
        [DataMember]
        public string PolyName { get; set; }
        [DataMember]
        public DateTime timestamp { get; set; }
        [DataMember]
        public Guid runID { get; set; }
        [DataMember]
        public DateTime lastMessageReceived { get; set; }
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public int ABI { get; set; }
    }

    [DataContract]
    public class CompanyAlerts
    {
        [DataMember]
        public Guid AlertID { get; set; }
        [DataMember]
        public string AlertName { get; set; }
        [DataMember]
        public string Email { get; set; }
    }

    [DataContract]
    public class VehicleSignal
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string MAC { get; set; }
        [DataMember]
        public DateTime timestamp{ get; set; }
        [DataMember]
        public float Lat { get; set; }
        [DataMember]
        public float Lon{ get; set; }
        [DataMember]
        public float dBm { get; set; }
        [DataMember]
        public float SINR { get; set; }
    }
    #endregion

    #region " Alarts "

    [DataContract]
    public class alertReturn {
        [DataMember]
        public string vehicleID { get; set; }
        [DataMember]
        public Guid alertID { get; set; }
        [DataMember]
        public string alertType { get; set; }
        [DataMember]
        public string alertName { get; set; }
        [DataMember]
        public bool alertActive { get; set; }
        [DataMember]
        public DateTime alertStart { get; set; }
        [DataMember]
        public DateTime alertEnd { get; set; }
        [DataMember]
        public string latLonStart { get; set; }
        [DataMember]
        public string latLonEnd { get; set; }
        [DataMember]
        public string maxVal { get; set; }
        [DataMember]
        public Guid runID { get; set; }
    }


    #endregion

    #region " var data "
    [DataContract]
    public class systemvar
    {
        [DataMember]
        public Guid varID { get; set; }
        [DataMember]
        public string varName { get; set; }
        [DataMember]
        public string varVal { get; set; }
        [DataMember]
        public int varType { get; set; }
        [DataMember]
        public string minValue { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public bool Editable { get; set; }
        [DataMember]
        public bool Required { get; set; }
    }

    #endregion

    #region " Status Object(s) "

    [DataContract]
    public class statusObjectReturn
    {
        [DataMember]
        public string DriverName { get; set; }
        [DataMember]
        public string VehicleNumber { get; set; }
        [DataMember]
        public string StatusName { get; set; }
        [DataMember]
        public DateTime? statusStart { get; set; }
        [DataMember]
        public DateTime? statusEnd { get; set; }
    }

    [DataContract]
    public class customStatus {
        [DataMember]
        public string companyName { get; set; }
        [DataMember]
        public string vehicleStatusName { get; set; }
        [DataMember]
        public string vehicleStatusDescription { get; set; }
        [DataMember]
        public string vehicleStatusColor { get; set; }
        [DataMember]
        public Guid customStatusID { get; set; }
        [DataMember]
        public Guid companyID { get; set; }
    }

    #endregion

    #region " Dispatches "

    [DataContract]
    public class dispatch {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public DateTime timeStamp { get; set;}
        [DataMember]
        public string UserEmail { get; set; }
        [DataMember]
        public string vehicleID { get; set; }
        [DataMember]
        public Guid? runID { get; set; }
        [DataMember]
        public string address { get; set; }
        [DataMember]
        public string city { get; set; }
        [DataMember]
        public string state { get; set; }
        [DataMember]
        public string zip { get; set; }
        [DataMember]
        public string note { get; set; }
        [DataMember]
        public bool acked { get; set; }
        [DataMember]
        public DateTime? ackTime { get; set; }
        [DataMember]
        public string ackMessage { get; set; }
        [DataMember]
        public string driverPIN { get; set; }
        [DataMember]
        public string completedMessage { get; set; }
        [DataMember]
        public DateTime? completedTime { get; set; }
        [DataMember]
        public string MACAddress { get; set; }
        [DataMember]
        public string routerID { get; set; }
        [DataMember]
        public DateTime sentToVehicle { get; set; }
    }

    #endregion

    #region driverVehicles

    [DataContract]
    public class driverVehicleReturn
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public Guid VehicleID { get; set; }
        [DataMember]
        public Guid DriverID { get; set; }
        [DataMember]
        public DateTime CreatedDate { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public DateTime ModifiedDate { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public VehicleExtendedData Vehicle { get; set; }

        [DataMember]
        public Driver driver { get; set; }
    }

    #endregion

    #region "ODBLog"
    [DataContract]
    public class OBDLog
    {
        [DataMember]
        public Guid OBDLogID { get; set; }
        [DataMember]
        public DateTime timestamp { get; set; }
        [DataMember]
        public Guid VehicleID { get; set; }
        [DataMember]
        public Guid runID { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string val { get; set; }
    }
    #endregion

    #endregion
}

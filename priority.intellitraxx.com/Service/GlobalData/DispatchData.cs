using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.GlobalData
{
    public static class DispatchData
    {
        public static List<dispatch> dispatches = new List<dispatch>();

        #region " Add a dispatch "

        public static void addDispatch(dispatch d) {
            d.ID = Guid.NewGuid();
            //first check to see if we have the vehicle loaded
            var vList = from v in GlobalData.vehicles
                                   where v.VehicleID == d.vehicleID
                                   select v;
            if (vList.Count() > 0)
            {
                //we've got a connected vehicle, update with current information
                foreach (Models.Vehicle v in vList)
                {
                    d.MACAddress = v.extendedData.MACAddress;
                    d.routerID = v.extendedData.RouterID;
                    d.timeStamp = DateTime.Now.ToUniversalTime();
                    d.runID = v.runID;
                    SQLCode sql = new SQLCode();
                    sql.logDispatchData(d);
                    dispatches.Add(d);
                }
            }
            else {
                //truck not currently logged into system, look it up in the database
                SQLCode sql = new SQLCode();
                VehicleExtendedData vd = sql.loadVehicleExtendedData(d.vehicleID);
                if (vd != null) {
                    d.routerID = vd.RouterID;
                    d.MACAddress = vd.MACAddress;
                    d.timeStamp = DateTime.Now.ToUniversalTime();
                    d.runID = new Guid("00000000-0000-0000-0000-000000000000");
                    sql.logDispatchData(d);
                    dispatches.Add(d);
                }
            }
        }

        #endregion

        #region " Vehicle responses to dispatch "

        public static void ackDispatch(Guid dispatchID, string note, string driverPIN) {
            try {
                Guid runID = Guid.Empty;
                var tList = from t in GlobalData.vehicles
                            where t.driver.DriverNumber == driverPIN
                            select t;
                if (tList.Count() > 0)
                {
                    foreach (Models.Vehicle v in tList)
                    {
                        runID = v.runID;
                    }
                }
                else {
                    throw new Exception("Can't find vehicle based on DriverPIN: " + driverPIN);
                }
                var dList = from d in dispatches
                            where d.ID == dispatchID
                            select d;
                if (dList.Count() > 0)
                {
                    foreach (dispatch d in dList)
                    {
                        d.acked = true;
                        d.ackTime = DateTime.Now.ToUniversalTime();
                        d.ackMessage = note;
                        d.driverPIN = driverPIN;
                        d.runID = runID;
                        SQLCode sql = new SQLCode();
                        sql.ackDispatch(d);
                    }
                }
                else {
                    //look for the dispatch in database
                    SQLCode sql = new SQLCode();
                    dispatch d = sql.findDispatch(dispatchID);
                    if (d != null)
                    {
                        d.acked = true;
                        d.ackTime = DateTime.Now.ToUniversalTime();
                        d.ackMessage = note;
                        d.driverPIN = driverPIN;
                        d.runID = runID;
                        sql.ackDispatch(d);
                    }
                    else {
                        //TODO log this as an error and move on.
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        public static void closeDispatch(Guid dispatchID, string completedMessage) {
            try {
                var dList = from d in dispatches
                            where d.ID == dispatchID
                            select d;
                if (dList.Count() > 0)
                {
                    foreach (dispatch d in dList)
                    {
                        d.completedMessage = completedMessage;
                        d.completedTime = DateTime.Now.ToUniversalTime();
                        SQLCode sql = new SQLCode();
                        sql.closeDispatch(d);
                    }
                }
                else {
                    SQLCode sql = new SQLCode();
                    dispatch d = sql.findDispatch(dispatchID);
                    if (d != null)
                    {
                        d.completedMessage = completedMessage;
                        d.completedTime = DateTime.Now.ToUniversalTime();
                        sql.closeDispatch(d);
                    }
                    else {
                        //TODO log this as an error and move on.
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Get dispatches for Website "

        public static List<dispatch> getAllDispatches() {
            List<dispatch> dispList = new List<dispatch>();
            try {
                foreach (dispatch d in dispatches) {
                    dispList.Add(d);
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
            return dispList;
        }

        public static List<dispatch> getDispatchesByRange(DateTime start, DateTime end) {
            List<dispatch> dispList = new List<dispatch>();
            try
            {
                //this list goes to the database to find historical information
                SQLCode sql = new SQLCode();
                //<dispatch> dispatched = sql.getDispatchesByRange(start, end); WTH!
                dispList = sql.getDispatchesByRange(start, end);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return dispList;
        }

        public static List<dispatch> getDispatchesByVehicle(string vehicleNumber) {
            List<dispatch> dispList = new List<dispatch>();
            try
            {
                var dList = from d in dispatches
                            where d.vehicleID == vehicleNumber
                            select d;
                foreach (dispatch d in dList) {
                    dispList.Add(d);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return dispList;
        }

        public static dispatch getDispatchesByID(Guid dispatchID)
        {
            dispatch disp = new dispatch();
            try
            {
                //this list goes to the database to find historical information
                SQLCode sql = new SQLCode();
                //<dispatch> dispatched = sql.getDispatchesByRange(start, end); WTH!
                disp = sql.findDispatchByID(dispatchID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return disp;
        }

        #endregion

        #region " Tablet Client Get Dispatches "

        public static dispatch getDispatchesForVehicle(string MACAddress) {
            dispatch dList = new dispatch();
            try {
                var vList = from v in GlobalData.vehicles
                            where v.extendedData.MACAddress == MACAddress
                            select v;
                if (vList.Count() > 0)
                {
                    foreach (Models.Vehicle v in vList)
                    {
                        var ds = from d in dispatches
                                 where d.vehicleID == v.VehicleID
                                 && d.sentToVehicle.AddMinutes(5) < DateTime.Now
                                 && d.acked == false
                                 select d;
                        foreach (dispatch d in ds)
                        {
                            d.sentToVehicle = DateTime.Now.ToUniversalTime();
                            dList = d;
                        }
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.ToString());
            }
            

            return dList;
        }

        #endregion
    }
}
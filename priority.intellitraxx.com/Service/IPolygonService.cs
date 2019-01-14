using LATATrax.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IPolygonService" in both code and config file together.
    [ServiceContract]
    public interface IPolygonService
    {
        /// <summary>
        /// return a comma delimeted list of all polygons in the db
        /// </summary>
        /// <returns>List of vehicles</returns>
        [OperationContract]
        List<polygonData> getPolygons();

        // <summary>
        /// return a single polygons from the db
        /// </summary>
        /// <returns>single polygondata</returns>
        [OperationContract]
        polygonData getPolygon(string id);

        /// <summary>
        /// adds a geo fence from the service and the database
        /// </summary>
        /// <param name="geoFenceID">polygonData poly</param>
        [OperationContract]
        string addPolygon(Models.polygonData poly);

        /// <summary>
        /// delete a geo fence from the service and the database
        /// </summary>
        /// <param name="geoFenceID">string fence ID</param>
        [OperationContract]
        void deletePolygon(Guid GeoFenceID);
    }
}

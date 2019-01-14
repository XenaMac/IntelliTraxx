using LATATrax.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace LATATrax
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "PolygonService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select PolygonService.svc or PolygonService.svc.cs at the Solution Explorer and start debugging.
    public class PolygonService : IPolygonService
    {
        /// <summary>
        /// Returns a comma-delimited string of all polygons in service
        /// </summary>
        /// <returns>list of vehicles</returns>
        public List<polygonData> getPolygons()
        {
            return GeoCode.GlobalGeo.polygons;
        }

        /// <summary>
        /// Returns a single string of all a polygon in service
        /// </summary>
        /// <returns>list of vehicles</returns>
        public polygonData getPolygon(string id)
        {
            Guid ID = new Guid(id);
            return GeoCode.GlobalGeo.polygons.Where(p => p.geoFenceID == ID).SingleOrDefault();
        }

        /// <summary>
        /// Returns a sbool of success after polygon deleted
        /// </summary>
        /// <returns>list of vehicles</returns>
        public string addPolygon(Models.polygonData poly)
        {
            try
            {
                if (poly.geoFenceID == new Guid("00000000-0000-0000-0000-000000000000"))
                {
                    poly.geoFenceID = Guid.NewGuid();
                    //poly.polyID = poly.geoFenceID.ToString();
                }
                string res = "OK";
                //Models.polygonData pd = JsonConvert.DeserializeObject<Models.polygonData>(poly);
                Models.polygonData pd = poly;
                res = GeoCode.GlobalGeo.addPolygon(pd);
                return res;
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.ToString();
            }
        }

        /// <summary>
        /// Returns a sbool of success after polygon deleted
        /// </summary>
        /// <returns>list of vehicles</returns>
        public void deletePolygon(Guid GeoFenceID)
        {
            try
            {
                GeoCode.GlobalGeo.deletePolygon(GeoFenceID);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }   
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LATATrax.GeoCode
{
    public static class GlobalGeo
    {
        #region " variables and lists "

        public static List<Models.polygonData> polygons = new List<Models.polygonData>();

        #endregion

        #region " Add / Delete Polygons "

        /// <summary>
        /// Add or Update a polygon. If the polygon exists it is updated, if not it is created
        /// </summary>
        /// <param name="polygon">Polygon Data Object</param>
        /// <returns>OK if all is well, else error message</returns>
        public static string addPolygon(Models.polygonData polygon)
        {
            string res = "OK";
            try
            {
                Models.polygonData found = polygons.Find(delegate(Models.polygonData find)
                {
                    return find.geoFenceID == polygon.geoFenceID;
                });
                if (found == null)
                {
                    polygons.Add(polygon);
                    //add to DB
                }
                else
                {
                    found.polyName = polygon.polyName;
                    found.notes = polygon.notes;
                    found.geoFence = polygon.geoFence;
                    found.geoType = polygon.geoType;
                    found.radius = polygon.radius;
                    //update in DB
                }
                GlobalData.SQLCode sql = new GlobalData.SQLCode();
                sql.setGeoFence(polygon, "jmckinley@xenatech.com");
            }
            catch (Exception ex)
            {
                res = ex.Message;
            }
            return res;
        }

        /// <summary>
        /// removes a polygon from service and the database
        /// </summary>
        /// <param name="geoFenceID">This needs to be changed</param>
        /// <returns>OK if it worked, else error message</returns>
        public static string deletePolygon(Guid geoFenceID)
        {
            try
            {
                string res = "OK";

                for (int i = polygons.Count - 1; i >= 0; i--)
                {
                    if (polygons[i].geoFenceID == geoFenceID)
                    {
                        polygons.RemoveAt(i);
                        GlobalData.SQLCode sql = new GlobalData.SQLCode();
                        sql.deleteGeoFence(geoFenceID);
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        #endregion

        #region " Geo Fence Checks "

        /// <summary>
        /// This will determine if a lat/lon pair is inside a polygon
        /// Deprecated and replaced with findInside which will return
        /// the name of the polygon the pair is inside of
        /// </summary>
        /// <param name="lat">Current Latitude</param>
        /// <param name="lon">Current Longitude</param>
        /// <returns>true/false</returns>
        public static bool isInside(double lat, double lon)
        {
            bool inside = false;

            foreach (Models.polygonData pd in polygons)
            {
                if (lat >= pd.minLat && lat <= pd.maxLat && lon >= pd.minLon && lon <= pd.maxLon)
                {
                    if (pd.geoType.ToUpper() == "POLYGON")
                    {
                        int i;
                        int j = pd.geoFence.Count - 1;
                        for (i = 0; i < pd.geoFence.Count; i++)
                        {
                            if (pd.geoFence[i].Lon < lon && pd.geoFence[j].Lon >= lon
                                || pd.geoFence[j].Lon < lon && pd.geoFence[i].Lon >= lon)
                            {
                                if (pd.geoFence[i].Lat + (lon - pd.geoFence[i].Lon) / (pd.geoFence[j].Lon - pd.geoFence[i].Lon) * (pd.geoFence[j].Lat - pd.geoFence[i].Lat) < lat)
                                {
                                    inside = !inside;
                                }
                            }
                            j = i;
                        }
                    }
                    else if (pd.geoType.ToUpper() == "CIRCLE")
                    {
                        //TODO:Add code for geo circle
                        Position center = new Position();
                        center.Latitude = pd.geoFence[0].Lat;
                        center.Longitude = pd.geoFence[0].Lon;
                        Position truck = new Position();
                        truck.Latitude = lat;
                        truck.Longitude = lon;
                        Haversine h = new Haversine();
                        double distance = h.Distance(center, truck, DistanceType.Kilometers);
                        if (distance < (pd.radius / 1000)) //radii are in meters, divide by 1k to get kilometers. Yay metric system!
                        {
                            inside = !inside;
                        }
                    }
                }
            }
            return inside;
        }

        /// <summary>
        /// return the name of the polygon the lat/lon pair is inside of.
        /// This replaces the isInside function by returning the name of the polygon
        /// return "NA" if not in any polygon
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns>Name of polygon</returns>
        public static Models.polygonData findInside(double lat, double lon)
        {
            Models.polygonData polyName = new Models.polygonData();
            bool inside = false;
            if (polygons.Count == 0) {
                return polyName;
            }
            try
            {
                foreach (Models.polygonData pd in polygons)
                {
                    if (lat >= pd.minLat && lat <= pd.maxLat && lon >= pd.minLon && lon <= pd.maxLon)
                    {
                        if (pd.geoType.ToUpper() == "POLYGON")
                        {
                            int i;
                            int j = pd.geoFence.Count - 1;
                            for (i = 0; i < pd.geoFence.Count; i++)
                            {
                                if (pd.geoFence[i].Lon < lon && pd.geoFence[j].Lon >= lon
                                    || pd.geoFence[j].Lon < lon && pd.geoFence[i].Lon >= lon)
                                {
                                    if (pd.geoFence[i].Lat + (lon - pd.geoFence[i].Lon) / (pd.geoFence[j].Lon - pd.geoFence[i].Lon) * (pd.geoFence[j].Lat - pd.geoFence[i].Lat) < lat)
                                    {
                                        inside = !inside;
                                        if (inside)
                                        {
                                            polyName = pd;
                                        }
                                        else
                                        {
                                            polyName = null;
                                        }
                                    }
                                }
                                j = i;
                            }
                        }
                        else if (pd.geoType.ToUpper() == "CIRCLE")
                        {
                            //add code for circle polyg
                            Position center = new Position();
                            center.Latitude = pd.geoFence[0].Lat;
                            center.Longitude = pd.geoFence[0].Lon;
                            Position truck = new Position();
                            truck.Latitude = lat;
                            truck.Longitude = lon;
                            Haversine h = new Haversine();
                            double distance = h.Distance(center, truck, DistanceType.Miles);
                            if (distance < (pd.radius / 1000))
                            {
                                inside = !inside;
                                if (inside)
                                {
                                    polyName = pd;
                                }
                                else
                                {
                                    polyName = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return polyName;
        }

        #endregion

        #region " Load polygon data from database "

        public static void loadStoredPolygons()
        {
            GlobalData.SQLCode sql = new GlobalData.SQLCode();
            List<Models.polygonData> pdList = sql.getGeoFences();
            polygons = pdList;
        }

        public static void reloadPolygon(Guid geoFenceID)
        {
            //reload from DB

            //Find the existing polygon, remove it, add the new one
        }

        #endregion

        #region " General Helpers "

        /// <summary>
        /// Finds the center of a polygon. Note, the center may not be in the polygon
        /// </summary>
        /// <param name="polyData"></param>
        /// <returns>Lat/Lon pair for center of polygon</returns>
        public static Models.latLonPair findCentroid(List<Models.LatLon> polyData)
        {
            Models.latLonPair center = new Models.latLonPair();
            center.lat = 0;
            center.lng = 0;
            double signedArea = 0.0;
            double x0 = 0.0;
            double y0 = 0.0;
            double x1 = 0.0;
            double y1 = 0.0;
            double a = 0.0;
            int i = 0;

            //for all but last piece
            for (i = 0; i < polyData.Count - 1; i++)
            {
                x0 = polyData[i].Lon;
                y0 = polyData[i].Lat;
                x1 = polyData[i + 1].Lon;
                y1 = polyData[i + 1].Lat;
                a = x0 * y1 - x1 * y0;
                signedArea += a;
                center.lng += (x0 + x1) * a;
                center.lat += (y0 + y1) * a;
            }

            //last piece;
            x0 = polyData[i].Lon;
            y0 = polyData[i].Lat;
            x1 = polyData[0].Lon;
            y1 = polyData[0].Lat;
            a = x0 * y1 - x1 * y0;
            signedArea += a;
            center.lng += (x0 + x1) * a;
            center.lat += (y0 + y1) * a;

            signedArea *= 0.5;
            center.lng /= (6 * signedArea);
            center.lat /= (6 * signedArea);

            return center;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace LATATrax
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AJAXVehicles
    {
        // To use HTTP GET, add [WebGet] attribute. (Default ResponseFormat is WebMessageFormat.Json)
        // To create an operation that returns XML,
        //     add [WebGet(ResponseFormat=WebMessageFormat.Xml)],
        //     and include the following line in the operation body:
        //         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        [OperationContract]
        public void DoWork()
        {
            // Add your operation implementation here
            return;
        }

        [OperationContract]
        [WebGet]
        public string getLogon(string userName, string password)
        {
            userData ud = new userData();
            ud.userID = userName + " checked";
            ud.password = password + " checked";
            return JsonConvert.SerializeObject(ud);
        }

        [OperationContract]
        [WebGet]
        public string serviceTest()
        {
            return "Found";
        }

        #region " get / set vehicle values "

        /// <summary>
        /// gets a lits of current vehicles in the system.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [WebGet]
        public string getJSONVehicles()
        {
            List<Models.Vehicle> vehicles = new List<Models.Vehicle>();
            foreach (Models.Vehicle veh in GlobalData.GlobalData.vehicles)
            {
                vehicles.Add(veh);
            }
            string str = JsonConvert.SerializeObject(vehicles);
            return str;
        }

        #endregion

        #region " geo fence designer interfaces "

        /// <summary>
        /// Primary interface for the geofence editor. objects are passed in JSON and converted
        /// to .NET polygonData objects by magic
        /// </summary>
        /// <param name="poly"></param>
        /// <returns>OK or Error</returns>
        [OperationContract]
        [WebInvoke]
        public string addPolygon(Models.polygonData poly, string type)
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
        /// returns JSON serialized polygonData objects to the geo fence editor
        /// </summary>
        /// <returns>JSON objects in string format</returns>
        [OperationContract]
        [WebGet]
        public string getGeoFenceDesignerObjects()
        {
            try
            {
                List<Models.polygonDesign> polys = new List<Models.polygonDesign>();
                foreach (Models.polygonData pd in GeoCode.GlobalGeo.polygons)
                {
                    Models.polygonDesign p = new Models.polygonDesign();
                    p.MapID = pd.geoFenceID.ToString();
                    p.zoom = 19;
                    p.tilt = 0;
                    p.mapTypeId = "hybrid";
                    Models.latLonPair llCenter = GeoCode.GlobalGeo.findCentroid(pd.geoFence);
                    p.center = llCenter;
                    Models.layer l = new Models.layer();
                    l.type = pd.geoType.ToLower();
                    l.title = pd.polyName;
                    l.content = pd.notes;
                    l.fillColor = "#000000";
                    l.fillOpacity = 0.3;
                    l.strokeColor = "#000000";
                    l.strokeOpacity = 0.9;
                    l.strokeWeight = 3;
                    List<Models.latLonPair> pathList = new List<Models.latLonPair>();
                    List<List<Models.latLonPair>> paths = new List<List<Models.latLonPair>>();
                    List<Models.layer> overlay = new List<Models.layer>();
                    foreach (Models.LatLon ll in pd.geoFence)
                    {
                        Models.latLonPair llp = new Models.latLonPair();
                        llp.lat = ll.Lat;
                        llp.lng = ll.Lon;
                        pathList.Add(llp);
                    }
                    paths.Add(pathList);
                    l.paths = paths;
                    overlay.Add(l);
                    p.overlays = overlay;
                    polys.Add(p);
                }
                return JsonConvert.SerializeObject(polys);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// returns a single geoFence object based on the name of the object
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns>JSON Encoded geo Fence object for Geo Fence Designer</returns>
        [OperationContract]
        [WebGet]
        public string getSpecificGeoFenceDesignerObject(string mapName)
        {
            try
            {
                Models.polygonData pdFound = GeoCode.GlobalGeo.polygons.Find(delegate (Models.polygonData find) {
                    return find.polyName == mapName; });
                if (pdFound != null)
                {
                    Models.polygonDesign p = new Models.polygonDesign();
                    p.MapID = pdFound.geoFenceID.ToString();
                    p.zoom = 19;
                    p.tilt = 0;
                    p.mapTypeId = "hybrid";
                    Models.latLonPair llCenter = GeoCode.GlobalGeo.findCentroid(pdFound.geoFence);
                    p.center = llCenter;
                    Models.layer l = new Models.layer();
                    l.type = pdFound.geoType.ToLower();
                    l.title = pdFound.polyName;
                    l.content = pdFound.notes;
                    l.fillColor = "#000000";
                    l.fillOpacity = 0.3;
                    l.strokeColor = "#000000";
                    l.strokeOpacity = 0.9;
                    l.strokeWeight = 3;
                    List<Models.latLonPair> pathList = new List<Models.latLonPair>();
                    List<List<Models.latLonPair>> paths = new List<List<Models.latLonPair>>();
                    List<Models.layer> overlay = new List<Models.layer>();
                    foreach (Models.LatLon ll in pdFound.geoFence)
                    {
                        Models.latLonPair llp = new Models.latLonPair();
                        llp.lat = ll.Lat;
                        llp.lng = ll.Lon;
                        pathList.Add(llp);
                    }
                    paths.Add(pathList);
                    l.paths = paths;
                    overlay.Add(l);
                    p.overlays = overlay;
                    return JsonConvert.SerializeObject(p);
                }
                else
                {
                    return "Couldn't find " + mapName;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// returns the names of all the fences in GeoCode GlobalGeo polygons
        /// </summary>
        /// <returns>List of string</returns>
        [OperationContract]
        [WebGet]
        public string getFenceNames()
        {
            try
            {
                List<string> fences = new List<string>();
                foreach (Models.polygonData pd in GeoCode.GlobalGeo.polygons)
                {
                    fences.Add(pd.polyName);
                }
                return JsonConvert.SerializeObject(fences);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// gets the internal model of the polygons
        /// THIS IS NOT WHAT YOU CALL WHEN YOU WANT
        /// A LIST OF POLYGONS FOR THE GEOFENCE EDITOR
        /// </summary>
        /// <returns>List of polygons, serialized to JSON</returns>
        [OperationContract]
        [WebGet]
        public string getPolygons()
        {
            try
            {
                return JsonConvert.SerializeObject(GeoCode.GlobalGeo.polygons);
            }
            catch (Exception ex)
            {
                return "ERROR: " + ex.ToString();
            }
        }


        [OperationContract]
        [WebGet]
        public string getKMLByPolygon(string polyName)
        {
            string KMLString = string.Empty;

            try
            {
                Models.polygonData found = GeoCode.GlobalGeo.polygons.Find(delegate (Models.polygonData find) {
                    return find.polyName == polyName;
                });
                if (found != null)
                {
                    Placemark placemark = new Placemark();
                    placemark.Name = "<![CDATA[" + found.polyName + "]]>";
                    var document = new Document();
                    
                    Description d = new Description();
                    d.Text = "<![CDATA[" + found.notes + "]]>";
                    placemark.Description = d;
                    Polygon poly = new Polygon();
                    poly.Extrude = false;
                    poly.AltitudeMode = AltitudeMode.RelativeToGround;
                    poly.OuterBoundary = new OuterBoundary();
                    LinearRing lr = new LinearRing();
                    CoordinateCollection cc = new CoordinateCollection();
                    foreach (Models.LatLon ll in found.geoFence)
                    {
                        cc.Add(new Vector(ll.Lat, ll.Lon, ll.Alt));
                    }
                    lr.Coordinates = cc;
                    poly.OuterBoundary.LinearRing = lr;
                    placemark.Geometry = poly;
                    Kml kml = new Kml();
                    document.AddFeature(placemark);
                    kml.Feature = document;
                    Serializer serializer = new Serializer();
                    serializer.Serialize(kml);
                    return serializer.Xml;
                }
                else
                {
                    return "Couldn't find " + polyName;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #region " Tablet Test Functions "

        [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Xml)]
        public speak sayHello() {
            speak s = new speak();
            s.hello = "Hello from WCF";
            return s;
        }

        #endregion

        #region " cell signal "
        [OperationContract]
        [WebGet]
        public string setCellSignal(string MAC, string name, DateTime timestamp, double lat, double lon, int dBm, double quality)
        {
            string success = "OK";
            try
            {
                
                return JsonConvert.SerializeObject(success);
            }
            catch (Exception ex)
            {
                return success;
            }
        }
        #endregion

        #region " Old Code "
        /* UNUSED

[OperationContract]
[WebGet]
public string addJSONPolygon(string jsonPolyData)
{
    try
    {
        string res = "OK";
        object data = JsonConvert.DeserializeObject(jsonPolyData);
        return res;
    }
    catch (Exception ex)
    {
        return "ERROR: " + ex.ToString();
    }
}
*/
        #endregion
    }

    [DataContract(Namespace ="http://tempuri.org")]
    public class speak
    {
        public string hello { get; set; }
    }

    public class userData
    {
        public string userID { get; set; }
        public string password { get; set; }
    }
}

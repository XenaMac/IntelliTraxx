$(function () {
    var map = null;
    var drawingManager = null;
    var selectedShape = null;
    var markers = [];
    var companies = [];
    var companyFilter = [];
    var vehicleClasses = [];
    var vehicleClassFilter = [];
    var drivers = [];
    var driverFilter = [];
    var refreshRate = 10000;
    var defaultZoom = 13;
    var options = {
        classname: 'my-class',
        id: 'my-id',
        bg: '#CECECE'
        //,target: document.getElementById('perspective')
    };
    var nanobar = new Nanobar(options);
    var second = 1;
    var setNanoBarInterval = null;
    var setVehicleinterval = null;
    var triangleCoords = [];
    var shapes = [];
    var currentAlerts = 0;
    var VehicleModalMap = null;
    var VehicleMapLat = null;
    var VehicleMapLng = null;
    var ModalVehicleID = null;
    var slider = null;
    var vehicle = null;
    var DisLocs =[];

    window.initiate = function() {
        $.post("Admin/GetParentCompanyLocation", null, function (data) {
            currentLocation = data;
            $.post("VehicleLocation/getVariables", null, function (data) {
                for (var i = 0; i < data.Result.length; i++) {
                    if (data.Result[i].varName == "TRACKINGREFRESHRATE") {
                        refreshRate = data.Result[i].varVal + "000";
                    }

                    if (data.Result[i].varName == "DEFAULTMAPZOOMRATE") {
                        defaultZoom = data.Result[i].varVal;
                    }
                }
                
                initMap();
            });
        });
    };

    initMap = function () {

        var geocoder = new google.maps.Geocoder();

        geocoder.geocode({ 'address': currentLocation }, function (results, status) {
            if (status === google.maps.GeocoderStatus.OK) {
                map = new google.maps.Map(document.getElementById('map'), {
                    center: results[0].geometry.location,
                    zoom: parseInt(defaultZoom),
                    mapTypeControl: true,
                    mapTypeControlOptions: {
                        style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR,
                        position: google.maps.ControlPosition.RIGHT_TOP
                    },
                    zoomControl: true,
                    zoomControlOptions: {
                        position: google.maps.ControlPosition.RIGHT_TOP
                    },
                    scaleControl: true,
                    streetViewControl: false,
                    fullscreenControl: false
                });

                //set up drawing tools
                drawingManager = new google.maps.drawing.DrawingManager({
                    drawingMode: google.maps.drawing.OverlayType.POLYGON,
                    drawingControl: true,
                    drawingControlOptions: {
                        position: google.maps.ControlPosition.TOP_CENTER,
                        drawingModes: [
                            google.maps.drawing.OverlayType.CIRCLE,
                            google.maps.drawing.OverlayType.POLYGON,
                            google.maps.drawing.OverlayType.RECTANGLE
                        ]
                    },
                    circleOptions: {
                        clickable: true,
                        editable: true,
                        zIndex: 1
                    },
                    polygonOptions: {
                        clickable: true,
                        editable: true,
                        zIndex: 2
                    },
                    polylineOptions: {
                        clickable: true,
                        editable: true,
                        zIndex: 3
                    },
                    rectangleOptions: {
                        clickable: true,
                        editable: true,
                        zIndex: 3
                    }
                });

                //set up event for capturing polygon
                google.maps.event.addListener(drawingManager, 'overlaycomplete', function (event) {
                    if (event.type == google.maps.drawing.OverlayType.POLYGON) {
                        drawingManager.setDrawingMode(null);

                        google.maps.event.addListener(event.overlay, 'click', function (e) {
                            setSelection(event);
                            $('#newPolygonModal').modal('show');
                        });

                        setSelection(event);
                    } else if(event.type == google.maps.drawing.OverlayType.CIRCLE) {
                        drawingManager.setDrawingMode(null);

                        google.maps.event.addListener(event.overlay, 'click', function (e) {
                            setSelection(event);
                            $('#newPolygonModal').modal('show');
                        });

                        setSelection(event);
                    } else if (event.type == google.maps.drawing.OverlayType.RECTANGLE) {
                        drawingManager.setDrawingMode(null);

                        google.maps.event.addListener(event.overlay, 'click', function (e) {
                            setSelection(event);
                            $('#newPolygonModal').modal('show');
                        });

                        setSelection(event);
                    }
                });

                setVehicles();
                setNanoBarInterval = setInterval(setNanoBar, 1000);
                setVehicleinterval = setInterval(setVehicles, parseInt(refreshRate));
            } else {
                alert('Geocode was not successful for the following reason: ' + status);
            }
        });
    }

    function clearSelection() {
        if (selectedShape) {
            selectedShape.setEditable(false);
            selectedShape = null;
        }
    }

    function setSelection(event) {
        clearSelection();
        selectedShape = event.overlay;
        selectedShape.type = event.type;
        selectedShape.setEditable(true);
    }

    function deleteSelectedShape() {
        if (selectedShape) {
            selectedShape.setMap(null);
        }
    }

    //Show or Hide GeoFences
    $('#showHideGF').change(function () {
        if (this.checked) {
            showPolygons();
            $('#showHideGFTxt').text('Hide Geo-Fences');
        } else {
            hidePolygons();
            $('#showHideGFTxt').text('Show Geo-Fences');
        }
    });

    //Turn Drawing On or Off
    $('#drawOnOff').change(function () {
        if (this.checked) {
            drawingManager.setMap(map);
        } else {
            drawingManager.setMap(null);
        }
    });

    Number.prototype.between = function(a, b) {
        var min = Math.min.apply(Math, [a, b]),
          max = Math.max.apply(Math, [a, b]);
        return this >= min && this <= max;
    };

    function showPolygons() {
        shapes = [];

        $.post("VehicleLocation/getAllFences", null, function (data) {
            var shape = null;
            for (var i = 0; i < data.length; i++) {
                
                if (data[i].geoType == "polygon") {                                       //POLYGON
                    triangleCoords = [];
                    for (var c = 0; c <= data[i].geoFence.length - 1; c++) {
                        var Coords = {
                            "lat": data[i].geoFence[c].Lat,
                            "lng": data[i].geoFence[c].Lon
                        }
                        triangleCoords.push(Coords);
                    }

                    shape = new google.maps.Polygon({
                        paths: triangleCoords,
                        strokeColor: '#000000',
                        strokeOpacity: 0.65,
                        strokeWeight: 2,
                        fillColor: '#000000', //getRandomColor() ,
                        fillOpacity: 0.25
                    });

                    google.maps.event.addListener(shape, 'dragend', function (event) {
                        selectedShape = this;
                        this.setEditable(true);
                    });

                } else if (data[i].geoType == "circle") {                           //CIRCLE
                    triangleCoords = [];
                    for (var c = 0; c <= data[i].geoFence.length - 1; c++) {
                        var Coords = {
                            "lat": data[i].geoFence[c].Lat,
                            "lng": data[i].geoFence[c].Lon
                        }
                        triangleCoords.push(Coords);
                    }

                    shape = new google.maps.Circle({
                        strokeColor: '#000000',
                        strokeOpacity: 0.65,
                        strokeWeight: 2,
                        fillColor: '#000000', //getRandomColor() ,
                        fillOpacity: 0.25,
                        center: { lat: triangleCoords[0].lat, lng: triangleCoords[0].lng },
                        radius: data[i].radius
                    });

                    google.maps.event.addListener(shape, 'radius_changed', function () {
                        selectedShape = this;
                    });

                    google.maps.event.addListener(shape, 'center_changed ', function (event) {
                        selectedShape = this;
                    });

                } else if (data[i].geoType == "rectangle") {                                          //RECTANGLE
                    triangleCoords = [];
                    
                    for (var c = 0; c <= data[i].geoFence.length - 1; c++) {
                        var Coords = {
                            "lat": data[i].geoFence[c].Lat,
                            "lng": data[i].geoFence[c].Lon
                        }
                        triangleCoords.push(Coords);
                    }

                    shape = new google.maps.Rectangle({
                        strokeColor: '#000000',
                        strokeOpacity: 0.65,
                        strokeWeight: 2,
                        fillColor: '#000000', //getRandomColor() ,
                        fillOpacity: 0.25,
                        bounds: new google.maps.LatLngBounds(
                           new google.maps.LatLng(triangleCoords[1]),
                           new google.maps.LatLng(triangleCoords[0])
                        )
                    });

                    google.maps.event.addListener(shape, 'bounds_changed', function (event) {
                        selectedShape = this;
                    });

                    google.maps.event.addListener(shape, 'dragend', function (event) {
                        selectedShape = this;
                    });
                }

                var obj = {
                    "type": data[i].geoType,
                    "actionIn": data[i].actionIn,
                    "actionOut": data[i].actionOut,
                    "geoFenceID": data[i].geoFenceID,
                    "notes": data[i].notes,
                    "polyName": data[i].polyName,
                    "actionInEmail": data[i].actionInEmail,
                    "actionOutEmail": data[i].actionOutEmail
                };

                shape.objInfo = obj;

                google.maps.event.addListener(shape, 'rightclick', function () {
                    if (this.editable) {
                        clearSelection();
                        selectedShape = this;
                        this.setEditable(false);
                        this.setDraggable(false);
                        PGModal(this.objInfo);
                    } else {
                        clearSelection();
                        selectedShape = this;
                        this.setEditable(true);
                        this.setDraggable(true);
                    }
                });

                google.maps.event.addListener(shape, 'click', function () {
                    this.setEditable(false);
                    clearSelection();
                    selectedShape = this;
                    PGModal(this.objInfo);  
                });

                shapes.push(shape);

            }

            setPolygonsVisible(true);
        });
    }

    function hidePolygons() {
        setPolygonsVisible(false);
    }

    function setPolygonsVisible(show) {
        for (var i = 0; i <= shapes.length - 1; i++) {
            if (show) {
                shapes[i].setMap(map);
                //polygons[i].setOptions({ visible: true });
            } else {
                shapes[i].setMap(null);
                //polygons[i].setOptions({ visible: false });
            }
        }
    }

    function PGModal(polygonInfo) {
        $('#polyName').val(polygonInfo.polyName);
        $('#polyNotes').text(polygonInfo.notes);
        $('#geoFenceID').val(polygonInfo.geoFenceID);
        $('#polygonModal').modal('show');

        if (polygonInfo.actionIn) {
            $('#polyEnterEnabled').bootstrapToggle('on')
            $('#polyEnterEmail').prop('disabled', false);
            $('#polyEnterEmail').val(polygonInfo.actionInEmail);
        } else {
            $('#polyEnterEnabled').bootstrapToggle('off')
            $('#polyEnterEmail').prop('disabled', true);
            $('#polyEnterEmail').val('');
        }

        if (polygonInfo.actionOut) {
            $('#polyExitEnabled').bootstrapToggle('on')
            $('#polyExitEmail').prop('disabled', false);
            $('#polyExitEmail').val(polygonInfo.actionOutEmail);
        } else {
            $('#polyExitEnabled').bootstrapToggle('off')
            $('#polyExitEmail').prop('disabled', true);
            $('#polyExitEmail').val('');
        }
    }

    function timeConvert(ds) {
        var D, dtime, T, tz, off,
        dobj = ds.match(/(-?\d+)|([+-])|(\d{4})/g);

        T = parseInt(dobj[0], 10);
        tz = dobj[1];
        off = dobj[2];

        if (off) {
            off = (parseInt(off.substring(0, 2), 10) * 3600000) + (parseInt(off.substring(2), 10) * 60000);
            if (tz == '-') off *= -1;
        }
        else off = 0;
        return new Date(T += off).toLocaleTimeString();
    }

    function dateConvert(ds) {
        var D, dtime, T, tz, off,
        dobj = ds.match(/(-?\d+)|([+-])|(\d{4})/g);

        T = parseInt(dobj[0], 10);
        tz = dobj[1];
        off = dobj[2];

        if (off) {
            off = (parseInt(off.substring(0, 2), 10) * 3600000) + (parseInt(off.substring(2), 10) * 60000);
            if (tz == '-') off *= -1;
        }
        else off = 0;
        return new Date(T += off);
    }

    function setNanoBar()
    {
        if(second < (refreshRate/1000)) {
            nanobar.go((100 / (refreshRate / 1000)) * second);
            ++second;
        }
        else {
            second = 1;
            nanobar.go(0);
        }
    }

    function setVehicles() {
        var dir = 0;
        var direction = "";
        var friendlyDirection = "";
        var AlertsText = "";

        //alert("Remove All markers");
        for (i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }

        markers = [];

        //#region getting vehicle locations alert("getAllVehicles");
        $.post("VehicleLocation/getAllVehicles", null, function (data) {
            //Remove if in Company/Class/Driver filter
            for (var i = 0; i < data.Result.length; i++) {
                //run through company filter
                for (var f = 0; f <= companyFilter.length - 1; f++) {
                    if (data.Result[i].extendedData.companyName == companyFilter[i]) {
                        data.Result.splice(data.Result.indexOf(data.Result[i]), 1);
                    }
                }

                //run through class filter
                for (var f = 0; f <= vehicleClassFilter.length - 1; f++) {
                    if (data.Result[i].extendedData.vehicleClass == vehicleClassFilter[i]) {
                        data.Result.splice(data.Result.indexOf(data.Result[i]), 1);
                    }
                }

                //run through class filter
                for (var f = 0; f <= driverFilter.length - 1; f++) {
                    if (data.Result[i].driver.DriverID == driverFilter[i].id) {
                        data.Result.splice(data.Result.indexOf(data.Result[i]), 1);
                    }
                }
            }

            //Paint the map
            for (var i = 0; i < data.Result.length; i++) {

                var alert = false;
                var communicating = true;
                var item = data.Result[i];
                var filter = false;

                //#region Set Filter vars
                addCompany(item.extendedData.companyName);
                addClass(item.extendedData.vehicleClass);
                var driver = {
                    "id": item.driver.DriverID,
                    "name": item.driver.DriverFirstName + " " + item.driver.DriverLastName,
                }
                addDriver(driver);
                //#endregion

                //#region is there truck communication?
                var fiveMinutesLater = new Date();
                fiveMinutesLater.setMinutes(fiveMinutesLater.getMinutes() - 5);
                fiveMinutesLater = fiveMinutesLater.getMinutes();
                var lastMessage = dateConvert(item.lastMessageReceived);
                lastMessage = lastMessage.getMinutes();
                if (lastMessage < fiveMinutesLater) {
                    communicating = false;
                }
                //#endregion

                //#region format truck icon: Is there an alert?
                for (var a = 0; a < item.alerts.length; a++) {
                    if (item.alerts[a].alertActive == true) {
                        alert = true;
                        AlertsText += "<strong><span style=\"color: red;\">Active Alert: </span></strong>" + item.alerts[a].alertName + " | " + timeConvert(item.alerts[a].alertStart) + "<br />";
                    }
                }
                //#endregion

                //#region format truck icon: What direction
                dir = item.gps.dir;
                if (dir.between(0, 10)) {
                    direction = "north";
                    friendlyDirection = "North";
                }
                else if (dir.between(11, 60)) {
                    direction = "north_east";
                    friendlyDirection = "North/East";
                }
                else if (dir.between(61, 100)) {
                    direction = "east";
                    friendlyDirection = "East"
                }
                else if (dir.between(101, 165)) {
                    direction = "south_east";
                    friendlyDirection = "South/East";
                }
                else if (dir.between(166, 195)) {
                    direction = "south";
                    friendlyDirection = "South";
                }
                else if (dir.between(196, 255)) {
                    direction = "south_west";
                    friendlyDirection = "South/West";
                }
                else if (dir.between(256, 295)) {
                    direction = "west";
                    friendlyDirection = "West";
                }
                else if (dir.between(296, 355)) {
                    direction = "north_west";
                    friendlyDirection = "North/West";
                }
                else if (dir.between(356, 360)) {
                    direction = "north";
                    friendlyDirection = "North";
                }

                var truckIcon = 'Images/Vicons/';

                if (alert) {
                    truckIcon += "alert_";
                }
                else if (!communicating) {
                    truckIcon += "null_";
                }
                else {
                    truckIcon += "vehicle_";
                }

                truckIcon += direction + '.png';
                //#endregion

                //#region create vehicle marker
                var vehicle = new google.maps.Marker({
                    position: { lat: item.gps.lat, lng: item.gps.lon },
                    title: item.extendedData.VehicleFriendlyName + "  (" + item.VehicleID + ")",
                    icon: truckIcon,
                    scale: 2,
                    draggable: false,
                    map: map
                });
                //#endregion

                //#region create infowindow for individual vehicle marker

                google.maps.event.addListener(vehicle, 'click', (function () {
                    return function () {
                        clearInterval(setNanoBarInterval);
                        clearInterval(setVehicleinterval);
                        this.setAnimation(google.maps.Animation.BOUNCE);
                        var regExp = /\(([^)]+)\)/;
                        var matches = regExp.exec(this.getTitle());
                        VDModal(matches[1]);
                    };
                })());
                //#endregion

                //add to marker array
                markers.push(vehicle);
            }

            //#region create filters in side menu
            if (companies.length == 1) {
                $('#companyFilterDivHeader').hide();
            } else {
                $('#companyFilterDiv').empty();
                for (var i = 0; i <= companies.length - 1; i++) {
                    var checked = "checked";
                    var ccb = companies[i];
                    if(!ccb.checked) { checked = ""; }
                    $('#companyFilterDiv').append('<input type="checkbox" value="' + ccb.company + '" class="company"' + checked + '/> ' + ccb.company + "<br />");
                }
            }

            if (vehicleClasses.length == 1) {
                $('#classFilterDivHeader').hide();
            } else {
                $('#classFilterDiv').empty();
                for (var i = 0; i <= vehicleClasses.length - 1; i++) {
                    var checked = "checked";
                    var vccb = vehicleClasses[i];
                    if (!vccb.checked) { checked = ""; }
                    $('#classFilterDiv').append('<input type="checkbox" value="' + vccb.class + '" class="vClass"' + checked + '/> ' + vccb.class + "<br />");
                }
            }

            //TODO change 0 to 1
            if (drivers.length == 0) {
                $('#driverFilterDivHeader').hide();
            } else {
                $('#driverFilterDiv').empty();
                for (var i = 0; i <= drivers.length - 1; i++) {
                    var checked = "checked";
                    var dr = drivers[i];
                    if (!dr.checked) { checked = ""; }
                    $('#driverFilterDiv').append('<input type="checkbox" value="' + dr.id + '" class="driver"' + checked + '/> ' + dr.name + "<br />");
                }
            }
            //#endregion
        });
        //#endregion
    }

    function VDModal(vehicleID) {
        //alert(vehicleID);
        $.post("VehicleLocation/GetVehicleAndDriver?vehicleID=" + vehicleID, null, function (data) {

            //Vehicle Modal
            if (data.Vehicle.vehicleClassImage != null) {
                $('#vImage').attr('src', "Images/types/" + data.Vehicle.vehicleClassImage);
            }
            $('#vehicleID').text(data.Vehicle.VehicleFriendlyName);
            
            var Desc = "This vehicle is a " + data.Vehicle.Year + ", " + data.Vehicle.Make + "-" + data.Vehicle.Model + ". Plate Number: " + data.Vehicle.licensePlate + ", has a Haul Limit of: " + data.Vehicle.haulLimit + "/lbs and has ID Number: " + data.Vehicle.vehicleID + ".";
            $('#vehicleDesc').html(Desc);

            if (data.Driver.ProfilePic != null) {
                $('#dImage').attr('src', "data:image/jpg;base64," + data.DriverImage64String);
            } else {
                $('#dImage').attr('src', "Images/drivers/tdriver.png");
            }
            
            $('#dName').text(data.Driver.DriverFirstName + " " + data.Driver.DriverLastName);
            $('#DriverNumber').text(data.Driver.DriverNumber);
            $('#DriverEmail').text(data.Driver.DriverEmail);
            $('#CurrentStatus').text(data.Driver.currentStatus);
            $('#HOS').text("IN");
            $('#Notes').text("No Notes in system.");
            
            //Alerts
            $('#lbl_ActiveAlerts').html('<br />');

            if (data.Alerts.length > 0) {
                for (var i = data.Alerts.length - 1; i >= 0; i--) {
                    if (data.Alerts[i].alertActive == true) {
                        if (data.Alerts[i].alertName == "SPEEDING") {
                            $('#lbl_ActiveAlerts').append('<div class=\"media\"><div class=\"media-left\"><a href=\"../Alerts/ViewAlert?alertID=' + data.Alerts[i].alertID + '\" style=\"color: red;\"><img class=\"media-object\" src=\"Images/speeding_icon.png\" /></div><div class=\"media-body\"><h4 class=\"media-heading\">Speeding</h4></a>Alert Started: <strong>' + new Date(parseInt(data.Alerts[i].alertStart.substr(6))).toLocaleTimeString() + '</strong>. Alert Ended: <strong>' + new Date(parseInt(data.Alerts[i].alertEnd.substr(6))).toLocaleTimeString() + '</strong>. Lat/Lon Started: <strong>' + data.Alerts[i].latLonStart + '</strong>. Lat/Lon Ended. <strong>' + data.Alerts[i].latLonEnd + '</strong></div></div><hr>');
                        } else {
                            $('#lbl_ActiveAlerts').append('<div class=\"media\"><div class=\"media-left\"><a href=\"../Alerts/ViewAlert?alertID=' + data.Alerts[i].alertID + '\" style=\"color: red;\"><img class=\"media-object\" src=\"Images/geofence_icon.png\" /></div><div class=\"media-body\"><h4 class=\"media-heading\">GEO Fence Alert</h4></a>Alert Started: <strong>' + new Date(parseInt(data.Alerts[i].alertStart.substr(6))).toLocaleTimeString() + '</strong>. Alert Ended: <strong>' + new Date(parseInt(data.Alerts[i].alertEnd.substr(6))).toLocaleTimeString() + '</strong>. Lat/Lon Started: <strong>' + data.Alerts[i].latLonStart + '</strong>. Lat/Lon Ended. <strong>' + data.Alerts[i].latLonEnd + '</strong></div></div><hr>');
                        }
                    } else {
                        if (data.Alerts[i].alertName == "SPEEDING") {
                            $('#lbl_ActiveAlerts').append('<div class=\"media\"><div class=\"media-left\"><a href=\"../Alerts/ViewAlert?alertID=' + data.Alerts[i].alertID + '\"><img class=\"media-object\" src=\"Images/speeding_icon.png\" /></div><div class=\"media-body\"><h4 class=\"media-heading\">' + data.Alerts[i].alertName + '</h4></a>Alert Started: <strong>' + new Date(parseInt(data.Alerts[i].alertStart.substr(6))).toLocaleTimeString() + '</strong>. Alert Ended: <strong>' + new Date(parseInt(data.Alerts[i].alertEnd.substr(6))).toLocaleTimeString() + '</strong>. Lat/Lon Started: <strong>' + data.Alerts[i].latLonStart + '</strong>. Lat/Lon Ended. <strong>' + data.Alerts[i].latLonEnd + '</strong></div></div><hr>');
                        } else {
                            $('#lbl_ActiveAlerts').append('<div class=\"media\"><div class=\"media-left\"><a href=\"../Alerts/ViewAlert?alertID=' + data.Alerts[i].alertID + '\"><img class=\"media-object\" src=\"Images/geofence_icon.png\" /></div><div class=\"media-body\"><h4 class=\"media-heading\">' + data.Alerts[i].alertName + '</h4></a>Alert Started: <strong>' + new Date(parseInt(data.Alerts[i].alertStart.substr(6))).toLocaleTimeString() + '</strong>. Alert Ended: <strong>' + new Date(parseInt(data.Alerts[i].alertEnd.substr(6))).toLocaleTimeString() + '</strong>. Lat/Lon Started: <strong>' + data.Alerts[i].latLonStart + '</strong>. Lat/Lon Ended. <strong>' + data.Alerts[i].latLonEnd + '</strong></div></div><hr>');
                        }
                    }
                }
            }
            else if (data.Alerts.length == 0) {
                $('#lbl_ActiveAlerts').html('No alerts');
            }

            currentAlerts = 0;

            //Locations
            var date = new Date(parseInt(data.lastMessageReceived.substr(6)));
            $('#LC').text(date.toLocaleDateString() + " @ " + date.toLocaleTimeString());
            $('#Lat').text(data.gps.lat);
            $('#Long').text(data.gps.lon);
            $('#Dir').text(data.gps.dir);
            $('#SPD').text(data.gps.spd);

            //initiate Loc-Map
            var mapDiv = document.getElementById('LocMap');
            VehicleModalMap = new google.maps.Map(mapDiv, {
                center: { lat: data.gps.lat, lng: data.gps.lon },
                zoom: 15,
                mapTypeControl: false,
                mapTypeControlOptions: {
                    position: google.maps.ControlPosition.RIGHT_TOP
                },
                zoomControl: true,
                zoomControlOptions: {
                    position: google.maps.ControlPosition.RIGHT_TOP
                },
                scaleControl: true,
                streetViewControl: false,
                fullscreenControl: false
            });

            //Telematics
            $('#telemtryLR').text(date.toLocaleDateString() + " @ " + date.toLocaleTimeString())

            //set VehicleMap
            VehicleMapLat = data.gps.lat;
            VehicleMapLng = data.gps.lon;
            ModalVehicleID = data.Vehicle.vehicleID;

            //Tracker        
            $.post("VehicleLocation/GetLastTwoHours?vehicleID=" + ModalVehicleID, null, function (data) {
                if (data) {

                    //set text lables bounds
                    $('#ST').text(new Date(parseInt(data[0].timestamp.substr(6))).toLocaleTimeString());
                    $('#ET').text(new Date(parseInt(data[data.length - 1].timestamp.substr(6))).toLocaleTimeString());

                    //vehicle marker
                    vehicle = new google.maps.Marker({
                        position: { lat: data[data.length - 1].Lat, lng: data[data.length - 1].Lon },
                        title: ModalVehicleID,
                        draggable: false,
                        map: VehicleModalMap
                    });

                    //slider
                    $('#ex1Slider').slider({
                        animate: "slow",
                        min: 1,
                        max: data.length,
                        step: 1,
                        orientation: "horizontal",
                        value: data.length,
                        slide: function (event, slideEvt) {
                            vehicle.setMap(null);
                            var index = slideEvt.value;
                            //Set values based on slider
                            var date = new Date(parseInt(data[index].timestamp.substr(6)));
                            $('#LC').text(date.toLocaleDateString() + " @ " + date.toLocaleTimeString());
                            $('#Lat').text(data[index].Lat);
                            $('#Long').text(data[index].Lon);
                            $('#Dir').text(data[index].Direction);
                            $('#SPD').text(data[index].Speed);

                            //center map
                            VehicleModalMap.setCenter({ lat: data[index].Lat, lng: data[index].Lon });

                            //add marker
                            vehicle = new google.maps.Marker({
                                position: { lat: data[index].Lat, lng: data[index].Lon },
                                title: ModalVehicleID,
                                draggable: false,
                                map: VehicleModalMap
                            });
                        }
                    });

                    $('.sliderDiv').show();
                }
            });

            //Set Dispatch
            $('#lblDispatch').text('Dispatch ' + $('#vehicleID').text() + ' to ...');
            $('#ModalAddress').val($('#Address').val());
            $('#ModalCity').val($('#City').val());
            $('#ModalState').val($('#State').val());
            $('#ModalZip').val($('#Zip').val());
            $('#vehicleModal').modal('show');
        });
    }

    $('#vehicleModal').on('hide.bs.modal', function (e) {
        second = 1;
        nanobar.go(0);
        setVehicles();
        VehicleModalMap = null;
        setNanoBarInterval = setInterval(setNanoBar, 1000);
        setVehicleinterval = setInterval(setVehicles, parseInt(refreshRate));
    });

    $('#vehicleModal').on('shown.bs.modal', function (e) {
        google.maps.event.trigger(VehicleModalMap, "resize");
        VehicleModalMap.setCenter(new google.maps.LatLng(VehicleMapLat, VehicleMapLng));
    });

    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        google.maps.event.trigger(VehicleModalMap, "resize");
        VehicleModalMap.setCenter(new google.maps.LatLng(VehicleMapLat, VehicleMapLng));
    });

    function addCompany(companyName) {
        var found = false;
        for(var i = 0; i <= companies.length - 1; i++) {
            var ccb = companies[i];
            if(ccb.company == companyName)
            {
                found = true;
            }
        }

        if (!found) {
            var ccb = {
                "company": companyName,
                "checked": true
            };
            
            companies.push(ccb);
        }
    }

    function addClass(className) {
        var found = false;
        for (var i = 0; i <= vehicleClasses.length - 1; i++) {
            var vccb = vehicleClasses[i];
            if (vccb.class == className) {
                found = true;
            }
        }

        if (!found) {
            var vccb = {
                "class": className,
                "checked": true
            };

            vehicleClasses.push(vccb);
        }
    }

    function addDriver(driver) {
        var found = false;
        for (var i = 0; i <= drivers.length - 1; i++) {
            var dr = drivers[i];
            if (dr.id == driver.id) {
                found = true;
            }
        }

        if (!found) {
            var dr = {
                "id": driver.id,
                "name": driver.name,
                "checked": true
            };

            drivers.push(dr);
        }
    }

    //filter checkbox checked or unchecked
    $(document).on('change', '[type=checkbox]', function() {
        //if checkbox was company
        if (!$(this).is(":checked") && $(this).hasClass("company")) {
            //alert('you unchecked company checkbox ' + $(this).val()) 

            //change company in companies array to checked = false
            var companyName = $(this).val();
            for (var c = 0; c <= companies.length - 1; c++) {
                if (companies[c].company == companyName)
                {
                    companies[c].checked = false;
                }
            }

            //add to filter
            companyFilter.push(companyName);

        } else {
            //alert('you checked company checkbox ' + $(this).val())

            //change company in companies array to checked = true
            var companyName = $(this).val();
            for (var c = 0; c <= companies.length - 1; c++) {
                if (companies[c].company == companyName) {
                    companies[c].checked = true;
                }
            }

            //remove from filter
            var i = companyFilter.indexOf(companyName);
            if (i != -1) {
                companyFilter.splice(i, 1);
            }
        }

        //if checkbox was class
        if (!$(this).is(":checked") && $(this).hasClass("vClass")) {
            //alert('you unchecked class checkbox ' + $(this).val()) 

            //change class in vehicleClasses array to checked = false
            var className = $(this).val();
            for (var cl = 0; cl <= vehicleClasses.length - 1; cl++) {
                if (vehicleClasses[cl].class == className) {
                    vehicleClasses[cl].checked = false;
                }
            }

            //add to filter
            vehicleClassFilter.push(className);

        } else {
            //alert('you checked class checkbox ' + $(this).val())

            //change class in vehicleClasses array to checked = true
            var className = $(this).val();
            for (var cl = 0; cl <= vehicleClasses.length - 1; cl++) {
                if (vehicleClasses[cl].class == className) {
                    vehicleClasses[cl].checked = true;
                }
            }

            //remove from filter
            var i = vehicleClassFilter.indexOf(className);
            if (i != -1) {
                vehicleClassFilter.splice(i, 1);
            }
        }

        //if checkbox was driver
        if (!$(this).is(":checked") && $(this).hasClass("driver")) {
            //alert('you unchecked criver checkbox ' + $(this).val()) 

            //change class in vehicleClasses array to checked = false
            var id = $(this).val();
            var name = $(this).text();

            for (var dr = 0; dr <= drivers.length - 1; dr++) {
                if (vehicleClasses[dr].id == id) {
                    vehicleClasses[dr].checked = false;
                }
            }

            //add to filter
            driverFilter.push(id);

        } else {
            //alert('you checked class checkbox ' + $(this).val())

            //change class in vehicleClasses array to checked = true
            var id = $(this).val();
            var name = $(this).text();

            for (var dr = 0; dr <= drivers.length - 1; dr++) {
                if (vehicleClasses[dr].id == id) {
                    vehicleClasses[dr].checked = true;
                }
            }

            //remove from filter
            var i = driverFilter.indexOf(id);
            if (i != -1) {
                driverFilter.splice(i, 1);
            }
        }
    });

    function getRandomColor() {
        var letters = '0123456789ABCDEF'.split('');
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

    $('#vdClose').click(function () {
        second = 1;
        nanobar.go(0);
        setVehicles();
        setNanoBarInterval = setInterval(setNanoBar, 1000);
        setVehicleinterval = setInterval(setVehicles, parseInt(refreshRate));
    });

    $('#deletePoly').click(function () {
        deleteSelectedShape();
    });

    $('#deletePolygon').click(function () {
        if (confirm("Deleting will remove this GEO Fence from the system entirley. Are you sure?")) {
            $.post("VehicleLocation/DeleteFence?id=" + $('#geoFenceID').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Deleted.");
                    $('#polygonModal').modal('hide');
                    deleteSelectedShape();
                    hidePolygons();
                    showPolygons();
                } else {
                    toastr.warning("Delete Aborted. There was an issue with deleting this GEO Fence:" + data);
                }
            });
        }
        
    });

    $('#savePoly').click(function () {

        var enterAlert = $('#EnterEnabled').is(':checked');
        var exitAlert = $('#ExitEnabled').is(':checked');

        if (selectedShape.type == google.maps.drawing.OverlayType.POLYGON) {
            var vertices = selectedShape.getPath();
            var coordinates = [];

            for (var i = 0; i < vertices.getLength() ; i++) {
                var xy = vertices.getAt(i);
                coordinates.push(xy.lat() + "^" + xy.lng());
            }

            $.post("VehicleLocation/AddFence?type=" + selectedShape.type + "&polyName=" + $('#polyName_C').val() + "&notes=" + $('#polyNotes_C').val() + "&geofenceID=00000000-0000-0000-0000-000000000000&geoFence=" + coordinates + "&radius=0&actionOut=" + exitAlert + "&actionOutEmail=" + $('#ExitEmail').val() + "&actionIn=" + enterAlert + "&actionInEmail=" + $('#EnterEmail').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    $('#newPolygonModal').modal('hide');
                    deleteSelectedShape();
                    $('#showHideGF').click();
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        } else if (selectedShape.type == google.maps.drawing.OverlayType.CIRCLE) {
            var radius = selectedShape.getRadius();
            var cLat = selectedShape.getCenter().lat();
            var cLng = selectedShape.getCenter().lng();

            $.post("VehicleLocation/AddFence?type=" + selectedShape.type + "&polyName=" + $('#polyName_C').val() + "&notes=" + $('#polyNotes_C').val() + "&geofenceID=00000000-0000-0000-0000-000000000000&geoFence=" + cLat + "^" + cLng + "&radius=" + radius + "&actionOut=" + exitAlert + "&actionOutEmail=" + $('#ExitEmail').val() + "&actionIn=" + enterAlert + "&actionInEmail=" + $('#EnterEmail').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");;
                    $('#newPolygonModal').modal('hide');
                    deleteSelectedShape();
                    $('#showHideGF').click();
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        } else if (selectedShape.type == google.maps.drawing.OverlayType.RECTANGLE) {
            var ne = selectedShape.getBounds().getNorthEast();
            var sw = selectedShape.getBounds().getSouthWest();
            var coordinates = [];
            coordinates.push(ne.lat() + "^" + ne.lng());
            coordinates.push(sw.lat() + "^" + sw.lng());

            $.post("VehicleLocation/AddFence?type=" + selectedShape.type + "&polyName=" + $('#polyName_C').val() + "&notes=" + $('#polyNotes_C').val() + "&geofenceID=00000000-0000-0000-0000-000000000000&geoFence=" + coordinates + "&radius=0&actionOut=" + exitAlert + "&actionOutEmail=" + $('#ExitEmail').val() + "&actionIn=" + enterAlert + "&actionInEmail=" + $('#EnterEmail').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    $('#newPolygonModal').modal('hide');
                    deleteSelectedShape();
                    $('#showHideGF').click();
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        }

        $('#polyName_C').val(polygonInfo.polyName);
        $('#polyNotes_C').text(polygonInfo.notes);
        $('#EnterEnabled').bootstrapToggle('on')
        $('#EnterEmail').prop('disabled', false);
        $('#EnterEmail').val('');
        $('#ExitEnabled').bootstrapToggle('off')
        $('#ExitEmail').prop('disabled', true);
        $('#ExitEmail').val('');
    });

    $('#saveEditedPoly').click(function () {
        if (selectedShape.objInfo.type == "polygon") {
            var vertices = selectedShape.getPath();
            var coordinates = [];

            for (var i = 0; i < vertices.getLength() ; i++) {
                var xy = vertices.getAt(i);
                coordinates.push(xy.lat() + "^" + xy.lng());
            }

            $.post("VehicleLocation/AddFence?type=" + selectedShape.objInfo.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=" + $('#geoFenceID').val() + "&geoFence=" + coordinates + "&radius=0&actionOut=" + $('#polyNoteOut').val() + "&actionIn=" + $('#polyNoteIn').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    $('#polygonModal').modal('hide');
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        } else if (selectedShape.objInfo.type == "circle") {
            var radius = selectedShape.getRadius();
            var cLat = selectedShape.getCenter().lat();
            var cLng = selectedShape.getCenter().lng();

            $.post("VehicleLocation/AddFence?type=" + selectedShape.objInfo.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=" + $('#geoFenceID').val() + "&geoFence=" + cLat + "^" + cLng + "&radius=" + radius + "&actionOut=" + $('#polyNoteOut').val() + "&actionIn=" + $('#polyNoteIn').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    $('#polygonModal').modal('hide');
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        } else if (selectedShape.objInfo.type == "rectangle") {
            var ne = selectedShape.getBounds().getNorthEast();
            var sw = selectedShape.getBounds().getSouthWest();
            var coordinates = [];
            coordinates.push(ne.lat() + "^" + ne.lng());
            coordinates.push(sw.lat() + "^" + sw.lng());

            $.post("VehicleLocation/AddFence?type=" + selectedShape.objInfo.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=" + $('#geoFenceID').val() + "&geoFence=" + coordinates + "&radius=0&actionOut=" + $('#polyNoteOut').val() + "&actionIn=" + $('#polyNoteIn').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    $('#polygonModal').modal('hide');
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        }
    });

    $('#EnterEnabled').change(function () {
        $('#EnterEnabled').toggle(this.checked);
        $('#EnterEmail').prop('disabled', function (i, v) { return !v; });
    });

    $('#ExitEnabled').change(function () {
        $('#ExitEnabled').toggle(this.checked);
        $('#ExitEmail').prop('disabled', function (r, t) { return !t; });
    });

    $('#polyEnterEnabled').change(function () {
        $('#polyEnterEnabled').toggle(this.checked);
        $('#polyEnterEmail').prop('disabled', function (i, v) { return !v; });
        $('#polyEnterEmail').val('');
    });

    $('#polyExitEnabled').change(function () {
        $('#polyExitEnabled').toggle(this.checked);
        $('#polyExitEmail').prop('disabled', function (r, t) { return !t; });
        $('#polyExitEmail').val('');
    });

    $('#SubmitDispatch').click(function () {
        if ($('#Address').val() != "" && $('#State').val() != "" && $('#City').val() != "" && $('#Zip').val() != "")
        {
            for (var i = 0; i < DisLocs.length; i++) {
                DisLocs[i].setMap(null);
            }
            DisLocs = [];

            var geocoder = new google.maps.Geocoder();
            var DisPatchLocation = $('#Address').val() + ", " + $('#City').val() + " " + $('#State').val() + " " + $('#Zip').val();

            geocoder.geocode({ 'address': DisPatchLocation }, function (results, status) {
                var DisLocation = new google.maps.Marker({
                    position: results[0].geometry.location,
                    title: DisPatchLocation,
                    draggable: false,
                    map: map
                });
                DisLocation.setIcon('http://maps.google.com/mapfiles/ms/icons/blue-dot.png')
                DisLocation.setAnimation(google.maps.Animation.DROP);
                DisLocs.push(DisLocation);
                for (var i = 0; i < DisLocs.length; i++) {
                    map.setCenter(results[0].geometry.location);
                    DisLocs[i].setMap(map);
                }
            });            
        } else {
            alert("Address, State and Zip are required");
        }      
    });

    $('#Go_Dispatch').click(function () {
        if ($('#ModalAddress').val() != "" && $('#ModalState').val() != "" && $('#ModalCity').val() != "" && $('#ModalZip').val() != "") {
            $.post("Dispatch/DispatchVehicle?address=" + $('#ModalAddress').val() + "&city=" + $('#ModalCity').val() + "&state=" + $('#ModalState').val() + "&vehicleID=" + ModalVehicleID + "&zip=" + $('#ModalZip').val() + "&note=" + $('#ModalNotes').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success(ModalVehicleID + " has been dispatched");
                    $('#vehicleModal').modal('hide');
                } else {
                    toastr.warning("Dispatch Aborted. There was an issue with dispatching this vehicle:" + data);
                }
            });
        } else {
            alert("Address, State and Zip are required");
        }
    });
});
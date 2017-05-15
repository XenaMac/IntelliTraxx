$(function () {
    //Variables
    var map = null;
    var centerMap = null;
    var drawingManager = null;
    var currentLocation = null;
    var defaultZoom = null;
    var vehicles = [];
    var selectedVehicle = null;
    var shapes = [];
    var selectedShape = null;
    var triangleCoords = [];
    var historyMap = null;
    var historyData = [];
    var drivePath = null;
    var drivePathCoordinates = [];
    var historyVehicle = null;
    var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var searchDate = new Date();
    var getHistorical = false;
    var polysVisible = false;
    var currentD2V = 0;

    $('[data-toggle="tooltip"]').tooltip()

    //#region REMOVE FOR PROD ROLL!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    $('#startSim').click(function () {
        window.open('http://38.124.164.213:9098/Index.aspx', 'IntellliTraxx Vehicle Simulator', '');
        return false;
    });
    //---------------------------------------------------

    initiate();

    //#region Vehicle class
    function Vehicle(data, selected) {
        this.ID = data.extendedData.ID
        this.VehicleID = data.VehicleID;
        this.Name = data.extendedData.VehicleFriendlyName;
        if (data.status[0] != null && data.status[0].statusName == "Active") {
            if (data.status[0].statusVal == "Inactive")
                this.status = "Inactive";
        } else {
            this.status = "Active";
        }
        this.lat = data.gps.lat;
        this.lon = data.gps.lon;
        this.dir = data.gps.dir;
        this.spd = data.gps.spd;
        this.selected = false;
        this.behaviors = data.behaviors;
        this.driver = data.driver;
        this.lastMessageReceived = data.lastMessageReceived;
        this.ABI = data.ABI;

        for (var a = 0; a < data.alerts.length; a++) {
            if (data.alerts[a].alertActive == true) {
                this.status = "InAlert";
            }
        }

        var _this = this;

        if (this.status == "InAlert") {
            this.Marker = new SlidingMarker({
                icon: {
                    url: '../Content/Images/red.png', // url
                    scaledSize: new google.maps.Size(32, 50), // scaled size
                    origin: new google.maps.Point(0, 0), // origin
                    anchor: new google.maps.Point(16, 50) // anchor
                },
                position: { lat: this.lat, lng: this.lon },
                map: map,
                title: this.Name,
                duration: this.ABI * 1000,
                easing: "linear"
            });
            mapLogEntry("alert", _this);
        } else if (this.status == "Inactive") {
            this.Marker = new SlidingMarker({
                icon: {
                    url: '../Content/Images/grey.png', // url
                    scaledSize: new google.maps.Size(32, 50), // scaled size
                    origin: new google.maps.Point(0, 0), // origin
                    anchor: new google.maps.Point(16, 50) // anchor
                },
                position: { lat: this.lat, lng: this.lon },
                map: map,
                title: this.Name,
                duration: this.ABI * 1000,
                easing: "linear"
            });
            mapLogEntry("alert", _this);
        } else {
            this.Marker = new SlidingMarker({
                icon: {
                    url: '../Content/Images/blue.png', // url
                    scaledSize: new google.maps.Size(32, 50), // scaled size
                    origin: new google.maps.Point(0, 0), // origin
                    anchor: new google.maps.Point(16, 50) // anchor
                },
                position: { lat: this.lat, lng: this.lon },
                map: map,
                title: this.Name + " (" + this.spd + ")",
                duration: this.ABI * 1000,
                easing: "linear"
            });
        }

        this.Marker.addListener('click', function () {
            selectThisVehicle(_this, selectedVehicle);
            map.setZoom(15);
            map.setCenter(this.getPosition());
        });

        mapLogEntry("position", this);

        if (this.status != "Inactive") {

            setInterval(function () {
                $.post("getGPS?id=" + _this.ID, null, function (data) {
                    if (data.length != 0) {
                        _this.status = data[0].status;
                        _this.lat = data[0].Lat;
                        _this.lon = data[0].Lon;
                        _this.dir = data[0].Direction;
                        _this.spd = data[0].Speed;

                        if (selectedVehicle == null) {
                            if (_this.status == "InAlert") {
                                _this.Marker.setIcon({
                                    url: '../Content/Images/red.png', // url
                                    scaledSize: new google.maps.Size(32, 50), // scaled size
                                    origin: new google.maps.Point(0, 0), // origin
                                    anchor: new google.maps.Point(16, 50) // anchor
                                });
                                $('#' + _this.ID).html('');
                                $('#' + _this.ID).html(_this.Name + '&nbsp;&nbsp;<span id=\'alerts\' class=\'glyphicons glyphicons-alert\' style=\'color: red;\'></span>');
                                mapLogEntry("alert", _this);
                            } else {
                                _this.Marker.setIcon({
                                    url: '../Content/Images/blue.png', // url
                                    scaledSize: new google.maps.Size(32, 50), // scaled size
                                    origin: new google.maps.Point(0, 0), // origin
                                    anchor: new google.maps.Point(16, 50) // anchor
                                });
                                $('#' + _this.ID).html('');
                                $('#' + _this.ID).html(_this.Name);
                            }
                        }

                        _this.Marker.setPosition({ lat: _this.lat, lng: _this.lon });
                        if (_this.selected == true) {
                            map.setCenter(_this.Marker.getPosition());
                        }
                        mapLogEntry("position", _this);
                    }
                });
            }, this.ABI * 1000);
        }


        this.modifyIcon = function (icon) {
            if (icon == "normal") {
                _this.Marker.setIcon({
                    url: '../Content/Images/blue.png', // url
                    scaledSize: new google.maps.Size(32, 50), // scaled size
                    origin: new google.maps.Point(0, 0), // origin
                    anchor: new google.maps.Point(16, 50) // anchor
                });
                _this.Marker.setAnimation(null);
                _this.selected = false;
            } else if (icon == "Inactive") {
                _this.Marker.setIcon({
                    url: '../Content/Images/grey.png', // url
                    scaledSize: new google.maps.Size(32, 50), // scaled size
                    origin: new google.maps.Point(0, 0), // origin
                    anchor: new google.maps.Point(16, 50) // anchor
                });
                _this.Marker.setAnimation(null);
                _this.selected = false;
            } else if (icon == "InAlert") {
                _this.Marker.setIcon({
                    url: '../Content/Images/red.png', // url
                    scaledSize: new google.maps.Size(32, 50), // scaled size
                    origin: new google.maps.Point(0, 0), // origin
                    anchor: new google.maps.Point(16, 50) // anchor
                });
                _this.Marker.setAnimation(null);
                _this.selected = false;
            } else if (icon == "selected") {
                _this.Marker.setIcon({
                    url: '../Content/Images/green.png', // url
                    scaledSize: new google.maps.Size(32, 50), // scaled size
                    origin: new google.maps.Point(0, 0), // origin
                    anchor: new google.maps.Point(16, 50) // anchor
                });
                _this.Marker.setAnimation(null);
                map.setCenter(_this.Marker.getPosition());
                _this.selected = true;
            }
        };
    }
    //#endregion

    function initiate() {
        $.post("GetParentCompanyLocation", null, function (data) {
            currentLocation = data;
            $.post("getVariables", null, function (data) {
                for (var i = 0; i < data.Result.length; i++) {
                    if (data.Result[i].varName == "DEFAULTMAPZOOMRATE") {
                        defaultZoom = data.Result[i].varVal;
                    }
                }

                $.getScript("https://cdnjs.cloudflare.com/ajax/libs/marker-animate-unobtrusive/0.2.8/vendor/markerAnimate.js", function (data, textStatus, jqxhr) {
                    $.getScript("https://cdnjs.cloudflare.com/ajax/libs/marker-animate-unobtrusive/0.2.8/SlidingMarker.min.js", function (data, textStatus, jqxhr) {
                        initMap();
                    });
                });

            });
        });
    };

    initMap = function () {
        var geocoder = new google.maps.Geocoder();

        geocoder.geocode({ 'address': currentLocation }, function (results, status) {
            if (status === google.maps.GeocoderStatus.OK) {
                centerMap = results[0].geometry.location;
                map = new google.maps.Map(document.getElementById('map'), {
                    center: results[0].geometry.location,
                    zoom: parseInt(defaultZoom),
                    mapTypeControl: true,
                    mapTypeControlOptions: {
                        style: google.maps.MapTypeControlStyle.DROPDOWN_MENU,
                        position: google.maps.ControlPosition.TOP_LEFT
                    },
                    zoomControl: true,
                    zoomControlOptions: {
                        position: google.maps.ControlPosition.TOP_LEFT
                    },
                    scaleControl: true,
                    streetViewControl: false,
                    fullscreenControl: false
                });

                //set up event for center changed
                google.maps.event.addListener(map, 'zoom_changed', function () {
                    //alert(map.getZoom());
                    //getVehicles();
                });

                //set up drawing tools
                drawingManager = new google.maps.drawing.DrawingManager({
                    drawingMode: google.maps.drawing.OverlayType.HAND,
                    drawingControl: true,
                    drawingControlOptions: {
                        position: google.maps.ControlPosition.TOP_LEFT,
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
                            clearSelection()
                            setSelection(event);
                            newPolygon()
                        });

                        setSelection(event);
                    } else if (event.type == google.maps.drawing.OverlayType.CIRCLE) {
                        drawingManager.setDrawingMode(null);

                        google.maps.event.addListener(event.overlay, 'click', function (e) {
                            clearSelection()
                            setSelection(event);
                            newPolygon()
                        });

                        setSelection(event);
                    } else if (event.type == google.maps.drawing.OverlayType.RECTANGLE) {
                        drawingManager.setDrawingMode(null);

                        google.maps.event.addListener(event.overlay, 'click', function (e) {
                            clearSelection()
                            setSelection(event);
                            newPolygon()
                        });

                        setSelection(event);
                    }
                });

                getVehicles(true);
                setInterval(function () {
                    getVehicles(false);
                }, 60000);
            } else {
                alert('Geocode was not successful for the following reason: ' + status);
            }
        });
    }

    function getVehicles(reload) {
        if (selectedVehicle == null) {
            SlidingMarker.initializeGlobally();

            //on if reload = true
            if (reload == true) {
                for (i = 0; i < vehicles.length; i++) {
                    vehicles[i].Marker.setMap(null);
                }
                vehicles = [];
                currentD2V = 0
                $("#vehicleList").empty();
                $("#vehicleList").append($('<option>', { value: 'None' }).text('-- Select Vehicle ID --'));
            } else {
                $("#vehicleList").empty();
                $("#vehicleList").append($('<option>', { value: 'None' }).text('-- Select Vehicle ID --'));
            }

            $.post("getAllVehicles?loadHistorical=" + getHistorical, null, function (data) {
                if (data.length != vehicles.length) {
                    for (var i = 0; i < data.length; i++) {
                        $("#vehicleList").append($('<option>', { value: data[i].extendedData.ID }).text(data[i].VehicleID));
                        if (!containsVehicle(data[i].extendedData.ID)) {
                            if (data[i].driver != null) {
                                currentD2V += 1;
                            }
                            vehicles.push(new Vehicle(data[i], false));
                        }
                    }
                }

                $('#currentD2V').html('Current Active Vehicles without Assigned Drivers: <strong>' + (data.length - currentD2V)+ "</strong><hr />")
            });
        }
    };

    var selectThisVehicle = function (selected, old) {

        if (selected != old) {
            if (old != null) {
                old.modifyIcon("normal");
                mapLogEntry("unselected", old);
            }
            selected.modifyIcon("selected");
            selectedVehicle = selected;
            mapLogEntry("selected", selected);
            openNav();
            getVehicleData(selected.ID);
            getVehicleAlertData(selected.ID);
            var to = moment.utc().format('YYYY-MM-DD HH:mm');
            var from = moment.utc().add(-2, "hours").format('YYYY-MM-DD HH:mm');
            getVehicleHistoryData(selectedVehicle.VehicleID, from, to);
            $("#vehicleList").val(selectedVehicle.ID);
            getAvailableDrivers();

            //if driver is not null then show driver panel
            //if no driver then no interface so then no dispatch
            $('#collapseSixPanel').show();
            $('#collapseSix').collapse('show');
            if (selectedVehicle.driver != null && selectedVehicle.driver.DriverID != "00000000-0000-0000-0000-000000000000") {
                var src = base64ArrayBuffer(selectedVehicle.driver.imageData);
                $('#driverPic').html('<img id="ItemPreview" src="data:image/gif;base64,' + src + '"  width="150" class="vertical- align: top;"/>');
                $('#driverName').html(selectedVehicle.driver.DriverFirstName + " " + selectedVehicle.driver.DriverLastName);
                $('#driverEmail').html(selectedVehicle.driver.DriverEmail);
                if (selectedVehicle.driver.currentStatus.statusName == null) {
                    $('#driverStatus').html("Administrative Log On");
                } else {

                    $('#driverStatus').html(selectedVehicle.driver.currentStatus.statusName);
                }
                $('#behaviorsDiv').show();
                $('#collapseEightPanel').show();
                $('#SubmitDispatch').append(' ' + selectedVehicle.Name);
            } else {
                $('#driverPic').html('<span class="glyphicons glyphicons-user-key" style="font-size: 150px;" id="driverIcon"></span>');
                $('#driverName').html('No Driver Assigned');
                $('#driverEmail').html('');
                $('#driverStatus').html('');
                $('#behaviorsDiv').hide();
                $('#collapseSix').collapse('hide');
            }

            //if ODB codes are null then ADD module not present
            if (selectedVehicle.OBDVals != null) {
                $('#collapseSevenPanel').show();
                alert('Need to bind Diagnostic data')
            } else {
                $('#collapseSevenPanel').hide();
            }
        } else {
            if (selected.status == "Inactive") {
                selected.modifyIcon("Inactive");
            } else if (selected.status == "InAlert") {
                selected.modifyIcon("InAlert");
            } else {
                selected.modifyIcon("normal");
            }
            selectedVehicle = null;
            mapLogEntry("unselected", selected);
            closeNav();
        }
    }

    //#region GetVehicleData Functions
    function getVehicleData(ID) { //Get a download of the vehicle for ID
        var _url = 'getVehicleData';
        var _data = "ID=" + ID;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getVehicleDataSuccess,
            error: getVehicleDataError
        });
    }

    function getVehicleDataSuccess(result) {
        $('#collapseFive').collapse('show');
        $('#info_panel').html("<div class='vehicleInfo'><div class=\"col-sm-12\" id=\"vehicleInfoDiv\"><img id='vehicleClassImage' class='img-responsive' src='../Content/VClasses/" + result.extendedData.vehicleClassImage + "' align='right'><h3>" + result.extendedData.VehicleFriendlyName + "</h3><br /><p>This vehicle is a " + result.extendedData.Year + ", " + result.extendedData.Make + "-" + result.extendedData.Model + ". Plate Number: " + result.extendedData.licensePlate + ", has a Haul Limit of: " + result.extendedData.haulLimit + "/lbs and has ID Number: " + result.extendedData.vehicleID + ".</p></div></div>");
    }

    function getVehicleDataError(result, error) {
        var err = error;
        alert('A problem occurred getting the selected vehicle data, please reload or contact the administrator');
    }
    //#endregion

    //#region GetVehicleAlertData Functions
    function getVehicleAlertData(ID) {
        $('#collapseOne').collapse('show')
        $('#alert_panel').html("<img src=\'../Content/Images/preloader.gif\' width=\'100\' />");

        var _url = 'getVehicleAlerts';
        var _data = "ID=" + ID;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getVehicleAlertDataSuccess,
            error: getVehicleAlertDataError
        });
    }

    function getVehicleAlertDataSuccess(result) {
        if (result.length == 0) {
            $('#alert_panel').html('No Current Alerts. You can use the <a href="../Alerts/Index">Alerts Page</a> to search for historical alerts with this vehicle.');
        } else {
            $('#alertNumb').html(result.length);
            //set alertPanel content to table
            $('#alert_panel').html('');

            var alertContent = "<small><table id='alertsTable' class='table table-hover table-responsive table-striped table-condensed'><thead><tr><td class='header'>Name</td><td class='header'>Start</td><td class='header'>End</td></tr></thead><tbody></tbody></table></small>";
            $('#alert_panel').html(alertContent);

            //set content within table
            $("#alertsTable tbody").html('');

            for (var a = 0; a < result.length; a++) {
                var typeName = null;
                var alertStart = new Date(parseInt(result[a].alertStart.substr(6)));
                var alertEnd = new Date(parseInt(result[a].alertEnd.substr(6)));

                if (result[a].alertType == 0) {
                    typeName = "Exit";
                } else if (result[a].alertType == 1) {
                    typeName = "Enter";
                } else if (result[a].alertType == 2) {
                    typeName = "Speeding";
                }

                if (result[a].alertActive == true) {
                    var markup = "<tr class=\'danger rowClick\'>";
                } else {
                    var markup = "<tr class=\'rowClick\'>";
                }

                markup += "<td class=\'hidden\'>" + result[a].alertID + "</td><td class='text-center'>" + result[a].alertName.substr(0, 25) + " ... </td><td class='text-center'>" + alertStart.toLocaleDateString() + "@" + alertStart.toLocaleTimeString().replace(/:\d{2}\s/, ' ') + "</td><td class='text-center'>" + alertEnd.toLocaleDateString() + "@" + alertEnd.toLocaleTimeString().replace(/:\d{2}\s/, ' ') + "</td></tr>";
                $("#alertsTable tbody").append(markup);
            };
        }
    }

    function getVehicleAlertDataError(result, error) {
        var err = error;
        alert('A problem occurred getting the selected vehicle alert data, please reload or contact the administrator');
    }
    //#endregion

    //#region getVehicleHistory
    function getVehicleHistoryData(ID, start, end) {
        //show loader

        $('#collapseThree').collapse('show')
        $('#playback_panel').html("<img src=\'../Content/Images/preloader.gif\' width=\'100\' />");

        var _url = 'getHistory';
        var _data = "ID=" + ID + "&start=" + start + "&end=" + end;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                getVehicleHistoryDataSuccess(data, start);
            },
            error: getVehicleHistoryDataError
        });
    }

    function getVehicleHistoryDataSuccess(result, start) {
        if (result.length <= 0) {
            $('#playback_panel').html('No location elements returned for that search criteria. Please enter new criteria and try again.');
            //paint the panel
            var pbcontent = '<div id="vehicleHistoryDiv" class="hiddden">' +
                '<div id="historyCriteria" class="row col-sm-12" style="margin-bottom: 3px;">' +
                //'<select id="vehicleHistoryList" class="col-sm-3" style="margin-right: 3px;">' +
                //'<option value="None">-- Vehicle ID --</option>' +
                //'</select>' +
                '<div class="col-sm-3" id="playBackVehicleID"></div>' +
                '<input type="text" id="playBackFrom" class="col-sm-4" aria-label="From" placeholder="From Date:">' +
                '<select id="increments" class="col-sm-3" style="margin-left: 3px;">' +
                '<option value="2">2 Hours</option>' +
                '<option value="6">6 Hours</option>' +
                '<option value="8">8 Hours</option>' +
                '<option value="12">12 Hours</option>' +
                '</select>' +
                '<button id="historySearch" class="glyphicons glyphicons-search x05" style="margin-left: 3px;"></button>' +
                '<button id="historyDownload" class="glyphicons glyphicons-download x05"></button>' +
                '</div>' +
                '<div id="historySlider" class="col-sm-9"></div>' +
                '<div id="slidernumbers" class="col-sm-3 text-right"><span id="gpsRecordNumb" class="mapText"></span></div>' +
                '<div id="historyMap" class="col-sm-12" style="height: 200px; border: 1px solid #000; z-index: 1; padding-left: 0px;"></div>' +
                '<div id="historyInfo" class="col-sm-12 small">' +
                '<label>Date: </label><span id="datetime" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Lat: </label><span id="lat" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Lng:  </label><span id="lon" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Dir: </label><span id="dir" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Spd: </label><span id="speed" class="mapText"></span>' +
                '</div>' +
                '</div>';

            $('#playback_panel').append(pbcontent);

            historyMap = new google.maps.Map(document.getElementById("historyMap"), {
                zoom: 12,
                center: centerMap,
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                mapTypeControl: false,
                zoomControl: true,
                scaleControl: true,
                streetViewControl: false,
                fullscreenControl: false
            });


            $('#playBackVehicleID').html("<strong>" + selectedVehicle.VehicleID + "</strong>");
            $('#datetime').text(moment(start).format('MM/DD/YYYY HH:mm'));
            $('#lat').text("");
            $('#lon').text("");
            $('#dir').text("");
            $('#speed').text("");
            $('#gpsRecordNumb').text('# 0 of ' + historyData.length);

            $('#historySlider').slider();
            $('#historySlider').slider("value", 1);

            $('#playBackFrom').datetimepicker({
                dayOfWeekStart: 1
            });

            $('#playBackFrom').val(moment(start).add(moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm'));

            $('#vehicleHistoryLoader').addClass('hidden');
            $('#vehicleHistoryDiv').removeClass('hidden');

            $('#historySearch').click(function () {
                var from = moment(new Date($('#playBackFrom').val())).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm');
                var to = moment(from).add($('#increments').val(), 'hours').format('YYYY-MM-DD HH:mm');
                getVehicleHistoryData($('#playBackVehicleID').text(), from, to);
            });

            $('#historyDownload').click(function () {
                var from = moment(new Date($('#playBackFrom').val())).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm');
                var to = moment(from).add($('#increments').val(), 'hours').format('YYYY-MM-DD HH:mm');
                dowloadVehicleHistory($('#playBackVehicleID').text(), from, to);
            });
        } else {
            //paint the panel
            var pbcontent = '<div id="vehicleHistoryDiv" class="hiddden">' +
                '<div id="historyCriteria" class="row col-sm-12" style="margin-bottom: 3px;">' +
                //'<select id="vehicleHistoryList" class="col-sm-3" style="margin-right: 3px;">' +
                //'<option value="None">-- Vehicle ID --</option>' +
                //'</select>' +
                '<div class="col-sm-3" id="playBackVehicleID"></div>' +
                '<input type="text" id="playBackFrom" class="col-sm-4" aria-label="From" placeholder="From Date:">' +
                '<select id="increments" class="col-sm-3" style="margin-left: 3px;">' +
                '<option value="2">2 Hours</option>' +
                '<option value="6">6 Hours</option>' +
                '<option value="8">8 Hours</option>' +
                '<option value="12">12 Hours</option>' +
                '</select>' +
                '<button id="historySearch" class="glyphicons glyphicons-search x05" style="margin-left: 3px;"></button>' +
                '<button id="historyDownload" class="glyphicons glyphicons-download x05"></button>' +
                '</div>' +
                '<div id="historySlider" class="col-sm-9"></div>' +
                '<div id="slidernumbers" class="col-sm-3 text-right"><span id="gpsRecordNumb" class="mapText"></span></div>' +
                '<div id="historyMap" class="col-sm-12" style="height: 200px; border: 1px solid #000; z-index: 1; padding-left: 0px;"></div>' +
                '<div id="historyInfo" class="col-sm-12 small">' +
                '<label>Date: </label><span id="datetime" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Lat: </label><span id="lat" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Lng:  </label><span id="lon" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Dir: </label><span id="dir" class="mapText"></span>' +
                '<label>&nbsp;&nbsp;Spd: </label><span id="speed" class="mapText"></span>' +
                '</div>' +
                '</div>';
            //jsPanel.activePanels.getPanel("playBackPanel").content.append(pbcontent);
            $('#playback_panel').html(pbcontent);

            historyData = result;

            for (var i = 0; i < historyData.length; i++) {
                drivePathCoordinates.push({ lat: historyData[i].Lat, lng: historyData[i].Lon });
            }

            drivePath = new google.maps.Polyline({
                path: drivePathCoordinates,
                geodesic: true,
                strokeColor: '#61c',
                strokeOpacity: 1.0,
                strokeWeight: 1
            });

            historyMap = new google.maps.Map(document.getElementById("historyMap"), {
                zoom: 12,
                center: drivePathCoordinates[0],
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                mapTypeControl: false,
                zoomControl: true,
                scaleControl: true,
                streetViewControl: false,
                fullscreenControl: false
            });

            //vehicle marker
            historyVehicle = new google.maps.Marker({
                position: { lat: historyData[0].Lat, lng: historyData[0].Lon },
                draggable: false,
                map: historyMap
            });

            //attached drivepath --- ugly ... putting off until can think of something prettier...
            //if (drivePath != null) {
            //    drivePath.setMap(historyMap)
            //}

            $('#playBackVehicleID').html("<strong>" + selectedVehicle.VehicleID + "</strong>");
            $('#datetime').text(moment(start).format('MM/DD/YYYY HH:mm'));
            $('#lat').text(historyData[0].Lat);
            $('#lon').text(historyData[0].Lon);
            $('#dir').text(historyData[0].Direction);
            $('#speed').text(historyData[0].Speed);
            $('#gpsRecordNumb').text('# 1 of ' + historyData.length);

            //slider
            $('#historySlider').slider({
                animate: "slow",
                min: 1,
                max: historyData.length,
                step: 1,
                orientation: "horizontal",
                value: historyData.length,
                slide: function (event, slideEvt) {
                    historyVehicle.setMap(null);
                    var index = slideEvt.value;
                    //Set values based on slider
                    $('#datetime').text(moment(historyData[index].timestamp).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm'));
                    $('#lat').text(historyData[index].Lat);
                    $('#lon').text(historyData[index].Lon);
                    $('#dir').text(historyData[index].Direction);
                    $('#speed').text(historyData[index].Speed);
                    $('#gpsRecordNumb').text('# ' + index + ' of ' + historyData.length);

                    //center map
                    historyMap.setCenter({ lat: historyData[index].Lat, lng: historyData[index].Lon });

                    //add marker
                    historyVehicle = new google.maps.Marker({
                        position: { lat: historyData[index].Lat, lng: historyData[index].Lon },
                        draggable: false,
                        map: historyMap
                    });
                }
            });

            $('#historySlider').slider("value", 1);

            $('#playBackFrom').datetimepicker({
                dayOfWeekStart: 1
            });

            $('#playBackFrom').val(moment(start).add(moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm'));

            $('#vehicleHistoryLoader').addClass('hidden');
            $('#vehicleHistoryDiv').removeClass('hidden');

            $('#historySearch').click(function () {
                var from = moment(new Date($('#playBackFrom').val())).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm');
                var to = moment(from).add($('#increments').val(), 'hours').format('YYYY-MM-DD HH:mm');
                getVehicleHistoryData($('#playBackVehicleID').text(), from, to);
            });

            $('#historyDownload').click(function () {
                var from = moment(new Date($('#playBackFrom').val())).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm');
                var to = moment(from).add($('#increments').val(), 'hours').format('YYYY-MM-DD HH:mm');
                dowloadVehicleHistory($('#playBackVehicleID').text(), from, to);
            });
        }
    }

    function getVehicleHistoryDataError(result, error) {
        var err = error;
        alert('A problem occurred getting the selected vehicle data, please reload or contact the administrator');
    }
    //#endregion

    //#region change alert status
    function changeAlertStatus(ID, enabled, updatedb) {
        var IDList = [ID];
        var _url = 'changeAlertStatus';
        var _data = "aList=" + IDList + "&enabled=" + enabled + "&updatedb=" + updatedb;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                changeAlertStatusSuccess(data, ID, enabled, updatedb);
            },
            error: changeAlertStatusError
        });
    }

    function changeAlertStatusSuccess(data, ID, enabled, updatedb) {
        if (data == "OK") {
            toastr.options = {
                "closeButton": false,
                "debug": false,
                "newestOnTop": false,
                "progressBar": false,
                "positionClass": "toast-bottom-left",
                "preventDuplicates": false,
                "onclick": null,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            }
            Command: toastr["success"]("GeoFence Alert Status Changed");
        } else {

        }
    }

    function changeAlertStatusError(result, error) {
        var err = error;
        alert('A problem occurred changing the status of this alert, please reload or contact the administrator');
    }
    //#endregion

    //#region download vehicle history data

    function dowloadVehicleHistory(ID, start, end) {
        var _url = 'getHistory';
        var _data = "ID=" + ID + "&start=" + start + "&end=" + end;
        $.ajax({
            type: "GET",
            dataType: "json",
            ID: ID,
            start: start,
            end: end,
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                dowloadVehicleHistorySuccess(data, this.ID, this.start, this.end);
            },
            error: dowloadVehicleHistoryError
        });
    }

    function dowloadVehicleHistorySuccess(result, ID, start, end) {
        //If JSONData is not an object then JSON.parse will parse the JSON string in an Object
        var arrData = typeof result != 'object' ? JSON.parse(result) : result;

        var CSV = '';
        //Set Report title in first row or line

        CSV += 'Vehicle ' + ID + ': Location history Data from ' + moment(start).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:MM') + ' to ' + moment(end).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:MM') + '.\r\n\n';

        //This condition will generate the Label/Header
        var row = "Direction,In Polygon,Lat,Lon,Poly Name,Speed,VehicleID,Status,Timestamp\r\n";
        CSV += row + '\r\n';

        //1st loop is to extract each row
        for (var i = 0; i < arrData.length; i++) {
            var notin = ['ExtensionData', 'ID', 'lastMessageReceived', 'runID'];

            var row = "";

            //2nd loop will extract each column and convert it in string comma separated
            for (var index in arrData[i]) {
                if (!containsObject(index, notin)) {
                    if (index != "timestamp") {
                        row += '"' + arrData[i][index] + '",';
                    } else {
                        row += moment(arrData[i][index]).add(moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm');
                    }
                }
            }

            row.slice(0, row.length - 1);

            //add a line break after each row
            CSV += row + '\r\n';
        }

        if (CSV == '') {
            alert("Invalid data");
            return;
        }

        //Generate a file name
        var fileName = ID + "_" + start + "_" + end;
        //this will remove the blank-spaces from the title and replace it with an underscore
        //fileName += ReportTitle.replace(/ /g, "_");

        //Initialize file format you want csv or xls
        var uri = 'data:text/csv;charset=utf-8,' + escape(CSV);

        // Now the little tricky part.
        // you can use either>> window.open(uri);
        // but this will not work in some browsers
        // or you will not get the correct file extension

        //this trick will generate a temp <a /> tag
        var link = document.createElement("a");
        link.href = uri;

        //set the visibility hidden so it will not effect on your web-layout
        link.style = "visibility:hidden";
        link.download = fileName + ".csv";

        //this part will append the anchor tag and remove it after automatic click
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    function dowloadVehicleHistoryError(result, error) {
        var err = error;
        alert('A problem occurred selecting and/or downloading the selected vehicle data, please reload or contact the administrator');
    }

    //#endregion

    //#region geofence functionality
    $('#gd_show').change(function () {
        if (this.checked) {
            showPolygons();
            drawingManager.setMap(map);
        } else {
            hidePolygons();
            drawingManager.setMap(null);
            if ($("#geoInfoDiv").is(":visible")) {
                $('#geoInfoDiv').collapse('hide');
            }
            $('#fenceAlertInfo').html('');
        }
    });

    //#region show historical functionality
    $('#historical').change(function () {
        if (this.checked) {
            getHistorical = false;
            getVehicles(true);
        } else {
            getHistorical = true;
            getVehicles(true);
        }
    });

    function showPolygons() {
        shapes = [];

        $.post("getAllFences", null, function (data) {
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
                        clearSelection();
                        selectedShape = this;
                        editPolygon(this.objInfo);
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
                        clearSelection();
                        selectedShape = this;
                        editPolygon(this.objInfo);
                    });

                    google.maps.event.addListener(shape, 'center_changed ', function (event) {
                        clearSelection();
                        selectedShape = this;
                        editPolygon(this.objInfo);
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
                        clearSelection();
                        selectedShape = this;
                        editPolygon(this.objInfo);
                    });

                    google.maps.event.addListener(shape, 'dragend', function (event) {
                        clearSelection();
                        selectedShape = this;
                        editPolygon(this.objInfo);
                    });
                }

                var obj = {
                    "type": data[i].geoType,
                    "geoFenceID": data[i].geoFenceID,
                    "notes": data[i].notes,
                    "polyName": data[i].polyName
                };

                shape.objInfo = obj;

                google.maps.event.addListener(shape, 'rightclick', function () {
                    if (this.editable) {
                        clearSelection();
                        selectedShape = this;
                        this.setEditable(false);
                        this.setDraggable(false);
                        editPolygon(this.objInfo);
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
                    this.setOptions({ fillColor: 'red', fillOpacity: 0.50, });
                    editPolygon(this.objInfo);
                });

                shapes.push(shape);

            }

            setPolygonsVisible(true);
        });
    }

    function hidePolygons() {
        setPolygonsVisible(false);
        $('#polyName').val('');
        $('#polyNotes').text('');
        $('#geoFenceID').val('');
        $('#fenceAlertInfo').html('');
        $('#geoInfoDiv').collapse('hide');
        $('#deletePolygon').addClass('hidden');
        $('#savePolygon').addClass('hidden');
        $('#deleteNewPolygon').addClass('hidden');
        $('#saveNewPolygon').addClass('hidden');
    }

    function setPolygonsVisible(show) {
        for (var i = 0; i <= shapes.length - 1; i++) {
            if (show) {
                shapes[i].setMap(map);
                polysVisible = true;
                //polygons[i].setOptions({ visible: true });
            } else {
                shapes[i].setMap(null);
                polysVisible = false;
                //polygons[i].setOptions({ visible: false });
            }
        }
    }

    function clearSelection() {
        for (var i = 0; i < shapes.length; i++) {
            shapes[i].setOptions({ fillColor: '#000000', fillOpacity: 0.25 });
        }

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

    function editPolygon(polygonInfo) {
        $('#polyName').val('');
        $('#polyNotes').text('');
        $('#polyName').val(polygonInfo.polyName);
        $('#polyNotes').text(polygonInfo.notes);
        $('#geoFenceID').val(polygonInfo.geoFenceID);

        $.post("getGeoFenceAlerts?id=" + polygonInfo.geoFenceID, null, function (data) {
            $('#fenceAlertInfo').html('');

            if (data.length > 0) {
                var alertContent = "<h5>Alerts attached to this fence.</h5><small><table id='fenceAlertsTable' class='table table-responsive table-striped table-condensed table-bordered'><thead class='ah'><tr><td class='header'>Alert Name</td><td class='header'>Start</td><td class='header'>End</td><td class='header'>Active</td><td class='header'>Edit</td></tr></thead><tbody></tbody></table></small>";

                $('#fenceAlertInfo').html(alertContent);

                //set content within table
                $("#fenceAlertsTable tbody").html('');

                for (var a = 0; a < data.length; a++) {
                    var markup = '';
                    var alertStart = moment(data[a].AlertStartTime).format('MM/DD/YYYY HH:ss');
                    var alertEnd = moment(data[a].AlertEndTime).format('MM/DD/YYYY HH:ss');

                    markup += "<tr><td class='text-center'>" + data[a].AlertFriendlyName + "</td><td class='text-center'>" + alertStart + "</td><td class='text-center'>" + alertEnd + "</td>";

                    if (data[a].AlertActive == true) {
                        markup += "<td class='text-center'><input id='" + data[a].AlertID + "' class='alertActive' type='checkbox' checked data-toggle='toggle' data-size='mini'></td>";
                    } else {
                        markup += "<td class='text-center'><input id='" + data[a].AlertID + "' class='alertActive' type='checkbox' data-toggle='toggle' data-size='mini'></td>";
                    }
                    markup += "<td class='text-center'><button type='button' class='btn btn-default btn-xs glyphicons glyphicons-pencil EditAlertButton' id='EA_" + data[a].AlertID + "'></button></td></tr>";
                    $("#fenceAlertsTable tbody").append(markup);
                };
            } else {
                $('#fenceAlertInfo').html('<h5>No Alerts attached to this fence.</h5>');
            }

            $("[class='alertActive']").bootstrapToggle();
            $(".alertActive").change(function () {
                if (this.checked) {
                    changeAlertStatus($(this).attr('id'), true, true);
                } else {
                    changeAlertStatus($(this).attr('id'), false, true);
                }
            });

            $(".EditAlertButton").click(function () {
                var id = $(this).attr('id').substr(3, $(this).attr('id').length);
                alert(id);
            });

        });

        $('#deletePolygon').removeClass('hidden');
        $('#savePolygon').removeClass('hidden');
        $('#deleteNewPolygon').addClass('hidden');
        $('#saveNewPolygon').addClass('hidden');
        $('#geoInfoDiv').collapse('show');
    }

    function newPolygon() {
        $('#polyName').val('');
        $('#polyNotes').text('');
        $('#geoFenceID').val('00000000-0000-0000-0000-000000000000');
        $('#fenceAlertInfo').html('');
        $('#deleteNewPolygon').removeClass('hidden');
        $('#saveNewPolygon').removeClass('hidden');
        $('#deletePolygon').addClass('hidden');
        $('#savePolygon').addClass('hidden');
        $('#geoInfoDiv').collapse('show')
    }

    $('#deletePolygon').click(function () {
        if (confirm("Deleting will remove this GEO Fence from the system entirely. Are you sure?")) {
            $.post("DeleteFence?id=" + $('#geoFenceID').val(), null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Deleted.");
                    $('#polygonModal').modal('hide');
                    deleteSelectedShape();
                    $('#geoInfoDiv').collapse('hide')
                    $('#polyName').val('');
                    $('#polyNotes').text('');
                    $('#geoFenceID').val('');
                    $('#deletePolygon').addClass('hidden');
                    $('#savePolygon').addClass('hidden');
                    $('#deleteNewPolygon').addClass('hidden');
                    $('#saveNewPolygon').addClass('hidden');
                    hidePolygons();
                    showPolygons();
                } else {
                    toastr.warning("Delete Aborted. There was an issue with deleting this GEO Fence:" + data);
                }
            });
        }

    });

    $('#savePolygon').click(function () {

        if (selectedShape.objInfo.type == "polygon") {
            var vertices = selectedShape.getPath();
            var coordinates = [];

            for (var i = 0; i < vertices.getLength(); i++) {
                var xy = vertices.getAt(i);
                coordinates.push(xy.lat() + "^" + xy.lng());
            }
            $.post("AddFence?type=" + selectedShape.objInfo.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=" + $('#geoFenceID').val() + "&geoFence=" + coordinates + "&radius=0", null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    hidePolygons();
                    showPolygons();
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        } else if (selectedShape.objInfo.type == "circle") {
            var radius = selectedShape.getRadius();
            var cLat = selectedShape.getCenter().lat();
            var cLng = selectedShape.getCenter().lng();

            $.post("AddFence?type=" + selectedShape.objInfo.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=" + $('#geoFenceID').val() + "&geoFence=" + cLat + "^" + cLng + "&radius=" + radius, null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    hidePolygons();
                    showPolygons();
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

            $.post("AddFence?type=" + selectedShape.objInfo.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=" + $('#geoFenceID').val() + "&geoFence=" + coordinates + "&radius=0", null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");

                    hidePolygons();
                    showPolygons();
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        }
    });

    $('#deleteNewPolygon').click(function () {
        deleteSelectedShape();
        $('#geoInfoDiv').collapse('hide');
        hidePolygons();
        showPolygons();
    });

    $('#saveNewPolygon').click(function () {

        if (selectedShape.type == google.maps.drawing.OverlayType.POLYGON) {
            var vertices = selectedShape.getPath();
            var coordinates = [];

            for (var i = 0; i < vertices.getLength(); i++) {
                var xy = vertices.getAt(i);
                coordinates.push(xy.lat() + "^" + xy.lng());
            }

            $.post("AddFence?type=" + selectedShape.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=00000000-0000-0000-0000-000000000000&geoFence=" + coordinates + "&radius=0", null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    deleteSelectedShape();
                    hidePolygons();
                    showPolygons();
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        } else if (selectedShape.type == google.maps.drawing.OverlayType.CIRCLE) {
            var radius = selectedShape.getRadius();
            var cLat = selectedShape.getCenter().lat();
            var cLng = selectedShape.getCenter().lng();

            $.post("AddFence?type=" + selectedShape.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=00000000-0000-0000-0000-000000000000&geoFence=" + cLat + "^" + cLng + "&radius=" + radius, null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    deleteSelectedShape();
                    hidePolygons();
                    showPolygons();
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

            $.post("AddFence?type=" + selectedShape.type + "&polyName=" + $('#polyName').val() + "&notes=" + $('#polyNotes').val() + "&geofenceID=00000000-0000-0000-0000-000000000000&geoFence=" + coordinates + "&radius=0", null, function (data) {
                if (data == "OK") {
                    toastr.success("GEO Fence Saved.");
                    deleteSelectedShape();
                    hidePolygons();
                    showPolygons();
                } else {
                    toastr.warning("Error during save. There was an issue with saving this GEO Fence:" + data);
                }
            });
        }

        $('#polyName').val(polygonInfo.polyName);
        $('#polyNotes').text(polygonInfo.notes);
    });

    //#endregion

    $('#appsIcon').click(function () {
        openNav();
    });

    $('#geofenceIcon').click(function () {
        if (!polysVisible) {
            openNav();
            $('#collapseTwo').collapse('show');
            $('#gd_show').bootstrapToggle('on')
        }
    });

//#region get monthly alerts
function getMonthsAlerts() { //Get a download of the vehicle for ID
    //empty alertpanel
    jsPanel.activePanels.getPanel("alertPanel").content.css("padding", "10px");
    jsPanel.activePanels.getPanel("alertPanel").content.empty();

    //add loader
    jsPanel.activePanels.getPanel("alertPanel").content.append("<div id=\'vehicleHistoryLoader\' class=\'loading\'><img src=\'../Content/Images/preloader.gif\' width=\'100\' /></div>");

    var _url = 'CurrentMonthAlerts';
    var _data = "";
    $.ajax({
        type: "GET",
        dataType: "json",
        url: _url,
        data: _data,
        contentType: "application/json; charset=utf-8",
        success: getMonthsAlertsSuccess,
        error: getMonthsAlertsError
    });
}

function getMonthsAlertsSuccess(data) {
    jsPanel.activePanels.getPanel("alertPanel").content.empty();
    jsPanel.activePanels.getPanel("alertPanel").content.append("<p class=\'text-center\'>Alerts for the current month");
    jsPanel.activePanels.getPanel("alertPanel").content.append("<div class=\'list-group\' id=\'alertlistGroup\'></div>");
    for (var i = 0; i < data.GridAlertList.length; i++) {
        $('#alertlistGroup').append("<a href=\'#\'class=\'list-group-item\'>" + data.GridAlertList[i].AlertName + "<span class=\'label label-danger\' style=\'margin-left: 25px;\'>" + data.GridAlertList[i].Alerts.length + "</span></a>");
    }
}

function getMonthsAlertsError(result, error) {
    //empty alertpanel
    jsPanel.activePanels.getPanel("alertPanel").content.empty();

    var err = error;
    alert('A problem occurred getting the chosen alert data, please reload or contact the administrator. Error:' + err.toString());
}
//#endregion

//#region getAllAlerts Functions
function getAllAlerts(from, to) { //Get a download of the vehicle for ID
    jsPanel.activePanels.getPanel("alertPanel").content.empty();

    //add loader
    jsPanel.activePanels.getPanel("alertPanel").content.append("<div id=\'vehicleHistoryLoader\' class=\'loading\'><img src=\'../Content/Images/preloader.gif\' width=\'100\' /></div>");

    var _url = 'getAlerts';
    var _data = "";
    if (from == null || from == "" && to == null || to == "") {
        _data = "";
    } else {
        _data = "from=" + from + "&to=" + to;
    }
    $.ajax({
        type: "GET",
        dataType: "json",
        url: _url,
        data: _data,
        contentType: "application/json; charset=utf-8",
        success: getAllAlertsSuccess,
        error: getallAlertsError
    });
}

function getAllAlertsSuccess(result) {
    jsPanel.activePanels.getPanel("alertPanel").content.empty();
    var markup = "";

    //create table
    markup += "<h2>Alerts</h2>";
    markup += "<div id=\'toolbar1\'>";
    markup += "<div class=\'form-inline\'><div class=\'input-group input-group\'><input type=\'text\' id=\'alertDTPFrom\' class=\'form-control\' aria-label=\'From Date\' placeholder=\'From Date\'><span class=\'input-group-btn\'><button class=\'btn btn-default\' type=\'button\' id=\'alertDTPfromDateOpener\'><i class=\'glyphicons glyphicons-calendar\'></i></button></span></div><div class=\'input-group\'><input type=\'text\' id=\'alertDTPTo\' class=\'form-control\' aria-label=\'To Date\' placeholder=\'To Date\'><span class=\'input-group-btn\'><button class=\'btn btn-default\' type=\'button\' id=\'alertDTPToDateOpener\'><i class=\'glyphicons glyphicons-calendar\'></i></button></span></div><button class=\'btn btn-default\' id=\'alertFilter\'><i class=\'glyphicons glyphicons-filter\'></i></button><button style=\'margin-left: 10px;\' class=\'btn btn-primary\' id=\'alertsAdmin\'><i class=\'glyphicons glyphicons-cogwheels\'></i> Alert Administration</button></div>";
    markup += "</div>";
    markup += "<table id=\'alertsPaneTable\'><thead><tr>";
    markup += "<th data-field=\'alertID\' data-sortable=\'true\' class=\'text-center\' data-visible=\'false\'>Alert ID</th>"
    markup += "<th data-field=\'alertName\' data-sortable=\'true\' class=\'text-center\'>Alert Name</th>";
    markup += "<th data-field=\'vehicleID\' data-sortable=\'true\' class=\'text-center\'>Vehicle ID</th>";
    markup += "<th data-field=\'alertType\' data-sortable=\'true\' class=\'text-center\'>Alert Type</th>";
    markup += "<th data-field=\'alertStart\' data-sortable=\'true\' class=\'text-center\'>Alert Start</th>";
    markup += "<th data-field=\'alertEnd\' data-sortable=\'true\' class=\'text-center\'>Alert End</th>";
    markup += "<th data-field=\'alertMaxValue\' data-sortable=\'true\' class=\'text-center\'>Max Value</th>";
    markup += "</tr></thead><tbody>";

    //get data
    for (var a = 0; a < result.length; a++) {
        var typeName = null;
        var alertStart = new Date(parseInt(result[a].alertStart.substr(6)));
        var alertEnd = new Date(parseInt(result[a].alertEnd.substr(6)));

        if (result[a].alertType == 0) {
            typeName = "Exit";
        } else if (result[a].alertType == 1) {
            typeName = "Enter";
        } else if (result[a].alertType == 2) {
            typeName = "Speeding";
        }

        markup += "<tr><td class=\'text-center\'>" + result[a].alertID + "</td><td class=\'text-center\'>" + result[a].alertName + "</td><td class=\'text-center\'>" + result[a].vehicleID + "</td><td class=\'text-center\'>" + typeName + "</td><td class=\'text-center\'>" + alertStart.toLocaleDateString() + " @ " + alertStart.toLocaleTimeString().replace(/:\d{2}\s/, ' ') + "</td><td class=\'text-center\'>" + alertEnd.toLocaleDateString() + " @ " + alertEnd.toLocaleTimeString().replace(/:\d{2}\s/, ' ') + "</td><td class=\'text-center\'>" + result[a].maxVal + "</td></tr>";

    }
    markup += "</tbody></table>"

    jsPanel.activePanels.getPanel("alertPanel").content.append(markup);

    //#region datetime and click table row stuff
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();
    today = mm + '/' + dd + '/' + yyyy;

    $('#alertDTPFrom').datetimepicker({
        dayOfWeekStart: 1,
        lang: 'en',
        disabledDates: ['1986/01/08', '1986/01/09', '1986/01/10'],
        startDate: today
    });

    $('#alertDTPfromDateOpener').click(function () {
        $('#alertDTPFrom').datetimepicker('show');
    });

    $('#alertDTPTo').datetimepicker({
        dayOfWeekStart: 1,
        lang: 'en',
        disabledDates: ['1986/01/08', '1986/01/09', '1986/01/10'],
        startDate: today
    });

    $('#alertDTPToDateOpener').click(function () {
        $('#alertDTPTo').datetimepicker('show');
    });

    $('#alertFilter').click(function () {
        //var url = '@Url.Action("/Index")' + '?from=' + $('#alertDTPFrom').val() + '&to=' + $('#alertDTPTo').val();
        getAllAlerts($('#alertDTPFrom').val(), $('#alertDTPTo').val());
    });

    var $table = $('#alertsPaneTable');

    $(function () {
        $table.on('click-row.bs.table', function (e, row, $element) {
            $('.success').removeClass('success');
            $($element).addClass('success');

            var index = $table.find('tr.success').data('index');
            getAlert($table.bootstrapTable('getData')[index].alertID);
            //alert($table.bootstrapTable('getData')[index].alertID);
        });
    });

    //#endregion

    //bootstrap table
    $('#alertsPaneTable').bootstrapTable({
        toolbar: '#toolbar1',
        pagination: true,
        striped: true,
        showColumns: true,
        showExport: true,
        search: true,
    });


    $('#alertsAdmin').click(function () {
        jsPanel.activePanels.getPanel("alertPanel").content.empty();
        jsPanel.activePanels.getPanel("alertPanel").content.append("What the hell goes here?");
    });
}

function getallAlertsError(result, error) {
    var err = error;
    alert('A problem occurred getting the vehicle alert data, please reload or contact support.');
}
//#endregion

//#region getSpecificAlert
function getAlert(alertID) { //Get a download of the vehicle for ID
    //view alert panel functionality
    $.jsPanel({
        id: "alertViewPanel",
        position: { my: "center-top", at: "center-top", offsetY: 100, offsetX: 0 },
        headerTitle: "Alert View",
        headerLogo: '<span class=\'glyphicons glyphicons-light-beacon\' style=\'margin: 4px 0 0 4px;font-size:2.0rem;\'></span>&nbsp;',
        theme: "#F7AD00",
        headerControls: {
            maximize: 'remove',
            minimize: 'remove',
            normalize: 'remove',
            smallify: 'remove'
        },
        content: '<div id=\'alertLoader\' class=\'loading hidden\'><img src=\'../Content/Images/preloader.gif\' width=\'100\' /></div>' +
        '<div id="specificAlertPane">' +
        '<h2>View Alert</h2>' +
        '<hr />' +
        '<input type="hidden" id=alertID />' +
        '<input type="hidden" id=vehicleID />' +
        '<div class="col-md-12">' +
        '<p id="alertDesc"></p>' +
        '<div class="col-md-12 pull-left">' +
        '<strong>As of: </strong><label id="LC" class="mapText"></label>&nbsp;&nbsp;' +
        '<strong>Lat: </strong><label id="Lat" class="mapText"></label>&nbsp;&nbsp;' +
        '<strong>Long: </strong><label id="Long" class="mapText"></label>&nbsp;&nbsp;' +
        '<strong>Direction: </strong><label id="Dir" class="mapText"></label>&nbsp;&nbsp;' +
        '<strong>Speed: </strong><label id="SPD" class="mapText"></label><br />' +
        '</div>' +
        '<div class="col-md-12" id="alertMap"><br /><br /></div>' +
        '<div class="col-md-12"></div>' +
        '<div class="col-md-12 text-center sliderDiv">' +
        '<br />' +
        '<div id="alertMapSlider"></div>' +
        '<div id="broadcastNum"></div>' +
        '<br />' +
        '</div>' +
        '<div class="col-md-2 sliderDiv"><label id="ST" class="sm pull-left"></label></div>' +
        '<div class="col-md-8 sliderDiv text-center"></div>' +
        '<div class="col-md-2 sliderDiv"><label id="ET" class="small pull-right"></label></div>' +
        '</div>' +
        '</div>',
        contentSize: {
            width: function () { return $(window).width() / 1.5 },
            height: function () { return $(window).height() / 1.5 },
        },
        callback: function () {
            this.content.css("padding", "10px");

            var alertmapDiv = null;
            var alertmap = null;

            viewAlert();

            //#region get alert
            function viewAlert() {
                //unhide loader
                $('#alertLoader').show();

                var _url = 'ViewAlert';
                var _data = "alertID=" + alertID;
                $.ajax({
                    type: "GET",
                    dataType: "json",
                    url: _url,
                    data: _data,
                    contentType: "application/json; charset=utf-8",
                    success: viewAlertSuccess,
                    error: viewAlertError
                });
            }

            function viewAlertSuccess(data) {
                if (data) {
                    $('#alertID').val(data.alertID);
                    $('#vehicleID').val(data.vehicleID);
                    $('#alertDesc').html("Vehicle <strong>" + data.vehicleID + "</strong> triggered the <strong>" + data.alertName + "</strong> alert that started at <strong>" + moment(data.alertStart).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm') + "</strong> and ended at <strong>" + moment(data.alertEnd).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm') + "</strong>.")
                    getAlertData();
                }
            }

            function viewAlertError(result, error) {
                //unhide loader
                $('#alertLoader').hide();
                $('#specificAlertPane').hide();

                //empty alertpanel
                jsPanel.activePanels.getPanel("alertViewPanel").content.empty();

                var err = error;
                alert('A problem occurred getting the chosen alert, please reload or contact the administrator. Error:' + err.toString());
            }
            //#endregion


            //#region get alert history
            function getAlertData() {
                var _url = 'GetAlertHistory';
                var _data = "alertID=" + $('#alertID').val() + "&vehicleID=" + $('#vehicleID').val();
                $.ajax({
                    type: "GET",
                    dataType: "json",
                    url: _url,
                    data: _data,
                    contentType: "application/json; charset=utf-8",
                    success: getAlertDataSuccess,
                    error: getAlertDataError
                });
            }

            function getAlertDataSuccess(data) {
                if (data) {
                    alertmapDiv = document.getElementById('alertMap');
                    alertmap = new google.maps.Map(alertmapDiv, {
                        zoom: 13,
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

                    //center map
                    alertmap.setCenter({ lat: data.Locations[0].Lat, lng: data.Locations[0].Lon });

                    //set initial mapping information
                    $('#LC').text(moment(data.Alert.alertStart).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY') + " @ " + moment(data.Alert.alertStart).add(moment().utcOffset(), 'minutes').format('HH:mm'));
                    $('#Lat').text(data.Locations[0].Lat);
                    $('#Long').text(data.Locations[0].Lon);
                    $('#Dir').text(data.Locations[0].Direction);
                    $('#SPD').text(data.Locations[0].Speed);
                    $('#broadcastNum').html("<p>GPS Broadcast #: 1 of " + data.Locations.length)

                    //vehicle marker
                    alertMapvehicle = new google.maps.Marker({
                        position: { lat: data.Locations[0].Lat, lng: data.Locations[0].Lon },
                        title: data.Alert.vehicleID,
                        draggable: false,
                        map: alertmap
                    });

                    //slider
                    $('#alertMapSlider').slider({
                        animate: "slow",
                        min: 1,
                        max: data.Locations.length,
                        step: 1,
                        orientation: "horizontal",
                        value: data.Locations.length,
                        slide: function (event, slideEvt) {
                            alertMapvehicle.setMap(null);
                            var index = slideEvt.value - 1;
                            //Set values based on slider
                            var date = new Date(parseInt(data.Locations[index].timestamp.substr(6)));
                            $('#LC').text(date.toLocaleDateString() + " at " + date.toLocaleTimeString());
                            $('#Lat').text(data.Locations[index].Lat);
                            $('#Long').text(data.Locations[index].Lon);
                            $('#Dir').text(data.Locations[index].Direction);
                            $('#SPD').text(data.Locations[index].Speed);

                            $('#broadcastNum').html("<p>GPS Broadcast #: " + (index + 1) + " of " + data.Locations.length)

                            //center map
                            alertmap.setCenter({ lat: data.Locations[index].Lat, lng: data.Locations[index].Lon });

                            //add marker
                            alertMapvehicle = new google.maps.Marker({
                                position: { lat: data.Locations[index].Lat, lng: data.Locations[index].Lon },
                                title: data.Alert.VehicleID,
                                draggable: false,
                                map: alertmap
                            });
                        }
                    });

                    $('#alertMapSlider').slider("value", 1);
                }

                //unhide loader
                $('#alertLoader').hide();
            }

            function getAlertDataError(result, error) {
                //empty alertpanel
                jsPanel.activePanels.getPanel("alertViewPanel").content.empty();

                var err = error;
                alert('A problem occurred getting the chosen alert data, please reload or contact the administrator. Error:' + err.toString());
            }
            //#endregion
        },
    });
}
//#endregion

//#region Open Playback
function OpenPlayback(ID, start, end) {
    jsPanel.activePanels.getPanel("playBackPanel").content.empty();

    var _url = 'getHistory';
    var _data = "ID=" + ID + "&start=" + start + "&end=" + end;

    if (ID == 0 && start == 0 && end == 0) {
        ID = vehicles[0].VehicleID;
        end = moment.utc().format('YYYY-MM-DD HH:mm');
        start = moment.utc().add(-2, "hours").format('YYYY-MM-DD HH:mm');
        _data = "ID=" + ID + "&start=" + start + "&end=" + end;
    }

    $.ajax({
        type: "GET",
        dataType: "json",
        ID: ID,
        start: start,
        end: end,
        url: _url,
        data: _data,
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            OpenPlaybackSuccess(data, this.ID, this.start, this.end);
        },
        error: OpenPlaybackError
    });
}

function OpenPlaybackSuccess(result, ID, start, end) {
    if (result.length <= 0) {
        alert('No location elements returned for that search criteria. Please enter new criteria and try again.');
        result = oldVehicleHistoryResults;
    } else {
        oldVehicleHistoryResults = result;
    }

    //paint the panel
    var pbcontent = '<br /><br /><br />' +
        '<div id="vehicleHistoryDiv" class="hiddden">' +
        '<div id="historyCriteria" class="row col-sm-12 center-block">' +
        '<select id="vehicleHistoryList_M" class="col-sm-3">' +
        '<option value="None">-- Vehicle ID --</option>' +
        '</select>' +
        //'<div class="col-sm-3" id="playBackVehicleID"></div>' +
        '<input type="text" id="playBackFrom_M" class="col-sm-4" aria-label="From" placeholder="From Date:">' +
        '<select id="increments_M" class="col-sm-3">' +
        '<option value="2">2 Hours</option>' +
        '<option value="6">6 Hours</option>' +
        '<option value="8">8 Hours</option>' +
        '<option value="12">12 Hours</option>' +
        '</select>' +
        '<button id="historySearch_M" class="btn btn-primary glyphicons glyphicons-search" style="margin-left: 3px; margin-right: 3px;" alt="Search"></button>' +
        '<button id="historyDownload_M" class="btn btn-primary glyphicons glyphicons-download" alt="download"></button>' +
        '</div>' +
        '<div id="historySlider" class="col-sm-12" style="margin-top: 15px; margin-bottom: 15px;"></div>' +
        '<div id="historyMap" class="col-sm-12" style="height: 55.0em; border: 1px solid #000; z-index: 1; padding-left: 0px;"></div>' +
        '<div id="historyInfo" class="col-sm-12">' +
        '<label>Date: </label><span id="datetime" class="mapText_M"></span>' +
        '<label>&nbsp;&nbsp;Lat: </label><span id="lat" class="mapText_M"></span>' +
        '<label>&nbsp;&nbsp;Lng:  </label><span id="lon" class="mapText_M"></span>' +
        '<label>&nbsp;&nbsp;Dir: </label><span id="dir" class="mapText_M"></span>' +
        '<label>&nbsp;&nbsp;Spd: </label><span id="speed" class="mapText_M"></span>' +
        '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span id="gpsRecordNumb" class="mapText_M"></span>' +
        '</div>' +
        '</div>';
    jsPanel.activePanels.getPanel("playBackPanel").content.append(pbcontent);

    //work drop down box of vehicles
    $('#vehicleHistoryList_M').empty();
    for (var index in vehicles) {
        $('#vehicleHistoryList_M').append('<option value="' + vehicles[index].VehicleID + '">' + vehicles[index].VehicleID + '</option>');
    }
    $('#vehicleHistoryList_M').val(ID);

    historyData = result;

    for (var i = 0; i < historyData.length; i++) {
        drivePathCoordinates.push({ lat: historyData[i].Lat, lng: historyData[i].Lon });
    }

    drivePath = new google.maps.Polyline({
        path: drivePathCoordinates,
        geodesic: true,
        strokeColor: '#61c',
        strokeOpacity: 1.0,
        strokeWeight: 1
    });

    historyMap = new google.maps.Map(document.getElementById("historyMap"), {
        zoom: 12,
        center: drivePathCoordinates[0],
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        mapTypeControl: false,
        zoomControl: true,
        scaleControl: true,
        streetViewControl: false,
        fullscreenControl: false
    });

    //vehicle marker
    historyVehicle = new google.maps.Marker({
        position: { lat: historyData[0].Lat, lng: historyData[0].Lon },
        draggable: false,
        map: historyMap
    });

    //attached drivepath --- ugly ... putting off until can think of something prettier...
    //if (drivePath != null) {
    //    drivePath.setMap(historyMap)
    //}

    $('#datetime').text(moment(start).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm'));
    $('#lat').text(historyData[0].Lat);
    $('#lon').text(historyData[0].Lon);
    $('#dir').text(historyData[0].Direction);
    $('#speed').text(historyData[0].Speed);
    $('#gpsRecordNumb').text('# 1 of ' + historyData.length);

    //slider
    $('#historySlider').slider({
        animate: "slow",
        min: 1,
        max: historyData.length,
        step: 1,
        orientation: "horizontal",
        value: historyData.length,
        slide: function (event, slideEvt) {
            historyVehicle.setMap(null);
            var index = slideEvt.value;
            //Set values based on slider
            $('#datetime').text(moment(historyData[index].timestamp).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm'));
            $('#lat').text(historyData[index].Lat);
            $('#lon').text(historyData[index].Lon);
            $('#dir').text(historyData[index].Direction);
            $('#speed').text(historyData[index].Speed);
            $('#gpsRecordNumb').text('# ' + index + ' of ' + historyData.length);

            //center map
            historyMap.setCenter({ lat: historyData[index].Lat, lng: historyData[index].Lon });

            //add marker
            historyVehicle = new google.maps.Marker({
                position: { lat: historyData[index].Lat, lng: historyData[index].Lon },
                draggable: false,
                map: historyMap
            });
        }
    });

    $('#historySlider').slider("value", 1);

    $('#playBackFrom_M').datetimepicker({
        dayOfWeekStart: 1
    });

    $('#playBackFrom_M').val(moment(start).add(moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm'));

    $('#vehicleHistoryLoader').addClass('hidden');
    $('#vehicleHistoryDiv').removeClass('hidden');

    $('#historySearch_M').click(function () {
        var from = moment(new Date($('#playBackFrom_M').val())).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm');
        var to = moment(from).add($('#increments_M').val(), 'hours').format('YYYY-MM-DD HH:mm');
        OpenPlayback($('#vehicleHistoryList_M').val(), from, to);
    });

    $('#historyDownload_M').click(function () {
        var from = moment(new Date($('#playBackFrom_M').val())).add(-1 * moment().utcOffset(), 'minutes').format('YYYY-MM-DD HH:mm');
        var to = moment(from).add($('#increments_M').val(), 'hours').format('YYYY-MM-DD HH:mm');
        dowloadVehicleHistory($('#vehicleHistoryList_M').val(), from, to);
    });
}

function OpenPlaybackError(result, error) {
    var err = error;
    alert('A problem occurred selecting and/or downloading the selected vehicle data, please reload or contact the administrator');
}
//#endregion

//#region getAvailableDrivers Functions
function getAvailableDrivers() { //Get a download of the vehicle for ID
    $('#availableDrivers').empty();
    var _url = 'getAvailableDrivers';
    var _data = "";
    $.ajax({
        type: "GET",
        dataType: "json",
        url: _url,
        data: _data,
        contentType: "application/json; charset=utf-8",
        success: getAvailableDriversSuccess,
        error: getAvailableDriversError
    });
}

function getAvailableDriversSuccess(data) {
    if (data.length > 0) {
        $('#availableDrivers').append($('<option>', { value: '' }).text('-- New Driver --'));
        for (var i = 0; i < data.length; i++) {
            var name = data[i].DriverFirstName + " " + data[i].DriverLastName;

            if (selectedVehicle.status == "NA") {
                $('#availableDrivers').prop('disabled', 'disabled')
                $('#availableDrivers').append($('<option>', { value: data[i].DriverID }).text(name));
                $('#availDriverText').html("<em>The selected vehicle is not active. Please assign drivers to inactive vehicles using the Drivers -> Vehicles Module inside administration.</em>")
            } else {
                $('#availableDrivers').prop('disabled', false)
                $('#availableDrivers').append($('<option>', { value: data[i].DriverID }).text(name));
                $('#availDriverText').html("<em>Changing a driver is permanent until you change drivers again.</em>")
            }
        }
    } else {
        $('#availDriverText').html("<em>There are no available drivers in teh sytem. Please add and assign drivers using the administration module.</em>")
    }
}

function getAvailableDriversError(result, error) {
    var err = error;
    alert('A problem occurred getting the pulling the avilable drivers, please reload or contact the administrator');
}
//#endregion

//#region changeDrivers Functions
function changeDrivers(from, to) { //Get a download of the vehicle for ID
    var _url = 'changeDrivers';
    var _data = "from=" + from + "&to=" + to + "&vehicleID=" + selectedVehicle.ID;
    $.ajax({
        type: "GET",
        dataType: "json",
        url: _url,
        data: _data,
        contentType: "application/json; charset=utf-8",
        success: changeDriversSuccess,
        error: changeDriversError
    });
}

function changeDriversSuccess(data) {
    if (data == "OK") {
        toastr.options = {
            "closeButton": false,
            "debug": false,
            "newestOnTop": false,
            "progressBar": false,
            "positionClass": "toast-bottom-left",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        }
        Command: toastr["success"]("New Driver Assigned. We need to refresh the map now.");
        selectedVehicle = null;
        closeNav();
        getVehicles(true);
    } else {
        alert('A problem occurred changing the drivers, please reload or contact the administrator');
    }
}

function changeDriversError(result, error) {
    var err = error;
    alert('A problem occurred changing the drivers, please reload or contact the administrator');
}
//#endregion




$(document).on('click', "#alertsTable > tbody > tr", function () {
    getAlert($(this).find("td.hidden").html());
});

var resetIcons = function () {
    for (var i = 0; i < vehicles.length; i++) {
        vehicles[i].Marker.setIcon({
            url: '../Content/Images/blue.png', // url
            scaledSize: new google.maps.Size(32, 50), // scaled size
            origin: new google.maps.Point(0, 0), // origin
            anchor: new google.maps.Point(16, 50) // anchor
        });
        vehicles[i].Marker.setAnimation(null);
    }
}

$('#vehicleList').on('change', function () {
    var listVehicle = null;
    for (var i = 0; i < vehicles.length; i++) {
        if (vehicles[i].ID == $(this).val()) {
            listVehicle = vehicles[i];
        }
    }
    selectThisVehicle(listVehicle, selectedVehicle);
    map.setZoom(15);
    map.setCenter(listVehicle.Marker.getPosition());
});

$('#openAlerts').click(function () {
    jsPanel.activePanels.getPanel("alertPanel").maximize();
});

function mapLogEntry(type, vehicle) {
    if (type == "position") {
        $('#log_panel').append("<strong>" + moment().format('MM/DD/YYYY hh:mm') + "</strong> -- " + vehicle.VehicleID + " -- Set Position(" + vehicle.lat + ", " + vehicle.lon + ")<br />");
    } else if (type == "selected") {
        $('#log_panel').append("<strong>" + moment().format('MM/DD/YYYY hh:mm') + "</strong> -- " + vehicle.VehicleID + " -- Selected for analysis<br />");
    } else if (type == "unselected") {
        $('#log_panel').append("<strong>" + moment().format('MM/DD/YYYY hh:mm') + "</strong> -- " + vehicle.VehicleID + " -- Unselected<br />");
    } else if (type == "alert") {
        $('#log_panel').append("<strong>" + moment().format('MM/DD/YYYY hh:mm') + "</strong> -- " + vehicle.VehicleID + " -- Alert: [" + vehicle.lat + ", " + vehicle.lon + "]<br />");
    }
}

function containsVehicle(ID) {
    var i;
    for (i = 0; i < vehicles.length; i++) {
        if (vehicles[i].ID == ID) {
            return true;
        }
    }

    return false;
}

function containsObject(obj, list) {
    var i;
    for (i = 0; i < list.length; i++) {
        if (list[i] === obj) {
            return true;
        }
    }

    return false;
}

function openNav() {
    document.getElementById("mySidenav").style.width = "45em";
    document.getElementById("map").style.marginright = "45em";
}

function closeNav() {
    //alert("closing nav");
    document.getElementById("mySidenav").style.width = "0";
    document.getElementById("map").style.marginLeft = "0";

    $('#collapseFive').collapse('hide')
    $('#info_panel').html('');
    $('#collapseOne').collapse('hide')
    $('#alert_panel').html('');
    $('#alertNumb').html('0');
    $('#collapseThree').collapse('hide')
    $('#playback_panel').html('');
    $('#collapseSix').collapse('hide');
    $('#collapseSixPanel').show();
    $('#collapseSevenPanel').show();
    $('#collapseEightPanel').show();

    //driver panel changes back... need to find more elegant way to do this
    $('#driverPic').html('<span class="glyphicons glyphicons-user-key" style="font-size: 150px;" id="driverIcon"></span>');
    $('#driverName').html('No Driver Assigned');
    $('#driverEmail').html('');
    $('#driverStatus').html('');
    $('#behaviorsDiv').hide();
    $('#collapseSix').collapse('hide');
    $('#availableDrivers').empty();
}

$('#closebtn').click(function () {
    closeNav();
});

function base64ArrayBuffer(arrayBuffer) {
    var base64 = ''
    var encodings = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'

    var bytes = new Uint8Array(arrayBuffer)
    var byteLength = bytes.byteLength
    var byteRemainder = byteLength % 3
    var mainLength = byteLength - byteRemainder

    var a, b, c, d
    var chunk

    // Main loop deals with bytes in chunks of 3
    for (var i = 0; i < mainLength; i = i + 3) {
        // Combine the three bytes into a single integer
        chunk = (bytes[i] << 16) | (bytes[i + 1] << 8) | bytes[i + 2]

        // Use bitmasks to extract 6-bit segments from the triplet
        a = (chunk & 16515072) >> 18 // 16515072 = (2^6 - 1) << 18
        b = (chunk & 258048) >> 12 // 258048   = (2^6 - 1) << 12
        c = (chunk & 4032) >> 6 // 4032     = (2^6 - 1) << 6
        d = chunk & 63               // 63       = 2^6 - 1

        // Convert the raw binary segments to the appropriate ASCII encoding
        base64 += encodings[a] + encodings[b] + encodings[c] + encodings[d]
    }

    // Deal with the remaining bytes and padding
    if (byteRemainder == 1) {
        chunk = bytes[mainLength]

        a = (chunk & 252) >> 2 // 252 = (2^6 - 1) << 2

        // Set the 4 least significant bits to zero
        b = (chunk & 3) << 4 // 3   = 2^2 - 1

        base64 += encodings[a] + encodings[b] + '=='
    } else if (byteRemainder == 2) {
        chunk = (bytes[mainLength] << 8) | bytes[mainLength + 1]

        a = (chunk & 64512) >> 10 // 64512 = (2^6 - 1) << 10
        b = (chunk & 1008) >> 4 // 1008  = (2^6 - 1) << 4

        // Set the 2 least significant bits to zero
        c = (chunk & 15) << 2 // 15    = 2^4 - 1

        base64 += encodings[a] + encodings[b] + encodings[c] + '='
    }

    return base64
}

$('#availableDrivers').change(function () {
    if (confirm('Change assigned driver for this vehicle to: ' + $('#availableDrivers option:selected').text() + '? This will reload all vehicles on the map.')) {
        if (selectedVehicle.driver != null) {
            if (selectedVehicle.driver.DriverID != "00000000-0000-0000-0000-000000000000") {
                changeDrivers(selectedVehicle.driver.DriverID, $(this).val());
            } else {

                changeDrivers(null, $(this).val());
            }
        } else {
            changeDrivers(null, $(this).val());
        }
    }
});
});
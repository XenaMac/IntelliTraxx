$(function () {
    var _alertName = '';

    $('[data-toggle="tooltip"]').tooltip()

    //#region Menu scripts

    $('#appsIcon').click(function () {
        openNav();
    });

    $('#mapIcon').click(function () {
        window.location.href = '../Fleet/index';
    });

    $('#alertsIcon').click(function () {
        window.location.href = '../Alerts/Index';
    });

    $('#geofenceIcon').click(function () { });

    $('#diagnosticsIcon').click(function () {
        window.location.href = '../Scheduling/Index';
    });

    //#endregion

    $('#filter').click(function () {
        getVehicleAlerts(_alertName);
    });

    $(document).on('click', "#alertsTableInPanel > tbody > tr", function () {
        jsPanel.activePanels.getPanel("alertPanel").close();
        //jsPanel.activePanels.getPanel("alertPanel").content.append("<object data=\'http://localhost/IntelliTraxx/Alerts/ViewAlert?alertID=" + $(this).find('input[name=alertID]').val() + "\' width=\'100%\' height=\'100%\'>");
    });

    getAlerts();
    getAllVehicles();

    //#region getAlerts Functions

    function getAlerts() {
        $('#byAlertDiv').html("<img src=\'../Content/Images/preloader.gif\' width=\'200\' class=\'center-block\'/>");

        var _url = 'getAllAlerts';
        var _data = "";

        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAlertsSuccess,
            error: getAlertsError
        });
    }

    function getAlertsSuccess(data) {
        var markup = "";
        var minVal = "";

        //set alerts panel content
        $('#byAlertDiv').html('<br /><br /><select id="byAlertDD" class="form-control"><option val="" selected>-- Select Alert --</option></select>');

        if (data.length == 0) {
            $('#byAlertDiv').html('There were no alerts found. Does that seem right to you?');
        } else {
            for (var a = 0; a < data.length; a++) {
                $('#byAlertDD').append($('<option>', { value:  data[a].AlertFriendlyName }).text(data[a].AlertFriendlyName));
            };
        }

        $('#byAlertDD').on('change', function () {
            var $this = $(this);
            getVehicleAlerts($this.val());
        })
    }

    function getAlertsError(data, error) {
        var err = error;
        alert('A problem occurred getting the alert, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion

    //#region getVehicleAlerts Functions

    function getVehicleAlerts(alertName) {
        var from = $('#datetimepicker').val().toString();
        var to = $('#toDatetimepicker').val().toString();
        from = moment(from, 'YYYY/MM/DD HH:mm').utc().format('MM/DD/YY HH:mm');
        to = moment(to, 'YYYY/MM/DD HH:mm').utc().format('MM/DD/YY HH:mm');

        $('#alertName').html(alertName);

        var _url = 'getAllAlertsByRangeByType';
        var _data = "?from=" + from + "&to=" + to + "&type=" + alertName;

        $('#alertsTable').html('<small><table id="alertTable" data-toggle="table" data-striped="true" data-query-params="queryParams" data-pagination="true" data-page-size="15" data-page-list="[5, 10, 20, 50]" data-classes="table table-hover table-condensed" data-sort-name="alertStart" data-sort-order="desc" data-search="true" data-show-refresh="true" data-show-toggle="true" data-show-columns="true" data-show-export="true" data-url="' + _url + _data + '"><thead><tr><th data-field="alertID" class="hidden">Alert ID</th><th data-halign="center" data-sortable="true" data-field="vehicleID" class="vehicleID">Vehicle</th><th data-halign="center" data-sortable="true" data-field="alertStart" data-formatter="dateFormat">Start</th><th data-halign="center" data-sortable="true" data-field="alertEnd" data-formatter="dateFormat">End</th><th data-halign="center" data-sortable="true" data-field="maxVal">Max Value</th></tr></thead><tbody></tbody></table></small><br />');

        $('#alertTable').bootstrapTable();
    }

    $(document).on('click', "#alertTable > tbody > tr", function () {
        getAlert($(this).find("td.hidden").html(), $(this).find("td.vehicleID").html());
    });

    function getAlert(_alertID, _vehicleID) {
        $.jsPanel({
            paneltype: 'modal',
            id: "alertViewPanel",
            position: { my: "center-top", at: "center-top", offsetY: 50, offsetX: 0 },
            headerTitle: "Alert View",
            headerLogo: '<span class=\'glyphicons glyphicons-light-beacon\' style=\'margin: 4px 0 0 4px;font-size:2.0rem;\'></span>&nbsp;',
            theme: "#D05100",
            headerControls: {
                maximize: 'remove',
                minimize: 'remove',
                normalize: 'remove',
                smallify: 'remove'
            },
            content: '<div id=\'alertLoader\' class=\'loading hidden\'><img src=\'../Content/Images/preloader.gif\' width=\'100\' /></div>' +
                        '<div id="specificAlertPane">' +
                        '<input type="hidden" id=alertID />' +
                        '<input type="hidden" id=vehicleID />' +
                        '<div class="col-md-12">' +
                        '<p id="alertDesc"></p>' +
                        '<div class="col-md-12 pull-left">' +
                        '<strong>As of: </strong><label id="LC" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Lat: </strong><label id="Lat" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Long: </strong><label id="Long" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Direction: </strong><label id="Dir" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Speed: </strong><label id="SPD" class="mapText_M"></label><br />' +
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
                        '<div class="col-md-12">' +
                        '<table id="gpsTable" data-toggle="table" data-striped="true" data-query-params="queryParams" data-pagination="true" data-page-size="15" data-page-list="[5, 10, 20, 50]" data-classes="table table-hover table-condensed" data-sort-name="timestamp" data-sort-order="asc" data-search="true" data-show-toggle="true" data-show-columns="true" data-show-export="true"><thead><tr><th data-sortable="true" data-field="VehicleID">Vehicle</th><th data-halign="center" data-sortable="true" data-field="timestamp" data-formatter="dateFormat">Date/Time</th><th data-halign="center" data-sortable="true" data-field="Lat">Lat</th><th data-halign="center" data-sortable="true" data-field="Lon" >Lon</th><th data-halign="center" data-sortable="true" data-field="Speed">Speed</th><th data-halign="center" data-sortable="true" data-field="Direction">Direction</th></tr></thead><tbody></tbody></table>' +
                        '</div>' +
                        '</div>',
            contentOverflow: 'scroll',
            contentSize: {
                width: function () { return $(window).width() / 1.5 },
                height: function () { return $(window).height() / 1.2 },
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
                    var _data = "alertID=" + _alertID;
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
                        $('#alertDesc').html("Vehicle <strong>" + data.vehicleID + "</strong> triggered the <strong>" + data.alertName + "</strong> alert which started at <strong>" + moment(data.alertStart).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm') + "</strong> and ended at <strong>" + moment(data.alertEnd).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm') + "</strong>.")
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

                        $('#gpsTable').bootstrapTable({});

                        $('#gpsTable').bootstrapTable('load', data.Locations);
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

    //#region getAllVehicles

    function getAllVehicles() {
        $('#byVehicleDiv').html("<img src=\'../Content/Images/preloader.gif\' width=\'200\' class=\'center-block\'/>");

        var _url = 'getAllVehicles';
        var _data = "loadHistorical=true";

        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAllVehiclesSuccess,
            error: getAllVehiclesError
        });
    }

    function getAllVehiclesSuccess(data) {
        var markup = "";

        //set alerts panel content
        $('#byVehicleDiv').html('<br /><br /><select id="byVehicleDD" class="form-control"><option val="" selected>-- Select Vehicle --</option></select>');

        if (data.length == 0) {
            $('#alertListGroup').html('No vehicles found. Does that seem right to you?');
        } else {
            for (var a = 0; a < data.length; a++) {
                $('#byVehicleDD').append($('<option>', { value: data[a].VehicleID }).text(data[a].VehicleID));
            };
        }

        $('#byVehicleDD').on('change', function () {
            var $this = $(this);
            getAllAlertsByRangeByVehicle($this.val());
        })
    }

    function getAllVehiclesError(data, error) {
        var err = error;
        alert('A problem occurred getting the alert vehicles, please reload or contact the administrator. Error: ' + error);
    }


    //#endregion

    //#region getAllAlertsByRangeByVehicle

    function getAllAlertsByRangeByVehicle(vehicleid) {
        var from = $('#datetimepicker').val().toString();
        var to = $('#toDatetimepicker').val().toString();
        from = moment(from, 'YYYY/MM/DD HH:mm').utc().format('MM/DD/YY HH:mm');
        to = moment(to, 'YYYY/MM/DD HH:mm').utc().format('MM/DD/YY HH:mm');

        $('#alertName').html(vehicleid);

        var _url = 'getAllAlertsByRangeByVehicle';
        var _data = "?from=" + from + "&to=" + to + "&vehicleid=" + vehicleid;

        $('#alertsTable').html('<small><table id="alertTable" data-toggle="table" data-striped="true" data-query-params="queryParams" data-pagination="true" data-page-size="15" data-page-list="[5, 10, 20, 50]" data-classes="table table-hover table-condensed" data-sort-name="alertStart" data-sort-order="desc" data-search="true" data-show-refresh="true" data-show-toggle="true" data-show-columns="true" data-show-export="true" data-url="' + _url + _data + '"><thead><tr><th data-field="alertID" class="hidden">Alert ID</th><th data-halign="center" data-sortable="true" data-field="alertName" class="vehicleID">Alert Name</th><th data-halign="center" data-sortable="true" data-field="alertStart" data-formatter="dateFormat">Start</th><th data-halign="center" data-sortable="true" data-field="alertEnd" data-formatter="dateFormat">End</th><th data-halign="center" data-sortable="true" data-field="maxVal">Max Value</th></tr></thead><tbody></tbody></table></small><br />');

        $('#alertTable').bootstrapTable();
    }

    $(document).on('click', "#alertTable > tbody > tr", function () {
        getAlert($(this).find("td.hidden").html(), $(this).find("td.vehicleID").html());
    });

    function getAlert(_alertID, _vehicleID) {
        $.jsPanel({
            paneltype: 'modal',
            id: "alertViewPanel",
            position: { my: "center-top", at: "center-top", offsetY: 50, offsetX: 0 },
            headerTitle: "Alert View",
            headerLogo: '<span class=\'glyphicons glyphicons-light-beacon\' style=\'margin: 4px 0 0 4px;font-size:2.0rem;\'></span>&nbsp;',
            theme: "#D05100",
            headerControls: {
                maximize: 'remove',
                minimize: 'remove',
                normalize: 'remove',
                smallify: 'remove'
            },
            content: '<div id=\'alertLoader\' class=\'loading\'><img src=\'../Content/Images/preloader.gif\' width=\'100\' /></div>' +
                        '<div id="specificAlertPane">' +
                        '<input type="hidden" id=alertID />' +
                        '<input type="hidden" id=vehicleID />' +
                        '<div class="col-md-12">' +
                        '<p id="alertDesc"></p>' +
                        '<div class="col-md-12 pull-left">' +
                        '<strong>As of: </strong><label id="LC" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Lat: </strong><label id="Lat" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Long: </strong><label id="Long" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Direction: </strong><label id="Dir" class="mapText_M"></label>&nbsp;&nbsp;' +
                        '<strong>Speed: </strong><label id="SPD" class="mapText_M"></label><br />' +
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
                        '<div class="col-md-12">' +
                        '<table id="gpsTable" data-toggle="table" data-striped="true" data-query-params="queryParams" data-pagination="true" data-page-size="15" data-page-list="[5, 10, 20, 50]" data-classes="table table-hover table-condensed" data-sort-name="timestamp" data-sort-order="asc" data-search="true" data-show-toggle="true" data-show-columns="true" data-show-export="true"><thead><tr><th data-sortable="true" data-field="VehicleID">Vehicle</th><th data-halign="center" data-sortable="true" data-field="timestamp" data-formatter="dateFormat">Date/Time</th><th data-halign="center" data-sortable="true" data-field="Lat">Lat</th><th data-halign="center" data-sortable="true" data-field="Lon" >Lon</th><th data-halign="center" data-sortable="true" data-field="Speed">Speed</th><th data-halign="center" data-sortable="true" data-field="Direction">Direction</th></tr></thead><tbody></tbody></table>' +
                        '</div>' +
                        '</div>',
            contentOverflow: 'scroll',
            contentSize: {
                width: function () { return $(window).width() / 1.5 },
                height: function () { return $(window).height() / 1.2 },
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
                    var _data = "alertID=" + _alertID;
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
                        $('#alertDesc').html("<h4>Vehicle <strong>" + data.vehicleID + "</strong> triggered the <strong>" + data.alertName + "</strong> alert which started at <strong>" + moment(data.alertStart).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm') + "</strong> and ended at <strong>" + moment(data.alertEnd).add(moment().utcOffset(), 'minutes').format('MM/DD/YYYY HH:mm') + "</strong></h4>.")
                        getAlertData();
                    }
                }

                function viewAlertError(result, error) {
                    //unhide loader
                    $('#alertLoader').hide();

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

                        $('#gpsTable').bootstrapTable({});

                        $('#gpsTable').bootstrapTable('load', data.Locations);
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

    $('#alertsAdmin').click(function () {
        window.location.href = '../Alerts/Admin';
    });

});
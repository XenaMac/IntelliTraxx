$(function () {
    var start = moment().startOf('month').format('YYYY-MM-DD hh:mm');
    var end = moment().endOf('month').format('YYYY-MM-DD hh:mm');
    var vehicleRouter = null;

    $('#startDtTm').datetimepicker();
    $('#startDtTm').val(start);
    $('#endDtTm').datetimepicker();
    $('#endDtTm').val(end);

    AIVehicles();
    getDriverAnalytics();
    getAlertsByRange(start, end);

    //#region Active/Inactive Vehicles
    function AIVehicles() {
        $('#vehicles').html("<img src=\'../Content/Images/preloader.gif\' width=\'200\' />");
        var _url = 'getAllVehicles';
        var _data = "loadHistorical=true";
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: AIVehiclesSuccess,
            error: AIVehiclesError
        });
    }

    function AIVehiclesSuccess(result) {
        var active = 0;
        var inactive = 0;
        if (result.length > 0) {
            $('#vehicles').html("");
            for (var i = 0; i < result.length; i++) {
                if (result[i].status.length > 0) {
                    inactive++;
                } else {
                    active++;
                }
            }
            Highcharts.getOptions().colors = Highcharts.map(Highcharts.getOptions().colors, function (color) {
                return {
                    radialGradient: {
                        cx: 0.5,
                        cy: 0.3,
                        r: 0.7
                    },
                    stops: [
                        [0, color],
                        [1, Highcharts.Color(color).brighten(-0.3).get('rgb')] // darken
                    ]
                };
            });
            $('#vehicles').highcharts({
                chart: {
                    borderColor: '#cecece',
                    borderRadius: 10,
                    borderWidth: 1,
                    type: 'pie',
                    options3d: {
                        enabled: true,
                        alpha: 45,
                        beta: 0
                    }
                },
                title: {
                    style: { "fontSize": "1.5em" },
                    text: 'Active/Inactive Vehicles:<br />' + moment().format("MM/DD/YYYY hh:mm")
                },
                tooltip: {
                    pointFormat: '{series.name}: <b>{point.y}</b>'
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        dataLabels: {
                            enabled: false,
                        }
                    }
                },
                series: [{
                    type: 'pie',
                    name: 'Vehicles',
                    data: [
                        ['Active', active],
                        {
                            name: 'Inactive',
                            y: inactive,
                            sliced: true,
                            selected: true
                        }
                    ]
                }]
            });

        } else {
            alert('A problem occurred getting the Active/Inactive Vehicles, please reload or contact the administrator.');
        }
    }

    function AIVehiclesError(result, error) {
        alert('A problem occurred getting the Active/Inactive Vehicles, please reload or contact the administrator. Error: ' + error.message);
    }
    //#endregion

    //#region Active/Inactive Vehicles
    function getDriverAnalytics() {
        $('#drivers').html("<img src=\'../Content/Images/preloader.gif\' width=\'200\' />");
        var _url = 'getDriverAnalytics';
        var _data = "";
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getDriverAnalyticsSuccess,
            error: getDriverAnalyticsError
        });
    }

    function getDriverAnalyticsSuccess(result) {
        $('#drivers').html("");
        $('#drivers').highcharts({
            chart: {
                type: 'pie',
                borderColor: '#cecece',
                borderRadius: 10,
                borderWidth: 1,
                options3d: {
                    enabled: true,
                    alpha: 45
                }
            },
            title: {
                text: 'Driver Status<br />' + moment().format("MM/DD/YYYY hh:mm")
            },
            subtitle: {
                text: ''
            },
            plotOptions: {
                pie: {
                    innerSize: 100,
                    depth: 65,
                    allowPointSelect: true,
                    dataLabels: {
                        enabled: false,
                    }
                }
            },
            series: [{
                name: 'Status',
                data: [
                    ['Driving', result.driving],
                    ['Not Driving', result.notDriving],
                    ['Unassigned', result.notAssigned],
                    ['Vehicles WO/Driver', result.vehiclesWithoutDrivers]
                ]
            }]
        });
    }

    function getDriverAnalyticsError(result, error) {
        alert('A problem occurred getting the Driver Analytics, please reload or contact the administrator. Error: ' + error.message);
    }
    //#endregion


    //#region Active/Inactive Vehicles
    function getAlertsByRange(start, end) {
        $('#alerts').html("<img src=\'../Content/Images/preloader.gif\' width=\'200\' />");
        var _url = 'getAlertsByRange';
        var _data = "start=" + start + "&end=" + end;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAlertsByRangeSuccess,
            error: getAlertsByRangeError
        });
    }

    function getAlertsByRangeSuccess(result) {
        $('#alerts').html("");
        var series = [];
        var drilldown = [];

        for (var i = 0; i < result.length; i++) {

            var SeriesObj = {
                'name': result[i].alertName,
                'y': 1,
                'drilldown': result[i].alertName
            };

            if (containsObject(SeriesObj, series)) {
                for (var a in series) {
                    if (series[a].name == result[i].alertName) {
                        series[a].y ++;
                        break; //Stop this loop, we found it!
                    }
                }
            } else {
                series.push(SeriesObj);
            }
        }

        Highcharts.chart('alerts', {
            chart: {
                type: 'column',
                options3d: {
                    enabled: true,
                    alpha: 45
                }
            },
            title: {
                text: 'Number of alerts between ' + start + " - " + end 
            },
            subtitle: {
                //text: 'Click the columns to drill down'
            },
            xAxis: {
                type: 'category'
            },
            yAxis: {
                title: {
                    text: 'Total alert type for the fleet'
                }

            },
            legend: {
                enabled: false
            },
            plotOptions: {
                series: {
                    borderWidth: 0,
                    dataLabels: {
                        enabled: true,
                        format: '{point.y}'
                    }
                }
            },

            tooltip: {
                headerFormat: '<span style="font-size:11px">{series.name}</span><br>',
                pointFormat: '<span style="color:{point.color}">{point.name}</span>: <b>{point.y}</b><br/>'
            },

            series: [{
                name: 'Alerts',
                colorByPoint: true,
                data: series
            }]
        });
    }

    function getAlertsByRangeError(result, error) {
        alert('A problem occurred getting the Alert Analytics, please reload or contact the administrator.');
    }
    //#endregion

    //reload alerts with date range
    $('#alertsReload').click(function () {
        start = $('#startDtTm').val();
        end = $('#endDtTm').val();

        getAlertsByRange(start, end);
    });

    //does array contain object function
    function containsObject(obj, list) {
        var i;
        for (i = 0; i < list.length; i++) {
            if (list[i].name == obj.name) {
                return true;
            }
        }

        return false;
    }

    $('#showVehicles').click(function () {
        $('#vehicleList').addClass('hidden');
        $('#vlPreloader').removeClass('hidden');
        getVehicleList();
        var month = moment().get('month') - 2;
        $('#vFromDate').datetimepicker({
            dayOfWeekStart: 1,
            minDate: '2017/' + month + '/1',
            maxDate: '+1970/01/01'//tomorrow is maximum date calendar
        });
        $('#vFromDate').val(moment().startOf('day').format('YYYY-MM-DD hh:mm'));
        $('#vToDate').datetimepicker({
            dayOfWeekStart: 1,
            minDate: '2017/' + month + '/1',
            maxDate: '+1970/01/01'//tomorrow is maximum date calendar
        });
        $('#vToDate').val(moment().endOf('day').format('YYYY-MM-DD hh:mm'));
    })

    //#region GetVehicleData Functions
    function getVehicleList() { //Get a download of the vehicle for ID
        var _url = 'getVehicleListMac';
        var _data = "";
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getVehicleListSuccess,
            error: getVehicleListError
        });
    }

    function getVehicleListSuccess(result) {
        if (result) {
            $('#vehicleList').empty();
            for (var i = 0; i < result.length; i++) {
                $("#vehicleList").append($('<option>', { value: result[i].macAddress }).text(result[i].vehicleID));
            }
            $('#vehicleList').removeClass('hidden');
            $('#vlPreloader').addClass('hidden');
        } else {
            alert('A problem occurred getting vehicles, please reload or contact the administrator');
        }
    }

    function getVehicleListError(result, error) {
        alert('A problem occurred getting vehicles, please reload or contact the administrator');
    }
    //#endregion

    $('#reload').click(function () {
        $("#vehiclePreloader").removeClass('hidden');
        $('#vehicleSummaries').slideUp();
        $('#communications').collapse('hide');
        getRouter($('#vehicleList').val());
    });

    //#region getECmRouter Information Functions
    function getRouter(macAddress) { //Get a download of the vehicle for ID
        var _url = 'getECMRouter';
        var _data = "macAddress=" + macAddress;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                getRouterSuccess(data, macAddress);
            },
            error: getRouterError
        });
    }

    function getRouterSuccess(data, macAddress) {
        if (data) {
            var accNum = data.account.split("/");
            getECMAccount(accNum[6]);

            if (data.mac == macAddress.toUpperCase()) {
                if (data.state == "offline") {
                    $('#routerIcon').removeClass("green");
                    $('#routerIcon').addClass("red");
                } else {
                    $('#routerIcon').removeClass("red");
                    $('#routerIcon').addClass("green");
                }
                $('#asset_id').html("<strong>Asset ID: </strong>" + data.asset_id);
                $('#name').html("<strong>Name: </strong>" + data.name);
                $('#description').html("<strong>Description: </strong>" + data.description);
                $('#full_product_name').html("<strong>Full Product Name: </strong>" + data.full_product_name);
                $('#state').html("<strong>State: </strong>" + data.state);
                $('#state_updated_at').html("<strong>State Update at : </strong>" + moment(data.state_updated_at).format("MM/DD/YYYY HH:mm"));
                $('#mac').html("<strong>MAC Address: </strong>" + data.mac);
                var tf = data.target_firmware.split("/");
                getTargetFirmware(tf[6]);
                var af = data.actual_firmware.split("/");
                getActualFirmware(af[6]);
                $('#created_at').html("<strong>Created at: </strong>" + moment(data.created_at).format("MM/DD/YYYY HH:mm"));
                $('#config_status').html("<strong>Config Status: </strong>" + data.config_status);

                getSignalStrength(data.id);
            }
        } else {
            alert('A problem occurred getting the router for this vehicle, please reload or contact the administrator');
        }
    }

    function getRouterError(result, error) {
        alert('A problem occurred getting the router for this vehicle, please reload or contact the administrator');
    }
    //#endregion

    //#region getECMAccount Information Functions
    function getECMAccount(num) { //Get a download of the vehicle for ID
        var _url = "getECMAccount";
        var _data = "accountNum=" + num;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getECMAccountSuccess,
            error: getECMAccountError
        });
    }

    function getECMAccountSuccess(result) {
        if (result) {
            $('#RC').html("Router Communications / " + result.name);
            $('#vehicleSummaries').slideDown();
        } else {
            alert('A problem occurred getting the Cradlepoint ECM Account for this vehicle, please reload or contact the administrator');
        }
    }

    function getECMAccountError(result, error) {
        alert('A problem occurred getting the Cradlepoint ECM Account for this vehicle, please reload or contact the administrator');
    }
    //#endregion

    //#region getECMSignalStrength Information Functions
    function getSignalStrength(id) { 
        $('#signalPreloader').removeClass('hidden');
        $('#signalTable').addClass('hidden');
        $("#signalTable tbody").html("");

        var _url = "getECMSignalStrength";
        var _data = "routerID=" + id;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getSignalStrengthSuccess,
            error: getSignalStrengthError
        });
    }

    function getSignalStrengthSuccess(result) {
        if (result) {
            for (var i = 0; i < result.length; i++) {
                var markup = "<tr>"

                if (result[i].connection_state == "connected") {
                    markup += "<td><i class=\"material-icons green\">cloud_done</i></td>";
                } else {
                    markup += "<td><i class=\"material-icons red\">error</i></td>";
                }

                if (result[i].signal_percent >= 0 && result[i].signal_percent <= 20) {
                    markup += "<td><img src=\"../Content/Images/20.png\" width=\"15\" /></td>";
                } else if (result[i].signal_percent > 21 && result[i].signal_percent <= 40) {
                    markup += "<td><img src=\"../Content/Images/40.png\" width=\"15\" /></td>";
                } else if (result[i].signal_percent > 41 && result[i].signal_percent <= 60) {
                    markup += "<td><img src=\"../Content/Images/60.png\" width=\"15\" /></td>";
                } else if (result[i].signal_percent > 61 && result[i].signal_percent <= 80) {
                    markup += "<td><img src=\"../Content/Images/80.png\" width=\"15\" /></td>";
                } else if (result[i].signal_percent > 81 && result[i].signal_percent <= 100) {
                    markup += "<td><img src=\"../Content/Images/100.png\" width=\"15\" /></td>";
                }

                markup += "<td>" + result[i].name + "</td>";
                markup += "<td>" + result[i].dbm + "</td>";
                markup += "<td>" + result[i].sinr + "% </td>";
                markup += "</tr>";

                $("#signalTable tbody").append(markup);
                $('#signalPreloader').addClass('hidden');
                $('#signalTable').removeClass('hidden');
            }
        } else {
            alert('A problem occurred getting the Cradlepoint router signal strength for this vehicle, please reload or contact the administrator');
        }
    }

    function getSignalStrengthError(result, error) {
        alert('A problem occurred getting the Cradlepoint router signal strength for this vehicle, please reload or contact the administrator');
    }
    //#endregion

    //#region getTragetFirware Information Functions
    function getTargetFirmware(id) {
        var _url = "getFirmware";
        var _data = "fw=" + id;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getTargetFirmwareSuccess,
            error: getTargetFirmwareError
        });
    }

    function getTargetFirmwareSuccess(result) {
        if (result) {
            $('#target_firmware').html("<strong>Target Firmware: </strong>" + result.version);
        } else {
            alert('A problem occurred getting the router target firmware, please reload or contact the administrator');
        }
    }

    function getTargetFirmwareError(result, error) {
        alert('A problem occurred getting the router target firmware, please reload or contact the administrator');
    }
    //#endregion

    //#region getActualFirware Information Functions
    function getActualFirmware(id) {
        var _url = "getFirmware";
        var _data = "fw=" + id;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getActualFirmwareSuccess,
            error: getActualFirmwareError
        });
    }

    function getActualFirmwareSuccess(result) {
        if (result) {
            $('#actual_firmware').html("<strong>Actual Firmware: </strong>" + result.version);
            $("#vehiclePreloader").addClass('hidden');
        } else {
            alert('A problem occurred getting the router actual firmware, please reload or contact the administrator');
        }
    }

    function getActualFirmwareError(result, error) {
        alert('A problem occurred getting the router actual firmware, please reload or contact the administrator');
    }
    //#endregion
});
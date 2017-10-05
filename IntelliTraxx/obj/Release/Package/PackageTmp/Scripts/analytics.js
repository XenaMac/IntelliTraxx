$(function () {
    var start = moment().startOf('month').format('YYYY-MM-DD HH:mm')
    var end = moment().endOf('month').format('YYYY-MM-DD HH:mm')
    var vehicleRouter = null
    var steps = 0
    var PIDS = []
    var MACID = null
    var MACAddress = null

    var month = moment().get('month') - 2;
    $('#startDtTm').datetimepicker({
        dayOfWeekStart: 1,
        minDate: '2017/' + month + '/1',
        maxDate: '+1970/01/01'//tomorrow is maximum date calendar
    });
    $('#startDtTm').val(start);
    $('#endDtTm').datetimepicker();
    $('#endDtTm').val(end);
    
    $('#PIDSTable').bootstrapTable({
        onPostBody: function () {
            alert("Table Ready")
            //$('#DGTable').removeClass('hidden')
            //$('#PIDSTableLoader').addClass('hidden')
        }
    })
    
    AIVehicles();
    getDriverAnalytics();
    getAlertsByRange(start, end);

    //#region Active/Inactive Vehicles
    function AIVehicles() {
        $('#vehicles').html("<img src=\'../Content/Images/preloader.gif\' width=\'200\' />");
        $('#vlPreloader').removeClass('hidden')
        $('#vehicleList').addClass('hidden')
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

            $('#vehicleList').empty();
            for (var i = 0; i < result.length; i++) {
                $("#vehicleList").append($('<option>', { value: result[i].extendedData.MACAddress + "|" + result[i].extendedData.ID }).text(result[i].VehicleID));
            }
            $('#vlPreloader').addClass('hidden')
            $('#vehicleList').removeClass('hidden')

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
        //getVehicleList();
        var month = moment().get('month') - 2;
        $('#vFromDate').datetimepicker({
            dayOfWeekStart: 1,
            minDate: '2017/' + month + '/1',
            maxDate: '+1970/01/01'//tomorrow is maximum date calendar
        });
        $('#vFromDate').val(moment().startOf('day').format('YYYY-MM-DD HH:mm'));
        $('#vToDate').datetimepicker({
            dayOfWeekStart: 1,
            minDate: '2017/' + month + '/1',
            maxDate: '+1970/01/01'//tomorrow is maximum date calendar
        });
        $('#vToDate').val(moment().endOf('day').format('YYYY-MM-DD HH:mm'));
    })
    
    $('#reload').click(function () {
        $("#vehiclePreloader").removeClass('hidden');
        $('#diagnostics').collapse('hide');
        $('#vehicleSummaries').slideUp();
        $('#communications').collapse('hide');
        $('#showCommunciations').removeClass('hidden');
        MACID = $('#vehicleList').val().split("|");
        MACAddress = formatMac(MACID[0])
        getRouter(MACAddress)
        getPIDSByDateRange(MACID[1], $('#vFromDate').val(), $('#vToDate').val());
        getTripsByDate($('#vehicleList option:selected').text(), $('#vFromDate').val(), $('#vToDate').val());
    });

    //#region getECmRouter Information Functions
    function getRouter(macAddress) { //Get a download of the vehicle for ID
        $('#EM').addClass("hidden");
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
                    $('#RC').removeClass("green");
                    $('#RC').addClass("red");
                } else {
                    $('#routerIcon').removeClass("red");
                    $('#routerIcon').addClass("green");
                    $('#RC').removeClass("red");
                    $('#RC').addClass("green");
                }
                $('#asset_id').html("<strong>Asset ID: </strong>" + data.asset_id);
                $('#name').html("<strong>Name: </strong>" + data.name);
                $('#description').html("<strong>Description: </strong>" + data.description);
                $('#full_product_name').html("<strong>Full Product Name: </strong>" + data.full_product_name);
                $('#state').html("<strong>State: </strong>" + data.state);
                $('#state_updated_at').html("<strong>State Update at : </strong>" + moment(data.state_updated_at).format("MM/DD/YYYY HH:mm"));
                $('#mac').html("<strong>MAC Address: </strong>" + data.mac);
                if (data.target_firmware != null) {
                    var tf = data.target_firmware.split("/");
                    getTargetFirmware(tf[6]);
                    var af = data.actual_firmware.split("/");
                    getActualFirmware(af[6]);
                } else {
                    $('#target_firmware').html("<strong>Target Firmware: </strong> NULL from ECM");
                    $('#actual_firmware').html("<strong>Actual Firmware: </strong> NULL from ECM");
                }
                $('#created_at').html("<strong>Created at: </strong>" + moment(data.created_at).format("MM/DD/YYYY HH:mm"));
                $('#config_status').html("<strong>Config Status: </strong>" + data.config_status);


                $("#commPanel").addClass("panel-warning")
                $("#commPanel").removeClass("panel-default")
                $("#commLink").removeClass('hidden');
                $("#commText").addClass('hidden');

                getSignalStrength(data.id);
                getDataUsage(data.id);

                stepCheck(1);
            }
        } else {
            $("#commPanel").removeClass("panel-warning")
            $("#commPanel").addClass("panel-default")
            $('#commInfo').html("--")
            $("#commLink").addClass('hidden');
            $("#commText").removeClass('hidden');
            $('#showCommunications').addClass('hidden');
            stepCheck(100);
            //alert('A problem occurred getting the router for this vehicle, please reload or contact the administrator');
        }
    }

    function getRouterError(result, error) {
        $("#commPanel").removeClass("panel-warning")
        $("#commPanel").addClass("panel-default")
        $("#vehiclePreloader").addClass("hidden")
        $('#commInfo').html("--")
        $("#commLink").addClass('hidden');
        $("#commText").removeClass('hidden');
        $('#showCommunciations').addClass('hidden');
        stepCheck(100);
        //alert('A problem occurred getting the router for this vehicle, please reload or contact the administrator');
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
            $('#RC').html("Router Communications / " + result.name + "<br /><br />");
            stepCheck(1);
        } else {
            alert('A problem occurred getting the Cradlepoint ECM Account for this vehicle, please reload or contact the administrator');
            stepCheck(0);
        }
    }

    function getECMAccountError(result, error) {
        alert('A problem occurred getting the Cradlepoint ECM Account for this vehicle, please reload or contact the administrator');
        stepCheck(0);
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
                stepCheck(1);
            }
        } else {
            alert('A problem occurred getting the Cradlepoint router signal strength for this vehicle, please reload or contact the administrator');
            stepCheck(0);
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
            stepCheck(1);
        } else {
            alert('A problem occurred getting the router target firmware, please reload or contact the administrator');
            stepCheck(0);
        }
    }

    function getTargetFirmwareError(result, error) {
        alert('A problem occurred getting the router target firmware, please reload or contact the administrator');
        stepCheck(0);
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
            stepCheck(1);
        } else {
            alert('A problem occurred getting the router actual firmware, please reload or contact the administrator');
            stepCheck(0);
        }
    }

    function getActualFirmwareError(result, error) {
        alert('A problem occurred getting the router actual firmware, please reload or contact the administrator');
        stepCheck(0);
    }
    //#endregion

    //#region getNetDeviceMetrics Information Functions
    function getDataUsage(routerid) {
        var _url = "getDataUsage";
        var _data = "routerID=" + routerid;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getDataUsageSuccess,
            error: getDataUsageError
        });
    }

    function getDataUsageSuccess(result) {
        if (result.length != 0) {
            var total = 0
            total += result[0].bytes_in;
            total += result[0].bytes_out;
            total = formatBytes(total, 0);

            $('#commInfo').html(total);
            $('#data_Usage').html(total);
            $('#usageDate').html("As of last router update to ECM: " + moment(result[0].update_ts).format("MM/DD/YYYY HH:mm"));

            $('#signalPreloader').addClass('hidden');
            $('#signalTable').removeClass('hidden');
            stepCheck(1);
        } else {
            $('#commInfo').html("Null");
            $('#data_Usage').html("Null");
            $('#usageDate').html("As of last router update to ECM: NULL");

            $('#signalPreloader').addClass('hidden');
            $('#signalTable').removeClass('hidden');

            $('#routerIcon').removeClass("green");
            $('#routerIcon').addClass("red");
            $('#RC').removeClass("green");
            $('#RC').addClass("red");
            stepCheck(1);
        }
    }

    function getDataUsageError(result, error) {
        alert('A problem occurred getting the router data usage, please reload or contact the administrator');
        stepCheck(0);
    }
    //#endregion   

    //#region getNetDeviceMetrics Information Functions
    function getPIDSByDateRange(VehicleID, from, to) {
        var _url = "getPIDSByDateRange";
        var _data = "VehicleID=" + VehicleID + "&from=" + from + "&to=" + to;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                getPIDSByDateRangeSuccess(result, VehicleID, from, to);
            },
            error: getPIDSByDateRangeError
        });
    }

    function getPIDSByDateRangeSuccess(result, VehicleID, from, to) {
        if (result.length != 0) {
            var codes = [];
            $('#PIDList').empty();

            for (var i = 0; i < result.length; i++) {
                if (!containsObject(result[i].name, codes)) {
                    codes.push(result[i].name)
                    $('#PIDList').append($('<option>', { value: result[i].name }).text(result[i].name));
                }
            }
            
            $('#PIDList').multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                buttonClass: 'btn btn-info',
                numberDisplayed: 50,
                buttonWidth: '300px',
                onChange: function (option, checked, select) {
                    getOBDByDateRange($('#PIDList').val(), VehicleID, from, to);
                },
                onSelectAll: function (option, checked, select) {
                    getOBDByDateRange($('#PIDList').val(), VehicleID, from, to);
                },
                onDeselectAll: function () {
                    getOBDByDateRange($('#PIDList').val(), VehicleID, from, to);
                }
            });

            $('#PIDList').multiselect('select', codes[0]);

            getOBDByDateRange(codes[0], VehicleID, from, to);

            $("#OBDPanel").addClass("panel-info")
            $("#OBDPanel").removeClass("panel-default")
            $('#OBDInfo').html(result.length)
            $("#OBDLink").removeClass('hidden')
            $("#OBDText").addClass('hidden')
            $('#showDiagnostics').removeClass('hidden')
            stepCheck(1);
        } else {
            $("#OBDPanel").removeClass("panel-info")
            $("#OBDPanel").addClass("panel-default")
            $('#OBDInfo').html("--")
            $("#OBDLink").addClass('hidden')
            $("#OBDText").removeClass('hidden')
            $('#showDiagnostics').addClass('hidden')
            stepCheck(1)
        }
    }

    function getPIDSByDateRangeError(result, error) {
        alert('A problem occurred getting the OBD PIDs, please reload or contact the administrator');
        stepCheck(0);
    }
    //#endregion

    //#region getNetDeviceMetrics Information Functions
    function getOBDByDateRange(PID, VehicleID, from, to) {
        $('#DGTable').addClass('hidden')
        $('#DGChart').addClass('hidden')
        $('#PIDSTableLoader').removeClass('hidden')
        var _url = "getOBDByDateRange";
        var _data = "PID=" + PID + "&VehicleID=" + VehicleID + "&from=" + from + "&to=" + to;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getOBDByDateRangeSuccess,
            error: getOBDByDateRangeError
        });
    }

    function getOBDByDateRangeSuccess(result) {
        if (result.length != 0) {
            $('#PIDSTable').bootstrapTable('removeAll');

            for (var i = 0; i < result.length; i++) {
                var ts = moment.utc(result[i].timestamp).format("MM/DD/YYYY h:mm:ss")
                $('#PIDSTable').bootstrapTable('insertRow', {
                    index: i,
                    row: {
                        0: result[i].ID,
                        1: result[i].name,
                        2: result[i].val,
                        3: ts
                    }
                });

                if (i == result.length - 1) {
                    $('#DGTable').removeClass('hidden')
                    $('#DGChart').removeClass('hidden')
                    $('#PIDSTableLoader').addClass('hidden')
                }
            }

            //chart data elements
            var ts0 = moment.utc(result[0].timestamp).format("MM/DD/YYYY h:mm:ss")
            var ts1 = moment.utc(result[result.length - 1].timestamp).format("MM/DD/YYYY h:mm:ss")
            var objects = [];
            $.each(result, function (index, value) {
                if (!containsPid(value, objects)) {
                    objects.push({
                        'name': value.name,
                        'data': [[moment.utc(value.timestamp).format("MM/DD h:mm:ss"), parseInt(value.val)] ]
                    });
                } else {
                    var PID = objects.filter(function (obj) {
                        return obj.name ==value.name;
                    });

                    PID[0].data.push([moment.utc(value.timestamp).format("MM/DD h:mm:ss"), parseInt(value.val)] );
                }
                
            });

            console.log(objects);

            Highcharts.chart('PIDChart', {
                chart: {
                    type: 'spline'
                },
                title: {
                    text: 'Diagnostics for ' + $('#vehicleList option:selected').text()
                },
                subtitle: {
                    text: ts0 + " - " + ts1
                },
                xAxis: {
                    type: 'category'
                },
                yAxis: {
                    title: {
                        text: 'PID Value'
                    },
                    min: 0
                },
                tooltip: {
                    formatter: function () {
                        console.log(this);
                        var txt = this.y + ' on: ' + this.key;                        
                        return txt;
                    }

                },
                plotOptions: {
                    spline: {
                        marker: {
                            enabled: true
                        }
                    }
                },
                series: objects
            });
        } else {
            $('#PIDSTable').bootstrapTable('removeAll');
            $('#DGTable').removeClass('hidden')
            $('#PIDSTableLoader').addClass('hidden')
            //alert('A problem occurred getting the diagnostic data, please reload or contact the administrator');
        }
    }

    function getOBDByDateRangeError(result, error) {
        alert('A problem occurred getting the diagnostic data, please reload or contact the administrator');
    }
    //#endregion

    //toggle table
    $('#tableToggle').on('click', function () {
        $('#DGTable').toggleClass("hidden");
    })

    //toggle chart
    $('#chartToggle').on('click', function () {
        $('#DGChart').toggleClass("hidden");
    })

    //#region getNetDeviceMetrics Information Functions
    function getTripsByDate(VehicleID, start, end) {
        var _url = "getTripsByDate";
        var _data = "ID=" + VehicleID + "&start=" + start + "&end=" + end;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                getTripsByDateSuccess(result, VehicleID, start, end);
            },
            error: getTripsByDateError
        });
    }

    function getTripsByDateSuccess(result, VehicleID, start, to) {
        var trip = 1;
        if (result.length != 0) {
            for (var i = 1; i < result.length; i++) {
                //if(result[i-1])
            }
        }
        else {

        }
    }

    function getTripsByDateError(result, error) {
        //alert('A problem occurred getting the Vehicle Trips data, please reload or contact the administrator');
        stepCheck(0);
    }
    //#endregion

    //--------------------------------------------------------------------------//

    //bytes conversion function
    function formatBytes(a, b) {
        if (0 == a) return "0 Bytes";

        var c = 1e3,
            d = b || 2,
            e = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"],
            f = Math.floor(Math.log(a) / Math.log(c));

        return parseFloat((a / Math.pow(c, f)).toFixed(d)) + " " + e[f]
    }

    //show the divs function
    function stepCheck(step)
    {
        if (step != 0) {
            steps += step
        } else {
            steps = 0
        }

        if (steps >= 7) {
            $('#vehicleSummaries').slideDown()
            $("#vehiclePreloader").addClass('hidden')
            steps = 0
        }
    }

    //return formatted mac address
    function formatMac(maccaddress) {        
        return maccaddress.toString(16)             // "4a8926c44578"
            .match(/.{1,2}/g)    // ["4a", "89", "26", "c4", "45", "78"]
            .join(':')  
    }

    //is object in array
    function containsObject(obj, list) {
        var i;
        for (i = 0; i < list.length; i++) {
            if (list[i] === obj) {
                return true;
            }
        }

        return false;
    }

    //PID IN Array
    function containsPid(obj, list) {
        var i;
        for (i = 0; i < list.length; i++) {
            if (list[i].name === obj.name) {
                return true;
            }
        }

        return false;
    }

});
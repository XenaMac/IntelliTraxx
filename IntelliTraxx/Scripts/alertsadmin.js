$(function () {
    var alertName;
    var alertClassID;
    var alertClassName;
    var alertStart;
    var alertEnd;
    var polygonIDs = [];
    var polygonNames = [];
    var polygonAlerts = ['STATIONARY', 'ENTER POLYGON', 'EXIT POLYGON']
    var vehicleOnlyAlerts = ['LAST POSITION', 'SPEEDING', 'STARTUP']
    var valueAlerts = ['SPEEDING', 'STATIONARY']
    var alertVehicles = [];
    var alertValue = null;
    var copy = '';
    var editAlertID = null;

    $('[data-toggle="tooltip"]').tooltip()

    //#region Menu scripts

    $('#mapIcon').click(function () {
        window.location.href = '../Fleet/index';
    });

    $('#alertsIcon').click(function () {
        window.location.href = '../Alerts/Index';
    });

    $('#diagnosticsIcon').click(function () {
        window.location.href = '../Scheduling/Index';
    });

    //#endregion

    getAlerts();

    //#region getAlerts Functions

    function getAlerts() {
        $('#alertsDiv').html('<img src="../Content/Images/preloader.gif" width="200" class="center-block"/>');

        var _url = 'getAllAlerts';
        var _data = "";

        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                getAlertsSuccess(data, _url, _data);
            },
            error: getAlertsError
        });
    }

    function getAlertsSuccess(data, _url, _data) {
        if (data.length == 0) {
            $('#alertsDiv').html('There were no alerts found. Does that seem right to you?');
        } else {
            $('#alertsDiv').html('<div id="toolbar" class="btn-group"><button id="newAlert" type="button" class="btn btn-success"><i class="glyphicons glyphicons-plus"></i>Create Alert</button></div><table id="coAlertsTable" data-toggle="table" data-striped="true" data-query-params="queryParams" data-pagination="true" data-page-size="15" data-page-list="[5, 10, 20, 50]" data-classes="table table-hover table-condensed" data-sort-name="alertStart" data-sort-order="desc" data-search="true" data-show-refresh="true" data-show-toggle="true" data-show-columns="true" data-toolbar="#toolbar" data-show-export="true" data-url="' + _url + _data + '"><thead><tr><th data-field="AlertID" class="hidden">Alert ID</th><th data-halign="center" data-sortable="true" data-field="AlertFriendlyName">Alert</th><th data-halign="center" data-sortable="true" data-field="AlertClassName">Alert Class</th><th data-halign="center" data-sortable="true" data-field="AlertActive" data-formatter="toggler" class="text-center">Enabled</th><th data-halign="center" data-sortable="true" data-field="AlertStartTime" data-formatter="dateFormat">Start</th><th data-halign="center" data-sortable="true" data-field="AlertEndTime" data-formatter="dateFormat">End</th><th data-halign="center" data-sortable="true" data-field="minVal">Min Value</th><th data-halign="center" data-sortable="true" data-field="minVal" data-formatter="editBtn" class="text-center">Edit</th></tr></thead><tbody></tbody></table><br />');

            $('#coAlertsTable').bootstrapTable({
                onLoadSuccess: function () {
                    $('.tog').bootstrapToggle({
                        on: 'Enabled',
                        off: 'Disabled'
                    });

                    $(".tog").change(function () {
                        if (this.checked) {
                            changeAlertStatus($(this).attr('id'), true, true);
                        } else {
                            changeAlertStatus($(this).attr('id'), false, true);
                        }
                    });

                    $('.editAlertBtn').click(function () {
                        $(this).html('<img src=\'../Content/Images/preloader.gif\' width=\'25\' />')
                        getAlert($(this).attr('id'));
                    });
                },
            });


            $('#newAlert').click(function () {
                getAlertClasses();
            });
        }
    }

    function getAlertsError(data, error) {
        var err = error;
        alert('A problem occurred retrieving the alerts, please reload or contact the administrator. Error: ' + error);
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

    //#region getAlertClasses

    function getAlertClasses() {
        $('#alertClassDiv').html("");
        var _url = 'getAlertClasses';
        var _data = ''
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAlertClassesSuccess,
            error: getAlertClassesError
        });
    }

    function getAlertClassesSuccess(data) {
        if (data.length > 0) {
            for (var i = 0; i < data.length; i++) {
                var markup;

                switch (data[i].AlertClassName) {
                    case "LAST POSITION":
                        markup = '<div class="col-lg-3"><div class="panel panel-info text-center classTN "><div class="panel-heading"><strong>' + data[i].AlertClassName + '</strong></div><div class="panel-body next" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '"><i class="material-icons md-48">all_out</i><br><small>Vehicle will trigger an alert upon startup, if last known position was inside a polygon.</small></div></div></div>'
                        break;
                    case "SPEEDING":
                        markup = '<div class="col-lg-3"><div class="panel panel-info text-center classTN"><div class="panel-heading"><strong>' + data[i].AlertClassName + '</strong></div><div class="panel-body next"  id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '"><i class="material-icons md-48">network_check</i><br><small>Vehicle will trigger an alert upon startup, if last known position was inside a polygon.</small></div></div></div>'
                        break;
                    case "STATIONARY":
                        markup = '<div class="col-lg-3"><div class="panel panel-info text-center classTN"><div class="panel-heading"><strong>' + data[i].AlertClassName + '</strong></div><div class="panel-body next" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '"><i class="material-icons md-48">gps_fixed</i><br><small>Vehicle will trigger an alert upon startup, if last known position was inside a polygon.</small></div></div></div>'
                        break;
                    case "STARTUP":
                        markup = '<div class="col-lg-3"><div class="panel panel-info text-center classTN"><div class="panel-heading"><strong>' + data[i].AlertClassName + '</strong></div><div class="panel-body next" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '"><i class="material-icons md-48">power_settings_new</i><br><small>Vehicle will trigger an alert upon startup, if last known position was inside a polygon.</small></div></div></div>'
                        break;
                    case "ENTER POLYGON":
                        markup = '<div class="col-lg-3"><div class="panel panel-info text-center classTN"><div class="panel-heading"><strong>' + data[i].AlertClassName + '</strong></div><div class="panel-body next" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '"><i class="material-icons md-48">input</i><br><small>Vehicle will trigger an alert upon startup, if last known position was inside a polygon.</small></div></div></div>'
                        break;
                    case "EXIT POLYGON":
                        markup = '<div class="col-lg-3"><div class="panel panel-info text-center classTN"><div class="panel-heading"><strong>' + data[i].AlertClassName + '</strong></div><div class="panel-body next" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '"><i class="material-icons md-48">open_in_new</i><br><small>Vehicle will trigger an alert upon startup, if last known position was inside a polygon.</small></div></div></div>'
                        break;
                    default:
                        markup = "";
                }
                $('#alertClassDiv').append(markup);
            }

            $('#myModal').modal('show');
            $('[href=\\#step1]').tab('show');

            $('.next').click(function () {
                if ($('#alertName').val() == '') {
                    $('#alertName').attr('style', "border-radius: 5px; border:#FF0000 1px solid; ::-webkit-input-placeholder { color: red; }");
                    $('#alertName').attr('placeholder', "Alert Name Required");
                } else {
                    alertClassID = $(this).attr('id');
                    alertClassName = $(this).attr('name');
                    alertName = $('#alertName').val();
                    alertStart = $('#startDate').val();
                    alertEnd = $('#endDate').val();

                    if (isInArray(polygonAlerts, alertClassName)) {
                        var nextId = $(this).parents('.tab-pane').next().attr("id");
                        $(this).parents('.thumbnail').css("background-color", "#81B944");
                        getPolygons();
                        $('[href=\\#' + nextId + ']').tab('show');
                    } else {
                        $('#step2well').html('Polygons can not be used for the alert class.');
                        $('[href=\\#step3]').tab('show');
                        getAllVehicles();
                    }
                }

                return false;
            })

            $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
                //update progress
                var step = $(e.target).data('step');
                var percent = (parseInt(step) / 5) * 100;

                $('.progress-bar').css({ width: percent + '%' });
                $('.progress-bar').text("Step " + step + " of 5");

                //e.relatedTarget // previous tab
            })
        } else {
            alert('A problem occurred pulling the alert classes, please reload or contact the administrator.');
        }
    }

    function getAlertClassesError(result, error) {
        var err = error;
        alert('A problem occurred pulling the alert classes, please reload or contact the administrator.');
    }

    //#endregion

    //#region getPolygons

    function getPolygons() {
        $('#step2well').html('<div class="row" id="thumbnails"></div>');
        var _url = 'GetAllFences';
        var _data = ''
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getPolygonsSuccess,
            error: getPolygonsError
        });
    }

    function getPolygonsSuccess(data) {
        if (data.length > 0) {
            $('#thumbnails').append('&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<input class="" id="allPolys" type="checkbox" data-toggle="toggle"><hr>');

            for (var i = 0; i < data.length; i++) {
                var markup = '';
                switch (data[i].geoType) {
                    case "circle":
                        markup += '<div class="col-sm-6 col-md-4"><div class="thumbnail text-center"><i class="material-icons md-48">adjust</i><div class="caption"><h3>' + data[i].polyName + '</h3><p>' + data[i].notes + '</p><p><input class="pgSelect" id="' + data[i].geoFenceID + '" name="' + data[i].polyName + '" type="checkbox" data-toggle="toggle" data-onstyle="success" data-size="small"></p></div></div></div>';
                        break;
                    case "polygon":
                        markup += '<div class="col-sm-6 col-md-4"><div class="thumbnail text-center""><i class="material-icons md-48">center_focus_strong</i><div class="caption"><h3>' + data[i].polyName + '</h3><p>' + data[i].notes + '</p><p><input class="pgSelect" id="' + data[i].geoFenceID + '" name="' + data[i].polyName + '" type="checkbox" data-toggle="toggle" data-onstyle="success" data-size="small"></p></div></div></div>';
                        break;
                    default:
                        markup += "";
                }
                $('#thumbnails').append(markup);
            }

            $('#allPolys').bootstrapToggle({
                on: 'Select All',
                off: 'Single Select',
                width: '100px',
                onstyle: 'info',
                size: 'small'
            });

            $("#allPolys").change(function () {
                if (this.checked) {
                    $('.pgSelect').each(function () {
                        $(this).bootstrapToggle('on');
                    });
                } else {
                    $('.pgSelect').each(function () {
                        $(this).bootstrapToggle('off');
                    });
                }
            });

            $('.pgSelect').bootstrapToggle({
                on: 'Yes',
                off: 'No'
            });

            $('.nnext').click(function () {
                polygonIDs = [];
                polygonNames = [];
                $(".pgSelect").each(function () {
                    if (this.checked) {
                        polygonIDs.push({ id: $(this).attr('id') });
                        polygonNames.push({ name: $(this).attr('name') });
                    }
                });

                if (polygonIDs.length != 0) {
                    var nextId = $(this).parents('.tab-pane').next().attr("id");
                    $('[href=\\#' + nextId + ']').tab('show');
                    getAllVehicles();
                } else {
                    alert("Please choose one or more polygons for this alert.")
                }

                return false;
            })
        } else {
            alert('A problem occurred pulling the polygon information, please reload or contact the administrator.');
        }
    }

    function getPolygonsError(result, error) {
        var err = error;
        alert('A problem occurred pulling the polygon information, please reload or contact the administrator.');
    }

    //#endregion

    //#region gelAllVehicles(true)

    function getAllVehicles() {
        $('#vehicTable').html('');
        var _url = 'getAllVehicles';
        var _data = 'loadHistorical=true'
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
        if (data.length > 0) {
            $('#vehicTable').html('<button class="btn btn-info btn-xs" data-toggle="button" id="selectAll">Select All</button><br /><small><table id="alertsVehicleTable" class="table table-responsive table-striped table-condensed"><thead><tr><td class="hidden">VehicleID</td><td class="header">Assign</td><td class="header">Vehicle ID</td><td class="header text-center">Alert E-mail(s)</td></tr></thead><tbody></tbody></table></small>');

            //set content within table
            $("#alertsVehicleTable tbody").html('');

            for (var i = 0; i < data.length; i++) {
                var markup = '<tr><td class="hidden">' + data[i].VehicleID + '</td><td class="text-center"><input id="tb_' + data[i].extendedData.ID + '" name="' + data[i].extendedData.VehicleFriendlyName + '" class="vehicleActive" type="checkbox" data-toggle="toggle" data-size="mini"></td><td class="text-center">' + data[i].extendedData.VehicleFriendlyName + '</td><td class="text-center"><input type="text" id="em_' + data[i].extendedData.ID + '" class="emailbox" size="75" placeholder="email1@url.com; email2@url.com; blank if none">&nbsp;<button id="' + data[i].extendedData.ID + '" class="btn btn-primary btn-xs assign glyphicons glyphicons-copy" data-toggle="tooltip" data-placement="top" title="Copy to All E-mail Fields"/></td></tr>';

                $("#alertsVehicleTable tbody").append(markup);
            }

            $('[data-toggle="tooltip"]').tooltip()

            $("[class='vehicleActive']").bootstrapToggle();

            $('#selectAll').click(function () {
                if ($(this).text() == "Unselect All") {
                    $(".vehicleActive").bootstrapToggle('off');
                    $(this).text('Select All')
                } else {
                    $(".vehicleActive").bootstrapToggle('on');
                    $(this).text('Unselect All')
                }
            })

            $('.assign').click(function () {
                var recordId = $(this).attr("id");
                $('.emailbox').val($('#em_' + recordId).val());
            })

            $('.nnnext').click(function () {
                alertVehicles = [];

                $('.vehicleActive').each(function () {
                    if (this.checked) {
                        alertVehicles.push({
                            id: $(this).attr('id').substring(3, $(this).attr('id').length),
                            name: $(this).attr('name'),
                            email : "EMAIL:" + $('#em_' + $(this).attr('id').substring(3, $(this).attr('id').length)).val()
                        });
                    }
                });

                if (alertVehicles.length == 0) {
                    alert("Please choose at least one vehicle for this alert");
                } else {
                    if (isInArray(valueAlerts, alertClassName)) {
                        loadValuesPane(alertClassName);
                    } else {
                        $('[href=\\#step5]').tab('show');
                        reviewAndSubmit();
                    }
                }

                return false;
            })

        } else {
            alert('A problem occurred pulling all vehicles, please reload or contact the administrator.');
        }
    }

    function getAllVehiclesError(result, error) {
        var err = error;
        alert('A problem occurred pulling the all vehicles, please reload or contact the administrator.');
    }

    //#endregion

    //#region  loadValuesPane()

    function loadValuesPane(alertClassName) {
        switch (alertClassName) {
            case "SPEEDING":
                $('#step4Div').append('<div class="form-group"><label for="maxValue">Maximum Speeding Value: </label><input type="number" class="form-control" id="alertValue" value="65"></div>');
                break;
            case "STATIONARY":
                $('#step4Div').append('<div class="form-group"><label for="maxValue">Maximum Stationary Time (minutes): </label><input type="number" class="form-control" id="alertValue" value="15"></div>');
                break;
            default:
                $('#step4Div').append('The Alert Classes chosen does not require a max or min value.');
        }

        $('[href=\\#step4]').tab('show');

        $('.nnnnext').click(function () {
            alertValue = $('#alertValue').val();
            var nextId = $(this).parents('.tab-pane').next().attr("id");
            $('[href=\\#' + nextId + ']').tab('show');
            reviewAndSubmit();

            return false;
        })
    }

    //#endregion

    //#region reviewAndSubmit()

    function reviewAndSubmit() {
        var markup = '';

        switch (alertClassName) {
            case "LAST POSITION":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                if (startDate != null && endDate != null) {
                    markup += '<p class="lead">Start Date/Time: <strong>' + alertStart + '</strong></p>';
                    markup += '<p class="lead">End Date/Time: <strong>' + alertEnd + '</strong></p>';
                }
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of the below vehicles start up and its last known position was inside of a polygon.</strong></p><ul>';
                for (var i = 0; i < alertVehicles.length; i++) {
                    markup += '<li><strong>' + alertVehicles[i].name + '</strong></li>';
                }
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "SPEEDING":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                if (startDate != null && endDate != null) {
                    markup += '<p class="lead">Start Date/Time: <strong>' + alertStart + '</strong></p>';
                    markup += '<p class="lead">End Date/Time: <strong>' + alertEnd + '</strong></p>';
                }
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of the below vehicles broadcast a speed faster than: <u>' + alertValue + '</u> miles per hour.</strong></p><ul>';
                for (var i = 0; i < alertVehicles.length; i++) {
                    markup += '<li><strong>' + alertVehicles[i].name + '</strong></li>';
                }
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "STATIONARY":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                if (startDate != null && endDate != null) {
                    markup += '<p class="lead">Start Date/Time: <strong>' + alertStart + '</strong></p>';
                    markup += '<p class="lead">End Date/Time: <strong>' + alertEnd + '</strong></p>';
                }
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of these vehicles: ';
                for (var i = 0; i < alertVehicles.length; i++) {
                    if (i != alertVehicles.length - 1) {
                        markup += alertVehicles[i].name + ', ';
                    } else {
                        markup += alertVehicles[i].name;
                    }
                }
                markup += ' remain stationary in any of the following fences: ';
                for(var i = 0; i < polygonNames.length; i++) {
                    if (i != polygonNames.length - 1) {
                        markup += polygonNames[i].name + ', ';
                    } else {
                        markup += polygonNames[i].name;
                    }
                }
                markup += ' for over ' + alertValue + ' minutes.</strong></p><ul>';
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "STARTUP":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                if (startDate != null && endDate != null) {
                    markup += '<p class="lead">Start Date/Time: <strong>' + alertStart + '</strong></p>';
                    markup += '<p class="lead">End Date/Time: <strong>' + alertEnd + '</strong></p>';
                }
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of the below vehicles start up:</strong></p><ul>';
                for (var i = 0; i < alertVehicles.length; i++) {
                    markup += '<li><strong>' + alertVehicles[i].name + '</strong></li>';
                }
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "ENTER POLYGON":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                if (startDate != null && endDate != null) {
                    markup += '<p class="lead">Start Date/Time: <strong>' + alertStart + '</strong></p>';
                    markup += '<p class="lead">End Date/Time: <strong>' + alertEnd + '</strong></p>';
                }
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of these vehicles: ';
                for (var i = 0; i < alertVehicles.length; i++) {
                    if (i != alertVehicles.length - 1) {
                        markup += alertVehicles[i].name + ', ';
                    } else {
                        markup += alertVehicles[i].name;
                    }
                }
                markup += ' enter any of the following fences: ';
                for (var i = 0; i < polygonNames.length; i++) {
                    if (i != polygonNames.length - 1) {
                        markup += polygonNames[i].name + ', ';
                    } else {
                        markup += polygonNames[i].name;
                    }
                }
                markup += '. </strong></p> <hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "EXIT POLYGON":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                if (startDate != null && endDate != null) {
                    markup += '<p class="lead">Start Date/Time: <strong>' + alertStart + '</strong></p>';
                    markup += '<p class="lead">End Date/Time: <strong>' + alertEnd + '</strong></p>';
                }
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of these vehicles: ';
                for (var i = 0; i < alertVehicles.length; i++) {
                    if (i != alertVehicles.length - 1) {
                        markup += alertVehicles[i].name + ', ';
                    } else {
                        markup += alertVehicles[i].name;
                    }
                }
                markup += ' exit any of the following fences: ';
                for (var i = 0; i < polygonNames.length; i++) {
                    if (i != polygonNames.length - 1) {
                        markup += polygonNames[i].name + ', ';
                    } else {
                        markup += polygonNames[i].name;
                    }
                }
                break;
            default:
                markup += "";
        }

        $('#step5Div').append(markup);
    }

    //#endregion

    //#region SubmitAlert()

    $('#submitAlert').click(function () {
        submitAlert();
    });

    function submitAlert() {
        var _url = 'updateAlertData';
        var _data = JSON.stringify({ 'alertClassID': alertClassID, 'alertClassName': alertClassName, 'alertName': alertName, 'editAlertID': editAlertID.AlertID, 'startDate': alertStart, 'endDate': alertEnd, 'polygonIDs': polygonIDs, 'polygonNames': polygonNames, 'alertVehicles': alertVehicles, 'alertValue': alertValue });
        //var _data = "alertClassID=" + alertClassID + "&AlertClassName=" + alertClassName + "&alertName=" + alertName + "&startDate=" + alertStart + "&endDate=" + alertEnd + "&polygonIDs=" + JSON.stringify(polygonIDs) + "&polygonNames=" + polygonNames + "&alertVehicles=" + alertVehicles + "&alertValue=" + alertValue;

        $.ajax({
            type: "POST",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: submitAlertSuccess,
            error: submitAlertError
        });
    }

    function submitAlertSuccess(data) {
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
            Command: toastr["success"]("Alert Created");
            clearVars();
            $('#myModal').modal('hide');
            $('#alertClassDiv').html('');
            $('#step2well').html('');
            $('#vehicTable').html('');
            $('#step4Div').html('');
            $('#step5Div').html('');
            $('[href=\\#step1]').tab('show');
            getAlerts();
        } else {
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
            Command: toastr["warning"]("Create Alert Failed");
        }
    }

    function submitAlertError(result, error) {
        var err = error;
        alert('A problem occurred submitting this alert, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion

    function isInArray(arr, obj) {
        return (arr.indexOf(obj) != -1);
    }

    $('#cancelCreateAlert').click(function () {
        clearVars();
        $('#coAlertsTable').bootstrapTable('refresh');
    });

    $('#myModal').on('hidden.bs.modal', function (e) {
        clearVars();
        $('#coAlertsTable').bootstrapTable('refresh');
    })

    function clearVars() {
        alertName;
        alertClassID;
        alertClassName;
        alertStart;
        alertEnd;
        polygonIDs = [];
        polygonNames = [];
        alertVehicles = [];
        alertValue = null;
        copy = '';
        $('#alertName').val('');
        $('#startDate').val('');
        $('#endDate').val('');
        editAlertID = null;
        $('#deleteAlert').addClass('hidden');
    }

    //#region EDIT ALERT: getAlert()

    function getAlert(ID) {
        var _url = 'getAlertData';
        var _data = "ID=" + ID;

        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAlertSuccess,
            error: getAlertError
        });
    }

    function getAlertSuccess(data) {
        if (data.length == 0) {
            alert('There were no alert found with this id' + ID + '. Does that seem right to you?');
        } else {
            //alert
            editAlertID = data.alert;
            alertClassID = data.alert.AlertClassID;
            alertClassName = data.alert.AlertClassName;
            alertName = data.alert.AlertFriendlyName;
            alertValue = data.alert.minVal;
            alertStart = moment(data.alert.AlertStartTime).format('MM/DD/YYYY HH:mm');
            alertEnd = moment(data.alert.AlertEndTime).format('MM/DD/YYYY HH:mm');

            for(var p = 0; p < data.extendedAlertFences.length; p++) {
                polygonIDs.push(data.extendedAlertFences[p].geoFenceID);
                polygonNames.push(data.extendedAlertFences[p].polyName);
            }

            for(var v = 0; v < data.extendedAlertVehicles.length; v++) {
                alertVehicles.push({
                    id: data.extendedAlertVehicles[v].extendedData.ID,
                    name: data.extendedAlertVehicles[v].extendedData.VehicleFriendlyName,
                    email: function () {
                        for (var i = 0; i < data.alertVehicles.length; i++) {
                            if (data.alertVehicles[i].VehicleID == data.extendedAlertVehicles[v].extendedData.ID) {
                                "EMAIL:" + data.alertVehicles[i].AlertAction;
                            }
                        }
                    }
                });
            }

            getAlertClasses()
            $('[href=\\#step1]').tab('show');
            $('#deleteAlert').removeClass('hidden');

            $('#myModal').on('shown.bs.modal', function () {
                $('#alertName').val(alertName);
                $('#startDate').val(alertStart);
                $('#endDate').val(alertEnd);
                $('#' + alertClassID).css({ 'background-color': '#2fa4e7', 'color': 'white' });


                $(document).on('shown.bs.tab', 'a[data-toggle="tab"]', function (e) {
                    if (e.target.toString().indexOf('#step2') !== -1) {
                        $('#step2well').addClass('hidden');
                        for (var p = 0; p < polygonIDs.length; p++) {
                            $('#' + polygonIDs[p]).bootstrapToggle('on')
                        }
                        $('#step2well').removeClass('hidden');
                    } else if (e.target.toString().indexOf('#step3') !== -1) {
                        $('#vehicTable').addClass('hidden');
                        $('#vehicTableLoading').html('<img src=\'../Content/Images/preloader.gif\' width=\'100\' />');
                        setTimeout(function () {
                            for (var v = 0; v < alertVehicles.length; v++) {
                                $('#tb_' + alertVehicles[v].id).bootstrapToggle('on')
                                for (var av = 0; av < data.alertVehicles.length; av++) {
                                    if (data.alertVehicles[av].VehicleID == alertVehicles[v].id) {
                                        $('#em_' + alertVehicles[v].id).val(data.alertVehicles[av].AlertAction.substring(6, data.alertVehicles[av].AlertAction.length));
                                    }
                                }
                            }
                            $('#vehicTableLoading').html('');
                            $('#vehicTable').removeClass('hidden');
                        }, 3000);
                    } else if (e.target.toString().indexOf('#step4') !== -1) {
                        $('#step4DivLoading').html('<img src=\'../Content/Images/preloader.gif\' width=\'100\' />');
                        $('#step4Div').addClass('hidden');
                        setTimeout(function () {
                            $('#alertValue').val(alertValue);
                            $('#step4DivLoading').html('');
                            $('#step4Div').removeClass('hidden');
                        }, 1000);
                    }
                });
            })
        }
    }

    function getAlertError(data, error) {
        var err = error;
        alert('A problem occurred retrieving the alert, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion

    //#region DELETE Alert

    $('#deleteAlert').click(function () {
        if (confirm("Are you sure? This will delete the alert forever.")) {
            deleteAlert();
        }
    });

    function deleteAlert() {
        var _url = 'deleteAlert';
        var _data =  JSON.stringify({ 'alert': editAlertID });

        $.ajax({
            type: "POST",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: deleteAlertSuccess,
            error: deleteAlertError
        });
    }

    function deleteAlertSuccess(data) {
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
            Command: toastr["success"]("Alert Deleted");
            clearVars();
            $('#myModal').modal('hide');
            $('#alertClassDiv').html('');
            $('#step2well').html('');
            $('#vehicTable').html('');
            $('#step4Div').html('');
            $('#step5Div').html('');
            $('[href=\\#step1]').tab('show');
            getAlerts();
        } else {
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
            Command: toastr["warning"]("Delete Alert Failed");
        }
    };

    function deleteAlertError(data, error) {
        var err = error;
        alert('A problem occurred deleting the alert, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion DELTE Alert

});
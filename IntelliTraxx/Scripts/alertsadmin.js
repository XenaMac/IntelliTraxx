$(function () {
    var alertClassID;
    var alertClassName;
    var polygonIDs = [];
    var polygonNames = [];
    var polygonAlerts = ['STATIONARY', 'ENTER POLYGON', 'EXIT POLYGON']
    var vehicleOnlyAlerts = ['LAST POSITION', 'SPEEDING', 'STARTUP']
    var valueAlerts = ['SPEEDING', 'STATIONARY']
    var alertVehicles = [];
    var alertValue = null;

    //#region Menu scripts

    $('#mapIcon').click(function () {
        window.location.href = '../Fleet/index';
    });

    $('#alertsIcon').click(function () {
        window.location.href = '../Alerts/Index';
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
                        alert($(this).attr('id'));
                    });

                    $('#newAlert').click(function () {
                        getAlertClasses();
                    });
                },
            });
        }
    }

    function getAlertsError(data, error) {
        var err = error;
        alert('A problem occurred getting the alert, please reload or contact the administrator. Error: ' + error);
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
        $('#step1well').html('');
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
            var markup = '<form class="form-horizontal"><div class="form-group" id="alertNameFormGroup"><label for="alertName" class="col-sm-2 control-label">Alert Name:</label><div class="col-sm-10"><input type="text" class="form-control" id="alertName" placeholder="Ex: Downtown Enter Alert"></div></div></form>';
            $('#step1well').append(markup);

            for (var i = 0; i < data.length; i++) {
                markup = '';

                switch (data[i].AlertClassName) {
                    case "LAST POSITION":
                        markup += '<div class="col-md-3"><div class="thumbnail text-center"><i class="material-icons md-48">all_out</i><div class="caption"><h4>' + data[i].AlertClassName + '</h4><small><p>Vehicle will trigger an alert upon startup, if last known position was inside a polygon.</p></small><p><a href="#" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '" class="btn btn-info btn-sm next" role="button">Select</a></div></div></div></div>';
                        break;
                    case "SPEEDING":
                        markup += '<div class="col-md-3"><div class="thumbnail text-center"><i class="material-icons md-48">network_check</i><div class="caption"><h4>' + data[i].AlertClassName + '</h4><small><p>Vehicle will trigger an alert when transmitted speed is over the assigned value.<br /><br /></p></small><p><a href="#" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '"  class="btn btn-info btn-sm next" role="button">Select</a></div></div></div></div>';
                        break;
                    //case "SCHEDULE":
                    //    markup += '<div class="col-md-3"><div class="thumbnail text-center"><i class="material-icons md-48">event_available</i><div class="caption"><h4>' + data[i].AlertClassName + '</h4><small><p>Vehicle will trigger an alert when it is active outside of an assigned schedule.<br /><br /></p></small><p><a href="#" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '" class="btn btn-info btn-sm next" role="button">Select</a></div></div></div></div>';
                    //    break;
                    case "STATIONARY":
                        markup += '<div class="col-md-3"><div class="thumbnail text-center"><i class="material-icons md-48">gps_fixed</i><div class="caption"><h4>' + data[i].AlertClassName + '</h4><small><p>Vehicle will trigger an alert when it has been stationary within an assigned geofence for a specific amount of time.</p></small><p><a href="#" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '" class="btn btn-info btn-sm next" role="button">Select</a></div></div></div></div>';
                        break;
                    case "STARTUP":
                        markup += '<div class="col-md-3"><div class="thumbnail text-center"><i class="material-icons md-48">power_settings_new</i><div class="caption"><h4>' + data[i].AlertClassName + '</h4><small><p>Vehicle will trigger an alert the very first time it starts up for the day.<br /><br /></p></small><p><a href="#" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '" class="btn btn-info btn-sm next" role="button">Select</a></div></div></div></div>';
                        break;
                    case "ENTER POLYGON":
                        markup += '<div class="col-md-3"><div class="thumbnail text-center"><i class="material-icons md-48">input</i><div class="caption"><h4>' + data[i].AlertClassName + '</h4><small><p>Vehicle will trigger an alert when it enters an assigned polygon.<br /><br /></p></small><p><a href="#" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '" class="btn btn-info btn-sm next" role="button">Select</a></div></div></div></div>';
                        break;
                    case "EXIT POLYGON":
                        markup += '<div class="col-md-3"><div class="thumbnail text-center"><i class="material-icons md-48">open_in_new</i><div class="caption"><h4>' + data[i].AlertClassName + '</h4><small><p>Vehicle will trigger an alert when it exits an assigned polygon.<br /><br /></p></small><p><a href="#" id="' + data[i].AlertClassID + '" name="' + data[i].AlertClassName + '" class="btn btn-info btn-sm next" role="button">Select</a></div></div></div></div>';
                        break;
                    default:
                        markup += "";
                }
                $('#step1well').append(markup);
            }

            $('#myModal').modal('show');

            $('.next').click(function () {
                if ($('#alertName').val() == '') {
                    $('#alertNameFormGroup').addClass('has-error');
                } else {
                    alertClassID = $(this).attr('id');
                    alertClassName = $(this).attr('name');

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

            $('.pgSelect').bootstrapToggle({
                on: 'Yes',
                off: 'No'
            });

            $('.nnext').click(function () {
                polygonIDs = [];
                polygonNames = [];
                $(".pgSelect").each(function () {
                    if (this.checked) {
                        polygonIDs.push($(this).attr('id'));
                        polygonNames.push($(this).attr('name'));
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
        $('#step3well').html('');
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
            $('#step3well').html('<div class="form-group" id="alertVehiclesForm"><label class="control-label" for="alertVehicles">Vehicles assigned to this alert:&nbsp;&nbsp;</label><select id="alertVehicles" class="form-control" multiple="multiple"></select></div>');
            var select = document.getElementById('alertVehicles');
            for (var i = 0; i < data.length; i++) {
                var opt = document.createElement('option');
                opt.value = data[i].VehicleID;
                opt.innerHTML = data[i].extendedData.VehicleFriendlyName;
                select.appendChild(opt);
            }

            $('#alertVehicles').multiselect({
                includeSelectAllOption: true,
                selectAllValue: 'select-all-value'
            });

            $('.nnnext').click(function () {
                if ($('#alertVehicles').val() == '') {
                    $('#alertVehiclesForm').addClass('has-error');
                } else {
                    alertVehicles = $('#alertVehicles').val();
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
            alert('A problem occurred pulling the all vehicles, please reload or contact the administrator.');
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
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of the below vehicles start up and its last known position was inside of a polygon.</strong></p><ul>';
                for (var i = 0; i < alertVehicles.length; i++) {
                    markup += '<li><strong>' + alertVehicles[i] + '</strong></li>';
                }
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "SPEEDING":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of the below vehicles broadcast a speed faster than: <u>' + alertValue + '</u> miles per hour.</strong></p><ul>';
                for (var i = 0; i < alertVehicles.length; i++) {
                    markup += '<li><strong>' + alertVehicles[i] + '</strong></li>';
                }
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "STATIONARY":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of these vehicles: ';
                for (var i = 0; i < alertVehicles.length; i++) {
                    if (i != alertVehicles.length - 1) {
                        markup += alertVehicles[i] + ', ';
                    } else {
                        markup += alertVehicles[i];
                    }
                }
                markup += ' remain stationary in any of the following fences: ';
                for(var i = 0; i < polygonNames.length; i++) {
                    if (i != polygonNames.length - 1) {
                        markup += polygonNames[i] + ', ';
                    } else {
                        markup += polygonNames[i];
                    }
                }
                markup += ' for over ' + alertValue + ' minutes.</strong></p><ul>';
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "STARTUP":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of the below vehicles start up:</strong></p><ul>';
                for (var i = 0; i < alertVehicles.length; i++) {
                    markup += '<li><strong>' + alertVehicles[i] + '</strong></li>';
                }
                markup += '</ul><hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "ENTER POLYGON":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of these vehicles: ';
                for (var i = 0; i < alertVehicles.length; i++) {
                    if (i != alertVehicles.length - 1) {
                        markup += alertVehicles[i] + ', ';
                    } else {
                        markup += alertVehicles[i];
                    }
                }
                markup += ' enter any of the following fences: ';
                for (var i = 0; i < polygonNames.length; i++) {
                    if (i != polygonNames.length - 1) {
                        markup += polygonNames[i] + ', ';
                    } else {
                        markup += polygonNames[i];
                    }
                }
                markup += '. </strong></p> <hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            case "EXIT POLYGON":
                markup += '<p class="lead">Alert Class: <strong>' + alertClassName + '</strong></p>';
                markup += '<p class="lead">Alert Name: <strong>' + $('#alertName').val() + '</strong></p>';
                markup += '<p class="lead">Description: <strong>This alert will trigger when any of these vehicles: ';
                for (var i = 0; i < alertVehicles.length; i++) {
                    if (i != alertVehicles.length - 1) {
                        markup += alertVehicles[i] + ', ';
                    } else {
                        markup += alertVehicles[i];
                    }
                }
                markup += ' exit any of the following fences: ';
                for (var i = 0; i < polygonNames.length; i++) {
                    if (i != polygonNames.length - 1) {
                        markup += polygonNames[i] + ', ';
                    } else {
                        markup += polygonNames[i];
                    }
                }
                markup += '. </strong></p> <hr  style="height:1px;border:none;color:#000;background-color:#000;">';
                break;
            default:
                markup += "";
        }

        markup += '<div class="form-group" style="overflow: hidden;"><label for="alertEmails">Email the below addresses upon trigger: </label><input type="text" class="form-control" id="alertEmails" placeholder="someone@someplace.com; someoneelse@someplace.com; finally@someplace.com">';

        $('#step5Div').append(markup);
    }

    //#endregion

    function isInArray(arr, obj) {
        return (arr.indexOf(obj) != -1);
    }

    $('#cancelCreateAlert').click(function () {
        location.reload();
    });
});
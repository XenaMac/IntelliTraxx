$(function () {

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
            $('#alertsDiv').html('<table id="coAlertsTable" data-toggle="table" data-striped="true" data-query-params="queryParams" data-pagination="true" data-page-size="15" data-page-list="[5, 10, 20, 50]" data-classes="table table-hover table-condensed" data-sort-name="alertStart" data-sort-order="desc" data-search="true" data-show-refresh="true" data-show-toggle="true" data-show-columns="true" data-show-export="true" data-url="' + _url + _data + '"><thead><tr><th data-field="AlertID" class="hidden">Alert ID</th><th data-halign="center" data-sortable="true" data-field="AlertFriendlyName">Alert</th><th data-halign="center" data-sortable="true" data-field="AlertClassName">Alert Class</th><th data-halign="center" data-sortable="true" data-field="AlertActive" data-formatter="toggler" class="text-center">Enabled</th><th data-halign="center" data-sortable="true" data-field="AlertStartTime" data-formatter="dateFormat">Start</th><th data-halign="center" data-sortable="true" data-field="AlertEndTime" data-formatter="dateFormat">End</th><th data-halign="center" data-sortable="true" data-field="minVal">Min Value</th><th data-halign="center" data-sortable="true" data-field="minVal" data-formatter="editBtn" class="text-center">Edit</th></tr></thead><tbody></tbody></table><br />');

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
});
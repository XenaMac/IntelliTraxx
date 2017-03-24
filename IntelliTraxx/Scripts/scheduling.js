$(function () {

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

    getAllSchedules();

    //#region getAllSchedules

    function getAllSchedules() {
        $('#loader').removeClass('hidden');
        $('#scheduleList').html('');
        var _url = 'getAllSchedules';
        var _data = "";

        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAllSchedulesSuccess,
            error: getAllSchedulesError
        });
    }

    function getAllSchedulesSuccess(data) {

        $('#loader').addClass('hidden');

        if (data.length == 0) {
            $('#alertsDiv').html('There were no schedules found. Does that seem right to you?');
        } else {
            var markup = '<div class="list-group">';
            for (var i = 0; i < data.length; i++) {
                if (i == 0) {
                    markup += '<a href="#" class="list-group-item schedule active"><h4 class="list-group-item-heading">' + data[i].scheduleName + '</h4><p class="list-group-item-text">' + moment(data[i].startTime).format('MM/DD/YYYY HH:mm') + '</p></a>';
                } else {
                    markup += '<a href="#" class="list-group-item schedule "><h4 class="list-group-item-heading">' + data[i].scheduleName + '</h4><p class="list-group-item-text">' + moment(data[i].startTime).format('MM/DD/YYYY HH:mm') + '</p></a>';
                }
            }
            markup += '</div>';
            $('#scheduleList').append(markup);
        }

        $('.list-group a').click(function (e) {
            e.preventDefault()
            $that = $(this);
            $that.parent().find('a').removeClass('active');
            $that.addClass('active');
        });
    }

    function getAllSchedulesError(data, error) {
        var err = error;
        alert('A problem occurred retrieving the schedules, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion
});
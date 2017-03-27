$(function () {
    var schedules = [];
    var vehicles = [];
    var scheduleVehicles = [];
    var updateVehicles = [];

    $('[data-toggle="tooltip"]').tooltip()
    $('#tbEffStDt').datetimepicker();
    $('#tbEffEndDt').datetimepicker();
    $('#timeFrom').datetimepicker({
        datepicker: false,
        format: 'H:i'
    });
    $('#timeTo').datetimepicker({
        datepicker: false,
        format: 'H:i'
    });

    initVehicleDropDown();
    getAllSchedules();

    //editSchedule(schedules[0].scheduleID);

    //#region getAllSchedules

    function getAllSchedules() {
        $('#loader').removeClass('hidden');
        $('#scheduleInfo').addClass('hidden');
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

        if (data.length == 0) {
            $('#alertsDiv').html('There were no schedules found. Does that seem right to you?');
        } else {
            schedules = data;
            var markup = '<div class="list-group">';
            for (var i = 0; i < schedules.length; i++) {
                markup += '<a href="#" class="list-group-item" data-alias="' + data[i].scheduleID + '"><h4 class="list-group-item-heading">' + schedules[i].scheduleName + '</h4><p class="list-group-item-text small">' + moment().day(schedules[i].DOW - 1).format('dddd') + ': ' + moment(schedules[i].startTime).format('HH:mm') + ' - ' + moment(schedules[i].endTime).format('HH:mm') + '</p></a>';
            }
            markup += '</div>';

            $('#scheduleList').append(markup);

            $('#loader').addClass('hidden');
            $('#scheduleInfo').removeClass('hidden');
        }

        $('.list-group a').click(function (e) {
            e.preventDefault()
            $that = $(this);
            $that.parent().find('a').removeClass('active');
            $that.addClass('active')
            var $id = $that.data('alias');
            editSchedule($id);
        });
    }

    function getAllSchedulesError(data, error) {
        var err = error;
        alert('A problem occurred retrieving the schedules, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion

    //#region initVehicleDropDown()

    function initVehicleDropDown() {
        $('#vehicleLoader').removeClass('hidden');
        $('#vehicleList').addClass('hidden');
        $('#vehicleList').empty();
        var _url = 'getAllVehicles';
        var _data = "loadHistorical=true";

        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: initVehicleDropDownSuccess,
            error: initVehicleDropDownError
        });
    }

    function initVehicleDropDownSuccess(data) {
        if (data.length == 0) {
            $('#alertsDiv').html('There were no vehicles found. Does that seem right to you?');
        } else {
            vehicles = data;

            for (var i = 0; i < vehicles.length; i++) {
                $('#vehicleList').append('<option value="' + vehicles[i].extendedData.ID + '">' + vehicles[i].extendedData.VehicleFriendlyName + '</option>')
            }
        }

        $('#vehicleList').multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            buttonClass: 'btn btn-primary',
            numberDisplayed: 50,
            buttonWidth: '300px'
        });

        $('#vehicleLoader').addClass('hidden');
        $('#vehicleList').removeClass('hidden');
    }

    function initVehicleDropDownError(data, error) {
        var err = error;
        alert('A problem occurred retrieving the vehicles, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion

    //#region editSchedule()

    function editSchedule(scheduleID) {
        if (scheduleID != null) {
            $('.btn-group label').removeClass('active');

            var schedule = findSchedule(scheduleID);

            $('#tbScheduleName').val(schedule[0].scheduleName);
            $('#tbEffStDt').datetimepicker({
                value: moment(schedule[0].EffDtStart).format('YYYY-MM-DD HH:mm')
            });
            $('#tbEffEndDt').datetimepicker({
                value: moment(schedule[0].EffDtEnd).format('YYYY-MM-DD HH:mm')
            });
            $('#' + schedule[0].DOW).attr('checked', 'checked');
            $('#' + schedule[0].DOW).parent('.btn').addClass('active');
            $('#timeFrom').datetimepicker({
                value: moment(schedule[0].startTime).format('HH:mm'),
                format: 'H:i',
                datepicker: false,
            });
            $('#timeTo').datetimepicker({
                value: moment(schedule[0].endTime).format('HH:mm'),
                format: 'H:i',
                datepicker: false,
            });
            getAllVehiclesBySchedule(scheduleID);
            if (schedule[0].active == true) {
                $('#active').bootstrapToggle('on');
            } else {

                $('#active').bootstrapToggle('off');
            }
            $('#createSchedule').addClass('hidden');
            $('#deleteSchedule').removeClass('hidden');
            $('#modifySchedule').removeClass('hidden');
        }
    }

    //#endregion

    //#region getAllVehiclesBySchedule

    function getAllVehiclesBySchedule(scheduleID) {
        $('#vehicleList').multiselect('deselect', scheduleVehicles);
        var _url = 'getAllVehiclesBySchedule';
        var _data = "scheduleID=" + scheduleID;

        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAllVehiclesByScheduleSuccess,
            error: getAllVehiclesByScheduleError
        });
    }

    function getAllVehiclesByScheduleSuccess(data) {

        for (var i = 0; i < data.length; i++) {
            scheduleVehicles.push(data[i].vID);
        }

        $('#vehicleList').multiselect('select', scheduleVehicles);
    }

    function getAllVehiclesByScheduleError(data, error) {
        var err = error;
        alert('A problem occurred retrieving the schedule vehicles, please reload or contact the administrator. Error: ' + error);
    }

    //#endregion

    function findSchedule(scheduleID) {
        return $.grep(schedules, function (item) {
            return item.scheduleID == scheduleID;
        });
    };

    function createGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    function arraysEqual(arr1, arr2) {
        if (arr1.length !== arr2.length)
            return false;
        for (var i = arr1.length; i--;) {
            if (arr1[i] !== arr2[i])
                return false;
        }

        return true;
    }

    $('#newSchedule').click(function () {
        $('.btn-group label').removeClass('active');
        $('#tbScheduleName').val('');
        $('#tbEffStDt').val('');
        $('#tbEffEndDt').val('');
        $('#timeFrom').val('');
        $('#timeTo').val('');
        $('#vehicleList').multiselect('deselectAll', false);
        $('#vehicleList').multiselect('updateButtonText');
        $('#alert').bootstrapToggle('on');
        $('#alertEmails').val('');
        $('#active').bootstrapToggle('on');
        $('.list-group a').each(function () {
            $(this).removeClass('active');
        });
        $('#createSchedule').removeClass('hidden');
        $('#deleteSchedule').addClass('hidden');
        $('#modifySchedule').addClass('hidden');
        $('#tbScheduleName').focus();
    });

    $('#logOff').click(function () {
        var form = $('#__AjaxAntiForgeryForm');
        var token = $('input[name="__RequestVerificationToken"]', form).val();
        $.ajax({
            url: '../Account/LogOff',
            type: 'POST',
            data: {
                __RequestVerificationToken: token
            },
            success: function (result) {
                window.location.href = 'Index';
            }
        });
        return false;
    })

    $('#scheduleForm').submit(function (e) {
        e.preventDefault();
        $('#submitter').removeClass('hidden');
        var scheduleList = [];
        var DOW = null;

        for (var i = 1; i < 6; i++) {
            if ($('#' + i).prop('checked')) {
                DOW = i;
            }
        }

        $('#vehicleList option:selected').each(function (index, brand) {
            updateVehicles.push($(this).val());
        });

        scheduleList.push({
            scheduleID: createGuid(),
            scheduleName: $('#tbScheduleName').val(),
            company: null,
            startTime: $('#timeFrom').val(),
            endTime: $('#timeTo').val(),
            createdBy: null,
            createdOn: null,
            modifiedBy: null,
            modifiedOn: null,
            DOW: DOW,
            EffDtStart: $('#tbEffStDt').val(),
            EffDtEnd: $('#tbEffEndDt').val(),
            active: $('#active').prop('checked')
        });

        updateSchedule(scheduleList);
    });

    //#region udpateSchedule

    function updateSchedule(scheduleList) {
        var _url = 'updateSchedules';
        var _data = JSON.stringify({ 'schedules': scheduleList });

        $.ajax({
            type: "POST",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                updateScheduleSuccess(data, scheduleList);
            },
            error: updateScheduleError
        });
    }

    function updateScheduleSuccess(data, schedueList) {
        if (data == "OK") {
            //send list of vehicles to bind to schedule.
            addVehiclesToSchedule(schedueList[0].scheduleID);
        } else {
            alert('There was an issue saving the schedule.  Please try again or contact the administrator');
        }
    }

    function updateScheduleError(data, error) {
        var err = error;
        alert('A problem occurred saving the schedule, please reload or contact the administrator. Error: ' + data.html);
    }

    //#endregion

    //#region add/Update Vehicles in schedule

    function addVehiclesToSchedule(scheduleID) {
        if (!arraysEqual(scheduleVehicles, updateVehicles)) {
            var _url = 'addVehicleToSchedule';
            var _data = JSON.stringify({ 'scheduleID': scheduleID, 'vehicleIDs': updateVehicles });

            $.ajax({
                type: "POST",
                dataType: "json",
                url: _url,
                data: _data,
                contentType: "application/json; charset=utf-8",
                success: addVehiclesToScheduleSuccess,
                error: addVehiclesToScheduleError
            });
        }
    }

    function addVehiclesToScheduleSuccess(data) {
        if (data == "OK") {
            $('#submitter').addClass('hidden');
            scheduleVehicles = [];
            updateVehicles = [];
            initVehicleDropDown();
            getAllSchedules();
            $('#newSchedule').click();
        } else {
            alert('There was an issue saving the vehicles.  Please try again or contact the administrator');
        }
    }

    function addVehiclesToScheduleError(data, error) {
        var err = error;
        alert('A problem occurred saving the vehicles to the schedule, please reload or contact the administrator.');
    }

    //#endregion
});
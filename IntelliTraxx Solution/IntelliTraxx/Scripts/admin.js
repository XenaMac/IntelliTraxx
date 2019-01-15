$(function () {
    $('[data-toggle="tooltip"]').tooltip()

    $('#assignDriver').click(function () {
        changeDrivers();
    });

    $('.DVR').click(function () {
        deleteVehicleDriver($(this).attr("id"));
    });
        
    //#region changeDrivers Functions
    function changeDrivers() { //Get a download of the vehicle for ID
        var _url = '../Fleet/changeDrivers';
        var _data = "from=null" + "&to=" + $('#driver').val() + "&vehicleID=" + $('#vehicle').val();
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
            Command: toastr["success"]("New Driver Assigned. We need to refresh the page now.");
            location.href = "Index?tab=DTV";
        } else {
            alert('A problem occurred changing the drivers, please reload or contact the administrator');
        }
    }

    function changeDriversError(result, error) {
        var err = error;
        alert('A problem occurred changing the drivers, please reload or contact the administrator');
    }
    //#endregion

    //#region deleteVehicleDriver() Functions
    function deleteVehicleDriver(ID) { //Get a download of the vehicle for ID
        var _url = 'deleteVehicleDriver';
        var _data = "id=" + ID;
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: deleteVehicleDriverSuccess,
            error: deleteVehicleDriverError
        });
    }

    function deleteVehicleDriverSuccess(data) {
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
            Command: toastr["success"]("Driver Deleted. We need to refresh the page now.");
            location.href = "Index?tab=DTV";
        } else {
            alert('A  problem occurred deleting the driver assignment, please reload or contact the administrator');
        }
    }

    function deleteVehicleDriverError(result, error) {
        var err = error;
        alert('A problem occurred deleting the driver assignment, please reload or contact the administrator');
    }
    //#endregion
});
$(function () {
    $("#dispatchModal").appendTo("body");
    $('#dispatchModal').modal('hide');

    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();
    today = mm + '/' + dd + '/' + yyyy;
    var currentLocation = null;
    var map = null;
    var mapCenter = null;
    var dispatch = null;

    $('#datetimepicker').datetimepicker({
        dayOfWeekStart: 1,
        lang: 'en',
        disabledDates: ['1986/01/08', '1986/01/09', '1986/01/10'],
        startDate: today
    });

    $('#fromDateOpener').click(function () {
        $('#datetimepicker').datetimepicker('show');
    });

    $('#toDatetimepicker').datetimepicker({
        dayOfWeekStart: 1,
        lang: 'en',
        disabledDates: ['1986/01/08', '1986/01/09', '1986/01/10'],
        startDate: today
    });

    $('#toDateOpener').click(function () {
        $('#toDatetimepicker').datetimepicker('show');
    });

    $('#filter').click(function () {
        if ($('#datetimepicker').val() != "" && $('#toDatetimepicker') != "") {
            var url = 'Index' + '?from=' + $('#datetimepicker').val() + '&to=' + $('#toDatetimepicker').val();
            window.location = url;
        }
    });

    $('#dispatchTable').on('click', '.modalCaller', function () {
        $.post("GetDispatchByID?ID=" + this.id, null, function (data) {
            if (data != "") {
                $('#title').text("Dispatch: " + data.address + ", " + data.city + ", " + data.state + " " + data.zip);

                var timeStamp = new Date(parseInt(data.timeStamp.substr(6)));

                var synopsis = "Dispatch was created on " + timeStamp.toLocaleDateString() + " " + timeStamp.toLocaleTimeString() + " to:<br /><br />";

                synopsis += "<strong>" + data.address + ", " + data.city + ", " + data.state + ", " + data.zip + "</strong><br /><br />";

                if (typeof data.notes != 'undefined') {
                    synopsis += "With the following notes:";
                    synopsis += data.notes;
                }

                if(data.ackTime != null) {
                    var ackTime = new Date(parseInt(data.ackTime.substr(6)));
                    synopsis += "Dispatch was acknowledged at " + ackTime.toLocaleDateString() + " " + ackTime.toLocaleTimeString() + " by:<br /><br />";
                } else {
                    var ackTime = "Unacknowledged";
                    synopsis += "Dispatch has yet to be acknowledged.<br /><br />";
                }
                if (data.completedTime != null) {
                    var completedTime = new Date(parseInt(data.completedTime.substr(6)));
                    synopsis += "Dispatch was completed on : " + ackTime.toLocaleDateString() + " " + completedTime.toLocaleTimeString() + "<br /><br />";
                    synopsis += "<strong>" + data.vehicleID + " (" + data.driverPIN + ")</strong><br /><br />";
                } else {
                    var completedTime = "Not Completed";
                    synopsis += "Dispatch has not yet been compelted.<br /><br />";
                }                
                
                if (typeof data.compeltedMessage != 'undefined') {
                    synopsis += "With the following completed message:";
                    synopsis += "<strong" > +data.compeltedMessage + "</strong>";
                }

                $('#disInfo').html(synopsis);

                currentLocation = data.address + ", " + data.city + ", " + data.state + " " + data.zip;
                var geocoder = new google.maps.Geocoder();

                geocoder.geocode({ 'address': currentLocation }, function (results, status) {
                    if (status === google.maps.GeocoderStatus.OK) {
                        mapCenter = results[0].geometry.location;
                        map = new google.maps.Map(document.getElementById('disMap'), {
                            center: mapCenter,
                            zoom: parseInt(15),
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
                    }
                });

                $('#dispatchModal').modal('show');
            }
        });        
    });

    $('#dispatchModal').on('shown.bs.modal', function (e) {
        google.maps.event.trigger(map, "resize");
        map.setCenter(mapCenter);
        //Dispatch marker
        dispatch = new google.maps.Marker({
            position: mapCenter,
            title: currentLocation,
            draggable: false,
            map: map
        });
    });
});
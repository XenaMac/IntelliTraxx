$(function () {
    var mapDiv = null;
    var map = null;

    $.post("GetAlertHistory?alertID=" + $('#alertID').val() + "&vehicleID=" + $('#vehicleID').val(), null, function (data) {
        if (data) {
            mapDiv = document.getElementById('alertMap');
            map = new google.maps.Map(mapDiv, {
                center: { lat: 44.540, lng: -78.546 },
                zoom: 15,
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
            map.setCenter({ lat: data.Locations[0].Lat, lng: data.Locations[0].Lon });

            //set text lables bounds
            $('#ST').text(new Date(parseInt(data.Alert.alertStart.substr(6))).toLocaleTimeString());
            $('#ET').text(new Date(parseInt(data.Alert.alertEnd.substr(6))).toLocaleTimeString());

            //vehicle marker
            vehicle = new google.maps.Marker({
                position: { lat: data.Locations[0].Lat, lng: data.Locations[0].Lon },
                title: data.Alert.vehicleID,
                draggable: false,
                map: map
            });

            //slider
            $('#ex1Slider').slider({
                animate: "slow",
                min: 1,
                max: data.Locations.length,
                step: 1,
                orientation: "horizontal",
                value: data.Locations.length,
                slide: function (event, slideEvt) {
                    vehicle.setMap(null);
                    var index = slideEvt.value - 1;
                    //Set values based on slider
                    var date = new Date(parseInt(data.Locations[index].timestamp.substr(6)));
                    $('#LC').text(date.toLocaleDateString() + " at " + date.toLocaleTimeString());
                    $('#Lat').text(data.Locations[index].Lat);
                    $('#Long').text(data.Locations[index].Lon);
                    $('#Dir').text(data.Locations[index].Direction);
                    $('#SPD').text(data.Locations[index].Speed);

                    //center map
                    map.setCenter({ lat: data.Locations[index].Lat, lng: data.Locations[index].Lon });

                    //add marker
                    vehicle = new google.maps.Marker({
                        position: { lat: data.Locations[index].Lat, lng: data.Locations[index].Lon },
                        title: data.Alert.VehicleID,
                        draggable: false,
                        map: map
                    });
                }
            });

            $('#ex1Slider').slider("value", 1);

            $('.sliderDiv').show();
        }
    });
});
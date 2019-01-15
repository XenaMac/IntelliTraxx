$(function () {
    var alertmapDiv = null;
    var alertmap = null;

    getAlertData();

    //#region get alert
    function getAlertData() {
        //add loader
        jsPanel.activePanels.getPanel("alertViewPanel").content.append("<div id=\'vehicleHistoryLoader\' class=\'loading\'><img src=\'../Content/Images/preloader.gif\' width=\'100\' /></div>");

        var _url = 'GetAlertHistory';
        var _data = "alertID=" + $('#alertID').val() + "&vehicleID=" + $('#vehicleID').val();
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getAlertDataSuccess,
            error: getAlertDataError
        });
    }

    function getAlertDataSuccess(data) {
        if (data) {
            alertmapDiv = document.getElementById('alertMap');
            alertmap = new google.maps.Map(alertmapDiv, {
                zoom: 13,
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
            alertmap.setCenter({ lat: data.Locations[0].Lat, lng: data.Locations[0].Lon });

            //set text lables bounds
            $('#ST').text(new Date(parseInt(data.Alert.alertStart.substr(6))).toLocaleTimeString());
            $('#ET').text(new Date(parseInt(data.Alert.alertEnd.substr(6))).toLocaleTimeString());

            //vehicle marker
            alertMapvehicle = new google.maps.Marker({
                position: { lat: data.Locations[0].Lat, lng: data.Locations[0].Lon },
                title: data.Alert.vehicleID,
                draggable: false,
                map: alertmap
            });

            //slider
            $('#alertMapSlider').slider({
                animate: "slow",
                min: 1,
                max: data.Locations.length,
                step: 1,
                orientation: "horizontal",
                value: data.Locations.length,
                slide: function (event, slideEvt) {
                    alertMapvehicle.setMap(null);
                    var index = slideEvt.value - 1;
                    //Set values based on slider
                    var date = new Date(parseInt(data.Locations[index].timestamp.substr(6)));
                    $('#LC').text(date.toLocaleDateString() + " at " + date.toLocaleTimeString());
                    $('#Lat').text(data.Locations[index].Lat);
                    $('#Long').text(data.Locations[index].Lon);
                    $('#Dir').text(data.Locations[index].Direction);
                    $('#SPD').text(data.Locations[index].Speed);

                    //center map
                    alertmap.setCenter({ lat: data.Locations[index].Lat, lng: data.Locations[index].Lon });

                    //add marker
                    alertMapvehicle = new google.maps.Marker({
                        position: { lat: data.Locations[index].Lat, lng: data.Locations[index].Lon },
                        title: data.Alert.VehicleID,
                        draggable: false,
                        map: alertmap
                    });
                }
            });

            $('#alertMapSlider').slider("value", 1);
        }
    }

    function getAlertDataError(result, error) {
        //empty alertpanel
        jsPanel.activePanels.getPanel("alertViewPanel").content.empty();

        var err = error;
        alert('A problem occurred getting the chosen alert data, please reload or contact the administrator. Error:' + err.toString());
    }
    //#endregion
});
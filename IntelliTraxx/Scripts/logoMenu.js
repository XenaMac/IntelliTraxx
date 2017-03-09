$(function () {
    var menuOpen = false;
    var menuInterval = null;

    $('#nav').draggable();
    $('#logo').click(function () {
        if (!menuOpen) {
            openMenu();
            menuOpen = true;
            menuInterval = setInterval(function () {
                closeMenu();
                menuOpen = false;
                clearInterval(menuInterval);
            }, 10000);
        } else {
            clearInterval(menuInterval);
            closeMenu();
            menuOpen = false;
        }
    })

    var openMenu = function () {
        $("#mapIcon").toggleClass("mapIcon_out", 500);
        $("#alertsIcon").toggleClass("alertsIcon_out", 500);
        $("#geofenceIcon").toggleClass("geofenceIcon_out", 500);
        $("#appsIcon").toggleClass("appsIcon_out", 500);
        $("#adminIcon").toggleClass("adminIcon_out", 500);
        $("#analyticsIcon").toggleClass("analyticsIcon_out", 500);
        $("#diagnosticsIcon").toggleClass("diagnosticsIcon_out", 500);
        $("#profileDiv").toggleClass("profileDiv_out", 500);
    }

    var closeMenu = function () {
        $("#mapIcon").toggleClass("mapIcon_out", 500);
        $("#alertsIcon").toggleClass("alertsIcon_out", 500);
        $("#geofenceIcon").toggleClass("geofenceIcon_out", 500);
        $("#appsIcon").toggleClass("appsIcon_out", 500);
        $("#adminIcon").toggleClass("adminIcon_out", 500);
        $("#analyticsIcon").toggleClass("analyticsIcon_out", 500);
        $("#diagnosticsIcon").toggleClass("diagnosticsIcon_out", 500);
        $("#profileDiv").toggleClass("profileDiv_out", 500);
    }

    if (document.location.href.indexOf('Fleet') === -1) {
        $("#geofenceIcon").addClass("wrapper");
        $("#appsIcon").addClass("wrapper");
    }
});
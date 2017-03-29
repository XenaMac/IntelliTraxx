$(function () {
    var menuOpen = false;
    var menuInterval = null;
    var dragging = false;

    $('#nav').draggable({
        containment: "window",
        start: function () {
            dragging = true;
        },
        stop: function () {
            dragging = false;
        }
    });

    $('#logo').mouseup(function () {
        if (!dragging) {
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

    $('#mapIcon').click(function () {
        window.location.href = '../Fleet/index';
    });

    $('#alertsIcon').click(function () {
        window.location.href = '../Alerts/Index';
    });

    $('#diagnosticsIcon').click(function () {
        window.location.href = '../Scheduling/Index';
    });

    $('#analyticsIcon').click(function () {
        window.location.href = '../Analytics/Index';
    });

    $('#adminIcon').click(function () {
        window.location.href = '../Admin/Index';
    });
});
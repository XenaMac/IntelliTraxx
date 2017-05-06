$(function () {

    AIVehicles();
    getDriverAnalytics();
    

    $('#fleetUtilization').highcharts({
        chart: {
            type: 'column',
            options3d: {
                enabled: true,
                alpha: 15,
                beta: 15,
                viewDistance: 25,
                depth: 40
            }
        },

        title: {
            text: 'Alerts'
        },

        xAxis: {
            categories: ['Inbound', 'On Time', 'Late', 'Off Course', 'Other']
        },

        yAxis: {
            allowDecimals: false,
            min: 0,
            title: {
                text: 'Number of Alerts'
            }
        },

        tooltip: {
            headerFormat: '<b>{point.key}</b><br>',
            pointFormat: '<span style="color:{series.color}">\u25CF</span> {series.name}: {point.y} / {point.stackTotal}'
        },

        plotOptions: {
            column: {
                stacking: 'normal',
                depth: 40
            }
        },

        series: [{
            name: 'Inbound',
            data: [5, 3, 4, 7, 2],
            stack: 'male'
        }, {
            name: 'On Time',
            data: [3, 4, 4, 2, 5],
            stack: 'male'
        }, {
            name: 'Off Course',
            data: [2, 5, 6, 2, 1],
            stack: 'female'
        }, {
            name: 'Other',
            data: [3, 0, 4, 4, 3],
            stack: 'female'
        }]
    });

    var gaugeOptions = {

        chart: {
            type: 'solidgauge'
        },

        title: null,

        pane: {
            center: ['50%', '85%'],
            size: '140%',
            startAngle: -90,
            endAngle: 90,
            background: {
                backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || '#EEE',
                innerRadius: '60%',
                outerRadius: '100%',
                shape: 'arc'
            }
        },

        tooltip: {
            enabled: false
        },

        // the value axis
        yAxis: {
            stops: [
                [0.1, '#55BF3B'], // green
                [0.5, '#DDDF0D'], // yellow
                [0.9, '#DF5353'] // red
            ],
            lineWidth: 0,
            minorTickInterval: null,
            tickPixelInterval: 400,
            tickWidth: 0,
            title: {
                y: -70
            },
            labels: {
                y: 16
            }
        },

        plotOptions: {
            solidgauge: {
                dataLabels: {
                    y: 5,
                    borderWidth: 0,
                    useHTML: true
                }
            }
        }
    };

    $('#gasAverage').highcharts({

        chart: {
            type: 'gauge',
            plotBackgroundColor: null,
            plotBackgroundImage: null,
            plotBorderWidth: 0,
            plotShadow: false
        },

        title: {
            text: 'Average Gas Price Paid'
        },

        pane: {
            startAngle: -150,
            endAngle: 150,
            background: [{
                backgroundColor: {
                    linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                    stops: [
                        [0, '#FFF'],
                        [1, '#333']
                    ]
                },
                borderWidth: 0,
                outerRadius: '109%'
            }, {
                backgroundColor: {
                    linearGradient: { x1: 0, y1: 0, x2: 0, y2: 1 },
                    stops: [
                        [0, '#333'],
                        [1, '#FFF']
                    ]
                },
                borderWidth: 1,
                outerRadius: '107%'
            }, {
                // default background
            }, {
                backgroundColor: '#DDD',
                borderWidth: 0,
                outerRadius: '105%',
                innerRadius: '103%'
            }]
        },

        // the value axis
        yAxis: {
            min: 1.50,
            max: 5.50,

            minorTickInterval: 'auto',
            minorTickWidth: 1,
            minorTickLength: 10,
            minorTickPosition: 'inside',
            minorTickColor: '#666',

            tickPixelInterval: 30,
            tickWidth: 2,
            tickPosition: 'inside',
            tickLength: 10,
            tickColor: '#666',
            labels: {
                step: 2,
                rotation: 'auto'
            },
            title: {
                text: '$/Gallon'
            },
            plotBands: [{
                from: 1.50,
                to: 2.50,
                color: '#55BF3B' // green
            }, {
                from: 2.51,
                to: 4.50,
                color: '#DDDF0D' // yellow
            }, {
                from: 4.51,
                to: 5.50,
                color: '#DF5353' // red
            }]
        },

        series: [{
            name: 'Average',
            data: [80],
            tooltip: {
                valueSuffix: ' $/gallon'
            }
        }]
    });

    $('#hos').highcharts({
        chart: {
            type: 'pie',
            options3d: {
                enabled: true,
                alpha: 45
            }
        },
        title: {
            text: 'Drivers HOS'
        },
        subtitle: {
            text: ''
        },
        plotOptions: {
            pie: {
                innerSize: 100,
                depth: 45
            }
        },
        series: [{
            name: 'HOS',
            data: [
                ['IN', 38],
                {
                    name: 'OUT',
                    y: 12,
                    sliced: true,
                    selected: true,
                    color: 'Red'
                }
            ]
        }]
    });


    //#region Active/Inactive Vehicles
    function AIVehicles() {
        var _url = 'getAllVehicles';
        var _data = "loadHistorical=true";
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: AIVehiclesSuccess,
            error: AIVehiclesError
        });
    }

    function AIVehiclesSuccess(result) {
        var active = 0;
        var inactive = 0;
        if (result.length > 0) {
            for (var i = 0; i < result.length; i++) {
                if (result[i].status.length > 0) {
                    inactive++;
                } else {
                    active++;
                }
            }
            Highcharts.getOptions().colors = Highcharts.map(Highcharts.getOptions().colors, function (color) {
                return {
                    radialGradient: {
                        cx: 0.5,
                        cy: 0.3,
                        r: 0.7
                    },
                    stops: [
                        [0, color],
                        [1, Highcharts.Color(color).brighten(-0.3).get('rgb')] // darken
                    ]
                };
            });
            $('#vehicles').highcharts({
                chart: {
                    borderColor: '#cecece',
                    borderRadius: 10,
                    borderWidth: 1,
                    type: 'pie',
                    options3d: {
                        enabled: true,
                        alpha: 45,
                        beta: 0
                    }
                },
                title: {
                    style: { "fontSize": "1.5em" },
                    text: 'Active/Inactive Vehicles:<br />' + moment().format("MM/DD/YYYY hh:mm")
                },
                tooltip: {
                    pointFormat: '{series.name}: <b>{point.y}</b>'
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        dataLabels: {
                            enabled: false,
                        }
                    }
                },
                series: [{
                    type: 'pie',
                    name: 'Vehicles',
                    data: [
                        ['Active', active],
                        {
                            name: 'Inactive',
                            y: inactive,
                            sliced: true,
                            selected: true
                        }
                    ]
                }]
            });

        } else {
            alert('A problem occurred getting the Active/Inactive Vehicles, please reload or contact the administrator.');
        }
    }

    function AIVehiclesError(result, error) {
        alert('A problem occurred getting the Active/Inactive Vehicles, please reload or contact the administrator. Error: ' + error.message);
    }
    //#endregion

    //#region Active/Inactive Vehicles
    function getDriverAnalytics() {
        var _url = 'getDriverAnalytics';
        var _data = "";
        $.ajax({
            type: "GET",
            dataType: "json",
            url: _url,
            data: _data,
            contentType: "application/json; charset=utf-8",
            success: getDriverAnalyticsSuccess,
            error: getDriverAnalyticsError
        });
    }

    function getDriverAnalyticsSuccess(result) {
        $('#drivers').highcharts({
            chart: {
                type: 'pie',
                borderColor: '#cecece',
                borderRadius: 10,
                borderWidth: 1,
                options3d: {
                    enabled: true,
                    alpha: 45
                }
            },
            title: {
                text: 'Driver Status<br />' + moment().format("MM/DD/YYYY hh:mm")
            },
            subtitle: {
                text: ''
            },
            plotOptions: {
                pie: {
                    innerSize: 100,
                    depth: 65,
                    allowPointSelect: true,
                    dataLabels: {
                        enabled: false,
                    }
                }
            },
            series: [{
                name: 'Status',
                data: [
                    ['Driving', result.driving],
                    ['Not Driving', result.notDriving],
                    ['Unassigned', result.notAssigned],
                    ['Vehicles WO/Driver', result.vehiclesWithoutDrivers]
                ]
            }]
        });
    }

    function getDriverAnalyticsError(result, error) {
        alert('A problem occurred getting the Driver Analytics, please reload or contact the administrator. Error: ' + error.message);
    }
    //#endregion
});
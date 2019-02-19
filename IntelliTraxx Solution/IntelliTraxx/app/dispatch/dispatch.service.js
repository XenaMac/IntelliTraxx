(function () {
    'use strict';
    angular.module("intelliTraxxApp").factory("dispatchService", ["$http", dispatchService]);

    function dispatchService($http) {
        return {
            getVehicles: function (isHistorical) {
                return $http({
                    method: 'GET',
                    url:  'getAllVehicles?loadHistorical=' + isHistorical
                }).
                    then(function (response) {
                        return response.data;
                    });
            },
            dispatch: function (vm) {
                return $http({
                    method: 'POST',
                    data: vm,
                    url: "SubmitDispatchRequest"
                }).
                    then(function (response) {
                        return response.data;
                    });
            }
        };
    }
}());
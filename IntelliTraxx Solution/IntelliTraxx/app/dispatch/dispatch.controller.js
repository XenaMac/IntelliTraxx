(function () {
    'use strict';
    angular.module("intelliTraxxApp.dispatch").controller("dispatchController", ['$scope', '$rootScope', 'dispatchService', dispatchController]);
    function dispatchController($scope, $rootScope, dispatchService) {
        $scope.header = "Dispatch Controller";

        $scope.pageConfig = {};

        $scope.dispatch = {
            vehicleId: "",
            address: "",
            city: "",
            state: "",
            zip: "",
            note: ""
        };
        $scope.loadHistoricalVehicles = true;
        $scope.isBusySubmittingDispatchRequest = false;
        $scope.isBusyGettingVehicles = false;
        $scope.vehicles = [];

        $scope.getVehicles = function () {
            $scope.isBusyGettingVehicles = true;
            dispatchService.getVehicles($scope.loadHistoricalVehicles).then(function (result) {
                $scope.isBusyGettingVehicles = false;
                console.log(result);
                $scope.vehicles = result;
            });
        };

        $scope.submitDispatchRequest = function () {
            $scope.isBusySubmittingDispatchRequest = true;
            dispatchService.dispatch($scope.dispatch).then(function (result) {
                $scope.isBusySubmittingDispatchRequest = false;
                if (result === "OK") {
                    toastr.success("Dispatch Request submitted");
                }     
                $scope.dispatch = {
                    vehicleId: "",
                    address: "",
                    city: "",
                    state: "",
                    zip: "",
                    note: ""
                };
            });
        };

        $scope.getVehicles();

    }
}());
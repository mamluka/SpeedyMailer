'use strict';

/* Controllers */


function UploadListController($scope, $http, apiSettings) {
    $http
        .get(apiSettings.baseUrl + '/lists/get')
        .success(function (data) {
            $scope.lists = data;
        });
}
UploadListController.$inject = ['$scope', '$http', 'apiSettings'];

function CreativeController($scope, $http, apiSettings) {
    $http
        .get(apiSettings.baseUrl + '/lists/get')
        .success(function (data) {
            $scope.lists = data;
        });

    $scope.send = function (creative) {
        $http
            .post(apiSettings + '/creative/save')
            .success(function () { alert("Saved") });
    };

}
CreativeController.$inject = ["$scope", "$http", 'apiSettings'];

function ListsController($scope, $http, apiSetttings) {
    $scope.create = function (list) {
        $http.post(apiSetttings.baseUrl + '/lists/create', { name: list.name });
    };
}
ListsController.$inject = ['$scope', "$http", 'apiSettings'];


function AppController() {
}
AppController.$inject = [];

'use strict';

/* Controllers */


function UploadListController($scope,$http) {
    $scope.uploadContactList = function () {
        $http.post('http://localhost:88/lists/upload', $scope.listFile);
    };
}
UploadListController.$inject = ['$scope','$http'];


function AppController() {
}
AppController.$inject = [];

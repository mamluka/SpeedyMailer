'use strict';

/* Controllers */


function UploadListController($scope,$http) {
    $scope.upload = function (data) {
        $http.post('http://api.speedymailer/lists/upload', data);
    };
}
UploadListController.$inject = ['$scope','$http'];


function AppController() {
}
AppController.$inject = [];

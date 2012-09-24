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

function CreativeController($scope, $http, listResource, creativeResource, templateResource) {
    $scope.lists = listResource.query();
    $scope.unsubscribeTemplate = templateResource.query({ templateType: 'unsubscribe' });

    $scope.save = function (creativeModel) {
        var creative = new creativeResource({
            listId: creativeModel.list.Id,
            subject: creativeModel.subject,
            daelUrl: creativeModel.dealUrl,
            body: creativeModel.body,
            templateId: creativeModel.unsubscribeTemplate.Id
        });

        creative.$save();
    };

}
CreativeController.$inject = ["$scope", "$http", 'List', 'Creative', 'Template'];

function ListsController($scope, listResource) {
    $scope.save = function (listModel) {
        var list = new listResource({ name: listModel.name });
        list.$save();
    };
}
ListsController.$inject = ['$scope', 'List'];


function AppController() {
}
AppController.$inject = [];

function PlaygroundController($scope, list) {
    $scope.lists = list.query();

}
PlaygroundController.$inject = ['$scope', 'List'];

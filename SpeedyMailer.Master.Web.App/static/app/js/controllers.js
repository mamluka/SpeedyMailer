'use strict';

/* Controllers */


function UploadListController($scope, $http, listResource) {
    $scope.lists = listResource.query();
    $scope.baseUrl = config.apiUrl;
}
UploadListController.$inject = ['$scope', '$http', 'List'];

function CreativeController($scope, $http, listResource, creativeResource, templateResource) {
    $scope.lists = listResource.query();
    $scope.unsubscribeTemplates = templateResource.query({ templateType: 'unsubscribe' });

    $scope.save = function (creativeModel) {
        var creative = new creativeResource({
            listId: creativeModel.list.Id,
            subject: creativeModel.subject,
            dealUrl: creativeModel.dealUrl,
            fromName: creativeModel.fromName,
            fromAddress: creativeModel.fromAddress,
            htmlBody: creativeModel.htmlBody,
            textBody: creativeModel.textBody,
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

function TemplatesController($scope, $http, templateResource, templateTypeResource) {
    $scope.types = templateTypeResource.query();

    $scope.save = function (templateModel) {
        var list = new templateResource({
            type: templateModel.type.name
        });

        list.body = templateModel.body;

        list.$save();
    };
}
TemplatesController.$inject = ['$scope', '$http', 'Template', 'TemplateType'];

function AppController() {
}
AppController.$inject = [];

function SendingController($scope, $http, creativeResource) {
    $scope.creatives = creativeResource.query();

    $scope.send = function (creativeModel) {
        $http.post('/creatives/send'.Apify(), { Id: creativeModel.Id });
    };
}
SendingController.$inject = ['$scope', '$http', 'Creative'];

function RulesController($scope, ruleResource) {
    var conditions = $scope.conditions = [];
    $scope.addCondition = function (condition) {
        conditions.push(condition);
    };

    $scope.removeCondition = function (condition) {
        conditions.splice(conditions.indexOf(condition), 1);
    };

    $scope.saveRule = function (group, interval) {
        var rule = new ruleResource({
            type: 'Interval',
            conditions: conditions,
            group: group,
            interval: interval
        });

        rule.$save();

        conditions = $scope.conditions = [];
        $scope.group = '';
        $scope.interval = "";
    };
}
RulesController.$inject = ['$scope', 'Rule'];

function PlaygroundController($scope, list) {
    $scope.lists = list.query();

}
PlaygroundController.$inject = ['$scope', 'List'];

function HeuristicsController($scope, deliveryHeuristicsResource) {

    var deliveryHeuristics = deliveryHeuristicsResource.query(function () {
        $scope.rules = deliveryHeuristics[0].rules || [];
        $scope.Id = deliveryHeuristics[0].Id;
    });

    var rules = $scope.rules;

    $scope.addHardBounce = function (hardBounce) {
        rules.push(hardBounce);
    };

    $scope.removeHardBounces = function (hardBounce) {
        rules.splice(hardBounce.indexOf(hardBounce), 1);
    };

    $scope.saveHeuristics = function () {
        var rule = new deliveryHeuristicsResource({
            rules: rules,
            Id: $scope.Id
        });

        rule.$save();
    };
}
HeuristicsController.$inject = ['$scope', 'DeliveryHeuristics'];

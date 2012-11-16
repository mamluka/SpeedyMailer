'use strict';

/* Controllers */


function UploadListController($scope, $http, listResource) {
    $scope.lists = listResource.query();
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
        $http.post('http://speedymailer.api/creatives/send', { Id: creativeModel.Id });
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

        $scope.conditions = [];
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

    var hardBounces;
    var ipBlocks;
    
    var deliveryHeuristics = deliveryHeuristicsResource.query(function () {
        $scope.hardBounces = deliveryHeuristics[0].HardBounceRules || [];
        $scope.ipBlocks = deliveryHeuristics[0].IpBlockingRules || [];
        $scope.Id = deliveryHeuristics[0].Id;

        hardBounces = $scope.hardBounces;
        ipBlocks = $scope.ipBlocks;
    });


    

    $scope.addHardBounce = function (hardBounce) {
        hardBounces.push(hardBounce);
    };

    $scope.removeHardBounces = function (hardBounce) {
        hardBounces.splice(hardBounce.indexOf(hardBounce), 1);
    };

    $scope.addIpBlock = function (ipBlock) {
        ipBlocks.push(ipBlock);
    };

    $scope.removeIpBlock = function (ipBlock) {
        ipBlocks.splice(ipBlock.indexOf(ipBlock), 1);
    };

    $scope.saveHeuristics = function () {
        var rule = new deliveryHeuristicsResource({
            HardBounceRules: hardBounces,
            IpBlockingRules: ipBlocks,
            Id: $scope.Id
        });

        rule.$save();
    };
}
HeuristicsController.$inject = ['$scope', 'DeliveryHeuristics'];

'use strict';

/* Services */

// Demonstrate how to register services
// In this case it is a simple value service.

String.prototype.Apify = function () { return config.apiUrl + this; };

angular.module('SpeedyMailer.settings', []).
  value('apiSettings', { baseUrl: "http://speedymailer.api" });

angular.module('SpeedyMailer.services', ['ngResource']).
    factory('List', function ($resource) {
        return $resource('/lists/list/:listId'.Apify(),
            {
                listId: '@id'
            });
    }).
    factory('Creative', function ($resource) {
        return $resource('/creatives/:creativeId'.Apify(),
            {
                creativeId: '@id'
            });
    }).
    factory('Template', function ($resource) {
        return $resource('/templates/:templateType/:templateId'.Apify(),
            {
                templateId: '@id',
                templateType: '@type'
            });
    }).
    factory('TemplateType', function ($resource) {
        return $resource('/templates/types/list'.Apify());
    }).
    factory('Rule', function ($resource) {
        return $resource('/rule'.Apify());
    }).
    factory('DeliveryHeuristics', function ($resource) {
        return $resource('/heuristics/delivery'.Apify());
    }).
    factory('Drones', function ($resource) {
        return $resource('/drones'.Apify());
    });
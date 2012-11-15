'use strict';

/* Services */


// Demonstrate how to register services
// In this case it is a simple value service.
angular.module('SpeedyMailer.services', []).
  value('version', '0.1');

angular.module('SpeedyMailer.settings', []).
  value('apiSettings', { baseUrl: "http://speedymailer.api" });

angular.module('SpeedyMailer.services', ['ngResource']).
    factory('List', function($resource) {
        return $resource('http://speedymailer.api/lists/list/:listId',
            {
                 listId: '@id'
            });
    }).
    factory('Creative', function($resource) {
        return $resource('http://speedymailer.api/creatives/:creativeId',
            {
                creativeId: '@id'
            });
    }).
    factory('Template', function($resource) {
        return $resource('http://speedymailer.api/templates/:templateType/:templateId',
            {
                templateId: '@id',
                templateType: '@type'
            });
    }).
    factory('TemplateType', function($resource) {
        return $resource('http://speedymailer.api/templates/types/list');
    }).
    factory('Rule', function ($resource) {
        return $resource('http://speedymailer.api/rules');
    }).
    factory('DeliveryHeuristics', function ($resource) {
        return $resource('http://speedymailer.api/heuristics/delivery');
    });

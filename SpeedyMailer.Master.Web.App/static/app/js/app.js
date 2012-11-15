'use strict';

// Declare app level module which depends on filters, and services
angular.module('SpeedyMailer', ['SpeedyMailer.filters', 'SpeedyMailer.services', 'SpeedyMailer.directives', 'SpeedyMailer.settings']).
  config(['$routeProvider', function ($routeProvider) {
      $routeProvider.when('/upload-contact-list', { templateUrl: 'partials/upload-contact-list.html', controller: UploadListController });
      $routeProvider.when('/create-creative', { templateUrl: 'partials/create-creative.html', controller: CreativeController });
      $routeProvider.when('/create-list', { templateUrl: 'partials/create-list.html', controller: ListsController });
      $routeProvider.when('/create-template', { templateUrl: 'partials/create-template.html', controller: TemplatesController });
      $routeProvider.when('/create-rules', { templateUrl: 'partials/create-rules.html', controller: RulesController });
      $routeProvider.when('/send-creatives', { templateUrl: 'partials/send-creatives.html', controller: SendingController });
      $routeProvider.when('/heuristics', { templateUrl: 'partials/heuristics.html', controller: HeuristicsController });
      $routeProvider.when('/playground', { templateUrl: 'partials/playground.html', controller: PlaygroundController });
      $routeProvider.when('/not-supported', { templateUrl: 'partials/not-supported.html', controller: AppController });
      $routeProvider.otherwise({ redirectTo: '/not-supported' });
  }]);

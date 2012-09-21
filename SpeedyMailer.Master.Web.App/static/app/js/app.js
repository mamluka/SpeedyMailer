'use strict';

// Declare app level module which depends on filters, and services
angular.module('SpeedyMailer', ['SpeedyMailer.filters', 'SpeedyMailer.services', 'SpeedyMailer.directives']).
  config(['$routeProvider', function($routeProvider) {
    $routeProvider.when('/upload-contact-list', {templateUrl: 'partials/upload-contact-list.html', controller: UploadListController});
    $routeProvider.when('/create-creative', {templateUrl: 'partials/create-creative.html', controller: CreativeController});
    $routeProvider.when('/create-list', {templateUrl: 'partials/create-list.html', controller: ListsController});
    $routeProvider.when('/not-supported', {templateUrl: 'partials/not-supported.html', controller: AppController});
    $routeProvider.otherwise({redirectTo: '/not-supported'});
  }]);

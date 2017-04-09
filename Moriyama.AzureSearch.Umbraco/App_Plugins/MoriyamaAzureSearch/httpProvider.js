angular.module('umbraco.services').config([
   '$httpProvider',
   function ($httpProvider) {

       $httpProvider.interceptors.push(function ($q) {
           return {
               'request': function (request) {

                   if (request.url.indexOf("/umbraco/backoffice/UmbracoApi/Entity/SearchAll?query=") > -1) {

                       var query = request.url.substring(request.url.indexOf("?query=") + 7);
                       request.url = '/umbraco/backoffice/api/BackOfficeAzureSearchApi/Search?query=' + escape(query);
                   }

                   return request || $q.when(request);
               }
           };
       });
   }]);
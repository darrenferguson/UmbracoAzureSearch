function moriyamaAzureSearchController($scope, umbRequestHelper, $filter, $log, $http) {

    $scope.configLoaded = false;

    $scope.reindexModel = {
        content: false,
        media: false,
        members: false
    };

    $http.get('/umbraco/backoffice/api/AzureSearchApi/GetConfiguration').then(function (response) {      
        $scope.config = response.data;
        $scope.configLoaded = true;
    });

    $http.get('/umbraco/backoffice/api/AzureSearchApi/GetStandardUmbracoFields').then(function (response) {
        $scope.umbracoFields = response.data;
    });


    $http.get('/umbraco/backoffice/api/AzureSearchApi/GetSearchIndexes').then(function (response) {
        $scope.searchIndexes = response.data;
    });
    
    $scope.updateServiceName = function() {

        $scope.updating = true;
        $http.get('/umbraco/backoffice/api/AzureSearchApi/ServiceName?value=' + escape($scope.config.SearchServiceName)).then(function (response) {
            $scope.updating = false;
        });

        
    };

    $scope.updateServiceApiKey = function () {

        $scope.updating = true;
        $http.get('/umbraco/backoffice/api/AzureSearchApi/ServiceApiKey?value=' + escape($scope.config.SearchServiceAdminApiKey)).then(function (response) {
            $scope.updating = false;
        });

        
    };

    $scope.testConfig = function () {
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetTestConfig').then(function (response) {
            $scope.configTest = response.data;
            $scope.canConnect = $scope.configTest.includes("Connected");
            $scope.showConfigTest = true;
        });
    };

    $scope.dropCreateIndex = function () {

        if (!confirm('Are you sure!'))
            return;

        $scope.showIndexDropCreate = true;
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetDropCreateIndex').then(function (response) {
            $scope.dropCreateResult = response.data;      
        });
    };

    $scope.reindex = function () {
        
        if (!confirm('Are you sure!'))
            return;
        $scope.finishedIndexing = false;
        $scope.showReIndexContent = false;

        $http.post('/umbraco/backoffice/api/AzureSearchApi/ReIndex', $scope.reindexModel).then(function (response) {
            $scope.reIndexContentResult = response.data;

            $scope.reindexContentPage(response.data.SessionId, 1);
        });
    };

    $scope.reindexContentPage = function (sessionId, page) {
        if ($scope.reindexModel.content) {

            $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexContent?sessionId=' + escape(sessionId) + '&page=' + page).then(function(response) {
                $scope.reIndexContentResult = response.data;

                var docsFinished = response.data.DocumentsQueued == 0;

                if (!response.data.Error && !docsFinished) {
                    $scope.reindexContentPage(sessionId, page + 1);
                } else {
                    $scope.reindexMediaPage(sessionId, 1);
                }
            });
        } else {
            $scope.reindexMediaPage(sessionId, 1);
        }
    };
    
    $scope.reindexMediaPage = function (sessionId, page) {
        if ($scope.reindexModel.media) {

            $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexMedia?sessionId=' + escape(sessionId) + '&page=' + page).then(function(response) {
                $scope.reIndexContentResult = response.data;

                var mediaFinished = response.data.MediaQueued == 0;

                if (!response.data.Error && !mediaFinished) {
                    $scope.reindexMediaPage(sessionId, page + 1);
                } else {
                    $scope.reindexMemberPage(sessionId, 1);
                }
            });
        } else {
            $scope.reindexMemberPage(sessionId, 1);
        }
    };

    $scope.reindexMemberPage = function (sessionId, page) {
        if ($scope.reindexModel.member) {
            $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexMember?sessionId=' + escape(sessionId) + '&page=' + page).then(function(response) {
                $scope.reIndexContentResult = response.data;

                var membersFinished = response.data.MembersQueued == 0;

                if (!response.data.Error && !membersFinished) {
                    $scope.reindexMemberPage(sessionId, page + 1);
                } else {
                    $scope.finishedIndexing = true;
                }
            });
        } else {
            $scope.finishedIndexing = true;
        }
    };

    $scope.saveConfig = function() {
        $http.post('/umbraco/backoffice/api/AzureSearchApi/SetConfiguration', $scope.config).then(function (response) {      
            $scope.config = response.data;
            $scope.configLoaded = true;
        });
    }
}

angular.module("umbraco").controller("Umbraco.Dashboard.MoriyamaAzureSearchController", moriyamaAzureSearchController);
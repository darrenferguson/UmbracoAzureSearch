function moriyamaAzureSearchController($scope, umbRequestHelper, $log, $http) {

    $scope.configLoaded = false;

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
    
    $scope.testConfig = function () {
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetTestConfig').then(function (response) {
            $scope.configTest = response.data;
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

    $scope.reindexContent = function () {
        
        if (!confirm('Are you sure!'))
            return;
        $scope.finishedIndexing = false;
        $scope.TypeProcessing = 'content';
        $scope.showReIndexContent = false;
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexContent').then(function (response) {
            $scope.reIndexContentResult = response.data;
            $scope.showReIndexContent = true;
            $scope.reindexContentPage(response.data.SessionId, 1);
        });
    };

    $scope.reindexContentPage = function (sessionId, page) {
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexContent?sessionId=' + escape(sessionId) + '&page=' + page).then(function (response) {
            $scope.reIndexContentResult = response.data;

            if (!response.data.Error && !response.data.Finished) {
                $scope.reindexContentPage(sessionId, page + 1);
            } else if (response.data.Finished) {
                $scope.TypeProcessing = 'media';
                $scope.reindexMediaPage(response.data.SessionId, 1);
            }
        });
    };
    
    $scope.reindexMediaPage = function (sessionId, page) {

        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexMedia?sessionId=' + escape(sessionId) + '&page=' + page).then(function (response) {
            $scope.reIndexContentResult = response.data;
            if (!response.data.Error && !response.data.Finished) {
                $scope.reindexMediaPage(sessionId, page + 1);
            } else if (response.data.Finished) {
                $scope.TypeProcessing = 'member';
                $scope.reindexMemberPage(response.data.SessionId, 1);
            }
        });
    };

    $scope.reindexMemberPage = function (sessionId, page) {

        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexMember?sessionId=' + escape(sessionId) + '&page=' + page).then(function (response) {
            $scope.reIndexContentResult = response.data;
            if (!response.data.Error && !response.data.Finished) {
                $scope.reindexMemberPage(sessionId, page + 1);
            } else if (response.data.Finished) {
                $scope.finishedIndexing = true;
            }
        });
    };

}

angular.module("umbraco").controller("Umbraco.Dashboard.MoriyamaAzureSearchController", moriyamaAzureSearchController);
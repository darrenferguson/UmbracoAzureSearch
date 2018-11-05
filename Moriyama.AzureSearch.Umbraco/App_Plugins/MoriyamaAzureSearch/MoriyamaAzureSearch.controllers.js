function moriyamaAzureSearchController($scope, umbRequestHelper, $filter, $log, $http) {

    $scope.configLoaded = false;
	$scope.indexesLoaded = false;

    $http.get('/umbraco/backoffice/api/AzureSearchApi/GetConfiguration').then(function (response) {      
        $scope.config = response.data;
        $scope.configLoaded = true;
    });

    $http.get('/umbraco/backoffice/api/AzureSearchApi/GetStandardUmbracoFields').then(function (response) {
        $scope.umbracoFields = response.data;
    });


	$scope.loadIndexes = function () {
		$scope.indexesLoaded = false;
		$http.get('/umbraco/backoffice/api/AzureSearchApi/GetSearchIndexes').then(function (response) {
			$scope.searchIndexes = response.data;
			$scope.indexesLoaded = true;
		});
	};
	$scope.loadIndexes();
    
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
		$scope.loadIndexes();
    };


    $scope.dropCreateIndex = function () {

        if (!confirm('Are you sure!'))
            return;

		$scope.indexesLoaded = false;
        $scope.showIndexDropCreate = true;
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetDropCreateIndex').then(function (response) {
			$scope.dropCreateResult = response.data;      
			$scope.loadIndexes();
        });
    };

    $scope.reindexAll = function () {
        
        if (!confirm('Are you sure!'))
            return;

        $scope.finishedIndexing = false;
        $scope.showReIndexContent = false;

		$scope.reindexResultStatus = "";
		$scope.reindexResultContent = "";
		$scope.reindexResultMedia = "";

        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexContent').then(function (response) {
            $scope.reIndexContentResult = response.data;

            $scope.reindexContentPage(response.data.SessionId, 1);
            $scope.showReIndexContent = true;
        });
    };

	$scope.reindexContentPage = function (sessionId, page) {
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexContent?sessionId=' + escape(sessionId) + '&page=' + page).then(function (response) {
            $scope.reIndexContentResult = response.data;
			$scope.reindexResultStatus = $scope.getIndexingStatusMessage($scope.reIndexContentResult.DocumentsProcessed, $scope.reIndexContentResult.DocumentCount, "content items");

            var docsFinished = response.data.Finished;
			
            if (!response.data.Error && !docsFinished) {
				$scope.reindexContentPage(sessionId, page + 1);				
            } else {
				$scope.reindexResultContent = $scope.reindexResultStatus;
				$scope.reindexMediaPage(sessionId, 1);
            }
        });
    };
    
    $scope.reindexMediaPage = function (sessionId, page) {
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexMedia?sessionId=' + escape(sessionId) + '&page=' + page).then(function (response) {
            $scope.reIndexContentResult = response.data;
			$scope.reindexResultStatus = $scope.getIndexingStatusMessage($scope.reIndexContentResult.DocumentsProcessed, $scope.reIndexContentResult.DocumentCount, "media items");

            var mediaFinished = response.data.Finished;

            if (!response.data.Error && !mediaFinished) {
                $scope.reindexMediaPage(sessionId, page + 1);
			} else {
				$scope.reindexResultMedia = $scope.reindexResultStatus;
                $scope.reindexMemberPage(sessionId, 1);
            }
        });

    };

    $scope.reindexMemberPage = function (sessionId, page) {
        $http.get('/umbraco/backoffice/api/AzureSearchApi/GetReIndexMember?sessionId=' + escape(sessionId) + '&page=' + page).then(function (response) {
            $scope.reIndexContentResult = response.data;
			$scope.reindexResultStatus = $scope.getIndexingStatusMessage($scope.reIndexContentResult.DocumentsProcessed, $scope.reIndexContentResult.DocumentCount, "members");

            var membersFinished = response.data.Finished;

            if (!response.data.Error && !membersFinished) {
                $scope.reindexMemberPage(sessionId, page + 1);
			} else {
                $scope.finishedIndexing = true;
            }
        });
	};


	$scope.getIndexingStatusMessage = function(processed, documentCount, typeProcessing) {
		return processed + " of " + documentCount + " " + typeProcessing + " processed.";
	}

    $scope.saveConfig = function() {
        $http.post('/umbraco/backoffice/api/AzureSearchApi/SetConfiguration', $scope.config).then(function (response) {      
            $scope.config = response.data;
            $scope.configLoaded = true;
        });
    }
}

angular.module("umbraco").controller("Umbraco.Dashboard.MoriyamaAzureSearchController", moriyamaAzureSearchController);


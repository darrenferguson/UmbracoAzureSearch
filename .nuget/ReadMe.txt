To create nuget packages, run following command line*:

nuget pack ..\Moriyama.AzureSearch.Umbraco\Moriyama.AzureSearch.Umbraco.nuspec
nuget pack ..\Moriyama.AzureSearch.Umbraco.Application\Moriyama.AzureSearch.Umbraco.Application.nuspec


* make sure:
	- nuspec files are updated (ex. versioning, files to include/exclude)
	- AssemblyInfo version matches nuspec file version (just for consistent)
set NugetUserId=<USER_ID>
set NugetPersonalAccessToken=<OUR_API_KEY>

dotnet pack

nuget.exe push -Source "FoundryBlazor" -ApiKey OUR_API_KEY bin\Debug\FoundryBlazor.1.2.3.nupkg




set NugetUserId=iobt
set NugetPersonalAccessToken=vciqzkoknrkcrinrkwxxbr62aal5zhad3kzaextnvc7mq7bncsda
nuget restore
dotnet clean
dotnet restore
dotnet build
dotnet run (edited) 
set NugetUserId=<USER_ID>
set NugetPersonalAccessToken=<OUR_API_KEY>

dotnet pack
nuget.exe push -Source "FoundryBlazor" -ApiKey vciqzkoknrkcrinrkwxxbr62aal5zhad3kzaextnvc7mq7bncsda bin\Debug\FoundryBlazor.1.4.0.nupkg

copy bin\Debug\FoundryBlazor.1.4.0.nupkg D:\Users\gtful\local.nuget
nuget.exe push -Source "FoundryBlazor" -ApiKey OUR_API_KEY bin\Debug\FoundryBlazor.1.4.0.nupkg




set NugetUserId=iobt
set NugetPersonalAccessToken=vciqzkoknrkcrinrkwxxbr62aal5zhad3kzaextnvc7mq7bncsda
nuget restore
dotnet clean
dotnet restore
dotnet build
dotnet run (edited) 

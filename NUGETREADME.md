set NugetUserId=<USER_ID>
set NugetPersonalAccessToken=<OUR_API_KEY>

dotnet pack
nuget.exe push -Source "FoundryBlazor" -ApiKey vciqzkoknrkcrinrkwxxbr62aal5zhad3kzaextnvc7mq7bncsda bin\Debug\FoundryBlazor.1.2.6.nupkg

copy bin\Debug\FoundryBlazor.1.2.6.nupkg D:\Users\gtful\local.nuget
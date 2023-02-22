dotnet pack
nuget.exe restore
nuget.exe push -Source "FoundryBlazor" -ApiKey OUR_API_KEY bin\Debug\FoundryBlazor.1.0.1.nupkg

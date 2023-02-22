dotnet pack
nuget.exe restore
nuget.exe push -Source "FoundryBlazor" -ApiKey OUR_API_KEY bin\Debug\FoundryBlazor.1.0.2.nupkg


dotnet nuget push bin\Debug\FoundryBlazor.1.0.2.nupkg --source "FoundryBlazor"


[-d|--disable-buffering] [--force-english-output]
    [--interactive] [-k|--api-key <API_KEY>] [-n|--no-symbols]
    [--no-service-endpoint] [-s|--source <SOURCE>] [--skip-duplicate]
    [-sk|--symbol-api-key <API_KEY>] [-ss|--symbol-source <SOURCE>]
    [-t|--timeout <TIMEOUT>]

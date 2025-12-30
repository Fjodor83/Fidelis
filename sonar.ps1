$token = "421f64fb00387fceb5bb4e1005624ec4e07e86ea"

dotnet sonarscanner begin `
    /k:"Fjodor83_FidelisApp" `
    /o:"fjodor83-1" `
    /d:sonar.host.url="https://sonarcloud.io" `
    /d:sonar.token="$token" `
    /d:sonar.cs.vscoveragexml.reportsPaths="coverage.xml"

dotnet build --no-incremental

dotnet-coverage collect "dotnet test" -f xml -o "coverage.xml"

dotnet sonarscanner end /d:sonar.token="$token"

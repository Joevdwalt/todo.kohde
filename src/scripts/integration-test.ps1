<#
.SYNOPSIS
Runs opt-in integration-test projects and writes TRX results.
#>

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
$resultsRoot = Join-Path $repositoryRoot 'artifacts/integration-test-results'
$projects = @(Get-ChildItem (Join-Path $repositoryRoot 'src') -Recurse -Filter '*.IntegrationTests.csproj' |
    Sort-Object FullName)

if ($projects.Count -eq 0) {
    throw 'No integration-test projects were found.'
}

foreach ($project in $projects) {
    $projectResults = Join-Path $resultsRoot $project.BaseName
    New-Item -ItemType Directory -Force -Path $projectResults | Out-Null
    dotnet test --project $project.FullName --configuration Release `
        --report-trx --results-directory $projectResults
    if ($LASTEXITCODE -ne 0) {
        throw "Integration tests failed for $($project.BaseName) with exit code $LASTEXITCODE."
    }
}

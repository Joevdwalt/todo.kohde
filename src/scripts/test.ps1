<#
.SYNOPSIS
Runs deterministic unit-test projects and writes TRX results.
#>

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
$resultsRoot = Join-Path $repositoryRoot 'artifacts/test-results'
$projects = @(Get-ChildItem (Join-Path $repositoryRoot 'src') -Recurse -Filter '*.Tests.csproj' |
    Where-Object { $_.Name -notlike '*.IntegrationTests.csproj' } | Sort-Object FullName)

if ($projects.Count -eq 0) {
    throw 'No unit-test projects were found. Scaffold tests before running task test.'
}

foreach ($project in $projects) {
    $projectResults = Join-Path $resultsRoot $project.BaseName
    New-Item -ItemType Directory -Force -Path $projectResults | Out-Null
    dotnet test --project $project.FullName --configuration Release `
        --report-trx --results-directory $projectResults
    if ($LASTEXITCODE -ne 0) {
        throw "Unit tests failed for $($project.BaseName) with exit code $LASTEXITCODE."
    }
}

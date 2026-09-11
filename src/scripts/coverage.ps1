<#
.SYNOPSIS
Collects unit-test coverage and generates merged reports.
#>

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
$coverageRoot = Join-Path $repositoryRoot 'artifacts/coverage'
$rawRoot = Join-Path $coverageRoot 'raw'
$reportRoot = Join-Path $coverageRoot 'report'
$projects = @(Get-ChildItem (Join-Path $repositoryRoot 'src') -Recurse -Filter '*.Tests.csproj' |
    Where-Object { $_.Name -notlike '*.IntegrationTests.csproj' } | Sort-Object FullName)

if ($projects.Count -eq 0) {
    throw 'No unit-test projects were found. Scaffold tests before running task coverage.'
}

if (Test-Path $coverageRoot) { Remove-Item $coverageRoot -Recurse -Force }
New-Item -ItemType Directory -Force -Path $rawRoot, $reportRoot | Out-Null

dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw "Local tool restore failed with exit code $LASTEXITCODE." }

$coverageFiles = [System.Collections.Generic.List[string]]::new()
foreach ($project in $projects) {
    $projectResults = Join-Path $rawRoot $project.BaseName
    $coverageFile = Join-Path $projectResults 'coverage.cobertura.xml'
    New-Item -ItemType Directory -Force -Path $projectResults | Out-Null
    dotnet test --project $project.FullName --configuration Release `
        --report-trx --results-directory $projectResults --coverage `
        --coverage-output $coverageFile --coverage-output-format cobertura
    if ($LASTEXITCODE -ne 0) {
        throw "Coverage failed for $($project.BaseName) with exit code $LASTEXITCODE."
    }
    $coverageFiles.Add($coverageFile)
}

dotnet tool run reportgenerator -- `
    "-reports:$($coverageFiles -join ';')" "-targetdir:$reportRoot" `
    '-reporttypes:Html;Cobertura;TextSummary'
if ($LASTEXITCODE -ne 0) { throw "Coverage report generation failed with exit code $LASTEXITCODE." }
Copy-Item (Join-Path $reportRoot 'Cobertura.xml') (Join-Path $coverageRoot 'coverage.cobertura.xml') -Force
Get-Content (Join-Path $reportRoot 'Summary.txt')

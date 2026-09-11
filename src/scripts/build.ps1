<#
.SYNOPSIS
Builds every production project in the repository.
#>

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
$projects = @(Get-ChildItem (Join-Path $repositoryRoot 'src') -Recurse -Filter '*.csproj' |
    Where-Object { $_.Name -notlike '*.Tests.csproj' -and $_.Name -notlike '*.IntegrationTests.csproj' } |
    Sort-Object FullName)

if ($projects.Count -eq 0) {
    throw 'No production projects were found. Scaffold the application before running task build.'
}

foreach ($project in $projects) {
    dotnet build $project.FullName --configuration Release --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Production build failed for $($project.BaseName) with exit code $LASTEXITCODE."
    }
}

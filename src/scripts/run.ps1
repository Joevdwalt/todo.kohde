<#
.SYNOPSIS
Runs the Todo App web project.
#>

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Resolve-Path (Join-Path $PSScriptRoot '../..')
$webProject = Join-Path $repositoryRoot 'src/TodoApp.Web/TodoApp.Web.csproj'

dotnet run --project $webProject
exit $LASTEXITCODE

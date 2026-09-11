<#
.SYNOPSIS
Upgrades this project from a Kohde template checkout.
#>

param(
    [Parameter(Mandatory)][string] $TemplateRoot,
    [string] $Target = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedTemplateRoot = [IO.Path]::GetFullPath($TemplateRoot)
if (-not (Test-Path $resolvedTemplateRoot -PathType Container)) {
    throw "TemplateRoot does not exist: $resolvedTemplateRoot. Run: task template-upgrade TEMPLATE=../kohde-ai-project-template"
}

$upgradeScript = Join-Path $resolvedTemplateRoot 'src/scripts/upgrade.ps1'
if (-not (Test-Path $upgradeScript -PathType Leaf)) {
    throw "TemplateRoot does not contain src/scripts/upgrade.ps1: $resolvedTemplateRoot"
}

$resolvedTarget = [IO.Path]::GetFullPath($Target)
& pwsh -NoProfile -File $upgradeScript -Target $resolvedTarget
exit $LASTEXITCODE

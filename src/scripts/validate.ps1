<#
.SYNOPSIS
Runs the complete automated validation gate.
#>

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

foreach ($script in @('build.ps1', 'test.ps1', 'integration-test.ps1')) {
    & (Join-Path $PSScriptRoot $script)
    if ($LASTEXITCODE -ne 0) {
        throw "Validation step $script failed with exit code $LASTEXITCODE."
    }
}

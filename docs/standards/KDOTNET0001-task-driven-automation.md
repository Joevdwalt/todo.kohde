# KDOTNET 0001: Task-Driven .NET Automation

## Status

Accepted

## Standard

.NET projects use the root `TaskFile.yml` as the supported entrypoint for
repository automation. Matching scripts use PowerShell, live under
`src/scripts/`, and use lowercase kebab-case action names.

Each script includes concise comment-based help, a `param` block,
`Set-StrictMode -Version Latest`, and `$ErrorActionPreference = 'Stop'`.
Tasks and scripts share names. Direct script execution is not the normal
developer or CI workflow.

The root `global.json` controls the SDK and Microsoft Testing Platform selection
for every project. Do not add nested `global.json` files without a documented
project RDR.

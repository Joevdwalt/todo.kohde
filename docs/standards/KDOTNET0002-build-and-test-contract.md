# KDOTNET 0002: Build and Test Contract

## Status

Accepted

## Standard

Production projects target .NET 10 with nullable reference types, implicit
usings, recommended analysis, and warnings as errors. Central package management
is enabled at the repository root.

When application projects are scaffolded, provide these tasks:

- `task build` builds every production project and no test projects.
- `task test` runs deterministic `<ProjectName>.Tests` projects and writes TRX.
- `task integration-test` explicitly runs `<ProjectName>.IntegrationTests`.
- `task coverage` runs unit tests with Microsoft Testing Platform coverage and
  produces raw Cobertura, merged Cobertura, HTML, and text summaries beneath
  ignored `artifacts/` paths.

Unit tests use xUnit v3 on Microsoft Testing Platform, FluentAssertions, the TRX
extension, and the Microsoft code-coverage extension. They do not use VSTest,
the Visual Studio xUnit runner, or Coverlet collectors. ReportGenerator is a
repository-local .NET tool. Do not enforce a coverage threshold until the
project establishes and records a meaningful baseline.

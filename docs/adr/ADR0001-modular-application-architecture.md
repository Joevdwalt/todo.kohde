# ADR 0001: Modular Application Architecture

## Status

Accepted

## Context

The todo MVP combines deterministic lifecycle and discovery rules, a real JSON
filesystem boundary, and an interactive Blazor UI. Four agents will implement
these areas concurrently, so ownership and dependency direction must prevent
both architectural coupling and routine merge conflicts.

## Decision

Use four .NET 10 production projects under `src/`:

- `TodoApp.Core` owns the todo model, validation, lifecycle orchestration,
  application results, time and identifier boundaries, and `ITodoStore`.
- `TodoApp.Discovery` owns pure view, search, filter, and sort behavior and
  depends only on Core.
- `TodoApp.Persistence.Json` implements `ITodoStore` and depends only on Core.
- `TodoApp.Web` is the ASP.NET Core Blazor Web App and composition root. It
  depends on Core, Discovery, and Persistence.Json.

The Web project uses Interactive Server render mode and exposes no external
HTTP API. Core has no infrastructure or UI dependencies. Discovery performs no
I/O. Persistence owns no product policy beyond validating and safely applying
the storage contract.

Each production project has a predictably named test project. Real filesystem
tests use `TodoApp.Persistence.Json.IntegrationTests`; deterministic behavior
uses ordinary `*.Tests` projects.

## Consequences

Agents can own non-overlapping project trees and integrate through frozen Core
contracts. Core must stabilize first, after which Discovery and Persistence can
merge independently and Web can complete composition. The architecture adds
project boundaries, but keeps deterministic logic isolated and makes storage
replaceable without adding an unused external API.

## References

- [SPEC0001](../spec/SPEC0001-todo-lifecycle.md)
- [SPEC0002](../spec/SPEC0002-todo-organization.md)
- [SPEC0003](../spec/SPEC0003-todo-discovery.md)
- [SPEC0004](../spec/SPEC0004-json-persistence.md)
- [SPEC0005](../spec/SPEC0005-blazor-user-experience.md)
- [KSTD0003](../standards/KSTD0003-boundaries-and-testability.md)
- [KSTD0006](../standards/KSTD0006-application-source-structure.md)

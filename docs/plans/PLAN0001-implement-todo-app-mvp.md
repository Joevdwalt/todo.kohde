# PLAN 0001: Implement Todo App MVP

## Status

Accepted

## Analysis Inputs

- Product brief or outcome: Deliver a local single-user todo web app on .NET 10
  with safe JSON persistence.
- Capability: Complete todo lifecycle, organization, discovery, persistence,
  and Blazor interaction.
- Acceptance criteria: All acceptance clauses in SPEC0001 through SPEC0005.
- Supporting analysis or references: [Todo App README](../../README.md)
- Specifications: [SPEC0001](../spec/SPEC0001-todo-lifecycle.md),
  [SPEC0002](../spec/SPEC0002-todo-organization.md),
  [SPEC0003](../spec/SPEC0003-todo-discovery.md),
  [SPEC0004](../spec/SPEC0004-json-persistence.md), and
  [SPEC0005](../spec/SPEC0005-blazor-user-experience.md)
- Accepted program design: [Todo Core](../program-design/PROGRAM-DESIGN0001-todo-core.md),
  [JSON Persistence](../program-design/PROGRAM-DESIGN0002-json-persistence.md),
  [Todo Discovery](../program-design/PROGRAM-DESIGN0003-todo-discovery.md), and
  [Blazor User Experience](../program-design/PROGRAM-DESIGN0004-blazor-user-experience.md)
- Accepted decisions: [ADR0001](../adr/ADR0001-modular-application-architecture.md)
  and [ADR0002](../adr/ADR0002-versioned-atomic-json-persistence.md)
- Assumptions: Four agents work from the same committed baseline in `source-1`
  through `source-4`; a coordinator owns shared files and integration.

## Summary

Build the MVP as four production projects with matching tests. After one short
shared scaffold and contract-freeze gate, four agents implement Core, JSON
Persistence, Discovery, and Blazor Web concurrently in non-overlapping paths.
Integrate Core first, Persistence and Discovery next, and Web last, then verify
the whole repository through supported tasks and manual accessibility checks.

## Changes

### 1. Commit this documentation baseline

Before code work, reconcile the working tree, verify all specifications,
decisions, and program designs are Accepted, validate Markdown links, and commit
the complete documentation set. All agents branch from this exact commit and
re-read the live records before editing.

### 2. Coordinator scaffolds shared structure

The coordinator alone owns root and shared automation files. Create:

```text
TodoApp.slnx
.gitignore
.config/dotnet-tools.json
src/TodoApp.Core/
src/TodoApp.Core.Tests/
src/TodoApp.Discovery/
src/TodoApp.Discovery.Tests/
src/TodoApp.Persistence.Json/
src/TodoApp.Persistence.Json.IntegrationTests/
src/TodoApp.Web/
src/TodoApp.Web.Tests/
```

Use .NET 10 class-library projects for Core, Discovery, and Persistence.Json;
xUnit projects for tests; and `dotnet new blazor` with Server interactivity, all
pages interactive, empty content, and no authentication for Web. Add every
project to `TodoApp.slnx` and add references exactly as ADR0001 defines.

Configure all tests for xUnit v3 on Microsoft Testing Platform using the
centrally managed packages required by KDOTNET0002. Add bUnit to Web.Tests for
focused Blazor component rendering. Resolve its latest stable .NET 10-compatible
version through `dotnet add package bunit` during the scaffold and commit the
exact resolved version to `Directory.Packages.props`.

Create a local tool manifest and install ReportGenerator into it. Ignore
`**/bin/`, `**/obj/`, `artifacts/`, `.DS_Store`, and
`src/TodoApp.Web/data/todos.json`. Add matching `run` and `validate` tasks and
PowerShell scripts without changing existing task behavior. `run` starts Web;
`validate` runs build, unit tests, and integration tests in that order.

Commit this gate as `chore: scaffold todo solution`. All four working copies
must update to this commit before feature implementation begins.

### 3. Freeze the Core contract

Agent 1 creates the public declarations from PROGRAM-DESIGN0001 first and
commits them as `feat(core): define todo contracts`. The frozen contract is:

- Immutable `TodoItem` with ID, editable fields, completion, timestamps, and
  `Version`.
- `TodoDraft`, `TodoPriority`, structured errors, and generic/non-generic
  results.
- `ITodoStore` asynchronous GetAll, Create, Replace, and Delete operations.
- `TodoService` asynchronous GetAll, Create, Update, SetCompletion, and Delete
  operations.
- `IGuidGenerator` and injected `TimeProvider`.

Agents 2 through 4 update to this contract commit before compiling their final
changes. Contract changes after this gate require a documentation update and
explicit notification to all agents.

### 4. Four concurrent implementation tracks

#### Agent 1 — Core (`source-1`, `agent/todo-core`)

Own only `src/TodoApp.Core/` and `src/TodoApp.Core.Tests/`, except for its
coordinator-approved contract commit. Implement normalization, validation,
lifecycle operations, versions, monotonic timestamps, result mapping, and unit
tests exactly as PROGRAM-DESIGN0001 specifies. Do not implement discovery,
filesystem, or UI behavior.

#### Agent 2 — JSON Persistence (`source-2`, `agent/todo-json-persistence`)

Own only `src/TodoApp.Persistence.Json/` and
`src/TodoApp.Persistence.Json.IntegrationTests/`. Implement strict DTOs and
codec, configuration and path resolution, singleton locking, compare-and-swap,
atomic file commit, dependency registration, and real-filesystem tests exactly
as PROGRAM-DESIGN0002 specifies. Do not add product policy or edit Core.

#### Agent 3 — Discovery (`source-3`, `agent/todo-discovery`)

Own only `src/TodoApp.Discovery/` and `src/TodoApp.Discovery.Tests/`. Implement
the pure query model and deterministic view/search/filter/sort pipeline exactly
as PROGRAM-DESIGN0003 specifies. Do not perform I/O or edit Core.

#### Agent 4 — Blazor Web (`source-4`, `agent/todo-blazor-web`)

Own only `src/TodoApp.Web/` and `src/TodoApp.Web.Tests/`. Implement the host,
composition, page state, focused components, accessible styling, configuration,
result-state mapping, and tests exactly as PROGRAM-DESIGN0004 specifies. Build
against Core contracts and use fakes until Persistence and Discovery are merged.
Do not edit the other projects or shared root files.

Each agent must run the narrowest applicable project build and tests, inspect
its diff, re-read current documentation, and commit with a Conventional Commit
message before handoff.

### 5. Integration order

Create `integration/todo-mvp` from the shared scaffold commit.

1. Merge Agent 1 and run Core tests.
2. Rebase Agents 2 and 3 on the merged Core, then merge them independently and
   run their focused tests after each merge.
3. Rebase Agent 4 on Core plus Persistence and Discovery. Resolve only Web
   composition issues, then merge and run Web tests.
4. The coordinator adds any newly discovered package version to
   `Directory.Packages.props`, updates `TodoApp.slnx`, and adjusts shared task
   scripts. Feature agents never resolve shared-file conflicts by overwriting.
5. Run the complete verification gate. Fix failures in the project that owns
   the behavior; do not move policy into Web or Persistence to make tests pass.
6. Fast-forward or merge the verified integration branch into `master` using
   the repository's normal Git policy.

### 6. Runtime composition

Register `JsonTodoStore`, `TodoQueryService`, `TimeProvider.System`, and the
production GUID generator as singletons. Register `TodoService` as scoped to the
Blazor circuit. Validate non-empty `TodoStorage:FilePath` configuration at
startup. Keep `Program.cs` limited to configuration, registration, middleware,
and endpoint/component mapping.

Set `TodoStorage:FilePath` to `data/todos.json` in `appsettings.json`. Do not
expose the data directory through static-file configuration.

## Verification

### Automated gate

Run only supported repository entrypoints:

```text
task build
task test
task integration-test
task coverage
task validate
```

Core tests cover all validation limits, normalization, version behavior,
current-version no-ops, stale and unknown targets, monotonic timestamps,
storage-result propagation, and cancellation.

Discovery tests cover every view, searchable field, filter, filter combination,
sort direction, priority rank, tied value, null due date, empty result, and
input-nonmutation guarantee.

Persistence integration tests cover missing and first-created files, round
trips, relative and absolute paths, every strict validation failure, preserved
array order, concurrent in-process operations, stale compare-and-swap, write and
replace failures, temporary cleanup, and byte-for-byte preservation of an
invalid or prior valid destination.

Web tests cover initial loading, CRUD actions, completion and reopening,
validation retention, delete cancel/confirm, query controls, post-mutation
reload, all expected error states, stale-form retention, explicit conflict
reload, accessible names, live regions, and composition startup.

### Manual acceptance

- Complete the entire lifecycle using only the keyboard and verify visible
  focus is restored after edit and delete operations.
- At a 320-pixel viewport, complete create, edit, filter, complete, and delete
  workflows without horizontal page scrolling.
- Run the app, create representative todos, restart, and verify every field and
  order round-trips.
- Corrupt a disposable test JSON file and verify the app reports invalid
  storage without changing it.
- Open two tabs, edit the same todo, save one, and verify the second reports a
  conflict without losing its entered values or overwriting the first change.
- Confirm the runtime exposes no authentication, external HTTP API, Azure
  DevOps integration, cloud synchronization, or multi-process write support.

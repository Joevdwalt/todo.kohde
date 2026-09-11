# PROGRAM-DESIGN 0004: Blazor User Experience

## Status

Accepted

## Design Inputs

- Capability: Operate the todo MVP through an accessible web interface.
- Acceptance criteria: SPEC0005 and the UI-facing behavior in SPEC0001-0004.
- Supporting analysis or references: Todo Core, Discovery, and JSON Persistence
  program designs.
- Specifications: [SPEC0005](../spec/SPEC0005-blazor-user-experience.md)
- Applicable standards and prior decisions: KSTD0003, KSTD0006,
  [ADR0001](../adr/ADR0001-modular-application-architecture.md), and
  [ADR0002](../adr/ADR0002-versioned-atomic-json-persistence.md)
- Assumptions: One local user may open multiple browser tabs.

## Summary

`TodoApp.Web` provides one Interactive Server Blazor page that composes Core,
Discovery, and JSON Persistence while keeping UI state and rendering separate
from application policy.

## Goals and Exclusions

Expose all MVP workflows with accessible, responsive feedback. Authentication,
external APIs, offline behavior, native clients, and real-time cross-tab updates
are excluded.

## Constraints

Target .NET 10. Use the Blazor Web App template with Server interactivity and no
authentication. Do not serve `data/` as static content. Treat all todo text as
plain encoded content.

## System Context

The browser holds transient form, query, confirmation, and status state inside
one Blazor circuit. The server calls Core services, applies Discovery queries,
and persists through the singleton JSON adapter.

## Responsibilities and Boundaries

- `Program.cs` is the small composition root.
- `Home.razor` owns the `/` route and delegates to focused components.
- `TodoPageState` coordinates loads and operations but contains no domain or
  query policy.
- `TodoEditor` handles create/edit input and validation display.
- `TodoFilters` produces `TodoQuery` values.
- `TodoList` and `TodoCard` render results and expose named actions.
- `DeleteTodoDialog` owns confirmation focus and cancellation.
- `StatusMessage` announces success and failure outcomes.

## Repository Layout

```text
src/TodoApp.Web/
  Program.cs
  appsettings.json
  Components/App.razor
  Components/Routes.razor
  Components/_Imports.razor
  Components/Layout/MainLayout.razor
  Components/Pages/Home.razor
  Components/Todos/TodoEditor.razor
  Components/Todos/TodoFilters.razor
  Components/Todos/TodoList.razor
  Components/Todos/TodoCard.razor
  Components/Todos/DeleteTodoDialog.razor
  Components/Todos/StatusMessage.razor
  Components/Todos/TodoPageState.cs
  wwwroot/app.css
src/TodoApp.Web.Tests/
  Components/TodoPageStateTests.cs
  Composition/WebCompositionTests.cs
```

Web references Core, Discovery, and Persistence.Json. Web.Tests references Web
and uses centrally managed xUnit, FluentAssertions, and bUnit packages.

## Interfaces and Data

Editor input maps to `TodoDraft`; edit, completion, reopening, and delete actions
carry the displayed `Version`. Filters map directly to `TodoQuery`. Page state
uses the structured Core result codes and exposes loading, empty, no-results,
validation, conflict, invalid-storage, storage-unavailable, and unexpected-error
states.

`TodoStorage:FilePath` defaults to `data/todos.json` in `appsettings.json` and
can be overridden by ordinary ASP.NET Core configuration.

## Control and Data Flow

Initial page load reads all todos and applies the current query. Every successful
mutation reloads authoritative persisted state before showing success. Query
changes reapply Discovery without writing. A conflict retains editor values and
requires explicit reload; no stale action is retried.

## Key Call Paths and Pseudocode

```text
load():
  state = Loading
  result = todoService.GetAll()
  if failure: map result code to actionable status
  else: cache returned immutable values; apply query; choose Empty or Ready

submitEdit(form, displayedVersion):
  result = todoService.Update(id, displayedVersion, form.toDraft())
  if Validation: retain values and focus validation summary
  if Conflict: retain values, show reload action, focus status
  if success: reload(); announce success; restore focus to changed todo

confirmDelete(todo):
  open modal; focus heading or safe cancel control
  cancel: close and restore focus to invoking control
  confirm: call Delete(id, displayedVersion); reload on success
```

## State and Persistence

Form and query state are circuit-local and disposable. Durable state is always
reloaded through Core after mutation. Browser tabs are not synchronized until
they load or act; versions prevent silent overwrites.

## Failure and Recovery

Expected result codes map to distinct messages. Conflict offers reload.
InvalidStorage tells the user the file requires correction. StorageUnavailable
permits retry after the environmental problem is fixed. Unexpected exceptions
are logged server-side and produce a generic non-success state.

## Security and Privacy

Rely on Razor encoding and never render todo content as markup. Do not expose
the JSON file through static files, logs, or diagnostics. Authentication is out
of scope, so deployment is local/trusted only.

## Observability and Operations

Use ASP.NET Core logging for startup, unexpected exceptions, and categorized
storage failures without todo content. Development diagnostics remain disabled
in production.

## Test Strategy

Unit-test `TodoPageState` with fake services for all result categories, reload
behavior, and stale-form retention. Use bUnit for component interactions,
labels, status live regions, delete confirmation, and focus targets. Add a host
composition smoke test. Manually verify keyboard workflows and 320-pixel layout
before release.

## Required Decisions

ADR0001 and ADR0002 are Accepted.

## Unresolved Questions

None

## Readiness Checklist

- [x] Approved requirements are covered without adding product behavior.
- [x] Responsibilities, boundaries, repository layout, interfaces, and flows are explicit.
- [x] Key scenarios have construction-ready call paths and pseudocode.
- [x] State, failures, security, operations, and testing are addressed.
- [x] Consequential choices link to accepted ADRs.
- [x] No unresolved construction decisions remain.
- [x] The design is detailed enough to produce a decision-complete PLAN.

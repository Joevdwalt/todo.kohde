# PROGRAM-DESIGN 0003: Todo Discovery

## Status

Accepted

## Design Inputs

- Capability: Read-only todo views, search, filtering, and sorting.
- Acceptance criteria: SPEC0003.
- Supporting analysis or references: [Todo Core design](PROGRAM-DESIGN0001-todo-core.md)
- Specifications: [SPEC0003](../spec/SPEC0003-todo-discovery.md)
- Applicable standards and prior decisions: KSTD0003, KSTD0006, and
  [ADR0001](../adr/ADR0001-modular-application-architecture.md)
- Assumptions: The caller supplies the current local `DateOnly` value.

## Summary

`TodoApp.Discovery` provides a pure deterministic transformation from an
immutable todo collection and query to an ordered result.

## Goals and Exclusions

Implement every accepted discovery rule without I/O or mutation. Persistence,
UI state, saved searches, paging, and full-text indexing are excluded.

## Constraints

Depend only on Core. Use ordinal case-insensitive comparison for text and tags.
Apply filters with AND semantics. Preserve deterministic final ordering by GUID.

## System Context

Web obtains todos from `TodoService`, builds a `TodoQuery`, supplies today's
server-local date, and renders the result returned by `TodoQueryService`.

## Responsibilities and Boundaries

- `TodoView` defines All, Active, Completed, DueToday, and Overdue.
- `TodoDueFilter` defines Any, DueToday, Overdue, and NoDueDate.
- `TodoSortField` defines Default, CreatedAt, DueDate, Priority, and Title.
- `SortDirection` defines Ascending and Descending.
- `TodoQuery` carries the selected view, text, optional filters, and sort.
- `TodoQueryService` applies matching and ordering as pure behavior.

## Repository Layout

```text
src/TodoApp.Discovery/
  Todos/TodoView.cs
  Todos/TodoDueFilter.cs
  Todos/TodoSortField.cs
  Todos/SortDirection.cs
  Todos/TodoQuery.cs
  Todos/TodoQueryService.cs
src/TodoApp.Discovery.Tests/
  Todos/TodoQueryServiceTests.cs
```

Discovery references Core. Discovery.Tests references both.

## Interfaces and Data

`TodoQueryService.Apply(IReadOnlyCollection<TodoItem> items, TodoQuery query,
DateOnly today)` returns a new `IReadOnlyList<TodoItem>`. It never returns or
mutates a caller-owned mutable collection.

Priority rank is None=0, Low=1, Medium=2, High=3. Blank search text behaves as
no search. Optional filters are absent unless selected.

## Control and Data Flow

Apply the predefined view, then trimmed search, then all selected filters, then
the requested ordering. `Default` is a distinct sort mode and ignores explicit
direction. Every explicit sort applies GUID ascending as the final tie-break.

## Key Call Paths and Pseudocode

```text
apply(items, query, today):
  result = items matching selected view(today)
  if search not blank:
    retain ordinal-ignore-case substring match in title, description, or tags
  retain items satisfying every selected status, priority, tag, and due filter
  if sort is Default:
    order active first, due date present first, due date ascending,
      created newest first, id ascending
  else:
    order by selected field and direction
    for DueDate keep null last in both directions
    then id ascending
  return materialized read-only result
```

## State and Persistence

Not applicable. Discovery does not write or retain state.

## Failure and Recovery

Null collections or structurally invalid Core values are programming errors and
may throw. Valid empty inputs and queries produce empty successful results.

## Security and Privacy

Matching remains in process and emits no todo content externally.

## Observability and Operations

No logging is required. The caller measures or logs user-visible operation
failures; discovery is deterministic and side-effect free.

## Test Strategy

Use table-driven tests for every view, filter, search field, comparison rule,
sort direction, null due date, priority rank, tie, and combined query. Assert
the input sequence and todo values remain unchanged.

## Required Decisions

ADR0001 is Accepted.

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

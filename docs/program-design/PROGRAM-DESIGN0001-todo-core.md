# PROGRAM-DESIGN 0001: Todo Core

## Status

Accepted

## Design Inputs

- Capability: Todo lifecycle and organization.
- Acceptance criteria: SPEC0001 and SPEC0002.
- Supporting analysis or references: [Todo App README](../../README.md)
- Specifications: [SPEC0001](../spec/SPEC0001-todo-lifecycle.md),
  [SPEC0002](../spec/SPEC0002-todo-organization.md)
- Applicable standards and prior decisions: KSTD0003, KSTD0006,
  [ADR0001](../adr/ADR0001-modular-application-architecture.md), and
  [ADR0002](../adr/ADR0002-versioned-atomic-json-persistence.md)
- Assumptions: `TimeProvider` supplies time; an application-owned
  `IGuidGenerator` supplies identifiers.

## Summary

`TodoApp.Core` defines immutable todo state, normalization and validation,
lifecycle operations, application results, and the storage boundary used by
the remaining projects.

## Goals and Exclusions

Provide deterministic, testable application behavior and frozen contracts for
the other three implementation tracks. Filesystem, discovery ordering, Blazor
state, authentication, and external APIs are excluded.

## Constraints

Target .NET 10 with nullable references and warnings as errors. Use immutable
values. Control time and identifiers through injected boundaries. Do not expose
JSON or Blazor types.

## System Context

Web invokes `TodoService`; the service validates and transforms Core values and
calls `ITodoStore`. Persistence implements the store. Discovery consumes the
immutable values returned by the service.

## Responsibilities and Boundaries

- `TodoItem` owns durable todo state but performs no I/O.
- `TodoRules` normalizes and validates titles, descriptions, priorities, and
  tags and calculates the next monotonic timestamp.
- `TodoService` orchestrates create, update, completion, reopening, deletion,
  and reads.
- `ITodoStore` is the application-owned persistence contract.
- `IGuidGenerator` isolates identifier generation. `TimeProvider` isolates time.

## Repository Layout

```text
src/TodoApp.Core/
  Todos/TodoItem.cs
  Todos/TodoPriority.cs
  Todos/TodoDraft.cs
  Todos/TodoRules.cs
  Todos/TodoService.cs
  Todos/ITodoStore.cs
  Todos/IGuidGenerator.cs
  Results/TodoErrorCode.cs
  Results/TodoError.cs
  Results/TodoResult.cs
src/TodoApp.Core.Tests/
  Todos/TodoRulesTests.cs
  Todos/TodoServiceTests.cs
```

Only Core.Tests references Core. Core has no project references.

## Interfaces and Data

`TodoItem` is an immutable record containing `Guid Id`, `string Title`, nullable
`string Description`, `bool IsCompleted`, nullable `DateOnly DueDate`,
`TodoPriority Priority`, `ImmutableArray<string> Tags`, `DateTimeOffset
CreatedAt`, `DateTimeOffset UpdatedAt`, and `long Version`.

`TodoPriority` is `None`, `Low`, `Medium`, `High`. `TodoDraft` carries the five
user-editable values. `TodoErrorCode` is `Validation`, `NotFound`, `Conflict`,
`InvalidStorage`, or `StorageUnavailable`. `TodoResult` and `TodoResult<T>`
represent expected outcomes without exceptions.

`TodoService` exposes asynchronous `GetAll`, `Create`, `Update`,
`SetCompletion`, and `Delete` methods. Update, completion, and delete commands
include `id` and `expectedVersion`.

`ITodoStore` exposes asynchronous `GetAll`, `Create`, `Replace`, and `Delete`.
Replace receives the complete replacement plus `expectedVersion`; delete
receives `id` plus `expectedVersion`.

## Control and Data Flow

The service loads current state, validates the target and version, applies pure
rules, and asks the store to compare-and-swap. Store conflicts remain conflicts;
they are never retried automatically. Successful mutations return the persisted
value so the UI can reload authoritative state.

## Key Call Paths and Pseudocode

```text
create(draft):
  normalized = TodoRules.normalize(draft)
  if invalid: return Validation
  now = timeProvider.GetUtcNow()
  todo = TodoItem(guid.New(), normalized, active, now, now, version=1)
  return store.Create(todo)

update(id, expectedVersion, draft):
  current = find(store.GetAll(), id) or return NotFound
  if current.Version != expectedVersion: return Conflict
  normalized = TodoRules.normalize(draft) or return Validation
  if normalized equals current editable values: return success(current)
  replacement = current with normalized values,
      UpdatedAt=max(nowUtc, current.UpdatedAt + 1 tick),
      Version=current.Version + 1
  return store.Replace(replacement, expectedVersion)

setCompletion(id, expectedVersion, desired):
  current = find current or return NotFound
  if version differs: return Conflict
  if current.IsCompleted == desired: return success(current)
  replacement = current with desired state, next timestamp, next version
  return store.Replace(replacement, expectedVersion)

delete(id, expectedVersion):
  return store.Delete(id, expectedVersion)
```

## State and Persistence

Core owns state semantics but not storage. `Version` is the concurrency token.
No-op operations do not write, increment, or advance timestamps.

## Failure and Recovery

Expected validation, lookup, conflict, and storage failures return structured
results. Cancellation propagates. Unexpected programming failures may throw and
are handled by the host; Core never converts them into success.

## Security and Privacy

Core treats todo text as untrusted input, enforces size limits, and returns it
as data. Rendering and filesystem protections belong to their adapters.

## Observability and Operations

Core emits no logs directly. The composition boundary logs unexpected failures
and storage outcomes without logging todo content.

## Test Strategy

Use a small in-memory `ITodoStore`, fixed `TimeProvider`, and deterministic GUID
generator. Cover all validation boundaries, tag normalization, version changes,
monotonic timestamps, no-op behavior, stale versions, missing targets, store
failure propagation, and cancellation.

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

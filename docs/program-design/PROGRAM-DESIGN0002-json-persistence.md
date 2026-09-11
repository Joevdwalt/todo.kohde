# PROGRAM-DESIGN 0002: JSON Persistence

## Status

Accepted

## Design Inputs

- Capability: Safe local JSON persistence.
- Acceptance criteria: SPEC0004.
- Supporting analysis or references: [Todo Core design](PROGRAM-DESIGN0001-todo-core.md)
- Specifications: [SPEC0004](../spec/SPEC0004-json-persistence.md)
- Applicable standards and prior decisions: KSTD0003, KSTD0007,
  [ADR0001](../adr/ADR0001-modular-application-architecture.md), and
  [ADR0002](../adr/ADR0002-versioned-atomic-json-persistence.md)
- Assumptions: One registered `JsonTodoStore` instance owns writes.

## Summary

`TodoApp.Persistence.Json` implements the Core store contract with strict JSON
validation, serialized compare-and-swap mutations, and same-directory atomic
replacement.

## Goals and Exclusions

Provide reliable storage for the complete MVP data contract. Databases, file
selection, migration of unknown schemas, multi-process coordination, and
product policy are excluded.

## Constraints

Use UTF-8 JSON, camel-case properties, named enums, ISO-8601 UTC timestamps, and
`YYYY-MM-DD` dates. Relative paths resolve against the host content root.
Unknown or noncanonical input is rejected before mutation.

## System Context

Core calls `ITodoStore`. `JsonTodoStore` maps between Core values and private
version-one DTOs and delegates final file replacement to a focused atomic-file
committer. The Web composition root registers the adapter as a singleton.

## Responsibilities and Boundaries

- `TodoStorageOptions` carries `FilePath` with default `data/todos.json`.
- `TodoStoragePath` resolves and validates the configured path.
- `TodoDocumentV1` and `TodoItemDtoV1` define the strict wire contract.
- `TodoDocumentCodec` serializes, parses, maps, and validates documents.
- `JsonTodoStore` owns the process-wide `SemaphoreSlim`, array semantics, and
  compare-and-swap flow.
- `IAtomicFileCommitter` and `PhysicalAtomicFileCommitter` own temporary-file,
  flush, replace, and cleanup behavior.

## Repository Layout

```text
src/TodoApp.Persistence.Json/
  Configuration/TodoStorageOptions.cs
  Configuration/TodoStoragePath.cs
  Documents/TodoDocumentV1.cs
  Documents/TodoItemDtoV1.cs
  Documents/TodoDocumentCodec.cs
  Files/IAtomicFileCommitter.cs
  Files/PhysicalAtomicFileCommitter.cs
  JsonTodoStore.cs
  ServiceCollectionExtensions.cs
src/TodoApp.Persistence.Json.IntegrationTests/
  JsonTodoStoreTests.cs
  PhysicalAtomicFileCommitterTests.cs
```

Persistence references Core. IntegrationTests references Persistence and Core.

## Interfaces and Data

Implement the frozen `ITodoStore` interface from PROGRAM-DESIGN0001. Configure
through `TodoStorage:FilePath`. JSON has root `schemaVersion: 1` and `todos`.
Each item includes every `TodoItem` field, including `version`.

DTOs reject unmapped members. The codec validates required values, exact enum
names, UTC timestamps, monotonic timestamps, non-empty unique GUIDs, positive
versions, canonical text, unique canonical tags, and the supported schema.

## Control and Data Flow

Reads parse and validate the current file without creating it. Every mutation
holds the store semaphore from read through commit. Create appends; replace
keeps the existing index; delete removes one matching item.

## Key Call Paths and Pseudocode

```text
mutate(operation):
  await semaphore.WaitAsync(cancellation)
  try:
    document = read current file, or empty v1 document when missing
    validate document, or return InvalidStorage
    validate operation target and expected version
    next = apply operation without changing unrelated array entries
    json = codec.serializeCanonical(next)
    committer.commit(destination, json)
    return success
  catch expected I/O failure:
    return StorageUnavailable
  finally:
    semaphore.Release()

commit(destination, bytes):
  ensure parent directory exists
  create uniquely named temporary file in parent
  write all bytes and flush to durable storage
  atomically move when destination is absent; otherwise atomically replace
  on failure, best-effort delete temporary file and rethrow
```

## State and Persistence

The JSON document is authoritative. There is no long-lived in-memory cache.
Every operation observes the latest file available to this process. Canonical
full-file writes intentionally replace formatting while preserving todo order
as specified.

## Failure and Recovery

Invalid content returns `InvalidStorage` and remains byte-for-byte unchanged.
Path, permission, open, flush, and replace failures return
`StorageUnavailable`. Not-found and version mismatches return their Core result
codes. No automatic retry occurs.

## Security and Privacy

Do not serve the data directory as static content. Do not log todo content or
the JSON payload. Treat configured paths as trusted operator configuration.

## Observability and Operations

Log storage category, destination path, and exception metadata at the host
boundary without payload content. Surface actionable user messages while
retaining detailed server logs.

## Test Strategy

Use isolated temporary directories. Cover missing-file reads, first creation,
round trips, path resolution, strict invalid cases, order preservation,
compare-and-swap conflicts, concurrent requests, and injected failures at write,
flush, and replace. Assert the prior destination remains byte-for-byte valid.

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

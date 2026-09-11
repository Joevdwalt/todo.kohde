# SPEC 0004: JSON Persistence

## Status

Draft

## Analysis Inputs

- Product brief or outcome: Retain a local user's todos without a database or
  external service.
- Capability: Load and safely persist the todo collection in JSON.
- Acceptance criteria: Successful mutations survive restart, failures preserve
  the previous valid document, and invalid storage is never silently replaced.
- Supporting analysis or references:
  [SPEC0001](SPEC0001-todo-lifecycle.md),
  [SPEC0002](SPEC0002-todo-organization.md),
  [KSTD0007](../standards/KSTD0007-safe-durable-state-changes.md)
- Stable outcome and acceptance summary: The JSON file is authoritative and is
  changed atomically by one running application process.
- Assumptions and unresolved questions: None.
- Scope exclusions: Databases, cloud storage, user-selected files, multi-process
  writers, synchronization, import/export, and schema migration from unknown
  versions.

## Purpose

Define the configuration, wire format, consistency, and recovery behavior of the
todo store.

## Requirements

1. The storage path is configured by `TodoStorage:FilePath` and defaults to
   `data/todos.json` relative to the application content root. An absolute
   configured path is used unchanged.
2. A missing file represents an empty collection. The parent directory and file
   are created by the first successful mutation, not by a read.
3. The UTF-8 JSON document uses camel-case properties and this versioned shape:

   ```json
   {
     "schemaVersion": 1,
     "todos": [
       {
         "id": "00000000-0000-0000-0000-000000000000",
         "title": "Example",
         "description": null,
         "isCompleted": false,
         "dueDate": "2026-09-11",
         "priority": "High",
         "tags": ["example"],
         "createdAt": "2026-09-11T10:00:00Z",
         "updatedAt": "2026-09-11T10:00:00Z"
       }
     ]
   }
   ```

4. `dueDate` is either `null` or `YYYY-MM-DD`. Timestamps are ISO-8601 UTC
   values. Priorities use the names defined by SPEC0002.
5. Duplicate todo identifiers, invalid field values, unexpected properties,
   malformed JSON, and unsupported schema versions make the document invalid.
   The application reports the problem, blocks mutations, and does not overwrite
   the file.
6. Reads load the current file. Each mutation acquires one application-wide
   asynchronous lock, re-reads and validates the current file, validates the
   target and requested change, and only then persists the complete result.
7. A write creates a temporary file in the destination directory, writes and
   flushes the complete document, and atomically replaces the destination. If no
   destination exists, the completed temporary file is atomically moved into
   place.
8. A failed write leaves the previous destination unchanged and removes the
   temporary file on a best-effort basis. The failure is returned to the user;
   no mutation is reported as successful before replacement completes.
9. One running application process owns writes. Concurrent requests inside that
   process are serialized; concurrent writers from other processes are
   unsupported.

## Acceptance

- A missing file loads as an empty list and is created on the first mutation.
- Every todo field round-trips through JSON and survives application restart.
- The configured relative and absolute storage paths resolve as specified.
- Malformed, structurally invalid, and unsupported-version files produce a
  visible error and remain byte-for-byte unchanged.
- Duplicate or stale mutation targets fail without changing other todos.
- Concurrent in-process mutations complete without lost successful updates.
- Simulated serialization, flush, and replacement failures preserve the prior
  valid destination and do not report success.

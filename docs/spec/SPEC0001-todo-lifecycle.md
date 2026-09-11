# SPEC 0001: Todo Lifecycle

## Status

Accepted

## Analysis Inputs

- Product brief or outcome: Provide dependable personal todo management in a
  local web application.
- Capability: Create and manage the lifecycle of a todo.
- Acceptance criteria: Users can create, view, edit, complete, reopen, and
  permanently delete todos, with invalid operations rejected safely.
- Supporting analysis or references: [README](../../README.md)
- Stable outcome and acceptance summary: Todo identity and lifecycle survive
  persistence and behave consistently in every user interface.
- Assumptions and unresolved questions: None.
- Scope exclusions: Trash, archive, deletion recovery, change history, subtasks,
  and shared ownership.

## Purpose

Define the durable identity, required content, and lifecycle transitions for a
todo.

## Requirements

1. A todo has an immutable GUID identifier, a title, a completion state, a UTC
   creation timestamp, a UTC last-updated timestamp, and a positive integer
   version used for optimistic concurrency.
2. A title is required, is trimmed before validation and storage, and contains
   no more than 200 characters. A title containing only whitespace is invalid.
3. Creating a todo generates its identifier and timestamps. The creation and
   last-updated timestamps are equal at creation, and the version is `1`.
4. Editing a todo may change its mutable content but never its identifier or
   creation timestamp. The request supplies the version last read. A successful
   change increments the version and advances the last-updated timestamp to the
   later of the current UTC time or one tick after its previous value.
5. Completing an active todo and reopening a completed todo are explicit
   operations that supply the version last read. After version validation, a
   request for the current state succeeds without writing or changing the
   version or timestamps. A state change increments the version and advances the
   last-updated timestamp according to requirement 4.
6. Deletion is permanent and requires explicit user confirmation. A cancelled
   confirmation does not issue a deletion operation.
7. Edit, completion, reopening, and deletion reject a version that does not
   equal the current stored version as a `Conflict`. An operation targeting an
   unknown todo returns `NotFound`. Both outcomes leave every todo unchanged.

## Acceptance

- Creating a valid todo returns a unique identifier and displays an active todo
  with the submitted title.
- Empty, whitespace-only, and over-length titles are rejected without a write.
- Editing preserves identity and creation time while updating mutable content
  and last-updated time.
- Complete and reopen operations result in the requested state and remain safe
  when repeated.
- Current-version no-op transitions do not write; stale mutations report a
  conflict without changing state.
- Cancelling deletion preserves the todo; confirming deletion removes only the
  selected todo.
- Lifecycle state remains correct after an application restart.

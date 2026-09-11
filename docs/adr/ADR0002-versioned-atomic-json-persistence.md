# ADR 0002: Versioned Atomic JSON Persistence

## Status

Accepted

## Context

The JSON document is authoritative, browser tabs may hold stale todo values,
and multiple requests may reach one server process concurrently. A failed or
stale mutation must not lose newer data or corrupt the last valid file.

## Decision

Give every todo a positive integer `Version`, created as `1` and incremented by
each actual change. Update and delete operations carry the version last read.
The JSON store performs compare-and-swap validation while holding one
application-wide asynchronous lock.

For each mutation, the store re-reads and strictly validates the complete file,
checks identity and expected version, applies the supplied replacement or
deletion, serializes a complete canonical document to a temporary file in the
destination directory, flushes it, and atomically moves or replaces it. A failed
commit preserves the prior destination and cleans up the temporary file on a
best-effort basis.

The supported consistency boundary is one application process. Other process
writers are unsupported. Invalid or unknown JSON is reported and never
automatically normalized or overwritten.

## Consequences

Stale browser actions fail explicitly instead of overwriting newer work.
Mutations incur a full-file read and write, which is acceptable for this local
single-user MVP. Canonical writes do not preserve source whitespace or property
ordering. Multi-process coordination and large-dataset scaling remain out of
scope.

## References

- [SPEC0001](../spec/SPEC0001-todo-lifecycle.md)
- [SPEC0004](../spec/SPEC0004-json-persistence.md)
- [SPEC0005](../spec/SPEC0005-blazor-user-experience.md)
- [KSTD0007](../standards/KSTD0007-safe-durable-state-changes.md)

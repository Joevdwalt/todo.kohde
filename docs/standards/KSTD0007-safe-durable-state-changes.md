# KSTD 0007: Safe Durable State Changes

## Status

Accepted

## Standard

Apply this standard when a Feature changes durable user or system state. Treat
the durable store as authoritative and separate deterministic parsing,
validation, and transformation from storage I/O so those decisions can be
tested without the real store.

Before writing, read the current state or use an equivalent concurrency token,
then validate the target identity, version, and relevant preconditions. Reject
stale or ambiguous targets without writing. Represent expected conflicts as
explicit outcomes rather than partial success or silent overwrites.

Change only the intended state and preserve unrelated user data, formatting,
metadata, and ordering when they are outside the operation's contract. Use an
atomic transaction, replacement, or compare-and-swap mechanism when the storage
technology supports one. Define idempotency, rollback, retry, and recovery
behavior according to the operation's risks and boundary capabilities.

Unit tests cover deterministic validation and transformation. Integration tests
cover the real storage boundary, including relevant concurrency, atomicity,
preservation, and recovery behavior.

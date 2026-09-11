# KSTD 0003: Boundaries and Testability

## Status

Accepted

## Standard

Keep deterministic application behavior separate from real system boundaries.
The application owns focused capability contracts; adapters translate external
models at composition roots. Do not expose vendor client types as core domain
interfaces or create abstractions merely to mirror production types.

Keep composition roots and top-level adapter APIs small. They assemble
dependencies, translate inputs and outputs, and delegate to focused application
units; they do not own feature policy. When an adapter or orchestrator collects
independent rendering, parsing, measurement, validation, or policy decisions,
extract those decisions into focused collaborators with explicit inputs and
outputs.

Prefer pure functions and immutable values for deterministic policy. Control
time, randomness, identifiers, configuration, persistence, networks, processes,
and filesystems through explicit inputs or boundaries when they affect behavior.

Expose a method, function, type, or module through the ordinary production API
when it represents a useful, named behavior unit that callers or focused tests
can exercise directly. Keep incidental mechanics private. Do not hide meaningful
behavior solely because it currently has one caller, and do not expose production
members solely to test incidental implementation details.

Unit tests target the smallest practical observable behavior with in-memory
values and small fakes. Keep a small set of tests at composition roots and
top-level adapter APIs to verify wiring and integration; do not comprehensively
retest focused behavior through the main API. Tests crossing real system
boundaries belong in an explicit integration-test scope. All tests must be
deterministic, independent, and safe to run in parallel.

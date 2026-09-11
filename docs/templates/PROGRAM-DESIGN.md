# PROGRAM-DESIGN NNNN: Feature Title

## Status

Proposed

## Design Inputs

- Capability:
- Acceptance criteria:
- Supporting analysis or references:
- Specifications:
- Applicable standards and prior decisions:
- Assumptions:

## Summary

Describe the technical outcome and the role this Feature plays in the program.

## Goals and Exclusions

State what this design must accomplish and what it intentionally does not
address.

## Constraints

List product, technical, operational, compatibility, and delivery constraints
that shape the design.

## System Context

Describe the Feature's users, surrounding components, and external systems.
Add a Mermaid diagram only when it materially clarifies the context; keep the
prose sufficient on its own.

## Responsibilities and Boundaries

Assign responsibilities to cohesive components and state dependency direction.
Keep deterministic behavior separate from real system boundaries and define
application-owned contracts for external capabilities. Identify composition
roots and top-level adapters, keep them focused on assembly and translation, and
delegate feature decisions to focused application units.

## Repository Layout

Show the intended repository tree for the Feature, including the new or changed
modules, files, and test locations. State each item's responsibility and any
dependency restrictions. Keep the layout concrete enough to guide construction
without turning this section into a task-by-task implementation plan. Follow the
implementation ecosystem's source-layout conventions and make production units
and their focused tests easy to locate from one another.

## Interfaces and Data

Define construction-relevant interfaces, messages, data shapes, validation,
ownership, and compatibility expectations. Do not invent requirements absent
from the approved inputs.

## Control and Data Flow

Describe the important interactions and state transitions, including
concurrency or ordering where they affect behavior.

## Key Call Paths and Pseudocode

For each important observable scenario, show the call path from its entry point
through application logic and system boundaries to its outcome. Provide
language-neutral pseudocode for non-trivial control flow, including important
validation, decisions, side effects, failure branches, and ordering. The
pseudocode must be sufficient for an agent to construct the behavior without
inventing the flow.

## State and Persistence

Define state ownership, persistence behavior, consistency expectations, and
migration or retention needs. For durable writes, define target identity,
concurrency or staleness checks, preservation requirements, atomicity, and
conflict outcomes. Write `Not applicable` when the Feature owns no state.

## Failure and Recovery

Describe expected failure modes, error handling, retries, idempotency,
degradation, and recovery behavior where applicable.

## Security and Privacy

Describe trust boundaries, authorization, sensitive data handling, and relevant
abuse or misuse cases.

## Observability and Operations

Define the signals needed to diagnose and operate the Feature, including
logging, metrics, tracing, alerting, and safe operational controls as
applicable.

## Test Strategy

Map observable requirements and important failure modes to unit, integration,
and end-to-end verification. Target the smallest practical observable behavior,
keep composition-root and top-level API tests limited to wiring and integration,
and verify real system boundaries in integration scope. Keep deterministic tests
independent of real system boundaries.

## Required Decisions

Link accepted ADRs for consequential application architecture choices and RDRs
for repository or tooling choices. Record a required new decision before
accepting this program design.

## Unresolved Questions

List unresolved construction questions. This section must say `None` before the
program design can be Accepted.

## Readiness Checklist

- [ ] Approved requirements are covered without adding product behavior.
- [ ] Responsibilities, boundaries, repository layout, interfaces, and flows
      are explicit.
- [ ] Key scenarios have construction-ready call paths and pseudocode.
- [ ] State, failures, security, operations, and testing are addressed.
- [ ] Consequential choices link to accepted ADRs or RDRs.
- [ ] No unresolved construction decisions remain.
- [ ] The program design is detailed enough to produce a decision-complete PLAN.

# GUIDE 0001: Move from Analysis to Implementation

## Audience

Product owners, engineers, and AI coding agents preparing project work.

## Prerequisites

- Access to the relevant approved product analysis.
- Agreement on the team's User Story and acceptance-criteria format.

## Procedure

1. Read the current repository guidance and applicable records from the working
   tree. Do not rely on an earlier copy retained in the agent's context.
2. Confirm the intended outcome, audience, success measures, and boundaries.
3. Confirm the capability to be delivered and how it contributes to that
   outcome.
4. Review acceptance criteria, dependencies, constraints, and exclusions.
5. Resolve material contradictions in the product analysis before treating it as
   approved input.
6. Create or update SPECs for durable behavior. Summarize stable outcomes and
   link to supporting analysis where useful.
7. Create a PROGRAM-DESIGN for the Feature that translates the approved
   specifications into a repository layout, responsibilities, boundaries, interfaces,
   construction-ready call-path pseudocode, operational behavior, and a test
   strategy.
8. Capture consequential architectural or repository choices surfaced by the
   program design in ADRs or RDRs.
9. Resolve construction questions, link accepted decisions, and accept the
   PROGRAM-DESIGN after its readiness checklist is satisfied.
10. Produce a decision-complete PLAN referencing the analysis, specifications,
   accepted program design, and decisions.
11. Implement and test, re-reading applicable records whenever concurrent
    changes are detected.
12. Before completion, inspect concurrent changes, re-read the applicable
    records, reconcile the result, and update the relevant repository records.

## Verification

- A reviewer can navigate from a SPEC, PROGRAM-DESIGN, or PLAN to its relevant
  product analysis.
- Durable requirements are understandable from the repository records.
- The Accepted PROGRAM-DESIGN covers the Feature's construction without
  unresolved questions or unrecorded consequential decisions.
- The plan contains no unresolved product or implementation decisions.
- The implementation was verified against the latest applicable repository
  records, including changes made while the task was in progress.

## Recovery

If an item is unavailable or inconsistent, stop the affected program-design or
planning decision, record the gap, and ask the responsible product owner to
resolve it in the project records.

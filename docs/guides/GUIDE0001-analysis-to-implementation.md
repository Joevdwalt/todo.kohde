# GUIDE 0001: Move from Analysis to Implementation

## Audience

Product owners, engineers, and AI coding agents preparing project work.

## Prerequisites

- Access to the relevant Azure Boards items.
- Agreement on the team's User Story and acceptance-criteria format.

## Procedure

1. Confirm the Epic outcome, audience, success measures, and boundaries.
2. Confirm the Feature capability and its contribution to the Epic.
3. Review the selected User Stories, acceptance criteria, dependencies,
   constraints, and exclusions.
4. Resolve material contradictions in Boards before treating analysis as
   approved input.
5. Create or update SPECs for durable behavior. Reference relevant work-item IDs
   and summarize stable outcomes, not priority or iteration metadata.
6. Create a PROGRAM-DESIGN for the Feature that translates the approved
   specifications into a repository layout, responsibilities, boundaries, interfaces,
   construction-ready call-path pseudocode, operational behavior, and a test
   strategy.
7. Capture consequential architectural or repository choices surfaced by the
   program design in ADRs or RDRs.
8. Resolve construction questions, link accepted decisions, and accept the
   PROGRAM-DESIGN after its readiness checklist is satisfied.
9. Produce a decision-complete PLAN referencing the analysis, specifications,
   accepted program design, and decisions.
10. Implement, test, and update the work items through the team's normal process.

## Verification

- A reviewer can navigate from a SPEC, PROGRAM-DESIGN, or PLAN to its
  originating work items.
- Durable requirements are understandable without copying changing backlog
  fields into the repository.
- The Accepted PROGRAM-DESIGN covers the Feature's construction without
  unresolved questions or unrecorded consequential decisions.
- The plan contains no unresolved product or implementation decisions.

## Recovery

If an item is unavailable or inconsistent, stop the affected program-design or
planning decision, record the gap, and ask the responsible product owner to
resolve it in Boards.

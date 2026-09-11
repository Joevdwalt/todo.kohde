# SPEC 0003: Todo Discovery

## Status

Draft

## Analysis Inputs

- Product brief or outcome: Let a user quickly find the work that needs
  attention.
- Capability: Browse, search, filter, and sort todos.
- Acceptance criteria: Discovery operations return deterministic results and do
  not change todo state.
- Supporting analysis or references:
  [SPEC0001](SPEC0001-todo-lifecycle.md),
  [SPEC0002](SPEC0002-todo-organization.md)
- Stable outcome and acceptance summary: Users can narrow and order the current
  todo collection through predictable, composable controls.
- Assumptions and unresolved questions: None.
- Scope exclusions: Saved searches, custom views, full-text indexing, and
  server-side paging.

## Purpose

Define the read-only ways a user can locate and prioritize todos.

## Requirements

1. The application provides `All`, `Active`, `Completed`, `Due Today`, and
   `Overdue` views using the lifecycle and date rules in SPEC0001 and SPEC0002.
2. Search performs case-insensitive substring matching across title,
   description, and tags. Leading and trailing search whitespace is ignored.
3. Users may filter by completion state, priority, tag, and due-date state.
   Multiple active filters combine using logical AND.
4. Tag filtering uses case-insensitive exact matching against individual tags.
5. Users may sort by creation date, due date, priority, or title in ascending or
   descending direction.
6. The default order places active todos before completed todos. Within each
   group, dated todos use earliest due date first, undated todos follow, and
   equal values use newest creation time first and then identifier for a stable
   tie-break.
7. Explicit due-date sorting always places undated todos last in either
   direction. Title sorting is case-insensitive and uses identifier as its final
   tie-break.
8. Search, filtering, and sorting never mutate or persist todo data.

## Acceptance

- Each predefined view contains exactly the todos matching its definition.
- Search matches partial text in each supported field regardless of case.
- Combined filters return only items satisfying every selected condition.
- Every supported sort produces repeatable ordering, including tied and undated
  values.
- Clearing search and filters restores the applicable unfiltered view.
- A query with no matches shows an empty result without altering stored todos.

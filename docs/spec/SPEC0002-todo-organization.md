# SPEC 0002: Todo Organization

## Status

Draft

## Analysis Inputs

- Product brief or outcome: Help a user describe and organize personal work.
- Capability: Add descriptive and organizational metadata to a todo.
- Acceptance criteria: Todos support optional descriptions, due dates,
  priorities, and tags with consistent validation and overdue behavior.
- Supporting analysis or references: [SPEC0001](SPEC0001-todo-lifecycle.md)
- Stable outcome and acceptance summary: Organizational metadata remains
  understandable, searchable, and stable across application restarts.
- Assumptions and unresolved questions: None.
- Scope exclusions: Due times, reminders, notifications, recurring todos,
  subtasks, and centrally managed tag catalogs.

## Purpose

Define the optional metadata used to describe, schedule, and categorize todos.

## Requirements

1. A todo may have an optional description of no more than 2,000 characters.
   The description is trimmed; an empty result is stored as absent.
2. A todo may have one date-only due date. Due dates contain no time or timezone
   and are interpreted using the server's local calendar date.
3. A todo has one priority: `None`, `Low`, `Medium`, or `High`. New todos default
   to `None`.
4. A todo may have multiple free-form tags. Each tag is trimmed, empty tags are
   discarded, and duplicates are removed using case-insensitive comparison.
   The casing of the first supplied occurrence is retained for display.
5. An incomplete todo is overdue when its due date is earlier than the server's
   current local date. Completed todos are never overdue.
6. Metadata changes follow SPEC0001 editing rules, including identity and
   timestamp preservation.

## Acceptance

- Description, due date, priority, and tags can be added, changed, and removed.
- An over-length description is rejected without changing persisted state.
- A due date equal to the current local date is due today but not overdue.
- An incomplete past-due todo is overdue; completing it removes the overdue
  state and reopening it restores that state when the date remains past.
- Tags differing only by whitespace or letter case produce one stored tag.
- Metadata survives an application restart without semantic changes.

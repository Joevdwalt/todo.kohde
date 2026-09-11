# SPEC 0005: Blazor User Experience

## Status

Accepted

## Analysis Inputs

- Product brief or outcome: Provide an approachable browser interface for a
  local user's todo list.
- Capability: Operate all MVP todo features through Blazor.
- Acceptance criteria: Core workflows are responsive, keyboard accessible, and
  transparent about validation and persistence failures.
- Supporting analysis or references: SPEC0001 through SPEC0004.
- Stable outcome and acceptance summary: A user can complete all MVP workflows
  without an external client or direct JSON editing.
- Assumptions and unresolved questions: None.
- Scope exclusions: External HTTP API, offline browser operation, native mobile
  clients, authentication, and real-time synchronization between browser tabs.

## Purpose

Define the observable web experience for the todo MVP.

## Requirements

1. The application is an ASP.NET Core Blazor Web App targeting .NET 10 and using
   Interactive Server render mode.
2. The primary view exposes the predefined views, search, filters, sorting, and
   the resulting todo list without requiring direct URL construction.
3. Create and edit experiences expose all fields defined by SPEC0001 and
   SPEC0002 and display validation messages beside the affected input.
4. Completion and reopening are available from each todo. Permanent deletion
   requires a confirmation that identifies the selected todo and offers cancel
   and delete actions.
5. The interface provides distinct loading, empty-collection, no-search-results,
   persistence-error, and operation-failure states. Failures never appear as
   successful changes. It distinguishes validation, not-found, conflict,
   invalid-storage, and storage-unavailable outcomes.
6. After every mutation, the initiating view reloads from persisted state before
   displaying success. This reduces stale updates across tabs but does not
   provide real-time cross-tab synchronization.
7. Interactive controls are keyboard reachable, focus is visible, inputs have
   programmatic labels, validation and operation outcomes are announced to
   assistive technology, and status is not communicated by color alone.
8. Core create, edit, discovery, completion, reopening, and deletion workflows
   remain usable at narrow mobile-sized browser widths without horizontal page
   scrolling.
9. Edit, completion, reopening, and deletion submit the todo version currently
   displayed. A conflict retains entered edit values, explains that the todo
   changed, and offers an explicit reload. The application never automatically
   resubmits a stale mutation.

## Acceptance

- A user can complete every operation in SPEC0001 through SPEC0003 using only
  the browser interface.
- Invalid input keeps entered values visible and associates an explanatory
  message with the invalid field.
- Empty data, no matches, corrupt storage, and failed writes produce distinct,
  actionable states.
- Cancelling a delete confirmation leaves the selected todo unchanged.
- A stale browser tab cannot silently overwrite a newer version; its mutation
  either applies to freshly loaded state or reports a stale/unknown target.
- The core workflow passes keyboard-only use, visible-focus, accessible-name,
  status-announcement, and narrow-viewport checks.

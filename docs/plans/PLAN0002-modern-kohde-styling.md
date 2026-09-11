# PLAN 0002: Modern Kohde Styling

## Status

Accepted

## Analysis Inputs

- User request: modernize the Todo App styling using the supplied Kohde reference stylesheet and collateral.
- Existing application: ASP.NET Core Blazor Web App with a single todo workspace and existing CRUD, filtering, validation, error, and accessibility behavior.
- Visual reference: [Kohde Digital Product Experience](../../referenes/Kohde_Digital_Product_Experience.css), [Kohde brand mark](../../referenes/kohde-brandmark.png), and [Kohde wordmark palette](../../referenes/kohde-wordmark-palette.png).
- Supporting decisions: [RDR0001](../rdr/RDR0001-codex-workflow.md), [ADR0001](../adr/ADR0001-modular-application-architecture.md), and [SPEC0005](../spec/SPEC0005-blazor-user-experience.md).

## Summary

Rework the Blazor UI into a modern Kohde-branded dashboard shell while preserving all current todo behavior, routes, APIs, and accessibility contracts. Use the reference palette and layout language without copying unrelated presentation content or adding new product capabilities.

Use local/system fonts only. Copy the standalone Kohde brand mark into the web app; keep the wordmark palette image as reference collateral rather than a production image.

## Scope and Ownership

The styling track owns:

- `src/TodoApp.Web/wwwroot/app.css`
- `src/TodoApp.Web/wwwroot/images/kohde-brandmark.png`
- `src/TodoApp.Web/Components/Layout/**`
- `src/TodoApp.Web/Components/Pages/Home.razor`
- `src/TodoApp.Web/Components/Todos/**`

Do not change Core, Discovery, Persistence, page-state APIs, storage, routes, or product behavior. Multiple agents must use non-overlapping file ownership and integrate through the shared Web project.

## Implementation Changes

### 1. Kohde design tokens and global styling

Replace the current light theme with a tokenized dark system based on the reference:

- deep navy page and panel backgrounds;
- pale primary text and muted blue-gray text;
- coral primary/destructive accent;
- sand highlight accent;
- periwinkle/cyan secondary accents;
- fine blue borders, restrained shadows, and compact monospace utility labels.

Add a consistent reset, typography scale, spacing scale, control states, surface treatments, and `:focus-visible` styling. Include `prefers-reduced-motion` rules for nonessential transitions and reconnect animation.

### 2. Dashboard shell

Update `MainLayout.razor` and the home page structure to provide:

- Kohde-branded top bar with the brand mark, Todo workspace context, and status area;
- a desktop-only compact workspace/context rail using presentational labels only;
- the central todo workspace;
- a branded footer;
- responsive collapse to a single-column mobile layout.

Do not add navigation behavior, routes, or new domain concepts.

### 3. Todo surfaces

Restyle the existing page header, capture action, filters, search controls, editor, validation messages, todo cards, metadata, tags, completion state, priority, actions, empty states, loading state, failures, conflicts, success messages, delete dialog, reconnect modal, and Blazor error UI.

Where needed, add semantic CSS modifier classes such as `completed`, `overdue`, priority variants, `error`, and `active`. These classes must represent existing state only and must not introduce new filtering or product rules.

Use the supplied brand mark at `wwwroot/images/kohde-brandmark.png`. Do not add external font, CSS, JavaScript, or network runtime dependencies.

### 4. Responsive and accessibility behavior

- Wide screens: show the complete dashboard shell.
- Tablet widths: reduce or hide secondary shell regions while preserving the workspace.
- 320px mobile width: use one column, preserve readable controls, and prevent horizontal scrolling.
- Maintain semantic labels, live regions, dialog semantics, keyboard operation, visible focus, and sufficient contrast.
- Preserve all existing event handlers and page-state behavior.

## Verification

- Build `TodoApp.Web` and `TodoApp.Web.Tests` with zero warnings/errors.
- Run the existing Web test suite; all behavior tests must remain passing.
- Start through `task run` and confirm the dashboard shell and brand asset load without runtime errors.
- Manually verify create, edit, complete, reopen, delete, search, filters, validation, conflict reload, and failure states.
- Manually verify keyboard-only navigation, focus visibility/restoration, delete-dialog interaction, live announcements, and reduced-motion behavior.
- Inspect the UI at desktop, tablet, and 320px mobile widths for overflow, readable hierarchy, and usable touch targets.

## Assumptions

- The reference stylesheet supplies the visual language, not application content or behavior.
- The standalone brand mark is the only supplied collateral copied into production assets.
- No new routes, navigation semantics, data fields, or product capabilities are introduced.
- The implementation may adjust Razor markup only where required to express the visual shell or semantic styling states.

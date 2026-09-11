# Todo App

A single-user .NET 10 web application for managing a personal todo list. The
application uses an ASP.NET Core Blazor Web App with Interactive Server rendering
and stores todos in a local JSON file.

**New to the repository? Start with [START-HERE.md](START-HERE.md).**

## Delivery Workflow

Product analysis is maintained in the repository:

```text
Epic -> Feature -> User Story -> SPEC -> PROGRAM-DESIGN -> ADR/RDR -> PLAN -> implementation
```

- Product analysis records the intended outcome, acceptance criteria,
  constraints, scope, and delivery status.
- Specifications preserve durable behavioral contracts.
- Program designs define how a Feature fits and is constructed within the
  program.
- ADRs and RDRs preserve application and repository decisions.
- Plans translate approved program designs into implementation.

Repository documents may link to supporting product records when useful, but
they retain the durable analysis needed to understand and implement the work.

## MVP Features

- Create, view, edit, complete, reopen, and permanently delete todos.
- Add descriptions, date-only due dates, priorities, and tags.
- Find todos through status views, search, filters, and deterministic sorting.
- Persist todos safely in a configurable, versioned JSON file.
- Use a responsive and accessible Blazor interface with clear validation and
  failure states.

The first release is for one local user and one running application process. It
does not include authentication, cloud synchronization, shared-file writers,
recurring tasks, subtasks, reminders, notifications, attachments, trash,
archive, import/export, change history, or an external HTTP API.

Detailed behavior is defined in the [functional specifications](docs/spec/README.md).

## Structure

```text
src/             Application source and repository scripts
docs/adr/        Project architectural decisions
docs/program-design/  Construction-ready feature-scoped program designs
docs/guides/     Maintained procedures
docs/plans/      AI-assisted implementation plans
docs/rdr/        Project repository decisions
docs/spec/       Durable functional specifications
docs/standards/  Accepted Kohde baseline standards
docs/templates/  Templates for new records
```

## AI Guidance

Read [AGENTS.md](AGENTS.md) and the relevant standards, analysis items,
specifications, and decisions before planning or changing the application.
Multiple agents may work in this repository concurrently, so agents must read
these files from the working tree at task start, re-check them when concurrent
changes appear, and confirm them again before reporting completion.

## Template Upgrades

Check out the desired version of `kohde-ai-project-template`, then run:

```text
task template-upgrade TEMPLATE=../kohde-ai-project-template
```

The upgrade updates unchanged managed files and preserves local edits. Review
`artifacts/template-upgrade/report.md` when the command reports conflicts.

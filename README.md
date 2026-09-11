# todo-app

Demo project for building a todo app.

**New to the repository? Start with [START-HERE.md](START-HERE.md).**

## Delivery Workflow

Product analysis is maintained in Azure Boards:

```text
Epic -> Feature -> User Story -> SPEC -> PROGRAM-DESIGN -> ADR/RDR -> PLAN -> implementation
```

- Azure Boards owns backlog hierarchy, priority, assignment, iteration, and
  delivery status.
- Specifications preserve durable behavioral contracts.
- Program designs define how a Feature fits and is constructed within the
  program.
- ADRs and RDRs preserve application and repository decisions.
- Plans translate approved program designs into implementation.

Repository documents reference relevant Boards IDs by convention. They do not
copy volatile backlog metadata.

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

## Template Upgrades

Check out the desired version of `kohde-ai-project-template`, then run:

```text
task template-upgrade TEMPLATE=../kohde-ai-project-template
```

The upgrade updates unchanged managed files and preserves local edits. Review
`artifacts/template-upgrade/report.md` when the command reports conflicts.

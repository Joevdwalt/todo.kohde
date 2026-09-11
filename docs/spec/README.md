# Functional Specifications

Create durable behavioral contracts from `docs/templates/SPEC.md` and use the
next available `SPECNNNN` number.

## Todo App MVP

| Specification | Capability | Dependencies |
| --- | --- | --- |
| [SPEC0001](SPEC0001-todo-lifecycle.md) | Todo lifecycle | None |
| [SPEC0002](SPEC0002-todo-organization.md) | Todo organization | SPEC0001 |
| [SPEC0003](SPEC0003-todo-discovery.md) | Search, filter, and sort | SPEC0001, SPEC0002 |
| [SPEC0004](SPEC0004-json-persistence.md) | Safe JSON persistence | SPEC0001, SPEC0002 |
| [SPEC0005](SPEC0005-blazor-user-experience.md) | Blazor user experience | SPEC0001 through SPEC0004 |

All MVP specifications remain Draft until reviewed and explicitly accepted.

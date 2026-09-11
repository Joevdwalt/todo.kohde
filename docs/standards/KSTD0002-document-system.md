# KSTD 0002: Repository Document System

## Status

Accepted

## Standard

Numbered project records use `{TYPE}{NNNN}-{kebab-case-slug}.md`, with an
independent sequence beginning at `0001` for each type:

| Type | Purpose | Directory |
| --- | --- | --- |
| ADR | Application architecture decisions | `docs/adr/` |
| PROGRAM-DESIGN | Construction-ready feature-scoped program designs | `docs/program-design/` |
| RDR | Repository operation decisions | `docs/rdr/` |
| SPEC | Durable functional contracts | `docs/spec/` |
| PLAN | Decision-complete implementation plans | `docs/plans/` |
| GUIDE | Maintained human procedures | `docs/guides/` |

Shared Kohde standards use their own immutable sequence under `docs/standards/`
and do not consume project record numbers. Accepted decisions and standards are
superseded by later records rather than silently rewritten.

README.md is the source of truth for purpose and layout. START-HERE.md provides
ordinary-language onboarding. AGENTS.md stays concise and refers agents to the
README and applicable records.

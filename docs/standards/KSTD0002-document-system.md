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

Repository documents are live coordination artifacts. Agents must read the
current working-tree versions of `README.md`, `AGENTS.md`, and all records
applicable to their task when work begins. An agent must not treat prompt
content, chat history, a summary, or a previously read copy as authoritative
when the repository version is available.

Before editing a file and again before reporting completion, agents inspect the
working tree for concurrent changes. If an applicable document changed after it
was read, the agent re-reads it and reconciles its work with the latest accepted
direction. Concurrent edits are preserved and must not be silently overwritten
or reverted.

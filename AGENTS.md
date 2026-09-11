# Agent Guidance

Read [README.md](README.md) before making changes. It is the source of truth for
the project's purpose, structure, and AI guidance.

## Project Intent

Demo project for building a todo app.

## Analysis and Planning

- Read the relevant Azure Boards Epic, Feature, and User Stories before writing
  a specification or plan.
- Preserve stated intent and acceptance criteria; do not invent missing product
  requirements.
- Treat unavailable or contradictory analysis as an explicit planning gap.
- Reference relevant work-item IDs in SPECs, PROGRAM-DESIGNs, and PLANs without
  copying volatile backlog state.
- Create feature-level, construction-ready PROGRAM-DESIGN records before
  implementation plans. Do not invent product behavior or leave construction
  decisions for the implementing agent.
- Include the intended repository layout and pseudocode for important call
  paths in each PROGRAM-DESIGN.
- Use an Accepted PROGRAM-DESIGN as the technical input to a feature PLAN.
- Keep decision-complete AI-assisted plans in `docs/plans/`.

## Working Conventions

- Check applicable standards, specifications, ADRs, and RDRs before changing
  behavior or architecture.
- Follow applicable PROGRAM-DESIGN records when constructing or changing a
  Feature.
- Keep external systems behind explicit application-owned boundaries.
- Keep composition roots and top-level adapters small; delegate behavior to
  focused application units.
- Expose useful named behavior through ordinary production APIs and test the
  smallest practical observable unit. Keep top-level API tests focused on wiring
  and integration.
- Keep deterministic logic independent of real system boundaries. Revalidate
  durable state before writes and preserve data outside the operation's contract.
- Place documentation in its designated `docs/` directory.
- Update README.md when the project purpose or repository layout changes.

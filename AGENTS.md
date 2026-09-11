# Agent Guidance

Read [README.md](README.md) before making changes. It is the source of truth for
the project's purpose, structure, and AI guidance.

## Project Intent

Demo project for building a todo app.

## Analysis and Planning

- Read the relevant approved product analysis before writing a specification or
  plan.
- Preserve stated intent and acceptance criteria; do not invent missing product
  requirements.
- Treat unavailable or contradictory analysis as an explicit planning gap.
- Record the relevant analysis in SPECs, PROGRAM-DESIGNs, and PLANs. Link to
  supporting records when useful, without depending on an external tracker.
- Create feature-level, construction-ready PROGRAM-DESIGN records before
  implementation plans. Do not invent product behavior or leave construction
  decisions for the implementing agent.
- Include the intended repository layout and pseudocode for important call
  paths in each PROGRAM-DESIGN.
- Use an Accepted PROGRAM-DESIGN as the technical input to a feature PLAN.
- Keep decision-complete AI-assisted plans in `docs/plans/`.

## Documentation Freshness

- Treat repository files as the current source of truth. Do not rely on copies
  retained in prompts, chat history, summaries, or an earlier working context.
- At the start of every task, read `README.md`, this file, and the applicable
  specifications, program designs, plans, ADRs, RDRs, guides, and standards from
  the working tree.
- Immediately before changing a file, check the working tree and re-read that
  file if another agent may have changed it since it was last inspected.
- If relevant documentation changes while work is in progress, pause at the
  next safe point, re-read the changed records, and reconcile the implementation
  with the latest accepted direction before continuing.
- Before reporting completion, re-read the applicable documentation, inspect
  concurrent changes, and verify that the result still conforms. Never overwrite
  or silently undo another agent's work.

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

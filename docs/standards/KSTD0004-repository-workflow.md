# KSTD 0004: Repository Workflow

## Status

Accepted

## Standard

Repositories expose named, documented entrypoints for build, test, validation,
generation, and maintenance. Developers, CI, and agents use the same supported
entrypoints. Technology profiles may select the task runner and script language.

Contributor-authored commits follow Conventional Commits 1.0.0. Use concise
imperative descriptions and mark breaking changes with `!` or a
`BREAKING CHANGE:` footer. Generated hosting-platform merge commits are exempt.

Procedural material belongs in numbered GUIDE records with audience,
prerequisites, placeholders instead of credentials, verification steps, and
recovery guidance where applicable.

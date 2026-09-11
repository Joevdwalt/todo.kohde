# KSTD 0006: Application Source Structure

## Status

Accepted

## Standard

Organize application source around cohesive capabilities so behavior, its
supporting types, and its tests can be understood and changed together. Keep
host startup and composition separate from application behavior, and keep
adapters for real system boundaries separate from deterministic policy. Make
dependency direction between these areas explicit.

Give each named behavior unit one clear owner and a predictable location. Follow
the conventions of the implementation language and ecosystem for files,
modules, packages, and namespaces. Where those constructs correspond to source
folders, keep the relationship consistent. Do not group unrelated named units
solely to reduce file count or fragment one cohesive unit solely to satisfy an
arbitrary layout rule.

Use a consistent convention that makes the focused tests for a production unit
easy to locate from its name or source location. Split an orchestration unit
when it accumulates independently changing responsibilities or becomes a bridge
across unrelated capabilities. Prefer focused collaborators and explicit
dependency flow over central classes or modules with a broad change radius.

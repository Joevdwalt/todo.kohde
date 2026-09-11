# KSTD 0001: AI-Assisted Delivery Lifecycle

## Status

Accepted

## Standard

Projects use this delivery path:

```text
Epic -> Feature -> User Story -> SPEC -> PROGRAM-DESIGN -> ADR/RDR -> PLAN -> implementation
```

Azure Boards is authoritative for analysis hierarchy, priority, assignment,
iteration, and status. An Epic describes a business outcome, audience, success
measures, and major boundaries. A Feature describes a coherent user capability.
A User Story describes an independently valuable and verifiable delivery slice.
Teams choose their own story and acceptance-criteria syntax.

Specifications consolidate durable behavioral contracts across relevant work
items. A PROGRAM-DESIGN translates a Feature's approved behavior into a
construction-ready technical model. ADRs and RDRs record consequential
decisions surfaced by that design. After those decisions are resolved, the
Accepted PROGRAM-DESIGN becomes an input to a PLAN describing how the Feature
will be implemented and verified. Product capabilities follow the full
hierarchy; bugs, maintenance, and operational work may follow the team's normal
Boards process.

SPECs, PROGRAM-DESIGNs, and PLANs reference relevant work-item IDs. Boards
items should link back when useful, but this traceability is a convention rather
than an automated quality gate. Repository documents do not mirror volatile
backlog metadata.

Agents must inspect the relevant analysis before designing or planning and must
identify missing, inaccessible, or contradictory inputs rather than invent
requirements.

# KSTD 0001: AI-Assisted Delivery Lifecycle

## Status

Accepted

## Standard

Projects use this delivery path:

```text
Product analysis -> SPEC -> PROGRAM-DESIGN -> ADR/RDR -> PLAN -> implementation
```

Product analysis is authoritative for the intended outcome, audience, success
measures, boundaries, acceptance criteria, and delivery status. Teams choose
the records and format that best fit their work.

Specifications consolidate durable behavioral contracts from approved product
analysis. A PROGRAM-DESIGN translates a capability's approved behavior into a
construction-ready technical model. ADRs and RDRs record consequential
decisions surfaced by that design. After those decisions are resolved, the
Accepted PROGRAM-DESIGN becomes an input to a PLAN describing how the capability
will be implemented and verified. Product capabilities follow the full delivery
path; bugs, maintenance, and operational work may follow the team's normal
process.

SPECs, PROGRAM-DESIGNs, and PLANs record the product analysis relevant to their
scope. They may link to supporting records when useful, but this traceability is
a convention rather than an automated quality gate.

Agents must inspect the relevant analysis before designing or planning and must
identify missing, inaccessible, or contradictory inputs rather than invent
requirements.

---
mode: ask
description: Generate a SPEC.md for a feature from a plain description
---

Generate a complete SPEC.md using the ChurchLearn specification template.

The spec must include all sections:
- **Summary** — one paragraph what and why
- **Actors** — who initiates, who is affected
- **Preconditions** — what must be true before this runs
- **API Contract** — method, path, auth required, role, request body, response body, all error responses with status codes
- **Validation Rules** — per-field rules
- **Business Rules** — domain logic
- **Database Changes** — tables affected, new columns, new indexes
- **Frontend Behavior** — loading state, error state, success state, empty state, redirect after success
- **Acceptance Criteria** — checkable `[ ]` items
- **Out of Scope** — what this feature deliberately excludes

Follow all rules in `.github/copilot-instructions.md`.
Reference `knowledge-graph/entities.md` for existing entity shapes.
Reference `knowledge-graph/api-map.md` to avoid duplicate endpoint paths.

Feature description:
[DESCRIBE THE FEATURE IN PLAIN LANGUAGE]

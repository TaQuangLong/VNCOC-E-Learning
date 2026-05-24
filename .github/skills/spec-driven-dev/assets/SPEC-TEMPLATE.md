# Sprint {NN} — {Title}

## Propose

### Goal
{What the feature does for the user — 1–3 sentences.}

### Why This Sprint
{Why this feature is needed now. What breaks or is blocked without it.}

### Success Criteria
- {Criterion 1 — independently verifiable}
- {Criterion 2}
- {Criterion 3}
- {Criterion 4}

---

## Design

### Technical Design

{Describe backend entities, business logic, algorithms, key design decisions.}

### Architecture Decisions
- {Decision 1 — what was chosen and why}
- {What is explicitly out of scope for this sprint}

### Entities Affected
- **{EntityName}** (new/modified): `Field1`, `Field2`, `Field3`
- EF Core migration: `{MigrationName}`
- Indexes: `{Table}({Column1}, {Column2})`
        
### API Changes

| Method | Path | Auth | Role | New |
|--------|------|------|------|-----|
| GET    | /api/{resource} | Yes | Student+ | ✓ |
| POST   | /api/admin/{resource} | Yes | Admin+ | ✓ |

### Frontend Changes
- {Component or page added/changed}
- {Hook added/changed}

---

## Tasks

### Backend
- [ ] {Task description}
- [ ] {Task description}

### Frontend
- [ ] {Task description}
- [ ] {Task description}

### DevOps
- [ ] {Task description — migrations, Docker, env vars, etc.}

### Git
- [ ] Commit: `{conventional-commit message}`

---

## Archive

### Status: 🔲 Not Started
### Completed: —
### Commit: —

### What Was Built
- {Bullet summary of delivered functionality}

### Known Issues
- {Deviations from spec, deferred items, or workarounds — or "None"}

### Notes
- {Build status, migration names, anything the next sprint needs to know}

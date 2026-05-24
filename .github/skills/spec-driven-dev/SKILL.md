---
name: spec-driven-dev
description: >
  Spec-Driven Development workflow for ChurchLearn. Handles all four lifecycle phases of a feature sprint.
  Use when asked to: propose a feature, plan a sprint, implement a spec, archive a completed sprint,
  write a spec, draft a SPEC.md, create spec, new sprint, "sdd propose", "sdd plan", "sdd implement",
  "sdd archive", spec out a feature, write sprint spec, plan sprint, start new feature, document sprint.
argument-hint: 'Phase to run: propose | plan | implement | archive — or a feature description to auto-propose'
---

# Spec-Driven Development

Every feature follows four sequential phases. Run them in order. Do not skip phases.

```
Propose → Plan → Implement → Archive
```

---

## How to Invoke

| Command | What it does |
|---------|-------------|
| `propose <feature description>` | Draft the Propose section of a new SPEC.md |
| `plan <spec path>` | Fill in the Design + Tasks sections of an existing SPEC.md |
| `implement <spec path>` | Execute every unchecked task in the spec |
| `archive <spec path>` | Fill in the Archive section and update PROGRESS.md |

If no phase keyword is given, infer from context:
- Describing a new feature → **propose**
- Spec has Propose but no Design → **plan**
- Spec has Design with unchecked tasks → **implement**
- All tasks checked, no archive date → **archive**

---

## Phase 1 — Propose

**Goal:** Capture what to build and why — before any code is written.

### Steps

1. Ask the user: feature name + one-line description (or infer from their message).
2. Determine the next sprint number by reading `specs/PROGRESS.md`.
3. Create `specs/sprint-{NN}-{slug}/SPEC.md` using the [spec template](./assets/SPEC-TEMPLATE.md).
4. Fill in **only** the `## Propose` section:
   - `### Goal` — what the feature does for the user (1–3 sentences)
   - `### Why This Sprint` — why it belongs now, what breaks without it
   - `### Success Criteria` — 4–8 bullet points, each independently verifiable
5. Leave `## Design`, `## Tasks`, and `## Archive` as empty stubs.
6. Add a row to `specs/PROGRESS.md` with status `🔲 Not Started`.

**Output:** Path to the new SPEC.md.

---

## Phase 2 — Plan

**Goal:** Turn the proposal into a concrete technical blueprint.

### Steps

1. Read the SPEC.md at the given path. Confirm it has a Propose section.
2. Read the relevant knowledge-graph files to understand current entities and APIs:
   - `knowledge-graph/entities.md`
   - `knowledge-graph/api-map.md`
   - `knowledge-graph/dependency-graph.md`
3. Fill in `## Design`:
   - `### Technical Design` — backend entities, business logic, algorithms, key decisions
   - `### Architecture Decisions` — trade-offs, deferred complexity, what is explicitly out of scope
   - `### Entities Affected` — new or modified EF Core entities with all fields; migrations needed; indexes
   - `### API Changes` — table: Method | Path | Auth | Role | New/Modified
   - `### Frontend Changes` — components, pages, hooks, and state changes
4. Fill in `## Tasks` — one checkbox per atomic unit of work, grouped by Backend / Frontend / DevOps / Git.
   - Each task must be independently completable and testable.
   - Order tasks so each one has its dependencies above it.

**Output:** Updated SPEC.md with Design + Tasks filled in.

---

## Phase 3 — Implement

**Goal:** Build exactly what the spec says — no more, no less.

### Steps

1. Read the full SPEC.md.
2. Identify every unchecked task `- [ ]` in `## Tasks`.
3. Work through tasks **in order**, top to bottom:
   - For each task, implement the change.
   - After completing the task, mark it `- [x]` in the SPEC.md.
   - Follow all rules in `copilot-instructions.md` (Result pattern, FluentValidation, no magic strings, etc.).
4. After all backend tasks: verify `dotnet build` passes (0 errors).
5. After all frontend tasks: verify `npm run build` passes (0 errors).
6. Do **not** add features beyond what is listed in the spec tasks.

**Checklist before finishing:**
- [ ] All tasks checked in SPEC.md
- [ ] No new EF Core entity is missing an index
- [ ] No endpoint returns a raw EF entity (always DTO)
- [ ] No JWT in localStorage
- [ ] No hardcoded secrets or connection strings
- [ ] FluentValidation on every POST/PUT request body

**Output:** Working implementation + all tasks checked in SPEC.md.

---

## Phase 4 — Archive

**Goal:** Record what was built for future reference.

### Steps

1. Read the SPEC.md. Confirm all tasks are checked.
2. Fill in `## Archive`:
   - `### Status` — `✅ Complete`
   - `### Completed` — today's date (`YYYY-MM-DD`)
   - `### Commit` — run `git log --oneline -1` and paste the short hash + message
   - `### What Was Built` — 4–8 bullet points summarising the delivered functionality
   - `### Known Issues` — any deviations from the spec, deferred items, or workarounds applied
   - `### Notes` — build status, migration names, anything the next sprint needs to know
3. Update `specs/PROGRESS.md`:
   - Set sprint status to `✅ Complete`
   - Fill in `Completed` date
   - Add `✓` in the Build column
4. Update the relevant knowledge-graph files if new entities, APIs, or dependencies were added:
   - `knowledge-graph/entities.md`
   - `knowledge-graph/api-map.md`
   - `knowledge-graph/dependency-graph.md`
   - Update the "Knowledge Graph Last Updated" table in `PROGRESS.md`

**Output:** Archived SPEC.md + updated PROGRESS.md + updated knowledge-graph files.

---

## Spec File Location Convention

```
specs/
  sprint-{NN}-{kebab-slug}/
    SPEC.md
```

- `NN` is zero-padded sprint number (01, 02, … 16)
- `kebab-slug` is 2–4 words describing the sprint (e.g. `quiz-backend`, `auth`, `courses-frontend`)
- Never rename an existing spec folder after planning begins

---

## References

- [Spec template](./assets/SPEC-TEMPLATE.md)
- [Project instructions](../../copilot-instructions.md)
- [Progress dashboard](../../../specs/PROGRESS.md)
- [Entity graph](../../../knowledge-graph/entities.md)
- [API map](../../../knowledge-graph/api-map.md)

# Sprint 0 — Project Preparation

## Propose

### Goal
Set up the project repository, confirm MVP scope, document the execution plan, and verify EC2 capacity before writing any code.

### Why This Sprint
No code should be written before the team agrees on what to build. This sprint produces the single source of truth — the execution plan — and ensures the infrastructure exists to support the project.

### Success Criteria
- GitHub repo exists and is cloneable
- MVP scope is written and agreed upon
- EC2 capacity is documented
- Ports 80 and 443 are confirmed available

---

## Design

### Technical Design
- Public GitHub repo with `main` as the production branch
- Project structure documented in execution plan
- Folder layout: `backend/`, `frontend/`, `knowledge-graph/`, `specs/`, `.github/`, `.vscode/`
- AI coding assistant configured with project-wide instructions

### Architecture Decisions
- Modular monolith with vertical slice architecture — confirmed for ~1,000 users
- No microservices, no Kubernetes — KISS principle
- YouTube embedded videos — no private video hosting in MVP
- PDFs via external URLs only — no file upload in MVP

### Entities Affected
None — no database entities created this sprint.

### API Changes
None.

### Frontend Changes
None.

---

## Tasks

### Setup
- [x] Create GitHub repository
- [x] Initialize `git` locally (`git init`, `git branch -M main`)
- [x] Create project README
- [x] Define MVP scope in execution plan
- [x] Define initial database model
- [x] Confirm EC2 resources

### AI Configuration
- [x] Create `.github/copilot-instructions.md` — global Copilot rules
- [x] Create `.vscode/instructions/backend.instructions.md`
- [x] Create `.vscode/instructions/frontend.instructions.md`
- [x] Create `.vscode/prompts/new-vertical-slice.prompt.md`
- [x] Create `.vscode/prompts/new-react-feature.prompt.md`
- [x] Create `.vscode/prompts/create-spec.prompt.md`
- [x] Create `.vscode/prompts/review-feature.prompt.md`
- [x] Create `.vscode/agents/implement-feature.agent.md`
- [x] Create `.vscode/settings.json` — `chat.tools.autoApprove: true`

### Knowledge Graph
- [x] Create `knowledge-graph/entities.md`
- [x] Create `knowledge-graph/api-map.md`
- [x] Create `knowledge-graph/dependency-graph.md`

### Specs
- [x] Create `specs/` folder and `specs/PROGRESS.md`

---

## Archive

### Status: ✅ Complete
### Completed: 2026-05-16

### What Was Built
- Git repository initialized with `main` branch
- Full `.gitignore` covering .NET, Node, secrets, OS files
- `.github/copilot-instructions.md` with project-wide AI rules
- Backend and frontend instruction files for VS Code Copilot
- 4 reusable prompt files (new-vertical-slice, new-react-feature, create-spec, review-feature)
- 1 agent workflow file (implement-feature)
- 3 knowledge-graph files (entities, api-map, dependency-graph)
- VS Code auto-approve setting
- Specs system with progress tracker

### Known Issues
None.

### Notes
Sprint 0 and Sprint 1 were run back-to-back on the same day.

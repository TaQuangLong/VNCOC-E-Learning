# AGENTS.md — ChurchLearn

Instructions for AI coding agents working on this repository.

**ChurchLearn** is a full-stack e-learning platform for ~1,000 church members. Members enroll in courses, watch lessons, take quizzes, and join discussions. Private — VNCOC Church internal use.

---

## Repository Layout

```
backend/                    # .NET 10 ASP.NET Core Web API
  src/ChurchLearn.Api/      # Main API project (vertical slices in Features/)
  tests/ChurchLearn.Tests/  # xUnit integration tests
frontend/                   # React 18 + TypeScript + Vite
  src/features/             # Feature modules (api.ts, types.ts, components)
  src/pages/                # Route-level pages (public/, student/, admin/)
  src/app/router.tsx        # React Router v6 route registration
specs/                      # Sprint specs (SPEC.md per feature)
knowledge-graph/            # Living docs: entities, API map, dependency graph
openspec/                   # OpenSpec change proposals and archives
.github/
  copilot-instructions.md   # Detailed coding rules (read before implementing)
  skills/                   # Spec-driven dev and other agent skills
```

---

## Dev Environment

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- Docker Desktop (optional, for full stack)

### Run locally

```bash
# Backend (API on http://localhost:5251)
cd backend && dotnet restore && dotnet run --project src/ChurchLearn.Api

# Frontend (http://localhost:5173)
cd frontend && npm install && npm run dev

# Database migration (PostgreSQL must be running)
cd backend && dotnet ef database update --project src/ChurchLearn.Api
```

### Run with Docker Compose

```bash
cp .env.example .env   # fill in POSTGRES_PASSWORD, JWT_SECRET, VITE_API_URL, etc.
docker compose up --build
```

- API: http://localhost:5000
- API docs (Scalar): http://localhost:5000/scalar/v1
- Frontend: http://localhost:5173

### Environment variables

Copy `.env.example` → `.env`. Never commit secrets. Key vars: `POSTGRES_PASSWORD`, `JWT_SECRET`, `VITE_API_URL`, `APP_DOMAIN`.

---

## Build & Test Commands

Run these before opening a PR. CI runs the same checks on `main` and `develop`.

```bash
# Backend
cd backend
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release

# Frontend
cd frontend
npm ci
npm run build        # requires VITE_API_URL (CI uses http://localhost:5000)
npm run lint
```

---

## Architecture

**Modular monolith — vertical slice architecture.** Each feature is self-contained; no shared service layer.

### Backend slice (`Features/{FeatureName}/{ActionName}/`)

| File | Purpose |
|------|---------|
| `{Action}Request.cs` | Input record |
| `{Action}Response.cs` | Output DTO (never EF entity) |
| `{Action}Validator.cs` | FluentValidation rules |
| `{Action}Handler.cs` | Business logic → `Result<T>` |
| `{Action}Endpoint.cs` | Route + auth policy; maps `Result<T>` to HTTP status |

Shared infrastructure lives in `Domain/`, `Infrastructure/`, and `Common/`.

### Frontend feature module (`src/features/{featureName}/`)

- `api.ts` — TanStack Query hooks (Axios via `src/lib/api-client.ts`)
- `types.ts` — TypeScript types and Zod schemas
- Components with **loading, error, and empty** states
- New routes → page in `src/pages/` + register in `src/app/router.tsx`

---

## Coding Rules

Read `.github/copilot-instructions.md` for the full rule set. Non-negotiable highlights:

### Backend

- `async/await` with `CancellationToken` on every DbContext call
- Handlers return `Result<T>` — never throw for expected errors (not found, conflict, forbidden)
- Endpoints map `ErrorCode` → HTTP status (404 / 409 / 403 / 400)
- FluentValidation at the API boundary
- Authorization checked server-side on every protected endpoint
- EF Core migrations for schema changes; add indexes on `UserId`, `CourseId`, `LessonId` join tables

### Frontend

- TypeScript strict — no implicit `any`
- TanStack Query for server state; React Hook Form + Zod for forms
- Tailwind CSS v4 + shadcn/ui; mobile-first responsive layout
- Never hardcode API URLs — use `import.meta.env.VITE_API_URL`
- JWT in React state only; refresh token in `httpOnly` cookie (never `localStorage`)

### Security

- Rate limiting on `/api/auth/login` and `/api/auth/register`
- Roles: `Student` | `Admin` | `SuperAdmin`
- No secrets in source control

---

## Feature Development Workflow

New work follows **Spec-Driven Development** (see `.github/skills/spec-driven-dev/SKILL.md`):

```
Propose → Plan → Implement → Archive
```

### Before implementing

1. Read the target `specs/sprint-{NN}-{slug}/SPEC.md`
2. Read `.github/copilot-instructions.md`
3. Read `knowledge-graph/entities.md`, `knowledge-graph/api-map.md`, `knowledge-graph/dependency-graph.md`
4. Implement only what the spec defines (YAGNI)

### After implementing

Update these files in the same PR:

| File | What to update |
|------|----------------|
| `knowledge-graph/api-map.md` | New or changed endpoints |
| `knowledge-graph/entities.md` | New or changed entities |
| `knowledge-graph/dependency-graph.md` | Sprint status |
| `specs/PROGRESS.md` | Sprint row + knowledge-graph timestamps |

Note migration commands needed; do not auto-run migrations unless asked.

### End-to-end agent workflow

For full feature implementation, follow `.vscode/agents/implement-feature.agent.md`.

---

## API Reference

- Local (Docker): http://localhost:5000/scalar/v1
- Local (dotnet run): http://localhost:5251/scalar/v1
- Production: https://churchlearn-api.fly.dev/scalar/v1
- Endpoint map: `knowledge-graph/api-map.md`

---

## What to Avoid

- Microservices, Kubernetes, event sourcing, complex CQRS
- Large shared service classes
- Returning EF entities from API endpoints
- Throwing exceptions for domain-level errors in handlers
- Features not defined in the current SPEC.md
- Committing `.env` or other secret files

---

## Production URLs

| Service | URL |
|---------|-----|
| Frontend | https://churchlearn-frontend.fly.dev |
| API | https://churchlearn-api.fly.dev |

Smoke-test steps are documented in `README.md` under **Testing in Production**.

---

## Related Files

| File | Purpose |
|------|---------|
| `README.md` | Human-facing setup, auth flow, smoke tests |
| `.github/copilot-instructions.md` | Canonical coding standards |
| `.github/skills/spec-driven-dev/SKILL.md` | Sprint lifecycle (propose/plan/implement/archive) |
| `.vscode/agents/implement-feature.agent.md` | End-to-end feature implementation checklist |
| `.vscode/prompts/new-vertical-slice.prompt.md` | Backend vertical slice scaffold prompt |
| `specs/PROGRESS.md` | Sprint completion dashboard |

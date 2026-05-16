# Sprint 1 — Solution Setup & Local Docker Environment

## Propose

### Goal
Create a fully runnable backend API, React frontend, and PostgreSQL database locally. All three services start with a single `docker compose up`.

### Why This Sprint
The foundation must work before any feature can be built. This sprint produces the skeleton that every future sprint builds on: project structure, configuration, build pipeline, and Docker environment.

### Success Criteria
- `dotnet build` passes with 0 errors
- `npm run build` passes with 0 errors
- `docker compose up` starts all three services
- `GET /api/health` returns healthy
- React app loads at `http://localhost:5173`

---

## Design

### Technical Design

**Backend:**
- `ChurchLearn.sln` + `ChurchLearn.Api` project targeting `net10.0`
- Serilog bootstrap logger — structured JSON logging
- Scalar.AspNetCore replaces Swagger UI (development only at `/scalar/v1`)
- CORS reads `Cors:AllowedOrigins` from config — never hardcoded
- Health check at `GET /api/health`
- Vertical slice folder structure pre-created
- Global exception middleware placeholder

**Frontend:**
- React 18 + TypeScript + Vite 8
- Tailwind CSS v4 via `@tailwindcss/vite` plugin
- shadcn/ui initialized (style=default, slate base)
- Path alias `@/*` → `./src/*` in both tsconfig and vite.config
- `src/app/App.tsx` — TanStack Query provider
- `src/app/router.tsx` — React Router v6 skeleton
- `src/lib/api-client.ts` — Axios with 401 refresh interceptor

**Docker:**
- `docker-compose.yml` — postgres:16, api, frontend (platform: linux/arm64 for M1)
- Backend multi-stage Dockerfile: sdk:10.0 → aspnet:10.0
- Frontend multi-stage Dockerfile: node:22-alpine → nginx:alpine
- `nginx.conf` — SPA fallback + security headers + cache headers
- `.env` + `.env.example`

### Architecture Decisions
- `UseHttpsRedirection()` removed — nginx handles TLS in production
- `appsettings.json` committed (no real secrets) — secrets via `.env`
- `appsettings.Development.json` gitignored — per developer override

### Entities Affected
None — no EF Core entities yet. `AppDbContext` is a placeholder.

### API Changes
- `GET /api/health` — new (no auth, returns healthy)

### Frontend Changes
- Vite project scaffolded
- All route placeholders added to `router.tsx`

---

## Tasks

### Backend
- [x] Create `ChurchLearn.sln`
- [x] Create `ChurchLearn.Api` project (`net10.0`)
- [x] Add NuGet packages: Npgsql EF Core, Identity, JWT Bearer, Serilog, FluentValidation, Scalar
- [x] Rewrite `Program.cs` — Serilog, CORS, OpenAPI, Scalar, health, auth
- [x] Update `appsettings.json` — Serilog, ConnectionStrings, JWT, Cors sections
- [x] Create folder structure: Features/, Common/, Infrastructure/, Domain/
- [x] Remove Vite template files (WeatherForecast)
- [x] Verify `dotnet build` — 0 errors

### Frontend
- [x] Scaffold Vite react-ts app
- [x] Install: react-router-dom, @tanstack/react-query, axios, react-hook-form, zod, @hookform/resolvers, tailwindcss, @tailwindcss/vite
- [x] Install shadcn/ui (`npx shadcn@latest init -d`)
- [x] Install `@types/node` for vite.config path alias
- [x] Update `vite.config.ts` — Tailwind plugin, `@` alias
- [x] Update `tsconfig.app.json` — `paths: { "@/*": ["./src/*"] }` (no `baseUrl`)
- [x] Clean `index.css` — remove Vite template vars, keep shadcn tokens
- [x] Create `src/app/App.tsx` — TanStack Query provider
- [x] Create `src/app/router.tsx` — placeholder routes
- [x] Create `src/lib/api-client.ts` — Axios with refresh interceptor
- [x] Create folder structure: app/, components/, features/, hooks/, lib/, pages/
- [x] Remove old Vite template files (App.tsx, App.css)
- [x] Verify `npm run build` — 0 errors

### DevOps
- [x] Create `docker-compose.yml` — postgres, api, frontend
- [x] Create `backend/src/ChurchLearn.Api/Dockerfile` — multi-stage
- [x] Create `frontend/Dockerfile` — multi-stage
- [x] Create `frontend/nginx.conf` — SPA fallback + security headers
- [x] Create `.env` — local dev values
- [x] Create `.env.example` — template for team/production

### Git
- [x] First commit: `feat(foundation): scaffold backend, frontend, AI config, and docker compose`

---

## Archive

### Status: ✅ Complete
### Completed: 2026-05-16
### Commit: `1dc9a3a`

### What Was Built
- .NET 10 Web API with Serilog, Scalar, health check, CORS, auth middleware
- React 18 + TypeScript + Vite 8 + Tailwind v4 + shadcn/ui
- Axios API client with 401 auto-refresh interceptor
- Docker Compose stack (postgres:16, api, frontend) for M1 arm64
- Multi-stage Dockerfiles for both backend and frontend
- `.env` / `.env.example` with documented secrets
- 47 files committed — 12,104 insertions

### Known Issues
- `shadcn/tailwind.css` and `@fontsource-variable/geist` imports removed from `index.css` because they referenced packages not installed (cleaned to minimal token set)
- `baseUrl` removed from `tsconfig.app.json` — was deprecated in TypeScript 7

### Notes
- Both `dotnet build` and `npm run build` pass clean (0 errors)
- Docker Compose not yet run end-to-end — pending `AppDbContext` for DB migrations

---
mode: agent
description: Implement a complete feature end-to-end from SPEC.md
tools: [read_file, create_file, replace_string_in_file, run_in_terminal]
---

# Implement Feature Agent

## Objective
Implement a complete feature end-to-end following the ChurchLearn specification.

## Workflow

1. Read `SPEC.md` for the target feature
2. Read `.github/copilot-instructions.md` for project rules
3. Read `knowledge-graph/entities.md` for existing entity relationships
4. Read `knowledge-graph/api-map.md` for existing endpoint paths (avoid duplicates)
5. Scaffold backend vertical slice:
   - `{ActionName}Request.cs` — input record
   - `{ActionName}Response.cs` — output DTO record
   - `{ActionName}Validator.cs` — FluentValidation rules
   - `{ActionName}Handler.cs` — business logic returning `Result<{ActionName}Response>`; use `Result<T>.Failure(error, errorCode)` for not-found, conflict, and forbidden cases; never throw domain exceptions
   - `{ActionName}Endpoint.cs` — endpoint registration with auth policy; map `Result<T>` to HTTP status via switch on `ErrorCode`
6. Add EF Core migration command if schema changed (note it, do not auto-run)
7. Scaffold frontend feature module:
   - `api.ts` — TanStack Query hooks
   - `types.ts` — TypeScript types and Zod schemas
   - React components with loading / error / empty states
   - Page component if this is a new route
8. Register new route in `src/app/router.tsx` if a new page was created
9. Update `knowledge-graph/api-map.md` with new endpoints added
10. Report: files created, files modified, migration command needed, items for manual testing

## Non-Negotiable Rules
- Follow all rules in `.github/copilot-instructions.md`
- Never add features not in the SPEC.md (YAGNI)
- Never expose EF entities from API endpoints
- Always check authorization server-side
- Always handle UI states: loading, error, empty
- CancellationToken in every DbContext async call
- Handlers must return `Result<T>` — never throw exceptions for domain errors
- Endpoints must map `Result<T>` errors to HTTP status codes (404 / 409 / 403 / 400)
- No bare `throw` or `try/catch` for expected error paths in handlers

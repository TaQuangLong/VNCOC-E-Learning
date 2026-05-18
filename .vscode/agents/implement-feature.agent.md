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
3. Query the graph for existing entities, endpoints, validators, and hooks — avoids loading full
   knowledge-graph files:
   ```bash
   python graphifyy/gq.py --context {featureName}
   ```
   Only read `knowledge-graph/entities.md` if you need index definitions or relationship details
   not present in the graph output.
4. Check all existing endpoint paths to avoid duplicate route registration:
   ```bash
   python graphifyy/gq.py --endpoints
   ```
   Only read `knowledge-graph/api-map.md` if you need auth requirements or role-level details.
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
10. Update `knowledge-graph/entities.md` with any new or changed entity definitions
11. Update `knowledge-graph/dependency-graph.md` — mark this sprint's feature as ✅ done
12. Update `specs/PROGRESS.md` — mark the sprint row as ✅ Complete with today's date and ✓ build, and update the Knowledge Graph Last Updated table
13. Refresh the auto-generated knowledge graph so the next agent session has accurate context:
    ```bash
    graphify .
    ```
    Check `graphify-out/GRAPH_REPORT.md` — the new feature's endpoints should appear under
    **Core Nodes** and the **missing-calls** count should decrease if frontend was also implemented.
14. Report: files created, files modified, migration command needed, items for manual testing

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

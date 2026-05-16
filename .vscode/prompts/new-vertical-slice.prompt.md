---
mode: agent
description: Scaffold a complete backend vertical slice for ChurchLearn
---

Create a complete vertical slice for the feature described below.

Follow ChurchLearn vertical slice architecture:
- Folder: `Features/{FeatureName}/{ActionName}/`
- `{ActionName}Request.cs` — input record, fields only
- `{ActionName}Response.cs` — output record (DTO only, never EF entity)
- `{ActionName}Validator.cs` — FluentValidation rules
- `{ActionName}Handler.cs` — business logic using AppDbContext and ICurrentUser
- `{ActionName}Endpoint.cs` — registers endpoint with RequireAuthorization

Rules (from .github/copilot-instructions.md):
- async/await with CancellationToken in all DbContext calls
- RequireAuthorization with the correct role policy in endpoint
- Return DTO — never return EF Core entity
- Use record types for Request and Response
- No magic strings — use constants or enums

Also reference:
- `knowledge-graph/entities.md` for existing entity relationships
- `knowledge-graph/api-map.md` for existing endpoint patterns

Feature to scaffold:
[PASTE SPEC.md CONTENT HERE or describe the feature in plain language]

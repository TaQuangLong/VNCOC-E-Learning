---
name: review-full-flow
description: >
  Use when asked to review, trace, or audit the full code flow for a feature —
  from React component through TanStack Query hook, Axios api-client, ASP.NET
  Core Minimal API endpoint, vertical-slice handler, FluentValidation, Result
  pattern, EF Core query, and service registration. Triggers on phrases like
  "review full flow", "trace feature flow", "audit feature", "check end-to-end",
  "full stack review", "review code flow".
---

# Skill: Review Full Feature Flow

This skill traces a single feature end-to-end across the ChurchLearn stack and
produces a layered review with a checklist at each layer.

---

## Context

Always load:
- `.github/copilot-instructions.md` — coding rules and architecture constraints
- `knowledge-graph/api-map.md` — route table, auth, and roles
- `knowledge-graph/entities.md` — entity definitions and relationships

---

## Step 1 — Resolve Feature Files

Search the workspace for files related to the feature name:
- `backend/src/ChurchLearn.Api/Features/{FeatureName}/`
- `frontend/src/features/{featureName}/`

Expected files to find for any complete feature:

| Node type | Meaning | Expected location |
|-----------|---------|-------------------|
| `endpoint` | API route | `backend/.../Features/{Name}/` |
| `validator` | FluentValidation class | same folder as endpoint |
| `class` (Handler) | vertical-slice handler | same folder as endpoint |
| `component`/`page` | React UI | `frontend/src/features/{name}/` or `pages/` |
| `hook` | TanStack Query hook | `frontend/src/features/{name}/api.ts` |
| `schema` | Zod schema | `frontend/src/features/{name}/types.ts` |

If a node type is **missing** from the graph output, that layer likely does not
exist yet — flag it as a gap in the review output.

---

## Step 2 — Frontend: React Component

**Files to read:** component files in `frontend/src/features/{featureName}/` or `frontend/src/pages/`.

### Checklist
- [ ] No `any` types — TypeScript strictly typed throughout
- [ ] Loading, error, and empty states all handled
- [ ] Tailwind responsive prefixes used (`sm:`, `md:`, `lg:`) — mobile-first layout
- [ ] `import.meta.env.VITE_API_URL` used — no hardcoded API URLs
- [ ] Uses shadcn/ui components for common UI patterns
- [ ] Mutation triggers via TanStack Query hook (not raw `apiClient` calls in components)
- [ ] Form uses React Hook Form + Zod schema for validation

---

## Step 3 — Frontend: TanStack Query Hook + API Call

**Files to read:** `frontend/src/features/{featureName}/api.ts` and `frontend/src/features/{featureName}/types.ts`.

### Pattern to verify
```ts
// Query keys are co-located with the feature
export const {featureName}Keys = {
  list: () => ['{featureName}', 'list'] as const,
  detail: (id: number) => ['{featureName}', 'detail', id] as const,
}

// Reads: useQuery
export function use{Action}() {
  return useQuery({
    queryKey: {featureName}Keys.list(),
    queryFn: () => apiClient.get<{ResponseType}>('/route').then(r => r.data),
  })
}

// Writes: useMutation + invalidate on success
export function use{Action}() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: {RequestType}) =>
      apiClient.post<{ResponseType}>('/route', payload).then(r => r.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: {featureName}Keys.list() })
    },
  })
}
```

### Checklist
- [ ] Query keys are stable, typed `as const`
- [ ] All reads use `useQuery`, writes use `useMutation`
- [ ] `onSuccess` invalidates related query keys
- [ ] Response type matches the backend DTO — check against `api-map.md`
- [ ] `enabled` guard on queries that depend on optional params (e.g., `enabled: !!courseId`)

---

## Step 4 — Frontend: api-client.ts

**File:** `frontend/src/lib/api-client.ts`

### Pattern to verify
```ts
// Base URL from env — never hardcoded
baseURL: `${import.meta.env.VITE_API_URL}/api`

// withCredentials — sends httpOnly refresh cookie
withCredentials: true

// Request interceptor injects Bearer access token from React state
// Response interceptor retries on 401 using /api/auth/refresh
```

### Checklist
- [ ] `withCredentials: true` — refresh cookie sent with every request
- [ ] Access token injected via request interceptor from in-memory state (NOT localStorage)
- [ ] 401 response triggers silent token refresh via `/api/auth/refresh`, then retries
- [ ] On refresh failure, user is redirected to `/login`

---

## Step 5 — Backend: Minimal API Endpoint

**File:** `backend/src/ChurchLearn.Api/Features/{FeatureName}/{FeatureName}Endpoints.cs`

### Pattern to verify
```csharp
app.MapPost("/{courseId:int}/enroll", async (
    int courseId,
    EnrollCourseHandler handler,   // injected from DI
    CancellationToken ct) =>
{
    var result = await handler.HandleAsync(courseId, ct);
    return result.ToHttpResult(r => Results.Created($"/api/me/courses/{r.CourseId}", r));
});
```

### Checklist
- [ ] Route matches `knowledge-graph/api-map.md` exactly (method + path)
- [ ] `.RequireAuthorization()` applied — never trust client-side auth
- [ ] Role policy applied if restricted (`"Admin"`, `"SuperAdmin"`, `"Student"`)
- [ ] Handler injected as DI parameter — not `new`-ed manually
- [ ] `CancellationToken ct` passed to handler
- [ ] Result mapped via `result.ToHttpResult(...)` — never `Results.Ok(result)` directly
- [ ] POST creates return `Results.Created(...)` with location header; GET returns `Results.Ok(...)`

---

## Step 6 — Backend: Vertical Slice Handler

**File:** `backend/src/ChurchLearn.Api/Features/{FeatureName}/{ActionName}/{ActionName}Handler.cs`

### Pattern to verify
```csharp
public class EnrollCourseHandler(AppDbContext db, ICurrentUser currentUser)
{
    public async Task<Result<EnrollCourseResponse>> HandleAsync(int courseId, CancellationToken ct)
    {
        // 1. Load entity — return NotFound if missing
        var course = await db.Courses.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId, ct);
        if (course is null)
            return Result<EnrollCourseResponse>.Failure($"Course {courseId} not found.", ErrorCodes.NotFound);

        // 2. Domain rule checks — return domain error Result, never throw
        if (course.Status != CourseStatus.Published)
            return Result<EnrollCourseResponse>.Failure("...", ErrorCodes.BadRequest);

        // 3. Duplicate/conflict checks
        var exists = await db.Enrollments.AnyAsync(e => e.UserId == currentUser.UserId, ct);
        if (exists)
            return Result<EnrollCourseResponse>.Failure("...", ErrorCodes.Conflict);

        // 4. Mutate + save
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync(ct);

        // 5. Return success DTO — never the EF entity itself
        return Result<EnrollCourseResponse>.Success(new EnrollCourseResponse(...));
    }
}
```

### Checklist
- [ ] Handler uses primary constructor DI — no field assignments needed
- [ ] `CancellationToken` passed to every EF Core async call
- [ ] `AsNoTracking()` on read-only queries (queries used only to check, not mutate)
- [ ] Returns `Result<T>` — never throws for domain errors (NotFound, Conflict, Forbidden)
- [ ] Returns a DTO record — never an EF Core entity
- [ ] Authorization checks use `ICurrentUser` — never trusts any client-provided userId
- [ ] Method stays under ~30 lines; complex logic extracted to private methods or separate handlers
- [ ] Uses `ErrorCodes.*` constants — never magic strings

---

## Step 7 — Backend: FluentValidation (if request body exists)

**File:** `backend/src/ChurchLearn.Api/Features/{FeatureName}/{ActionName}/{ActionName}Validator.cs`

### Pattern to verify
```csharp
public class CreateCourseValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9-]+$");
    }
}
```

### Checklist
- [ ] Validator present for every command/mutation endpoint (POST, PUT)
- [ ] No validation in handler business logic that belongs in the validator
- [ ] Validator registered and wired into the endpoint pipeline

---

## Step 8 — Backend: Result → HTTP Mapping

**File:** `backend/src/ChurchLearn.Api/Common/Extensions/ResultExtensions.cs`

### Mapping table — verify error codes produce correct HTTP status
| `ErrorCode` | HTTP Status |
|-------------|-------------|
| `NOT_FOUND` | 404 Not Found |
| `CONFLICT` | 409 Conflict |
| `UNAUTHORIZED` | 401 Unauthorized |
| `FORBIDDEN` | 403 Forbidden |
| `BAD_REQUEST` (default) | 400 Bad Request |

### Checklist
- [ ] Handler uses the correct `ErrorCodes.*` constant for the intended HTTP status
- [ ] Endpoint calls `result.ToHttpResult(...)` — not raw `Results.*` with no error handling
- [ ] No `try/catch` wrapping `handler.HandleAsync(...)` in the endpoint

---

## Step 9 — Backend: EF Core Query Quality

Check every `db.*` call inside the handler.

### Checklist
- [ ] `AsNoTracking()` on reads that don't lead to `SaveChangesAsync`
- [ ] `AnyAsync` used for existence checks (not `FirstOrDefaultAsync` + null check)
- [ ] `CountAsync` used for counts (not `ToListAsync().Count`)
- [ ] Queries filter with indexed columns: `UserId`, `CourseId`, `LessonId`, `Slug`
- [ ] No N+1 — related data loaded via `.Include()` or separate batched queries, not in a loop
- [ ] `await db.SaveChangesAsync(ct)` — cancellation token passed

---

## Step 10 — Backend: Service Registration

**Files:**
- `{FeatureName}ServiceRegistration.cs` — handler DI registration
- `{FeatureName}Endpoints.cs` — endpoint mapping registered in `Program.cs`

### Checklist
- [ ] Every handler in the feature's action folders is registered with `services.AddScoped<...>()`
- [ ] `Map{FeatureName}Endpoints(app)` is called from `Program.cs`
- [ ] New action folder handlers are added to service registration (easy to miss on new slices)

---

## Step 11 — Security Cross-Cut

Review across all layers:

### Checklist
- [ ] Server-side authorization is enforced in the handler — never only on the frontend
- [ ] JWT access token NOT in localStorage — stored in React state; refreshed via httpOnly cookie
- [ ] FluentValidation runs on all mutation endpoints
- [ ] Rate limiting applied on `/api/auth/login` and `/api/auth/register`
- [ ] No secrets or connection strings in code — only environment variables
- [ ] No unvalidated external URLs stored as-is

---

## Step 12 — Knowledge Graph Consistency Check

After reviewing, verify both the hand-written and auto-generated graphs:

**Hand-written (update manually if stale):**
- `knowledge-graph/api-map.md` — does the route table reflect current routes, auth, and roles?
- `knowledge-graph/entities.md` — are new entities or fields documented?
- `knowledge-graph/dependency-graph.md` — is sprint status accurate?

---

## Output Format

Produce a review summary structured as:

```
## Full-Flow Review: {Feature} — {Action}

### Layer Summary
| Layer | File | Status | Issues |
|-------|------|--------|--------|
| React Component | ... | ✅ / ⚠️ / ❌ | ... |
| TanStack Query Hook | ... | ✅ / ⚠️ / ❌ | ... |
| api-client.ts | ... | ✅ / ⚠️ / ❌ | ... |
| Endpoint | ... | ✅ / ⚠️ / ❌ | ... |
| Handler | ... | ✅ / ⚠️ / ❌ | ... |
| Validator | ... | ✅ / ⚠️ / ❌ | ... |
| Result → HTTP | ... | ✅ / ⚠️ / ❌ | ... |
| EF Core Queries | ... | ✅ / ⚠️ / ❌ | ... |
| Service Registration | ... | ✅ / ⚠️ / ❌ | ... |

### Issues Found
List each violation with: Layer | File | Line | Rule Violated | Suggested Fix

### Knowledge Graph Gaps
List any api-map.md / entities.md discrepancies found.
```

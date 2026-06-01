## Context

The platform already has an `Author` entity (`Id`, `Name`, `Bio`, `AvatarUrl`, `UserId`, `CreatedAt`, `UpdatedAt`) with two working endpoints: `GET /api/admin/authors` (list) and `POST /api/admin/authors` (create). The `CourseForm` uses `useAuthors()` to populate an author picker, and admin courses table shows the author name.

What is missing: a dedicated admin UI to manage authors (list with actions, create, edit, delete) and the corresponding backend endpoints for single-record fetch, update, and delete.

## Goals / Non-Goals

**Goals:**
- Add `GET /api/admin/authors/:id`, `PUT /api/admin/authors/:id`, `DELETE /api/admin/authors/:id` handlers
- Guard DELETE: reject with `400 Conflict` if author has assigned courses
- Add `AdminAuthorsPage` (list with Create/Edit/Delete actions), `CreateAuthorPage`, `EditAuthorPage` under `pages/admin/`
- Wire three new routes into `router.tsx` under `AdminRoute`
- Add `useAuthor(id)`, `useCreateAuthor()`, `useUpdateAuthor()`, `useDeleteAuthor()` hooks alongside existing `useAuthors()`

**Non-Goals:**
- No linking Authors to `AppUser` accounts via the UI (UserId is an optional field, manage via API if needed)
- No avatar upload (URL string input only)
- No pagination on the authors list (expected count: < 100)
- No public-facing author profile pages

## Decisions

### D1: Keep Author handlers inside `Features/Courses/`
**Decision**: Add `GetAuthorById`, `UpdateAuthor`, `DeleteAuthor` folders under `Features/Courses/`, consistent with `CreateAuthor` and `GetAuthors` already there.  
**Rationale**: Author is tightly coupled to Course. A separate `Features/Authors/` module would be over-engineering for the scope of this change. If Authors grow into a domain of their own, extraction is straightforward.

### D2: Add `AuthorDetail` DTO for GET/PUT
**Decision**: Return a new `AuthorDetail` record (`Id`, `Name`, `Bio`, `AvatarUrl`, `UserId`, `CourseCount`) from `GetAuthorById`, separate from the existing `AuthorSummary`.  
**Rationale**: `CourseCount` is needed to display in the list and to inform the delete guard in the UI. Adding it to `AuthorSummary` would change the existing list endpoint contract; a separate DTO avoids a breaking change.

### D3: Colocate Author hooks in `features/courses/api.ts`
**Decision**: Add the new hooks (`useAuthor`, `useCreateAuthor`, `useUpdateAuthor`, `useDeleteAuthor`) to the existing `frontend/src/features/courses/api.ts`.  
**Rationale**: `useAuthors()` already lives there. Creating a separate `features/admin/authors/` module for 4 hooks would fragment a small, cohesive set. Revisit if the authors feature grows independently.

### D4: Inline confirmation dialog for delete
**Decision**: Use a shadcn/ui `AlertDialog` inline in `AdminAuthorsPage` rather than a separate delete page.  
**Rationale**: Matches the pattern used for course deletion in `AdminCoursesPage`. Avoids a dead-end route for a destructive action.

### D5: 409 Conflict on DELETE when author has courses
**Decision**: `DeleteAuthorHandler` queries `db.Courses.AnyAsync(c => c.AuthorId == id)` and returns `Result.Failure(ErrorCodes.Conflict)` if true, mapped to HTTP 409 at the endpoint.  
**Rationale**: Prevents orphaned course records. The frontend surfaces this as a user-friendly toast message listing that the author still has courses.

## Risks / Trade-offs

- [No cascade path] If an admin wants to delete an author who owns courses, they must reassign all courses first. No bulk-reassign UI exists → Mitigation: Error message should instruct the admin to reassign courses before deleting; acceptable UX at this scale.
- [AuthorSummary list does not include CourseCount] The existing `GET /api/admin/authors` list only returns `AuthorSummary` without course count → Mitigation: D2 adds `CourseCount` to `AuthorDetail` returned by the single-record endpoint. The list page will show course count by fetching the list and then enriching on demand, OR we extend `AuthorSummary` — deferred decision to tasks.

## Context

ChurchLearn has a mature **Course** model (Draft/Published/Archived, slug, author, lessons) and per-course **Enrollment** with progress tracking. Courses are browsed individually at `/courses` with no higher-level curriculum structure.

Members need **learning paths** — curated foundations that group existing courses into ordered sections (similar to [Code with Mosh Fundamentals](https://codewithmosh.com/p/fundamentals)). Admins must be able to compose paths from published courses without duplicating course content.

**Confirmed product decisions:**
- Browse-only enrollment — path is a guide; students enroll in each course separately
- Grouped sections within a path (not a flat list only)
- Derived path progress for logged-in students (no path-level enrollment entity)
- Published paths visible publicly (guests + students); only Published courses can be added
- A course may appear in multiple paths
- Unpublishing or archiving a course automatically returns affected Published paths to Draft

## Goals / Non-Goals

**Goals:**
- New `LearningPath`, `LearningPathSection`, `LearningPathCourse` entities with EF migration
- Admin CRUD with nested section/course ordering (create, edit, publish, unpublish, archive)
- Public catalog (`/learning-paths`) and detail (`/learning-paths/:slug`) pages
- Path detail shows sections, ordered courses, links to `/courses/:slug`, and derived progress when authenticated
- Reuse existing Course status, slug, and Enrollment models — no enrollment changes
- Keep published paths internally consistent when a referenced course is unpublished or archived

**Non-Goals:**
- One-click or sequential path enrollment / course unlocking
- Path-level enrollment entity or separate progress table
- Nested paths (path within path) or prerequisites between paths
- Course duplication inside a path (same course twice in one path)
- File upload for path thumbnails (URL string only, consistent with courses)
- Drag-and-drop reorder UI in MVP (up/down buttons or numeric order index is sufficient)

## Decisions

### D1: Three-entity relational model
**Decision**: `LearningPath` → many `LearningPathSection` (ordered by `OrderIndex`) → many `LearningPathCourse` join rows (ordered by `OrderIndex`, FK to `Course`).

**Rationale**: Sections match the Mosh pattern ("Programming Languages", "Git", etc.). Normalized join table supports reordering without duplicating course data.

**Alternatives considered**: JSON column for sections/courses — rejected (harder to query, validate FKs, and index).

### D2: Reuse `CourseStatus` pattern for paths
**Decision**: `LearningPathStatus` enum: `Draft`, `Published`, `Archived`. Public endpoints return only `Published`. Soft-delete via `Archived`.

**Rationale**: Consistent with existing Course lifecycle and admin mental model.

Archived paths are terminal in MVP: they cannot be edited, published, or unpublished. Attempts return HTTP 409.

### D3: Replace-all sections on update
**Decision**: `PUT /api/admin/learning-paths/:id` accepts the full nested payload (path fields + sections + course IDs with order). Handler replaces section/course rows transactionally.

**Rationale**: Simpler than partial PATCH for reorder/add/remove. Acceptable for small paths (< 20 courses). Matches admin form submit pattern.

**Alternatives considered**: Granular section/course endpoints — deferred; adds API surface without MVP benefit.

### D4: Derived progress — no path enrollment
**Decision**: `GET /api/learning-paths/:slug` accepts optional auth. When a user is authenticated, response includes `progress: { completedCoursesCount, totalCoursesCount, progressPercent }` and per-course flags (`isEnrolled`, `progressPercent`, `isCompleted`) computed from existing `Enrollment` records.

**Rationale**: Matches Mosh's browse-and-enroll model. Avoids new enrollment flows and keeps path as a curated view layer. `progressPercent` = average of enrolled courses' progress, or `completedCoursesCount / totalCoursesCount * 100` — use **completion count ratio** for clarity (a course counts complete when `Enrollment.CompletedAt` is set or `ProgressPercent >= 100`).

### D5: Only Published courses eligible
**Decision**: Admin create/update validators reject any submitted course ID where `Course.Status != Published`. Publishing also verifies that the path contains at least one section with at least one course and that every referenced course is still Published.

When an existing course is unpublished or archived, the course status change and transition of every affected Published learning path to Draft occur in the same database transaction. Draft paths retain their references so an admin can replace or remove the unavailable course before publishing again.

As defense in depth, public list and detail queries also require every referenced course to remain Published. An inconsistent path is treated as unavailable rather than returning links to inaccessible courses.

**Rationale**: Public paths must never reference unavailable content, while course lifecycle operations should remain available to administrators.

### D6: Unique constraints
**Decision**:
- Unique index on `LearningPaths.Slug`
- Unique index on `(LearningPathId, CourseId)` — a course appears at most once per path (across all sections)
- Unique index on `(LearningPathId, OrderIndex)` for sections
- Unique index on `(LearningPathSectionId, OrderIndex)` for path courses

**Rationale**: Prevents duplicate courses in one path and ordering collisions.

### D7: New `Features/LearningPaths/` vertical slice module
**Decision**: All handlers under `Features/LearningPaths/{Action}/`; register public routes at `/api/learning-paths` and admin routes at `/api/admin/learning-paths`.

**Rationale**: Learning paths are a distinct domain from single-course management. Keeps slices cohesive per project conventions.

The module includes `LearningPathsServiceRegistration` and `LearningPathsEndpoints`, wired through `AddLearningPathsFeature()` and `MapLearningPathsEndpoints()` in `Program.cs`.

### D8: Frontend feature module `features/learning-paths/`
**Decision**: Colocate `types.ts`, `api.ts`, and shared components. Admin pages under `pages/admin/`, public pages under `pages/public/`.

**Rationale**: Matches existing feature module pattern (`features/courses/`, `features/enrollment/`).

## Risks / Trade-offs

- [Unpublished course in published path] A course later archived/unpublished could make a public path invalid → Mitigation: course status handlers atomically return affected Published paths to Draft; public queries defensively exclude inconsistent paths; admin edit page shows warnings for retained unavailable-course references
- [Replace-all update race] Concurrent admin edits could overwrite each other → Mitigation: Acceptable at church scale; optimistic UI with last-write-wins
- [Progress without enrollment] Guest sees no progress; partially enrolled student sees mixed state → Mitigation: UI labels "Enroll to start" per course; path header shows "X of Y courses complete"
- [Catalog ordering] Curated ordering may be needed later → Mitigation: MVP uses deterministic `CreatedAt DESC, Id DESC`; custom display ordering is deferred

## Migration Plan

1. Create EF migration `AddLearningPaths` with new tables and indexes; do not apply it automatically
   - Apply later from `backend/` with `dotnet ef database update --project src/ChurchLearn.Api`
2. Deploy backend API endpoints
3. Deploy frontend pages and navigation
4. Rollback: migration down removes tables; no changes to existing Course/Enrollment data

## Open Questions

- Should student dashboard or `/my-learning` surface "Recommended paths"? → Defer to follow-up; out of MVP scope
- Estimated duration field on path (e.g. "3–6 months")? → Resolved: include optional `EstimatedDurationLabel` string in MVP
- Custom catalog display ordering? → Defer to follow-up; MVP sorts by `CreatedAt DESC, Id DESC`

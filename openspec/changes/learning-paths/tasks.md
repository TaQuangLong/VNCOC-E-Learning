## 1. Backend — Domain & Migration

- [x] 1.1 Add `LearningPathStatus` enum (Draft, Published, Archived) in `Domain/Enums/`
- [x] 1.2 Add entities `LearningPath`, `LearningPathSection`, `LearningPathCourse` with navigation properties and indexes per design (unique slug, unique path+course, order indices)
- [x] 1.3 Register entities in `AppDbContext` with Fluent API configuration
- [x] 1.4 Create EF migration `AddLearningPaths`; do not apply it automatically, and document the `dotnet ef database update` command
- [x] 1.5 Add `LearningPathsServiceRegistration` and `LearningPathsEndpoints`; wire `AddLearningPathsFeature()` and `MapLearningPathsEndpoints()` into `Program.cs`

## 2. Backend — CreateLearningPath

- [x] 2.1 Create `Features/LearningPaths/CreateLearningPath/` with request DTO (nested sections + course IDs), FluentValidation, and handler
- [x] 2.2 Validator/handler validation: title/slug rules, unique slug check, all course IDs exist and are Published, no duplicate course in path, and no duplicate order indices
- [x] 2.3 Handler persists path + sections + join rows in one transaction; returns `Result<CreateLearningPathResponse>`
- [x] 2.4 Register `POST /api/admin/learning-paths` in admin group with Admin/SuperAdmin auth

## 3. Backend — GetAdminLearningPaths & GetAdminLearningPath

- [ ] 3.1 Create `GetAdminLearningPaths` handler with pagination, status filter, returning summaries with section/course counts
- [ ] 3.2 Create `GetAdminLearningPath` handler returning full nested path detail for edit form
- [ ] 3.3 Register `GET /api/admin/learning-paths` and `GET /api/admin/learning-paths/{id}`

## 4. Backend — UpdateLearningPath

- [ ] 4.1 Create `Features/LearningPaths/UpdateLearningPath/` with request, validator, handler (replace-all sections/courses transactionally); reject updates to Archived paths with 409
- [ ] 4.2 Register `PUT /api/admin/learning-paths/{id}` mapping Result to HTTP status codes

## 5. Backend — Publish, Unpublish, Archive

- [ ] 5.1 Create `PublishLearningPath` handler — require at least one non-empty section and verify all courses Published before setting status; reject Archived paths with 409
- [ ] 5.2 Create `UnpublishLearningPath` handler — set status to Draft; reject Archived paths with 409
- [ ] 5.3 Create `ArchiveLearningPath` handler — set status to Archived (soft delete)
- [ ] 5.4 Register `POST .../publish`, `POST .../unpublish`, `DELETE .../{id}` admin routes
- [ ] 5.5 Update course unpublish/archive handlers so the course status change and transition of affected Published learning paths to Draft happen transactionally

## 6. Backend — Public List & Detail

- [ ] 6.1 Create `GetLearningPaths` handler — paginated paths whose status and referenced course statuses are all Published (no auth), ordered by `CreatedAt DESC, Id DESC`
- [ ] 6.2 Create `GetLearningPathBySlug` handler — require the path and all referenced courses to be Published; return nested sections + course summaries with optional auth for derived progress from Enrollments
- [ ] 6.3 Register public routes `GET /api/learning-paths` and `GET /api/learning-paths/{slug}`

## 7. Backend — Integration Tests

- [ ] 7.1 Test create path with sections and courses (happy path)
- [ ] 7.2 Test duplicate slug → 409, unpublished course → 400, duplicate course in path → 400
- [ ] 7.3 Test publish blocked when course not published; publish success
- [ ] 7.4 Test public list returns only published; draft slug → 404
- [ ] 7.5 Test authenticated detail includes derived progress; guest omits progress
- [ ] 7.6 Test student cannot access admin endpoints → 403
- [ ] 7.7 Test publishing an empty path is rejected and Archived paths cannot be edited, published, or unpublished
- [ ] 7.8 Test unpublishing or archiving a course atomically returns affected Published paths to Draft

## 8. Frontend — Feature Module

- [ ] 8.1 Create `features/learning-paths/types.ts` with Zod schemas and TypeScript interfaces for admin and public DTOs
- [ ] 8.2 Create `features/learning-paths/api.ts` with TanStack Query hooks: public list/detail, admin list/detail, create, update, publish, unpublish, archive
- [ ] 8.3 Add helper to fetch published courses for course picker (reuse or extend existing admin courses hook filtered to Published)

## 9. Frontend — Admin Pages

- [ ] 9.1 Create `AdminLearningPathsPage.tsx` — table with title, slug, status, course count, actions (edit, publish/unpublish, archive)
- [ ] 9.2 Create shared `LearningPathForm.tsx` — path fields + section builder (add/remove/reorder sections, add/remove/reorder courses per section via published course picker)
- [ ] 9.3 Create `CreateLearningPathPage.tsx` and `EditLearningPathPage.tsx` wrapping the form
- [ ] 9.4 Implement loading, error, and empty states; toast notifications; AlertDialog for archive confirmation

## 10. Frontend — Public Pages

- [ ] 10.1 Create `LearningPathsPage.tsx` at `/learning-paths` — card grid of published paths
- [ ] 10.2 Create `LearningPathDetailPage.tsx` at `/learning-paths/:slug` — hero, estimated duration, section headings, ordered course cards linking to `/courses/:slug`
- [ ] 10.3 Show derived progress header for authenticated users; per-course enrolled/completed badges
- [ ] 10.4 Implement loading, error, and empty states on both pages

## 11. Frontend — Routing & Navigation

- [ ] 11.1 Register routes in `router.tsx`: public `/learning-paths`, `/learning-paths/:slug`; admin `/admin/learning-paths`, `/new`, `/:id/edit` under `AdminRoute`
- [ ] 11.2 Add "Learning Paths" to admin sidebar navigation
- [ ] 11.3 Add "Learning Paths" link to public/student header navigation (alongside Courses)

## 12. Documentation

- [ ] 12.1 Update `knowledge-graph/entities.md` with LearningPath, LearningPathSection, LearningPathCourse
- [ ] 12.2 Update `knowledge-graph/api-map.md` with new endpoints
- [ ] 12.3 Update `knowledge-graph/dependency-graph.md` and `specs/PROGRESS.md` when sprint completes

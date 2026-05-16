# Sprint 3 — Course Management Backend

## Propose

### Goal
Admin can create, update, publish, unpublish, and delete courses via REST API. Students and guests can browse published courses. Every course has a unique slug.

### Why This Sprint
Courses are the central entity of the platform. Every lesson, enrollment, quiz, and progress record depends on a course existing. The backend must be solid before any frontend is built on top of it.

### Success Criteria
- Admin can create a draft course (POST /api/admin/courses)
- Admin can publish a course (POST /api/admin/courses/{id}/publish)
- Guest/student can list only published courses (GET /api/courses)
- Duplicate slug returns 409 Conflict
- Student cannot call admin course endpoints (403)

---

## Design

### Technical Design

**Entities:**
- `Author` — optional link to a user, display info for course creator
- `Course` — linked to Author, has Slug (unique), Status (Draft/Published/Archived)
- `CourseStatus` enum: Draft, Published, Archived

**Validation:**
- Title: required, 1–200 chars
- Slug: required, lowercase-kebab-case, unique across all courses
- AuthorId: must reference an existing Author

**Pagination:**
- GET /api/courses — paginated (page, pageSize), filter by category/level
- GET /api/admin/courses — paginated, filter by status/title

**Slug uniqueness:**
- Unique index on `Courses.Slug`
- Handler checks for conflict before insert — returns 409

### Architecture Decisions
- Course thumbnail is a URL string — no file upload in MVP
- Soft delete via `Status = Archived` — no hard deletes on courses
- `CourseStatus` enum stored as string in DB for readability

### Entities Affected
- **Author** (new): `Id`, `UserId?`, `Name`, `Bio`, `AvatarUrl`, `CreatedAt`, `UpdatedAt`
- **Course** (new): `Id`, `Title`, `Slug`, `ShortDescription`, `Description`, `ThumbnailUrl`, `Category`, `Level`, `Language`, `AuthorId`, `Status`, `CreatedAt`, `UpdatedAt`
- EF Core migration: `AddCoursesAndAuthors`

### API Changes

| Method | Path                              | Auth | Role         | New |
|--------|-----------------------------------|------|--------------|-----|
| GET    | /api/courses                      | No   | Any          | ✓   |
| GET    | /api/courses/{slug}               | No   | Any          | ✓   |
| GET    | /api/admin/courses                | Yes  | Admin+       | ✓   |
| GET    | /api/admin/courses/{id}           | Yes  | Admin+       | ✓   |
| POST   | /api/admin/courses                | Yes  | Admin+       | ✓   |
| PUT    | /api/admin/courses/{id}           | Yes  | Admin+       | ✓   |
| DELETE | /api/admin/courses/{id}           | Yes  | Admin+       | ✓   |
| POST   | /api/admin/courses/{id}/publish   | Yes  | Admin+       | ✓   |
| POST   | /api/admin/courses/{id}/unpublish | Yes  | Admin+       | ✓   |
| GET    | /api/admin/authors                | Yes  | Admin+       | ✓   |
| POST   | /api/admin/authors                | Yes  | Admin+       | ✓   |

### Frontend Changes
None this sprint. Frontend is Sprint 4.

---

## Tasks

### Backend
- [ ] Create `Author.cs` entity in `Domain/Entities/`
- [ ] Create `Course.cs` entity in `Domain/Entities/`
- [ ] Create `CourseStatus.cs` enum in `Domain/Enums/`
- [ ] Add `Authors` and `Courses` DbSets to `AppDbContext`
- [ ] Configure EF Core relationships and unique indexes
- [ ] Add migration: `AddCoursesAndAuthors`
- [ ] Implement `GetPublishedCourses` vertical slice (paginated, public)
- [ ] Implement `GetCourseBySlug` vertical slice (public)
- [ ] Implement `GetAdminCourses` vertical slice (paginated, admin only)
- [ ] Implement `GetAdminCourse` vertical slice (by id, admin only)
- [ ] Implement `CreateCourse` vertical slice
- [ ] Implement `UpdateCourse` vertical slice
- [ ] Implement `DeleteCourse` vertical slice (soft delete → Archived)
- [ ] Implement `PublishCourse` vertical slice
- [ ] Implement `UnpublishCourse` vertical slice
- [ ] Implement `GetAuthors` vertical slice (admin, for dropdown)
- [ ] Implement `CreateAuthor` vertical slice
- [ ] Write unit tests for slug uniqueness check
- [ ] Write unit tests for publish/unpublish state transitions
- [ ] Verify `dotnet build` — 0 errors

---

## Archive

### Status: 🔲 Not Started
### Completed: —

### What Was Built
_To be filled after sprint completes._

### Known Issues
_To be filled after sprint completes._

### Notes
_To be filled after sprint completes._

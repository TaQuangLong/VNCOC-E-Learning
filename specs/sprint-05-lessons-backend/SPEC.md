# Sprint 5 — Lesson Management Backend

## Propose

### Goal
Admin can create, update, reorder, and delete lessons within a course. Each lesson supports one of three content types: YouTube video, text/HTML content, or external PDF URL. Enrolled students can retrieve lesson data.

### Why This Sprint
Lessons are the core learning content. Without lesson CRUD APIs, the learning page and progress tracking cannot be built. The backend must be stable before any frontend lesson work begins.

### Success Criteria
- Admin can add a lesson to a course with content type (Video / Text / PDF)
- Lessons return in the correct order (OrderIndex)
- Admin can reorder lessons
- YouTube URL and PDF URL are validated as proper URLs
- Enrolled students can fetch lesson detail; unenrolled students see preview lessons only
- Unauthenticated users cannot access locked lessons

---

## Design

### Technical Design

**Entities:**
- `Lesson` — belongs to Course, has `ContentType` enum, `OrderIndex`
- `Resource` — optional linked file/URL per lesson
- `ContentType` enum: Video, Text, Pdf

**Content Rules:**
- Video: requires `YouTubeUrl` — validated as YouTube URL
- Text: requires `TextContent` — stored as markdown or plain text
- PDF: requires `PdfUrl` — validated as URL (external only, no upload)
- A lesson can technically have more than one content field but only the matching type is rendered

**Ordering:**
- `OrderIndex` is integer, set by admin
- Reorder endpoint accepts ordered list of lesson IDs → updates all indexes in one transaction

**Access Control:**
- `IsPreview = true` → accessible to all, including unauthenticated users
- `IsPreview = false` → requires active enrollment
- Admin always has full access

### Architecture Decisions
- PDF is an external URL only — no file upload in MVP
- `TextContent` stored in DB (not a CDN) — adequate for ~1,000 users
- Resources are separate entity — allows multiple resources per lesson

### Entities Affected
- **Lesson** (new): `Id`, `CourseId`, `Title`, `Description`, `ContentType`, `YouTubeUrl`, `TextContent`, `PdfUrl`, `DurationSeconds`, `OrderIndex`, `IsPreview`, `CreatedAt`, `UpdatedAt`
- **Resource** (new): `Id`, `LessonId`, `Title`, `Url`, `CreatedAt`
- EF Core migration: `AddLessonsAndResources`
- Indexes: `Lessons(CourseId, OrderIndex)`, `Resources(LessonId)`

### API Changes

| Method | Path                                        | Auth | Role         | New |
|--------|---------------------------------------------|------|--------------|-----|
| GET    | /api/courses/{courseId}/lessons             | No   | Any          | ✓   |
| GET    | /api/lessons/{id}                           | Yes  | Student+     | ✓   |
| POST   | /api/admin/courses/{courseId}/lessons       | Yes  | Admin+       | ✓   |
| PUT    | /api/admin/lessons/{id}                     | Yes  | Admin+       | ✓   |
| DELETE | /api/admin/lessons/{id}                     | Yes  | Admin+       | ✓   |
| PUT    | /api/admin/courses/{courseId}/lessons/order | Yes  | Admin+       | ✓   |
| GET    | /api/admin/lessons/{id}/resources           | Yes  | Admin+       | ✓   |
| POST   | /api/admin/lessons/{id}/resources           | Yes  | Admin+       | ✓   |
| DELETE | /api/admin/resources/{id}                   | Yes  | Admin+       | ✓   |

### Frontend Changes
None this sprint. Frontend is Sprint 6.

---

## Tasks

### Backend
- [ ] Create `ContentType.cs` enum in `Domain/Enums/`
- [ ] Create `Lesson.cs` entity in `Domain/Entities/`
- [ ] Create `Resource.cs` entity in `Domain/Entities/`
- [ ] Add `Lessons` and `Resources` DbSets to `AppDbContext`
- [ ] Configure EF Core relationships and indexes
- [ ] Add migration: `AddLessonsAndResources`
- [ ] Implement `GetCourseLessons` vertical slice (public, preview-aware)
- [ ] Implement `GetLesson` vertical slice (enrollment-gated, with preview support)
- [ ] Implement `CreateLesson` vertical slice
- [ ] Implement `UpdateLesson` vertical slice
- [ ] Implement `DeleteLesson` vertical slice
- [ ] Implement `ReorderLessons` vertical slice
- [ ] Implement `GetLessonResources` vertical slice
- [ ] Implement `AddResource` vertical slice
- [ ] Implement `DeleteResource` vertical slice
- [ ] Add YouTube URL format validator
- [ ] Add PDF URL format validator
- [ ] Write unit tests for lesson ordering logic
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

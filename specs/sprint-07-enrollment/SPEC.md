# Sprint 7 — Enrollment & My Learning

## Propose

### Goal
Authenticated students can enroll in published courses. Enrolled courses appear on a "My Learning" page. Students cannot enroll twice in the same course. Non-enrolled students are blocked from lesson content.

### Why This Sprint
Enrollment is the gate between browsing and learning. Without it, there is no way to know which students have access to which lessons, and progress tracking cannot be scoped per user per course.

### Success Criteria
- Student can click Enroll on a course detail page and gain access
- Duplicate enrollment returns a clear error (not 500)
- Enrolled courses appear on `/my-learning` with continue learning button
- Continue Learning opens the learning page at the last lesson (or first lesson)
- Unenrolled student trying to access a locked lesson is redirected to course detail

---

## Design

### Technical Design

**Entities:**
- `Enrollment` — `(UserId, CourseId)` unique index, tracks enrollment date and progress stats

**Enrollment Flow:**
1. Student calls `POST /api/courses/{courseId}/enroll`
2. Handler checks course is Published
3. Handler checks no duplicate enrollment exists
4. Creates Enrollment row with initial `ProgressPercent = 0`, `CompletedLessonsCount = 0`, `TotalLessonsCount = count of lessons`
5. Returns enrollment confirmation

**My Learning:**
- `GET /api/me/courses` — returns all enrolled courses with progress summary
- Each item includes: course title, thumbnail, progress %, last accessed lesson ID

**Continue Learning:**
- `LastAccessedLessonId` stored on Enrollment (updated when student opens a lesson)
- Frontend uses this to navigate to the right lesson on "Continue Learning" click

**Access Guard:**
- `GetLesson` handler from Sprint 5 checks enrollment before returning full lesson data
- `IsPreview = true` lessons bypass enrollment check

### Architecture Decisions
- `TotalLessonsCount` is denormalized on Enrollment at enrollment time — acceptable for MVP, updated when admin adds/removes lessons
- No wait-list or approval — all published courses are open enrollment
- Students cannot unenroll in MVP (Could Have)

### Entities Affected
- **Enrollment** (new): `Id`, `UserId`, `CourseId`, `EnrolledAt`, `ProgressPercent`, `CompletedLessonsCount`, `TotalLessonsCount`, `LastAccessedLessonId?`, `CompletedAt?`
- EF Core migration: `AddEnrollments`
- Indexes: unique `(UserId, CourseId)`, `UserId`, `CourseId`

### API Changes

| Method | Path                              | Auth | Role     | New |
|--------|-----------------------------------|------|----------|-----|
| POST   | /api/courses/{courseId}/enroll    | Yes  | Student+ | ✓   |
| GET    | /api/me/courses                   | Yes  | Student+ | ✓   |
| GET    | /api/me/courses/{courseId}        | Yes  | Student+ | ✓   |
| GET    | /api/courses/{courseId}/enrollment-status | Yes | Student+ | ✓ |

### Frontend Changes
- `src/features/enrollment/` — api.ts, types.ts
- `src/pages/student/MyLearningPage.tsx`
- Update `CourseDetailPage.tsx` — add Enroll button / Already Enrolled state
- Update `LearnPage.tsx` — enrollment gate, redirect if not enrolled

---

## Tasks

### Backend
- [ ] Create `Enrollment.cs` entity in `Domain/Entities/`
- [ ] Add `Enrollments` DbSet to `AppDbContext`
- [ ] Configure unique index on `(UserId, CourseId)`
- [ ] Add migration: `AddEnrollments`
- [ ] Implement `EnrollCourse` vertical slice
- [ ] Implement `GetMyEnrolledCourses` vertical slice
- [ ] Implement `GetMyEnrollmentStatus` vertical slice
- [ ] Update `GetLesson` handler — enrollment gate for non-preview lessons
- [ ] Update `Enrollment.TotalLessonsCount` when lessons are added/removed (Sprint 5 handlers)
- [ ] Write unit tests for duplicate enrollment check
- [ ] Verify `dotnet build` — 0 errors

### Frontend
- [ ] Create `src/features/enrollment/types.ts`
- [ ] Create `src/features/enrollment/api.ts`
- [ ] Create `MyLearningPage.tsx` — enrolled courses grid with progress bars
- [ ] Update `CourseDetailPage.tsx` — show Enroll / Continue Learning / Enrolled badge
- [ ] Add enrollment gate in `LearnPage.tsx` — redirect unenrolled student
- [ ] Add empty state for My Learning (no enrollments yet)
- [ ] Add loading state for enrollment action
- [ ] Verify `npm run build` — 0 errors

---

## Archive

### Status: ✅ Done
### Completed: Sprint 7

### What Was Built
- `Enrollment` entity with `LastAccessedLessonId` field (extends spec)
- Unique `(UserId, CourseId)` index + indexes on `UserId`, `CourseId`
- EF Core migration: `AddEnrollmentFields`
- `EnrollCourse` vertical slice — checks published status, prevents duplicate, sets `TotalLessonsCount`
- `GetMyEnrolledCourses` vertical slice — returns all enrolled courses with progress summary
- `GetMyEnrollmentStatus` vertical slice — returns enrollment state including `LastAccessedLessonId`
- `EnrollmentsEndpoints` — routes: POST enroll, GET /me/courses, GET /me/courses/{courseId}, GET enrollment-status
- `CreateLesson` / `DeleteLesson` handlers updated to keep `TotalLessonsCount` in sync
- `GetLesson` handler — enrollment gate (non-preview lessons require enrollment; admins bypass)
- `MyLearningPage.tsx` — enrolled courses grid with progress bars, loading/empty/error states
- `CourseDetailPage.tsx` — Enroll / Continue Learning / Login to Enroll states
- `LearnPage.tsx` — enrollment gate redirects unenrolled student to /courses
- `/my-learning` route registered in router.tsx
- 5 unit tests covering: happy path, duplicate, draft course, not found, TotalLessonsCount

### Known Issues
_None._

### Notes
- `TotalLessonsCount` is denormalized on `Enrollment` — updated on lesson create/delete
- No unenroll in MVP (Could Have)

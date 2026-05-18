# Sprint 8 — Progress Tracking

## Propose

### Goal
Students can manually mark lessons as completed. Course progress percentage is calculated and displayed. Students can continue from their last-accessed lesson. Progress persists after logout and login.

### Why This Sprint
Progress tracking is what makes the platform feel like a learning system, not just a content viewer. Members need to see how far they are in a course and be able to pick up where they left off.

### Success Criteria
- Student can click "Mark as Completed" on any lesson
- Course progress percentage updates immediately
- Completed lessons show a checkmark in the sidebar
- Progress percentage is correct (completedLessons / totalLessons × 100)
- Last-accessed lesson is recorded and restored on "Continue Learning"
- Progress data persists across logout/login

---

## Design

### Technical Design

**Entities:**
- `LessonProgress` — per user per lesson, tracks completion + video position

**Mark Complete Flow:**
1. Student calls `POST /api/lessons/{lessonId}/complete`
2. Handler upserts `LessonProgress` row for `(UserId, LessonId)`
3. Handler recalculates `Enrollment.ProgressPercent` and `CompletedLessonsCount`
4. Updates `Enrollment.LastAccessedLessonId`
5. Returns updated progress summary

**Progress Calculation:**
```
ProgressPercent = (CompletedLessonsCount / TotalLessonsCount) × 100
```
- Rounded to nearest integer
- `TotalLessonsCount` taken from `Enrollment` (denormalized)
- Recalculated on every mark/unmark

**Video Progress (optional, stored but not required for MVP):**
- `POST /api/lessons/{lessonId}/video-progress` — stores `VideoProgressPercent` and `VideoWatchedSeconds`
- Not required for progress calculation — manual mark-complete is the gate

**Get Course Progress:**
- `GET /api/me/courses/{courseId}/progress` — returns lesson-by-lesson completion status

### Architecture Decisions
- Manual completion is the only gate in MVP — no auto-complete on video watch percentage
- Video progress stored for future use but not surfaced in MVP UI
- Unmark (un-complete) is optional and not surfaced in UI for MVP

### Entities Affected
- **LessonProgress** (new): `Id`, `UserId`, `CourseId`, `LessonId`, `IsCompleted`, `CompletedAt?`, `VideoProgressPercent`, `VideoWatchedSeconds`, `LastWatchedAt?`
- Update **Enrollment**: no new fields, but `ProgressPercent`, `CompletedLessonsCount`, `LastAccessedLessonId` are now actively updated
- EF Core migration: `AddLessonProgress`
- Indexes: unique `(UserId, LessonId)`, `(UserId, CourseId)`

### API Changes

| Method | Path                                    | Auth | Role     | New |
|--------|-----------------------------------------|------|----------|-----|
| POST   | /api/lessons/{lessonId}/complete        | Yes  | Student+ | ✓   |
| POST   | /api/lessons/{lessonId}/video-progress  | Yes  | Student+ | ✓   |
| GET    | /api/me/courses/{courseId}/progress     | Yes  | Student+ | ✓   |

### Frontend Changes
- Update `LessonSidebar.tsx` — show checkmarks on completed lessons
- Update `LearnPage.tsx` — show Mark as Completed button, update on success
- Update `MyLearningPage.tsx` — show real progress bar
- `src/features/progress/` — api.ts, types.ts

---

## Tasks

### Backend
- [x] Create `LessonProgress.cs` entity in `Domain/Entities/`
- [x] Add `LessonProgresses` DbSet to `AppDbContext`
- [x] Configure unique index on `(UserId, LessonId)` and index on `(UserId, CourseId)`
- [x] Add migration: `AddLessonProgress`
- [x] Implement `MarkLessonComplete` vertical slice (upsert + recalculate enrollment progress)
- [x] Implement `SaveVideoProgress` vertical slice
- [x] Implement `GetCourseProgress` vertical slice
- [x] Write unit tests for `CalculateCourseProgress` logic
- [x] Verify `dotnet build` — 0 errors

### Frontend
- [x] Create `src/features/progress/types.ts`
- [x] Create `src/features/progress/api.ts`
- [x] Update `LessonSidebar.tsx` — show checkmark icons on completed lessons
- [x] Update `LearnPage.tsx` — Mark as Completed button with optimistic UI
- [ ] Update `MyLearningPage.tsx` — real progress percentage from API
- [ ] Add progress ring or progress bar component using shadcn/ui Progress
- [x] Verify `npm run build` — 0 errors

---

## Archive

### Status: ✅ Complete
### Completed: 2026-05-17

### What Was Built
- `LessonProgress` entity with unique index on `(UserId, LessonId)` and composite index on `(UserId, CourseId)`.
- EF Core migration `AddLessonProgress`.
- `MarkLessonComplete` handler — idempotent upsert, recalculates `Enrollment.ProgressPercent` and `CompletedLessonsCount` in a single `SaveChanges` call. Updates `LastAccessedLessonId`.
- `SaveVideoProgress` handler — stores video watch position; does not affect completion.
- `GetCourseProgress` handler — returns per-lesson completion status for a course.
- `ProgressEndpoints` + `ProgressServiceRegistration` wired into `Program.cs`.
- 12 unit tests covering happy path, idempotency, not-enrolled forbidden, not-found, and rounding theory tests (31 total pass).
- Frontend: `src/features/progress/types.ts` and `api.ts` with `useCourseProgress`, `useMarkLessonComplete`, `useSaveVideoProgress`.
- `LessonSidebar` now accepts `completedLessonIds` and renders a checkmark SVG on completed lessons.
- `LearnPage` shows a "Mark as Completed" button (disabled during mutation) that turns into a green "Lesson completed" badge once done.
- `MyLearningPage` already displayed live `progressPercent` from the enrollment endpoint — no changes needed.

### Known Issues
- `MyLearningPage` progress bar and shadcn/ui `Progress` component task left unchecked as the existing `ProgressBar` component already fulfils the requirement visually.

### Notes
- Video progress is stored for future use; it does not gate completion.
- Unmark (un-complete) not surfaced in MVP UI per spec.

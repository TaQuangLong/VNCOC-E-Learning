## Context

The `/dashboard` route in `router.tsx` renders a static `<div>Dashboard (coming soon)</div>` inside a `ProtectedRoute`. All necessary backend endpoints are already implemented and tested:
- `GET /api/enrollments/me/courses` — returns `MyEnrolledCourse[]` (progress %, last lesson, lesson counts)
- `GET /api/courses` — returns paginated `CourseSummary[]` for public published courses

Frontend hooks `useMyEnrolledCourses()` and `usePublishedCourses()` are already available in `features/enrollment/api.ts` and `features/courses/api.ts` respectively. No backend work or new dependencies are needed.

## Goals / Non-Goals

**Goals:**
- Replace the placeholder at `/dashboard` with `StudentDashboardPage`
- Show a "Continue Learning" hero card resuming from the student's last active course/lesson
- Show enrolled courses in a Udemy-style card grid with progress bars and lesson counts
- Show published courses the student is not enrolled in for discovery
- Handle all three states per section: loading skeleton, error, and empty

**Non-Goals:**
- No new backend endpoints
- No charts or analytics on this page (that is the admin reports domain)
- No pagination on the dashboard (show top 6 browse courses max)
- No quiz history on this page
- No notification system

## Decisions

### D1: Single page component, no sub-routes
**Decision**: Implement as a single `StudentDashboardPage.tsx` under `pages/student/`.
**Rationale**: The dashboard is a single view with three stacked sections. Sub-routing would add complexity with no benefit. Consistent with how `MyLearningPage` is structured.

### D2: Reuse existing hooks, no new API calls
**Decision**: Use `useMyEnrolledCourses()` for enrolled data and `usePublishedCourses({ pageSize: 6 })` for discovery — no new custom hooks.
**Rationale**: Avoids duplicating query logic. Both hooks already handle caching via TanStack Query.

### D3: Filter browse courses client-side
**Decision**: Filter out already-enrolled courses from the "Browse" section by comparing `courseId` against enrolled course IDs.
**Rationale**: The number of courses is small (~tens). Avoids a new backend endpoint for "unenrolled published courses". If the catalogue grows large, this can be moved to a backend filter.

### D4: "Continue Learning" uses last enrolled course with a lesson
**Decision**: Find the first enrolled course where `lastAccessedLessonId !== null`, falling back to the most recently enrolled course.
**Rationale**: `lastAccessedLessonId` already populated by the learn page. Simple heuristic that matches user expectation.

## Risks / Trade-offs

- [Small catalogue assumption] Fetching all published courses for client-side filtering works at ~100 courses; beyond that a backend filter is needed → Mitigation: `pageSize: 6` for browse section limits over-fetching today; revisit if course count exceeds 50
- [Two parallel requests on dashboard load] Both `useMyEnrolledCourses` and `usePublishedCourses` fire simultaneously → Mitigation: TanStack Query caches both; subsequent visits are instant

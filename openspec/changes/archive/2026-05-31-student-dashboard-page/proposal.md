## Why

The `/dashboard` route currently shows a placeholder "Dashboard (coming soon)" — authenticated students land here after login with no useful content. Students need a central home page that shows their enrolled courses, learning progress, and a quick way to continue learning.

## What Changes

- Replace the `/dashboard` placeholder with a fully implemented `StudentDashboardPage` component
- Add a "Continue Learning" hero card that resumes from the student's last accessed lesson
- Add a "My Courses" section with a Udemy-style course grid (enrolled courses with progress bars)
- Add a "Browse All Courses" section showing published courses the student is not yet enrolled in
- Wire the `/dashboard` route in `router.tsx` to the new page component

## Capabilities

### New Capabilities

- `student-dashboard`: A personalized dashboard page at `/dashboard` for authenticated students showing enrolled course progress and course discovery

### Modified Capabilities

- `routing`: The `/dashboard` route changes from a placeholder `<div>` to `<StudentDashboardPage />` — no spec-level behavior change, implementation only

## Impact

- **Frontend files changed**: `router.tsx`, new `StudentDashboardPage.tsx`
- **APIs used**: `GET /api/enrollments/my` (already exists via `useMyEnrolledCourses`), `GET /api/courses` (already exists via `useCourses`)
- **No backend changes required** — all data is available from existing endpoints
- **No new dependencies required** — uses existing shadcn/ui, TanStack Query, Lucide icons

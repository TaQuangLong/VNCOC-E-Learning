## 1. Page Component

- [x] 1.1 Create `frontend/src/pages/student/StudentDashboardPage.tsx` with the three-section layout (hero, my courses, browse)
- [x] 1.2 Implement the "Continue Learning" hero card using `lastAccessedLessonId` from `useMyEnrolledCourses()`
- [x] 1.3 Implement the "My Courses" responsive card grid with progress bars and lesson counts
- [x] 1.4 Implement the "Browse All Courses" section filtering out already-enrolled courses from `usePublishedCourses({ pageSize: 6 })`
- [x] 1.5 Add loading skeleton states for both the My Courses and Browse sections
- [x] 1.6 Add empty state for My Courses section (no enrollments)
- [x] 1.7 Handle error state for both data fetches

## 2. Routing

- [x] 2.1 Update `frontend/src/app/router.tsx` — replace the `/dashboard` placeholder `<div>` with `<StudentDashboardPage />` and add the import

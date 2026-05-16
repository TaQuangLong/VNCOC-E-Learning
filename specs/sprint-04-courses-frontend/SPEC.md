# Sprint 4 — Course Management Frontend

## Propose

### Goal
Admin can manage courses from the browser UI. Students can browse the published course catalog and view course detail pages. All pages are mobile-responsive.

### Why This Sprint
The course catalog is the first thing a church member sees when they open the platform. It must be clean, fast, and usable on mobile before any deeper features are added.

### Success Criteria
- Admin can create, edit, publish, and unpublish courses from the UI
- Public course catalog displays published courses (1 col mobile / 2 col tablet / 3 col desktop)
- Course detail page shows description, author, lessons count
- All pages handle loading, error, and empty states
- Draft courses are not visible to students

---

## Design

### Technical Design

**Admin Pages:**
- `/admin/courses` — paginated list, status filter, create button
- `/admin/courses/new` — create course form
- `/admin/courses/:id/edit` — edit course form
- Status badge: Draft (gray), Published (green), Archived (red)

**Student/Public Pages:**
- `/courses` — public catalog, search by title, filter by category/level
- `/courses/:slug` — course detail: title, description, author, lessons list, enroll button (Sprint 7)

**Forms:**
- React Hook Form + Zod for all forms
- Slug auto-generated from title on create (editable)
- Author select from `/api/admin/authors`

**State:**
- TanStack Query for all data fetching
- Optimistic UI not required — simple refetch on mutation success
- Toast notification on create/update/publish success

### Architecture Decisions
- Course thumbnail: URL input field only — no file upload
- Slug generation: client-side from title (lowercase, hyphenated), but server validates uniqueness
- Admin pages protected by `AdminRoute` component

### Entities Affected
None — no new entities. Consumes Sprint 3 API.

### API Changes
None — all APIs from Sprint 3.

### Frontend Changes
- `src/features/courses/` — api.ts, types.ts, CourseCard.tsx, CourseForm.tsx
- `src/pages/public/CoursesPage.tsx`
- `src/pages/public/CourseDetailPage.tsx`
- `src/pages/admin/AdminCoursesPage.tsx`
- `src/pages/admin/CreateCoursePage.tsx`
- `src/pages/admin/EditCoursePage.tsx`
- Route updates in `router.tsx`

---

## Tasks

### Frontend
- [ ] Create `src/features/courses/types.ts` — Course, Author TypeScript types + Zod schemas
- [ ] Create `src/features/courses/api.ts` — TanStack Query hooks for all course APIs
- [ ] Create `CourseCard.tsx` — responsive card (thumbnail, title, author, status badge)
- [ ] Create `CourseForm.tsx` — shared form for create and edit
- [ ] Create `CoursesPage.tsx` — public catalog with search/filter, responsive grid
- [ ] Create `CourseDetailPage.tsx` — full course detail (public)
- [ ] Create `AdminCoursesPage.tsx` — admin list with status filter, publish/unpublish actions
- [ ] Create `CreateCoursePage.tsx` — uses CourseForm in create mode
- [ ] Create `EditCoursePage.tsx` — uses CourseForm in edit mode
- [ ] Add slug auto-generation utility from title string
- [ ] Add status badges using shadcn/ui Badge component
- [ ] Add loading skeletons for course list and detail
- [ ] Add empty state for no courses
- [ ] Add error state for API failures
- [ ] Register all routes in `router.tsx`
- [ ] Verify all pages render correctly on mobile (360px), tablet (768px), desktop (1024px)
- [ ] Verify `npm run build` — 0 errors

### Backend (minor additions)
- [ ] Add search by title query param to `GetPublishedCourses`
- [ ] Add filter by category and level query params
- [ ] Confirm pagination response includes `totalCount`, `page`, `pageSize`

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

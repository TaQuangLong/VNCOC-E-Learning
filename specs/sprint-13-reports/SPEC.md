# Sprint 13 — Admin Reports & Dashboard

## Propose

### Goal
Admins can see a platform overview dashboard and drill into learner progress reports. Reports show total users, enrollments, course completion rates, and per-user progress detail.

### Why This Sprint
Reports let church leadership see whether the platform is being used and which courses are effective. This is what justifies the platform investment internally.

### Success Criteria
- Admin sees overview stats on the dashboard (total users, courses, enrollments)
- Admin can view a course learner list with per-learner progress
- Admin can view one user's complete progress across all courses
- All report queries are read-only and do not affect application state

---

## Design

### Technical Design

**Overview Report:**
- Total registered users
- Total published courses
- Total active enrollments
- Total quiz attempts
- Recent registrations (last 7 days)
- Most popular courses (by enrollment count)

**Course Learner Report:**
- List all enrolled students for a course
- Per student: enrollment date, progress %, completed lessons, quiz passed count

**User Progress Report:**
- All courses a user is enrolled in
- Per course: enrollment date, progress %, completion status, quiz results

**Implementation:**
- All reports use direct EF Core queries — no separate reporting database
- Results are not cached in MVP — acceptable for ~1,000 users
- Queries use `.AsNoTracking()` for performance

### Architecture Decisions
- No real-time streaming of report data — simple request/response
- No export to CSV in MVP (Could Have)
- Pagination applied to course learner list (could be 1,000+ rows)

### Entities Affected
None — no new entities. Reads existing data.

### API Changes

| Method | Path                                          | Auth | Role   | New |
|--------|-----------------------------------------------|------|--------|-----|
| GET    | /api/admin/reports/overview                   | Yes  | Admin+ | ✓   |
| GET    | /api/admin/reports/courses/{courseId}/learners | Yes | Admin+ | ✓  |
| GET    | /api/admin/reports/users/{userId}/progress    | Yes  | Admin+ | ✓   |

### Frontend Changes
- `src/features/reports/` — api.ts, types.ts, OverviewCards.tsx, CourseLearnersTable.tsx, UserProgressDetail.tsx
- `src/pages/admin/AdminDashboardPage.tsx`
- `src/pages/admin/CourseLearnersPage.tsx`
- `src/pages/admin/UserProgressPage.tsx`
- Link report pages from admin sidebar/navigation

---

## Tasks

### Backend
- [ ] Implement `GetAdminOverview` vertical slice
- [ ] Implement `GetCourseLearners` vertical slice (paginated)
- [ ] Implement `GetUserProgress` vertical slice
- [ ] Add `.AsNoTracking()` to all report queries
- [ ] Verify `dotnet build` — 0 errors

### Frontend
- [ ] Create `src/features/reports/types.ts`
- [ ] Create `src/features/reports/api.ts`
- [ ] Create `OverviewCards.tsx` — stat cards for dashboard
- [ ] Create `CourseLearnersTable.tsx` — sortable table
- [ ] Create `UserProgressDetail.tsx` — per-user progress breakdown
- [ ] Create `AdminDashboardPage.tsx`
- [ ] Create `CourseLearnersPage.tsx`
- [ ] Create `UserProgressPage.tsx`
- [ ] Add loading skeletons for all report pages
- [ ] Add empty state for no data
- [ ] Verify tables are horizontally scrollable on mobile
- [ ] Verify `npm run build` — 0 errors

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

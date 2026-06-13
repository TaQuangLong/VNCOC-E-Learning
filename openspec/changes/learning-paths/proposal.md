## Why

ChurchLearn courses exist independently, but members need curated **foundations** — ordered bundles that guide what to study first (similar to [Code with Mosh Fundamentals](https://codewithmosh.com/p/fundamentals)). Without learning paths, admins cannot organize existing courses into structured journeys, and students must discover the right sequence on their own.

## What Changes

- Add **LearningPath** entity with grouped **sections**, each containing ordered **courses** (many-to-many — a course can appear in multiple paths)
- Admin and SuperAdmin users can create, edit, publish, unpublish, and archive learning paths from existing **Published** courses
- Admin UI to manage paths: list, create/edit form with section builder (add/reorder sections, add/reorder courses within sections)
- Public **learning paths catalog** page (published paths visible to guests and students)
- Public **learning path detail** page showing path overview, grouped sections, and ordered courses with links to course detail pages
- Logged-in students see **derived progress** on path detail (e.g. "2 of 5 courses complete") computed from existing course enrollments — no separate path enrollment
- Students enroll in each course individually (browse-only guide model; path does not auto-enroll)
- Draft/Published/Archived status model consistent with courses

## Capabilities

### New Capabilities

- `learning-paths`: Full learning-path feature — admin CRUD with section/course ordering, public catalog and detail pages, and derived student progress display

### Modified Capabilities

- `courses`: Unpublishing or archiving a course automatically returns any published learning paths containing that course to Draft

## Impact

- **Backend**: New entities `LearningPath`, `LearningPathSection`, `LearningPathCourse`; new vertical slices under `Features/LearningPaths/`; EF Core migration; public and admin API endpoints; course unpublish/archive handlers maintain published-path integrity
- **Frontend**: New feature module `features/learning-paths/`; admin pages under `/admin/learning-paths`; public pages `/learning-paths` and `/learning-paths/:slug`; navigation updates (admin sidebar, optional student/public nav link)
- **Data**: Join table for path-section-course ordering; unique slug on learning paths; only Published courses eligible for inclusion
- **Dependencies**: Builds on existing Course, Enrollment, and Author entities — no changes to enrollment model

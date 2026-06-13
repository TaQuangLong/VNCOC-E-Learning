## ADDED Requirements

### Requirement: Learning path data model
The system SHALL persist learning paths as a `LearningPath` entity with unique slug, title, optional descriptions and thumbnail URL, optional estimated duration label, status (Draft | Published | Archived), and timestamps. Each path SHALL contain ordered `LearningPathSection` records (title, optional description, order index). Each section SHALL contain ordered `LearningPathCourse` join rows referencing existing `Course` records by ID. A course MAY appear in multiple paths but SHALL appear at most once within a single path.

#### Scenario: Path with two sections and ordered courses
- **WHEN** an admin saves a path with section "Foundations" (courses A, B) and section "Advanced" (course C)
- **THEN** the database stores one learning path, two sections with distinct order indices, and three join rows preserving course order within each section

#### Scenario: Duplicate course in same path rejected
- **WHEN** an admin attempts to add the same course ID to two sections within one path
- **THEN** the system returns HTTP 400 with a validation error indicating duplicate course in path

---

### Requirement: Admin can list learning paths
The system SHALL expose `GET /api/admin/learning-paths` returning paginated learning path summaries (id, title, slug, status, section count, course count, createdAt, updatedAt). Access SHALL be restricted to Admin and SuperAdmin roles.

#### Scenario: Paths exist
- **WHEN** an admin requests the learning paths list
- **THEN** the response is HTTP 200 with a paginated list of all paths regardless of status

#### Scenario: Student denied
- **WHEN** a student requests `GET /api/admin/learning-paths`
- **THEN** the response is HTTP 403

---

### Requirement: Admin can create a learning path
The system SHALL allow an admin to create a draft learning path via `POST /api/admin/learning-paths` with title (required, ≤200 chars), slug (required, unique, kebab-case), optional shortDescription, description, thumbnailUrl, estimatedDurationLabel, and nested sections each containing ordered published course IDs.

#### Scenario: Valid create
- **WHEN** an admin submits a valid path with title "Fundamentals", unique slug "fundamentals", and at least one section with one published course
- **THEN** the system creates the path in Draft status and returns HTTP 201 with the created path ID

#### Scenario: Duplicate slug
- **WHEN** an admin submits a slug that already exists
- **THEN** the system returns HTTP 409

#### Scenario: Unpublished course in payload
- **WHEN** an admin includes a Draft or Archived course ID in sections
- **THEN** the system returns HTTP 400 with a validation error

---

### Requirement: Admin can update a learning path
The system SHALL allow an admin to update a non-Archived learning path via `PUT /api/admin/learning-paths/:id`, replacing path fields and the full section/course structure transactionally. Updating an Archived path SHALL return HTTP 409.

#### Scenario: Valid update with reordered sections
- **WHEN** an admin updates path ID 1 with reordered sections and courses
- **THEN** the system persists the new order, returns HTTP 200, and subsequent GET reflects the changes

#### Scenario: Path not found
- **WHEN** an admin updates path ID 999 which does not exist
- **THEN** the system returns HTTP 404

#### Scenario: Archived path cannot be updated
- **WHEN** an admin attempts to update an Archived path
- **THEN** the system returns HTTP 409 and leaves the path unchanged

---

### Requirement: Admin can publish and unpublish a learning path
The system SHALL expose `POST /api/admin/learning-paths/:id/publish` and `POST /api/admin/learning-paths/:id/unpublish`. Publish SHALL succeed only when the path contains at least one section with at least one course and all referenced courses are Published. Archived paths SHALL NOT be published or unpublished.

#### Scenario: Publish valid path
- **WHEN** an admin publishes a draft path whose courses are all Published
- **THEN** the path status becomes Published and returns HTTP 200

#### Scenario: Publish blocked by unpublished course
- **WHEN** an admin attempts to publish a path referencing a non-Published course
- **THEN** the system returns HTTP 400 with a message identifying the invalid course(s)

#### Scenario: Publish blocked for empty path
- **WHEN** an admin attempts to publish a path with no sections or no courses
- **THEN** the system returns HTTP 400 with a validation message requiring at least one course

#### Scenario: Unpublish
- **WHEN** an admin unpublishes a published path
- **THEN** the path status becomes Draft and it no longer appears in public list endpoints

#### Scenario: Archived path transition rejected
- **WHEN** an admin attempts to publish or unpublish an Archived path
- **THEN** the system returns HTTP 409 and leaves the path Archived

---

### Requirement: Admin can archive a learning path
The system SHALL allow an admin to archive a path via `DELETE /api/admin/learning-paths/:id` (soft delete setting status to Archived).

#### Scenario: Archive existing path
- **WHEN** an admin archives path ID 1
- **THEN** the path status becomes Archived, returns HTTP 204, and it no longer appears in public list endpoints

---

### Requirement: Course status changes preserve published-path integrity
When an admin unpublishes or archives a course, the system SHALL atomically change that course's status and return every Published learning path containing the course to Draft. Draft and Archived paths SHALL retain their course references. The system SHALL never expose a Published learning path containing a non-Published course.

#### Scenario: Unpublish course used by published paths
- **WHEN** an admin unpublishes a course referenced by two Published paths and one Draft path
- **THEN** the course becomes Draft, both Published paths become Draft, and the existing Draft path remains Draft with its reference intact

#### Scenario: Archive course and path transitions are atomic
- **WHEN** an admin archives a course referenced by a Published path
- **THEN** the course becomes Archived and the path becomes Draft in the same transaction

---

### Requirement: Public can browse published learning paths
The system SHALL expose `GET /api/learning-paths` returning paginated published paths whose referenced courses are all Published (title, slug, shortDescription, thumbnailUrl, estimatedDurationLabel, courseCount). Results SHALL be ordered by `CreatedAt` descending and then `Id` descending. No authentication required.

#### Scenario: Only published paths returned
- **WHEN** a guest requests the public learning paths list
- **THEN** the response includes only paths with status Published

#### Scenario: Empty catalog
- **WHEN** no published paths exist
- **THEN** the response is HTTP 200 with an empty items array

---

### Requirement: Public can view a published learning path detail
The system SHALL expose `GET /api/learning-paths/:slug` returning path metadata and nested sections with ordered course summaries (id, title, slug, shortDescription, thumbnailUrl, level, lessonCount). Access SHALL not require authentication when the path and all its referenced courses are Published. A path containing any non-Published course SHALL be treated as unavailable and return HTTP 404.

#### Scenario: Published path detail for guest
- **WHEN** a guest requests `GET /api/learning-paths/fundamentals` and the path is Published
- **THEN** the response is HTTP 200 with path info, sections, and course summaries in order

#### Scenario: Draft path not accessible publicly
- **WHEN** a guest requests a slug belonging to a Draft path
- **THEN** the response is HTTP 404

#### Scenario: Path not found
- **WHEN** a guest requests a slug that does not exist
- **THEN** the response is HTTP 404

---

### Requirement: Authenticated student sees derived path progress
When `GET /api/learning-paths/:slug` is called by an authenticated student, the response SHALL include derived progress: `completedCoursesCount`, `totalCoursesCount`, `progressPercent`, and per-course fields `isEnrolled`, `progressPercent`, and `isCompleted` based on existing Enrollment records. The system SHALL NOT create a path-level enrollment.

#### Scenario: Partially completed path
- **WHEN** a student enrolled in 2 of 5 path courses with 1 course fully completed requests path detail
- **THEN** the response includes `completedCoursesCount: 1`, `totalCoursesCount: 5`, and per-course progress flags reflecting enrollment state

#### Scenario: Guest sees no progress block
- **WHEN** an unauthenticated user requests path detail
- **THEN** the response omits student-specific progress fields

---

### Requirement: Admin learning paths UI
The admin portal SHALL provide pages at `/admin/learning-paths` (list), `/admin/learning-paths/new` (create), and `/admin/learning-paths/:id/edit` (edit). The edit/create form SHALL allow managing sections (add, remove, reorder) and adding/removing/reordering published courses within each section. List page SHALL support publish, unpublish, and archive actions.

#### Scenario: Admin creates a path via UI
- **WHEN** an admin fills the create form with title, slug, one section, and selects published courses
- **THEN** submitting creates the path and redirects to the list with a success message

#### Scenario: Loading and empty states
- **WHEN** the admin list is loading or no paths exist
- **THEN** the page shows skeleton rows or an empty state with a create call-to-action

---

### Requirement: Public learning paths UI
The study portal SHALL provide `/learning-paths` (catalog) and `/learning-paths/:slug` (detail). The catalog SHALL display published paths as cards. The detail page SHALL render path description, estimated duration (if set), grouped sections with ordered courses, and links to each course's detail page at `/courses/:slug`. Logged-in students SHALL see derived progress on the detail page.

#### Scenario: Guest browses catalog
- **WHEN** a guest navigates to `/learning-paths`
- **THEN** the page shows published path cards with title and short description

#### Scenario: Student views path with progress
- **WHEN** a logged-in student opens `/learning-paths/fundamentals`
- **THEN** the page shows section groupings, course cards with links, and a header progress indicator (e.g. "2 of 5 courses complete")

#### Scenario: Student enrolls via course link
- **WHEN** a student clicks a course within a path
- **THEN** they navigate to `/courses/:slug` where they can enroll using the existing enrollment flow

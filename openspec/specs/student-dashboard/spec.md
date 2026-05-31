## ADDED Requirements

### Requirement: Student dashboard renders at /dashboard
The system SHALL render a `StudentDashboardPage` component at the `/dashboard` route for all authenticated users (role: Student or Admin).

#### Scenario: Authenticated student visits /dashboard
- **WHEN** an authenticated user navigates to `/dashboard`
- **THEN** the page renders without redirect and displays the student dashboard

#### Scenario: Unauthenticated user visits /dashboard
- **WHEN** an unauthenticated user navigates to `/dashboard`
- **THEN** the `ProtectedRoute` redirects them to `/login`

---

### Requirement: Continue Learning hero card
The system SHALL display a prominent hero card showing the course the student was most recently learning, with a direct link to resume from the last accessed lesson.

#### Scenario: Student has at least one course with a last accessed lesson
- **WHEN** the dashboard loads and `lastAccessedLessonId` is non-null on at least one enrolled course
- **THEN** the hero card shows the course title, progress percentage, and a "Continue" button linking to `/learn/{courseId}/lessons/{lastAccessedLessonId}`

#### Scenario: Student is enrolled but has not started any lesson
- **WHEN** all enrolled courses have `lastAccessedLessonId === null`
- **THEN** the hero card shows the most recently enrolled course with a "Start Learning" button linking to `/learn/{courseId}`

#### Scenario: Student has no enrolled courses
- **WHEN** the enrolled courses list is empty
- **THEN** the hero card section is not rendered

---

### Requirement: My Courses grid
The system SHALL display all enrolled courses in a responsive card grid with progress indicators.

#### Scenario: Student has enrolled courses
- **WHEN** the dashboard loads with one or more enrolled courses
- **THEN** each course card shows: thumbnail (or fallback icon), title, category badge, progress bar, "X / Y lessons" count, and a "Continue" or "Start" button

#### Scenario: Loading state
- **WHEN** enrolled courses are being fetched
- **THEN** the grid shows animated skeleton placeholder cards

#### Scenario: Student has no enrolled courses
- **WHEN** the enrolled courses list is empty
- **THEN** the My Courses section shows an empty state message with a link to `/courses`

---

### Requirement: Browse All Courses discovery section
The system SHALL display a selection of published courses the student is not yet enrolled in, to encourage discovery.

#### Scenario: Published courses exist that student is not enrolled in
- **WHEN** the dashboard loads and there are published courses outside the student's enrollments
- **THEN** up to 6 unenrolled courses are shown in a card grid with title, category, and an "Enroll" or "View" button linking to `/courses/{slug}`

#### Scenario: Student is enrolled in all available courses
- **WHEN** the student is enrolled in all published courses
- **THEN** the Browse section is not rendered

#### Scenario: Loading state
- **WHEN** published courses are being fetched
- **THEN** the Browse section shows animated skeleton placeholder cards

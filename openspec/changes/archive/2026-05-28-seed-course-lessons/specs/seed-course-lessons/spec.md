## ADDED Requirements

### Requirement: Seed lessons for each course on startup
The `DatabaseSeeder` SHALL call `SeedLessonsAsync` after `SeedCoursesAsync` when seeding demo data. For each of the 15 seeded courses, the seeder SHALL insert exactly 6 `Video` lessons if the course currently has 0 lessons.

#### Scenario: Fresh database — all courses get lessons
- **WHEN** the application starts with an empty database
- **THEN** `SeedAsync` creates 15 courses and then seeds 6 lessons for each course (90 lessons total)

#### Scenario: Lessons already exist — seeder skips the course
- **WHEN** a course already has at least 1 lesson in the database
- **THEN** `SeedLessonsAsync` MUST NOT insert any additional lessons for that course

#### Scenario: Some courses have lessons, others do not
- **WHEN** the database has lessons for a subset of courses
- **THEN** `SeedLessonsAsync` seeds lessons only for the courses that have 0 lessons, leaving existing lessons untouched

### Requirement: Lesson field constraints
Each seeded lesson SHALL conform to the following:

- `ContentType` SHALL be `Video`
- `YouTubeUrl` SHALL be a non-empty string in the format `https://www.youtube.com/watch?v=<ID>` using a video ID from the VNCOC channel
- `Title` SHALL be a non-empty English string topically aligned to the parent course
- `Description` SHALL be a non-empty English string summarising the lesson topic
- `OrderIndex` SHALL be set 1 through 6 within each course
- `IsPreview` SHALL be `true` for the lesson with `OrderIndex == 1` and `false` for all others
- `DurationSeconds` SHALL be a positive integer approximating the video length

#### Scenario: Lesson 1 is marked as preview
- **WHEN** lessons are seeded for any course
- **THEN** the lesson with `OrderIndex == 1` has `IsPreview == true`
- **AND** all other lessons (OrderIndex 2–6) have `IsPreview == false`

#### Scenario: All lessons have a valid YouTube URL
- **WHEN** lessons are seeded
- **THEN** every lesson's `YouTubeUrl` contains a YouTube video ID from the VNCOC channel (`https://www.youtube.com/@VNCOC`)

#### Scenario: Lessons are ordered correctly within a course
- **WHEN** lessons are queried for a course ordered by `OrderIndex`
- **THEN** the result contains exactly 6 lessons numbered 1 through 6 with no gaps

## ADDED Requirements

### Requirement: Demo environments seed curated learning paths
When demo data seeding is enabled, the system SHALL seed the published English learning paths Christian Foundations, Bible & Theology, and Ministry Leadership after the existing demo courses have been seeded.

#### Scenario: Demo data seeding is enabled
- **WHEN** the application starts in Development or Staging, or with `Seed:DemoData=true`
- **THEN** the database contains the three configured published learning paths when all required seeded courses are available

#### Scenario: Demo data seeding is disabled
- **WHEN** the application starts outside Development and Staging with `Seed:DemoData=false`
- **THEN** the system does not seed demo learning paths

### Requirement: Seeded paths use existing published courses
The system SHALL compose each seeded learning path only from existing demo courses whose status is Published. The system SHALL preserve the configured section order and course order and SHALL NOT create additional courses for learning-path seeding.

#### Scenario: Required courses are available
- **WHEN** every configured course slug for a learning path resolves to an existing Published course
- **THEN** the system creates the complete path with its configured sections and ordered course references

#### Scenario: A required course is unavailable
- **WHEN** a configured course is missing or is not Published and the learning path does not already exist
- **THEN** the system skips creation of that entire learning path and does not create a partial path

### Requirement: Learning path seeding is idempotent
The system SHALL identify seeded learning paths by slug and create a path only when that slug is absent.

#### Scenario: Seeder runs repeatedly
- **WHEN** demo data seeding runs more than once against the same database
- **THEN** no duplicate learning paths, sections, or course join rows are created

#### Scenario: Administrator has edited an existing seeded path
- **WHEN** a learning path with a configured seed slug already exists
- **THEN** the seeder leaves the existing path and all of its current fields, status, sections, and courses unchanged

### Requirement: Seeded curriculum is deterministic
The system SHALL seed the following curricula:

- Christian Foundations: Start Here (Foundations of Spiritual Growth, Core Christian Doctrines, Life of Christ: The Four Gospels) and Practices for Everyday Faith (The Art of Prayer, Healthy Relationships in Community, Sharing Your Faith).
- Bible & Theology: The Biblical Story (Walking Through the Old Testament, Life of Christ: The Four Gospels), Christian Doctrine (Core Christian Doctrines, Systematic Theology Essentials), and Church Through the Ages (The Early Church: 100–500 AD, Reformation & Modern Christianity).
- Ministry Leadership: Lead Like Jesus (Servant Leadership in the Church, Growing Your Ministry Team) and Serve People Well (Healthy Relationships in Community, Sharing Your Faith, Raising Faith-Filled Families).

#### Scenario: Fresh demo database is seeded
- **WHEN** learning-path seeding runs against a fresh database containing the existing demo courses
- **THEN** each path contains exactly the configured sections and courses in the specified order

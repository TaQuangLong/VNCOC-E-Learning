## Context

`DatabaseSeeder` currently creates roles and a SuperAdmin account on every startup, then seeds demo authors, courses, and lessons in Development, Staging, or when `Seed:DemoData=true`. The Learning Paths feature is complete, but those same demo environments have no catalog data for it.

The new seed data must reuse the 15 existing English demo courses, respect the rule that published paths contain only Published courses, and remain safe on databases where administrators may have edited demo content.

## Goals / Non-Goals

**Goals:**

- Seed Christian Foundations, Bible & Theology, and Ministry Leadership.
- Create complete ordered section/course graphs using existing seeded course IDs.
- Publish newly seeded paths so they appear immediately in the public catalog.
- Reuse the existing demo-data environment/configuration gate.
- Make repeated startup safe and preserve administrator edits.
- Add focused automated tests for content and idempotency.

**Non-Goals:**

- Creating new courses, lessons, enrollments, or student progress.
- Updating or repairing an existing path with a seeded slug.
- Translating seed content or adding multilingual seed variants.
- Adding a new configuration key, API endpoint, UI, or migration.
- Guaranteeing creation when required courses have been removed, archived, or unpublished.

## Decisions

### D1: Extend the existing demo-data seed pipeline

Add learning-path seeding after `SeedCoursesAsync` and `SeedLessonsAsync` inside the existing Development/Staging/`Seed:DemoData` branch.

This preserves the current deployment controls and ensures course records exist before path course IDs are resolved. A separate startup service or command was considered but rejected because it would duplicate environment/configuration logic for a small deterministic dataset.

### D2: Identify dependencies and paths by slug

Course references are resolved from the configured course slugs, and each path is identified by a stable path slug:

- `christian-foundations`
- `bible-and-theology`
- `ministry-leadership`

Slugs are stable natural keys already protected by unique indexes. Hard-coded database IDs were rejected because IDs differ across installations.

### D3: Create only complete, publicly valid paths

Before adding a path, resolve all of its configured course slugs and verify every course is Published. If any dependency is missing or unavailable, skip that entire path.

Creating partial paths would make the demo curriculum misleading. Forcing course status back to Published was rejected because it would overwrite administrator intent. Throwing and preventing application startup was also rejected because demo-data drift should not make the service unavailable.

### D4: Preserve existing paths without reconciliation

If a path slug already exists, do nothing for that path. Do not overwrite metadata, restore Published status, replace sections, or add missing courses.

This matches the existing create-if-missing course seed behavior and protects administrator edits. Full reconciliation was rejected because startup would silently undo legitimate content management.

### D5: Persist each path graph atomically

Build each new `LearningPath` with its nested `LearningPathSection` and `LearningPathCourse` entities, then save the complete graph in one `SaveChangesAsync` operation. Use zero-based `OrderIndex` values consistently with the existing learning-path handlers.

Each path has at most six courses, so resolving the required course set once and building in memory is simpler than introducing bulk-loading infrastructure.

### D6: Fixed English curriculum

Seed the following ordered structure:

**Christian Foundations**

1. Start Here
   - Foundations of Spiritual Growth
   - Core Christian Doctrines
   - Life of Christ: The Four Gospels
2. Practices for Everyday Faith
   - The Art of Prayer
   - Healthy Relationships in Community
   - Sharing Your Faith

**Bible & Theology**

1. The Biblical Story
   - Walking Through the Old Testament
   - Life of Christ: The Four Gospels
2. Christian Doctrine
   - Core Christian Doctrines
   - Systematic Theology Essentials
3. Church Through the Ages
   - The Early Church: 100–500 AD
   - Reformation & Modern Christianity

**Ministry Leadership**

1. Lead Like Jesus
   - Servant Leadership in the Church
   - Growing Your Ministry Team
2. Serve People Well
   - Healthy Relationships in Community
   - Sharing Your Faith
   - Raising Faith-Filled Families

Course reuse across paths is intentional. No course is repeated within one path.

### D7: Extract seed logic for focused testing

Keep startup orchestration in `DatabaseSeeder.SeedAsync`, but place learning-path creation in a focused method that accepts `AppDbContext` and `CancellationToken`. Tests will use the existing test database approach to call this logic directly or through an internal test-visible entry point.

Testing only through full application startup was considered but would make idempotency and existing-path preservation cases slower and harder to isolate.

## Risks / Trade-offs

- [Admin changes a required seeded course before a missing path is created] → Skip that path rather than overriding course status or creating invalid public data.
- [A slug collides with a manually created path] → Treat the existing record as authoritative and preserve it unchanged.
- [Seed definitions drift from course seed slugs] → Cover every configured path and ordered course mapping with tests.
- [Concurrent application instances seed the same fresh database] → The unique slug index remains the final safeguard; normal deployment should run migrations/startup serially.

## Migration Plan

1. Deploy the backend code with the existing Learning Paths migration already available.
2. On startup, environments eligible for demo data run course, lesson, then learning-path seeding.
3. Existing databases receive only missing paths whose required courses are all Published.
4. Rollback requires only reverting the code; seeded records remain ordinary admin-manageable data and are not deleted automatically.

## Open Questions

None. Product decisions confirmed:

- Seed exactly three paths.
- Reuse existing courses only.
- Preserve existing paths by slug.
- Use English content.

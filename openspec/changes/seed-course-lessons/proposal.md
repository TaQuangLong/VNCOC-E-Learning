## Why

All 13 seeded courses exist but have zero lessons, making the platform unusable for demos and development testing — students see empty course pages and can't experience the learning flow. Adding 6 real video lessons per course with content from the VNCOC YouTube channel gives every course functional content immediately after a fresh database seed.

## What Changes

- Add `SeedLessonsAsync` method to `DatabaseSeeder` — 6 `Video` lessons per course, each pointing to a real YouTube URL from `https://www.youtube.com/@VNCOC`
- Lesson titles and descriptions are written in English and topically aligned to their parent course
- Seeding is idempotent: lessons are only inserted when a course has 0 lessons
- `OrderIndex` is set 1–6 per course; `DurationSeconds` is estimated per video; `IsPreview` is `true` for lesson 1 of each course
- No new migrations, entities, or endpoints required

## Capabilities

### New Capabilities

- `seed-course-lessons`: Seed 6 video lessons for each of the 13 existing seeded courses using VNCOC YouTube videos

### Modified Capabilities

<!-- No existing capability requirements are changing -->

## Impact

- **Modified file**: `backend/src/ChurchLearn.Api/Infrastructure/Persistence/DatabaseSeeder.cs`
- No API, schema, or frontend changes
- Affects Development, Staging, and any environment with `Seed:DemoData = true`

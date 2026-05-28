## Context

`DatabaseSeeder.cs` already seeds roles, a SuperAdmin user, authors, and 15 courses. All 15 courses seed with zero lessons, leaving every course page empty. The seeder runs on startup in Development/Staging or when `Seed:DemoData = true`.

YouTube video pool sourced from `https://www.youtube.com/@VNCOC` (Viet Nam Church of Christ — 83 videos, ~194 subscribers). Videos are real sermon recordings and church content.

## Goals / Non-Goals

**Goals:**
- Add `SeedLessonsAsync` to `DatabaseSeeder` that creates 6 video lessons per course
- Lessons have English titles/descriptions aligned to their course topic
- Seeding is idempotent (skip if course already has lessons)
- First lesson of each course has `IsPreview = true`

**Non-Goals:**
- No new migrations or schema changes
- No frontend changes
- Not seeding quizzes, resources, or enrollments
- Not exposing new API endpoints

## Decisions

### 1. Idempotency per course (not global flag)
Check `AnyAsync(l => l.CourseId == courseId)` for each course before inserting its lessons. This allows partial re-seeding (e.g., if a course was added later) without wiping existing data.

**Alternative considered**: A single `db.Lessons.AnyAsync()` global check — rejected because it would prevent new courses from getting lessons if any course already has them.

### 2. Single curated video pool (12 IDs, reused across courses)
Use a pool of ~12 YouTube video IDs from the VNCOC channel and assign them to lessons with English titles that match the course topic. The same video ID may appear in multiple courses.

**Video pool (ID → English description → duration seconds):**
| YouTube ID    | English Title                                    | Duration (s) |
|---------------|--------------------------------------------------|-------------|
| pQ9CKq4xl9E   | Sunday Worship Service                           | 2280        |
| yaf4uGpe3gI   | Loving Yourself Rightly                          | 3007        |
| O0MyKgMzWBk   | Transformed by Questions                         | 5173        |
| CtjdTNuzTzc   | When Waiting for Success                         | 3392        |
| DTDdX3F7rao   | My Ebenezers — Stones of Remembrance             | 2366        |
| 4l85ZMZXaG8   | Learning from Followers of Jesus                 | 3656        |
| Saw84fmOBBs   | God Is Love                                      | 3196        |
| LpcXBQw1f_s   | Jesus — Source of Our Fellowship                 | 3072        |
| NJHh5SRtF7g   | God Is Holy — Worshipping Through Holy Living    | 2969        |
| EQ84qkq2Q6s   | Journey of Grace — 31 Years of VNCOC             | 399         |
| rbxx65C2f_U   | One Man's Journey of Following Christ            | 236         |
| FOgkX2wqGuQ   | Called Back from Japan to Serve God              | 179         |

**Alternative considered**: Scraping all 83 videos and assigning unique videos per lesson — rejected due to complexity and the fact that this is demo data.

### 3. Inline lesson data as a C# anonymous-type array
Define per-course lessons as a local `var lessons` array inside `SeedLessonsAsync` alongside each course slug lookup. Clean, readable, and avoids any helper classes or configuration files.

**Alternative considered**: JSON/YAML config file — rejected as over-engineering for static seed data.

### 4. Call order in `SeedAsync`
Call `SeedLessonsAsync(db)` immediately after `SeedCoursesAsync(db)` completes so courses are guaranteed to exist in the database before lesson insertion.

## Risks / Trade-offs

| Risk | Mitigation |
|------|-----------|
| YouTube video removed by channel | IDs are from an active church channel; this is acceptable for demo seed data |
| Course slug mismatch if course seed changes | Slug lookup with `FirstOrDefaultAsync` + null guard — silently skips |
| Large number of DB inserts on first run | 15 courses × 6 lessons = 90 rows — negligible at this scale |

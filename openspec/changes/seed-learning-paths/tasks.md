## 1. Learning Path Seed Implementation

- [x] 1.1 Add deterministic seed definitions for Christian Foundations, Bible & Theology, and Ministry Leadership, including English metadata, stable slugs, ordered sections, and existing course slugs
- [x] 1.2 Implement focused learning-path seed logic that resolves required courses by slug, requires Published status, creates complete nested path graphs, and skips paths with unavailable dependencies
- [x] 1.3 Make the seed logic idempotent by skipping any path whose slug already exists without modifying its metadata, status, sections, or courses
- [x] 1.4 Invoke learning-path seeding after course and lesson seeding inside the existing Development/Staging/`Seed:DemoData` gate, using cancellation tokens for EF Core calls

## 2. Automated Tests

- [x] 2.1 Test that a fresh demo dataset creates exactly the three Published paths with the specified section and course ordering
- [x] 2.2 Test that running learning-path seeding repeatedly creates no duplicate paths, sections, or course join rows
- [x] 2.3 Test that an existing path with a seeded slug is preserved unchanged
- [x] 2.4 Test that a path is skipped without partial data when any required course is missing or not Published
- [x] 2.5 Test or otherwise verify that learning paths are not seeded when the existing demo-data environment/configuration gate is disabled

## 3. Documentation & Verification

- [x] 3.1 Update README demo-data documentation to list the three seeded learning paths and clarify the existing `Seed:DemoData` behavior
- [x] 3.2 Run the backend Release build and test suite and confirm zero failures

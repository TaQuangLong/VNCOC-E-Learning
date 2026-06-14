## Why

The demo environment already seeds published courses and lessons, but the Learning Paths catalog remains empty until an administrator creates paths manually. Seeding representative paths makes the completed feature immediately usable in local, staging, and explicitly enabled demo deployments.

## What Changes

- Seed three published English learning paths: Christian Foundations, Bible & Theology, and Ministry Leadership.
- Compose the paths exclusively from the existing seeded published courses; do not create additional courses.
- Organize each path into ordered sections with ordered course references.
- Run learning-path seeding under the existing demo-data conditions: Development, Staging, or `Seed:DemoData=true`.
- Make seeding idempotent by creating each path only when its slug is absent.
- Preserve any existing path, including administrator edits, when a seeded slug already exists.

## Capabilities

### New Capabilities

- `learning-path-demo-seeding`: Deterministic, idempotent demo seeding for curated learning paths built from existing seeded courses.

### Modified Capabilities

None.

## Impact

- **Backend**: Extend `Infrastructure/Persistence/DatabaseSeeder.cs` after course and lesson seeding.
- **Data**: Add three demo `LearningPath` records with sections and course join rows only when their slugs do not already exist.
- **Configuration**: Reuse the existing `Seed:DemoData` switch and environment checks; no new configuration keys.
- **Testing**: Add focused coverage for seed composition, publication status, repeat-run idempotency, and preservation of existing paths.
- **APIs / Frontend**: No endpoint or UI changes.

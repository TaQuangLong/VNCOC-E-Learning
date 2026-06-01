## Why

Authors are a first-class entity in the platform — every course requires one — but admins currently have no dedicated UI to list, create, edit, or delete them. Authors can only be created via the API directly, and there is no way to update or remove an author through any interface.

## What Changes

- Add `GET /api/admin/authors/:id` endpoint (fetch single author)
- Add `PUT /api/admin/authors/:id` endpoint (update name, bio, avatarUrl, userId)
- Add `DELETE /api/admin/authors/:id` endpoint (with course-assignment guard)
- Add an **Admin Authors** page at `/admin/authors` listing all authors with create/edit/delete actions
- Add an author detail/edit form page at `/admin/authors/:id/edit`
- Add a create author page at `/admin/authors/new`
- Wire the new pages into the admin sidebar/navigation

## Capabilities

### New Capabilities

- `admin-authors-crud`: Full CRUD management of Author records in the admin panel — list, create, edit, and delete with assignment guards

### Modified Capabilities

<!-- No existing spec-level requirements change -->

## Impact

- **Backend**: New handlers `GetAuthorById`, `UpdateAuthor`, `DeleteAuthor` under `Features/Courses/`; new endpoint registrations in `CoursesEndpoints.cs`
- **Frontend**: New pages `AdminAuthorsPage`, `CreateAuthorPage`, `EditAuthorPage` under `pages/admin/`; new hooks/api calls in `features/admin/` or `features/courses/api.ts`; admin router entries
- **Data**: `DELETE` must block if the author has assigned courses to prevent orphaned data

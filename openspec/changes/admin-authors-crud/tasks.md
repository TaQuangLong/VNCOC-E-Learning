## 1. Backend — GetAuthorById

- [x] 1.1 Create `Features/Courses/GetAuthorById/` folder with `GetAuthorByIdHandler.cs` returning `AuthorDetail` record (`Id`, `Name`, `Bio`, `AvatarUrl`, `UserId`, `CourseCount`)
- [x] 1.2 Query includes `db.Authors.Include(a => a.Courses)` and returns 404 via `Result.Failure(ErrorCodes.NotFound)` when author not found
- [x] 1.3 Register `GET /api/admin/authors/{id}` in `MapAdminAuthorEndpoints` in `CoursesEndpoints.cs`

## 2. Backend — UpdateAuthor

- [x] 2.1 Create `Features/Courses/UpdateAuthor/` folder with `UpdateAuthorRequest`, `UpdateAuthorValidator`, and `UpdateAuthorHandler`
- [x] 2.2 Validator rules: `Name` not empty ≤200, `Bio` ≤1000 (when present), `AvatarUrl` ≤2048 (when present)
- [x] 2.3 Handler fetches author by ID; returns `Result.Failure(ErrorCodes.NotFound)` if missing; applies field updates and saves
- [x] 2.4 Register `PUT /api/admin/authors/{id}` in `MapAdminAuthorEndpoints`

## 3. Backend — DeleteAuthor

- [x] 3.1 Create `Features/Courses/DeleteAuthor/` folder with `DeleteAuthorHandler`
- [x] 3.2 Handler checks `db.Courses.AnyAsync(c => c.AuthorId == id)` and returns `Result.Failure(ErrorCodes.Conflict)` if true
- [x] 3.3 Handler returns `Result.Failure(ErrorCodes.NotFound)` if author does not exist; otherwise deletes and returns `Result.Success()`
- [x] 3.4 Register `DELETE /api/admin/authors/{id}` in `MapAdminAuthorEndpoints` mapping to HTTP 204 on success, 409 on conflict

## 4. Frontend — API hooks

- [x] 4.1 Add `AuthorDetail` interface to `frontend/src/features/courses/types.ts` (`id`, `name`, `bio`, `avatarUrl`, `userId`, `courseCount`)
- [x] 4.2 Add `useAuthor(id: number)` query hook in `features/courses/api.ts` calling `GET /api/admin/authors/:id`
- [x] 4.3 Add `useCreateAuthor()` mutation hook calling `POST /api/admin/authors`
- [x] 4.4 Add `useUpdateAuthor()` mutation hook calling `PUT /api/admin/authors/:id`
- [x] 4.5 Add `useDeleteAuthor()` mutation hook calling `DELETE /api/admin/authors/:id`

## 5. Frontend — AdminAuthorsPage

- [x] 5.1 Create `frontend/src/pages/admin/AdminAuthorsPage.tsx` with a table showing name, bio excerpt, course count, and Edit/Delete action buttons
- [x] 5.2 Implement loading skeleton rows and empty state with a "Create Author" button
- [x] 5.3 Add an inline `AlertDialog` for delete confirmation; on confirm call `useDeleteAuthor()` and show toast
- [x] 5.4 Handle 409 conflict response: show a descriptive toast "This author has assigned courses. Reassign all courses before deleting."

## 6. Frontend — CreateAuthorPage

- [x] 6.1 Create `frontend/src/pages/admin/CreateAuthorPage.tsx` with a form (name required, bio optional, avatarUrl optional) using React Hook Form + Zod
- [x] 6.2 On submit call `useCreateAuthor()` and redirect to `/admin/authors` on success with a success toast

## 7. Frontend — EditAuthorPage

- [x] 7.1 Create `frontend/src/pages/admin/EditAuthorPage.tsx` that fetches author by `:id` using `useAuthor(id)` and pre-fills the same form fields
- [x] 7.2 On submit call `useUpdateAuthor()` and redirect to `/admin/authors` on success with a success toast
- [x] 7.3 Show 404 message if `useAuthor` returns a not-found error

## 8. Frontend — Routing & Navigation

- [x] 8.1 Add three routes in `router.tsx` under `AdminRoute`: `GET /admin/authors` → `AdminAuthorsPage`, `GET /admin/authors/new` → `CreateAuthorPage`, `GET /admin/authors/:id/edit` → `EditAuthorPage`
- [x] 8.2 Add "Authors" link to the admin sidebar/navigation (same level as Courses)

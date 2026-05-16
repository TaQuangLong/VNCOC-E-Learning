# ChurchLearn — API Map

_Update this file each sprint as new endpoints are implemented._

---

## Auth

| Method | Path                          | Auth | Roles | Sprint | Status |
|--------|-------------------------------|------|-------|--------|--------|
| POST   | /api/auth/register            | No   | Any   | 2      | ✅ done |
| POST   | /api/auth/login               | No   | Any   | 2      | ✅ done |
| POST   | /api/auth/logout              | Yes  | Any   | 2      | ✅ done |
| GET    | /api/auth/me                  | Yes  | Any   | 2      | ✅ done |
| POST   | /api/auth/refresh             | No   | Any   | 2      | ✅ done |
| POST   | /api/auth/forgot-password     | No   | Any   | 2      | ✅ done |
| POST   | /api/auth/reset-password      | No   | Any   | 2      | ✅ done |

## Admin — Users

| Method | Path                              | Auth | Roles             | Sprint | Status      |
|--------|-----------------------------------|------|-------------------|--------|-------------|
| GET    | /api/admin/users                  | Yes  | Admin, SuperAdmin | 2      | ✅ done     |
| GET    | /api/admin/users/{id}             | Yes  | Admin, SuperAdmin | 2      | ✅ done     |
| PUT    | /api/admin/users/{id}/roles       | Yes  | SuperAdmin        | 2      | ✅ done     |
| POST   | /api/admin/users/{id}/activate    | Yes  | Admin, SuperAdmin | 3      | not-started |
| POST   | /api/admin/users/{id}/deactivate  | Yes  | Admin, SuperAdmin | 3      | not-started |

## Courses

| Method | Path                                  | Auth | Roles             | Sprint | Status      |
|--------|---------------------------------------|------|-------------------|--------|-------------|
| GET    | /api/courses                          | No   | Any               | 3      | not-started |
| GET    | /api/courses/{slug}                   | No   | Any               | 3      | not-started |
| POST   | /api/admin/courses                    | Yes  | Admin, SuperAdmin | 3      | not-started |
| PUT    | /api/admin/courses/{id}               | Yes  | Admin, SuperAdmin | 3      | not-started |
| DELETE | /api/admin/courses/{id}               | Yes  | Admin, SuperAdmin | 3      | not-started |
| POST   | /api/admin/courses/{id}/publish       | Yes  | Admin, SuperAdmin | 3      | not-started |
| POST   | /api/admin/courses/{id}/unpublish     | Yes  | Admin, SuperAdmin | 3      | not-started |

## Lessons

| Method | Path                                        | Auth | Roles             | Sprint | Status      |
|--------|---------------------------------------------|------|-------------------|--------|-------------|
| GET    | /api/courses/{courseId}/lessons             | No   | Any               | 5      | not-started |
| GET    | /api/lessons/{id}                           | Yes  | Student+          | 5      | not-started |
| POST   | /api/admin/courses/{courseId}/lessons       | Yes  | Admin, SuperAdmin | 5      | not-started |
| PUT    | /api/admin/lessons/{id}                     | Yes  | Admin, SuperAdmin | 5      | not-started |
| DELETE | /api/admin/lessons/{id}                     | Yes  | Admin, SuperAdmin | 5      | not-started |
| PUT    | /api/admin/courses/{courseId}/lessons/order | Yes  | Admin, SuperAdmin | 5      | not-started |

## Enrollment & Progress

| Method | Path                                        | Auth | Roles   | Sprint | Status      |
|--------|---------------------------------------------|------|---------|--------|-------------|
| POST   | /api/courses/{courseId}/enroll              | Yes  | Student | 7      | not-started |
| GET    | /api/me/courses                             | Yes  | Student | 7      | not-started |
| GET    | /api/me/courses/{courseId}/progress         | Yes  | Student | 8      | not-started |
| POST   | /api/lessons/{lessonId}/complete            | Yes  | Student | 8      | not-started |
| POST   | /api/lessons/{lessonId}/video-progress      | Yes  | Student | 8      | not-started |

## Quizzes

| Method | Path                                        | Auth | Roles             | Sprint | Status      |
|--------|---------------------------------------------|------|-------------------|--------|-------------|
| GET    | /api/lessons/{lessonId}/quiz                | Yes  | Student+          | 9      | not-started |
| POST   | /api/admin/lessons/{lessonId}/quiz          | Yes  | Admin, SuperAdmin | 9      | not-started |
| PUT    | /api/admin/quizzes/{quizId}                 | Yes  | Admin, SuperAdmin | 9      | not-started |
| POST   | /api/quizzes/{quizId}/submit                | Yes  | Student           | 9      | not-started |
| GET    | /api/quizzes/{quizId}/attempts/me           | Yes  | Student           | 9      | not-started |

## Discussions

| Method | Path                                        | Auth | Roles             | Sprint | Status      |
|--------|---------------------------------------------|------|-------------------|--------|-------------|
| GET    | /api/lessons/{lessonId}/discussions         | Yes  | Student+          | 11     | not-started |
| POST   | /api/lessons/{lessonId}/discussions         | Yes  | Student           | 11     | not-started |
| POST   | /api/discussions/{discussionId}/reply       | Yes  | Student           | 11     | not-started |
| PUT    | /api/discussions/{discussionId}             | Yes  | Student (own)     | 11     | not-started |
| DELETE | /api/admin/discussions/{discussionId}       | Yes  | Admin, SuperAdmin | 11     | not-started |

## Reports

| Method | Path                                            | Auth | Roles             | Sprint | Status      |
|--------|--------------------------------------------------|------|-------------------|--------|-------------|
| GET    | /api/admin/reports/overview                     | Yes  | Admin, SuperAdmin | 13     | not-started |
| GET    | /api/admin/reports/courses/{courseId}/learners  | Yes  | Admin, SuperAdmin | 13     | not-started |
| GET    | /api/admin/reports/users/{userId}/progress      | Yes  | Admin, SuperAdmin | 13     | not-started |

## Health

| Method | Path            | Auth | Sprint | Status      |
|--------|-----------------|------|--------|-------------|
| GET    | /api/health     | No   | 1      | not-started |
| GET    | /api/health/db  | No   | 1      | not-started |

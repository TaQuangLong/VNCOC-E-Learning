# ChurchLearn — Entity Relationships

_Updated: Sprint 8 (progress tracking). Update this file each sprint as new entities are added._

---

## AppUser (AspNetUsers) — ✅ Sprint 2
- Extends `IdentityUser` (ASP.NET Core Identity)
- Extra fields: `DisplayName` (string), `IsActive` (bool, default true), `CreatedAt` (DateTime UTC)
- Has many Enrollments
- Has many LessonProgress records
- Has many QuizAttempts
- Has many Discussions
- Belongs to many Roles (via AspNetUserRoles)

## Role (AspNetRoles) — ✅ Sprint 2
- Values: `Student` | `Admin` | `SuperAdmin`
- Default on registration: `Student`
- Seeded at app startup via `DatabaseSeeder`

## Author — ✅ Sprint 3
- Optionally linked to one User (UserId nullable)
- Has many Courses
- Fields: Id, UserId, Name, Bio, AvatarUrl, CreatedAt, UpdatedAt

## Course — ✅ Sprint 3
- Belongs to one Author
- Has many Lessons
- Has many Enrollments
- Status: Draft | Published | Archived
- Unique index on: Slug
- Fields: Id, Title, Slug, ShortDescription, Description, ThumbnailUrl, Category, Level, Language, AuthorId, Status, CreatedAt, UpdatedAt

## Lesson — ✅ Sprint 5
- Belongs to one Course
- Has one optional Quiz
- Has many Resources
- Has many LessonProgress records
- Has many Discussions
- ContentType: Video | Text | PDF | Mixed
- Ordered by: OrderIndex within Course
- Fields: Id, CourseId, Title, Description, ContentType, YouTubeUrl, TextContent, PdfUrl, DurationSeconds, OrderIndex, IsPreview, CreatedAt, UpdatedAt
- Index on: (CourseId, OrderIndex)

## Resource — ✅ Sprint 5
- Belongs to one Lesson
- Fields: Id, LessonId, Title, Url, CreatedAt

## Enrollment — ✅ Sprint 7
- Belongs to one User
- Belongs to one Course
- Unique index on: (UserId, CourseId)
- Additional indexes on: UserId, CourseId
- Tracks: ProgressPercent, CompletedLessonsCount, TotalLessonsCount, LastAccessedLessonId
- Fields: Id, UserId, CourseId, EnrolledAt, ProgressPercent, CompletedLessonsCount, TotalLessonsCount, LastAccessedLessonId?, CompletedAt?
- TotalLessonsCount is denormalized — updated when lessons are added/removed
- No unenroll in MVP

## LessonProgress — ✅ Sprint 8
- Belongs to one User
- Belongs to one Lesson
- Unique index on: (UserId, LessonId)
- Composite index on: (UserId, CourseId)
- Tracks: IsCompleted, VideoProgressPercent, VideoWatchedSeconds
- Fields: Id, UserId, CourseId, LessonId, IsCompleted, CompletedAt?, VideoProgressPercent, VideoWatchedSeconds, LastWatchedAt?
- QuizPassed stored in QuizAttempt (not LessonProgress) — out of scope for Sprint 8

## Quiz
- Belongs to one Lesson (one-to-one)
- Has many Questions
- Has many QuizAttempts
- Fields: Id, LessonId, Title, Description, PassingScore, IsRequired, CreatedAt, UpdatedAt

## Question
- Belongs to one Quiz
- Has many AnswerOptions
- Type: SingleChoice | MultipleChoice | TrueFalse
- Fields: Id, QuizId, Text, Type, OrderIndex

## AnswerOption
- Belongs to one Question
- Fields: Id, QuestionId, Text, IsCorrect, OrderIndex

## QuizAttempt
- Belongs to one Quiz
- Belongs to one User
- Has many QuizAttemptAnswers
- Index on: (UserId, QuizId)
- Fields: Id, QuizId, UserId, Score, Passed, StartedAt, SubmittedAt

## QuizAttemptAnswer
- Belongs to one QuizAttempt
- Fields: Id, QuizAttemptId, QuestionId, SelectedAnswerOptionId, IsCorrect

## Discussion
- Belongs to one Lesson
- Belongs to one User
- Can have one parent Discussion (self-referencing, for replies)
- Index on: LessonId
- Soft delete: IsDeleted, DeletedBy, DeletedAt
- Fields: Id, LessonId, UserId, ParentDiscussionId, Content, CreatedAt, UpdatedAt, IsDeleted, DeletedBy, DeletedAt

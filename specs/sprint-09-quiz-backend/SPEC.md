# Sprint 9 — Quiz Backend

## Propose

### Goal
Admin can create quizzes with multiple-choice, single-choice, and true/false questions. Students can submit quiz attempts and receive a pass/fail result. Students can retry unlimited times. Quiz pass status integrates with lesson progress.

### Why This Sprint
Quizzes are how the platform confirms learning has happened. They must be reliable — wrong scoring or missing retry logic would break trust with church members immediately.

### Success Criteria
- Admin can create a quiz with at least one question
- Admin can add/update/delete questions and answer options
- Student can submit a quiz attempt and get a score + pass/fail
- Score is calculated correctly (correct answers / total questions × 100)
- Student can retry without limit
- Quiz passed status is stored in `LessonProgress`
- Best attempt is tracked

---

## Design

### Technical Design

**Entities:**
- `Quiz` — one quiz per lesson, has `PassingScore` (0–100), `IsRequired`
- `Question` — belongs to Quiz, has `Type` (SingleChoice/MultipleChoice/TrueFalse), `OrderIndex`
- `AnswerOption` — belongs to Question, has `IsCorrect`, `OrderIndex`
- `QuizAttempt` — one per submission, has `Score`, `Passed`, timestamps
- `QuizAttemptAnswer` — one per question per attempt, records selected option + correctness

**Scoring Logic:**
```
Score = (correctAnswerCount / totalQuestionCount) × 100
Passed = Score >= Quiz.PassingScore
```
- Multi-select: question is correct only if all correct options selected AND no incorrect options selected

**Retry:**
- No limit on retries
- Each attempt is a new row in `QuizAttempts`
- Best attempt = highest score across all attempts

**Quiz and Progress Integration:**
- On quiz passed: update `LessonProgress.QuizPassed = true`
- If `Quiz.IsRequired = true`: lesson cannot be marked complete until quiz is passed

**Security:**
- Submit endpoint never returns `IsCorrect` values from the DB in the response during quiz
- Correct answers only returned after submission in the result response

### Architecture Decisions
- Questions and options are not shuffled in MVP (Could Have)
- No time limit on quizzes in MVP
- Best attempt logic computed in handler, not stored as a separate field

### Entities Affected
- **Quiz** (new): `Id`, `LessonId`, `Title`, `Description`, `PassingScore`, `IsRequired`, `CreatedAt`, `UpdatedAt`
- **Question** (new): `Id`, `QuizId`, `Text`, `Type`, `OrderIndex`
- **AnswerOption** (new): `Id`, `QuestionId`, `Text`, `IsCorrect`, `OrderIndex`
- **QuizAttempt** (new): `Id`, `QuizId`, `UserId`, `Score`, `Passed`, `StartedAt`, `SubmittedAt`
- **QuizAttemptAnswer** (new): `Id`, `QuizAttemptId`, `QuestionId`, `SelectedAnswerOptionId`, `IsCorrect`
- EF Core migration: `AddQuizEntities`
- Indexes: `QuizAttempts(UserId, QuizId)`, `QuizAttemptAnswers(QuizAttemptId)`

### API Changes

| Method | Path                                   | Auth | Role     | New |
|--------|----------------------------------------|------|----------|-----|
| GET    | /api/lessons/{lessonId}/quiz           | Yes  | Student+ | ✓   |
| POST   | /api/admin/lessons/{lessonId}/quiz     | Yes  | Admin+   | ✓   |
| PUT    | /api/admin/quizzes/{quizId}            | Yes  | Admin+   | ✓   |
| POST   | /api/admin/quizzes/{quizId}/questions  | Yes  | Admin+   | ✓   |
| PUT    | /api/admin/questions/{questionId}      | Yes  | Admin+   | ✓   |
| DELETE | /api/admin/questions/{questionId}      | Yes  | Admin+   | ✓   |
| POST   | /api/quizzes/{quizId}/submit           | Yes  | Student+ | ✓   |
| GET    | /api/quizzes/{quizId}/attempts/me      | Yes  | Student+ | ✓   |

### Frontend Changes
None this sprint. Frontend is Sprint 10.

---

## Tasks

### Backend
- [ ] Create `QuestionType.cs` enum in `Domain/Enums/`
- [ ] Create `Quiz.cs` entity
- [ ] Create `Question.cs` entity
- [ ] Create `AnswerOption.cs` entity
- [ ] Create `QuizAttempt.cs` entity
- [ ] Create `QuizAttemptAnswer.cs` entity
- [ ] Add all DbSets to `AppDbContext`
- [ ] Configure EF Core relationships and indexes
- [ ] Add migration: `AddQuizEntities`
- [ ] Implement `GetLessonQuiz` vertical slice (questions without IsCorrect for student)
- [ ] Implement `CreateQuiz` vertical slice
- [ ] Implement `UpdateQuiz` vertical slice
- [ ] Implement `AddQuestion` vertical slice
- [ ] Implement `UpdateQuestion` vertical slice
- [ ] Implement `DeleteQuestion` vertical slice
- [ ] Implement `SubmitQuiz` vertical slice (score calculation + LessonProgress update)
- [ ] Implement `GetMyQuizAttempts` vertical slice
- [ ] Write unit tests for `CalculateQuizScore` (single choice, multi-select, all wrong, all right)
- [ ] Write unit tests for `IsRequired` + lesson completion integration
- [ ] Verify `dotnet build` — 0 errors

---

## Archive

### Status: 🔲 Not Started
### Completed: —

### What Was Built
_To be filled after sprint completes._

### Known Issues
_To be filled after sprint completes._

### Notes
_To be filled after sprint completes._

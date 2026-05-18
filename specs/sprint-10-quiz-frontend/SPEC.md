# Sprint 10 — Quiz Frontend

## Propose

### Goal
Admin can build quizzes with the quiz builder UI. Students can take quizzes inside the lesson page, see their score and pass/fail result, and retry unlimited times.

### Why This Sprint
Without the quiz UI, the quiz backend has no value. This sprint closes the loop on the core learning assessment flow.

### Success Criteria
- Admin can create and manage a quiz from the lesson edit page
- Student can see the Quiz tab on a lesson page
- Student can select answers and submit the quiz
- Pass/fail result and score are displayed
- Retry button is available
- Previous attempts are shown in a simple list
- Passed quiz updates the lesson completion state

---

## Design

### Technical Design

**Admin Quiz Builder:**
- Embedded in lesson edit page (`/admin/lessons/:id/edit`)
- Quiz section: title, description, passing score, is-required toggle
- Question list: add / edit / delete questions
- Per question: text, type selector (Single/Multi/TrueFalse), answer options list
- Answer options: text, correct toggle (radio for single/TrueFalse, checkbox for multi)

**Student Quiz Tab:**
- Tab inside `LearnPage.tsx` alongside content tabs
- Renders all questions in order
- Single choice: radio group
- Multiple choice: checkbox group
- True/False: two-button toggle
- Submit button disabled until all questions answered
- After submit: show result card (score, pass/fail message, per-question breakdown)
- Retry button re-renders the question form fresh

**Previous Attempts:**
- Simple accordion or list below the quiz
- Shows attempt date, score, pass/fail

### Architecture Decisions
- Quiz answers are not shuffled in MVP
- No timer or countdown in MVP
- Pass/fail message uses friendly church-appropriate language ("Great job!" / "Keep learning!")
- Quiz result does not reveal which answers were correct in MVP (Could Have)

### Entities Affected
None — no new entities. Consumes Sprint 9 API.

### API Changes
None — all APIs from Sprint 9.

### Frontend Changes
- `src/features/quiz/` — api.ts, types.ts, QuizBuilder.tsx, QuizPlayer.tsx, QuizResult.tsx, AttemptList.tsx
- Update `EditLessonPage.tsx` — embed QuizBuilder
- Update `LearnPage.tsx` — add Quiz tab with QuizPlayer

---

## Tasks

### Frontend
- [ ] Create `src/features/quiz/types.ts`
- [ ] Create `src/features/quiz/api.ts`
- [ ] Create `QuizBuilder.tsx` — admin quiz + question management
- [ ] Create `QuestionEditor.tsx` — single question form with answer options
- [ ] Create `QuizPlayer.tsx` — student quiz with radio/checkbox/toggle per type
- [ ] Create `QuizResult.tsx` — score card, pass/fail message, retry button
- [ ] Create `AttemptList.tsx` — previous attempts list
- [ ] Update `EditLessonPage.tsx` — add QuizBuilder section
- [ ] Update `LearnPage.tsx` — add Quiz tab
- [ ] Disable submit button until all questions are answered
- [ ] Add loading state while submitting quiz
- [ ] Add empty state for lesson with no quiz
- [ ] Verify quiz works on mobile (radio/checkbox tappable, scroll works)
- [ ] Verify `npm run build` — 0 errors

---

## Archive

### Status: ✅ Complete
### Completed: 2026-05-18

### What Was Built
- `src/features/quiz/types.ts` — all TypeScript types + Zod schemas for admin forms
- `src/features/quiz/api.ts` — TanStack Query hooks for all 8 quiz endpoints
- `src/features/quiz/QuestionEditor.tsx` — reusable question form (SingleChoice/MultipleChoice/TrueFalse)
- `src/features/quiz/QuizBuilder.tsx` — admin quiz builder embedded in EditLessonPage (create quiz + manage questions)
- `src/features/quiz/QuizPlayer.tsx` — student quiz with radio/checkbox/two-button toggle per question type
- `src/features/quiz/QuizResult.tsx` — score card, pass/fail message, per-question breakdown, retry button
- `src/features/quiz/AttemptList.tsx` — previous attempts list with best score
- Updated `EditLessonPage.tsx` — Quiz section with QuizBuilder
- Updated `LearnPage.tsx` — Content / Quiz tab bar, QuizTab component

### Known Issues
None at time of sprint close.

### Notes
- GET quiz API does not expose `isCorrect` on options. When admin edits an existing question, options are pre-filled with `isCorrect: false` — admin must re-mark correct answers.
- Quiz result does not reveal correct answers (per spec decision).

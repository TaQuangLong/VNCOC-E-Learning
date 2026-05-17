---
mode: ask
description: Review a feature against the ChurchLearn quality checklist
---

Review the selected code or feature against this checklist.
Report each item as **PASS**, **FAIL**, or **N/A** with a one-line explanation.
For every FAIL, provide a specific fix with a code example.

## Architecture
- [ ] Feature follows vertical slice folder structure (`Features/{Name}/{Action}/`)
- [ ] Handler is self-contained — no dependency on a shared service layer
- [ ] DTOs used for API request and response (no EF entities exposed)
- [ ] Endpoint registers authorization policy via `RequireAuthorization`

## Code Quality
- [ ] Methods are short and focused (under 30 lines)
- [ ] No magic strings or magic numbers (constants or enums used)
- [ ] No duplicated business logic
- [ ] Naming is explicit and readable (`MarkLessonAsCompleted` not `DoStuff`)
- [ ] Handler returns `Result<T>` — no exceptions thrown for expected domain errors
- [ ] Endpoint maps `Result<T>` errors to correct HTTP status codes (404 / 409 / 403 / 400)
- [ ] No bare `throw` or `try/catch` in handler business logic for expected error paths

## Security
- [ ] Authorization is checked server-side
- [ ] Input validation implemented with FluentValidation
- [ ] No secrets hardcoded in code
- [ ] No JWT stored in localStorage

## Database
- [ ] Indexes exist for all lookup fields (UserId, CourseId, LessonId)
- [ ] EF migration added for any schema changes
- [ ] Unique constraints in place where required

## UI (if frontend code included)
- [ ] Loading state handled
- [ ] Error state handled with user-friendly message
- [ ] Empty state handled
- [ ] Layout is responsive on mobile (Tailwind responsive prefixes used)
- [ ] No hardcoded API URLs (uses `import.meta.env.VITE_API_URL`)

## Testing
- [ ] All acceptance criteria from SPEC.md can be verified
- [ ] Edge cases handled (duplicate, unauthorized, not found)

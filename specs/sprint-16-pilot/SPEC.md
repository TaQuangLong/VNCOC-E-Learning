# Sprint 16 — Testing & Pilot Launch

## Propose

### Goal
Run a structured pilot with 5–10 real church members to validate the complete learning flow end-to-end. Collect feedback, fix critical bugs, and confirm the platform is ready for a wider church launch.

### Why This Sprint
A platform that was never tested with real users will always have surprises. A small pilot uncovers confusing UI, broken flows, and performance issues before they affect the whole congregation.

### Success Criteria
- 5–10 pilot users complete the full learning flow (register → enroll → learn → quiz → discuss)
- No data-loss or security issue found during pilot
- Admin can manage content without developer help
- Critical feedback items are addressed before wider launch
- Code passes a final Clean Code / SOLID / KISS review

---

## Design

### Technical Design

**Test Coverage Goals:**

| Area | Approach |
|------|----------|
| Auth logic | Unit tests (register, login, token refresh, roles) |
| Course slug uniqueness | Unit test |
| Enrollment duplicate prevention | Unit test |
| Progress calculation | Unit test |
| Quiz scoring (all types) | Unit test |
| Discussion soft delete | Unit test |
| Full user flow | Manual test checklist |
| Responsive layout | Browser DevTools at 360px, 768px, 1024px |
| Backup restore | Manual test on local machine |

**Manual Test Checklist:**
- [ ] Register new user on mobile
- [ ] Login as student
- [ ] Browse course catalog
- [ ] View course detail
- [ ] Enroll in course
- [ ] Watch YouTube lesson
- [ ] Read text lesson
- [ ] Open PDF link
- [ ] Mark lesson as completed
- [ ] Check progress bar updates
- [ ] Take quiz (pass and fail scenarios)
- [ ] Retry quiz
- [ ] Post discussion question
- [ ] Reply to discussion
- [ ] Login as admin
- [ ] Create course, add lessons, publish
- [ ] View reports
- [ ] Moderate discussion
- [ ] Test forgot/reset password flow
- [ ] Test backup and restore

**Pilot Setup:**
- Create 1–2 real courses with meaningful church content
- Pre-create 3 study guide questions per lesson
- Create a test quiz per course
- Invite pilot group via email with registration link

**Feedback Collection:**
- Simple Google Form with 5 questions (what worked, what confused you, what is missing, overall rating)
- In-person walkthrough with 2–3 pilot users to watch them use the app

**Code Review (final):**
- Run `.vscode/prompts/review-feature.prompt.md` against each major feature
- Check OWASP Top 10 against auth and admin endpoints
- Verify no `console.log` left in production frontend build
- Verify no hardcoded secrets anywhere

### Architecture Decisions
- No feature flags or staged rollout — full pilot with all features
- Bug fixes during pilot go through `fix/*` branches and fast-merge to main with CI

### Entities Affected
None — no new entities.

### API Changes
None planned. Bug fixes only.

---

## Tasks

### Testing
- [ ] Write unit tests for auth: register, login, token issuance, refresh
- [ ] Write unit tests for enrollment duplicate check
- [ ] Write unit tests for progress calculation (ProgressPercent formula)
- [ ] Write unit tests for quiz scoring (single choice, multi, TrueFalse)
- [ ] Write unit tests for discussion soft delete
- [ ] Run full manual test checklist (all items above)
- [ ] Test responsive layout at 360px, 768px, 1024px, 1280px in browser DevTools
- [ ] Test backup restore on local machine

### Pilot
- [ ] Create 1–2 real church courses with real content
- [ ] Create quizzes for each course
- [ ] Invite 5–10 pilot users by email
- [ ] Monitor for errors (Serilog logs on EC2)
- [ ] Collect feedback via form or interview

### Bug Fixes
- [ ] Address all feedback with priority: Critical > High > Medium
- [ ] Re-run manual test checklist after critical fixes

### Code Review
- [ ] Run review-feature prompt against each feature
- [ ] Remove all debug/console.log
- [ ] Verify no secrets in code or git history
- [ ] Final `dotnet build` — 0 warnings, 0 errors
- [ ] Final `npm run build` — 0 errors

### Launch Readiness
- [ ] Update README with setup instructions for future developers
- [ ] Archive this SPEC with pilot results

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

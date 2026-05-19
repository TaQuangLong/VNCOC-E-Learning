---
name: commit-and-push
description: >
  Use when asked to commit and push code changes. Reads all staged and unstaged
  changes, generates a precise conventional-commit message aligned with the
  diff, stages everything, commits, and pushes to the current branch.
  Triggers on phrases like "commit and push", "commit changes", "push code",
  "commit my work", "save and push", "commit then push".
argument-hint: 'Optional: extra context to include in the commit message'
---

# Skill: Commit and Push

Reads every pending change, writes a precise conventional-commit message, then
commits and pushes to the current remote branch — all in one pass.

---

## Procedure

### Step 1 — Read the working tree

Run these commands to understand what changed:

```bash
git status --short
git diff HEAD
```

If there are staged changes already, also run:
```bash
git diff --cached
```

### Step 2 — Analyse the diff

Read the output carefully. Group the changes by:

| Group | How to identify |
|-------|-----------------|
| **feat** | New files, new endpoints, new components, new DB migrations |
| **fix** | Bug fixes, corrected logic, error-handling improvements |
| **refactor** | Code moved/renamed with no behaviour change |
| **test** | Changes only inside `tests/` or `*.Tests/` |
| **chore** | Config, tooling, CI, dependency bumps |
| **docs** | Markdown, comments only |
| **style** | Formatting, Tailwind classes, no logic change |

Pick the **primary** type from the table above.

### Step 3 — Write the commit message

Follow the **Conventional Commits** spec:

```
<type>(<optional-scope>): <imperative summary, ≤72 chars>

<optional body — what changed and why, wrapped at 72 chars>
```

Rules:
- Summary is lowercase, imperative mood ("add", "fix", "remove" — not "added")
- Scope is the feature folder name (e.g. `auth`, `courses`, `lessons`, `quiz`)
- Body is optional; include it when the diff spans multiple areas or needs context
- If breaking change: append `BREAKING CHANGE: <description>` in the footer

Examples:
```
feat(auth): add refresh-token rotation on every login

fix(courses): return 404 when course slug does not exist

chore: bump .NET SDK to 10.0.200

test(enrollment): add handler unit tests for duplicate enrollment
```

### Step 4 — Stage all changes

```bash
git add -A
```

### Step 5 — Commit

```bash
git commit -m "<generated message>"
```

If the body is non-trivial, use:
```bash
git commit -m "<summary>" -m "<body paragraph>"
```

### Step 6 — Push to current branch

```bash
git push origin HEAD
```

If the upstream is not set yet:
```bash
git push --set-upstream origin $(git branch --show-current)
```

---

## Safety Rules

- **Never force-push** (`--force` / `--force-with-lease`) without explicit user approval.
- **Never amend a published commit** without explicit user approval.
- If `git push` is rejected (non-fast-forward), stop and tell the user — do not rebase automatically.
- If there are **merge conflicts**, stop and report them — do not resolve automatically.

---

## Scope Reference (ChurchLearn)

| Changed path prefix | Scope |
|---------------------|-------|
| `backend/src/…/Features/Auth/` | `auth` |
| `backend/src/…/Features/Courses/` | `courses` |
| `backend/src/…/Features/Lessons/` | `lessons` |
| `backend/src/…/Features/Enrollments/` | `enrollment` |
| `backend/src/…/Features/Progress/` | `progress` |
| `backend/src/…/Features/Quizzes/` | `quiz` |
| `backend/src/…/Features/Discussions/` | `discussion` |
| `backend/src/…/Migrations/` | `db` |
| `frontend/src/features/auth/` | `auth` |
| `frontend/src/features/courses/` | `courses` |
| `frontend/src/features/lessons/` | `lessons` |
| `frontend/src/…/components/` | `ui` |
| `specs/` | `specs` |
| `.github/` | `ci` or `chore` |
| Root config files | *(no scope)* |
| `tests/` or `*.Tests/` | `test` |

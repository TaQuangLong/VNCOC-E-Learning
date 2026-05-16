# Sprint 12 — Discussion Frontend

## Propose

### Goal
Students can read, post, reply to, edit, and delete discussion posts inside each lesson. Admins can moderate content. The discussion tab is part of the lesson learning page.

### Why This Sprint
Discussion is the community layer of the platform. Church members value being able to ask questions and share reflections. This sprint makes the learning experience interactive.

### Success Criteria
- Discussion tab appears on the lesson page
- Student can post a new question/comment
- Student can reply to an existing post
- Student can edit their own post inline
- Student can delete their own post (soft delete)
- Admin sees a delete/moderate button on any post
- Deleted posts show a placeholder message
- Discussion list refreshes after posting without full page reload
- Empty state shown when no discussions exist yet

---

## Design

### Technical Design

**Discussion Tab Layout:**
```
[ Post new question / comment form ]
---
[ Post 1 ]                    [ Edit | Delete (if own) ]
  [ Reply 1 ]
  [ Reply 2 ]
  [ Reply button ]
---
[ Post 2 ]
  ...
```

**Interactions:**
- Post form: multi-line textarea (max 2,000 chars) + Submit button
- Reply form: collapsible — click "Reply" to open, submit closes it
- Edit: inline — replaces post content with textarea, Save / Cancel
- Delete: confirms with shadcn/ui AlertDialog before calling API
- Admin: sees red "Remove" button on any post

**Loading/Empty States:**
- Loading: skeleton for post list
- Empty: "Be the first to ask a question in this lesson."
- Error: "Could not load discussions. Please try again."

**Pagination:**
- Load top-level posts 10 at a time — "Load More" button
- Replies loaded on demand when user clicks "See Replies (N)"

### Architecture Decisions
- No real-time updates (WebSocket) in MVP — simple TanStack Query refetch on mutation
- Reply form not shown for deleted posts
- Rich text editor not required — plain textarea is enough for MVP

### Entities Affected
None — no new entities. Consumes Sprint 11 API.

### API Changes
None — all APIs from Sprint 11.

### Frontend Changes
- `src/features/discussion/` — api.ts, types.ts, DiscussionList.tsx, DiscussionPost.tsx, DiscussionForm.tsx, ReplyList.tsx
- Update `LearnPage.tsx` — add Discussion tab

---

## Tasks

### Frontend
- [ ] Create `src/features/discussion/types.ts`
- [ ] Create `src/features/discussion/api.ts`
- [ ] Create `DiscussionForm.tsx` — new post / reply form with char counter
- [ ] Create `DiscussionPost.tsx` — single post with edit, delete, reply controls
- [ ] Create `ReplyList.tsx` — collapsible reply list per post
- [ ] Create `DiscussionList.tsx` — paginated list of top-level posts
- [ ] Update `LearnPage.tsx` — add Discussion tab
- [ ] Add inline edit functionality (replace text with textarea, save/cancel)
- [ ] Add delete confirmation dialog using shadcn/ui AlertDialog
- [ ] Add admin Remove button visibility based on user role
- [ ] Add "Load More" pagination for posts
- [ ] Add loading skeleton for discussion list
- [ ] Add empty state message
- [ ] Verify discussion works on mobile (textarea usable, buttons tappable)
- [ ] Verify `npm run build` — 0 errors

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

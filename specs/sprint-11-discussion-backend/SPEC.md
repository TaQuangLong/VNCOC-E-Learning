# Sprint 11 — Discussion Backend

## Propose

### Goal
Students can post questions or comments in each lesson's discussion section. They can reply to others' posts, edit their own posts, and soft-delete their own posts. Admins can moderate and remove inappropriate content.

### Why This Sprint
Discussion transforms isolated learning into community learning — critical for a church context where members want to ask questions and encourage each other. The backend must exist before the UI is built.

### Success Criteria
- Student can post a discussion question on a lesson
- Student can reply to an existing discussion
- Student can edit their own post
- Student can soft-delete their own post
- Admin can hard-delete any post for moderation
- Deleted posts are soft-deleted (content replaced with deletion message)
- Discussions are scoped to the correct lesson
- Pagination or cursor-based load-more works

---

## Design

### Technical Design

**Entities:**
- `Discussion` — self-referencing for replies (`ParentDiscussionId?`)
- Soft delete: `IsDeleted`, `DeletedBy`, `DeletedAt` — content set to `null` or "[deleted]" on display
- No nested replies beyond one level in MVP (replies to replies not supported)

**Threading Model:**
- Top-level posts: `ParentDiscussionId = null`
- Replies: `ParentDiscussionId = parentPostId`
- Max nesting: 1 level (a reply cannot be replied to)

**Pagination:**
- `GET /api/lessons/{lessonId}/discussions` — returns top-level posts with embedded reply counts
- `GET /api/discussions/{discussionId}/replies` — returns replies to a specific post
- Simple page/pageSize pagination

**Authorization:**
- Edit own post: `UserId == currentUser.Id`
- Delete own post: `UserId == currentUser.Id` → soft delete
- Admin delete: admin can soft-delete or hard-delete any post

**Content Validation:**
- Max 2,000 characters per post
- Empty content rejected

### Architecture Decisions
- Only one level of nesting for MVP — avoids complex recursive rendering
- Soft delete preserves thread structure — "[This post has been removed]" placeholder shown
- No like/upvote in MVP

### Entities Affected
- **Discussion** (new): `Id`, `LessonId`, `UserId`, `ParentDiscussionId?`, `Content`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedBy?`, `DeletedAt?`
- EF Core migration: `AddDiscussions`
- Indexes: `Discussions(LessonId)`, `Discussions(ParentDiscussionId)`, `Discussions(UserId)`

### API Changes

| Method | Path                                        | Auth | Role      | New |
|--------|---------------------------------------------|------|-----------|-----|
| GET    | /api/lessons/{lessonId}/discussions         | Yes  | Student+  | ✓   |
| POST   | /api/lessons/{lessonId}/discussions         | Yes  | Student+  | ✓   |
| GET    | /api/discussions/{discussionId}/replies     | Yes  | Student+  | ✓   |
| POST   | /api/discussions/{discussionId}/reply       | Yes  | Student+  | ✓   |
| PUT    | /api/discussions/{discussionId}             | Yes  | Own user  | ✓   |
| DELETE | /api/discussions/{discussionId}             | Yes  | Own user  | ✓   |
| DELETE | /api/admin/discussions/{discussionId}       | Yes  | Admin+    | ✓   |

### Frontend Changes
None this sprint. Frontend is Sprint 12.

---

## Tasks

### Backend
- [ ] Create `Discussion.cs` entity in `Domain/Entities/`
- [ ] Add `Discussions` DbSet to `AppDbContext`
- [ ] Configure self-referencing relationship + indexes
- [ ] Add migration: `AddDiscussions`
- [ ] Implement `GetLessonDiscussions` vertical slice (top-level with reply count)
- [ ] Implement `GetDiscussionReplies` vertical slice
- [ ] Implement `CreateDiscussion` vertical slice
- [ ] Implement `CreateReply` vertical slice (enforce one-level max)
- [ ] Implement `UpdateDiscussion` vertical slice (own post only)
- [ ] Implement `DeleteDiscussion` vertical slice (own post → soft delete)
- [ ] Implement `AdminDeleteDiscussion` vertical slice (any post → soft delete)
- [ ] Add content length validation (max 2,000 chars)
- [ ] Write unit tests for soft delete behavior
- [ ] Write unit tests for one-level nesting enforcement
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

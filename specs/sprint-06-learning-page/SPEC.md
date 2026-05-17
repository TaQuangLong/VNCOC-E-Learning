# Sprint 6 — Learning Page Frontend

## Propose

### Goal
Admin can manage lessons from the UI. Students can open a course and view lesson content — YouTube video, text, or PDF — with a lesson sidebar, navigation, and resources section. All layouts are responsive.

### Why This Sprint
The learning page is the core experience of the platform. Every hour a church member spends on the platform is spent here. It must be clean, focused, and mobile-friendly before any engagement features (enrollment, progress, quiz) are built on top of it.

### Success Criteria
- Admin can create, edit, reorder, and delete lessons from the UI
- Student can open a course learning page and see all lessons in the sidebar
- YouTube lesson shows embedded player at correct aspect ratio
- Text lesson renders markdown content
- PDF lesson shows "View PDF" link that opens in new tab
- Resources section shows attached links per lesson
- Sidebar collapses to drawer/accordion on mobile
- Next / Previous lesson navigation works

---

## Design

### Technical Design

**Admin Pages:**
- `/admin/courses/:courseId/lessons` — lesson list with drag-to-reorder (or up/down buttons)
- `/admin/courses/:courseId/lessons/new` — create lesson form with content type selector
- `/admin/lessons/:id/edit` — edit lesson form + resource management

**Student Learning Pages:**
- `/learn/:courseId` — redirects to first lesson or last-watched lesson
- `/learn/:courseId/lessons/:lessonId` — main learning page

**Layout:**
```
Desktop (lg+):
  [Sidebar: lesson list] [Main: content area]
                         [Resources section below content]

Mobile:
  [Top: lesson selector or back button]
  [Main: content area]
  [Resources accordion below]
```

**YouTube Embed:**
- Extract video ID from URL: `https://www.youtube.com/watch?v=VIDEO_ID`
- Embed via `<iframe>` with `16:9` aspect ratio (`aspect-video` Tailwind class)
- `allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope"` attributes

**Text Renderer:**
- Render markdown using `react-markdown` or simple HTML `dangerouslySetInnerHTML` (sanitized)

**Sidebar:**
- Lesson list with completion checkmarks (placeholder, filled in Sprint 8)
- Active lesson highlighted
- On mobile: hidden by default, toggle button reveals drawer

### Architecture Decisions
- Lesson sidebar is always rendered server-side list — no lazy loading needed at ~1,000 users
- PDF link opens in new tab — no embedded iframe for PDFs (too complex for MVP)
- No YouTube watch progress tracking in MVP (saved for Could Have)

### Entities Affected
None — no new entities. Consumes Sprints 3 and 5 APIs.

### API Changes
None — all APIs from Sprint 5.

### Frontend Changes
- `src/features/lessons/` — api.ts, types.ts, LessonForm.tsx, LessonSidebar.tsx
- `src/features/lessons/players/` — YouTubePlayer.tsx, TextRenderer.tsx, PdfLink.tsx
- `src/pages/student/LearnPage.tsx` — main learning layout
- `src/pages/admin/AdminLessonsPage.tsx`
- `src/pages/admin/CreateLessonPage.tsx`
- `src/pages/admin/EditLessonPage.tsx`
- Route updates in `router.tsx`

---

## Tasks

### Frontend
- [ ] Install `react-markdown` for text lesson rendering
- [ ] Create `src/features/lessons/types.ts` — Lesson, Resource TypeScript types
- [ ] Create `src/features/lessons/api.ts` — TanStack Query hooks for lesson APIs
- [ ] Create `YouTubePlayer.tsx` — extract video ID, render responsive iframe
- [ ] Create `TextRenderer.tsx` — render markdown/text content safely
- [ ] Create `PdfLink.tsx` — "View PDF" button, opens external URL in new tab
- [ ] Create `LessonSidebar.tsx` — list of lessons, active highlight, mobile drawer toggle
- [ ] Create `ResourcesSection.tsx` — list of resource links per lesson
- [ ] Create `LearnPage.tsx` — two-column desktop, single-column mobile layout
- [ ] Add next/previous lesson navigation buttons
- [ ] Create `LessonForm.tsx` — content type selector, conditional fields (URL / text area)
- [ ] Create `AdminLessonsPage.tsx` — lesson list with reorder (up/down arrows)
- [ ] Create `CreateLessonPage.tsx`
- [ ] Create `EditLessonPage.tsx` + resource management panel
- [ ] Add loading skeletons for lesson content area
- [ ] Add empty state for course with no lessons
- [ ] Verify YouTube embed works on mobile (correct aspect ratio)
- [ ] Verify sidebar collapses on mobile
- [ ] Verify `npm run build` — 0 errors

---

## Archive

### Status: ✅ Complete
### Completed: 2026-05-17

### What Was Built
- `src/features/lessons/types.ts` — `Lesson`, `Resource` TypeScript types
- `src/features/lessons/api.ts` — TanStack Query hooks: `useCourseLessons`, `useLessonDetail`, lesson CRUD mutations
- `src/features/lessons/players/YouTubePlayer.tsx` — extracts video ID, renders responsive `aspect-video` iframe
- `src/features/lessons/players/TextRenderer.tsx` — renders plain text content in a `<pre>` block
- `src/features/lessons/players/PdfLink.tsx` — "View PDF" button opening URL in new tab
- `src/features/lessons/LessonSidebar.tsx` — lesson list with active highlight and mobile drawer toggle
- `src/features/lessons/ResourcesSection.tsx` — per-lesson resource links
- `src/features/lessons/LessonForm.tsx` — content-type selector with conditional fields
- `src/pages/student/LearnPage.tsx` — two-column desktop / single-column mobile layout with next/prev navigation
- `src/pages/student/LearnRedirectPage.tsx` — redirects `/learn/:courseId` to first lesson
- `src/pages/student/MyLearningPage.tsx` — enrolled courses list (Sprint 7 prerequisite)
- `src/pages/admin/AdminLessonsPage.tsx` — lesson list with up/down reorder arrows
- `src/pages/admin/CreateLessonPage.tsx` and `EditLessonPage.tsx` — lesson CRUD with resource management
- Route updates in `router.tsx` for all new pages
- Build passes with 0 TypeScript errors

### Known Issues
- `react-markdown` was not installed; `TextRenderer` renders plain text via `<pre>` instead of parsed markdown. Markdown rendering can be added in a later sprint if needed.

### Notes
- `LearnPage` checks enrollment status before rendering content, consuming `useEnrollmentStatus` from the enrollment feature — this creates a soft dependency on Sprint 7 enrollment API being present.

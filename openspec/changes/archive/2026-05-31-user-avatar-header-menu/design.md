## Context

The app currently has no global header component — each page renders its own top section independently. The `LearnPage` has a custom `<header>` element; other pages use `<div className="mb-8">` titles. There is no shared navigation shell.

Auth state is provided by `AuthContext` (via `useAuth`), which exposes `user` (`{ userId, email, displayName, roles }`) and a `logout()` async function. The only shadcn/ui component currently installed is `button.tsx`.

This change introduces a standalone `UserAvatarMenu` component placed inside page headers (or a new shared `AppHeader`). It does not require a full layout shell refactor.

## Goals / Non-Goals

**Goals:**
- Render a circular avatar with 1–2 letter initials in the top-right corner of authenticated pages
- Initials derived from `displayName` (first letters of each word, up to 2) or email (first 2 chars before `@`)
- Clicking the avatar opens a dropdown showing: user email (read-only), "My Learning" link (`/my-learning`), "Logout" action
- Dropdown closes on outside click (standard Radix behavior)
- Works on mobile (touch-friendly tap target ≥ 44px)

**Non-Goals:**
- Full layout shell / persistent sidebar (separate concern)
- Profile editing from the dropdown
- Admin-specific nav items in the dropdown
- Backend changes

## Decisions

### 1. New component: `UserAvatarMenu`

A self-contained component at `src/components/layout/UserAvatarMenu.tsx`.  
Reads `user` from `useAuth()`, derives initials locally, renders the avatar button + dropdown.

**Why not inline in every page?** Centralising avoids duplication — initials logic, dropdown markup, and logout wiring exist in one place.

### 2. Use shadcn/ui `DropdownMenu` (install via CLI)

Radix-based, keyboard-accessible, closes on outside click. Already aligned with the project's shadcn/ui setup.

**Alternative considered: custom Popover with `useState` + `useEffect` click-outside listener.** Rejected — more boilerplate, harder to keep accessible.

### 3. Placement strategy: inject into existing page headers

Rather than creating a full `AppHeader` wrapper (which would require refactoring all page layouts), `UserAvatarMenu` is placed inside each page's existing top bar as `<UserAvatarMenu className="ml-auto" />`. This is lower-risk and scoped.

Pages that need the avatar added: `CoursesPage`, `StudentDashboardPage`, `MyLearningPage`, `AdminDashboardPage`, `AdminCoursesPage`, `LearnPage` (already has a header element).

**Alternative: global AppHeader shell wrapping all routes.** Deferred — higher blast radius and would touch the router. A dedicated layout sprint is the right place for that.

### 4. Initials derivation

```
displayName "John Doe"  → "JD"
displayName "Mary"      → "M"
email "ab@church.org"   → "AB"
```

Computed as: split `displayName` on whitespace, take first char of first two words, uppercase. Fallback to first two chars of email local-part if `displayName` is absent.

## Risks / Trade-offs

- **Placement inconsistency** — injecting into each page header individually means different pages may look slightly different until all are updated. → Mitigation: update all authenticated pages in the same PR.
- **No avatar image** — initials only for now. Users may expect a photo upload later. → Acceptable for v1; avatar image support is a separate capability.
- **Dropdown not shown on public/unauthenticated pages** — component renders `null` when `user` is null. → Expected behaviour; public pages (login, register, course catalog) don't show the avatar.

## Open Questions

- Should the avatar appear on the public `/courses` page for logged-in users? (Currently no; easy to add later by rendering `UserAvatarMenu` conditionally only when `user !== null`.)

## Why

Currently the app header has no user identity indicator. Members have no quick way to see who is logged in or access account actions (profile, learning, logout) without navigating away — reducing the sense of personalisation and discoverability of key actions.

## What Changes

- Add a circular avatar component in the top-right corner of the global app header
- Avatar defaults to a two-letter acronym derived from the user's display name or email (e.g. "JD" for John Doe, "AB" for ab@church.org)
- Clicking or hovering the avatar opens a dropdown menu containing:
  - User email (read-only, displayed at the top)
  - "My Learning" link — navigates to the student dashboard / enrolled courses
  - "Logout" action — signs the user out and redirects to login
- Dropdown closes on outside click / focus loss

## Capabilities

### New Capabilities

- `user-avatar-header-menu`: Circular avatar in the app header with a dropdown that shows user email, My Learning link, and Logout action

### Modified Capabilities

<!-- No existing spec-level requirements are changing -->

## Impact

- **Frontend only** — no backend changes required
- `src/components/layout/` — global header/layout component updated
- `src/features/auth/` — logout action reused from existing auth feature
- `useAuth` hook — consumed to read current user email / display name
- shadcn/ui `DropdownMenu` component added (or reused if already present)
- `src/app/router.tsx` — "My Learning" route path referenced in the dropdown

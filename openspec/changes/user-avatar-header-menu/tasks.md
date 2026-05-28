## 1. Dependencies

- [x] 1.1 Install shadcn/ui `DropdownMenu` component via CLI (`npx shadcn@latest add dropdown-menu`)
- [x] 1.2 Verify `dropdown-menu.tsx` is present in `src/components/ui/`

## 2. UserAvatarMenu Component

- [x] 2.1 Create `src/components/layout/UserAvatarMenu.tsx`
- [x] 2.2 Implement `getInitials(displayName?: string, email?: string): string` — derives 1–2 uppercase initials per spec
- [x] 2.3 Render circular avatar button with initials (Tailwind: `rounded-full`, fixed size, bg colour)
- [x] 2.4 Wrap with shadcn `DropdownMenu` / `DropdownMenuTrigger`
- [x] 2.5 Add `DropdownMenuContent` with three items: email label (non-interactive), "My Learning" link, "Logout" button
- [x] 2.6 Wire "My Learning" item to navigate to `/my-learning` via `useNavigate`
- [x] 2.7 Wire "Logout" item to call `logout()` from `useAuth` then navigate to `/login`
- [x] 2.8 Return `null` when `user` is `null` (unauthenticated guard)

## 3. Integrate into Page Headers

- [x] 3.1 Add `<UserAvatarMenu />` to the top-right of `CoursesPage` header
- [x] 3.2 Add `<UserAvatarMenu />` to `StudentDashboardPage` header
- [x] 3.3 Add `<UserAvatarMenu />` to `MyLearningPage` header
- [x] 3.4 Add `<UserAvatarMenu />` to `AdminDashboardPage` header
- [x] 3.5 Add `<UserAvatarMenu />` to `AdminCoursesPage` header
- [x] 3.6 Add `<UserAvatarMenu />` to `LearnPage` header (existing `<header>` element)

## 4. Verification

- [x] 4.1 Confirm avatar renders with correct initials for a logged-in user
- [x] 4.2 Confirm dropdown opens/closes on click and outside-click
- [x] 4.3 Confirm "My Learning" navigates to `/my-learning`
- [x] 4.4 Confirm "Logout" clears session and redirects to `/login`
- [x] 4.5 Confirm avatar is absent on unauthenticated pages (login, register)
- [x] 4.6 Confirm no TypeScript errors (`tsc --noEmit`)

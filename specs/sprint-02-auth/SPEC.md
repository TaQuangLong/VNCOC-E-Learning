# Sprint 2 — Authentication & Roles

## Propose

### Goal
Users can register with email, login with JWT, persist session via httpOnly refresh cookie, access role-protected routes, and reset a forgotten password by email.

### Why This Sprint
Auth is the gate to every other feature. No enrollment, progress, quiz, or discussion can function without knowing who the user is. Building auth first also forces the team to get security patterns right before any business logic is written.

### Success Criteria
- New user registers and receives Student role automatically
- User logs in and receives a JWT access token in memory + httpOnly refresh cookie
- `/api/auth/me` returns the authenticated user's info
- Student cannot call admin endpoints (403)
- SuperAdmin seed account exists
- User can request and complete a password reset via email
- React login/register pages work and store token in memory only (never localStorage)

---

## Design

### Technical Design

**Backend — Identity & JWT:**
- `AppDbContext : IdentityDbContext<AppUser>` — EF Core + Identity
- `AppUser` extends `IdentityUser` with `DisplayName`, `IsActive`, `CreatedAt`
- Roles seeded at startup: `Student`, `Admin`, `SuperAdmin`
- `SuperAdmin` seed account from config/env
- JWT: `Microsoft.AspNetCore.Authentication.JwtBearer`
- Access token: short-lived (60 min), returned in JSON response body
- Refresh token: long-lived (7 days), stored as httpOnly Secure SameSite=Strict cookie
- `ICurrentUser` interface + `CurrentUserService` — reads `ClaimsPrincipal` from HTTP context

**Backend — Rate Limiting:**
- ASP.NET Core built-in `RateLimiter` middleware
- Fixed window: 10 requests per minute per IP on `/api/auth/login` and `/api/auth/register`

**Backend — Email:**
- `IEmailSender` interface in `Infrastructure/Email/`
- `SmtpEmailSender` implementation — config from `Email:*` settings
- `NoOpEmailSender` for local development (logs to console)

**Backend — Password Reset:**
- `POST /api/auth/forgot-password` — generates reset token via `UserManager`, sends email
- `POST /api/auth/reset-password` — validates token, sets new password

**Frontend — Auth Flow:**
- Access token stored in React context/state (never localStorage)
- Refresh token is httpOnly cookie — transparent to JS
- On page reload: call `/api/auth/me` with cookie to silently restore session
- `useAuth` hook — exposes `user`, `login`, `logout`, `register`
- Protected route wrapper — redirects unauthenticated users to `/login`
- Admin route guard — redirects non-admin users to `/dashboard`

### Architecture Decisions
- Refresh token in httpOnly cookie prevents XSS token theft — non-negotiable security rule
- Access token in memory means it is lost on page reload — silent refresh via `/api/auth/me` on app load
- `ICurrentUser` abstracts `HttpContext` so handlers do not import `Microsoft.AspNetCore.Http` directly

### Entities Affected
- **AppUser** (new): `Id`, `Email`, `UserName`, `DisplayName`, `IsActive`, `CreatedAt`
- **AspNetRoles** (seeded): Student, Admin, SuperAdmin
- EF Core migration: `InitialIdentity`

### API Changes

| Method | Path                         | Auth | Role    | New |
|--------|------------------------------|------|---------|-----|
| POST   | /api/auth/register           | No   | Any     | ✓   |
| POST   | /api/auth/login              | No   | Any     | ✓   |
| POST   | /api/auth/logout             | Yes  | Any     | ✓   |
| GET    | /api/auth/me                 | Yes  | Any     | ✓   |
| POST   | /api/auth/refresh            | No   | Any     | ✓   |
| POST   | /api/auth/forgot-password    | No   | Any     | ✓   |
| POST   | /api/auth/reset-password     | No   | Any     | ✓   |
| GET    | /api/admin/users             | Yes  | Admin+  | ✓   |
| GET    | /api/admin/users/{id}        | Yes  | Admin+  | ✓   |
| PUT    | /api/admin/users/{id}/roles  | Yes  | SuperAdmin | ✓ |

### Frontend Changes
- `src/features/auth/` — api.ts, types.ts, AuthContext.tsx, useAuth.ts
- `src/pages/public/LoginPage.tsx`
- `src/pages/public/RegisterPage.tsx`
- `src/pages/public/ForgotPasswordPage.tsx`
- `src/pages/public/ResetPasswordPage.tsx`
- `src/components/layout/ProtectedRoute.tsx`
- `src/components/layout/AdminRoute.tsx`
- Routes registered in `router.tsx`

### CI/CD
- GitHub Actions backend build + test workflow
- GitHub Actions frontend build workflow
- Workflows trigger on PR and push to `main`

---

## Tasks

### Backend
- [ ] Create `AppUser.cs` in `Domain/Entities/`
- [ ] Create `AppDbContext.cs` in `Infrastructure/Persistence/` extending `IdentityDbContext<AppUser>`
- [ ] Register Identity + EF Core in `Program.cs`
- [ ] Configure JWT authentication in `Program.cs`
- [ ] Create `ICurrentUser` interface + `CurrentUserService`
- [ ] Add rate limiting middleware for auth endpoints
- [ ] Seed roles (Student, Admin, SuperAdmin) and SuperAdmin user on startup
- [ ] Create `IEmailSender` + `NoOpEmailSender` + `SmtpEmailSender`
- [ ] Add first EF Core migration: `InitialIdentity`
- [ ] Implement `Register` vertical slice (Features/Auth/Register/)
- [ ] Implement `Login` vertical slice (Features/Auth/Login/)
- [ ] Implement `Logout` vertical slice (Features/Auth/Logout/)
- [ ] Implement `GetCurrentUser` vertical slice (Features/Auth/GetCurrentUser/)
- [ ] Implement `RefreshToken` vertical slice (Features/Auth/RefreshToken/)
- [ ] Implement `ForgotPassword` vertical slice (Features/Auth/ForgotPassword/)
- [ ] Implement `ResetPassword` vertical slice (Features/Auth/ResetPassword/)
- [ ] Implement `GetUsers` admin vertical slice (Features/Users/GetUsers/)
- [ ] Implement `GetUser` admin vertical slice (Features/Users/GetUser/)
- [ ] Implement `UpdateUserRoles` admin vertical slice (Features/Users/UpdateUserRoles/)
- [ ] Verify `dotnet build` — 0 errors

### Frontend
- [ ] Create `AuthContext.tsx` — stores access token in memory, exposes user state
- [ ] Create `useAuth.ts` hook
- [ ] Create `LoginPage.tsx` with React Hook Form + Zod validation
- [ ] Create `RegisterPage.tsx` with React Hook Form + Zod validation
- [ ] Create `ForgotPasswordPage.tsx`
- [ ] Create `ResetPasswordPage.tsx`
- [ ] Create `ProtectedRoute.tsx` — redirect to /login if not authenticated
- [ ] Create `AdminRoute.tsx` — redirect if not Admin/SuperAdmin
- [ ] Update `router.tsx` — wire up all auth routes and guards
- [ ] Update `App.tsx` — wrap with `AuthProvider`
- [ ] Implement silent session restore on app load (call /api/auth/me)
- [ ] Verify `npm run build` — 0 errors

### DevOps / CI
- [ ] Add `.github/workflows/ci.yml` — build + test on PR
- [ ] Add GitHub repo secrets: EC2_HOST, EC2_USER, EC2_SSH_KEY, GHCR_USERNAME, GHCR_TOKEN, POSTGRES_PASSWORD, JWT_SECRET, APP_DOMAIN
- [ ] Verify `docker compose up` starts all 3 services with DB migration

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

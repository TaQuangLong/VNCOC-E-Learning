# ChurchLearn — Church E-Learning Platform

A full-stack e-learning platform built for ~1,000 church members. Members can enroll in courses, watch lessons, take quizzes, and join discussions — all in one place.

---

## Tech Stack

### Backend
| Layer | Technology |
|---|---|
| Runtime | .NET 10 LTS — ASP.NET Core Web API |
| Database | PostgreSQL 16 + EF Core 10 (Npgsql) |
| Auth | ASP.NET Core Identity + JWT Bearer |
| Validation | FluentValidation |
| Logging | Serilog |
| API Docs | Scalar (`/scalar/v1` in dev) |

### Frontend
| Layer | Technology |
|---|---|
| Framework | React 18 + TypeScript + Vite |
| Routing | React Router v6 |
| State / Data | TanStack Query v5 |
| Forms | React Hook Form + Zod |
| UI | Tailwind CSS v4 + shadcn/ui |
| HTTP | Axios |

### Infrastructure
- Docker Compose (PostgreSQL + API + Frontend)
- GitHub Actions CI (build checks on push / PR)

---

## Architecture

**Modular Monolith — Vertical Slice Architecture**

Each feature lives in its own folder and is fully self-contained:

```
backend/src/ChurchLearn.Api/
├── Features/
│   ├── Auth/
│   │   ├── Register/     ← RegisterRequest, RegisterHandler
│   │   ├── Login/        ← LoginRequest, LoginHandler
│   │   ├── RefreshToken/
│   │   ├── GetCurrentUser/
│   │   ├── ForgotPassword/
│   │   └── ResetPassword/
│   ├── Courses/
│   ├── Lessons/
│   ├── Enrollments/
│   ├── Progress/
│   ├── Quizzes/
│   ├── Discussions/
│   ├── Reports/
│   └── Users/            ← Admin user management
├── Domain/
│   ├── Entities/         ← AppUser, Course, Lesson, ...
│   └── Enums/            ← AppRoles, CourseStatus, ...
├── Infrastructure/
│   ├── Persistence/      ← AppDbContext, DatabaseSeeder
│   ├── Identity/         ← JwtTokenService, CurrentUserService
│   └── Email/            ← IEmailSender, NoOpEmailSender, SmtpEmailSender
└── Common/
    ├── Interfaces/        ← ICurrentUser
    └── Middleware/        ← GlobalExceptionHandler
```

---

## URLs

### Local Development

| Service | URL |
|---|---|
| Frontend | http://localhost:5173 |
| API | http://localhost:5251 |
| API Docs (Scalar) | http://localhost:5251/scalar/v1 |
| PostgreSQL | `localhost:5432` |

> When running via Docker Compose the API is proxied to port **5000** instead of 5251.

### Fly.io (Production)

| Service | URL |
|---|---|
| Frontend | https://churchlearn-frontend.fly.dev |
| API | https://churchlearn-api.fly.dev |
| API Docs (Scalar) | https://churchlearn-api.fly.dev/scalar/v1 |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Run with Docker Compose

```bash
# Copy env file and fill in values
cp .env.example .env

# Start all services (postgres + api + frontend)
docker compose up --build
```

- API: http://localhost:5000
- API docs: http://localhost:5000/scalar/v1
- Frontend: http://localhost:5173

### Run locally (without Docker)

**Backend:**
```bash
cd backend
dotnet restore
dotnet run --project src/ChurchLearn.Api
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

**Database migration** (requires PostgreSQL running):
```bash
cd backend
dotnet ef database update --project src/ChurchLearn.Api
```

---

## Environment Variables

Copy `.env.example` to `.env` and set:

| Variable | Description |
|---|---|
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `JWT_SECRET` | JWT signing secret (min 32 chars) |
| `APP_DOMAIN` | App domain (e.g. `localhost`) |
| `VITE_API_URL` | Backend API URL for the frontend |

Backend config (`appsettings.json`) also reads:
- `ConnectionStrings:DefaultConnection`
- `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiryMinutes`
- `SuperAdmin:Email`, `SuperAdmin:Password`
- `Cors:AllowedOrigins`

---

## Auth Flow

- **Registration** → assigned `Student` role automatically
- **Access token** → 60-min JWT, returned in JSON response body, stored in React state only
- **Refresh token** → 7-day JWT, stored in `httpOnly` `Secure` `SameSite=Strict` cookie
- **Silent restore** → on page load the frontend calls `/api/auth/refresh` using the cookie to get a new access token
- **Roles** → `Student` | `Admin` | `SuperAdmin` (seeded at startup)
- **SuperAdmin** seed account created from `SuperAdmin:Email` / `SuperAdmin:Password` config

---

## API Endpoints (Sprint 2)

| Method | Path | Auth | Role |
|---|---|---|---|
| `POST` | `/api/auth/register` | — | Any |
| `POST` | `/api/auth/login` | — | Any |
| `POST` | `/api/auth/logout` | ✓ | Any |
| `GET` | `/api/auth/me` | ✓ | Any |
| `POST` | `/api/auth/refresh` | — | Any |
| `POST` | `/api/auth/forgot-password` | — | Any |
| `POST` | `/api/auth/reset-password` | — | Any |
| `GET` | `/api/admin/users` | ✓ | Admin+ |
| `GET` | `/api/admin/users/{id}` | ✓ | Admin+ |
| `PUT` | `/api/admin/users/{id}/roles` | ✓ | SuperAdmin |

Full endpoint map: [`knowledge-graph/api-map.md`](knowledge-graph/api-map.md)

---

## Security

- JWT never stored in `localStorage` — React state only
- Refresh token in `httpOnly` cookie — not accessible to JavaScript
- Rate limiting on `/auth/login` and `/auth/register` (10 req/min per IP)
- All inputs validated with FluentValidation at the API boundary
- Authorization checked server-side on every protected endpoint
- No secrets committed — use environment variables

---

## Testing in Production

Use the following steps to smoke-test the live deployment at **https://churchlearn-frontend.fly.dev**.

---

### As a Student

| # | Step | Where | Expected |
|---|------|--------|----------|
| 1 | Open the site | `/courses` | Course list loads; no login required |
| 2 | Register a new account | `/register` | Form submits; auto-logged in as Student |
| 3 | Log in with the new account | `/login` | Redirected to `/courses`; name visible in nav |
| 4 | Open any published course | `/courses/:slug` | Course details, lesson list, and **Enroll** button visible |
| 5 | Enroll in the course | `/courses/:slug` | Enrollment confirmed; **Start Learning** button appears |
| 6 | Open a lesson | `/learn/:courseId/lessons/:lessonId` | Lesson content (video/text/PDF) renders correctly |
| 7 | Complete the lesson | Navigate to next lesson | Lesson marked complete (✓ in sidebar); progress bar updates |
| 8 | Take a quiz (if present) | Lesson page → Take Quiz | Questions load; score returned after submit |
| 9 | Post a discussion comment | Lesson page → Discussion | Comment appears immediately |
| 10 | Check overall progress | `/my-learning` | Enrolled course shown with correct completion % |
| 11 | Log out and log back in | `/login` | Session restored; progress still visible |
| 12 | Test forgot password flow | `/login` → Forgot Password → `/reset-password` | Reset email received; password updated; login works |

---

### As an Admin

Log in with the seeded **SuperAdmin** account (credentials from `SuperAdmin:Email` / `SuperAdmin:Password` env vars), then promote a test user to Admin if needed via `PUT /api/admin/users/{id}/roles`.

| # | Step | Where | Expected |
|---|------|--------|----------|
| 1 | Log in as Admin/SuperAdmin | `/login` | Admin nav items visible |
| 2 | Open admin course list | `/admin/courses` | All courses listed; **New Course** button present |
| 3 | Create a new course (Draft) | `/admin/courses/new` | Course saved; status = Draft; not visible to students |
| 4 | Add at least one lesson | `/admin/courses/:courseId/lessons/new` | Lesson saved; appears in the lesson list |
| 5 | Add a quiz to the lesson | Lesson edit page → Add Quiz | Quiz saved with at least one question and passing score |
| 6 | Publish the course | Edit course → Status = Published → Save | Course now visible on `/courses` for students |
| 7 | Verify student can see and enroll | Open incognito, go to `/courses` | New course card appears; student can enroll |
| 8 | View enrolled learners | `/admin/courses/:courseId/learners` | Enrolled users listed with enrollment date and progress % |
| 9 | View a specific learner's progress | `/admin/users/:userId/progress` | Lesson-by-lesson completion breakdown shown |
| 10 | View admin dashboard | `/admin` | Stats visible: user count, course count, enrollments, quiz attempts, top 5 courses |
| 11 | Assign a role (SuperAdmin only) | `PUT /api/admin/users/{id}/roles` via Scalar or API client | Role updated; user gains new permissions on next login |
| 12 | Set course back to Draft | Edit course → Status = Draft → Save | Course hidden from `/courses`; enrolled students lose access |

---

### API Smoke Test (Scalar UI)

Open **https://churchlearn-api.fly.dev/scalar/v1** and verify:

1. `POST /api/auth/login` returns `200` with an access token
2. `GET /api/auth/me` with the token returns the current user's profile
3. `GET /api/courses` returns the list of published courses
4. `POST /api/auth/refresh` (cookie present) returns a new access token
5. `POST /api/auth/logout` clears the refresh token cookie

---

## License

Private — VNCOC Church Internal Use

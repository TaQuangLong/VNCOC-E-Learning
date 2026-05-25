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

## License

Private — VNCOC Church Internal Use

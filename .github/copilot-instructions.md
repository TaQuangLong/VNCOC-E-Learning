# ChurchLearn — GitHub Copilot Instructions

## Project Overview
Church e-learning platform for ~1,000 members.
ASP.NET Core Web API backend (.NET 10 LTS), React TypeScript frontend.
Deployed on EC2 with Docker Compose.

## Architecture
- Modular Monolith with Vertical Slice Architecture
- Each feature lives in Features/{FeatureName}/{ActionName}/
- No shared services layer — each handler is self-contained
- Frontend features live in src/features/{featureName}/

## Backend Stack
- .NET 10 LTS — ASP.NET Core Web API
- EF Core with PostgreSQL (Npgsql provider)
- ASP.NET Core Identity for auth and roles
- JWT for access tokens, httpOnly cookie for refresh token
- FluentValidation for request validation
- Serilog for structured logging
- Scalar.AspNetCore for API reference UI

## Frontend Stack
- React 18 + TypeScript + Vite
- React Router v6
- TanStack Query v5
- React Hook Form + Zod
- Tailwind CSS + shadcn/ui
- Axios for HTTP calls

## Backend Coding Rules
- PascalCase: classes, methods, properties, records
- camelCase: local variables, parameters
- async/await for all I/O operations
- Always include CancellationToken in handler methods
- Never return EF Core entities from endpoints — always use DTOs
- Use FluentValidation for request validation
- Use constants or enums — never magic strings or magic numbers
- Keep methods short and focused (prefer under 30 lines)
- Prefer explicit names: MarkLessonAsCompleted not DoStuff
- Use the Result Pattern for error handling — handlers return `Result<T>`, never throw domain exceptions
- Endpoints map `Result<T>` to HTTP status codes (200/201 on success, 4xx on failure)
- Never throw exceptions for expected domain errors (not found, conflict, forbidden) — encode them in Result

## Frontend Coding Rules
- TypeScript strictly — no implicit any
- PascalCase for React components
- camelCase for variables, functions, hooks, props
- Prefix custom hooks with use
- Always handle loading, error, and empty states in every component
- Always use Tailwind responsive prefixes for layout (mobile-first)
- Never hardcode API URLs — use import.meta.env.VITE_API_URL
- Use shared shadcn/ui components for common UI patterns

## Security Rules (Non-Negotiable)
- Always check authorization server-side — never trust the client
- Never store JWT in localStorage — use React state + httpOnly cookie
- Validate all input at the API boundary with FluentValidation
- Never commit secrets — use environment variables
- Rate limiting is required on /api/auth/login and /api/auth/register

## Database Rules
- Always use EF Core migrations for schema changes
- Add indexes for: UserId, CourseId, LessonId on join/lookup tables
- Unique index on (UserId, CourseId) for Enrollments
- Unique index on (UserId, LessonId) for LessonProgress
- Unique index on Slug for Courses

## What to Avoid
- No microservices
- No Kubernetes
- No event sourcing
- No complex CQRS infrastructure
- No large shared service classes
- No hardcoded connection strings or secrets
- No unvalidated external URLs stored as-is
- No localStorage for JWT tokens
- No throwing exceptions for domain-level errors — use Result Pattern instead
- No bare `throw` or `try/catch` in handler business logic for expected error paths

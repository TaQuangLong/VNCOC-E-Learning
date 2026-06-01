---
name: run-local-dev
description: 'Start the local development environment for manual testing. Builds and runs the .NET 10 backend API and the React/Vite frontend side-by-side. Use when: run locally, start dev server, run backend, run frontend, local testing, manual testing, start API, dotnet run, npm run dev, spin up local, debug locally.'
argument-hint: 'Optional: "backend", "frontend", or leave blank for both'
---

# Run Local Development Environment

Starts the ChurchLearn backend API and React frontend for manual testing on your local machine.

## Prerequisites

Verify these are satisfied before starting:

1. **PostgreSQL running** on `localhost:5432`
   - Database: `churchlearn`, User: `churchlearn`, Password: `localdevpassword123`
   - Start if needed: `brew services start postgresql@16` (or your version)

2. **`frontend/.env.local` exists** with:
   ```
   VITE_API_URL=http://localhost:5251
   ```
   - If missing: `echo "VITE_API_URL=http://localhost:5251" > frontend/.env.local`

3. **.NET 10 SDK** installed: `dotnet --version` should show `10.x`

4. **Node.js / npm** installed: `node --version`

---

## Step 1 — Start the Backend API

Open a terminal, navigate to the API project, and run:

```bash
cd backend/src/ChurchLearn.Api
dotnet run --launch-profile http
```

**Expected output:**
```
Now listening on: http://localhost:5251
Application started. Press Ctrl+C to shut down.
```

**Verify:**
- Health check: `curl http://localhost:5251/api/health` → `Healthy`
- Scalar API UI: `http://localhost:5251/scalar/v1`

> If you see a database migration error, run:
> ```bash
> dotnet ef database update
> ```
> from the same directory.

---

## Step 2 — Start the Frontend

Open a **second terminal**, navigate to the frontend, and run:

```bash
cd frontend
npm install        # skip if node_modules already up to date
npm run dev
```

**Expected output:**
```
  VITE v6.x.x  ready in xxx ms
  ➜  Local:   http://localhost:5173/
```

**Verify:** Open `http://localhost:5173` in your browser.

---

## Step 3 — Manual Testing Checklist

| Area | URL | Notes |
|------|-----|-------|
| Frontend app | http://localhost:5173 | React SPA |
| API health | http://localhost:5251/api/health | Should return `Healthy` |
| Scalar API UI | http://localhost:5251/scalar/v1 | Interactive API docs |
| Seed login | `superadmin@churchlearn.local` / `Admin@123456!` | SuperAdmin account |

---

## Stopping

- **Backend**: `Ctrl+C` in the backend terminal
- **Frontend**: `Ctrl+C` in the frontend terminal

---

## Common Issues

| Symptom | Fix |
|---------|-----|
| Port 5251 already in use | `lsof -ti :5251 | xargs kill -9` |
| Port 5173 already in use | `lsof -ti :5173 | xargs kill -9` |
| DB connection refused | Check PostgreSQL is running: `pg_isready -h localhost -p 5432` |
| `VITE_API_URL` undefined | Ensure `frontend/.env.local` exists with the correct value |
| CORS errors in browser | Confirm backend `appsettings.Development.json` has `http://localhost:5173` in `Cors.AllowedOrigins` |
| EF migration error | Run `dotnet ef database update` in `backend/src/ChurchLearn.Api/` |

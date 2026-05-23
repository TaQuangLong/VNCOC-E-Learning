# Deployment Guide

---

## Fly.io

### Step 1 — Install flyctl & sign up

```bash
# macOS
brew install flyctl
fly auth signup   # opens browser — sign up with GitHub
```

### Step 2 — Create apps & Postgres

```bash
# Create apps (names must be globally unique on Fly.io — change if taken)
fly apps create churchlearn-api
fly apps create churchlearn-frontend

# Create Postgres (free, Singapore region)
fly postgres create --name churchlearn-db --region sin
# Note the connection string printed at the end — you'll need it below
```

### Step 3 — Set API secrets

```bash
fly secrets set --app churchlearn-api \
  ConnectionStrings__DefaultConnection="Host=churchlearn-db.internal;Port=5432;Database=churchlearn;Username=churchlearn;Password=<PG_PASSWORD>" \
  Jwt__Secret="<generate: openssl rand -base64 64>" \
  Jwt__Issuer="https://churchlearn-api.fly.dev" \
  Jwt__Audience="https://churchlearn-api.fly.dev" \
  Cors__AllowedOrigins__0="https://churchlearn-frontend.fly.dev"
```

> The `PG_PASSWORD` and connection details come from the output of `fly postgres create` in Step 2.

### Step 4 — First deploy (manual, run once)

```bash
# Deploy API first
cd backend
fly deploy --remote-only

# Then deploy frontend
cd ../frontend
fly deploy --remote-only
```

### Step 5 — Set up auto-deploy (CI/CD)

1. Go to fly.io → **Account Settings → Access Tokens** → Create token
2. Add to GitHub repo: **Settings → Secrets and variables → Actions**
   - `FLY_API_TOKEN` = the token from above

From now on every `git push` to `main` triggers `.github/workflows/cd-fly.yml` automatically.

### Demo URLs (Fly.io)

| | URL |
|---|---|
| **Frontend** | `https://churchlearn-frontend.fly.dev` |
| **API Health** | `https://churchlearn-api.fly.dev/api/health` |
| **API Docs (Scalar)** | `https://churchlearn-api.fly.dev/scalar` |

> First request after idle may take ~5s (cold start on free tier).


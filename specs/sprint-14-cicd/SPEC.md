# Sprint 14 — GitHub Actions CI/CD

## Propose

### Goal
Every push to `main` automatically builds Docker images, tags them with the git SHA, pushes to GitHub Container Registry, deploys to EC2, runs a health check, and rolls back if the health check fails.

### Why This Sprint
Manual deployments break under time pressure. CI/CD is what allows a solo developer to ship confidently and recover quickly from a bad deployment without downtime.

### Success Criteria
- Pull request triggers build + test workflow and reports pass/fail
- Push to `main` builds, pushes, and deploys Docker images to EC2
- Health check at `/api/health` must pass after deploy
- If health check fails, previous image tag is automatically re-deployed
- No secrets are committed to the repository

---

## Design

### Technical Design

**CI Workflow (`.github/workflows/ci.yml`):**
- Triggers: `pull_request` to `main`
- Jobs:
  1. Backend: restore → build → test
  2. Frontend: install → build

**CD Workflow (`.github/workflows/cd.yml`):**
- Triggers: `push` to `main`
- Jobs:
  1. Build backend Docker image → tag with `git sha` and `latest` → push to GHCR
  2. Build frontend Docker image → tag with `git sha` and `latest` → push to GHCR
  3. Deploy to EC2:
     - SSH into EC2 (using `EC2_SSH_KEY` secret)
     - Save current image tag to `/deploy/.last-image-tag`
     - Pull new images
     - Run `docker compose up -d`
     - Run EF Core migrations: `dotnet ef database update` inside API container
     - Health check: `curl -f http://localhost:5000/api/health`
     - On failure: pull previous image from `.last-image-tag`, re-deploy

**Rollback Script (`/deploy/rollback.sh` on EC2):**
```bash
PREV_TAG=$(cat /deploy/.last-image-tag)
docker compose pull api:$PREV_TAG frontend:$PREV_TAG
docker compose up -d
```

**Image Naming:**
```
ghcr.io/{org}/churchlearn-api:{sha}
ghcr.io/{org}/churchlearn-api:latest
ghcr.io/{org}/churchlearn-frontend:{sha}
ghcr.io/{org}/churchlearn-frontend:latest
```

### GitHub Secrets Needed

| Secret | Description |
|--------|-------------|
| EC2_HOST | EC2 public IP or hostname |
| EC2_USER | SSH user (e.g. `ec2-user`) |
| EC2_SSH_KEY | Private SSH key PEM content |
| GHCR_USERNAME | GitHub username |
| GHCR_TOKEN | GitHub personal access token with `packages:write` |
| POSTGRES_PASSWORD | Production DB password |
| JWT_SECRET | Production JWT signing secret (min 32 chars) |
| APP_DOMAIN | Production domain (e.g. `learn.yourchurch.com`) |

### Architecture Decisions
- GHCR used instead of Docker Hub — free, integrated with GitHub repo
- Image tagged with git SHA for traceability and rollback
- Migrations run inside the container on deploy — safe for additive migrations
- Health check uses `curl -f` — exits non-zero on 4xx/5xx

### Entities Affected
None.

### API Changes
None.

---

## Tasks

### CI/CD
- [ ] Create `.github/workflows/ci.yml` — PR build/test
- [ ] Create `.github/workflows/cd.yml` — push to main deploy
- [ ] Add GHCR login step using `GHCR_TOKEN` secret
- [ ] Build and push backend image with `sha` and `latest` tags
- [ ] Build and push frontend image with `sha` and `latest` tags (pass `VITE_API_URL` as build arg)
- [ ] Add SSH deploy step to EC2
- [ ] Add health check step with retry loop (3 attempts, 10s wait)
- [ ] Add automatic rollback on health check failure
- [ ] Create `/deploy/rollback.sh` on EC2
- [ ] Add branch protection rule: require CI to pass before merge (optional, GitHub Settings)
- [ ] Add all required GitHub repo secrets
- [ ] Verify CI workflow passes on a test PR
- [ ] Verify CD workflow deploys successfully on push to main

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

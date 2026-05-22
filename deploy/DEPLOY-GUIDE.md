# Deployment Guide — Testing & Demo

Two free options: **Fly.io** (fastest) and **Oracle Cloud Always Free** (mirrors production EC2).

---

## Option A — Fly.io

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

---

## Option B — Oracle Cloud Always Free

### Step 1 — Create VM

1. Sign up at [cloud.oracle.com](https://cloud.oracle.com) (credit card required, never charged on free tier)
2. Go to **Compute → Instances → Create Instance**
3. Change shape: **Ampere → VM.Standard.A1.Flex** → set **4 OCPUs, 24 GB RAM** (all free)
4. Image: **Ubuntu 22.04**
5. Download the generated SSH key pair
6. Click **Create**

### Step 2 — Open ports in OCI Console

**Networking → VCN → Security Lists → Default Security List → Add Ingress Rules:**

| Protocol | Port | Source CIDR |
|---|---|---|
| TCP | 80 | 0.0.0.0/0 |
| TCP | 443 | 0.0.0.0/0 |
| TCP | 5000 | 0.0.0.0/0 |
| TCP | 5173 | 0.0.0.0/0 |

### Step 3 — Bootstrap the VM

```bash
# Copy the setup script to the VM
scp -i <your-key.pem> deploy/setup-oracle.sh ubuntu@<VM_PUBLIC_IP>:~/

# SSH in and run it
ssh -i <your-key.pem> ubuntu@<VM_PUBLIC_IP>
sudo ~/setup-oracle.sh
```

### Step 4 — Create `.env` on the VM

```bash
cp /deploy/.env.template /deploy/.env
nano /deploy/.env
```

Fill in real values:

```env
POSTGRES_PASSWORD=<strong-password>
JWT_SECRET=<run: openssl rand -base64 64>
APP_DOMAIN=<VM_PUBLIC_IP>
APP_FRONTEND_ORIGIN=http://<VM_PUBLIC_IP>:5173
GHCR_ORG=<your-github-username-lowercase>
IMAGE_TAG=latest
```

### Step 5 — Set GitHub secrets

Go to your repo → **Settings → Secrets and variables → Actions** → add:

| Secret | Value |
|---|---|
| `OCI_HOST` | VM public IP |
| `OCI_USER` | `ubuntu` |
| `OCI_SSH_KEY` | full contents of the private key file downloaded in Step 1 |
| `GHCR_USERNAME` | your GitHub username (lowercase) |
| `GHCR_TOKEN` | GitHub PAT with `read:packages` + `write:packages` scope |
| `POSTGRES_PASSWORD` | same as in `.env` |
| `JWT_SECRET` | same as in `.env` |
| `APP_DOMAIN` | VM public IP |

> To create a GitHub PAT: **GitHub → Settings → Developer settings → Personal access tokens → Fine-grained tokens** → grant `read/write:packages`.

### Step 6 — Trigger first deploy

Push any commit to `main`, or manually trigger via:
**GitHub → Actions → CD — Oracle Cloud → Run workflow**

### Demo URLs (Oracle Cloud)

| | URL |
|---|---|
| **Frontend** | `http://<VM_PUBLIC_IP>:5173` |
| **API Health** | `http://<VM_PUBLIC_IP>:5000/api/health` |
| **API Docs (Scalar)** | `http://<VM_PUBLIC_IP>:5000/scalar` |

> Oracle Cloud demo runs on `http://` (no SSL). This is fine for testing. Add Caddy + a domain for SSL when going to production.

---

## Comparison

| | Fly.io | Oracle Cloud |
|---|---|---|
| Setup time | ~15 min | ~45 min |
| Always on | No (sleeps after idle) | Yes |
| SSL | Auto (HTTPS) | No (HTTP only, unless you add Caddy) |
| Mirrors EC2 pipeline | No | Yes |
| Free forever | Yes (with limits) | Yes (4 OCPUs, 24 GB RAM) |
| Best for | Quick demos | Persistent testing |

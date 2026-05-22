# Oracle Cloud Free Tier — Step-by-Step Deployment Guide

> **Target:** Ubuntu 22.04 VM on Oracle Cloud Ampere (ARM) — always-on, free forever  
> **Result:** ChurchLearn running at `http://<VM_IP>:5173` (frontend) + `http://<VM_IP>:5000` (API)

---

## Overview of all steps

| # | Where | What |
|---|---|---|
| 1 | OCI Console (browser) | Create Oracle account + VM |
| 2 | OCI Console (browser) | Open firewall ports in VCN |
| 3 | Your Mac (terminal) | SCP setup script to VM |
| 4 | VM (SSH) | Run bootstrap script |
| 5 | VM (SSH) | Create `.env` file |
| 6 | GitHub (browser) | Add 8 repository secrets |
| 7 | GitHub (browser) | Trigger first deploy |
| 8 | Your Mac (terminal) | Verify deployment |

---

## Step 1 — Create Oracle Cloud account and VM

### 1.1 Sign up

1. Go to **https://cloud.oracle.com**
2. Click **Start for free**
3. Fill in name, email, country
4. Verify email → set password
5. Enter credit card (required for verification — **you will not be charged** on Always Free resources)
6. Choose your **Home Region** — pick the closest one to your users (e.g. `ap-singapore-1` for Southeast Asia). **This cannot be changed later.**
7. Complete sign-up → you will land in the OCI Console

### 1.2 Create the VM

1. In the OCI Console top menu click **☰ → Compute → Instances**
2. Click **Create instance**
3. Fill in the form:

   | Field | Value |
   |---|---|
   | **Name** | `churchlearn-vm` |
   | **Compartment** | `root` (default) |
   | **Placement / Availability domain** | Any (AD-1 recommended) |

4. Under **Image and shape** click **Edit**:
   - Click **Change shape**
   - Select **Ampere** tab
   - Select **VM.Standard.A1.Flex**
   - Set **OCPUs: 4** and **Memory: 24 GB** (both free)
   - Click **Select shape**
   - Image: keep **Ubuntu 22.04** (Minimal or Server, either works)

5. Under **Networking** — leave defaults (it creates a VCN automatically)

6. Under **Add SSH keys**:
   - Select **Generate a key pair for me**
   - Click **Save private key** → save as `churchlearn-key.pem` somewhere safe (e.g. `~/.ssh/churchlearn-key.pem`)
   - Click **Save public key** (optional backup)

7. Click **Create** at the bottom

8. Wait ~2 minutes for the instance state to show **Running**

9. On the instance detail page, copy the **Public IP address** — you will use it throughout this guide as `<VM_IP>`

---

## Step 2 — Open firewall ports in OCI Console

Oracle Cloud has two layers of firewall:
- **VCN Security List** — cloud-level rules (configured here)
- **OS iptables** — handled automatically by `setup-oracle.sh` in Step 4

### 2.1 Open the Security List

1. On your instance detail page, scroll down to **Primary VNIC**
2. Click the **Subnet** link
3. Click **Default Security List for …**
4. Click **Add Ingress Rules** and add **four** rules one by one (or all at once):

   | Stateless | Source Type | Source CIDR | IP Protocol | Destination Port |
   |---|---|---|---|---|
   | No | CIDR | 0.0.0.0/0 | TCP | 80 |
   | No | CIDR | 0.0.0.0/0 | TCP | 443 |
   | No | CIDR | 0.0.0.0/0 | TCP | 5000 |
   | No | CIDR | 0.0.0.0/0 | TCP | 5173 |

   Leave **Description** blank, leave all other fields as default.

5. Click **Add Ingress Rules**

---

## Step 3 — Copy the bootstrap script to the VM (your Mac)

Open a terminal on your Mac:

```bash
# Fix permissions on the key (SSH requires this)
chmod 400 ~/.ssh/churchlearn-key.pem

# Copy the setup script to the VM
scp -i ~/.ssh/churchlearn-key.pem \
  /Users/mac/ENTERPRISE/VNCOC-ELearning/deploy/setup-oracle.sh \
  ubuntu@<VM_IP>:~/setup-oracle.sh
```

Replace `<VM_IP>` with the public IP from Step 1.9.

If prompted `Are you sure you want to continue connecting?` → type **yes** and press Enter.

---

## Step 4 — Bootstrap the VM (SSH into VM)

```bash
# SSH into the VM
ssh -i ~/.ssh/churchlearn-key.pem ubuntu@<VM_IP>

# Inside the VM — run the bootstrap script
chmod +x ~/setup-oracle.sh
sudo ~/setup-oracle.sh
```

The script will:
- Update apt packages
- Install Docker CE + Docker Compose plugin
- Open iptables ports 80, 443, 5000, 5173
- Create `/deploy/` directory
- Write `/deploy/.env.template`

Expected last line of output:
```
 Setup complete.
```

**Important:** After the script finishes, run this so your current shell picks up the docker group:
```bash
newgrp docker
```

Verify Docker works:
```bash
docker ps
# Should show: CONTAINER ID   IMAGE   COMMAND   ...  (empty table is fine)
```

---

## Step 5 — Create the `.env` file on the VM

Still inside the SSH session on the VM:

```bash
cp /deploy/.env.template /deploy/.env
nano /deploy/.env
```

Fill in **all 6 values**:

```env
POSTGRES_PASSWORD=<make up a strong password, e.g. Xk9#mP2$vQ7!wRn>
JWT_SECRET=<run the command below and paste the output>
APP_DOMAIN=<VM_IP>
APP_FRONTEND_ORIGIN=http://<VM_IP>:5173
GHCR_ORG=taquanglong
IMAGE_TAG=latest
```

**Generate `JWT_SECRET`** — run this in a separate terminal (not on the VM) and paste the output:
```bash
openssl rand -base64 64 | tr -d '\n'
```

Save with `Ctrl+O`, Enter, then `Ctrl+X` to exit nano.

Verify the file looks correct:
```bash
cat /deploy/.env
```

Exit the VM:
```bash
exit
```

---

## Step 6 — Add GitHub repository secrets

Go to: **https://github.com/TaQuangLong/VNCOC-E-Learning/settings/secrets/actions**

Click **New repository secret** for each of the following (exact names, case-sensitive):

| Secret name | Value |
|---|---|
| `OCI_HOST` | `<VM_IP>` — the public IP from Step 1.9 |
| `OCI_USER` | `ubuntu` |
| `OCI_SSH_KEY` | Full contents of `~/.ssh/churchlearn-key.pem` — open the file, select all, paste |
| `GHCR_USERNAME` | `taquanglong` (your GitHub username, all lowercase) |
| `GHCR_TOKEN` | A GitHub PAT — see Step 6.1 below |
| `POSTGRES_PASSWORD` | Same value you put in `/deploy/.env` |
| `JWT_SECRET` | Same value you put in `/deploy/.env` |
| `APP_DOMAIN` | `<VM_IP>` — same as `OCI_HOST` |

### 6.1 Create a GitHub Personal Access Token (GHCR_TOKEN)

1. Go to **https://github.com/settings/tokens?type=beta** (Fine-grained tokens)
2. Click **Generate new token**
3. Set:
   - **Token name:** `churchlearn-ghcr`
   - **Expiration:** 1 year (or No expiration)
   - **Repository access:** Only select repositories → `VNCOC-E-Learning`
   - **Permissions → Repository permissions:**
     - `Contents` → Read-only
     - `Packages` → Read and write
4. Click **Generate token**
5. Copy the token immediately (shown only once) → paste it as `GHCR_TOKEN`

### 6.2 How to paste the SSH private key

On your Mac terminal:
```bash
cat ~/.ssh/churchlearn-key.pem
```

Copy **everything** including the `-----BEGIN RSA PRIVATE KEY-----` and `-----END RSA PRIVATE KEY-----` lines, and paste it into the `OCI_SSH_KEY` secret field.

---

## Step 7 — Trigger the first deploy

### Option A — Automatic (push to main)

```bash
cd /Users/mac/ENTERPRISE/VNCOC-ELearning
git add .
git commit -m "chore: trigger first oracle deploy"
git push origin main
```

### Option B — Manual trigger (no code changes needed)

1. Go to **https://github.com/TaQuangLong/VNCOC-E-Learning/actions**
2. Click **CD — Oracle Cloud** in the left sidebar
3. Click **Run workflow** (top right) → **Run workflow**

### 7.1 Watch the pipeline

1. Click the running workflow to see live logs
2. Two jobs run in sequence:
   - **Build & Push Docker Images** (~5–8 min) — builds API + frontend images, pushes to `ghcr.io`
   - **Deploy to Oracle Cloud VM** (~2–3 min) — SSHs into VM, pulls images, starts containers, runs health check

A green checkmark means everything worked.

If it fails, click the failed step to read the error log.

---

## Step 8 — Verify the deployment

From your Mac terminal:

```bash
# Check API health endpoint
curl http://<VM_IP>:5000/api/health
# Expected: {"status":"Healthy"} or similar JSON

# Check API docs are accessible
curl -I http://<VM_IP>:5000/scalar
# Expected: HTTP/1.1 200 OK
```

Open in your browser:

| URL | What you should see |
|---|---|
| `http://<VM_IP>:5173` | ChurchLearn login page |
| `http://<VM_IP>:5000/scalar` | API reference docs (Scalar UI) |
| `http://<VM_IP>:5000/api/health` | `{"status":"Healthy"}` |

---

## Troubleshooting

### "Permission denied (publickey)" when SSH-ing into VM

```bash
# Make sure the key permissions are correct
chmod 400 ~/.ssh/churchlearn-key.pem
# Make sure you're using the right key and IP
ssh -i ~/.ssh/churchlearn-key.pem ubuntu@<VM_IP>
```

### Frontend loads but API calls fail (CORS error in browser console)

Check that `APP_FRONTEND_ORIGIN` in `/deploy/.env` on the VM exactly matches what the browser shows in the address bar (including port 5173):
```bash
ssh -i ~/.ssh/churchlearn-key.pem ubuntu@<VM_IP>
grep APP_FRONTEND_ORIGIN /deploy/.env
# Should be: APP_FRONTEND_ORIGIN=http://<VM_IP>:5173
```

### Docker containers not running on VM

```bash
ssh -i ~/.ssh/churchlearn-key.pem ubuntu@<VM_IP>
cd /deploy
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs --tail=50
```

### Port not accessible from browser (connection refused / timeout)

Verify both firewall layers:
1. **OCI Security List** — Step 2 above (cloud firewall)
2. **iptables** — run on VM:
   ```bash
   sudo iptables -L INPUT -n --line-numbers | grep -E "5000|5173"
   # Should show ACCEPT rules for both ports
   ```
   If missing:
   ```bash
   sudo iptables -I INPUT 6 -p tcp --dport 5000 -j ACCEPT
   sudo iptables -I INPUT 6 -p tcp --dport 5173 -j ACCEPT
   sudo netfilter-persistent save
   ```

### GitHub Actions deploy job fails with "Host key verification failed"

This happens on the very first deploy. Add the VM's host key to known_hosts by SSHing in manually from your Mac first (Step 3 and 4 already do this).

### Containers start but health check fails

```bash
ssh -i ~/.ssh/churchlearn-key.pem ubuntu@<VM_IP>
# Check API logs for startup errors
docker compose -f /deploy/docker-compose.prod.yml logs api --tail=100
# Common causes: wrong POSTGRES_PASSWORD in .env, DB not yet ready
```

---

## Rollback to previous version

If a bad deploy is ever pushed, SSH into the VM and run:

```bash
ssh -i ~/.ssh/churchlearn-key.pem ubuntu@<VM_IP>
GHCR_ORG=taquanglong /deploy/rollback.sh
```

---

## What auto-deploys after this

Every `git push` to the `main` branch automatically:
1. Builds new Docker images tagged with the Git SHA
2. Pushes them to GitHub Container Registry
3. SSHs into your Oracle VM and rolls out the new images
4. Runs a health check — auto-rolls back if the API doesn't respond

No manual steps needed for subsequent deploys.

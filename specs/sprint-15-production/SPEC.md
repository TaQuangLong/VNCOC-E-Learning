# Sprint 15 — Production Deployment (EC2)

## Propose

### Goal
The platform is live on EC2 with a real domain, HTTPS via Let's Encrypt, Nginx reverse proxy, persistent PostgreSQL volume, and a working backup script. Production is hardened and ready for pilot users.

### Why This Sprint
No pilot launch can happen without a stable production environment. This sprint turns the Docker Compose setup into a real running service accessible by church members.

### Success Criteria
- Website accessible at `https://learn.yourchurch.com`
- HTTPS certificate is valid (Let's Encrypt)
- API accessible at `https://learn.yourchurch.com/api`
- PostgreSQL is not publicly exposed
- SuperAdmin can log in and manage content
- Database backup script runs successfully
- `GET /api/health` returns healthy in production

---

## Design

### Technical Design

**EC2 Server Setup:**
- Ubuntu 22.04 LTS (or Amazon Linux 2)
- Docker Engine installed
- Docker Compose plugin installed
- Production folder: `/deploy/`
- `.env` file at `/deploy/.env` — production secrets (not committed)

**Nginx Reverse Proxy:**
- Listens on port 80 → redirect to 443
- `learn.yourchurch.com` → frontend container (port 80 internally)
- `learn.yourchurch.com/api` → API container (port 8080 internally)
- Certbot / Let's Encrypt for HTTPS

**Production Docker Compose Services:**
```
nginx        — Nginx reverse proxy (host ports 80, 443)
api          — ChurchLearn API (internal port 8080)
frontend     — React + Nginx (internal port 80)
postgres     — PostgreSQL 16 (internal only, not exposed to host)
```

**Security Hardening:**
- EC2 security group: allow 80, 443, 22 only — no 5432, no 5000
- PostgreSQL password is strong random string
- JWT secret is min 64 chars random string
- CORS set to production domain only
- API in Production environment (`ASPNETCORE_ENVIRONMENT=Production`)

**Database Backup:**
- Script at `/deploy/backup.sh`
- Runs daily via crontab
- Stores dumps at `/deploy/backups/churchlearn_YYYYMMDD_HHMMSS.sql`
- Keep last 30 days, delete older

**Nginx Config (production):**
```nginx
server {
    listen 80;
    server_name learn.yourchurch.com;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl;
    server_name learn.yourchurch.com;
    ssl_certificate /etc/letsencrypt/live/...;
    ssl_certificate_key /etc/letsencrypt/live/...;

    location /api {
        proxy_pass http://api:8080;
    }

    location / {
        proxy_pass http://frontend:80;
    }
}
```

### Architecture Decisions
- Certbot runs in a separate container — auto-renews every 60 days
- No ELB/ALB — Nginx handles SSL termination directly (single server, cost conscious)
- Database container NOT exposed on host port — internal Docker network only

### Entities Affected
None.

### API Changes
None.

---

## Tasks

### EC2 Setup
- [ ] Provision EC2 instance (or verify existing server capacity)
- [ ] Install Docker Engine on EC2
- [ ] Install Docker Compose plugin on EC2
- [ ] Create `/deploy/` production directory
- [ ] Create `/deploy/.env` with production secrets
- [ ] Create `/deploy/docker-compose.prod.yml`
- [ ] Create `/deploy/nginx.conf` with HTTPS + reverse proxy config

### HTTPS
- [ ] Point domain DNS to EC2 IP address
- [ ] Run Certbot to obtain Let's Encrypt certificate
- [ ] Configure auto-renewal via crontab or systemd timer

### Backup
- [ ] Create `/deploy/backup.sh` with pg_dump + rotation script
- [ ] Add crontab entry for daily backup
- [ ] Run backup script manually and verify dump file is valid
- [ ] Test restore: `psql churchlearn < backup.sql` on local DB

### Launch Verification
- [ ] Run `docker compose up -d` on EC2
- [ ] Verify `https://learn.yourchurch.com` loads React app
- [ ] Verify `https://learn.yourchurch.com/api/health` returns healthy
- [ ] Verify `https://learn.yourchurch.com/scalar/v1` loads Scalar (dev mode — disable in prod)
- [ ] Login as SuperAdmin in production
- [ ] Create a test course in production
- [ ] Verify enrollment and lesson access work end-to-end
- [ ] Verify PostgreSQL is NOT accessible from outside EC2 (port scan check)

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

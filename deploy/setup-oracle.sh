#!/usr/bin/env bash
# setup-oracle.sh — Bootstrap an Oracle Cloud Free Tier VM for ChurchLearn
#
# Tested on: Ubuntu 22.04 LTS (Ampere ARM / AMD x86)
# Run once as the default user with sudo after VM creation:
#
#   chmod +x setup-oracle.sh
#   sudo ./setup-oracle.sh
#
# After this script completes:
#   1. Copy your docker-compose.prod.yml to /deploy/
#   2. Create /deploy/.env with your real values (see template below)
#   3. Set GitHub secrets: OCI_HOST, OCI_USER, OCI_SSH_KEY, GHCR_USERNAME,
#      GHCR_TOKEN, POSTGRES_PASSWORD, JWT_SECRET, APP_DOMAIN
#   4. Re-login or run: newgrp docker

set -euo pipefail

DEPLOY_USER="${SUDO_USER:-ubuntu}"
DEPLOY_DIR="/deploy"

echo "==> Updating system packages..."
apt-get update -y
apt-get upgrade -y

echo "==> Installing Docker..."
apt-get install -y ca-certificates curl gnupg lsb-release

install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
  | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
chmod a+r /etc/apt/keyrings/docker.gpg

echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" \
  | tee /etc/apt/sources.list.d/docker.list > /dev/null

apt-get update -y
apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

systemctl enable docker
systemctl start docker

echo "==> Adding $DEPLOY_USER to docker group..."
usermod -aG docker "$DEPLOY_USER"

# -----------------------------------------------------------------------
# Oracle Cloud VMs ship with iptables rules that block all inbound traffic
# by default, even if the Security List/VCN allows it. Open the ports here.
# -----------------------------------------------------------------------
echo "==> Opening firewall ports (iptables)..."
apt-get install -y iptables-persistent

iptables -I INPUT 6 -p tcp --dport 80   -j ACCEPT
iptables -I INPUT 6 -p tcp --dport 443  -j ACCEPT
iptables -I INPUT 6 -p tcp --dport 5000 -j ACCEPT  # API direct (optional)
iptables -I INPUT 6 -p tcp --dport 5173 -j ACCEPT  # Frontend

netfilter-persistent save

echo "==> Creating deploy directory..."
mkdir -p "$DEPLOY_DIR"
chown "$DEPLOY_USER":"$DEPLOY_USER" "$DEPLOY_DIR"

echo "==> Writing .env template..."
cat > "$DEPLOY_DIR/.env.template" << 'EOF'
# Copy this file to .env and fill in real values before first deploy.
POSTGRES_PASSWORD=change-me-strong-password
JWT_SECRET=change-me-use-a-very-long-random-string
APP_DOMAIN=your-vm-public-ip
APP_FRONTEND_ORIGIN=http://your-vm-public-ip:5173
GHCR_ORG=your-github-username-lowercase
IMAGE_TAG=latest
EOF

chown "$DEPLOY_USER":"$DEPLOY_USER" "$DEPLOY_DIR/.env.template"

echo ""
echo "================================================================="
echo " Setup complete."
echo "================================================================="
echo ""
echo " Next steps:"
echo "   1. Copy docker-compose.prod.yml to $DEPLOY_DIR/"
echo "      (The CD pipeline does this automatically on each deploy)"
echo ""
echo "   2. Create $DEPLOY_DIR/.env:"
echo "      cp $DEPLOY_DIR/.env.template $DEPLOY_DIR/.env"
echo "      nano $DEPLOY_DIR/.env"
echo ""
echo "   3. Set these GitHub repository secrets:"
echo "      OCI_HOST          = $(curl -s ifconfig.me 2>/dev/null || echo '<your-public-ip>')"
echo "      OCI_USER          = $DEPLOY_USER"
echo "      OCI_SSH_KEY       = <contents of your private SSH key>"
echo "      GHCR_USERNAME     = <github username lowercase>"
echo "      GHCR_TOKEN        = <github PAT with read:packages scope>"
echo "      POSTGRES_PASSWORD = <same as in .env>"
echo "      JWT_SECRET        = <same as in .env>"
echo "      APP_DOMAIN        = <public IP or domain>"
echo ""
echo "   4. Also open ports 80 and 443 in the OCI Console:"
echo "      VCN > Security List > Ingress Rules"
echo ""
echo "   5. Re-login or run: newgrp docker"
echo ""

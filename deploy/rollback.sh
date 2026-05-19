#!/usr/bin/env bash
# rollback.sh — Manually re-deploy the previous image tag on EC2
#
# Usage:
#   GHCR_ORG=<owner> ./rollback.sh
#
# Prerequisites:
#   - Run from the /deploy directory, or set DEPLOY_DIR explicitly
#   - GHCR_ORG must be set in the environment (e.g. your GitHub username/org, lowercase)
#   - /deploy/.previous-image-tag must exist (written by the CD workflow on each deploy)

set -e

DEPLOY_DIR="$(cd "$(dirname "$0")" && pwd)"
PREV_TAG_FILE="$DEPLOY_DIR/.previous-image-tag"

if [ -z "$GHCR_ORG" ]; then
  echo "Error: GHCR_ORG environment variable is not set."
  echo "Usage: GHCR_ORG=<owner> ./rollback.sh"
  exit 1
fi

if [ ! -f "$PREV_TAG_FILE" ]; then
  echo "Error: No previous image tag found at $PREV_TAG_FILE."
  echo "Cannot rollback — no prior deployment recorded."
  exit 1
fi

PREV_TAG=$(cat "$PREV_TAG_FILE")
echo "Rolling back to image tag: $PREV_TAG"

export GHCR_ORG="$GHCR_ORG"
export IMAGE_TAG="$PREV_TAG"

docker compose -f "$DEPLOY_DIR/docker-compose.prod.yml" pull api frontend
docker compose -f "$DEPLOY_DIR/docker-compose.prod.yml" up -d

echo "Rollback to $PREV_TAG complete."

#!/bin/bash
set -e

IMAGE_NAME="doanson44/thaix"
TAG="${IMAGE_TAG:-latest}"
CONTAINER_NAME="thaix"

MAX_RETRIES=30
RETRY_INTERVAL=3
STARTUP_WAIT=30
PORT=8080

echo "Pulling image ${IMAGE_NAME}:${TAG}..."
docker pull "${IMAGE_NAME}:${TAG}"

if [ "$(docker ps -aq -f name=^/${CONTAINER_NAME}$)" ]; then
    echo "Stopping and removing existing container..."
    docker rm -f "${CONTAINER_NAME}"
fi

echo "Ensuring host directories exist..."
sudo mkdir -p /srv/thaix/{storage,logs,keys}

echo "Starting new container..."
docker run -d --name "${CONTAINER_NAME}" --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v /srv/thaix/appsettings.Production.json:/app/appsettings.Production.json:ro,z \
  -v /srv/thaix/storage:/app/storage:z \
  -v /srv/thaix/logs:/app/Logs:z \
  -v /srv/thaix/keys:/root/.aspnet/DataProtection-Keys:z \
  "${IMAGE_NAME}:${TAG}"

echo "Waiting ${STARTUP_WAIT}s before health checks..."
sleep "${STARTUP_WAIT}"

echo "Checking application health..."
RETRY=0
until curl -sS -o /dev/null --connect-timeout 2 --max-time 5 "http://127.0.0.1:${PORT}/"; do
    RETRY=$((RETRY + 1))

    if [ "${RETRY}" -ge "${MAX_RETRIES}" ]; then
        echo "Application did not become ready after ${MAX_RETRIES} retries."
        docker ps -a --filter "name=${CONTAINER_NAME}"
        docker logs --tail 100 "${CONTAINER_NAME}" || true
        exit 1
    fi

    echo "Health check failed. Retry ${RETRY}/${MAX_RETRIES} in ${RETRY_INTERVAL}s..."
    sleep "${RETRY_INTERVAL}"
done

echo "Application is ready."

echo "Cleaning up dangling images..."
docker image prune -f

echo "Deployment complete! Checking status:"
docker ps --filter "name=${CONTAINER_NAME}"
docker inspect "${CONTAINER_NAME}" --format '{{.Config.Image}}'

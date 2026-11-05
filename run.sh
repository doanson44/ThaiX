#!/bin/bash
set -e

IMAGE_NAME="doanson44/thaix"
TAG="${IMAGE_TAG:-latest}"
CONTAINER_NAME="thaix"
NETWORK_NAME="thaix-network"
DB_CONTAINER_NAME="thaix-mariadb"
DATABASE="thaix_db"
DB_ROOT_PASSWORD="${MARIADB_ROOT_PASSWORD:-Abc@12345}"

MAX_RETRIES=30
RETRY_INTERVAL=3
STARTUP_WAIT=30
PORT=8080

echo "Ensuring Docker network ${NETWORK_NAME} exists..."
if ! docker network inspect "${NETWORK_NAME}" >/dev/null 2>&1; then
    docker network create "${NETWORK_NAME}" >/dev/null
fi

echo "Pulling image ${IMAGE_NAME}:${TAG}..."
docker pull "${IMAGE_NAME}:${TAG}"

echo "Waiting for MariaDB container ${DB_CONTAINER_NAME}..."
if ! docker inspect "${DB_CONTAINER_NAME}" >/dev/null 2>&1; then
    echo "MariaDB container not found. Run db.sh before deploying the application."
    exit 1
fi

DB_RETRIES=0
until docker exec -e "MYSQL_PWD=${DB_ROOT_PASSWORD}" "${DB_CONTAINER_NAME}" mariadb-admin ping -uroot --silent >/dev/null 2>&1; do
    DB_RETRIES=$((DB_RETRIES + 1))
    if [ "${DB_RETRIES}" -ge "${MAX_RETRIES}" ]; then
        echo "MariaDB did not become ready."
        docker logs --tail 100 "${DB_CONTAINER_NAME}" || true
        exit 1
    fi
    sleep "${RETRY_INTERVAL}"
done

OLD_MODE_SQL="SET GLOBAL old_mode = CONCAT_WS(',', NULLIF(@@GLOBAL.old_mode, ''), IF(FIND_IN_SET('UTF8_IS_UTF8MB3', @@GLOBAL.old_mode) = 0, 'UTF8_IS_UTF8MB3', NULL), IF(FIND_IN_SET('NO_NULL_COLLATION_IDS', @@GLOBAL.old_mode) = 0, 'NO_NULL_COLLATION_IDS', NULL));"
docker exec -e "MYSQL_PWD=${DB_ROOT_PASSWORD}" "${DB_CONTAINER_NAME}" mariadb -uroot -e "${OLD_MODE_SQL}" >/dev/null
echo "MariaDB old_mode compatibility flags configured."

# New tables inherit the database collation; it must match the utf8mb4_unicode_ci key columns (FK errno 150).
docker exec -e "MYSQL_PWD=${DB_ROOT_PASSWORD}" "${DB_CONTAINER_NAME}" mariadb -uroot -e "ALTER DATABASE ${DATABASE} CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;" >/dev/null
echo "MariaDB default collation set to utf8mb4_unicode_ci."

if [ "$(docker ps -aq -f name=^/${CONTAINER_NAME}$)" ]; then
    echo "Stopping and removing existing container..."
    docker rm -f "${CONTAINER_NAME}"
fi

echo "Ensuring host directories exist..."
sudo mkdir -p /srv/thaix/{storage,logs,keys}

echo "Starting new container..."
docker run -d --name "${CONTAINER_NAME}" --restart unless-stopped \
  --network "${NETWORK_NAME}" \
  --init \
  --ipc=host \
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

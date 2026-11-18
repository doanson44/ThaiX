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

OLD_IMAGE_AVAILABLE=false
DEPLOYMENT_STARTED=false
DEPLOYMENT_SUCCEEDED=false

start_container() {
    local image_ref="$1"

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
      "${image_ref}"
}

wait_for_health() {
    local retry=0

    echo "Waiting ${STARTUP_WAIT}s before health checks..."
    sleep "${STARTUP_WAIT}"

    echo "Checking application health..."
    until curl -sS -o /dev/null --connect-timeout 2 --max-time 5 "http://127.0.0.1:${PORT}/"; do
        retry=$((retry + 1))

        if [ "${retry}" -ge "${MAX_RETRIES}" ]; then
            echo "Application did not become ready after ${MAX_RETRIES} retries."
            docker ps -a --filter "name=${CONTAINER_NAME}"
            docker logs --tail 100 "${CONTAINER_NAME}" || true
            return 1
        fi

        echo "Health check failed. Retry ${retry}/${MAX_RETRIES} in ${RETRY_INTERVAL}s..."
        sleep "${RETRY_INTERVAL}"
    done

    echo "Application is ready."
}

rollback_on_exit() {
    local exit_code=$?

    if [ "${exit_code}" -eq 0 ] || [ "${DEPLOYMENT_SUCCEEDED}" = true ] || [ "${DEPLOYMENT_STARTED}" != true ]; then
        return
    fi

    trap - EXIT
    set +e

    echo "Deployment failed. Starting automatic rollback..."

    if [ "${OLD_IMAGE_AVAILABLE}" != true ]; then
        echo "No previous image is available; automatic rollback is not possible."
        return
    fi

    if docker inspect "${CONTAINER_NAME}" >/dev/null 2>&1; then
        docker rm -f "${CONTAINER_NAME}" >/dev/null
    fi

    if start_container "${IMAGE_NAME}:old"; then
        if wait_for_health; then
            docker image tag "${IMAGE_NAME}:old" "${IMAGE_NAME}:latest"
            echo "Rollback completed successfully using ${IMAGE_NAME}:old."
            return
        fi
    fi

    echo "CRITICAL: automatic rollback failed. Recent container logs:"
    docker logs --tail 100 "${CONTAINER_NAME}" || true
    return
}

trap rollback_on_exit EXIT

echo "Ensuring Docker network ${NETWORK_NAME} exists..."
if ! docker network inspect "${NETWORK_NAME}" >/dev/null 2>&1; then
    docker network create "${NETWORK_NAME}" >/dev/null
fi

if docker inspect "${CONTAINER_NAME}" >/dev/null 2>&1; then
    CURRENT_IMAGE_ID="$(docker inspect --format='{{.Image}}' "${CONTAINER_NAME}")"
    echo "Tagging current image as ${IMAGE_NAME}:old..."
    docker image tag "${CURRENT_IMAGE_ID}" "${IMAGE_NAME}:old"
    OLD_IMAGE_AVAILABLE=true
fi

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

echo "Ensuring host directories exist..."
sudo mkdir -p /srv/thaix/{storage,logs,keys}

DEPLOYMENT_STARTED=true

if docker inspect "${CONTAINER_NAME}" >/dev/null 2>&1; then
    echo "Stopping and removing existing container..."
    docker rm -f "${CONTAINER_NAME}"
fi

echo "Removing obsolete ThaiX images before pulling the new image..."
while IFS= read -r IMAGE_REF; do
    [ -n "${IMAGE_REF}" ] || continue
    [ "${IMAGE_REF}" = "${IMAGE_NAME}:old" ] && continue
    docker image rm "${IMAGE_REF}" || {
        echo "Warning: could not remove ${IMAGE_REF}; it may be referenced by another container."
    }
done < <(docker image ls "${IMAGE_NAME}" --format '{{.Repository}}:{{.Tag}}')

docker image prune -f

echo "Pulling image ${IMAGE_NAME}:${TAG}..."
docker pull "${IMAGE_NAME}:${TAG}"

echo "Starting new container..."
start_container "${IMAGE_NAME}:${TAG}"

wait_for_health

echo "Updating latest tag to the deployed image..."
docker image tag "${IMAGE_NAME}:${TAG}" "${IMAGE_NAME}:latest"

DEPLOYMENT_SUCCEEDED=true
trap - EXIT

echo "Deployment complete! Checking status:"
docker ps --filter "name=${CONTAINER_NAME}"
docker inspect "${CONTAINER_NAME}" --format '{{.Config.Image}}'

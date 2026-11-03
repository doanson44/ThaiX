#!/usr/bin/env bash
set -Eeuo pipefail

CONTAINER_NAME="thaix-mariadb"
API_CONTAINER="thaix"
NETWORK_NAME="thaix-network"
IMAGE="mariadb:11.4"
DATA_DIR="/srv/thaix/data"
DATABASE="thaix_db"
DB_USER="thaix_user"
DB_PASSWORD="Abc@12345"
ROOT_PASSWORD="Abc@12345"
STARTUP_TIMEOUT=180

log() {
    printf '[%s] %s\n' "$(date '+%Y-%m-%d %H:%M:%S')" "$*"
}

fail() {
    printf 'ERROR: %s\n' "$*" >&2
    exit 1
}

is_connected() {
    local container="$1"
    docker inspect -f "{{if index .NetworkSettings.Networks \"$NETWORK_NAME\"}}yes{{else}}no{{end}}" "$container" 2>/dev/null | grep -qx yes
}

ensure_network() {
    if ! docker network inspect "$NETWORK_NAME" >/dev/null 2>&1; then
        log "Creating user-defined Docker network: $NETWORK_NAME"
        docker network create "$NETWORK_NAME" >/dev/null
    else
        log "Docker network already exists: $NETWORK_NAME"
    fi
}

ensure_container_network() {
    local container="$1"
    local alias="${2:-}"

    if ! is_connected "$container"; then
        if [[ -n "$alias" ]]; then
            docker network connect --alias "$alias" "$NETWORK_NAME" "$container"
        else
            docker network connect "$NETWORK_NAME" "$container"
        fi
        log "Connected '$container' to '$NETWORK_NAME'."
        return
    fi

    if [[ -n "$alias" ]]; then
        local aliases
        aliases="$(docker inspect -f "{{range \$name, \$net := .NetworkSettings.Networks}}{{if eq \$name \"$NETWORK_NAME\"}}{{range \$net.Aliases}}{{println .}}{{end}}{{end}}{{end}}" "$container")"
        if ! grep -Fxq "$alias" <<<"$aliases"; then
            log "Refreshing network attachment to add alias '$alias' for '$container'."
            docker network disconnect "$NETWORK_NAME" "$container"
            docker network connect --alias "$alias" "$NETWORK_NAME" "$container"
        fi
    fi
}

[[ "$(id -u)" -eq 0 ]] || fail "Run this script as root (sudo ./db.sh)."
command -v docker >/dev/null 2>&1 || fail "Docker CLI is not installed."
docker info >/dev/null 2>&1 || fail "Docker daemon is not running or is inaccessible."

[[ -d "$DATA_DIR" ]] || fail "Data directory does not exist: $DATA_DIR"
[[ -d "$DATA_DIR/mysql" && -f "$DATA_DIR/ibdata1" ]] || fail \
    "MariaDB data files were not found directly in $DATA_DIR. Expected mysql/ and ibdata1."

docker inspect "$API_CONTAINER" >/dev/null 2>&1 || fail \
    "API container '$API_CONTAINER' was not found. Deploy the API container first."

ensure_network

# Ensure the API is attached even if it was recreated by an older deployment script.
ensure_container_network "$API_CONTAINER"

log "Using Docker network: $NETWORK_NAME"
log "Using MariaDB data directory: $DATA_DIR"

VOLUME_SPEC="$DATA_DIR:/var/lib/mysql"
if command -v getenforce >/dev/null 2>&1 && [[ "$(getenforce 2>/dev/null)" == "Enforcing" ]]; then
    VOLUME_SPEC="$VOLUME_SPEC:Z"
    log "SELinux is enforcing; applying a private Docker label to the data directory."
fi

if docker inspect "$CONTAINER_NAME" >/dev/null 2>&1; then
    EXISTING_SOURCE="$(docker inspect -f '{{range .Mounts}}{{if eq .Destination "/var/lib/mysql"}}{{.Source}}{{end}}{{end}}' "$CONTAINER_NAME")"
    [[ "$EXISTING_SOURCE" == "$DATA_DIR" ]] || fail \
        "Container '$CONTAINER_NAME' exists but its /var/lib/mysql mount is '$EXISTING_SOURCE', not '$DATA_DIR'. No changes were made."

    if [[ "$(docker inspect -f '{{.State.Running}}' "$CONTAINER_NAME")" != "true" ]]; then
        docker start "$CONTAINER_NAME" >/dev/null
        log "Started existing MariaDB container."
    else
        log "MariaDB container is already running."
    fi

    ensure_container_network "$CONTAINER_NAME" "mariadb"
else
    log "Pulling image $IMAGE if it is not already available."
    docker image inspect "$IMAGE" >/dev/null 2>&1 || docker pull "$IMAGE"

    docker run -d \
        --name "$CONTAINER_NAME" \
        --restart unless-stopped \
        --network "$NETWORK_NAME" \
        --network-alias mariadb \
        -e "MARIADB_ROOT_PASSWORD=$ROOT_PASSWORD" \
        -e "MARIADB_DATABASE=$DATABASE" \
        -e "MARIADB_USER=$DB_USER" \
        -e "MARIADB_PASSWORD=$DB_PASSWORD" \
        -v "$VOLUME_SPEC" \
        "$IMAGE" >/dev/null

    log "Created and started MariaDB container."
fi

log "Waiting for MariaDB to become ready (timeout: ${STARTUP_TIMEOUT}s)."
SECONDS_WAITED=0
until docker exec "$CONTAINER_NAME" mariadb-admin ping -uroot "-p$ROOT_PASSWORD" --silent >/dev/null 2>&1; do
    if [[ "$SECONDS_WAITED" -ge "$STARTUP_TIMEOUT" ]]; then
        log "MariaDB did not become ready. Recent container logs:"
        docker logs --tail 100 "$CONTAINER_NAME" || true
        fail "MariaDB startup timed out."
    fi
    sleep 3
    SECONDS_WAITED=$((SECONDS_WAITED + 3))
done

log "MariaDB is ready."
log "API container: $API_CONTAINER"
log "MariaDB container: $CONTAINER_NAME"
log "Docker network: $NETWORK_NAME"
log "MariaDB network alias: mariadb"
log "Host data directory: $DATA_DIR"
log "Database: $DATABASE"
log "No host port was published."

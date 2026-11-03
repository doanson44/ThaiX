#!/usr/bin/env bash
set -Eeuo pipefail

readonly SCRIPT_DIR="$(cd -- "$(dirname -- "\${BASH_SOURCE[0]}")" && pwd)"
readonly REPO_ROOT="$(cd -- "\${SCRIPT_DIR}/.." && pwd)"
readonly COMPOSE_FILE="\${REPO_ROOT}/docker-compose.yml"
readonly DATA_ROOT="/srv/thaix/data"
readonly DATA_DIR="\${DATA_ROOT}/mariadb"
readonly ENV_FILE="\${DATA_ROOT}/.env"

if [[ "\${EUID}" -ne 0 ]]; then
    printf 'Run this script as root: sudo %s\n' "$0" >&2
    exit 1
fi

for command in docker openssl; do
    if ! command -v "$command" >/dev/null 2>&1; then
        printf 'Required command not found: %s\n' "$command" >&2
        exit 1
    fi
done

docker compose version >/dev/null

if [[ ! -f "$COMPOSE_FILE" ]]; then
    printf 'docker-compose.yml not found: %s\n' "$COMPOSE_FILE" >&2
    exit 1
fi

install -d -m 0750 "$DATA_DIR"

if [[ ! -f "$ENV_FILE" ]]; then
    umask 077
    {
        printf 'DB_PASSWORD=%s\n' "$(openssl rand -hex 32)"
        printf 'MARIADB_DATA_DIR=%s\n' "$DATA_DIR"
    } > "$ENV_FILE"
    printf 'Created database credentials: %s\n' "$ENV_FILE"
elif ! grep -qE '^DB_PASSWORD=.+$' "$ENV_FILE"; then
    printf 'DB_PASSWORD is missing or empty in %s\n' "$ENV_FILE" >&2
    exit 1
fi

if grep -q '^MARIADB_DATA_DIR=' "$ENV_FILE"; then
    if ! grep -Fxq "MARIADB_DATA_DIR=$DATA_DIR" "$ENV_FILE"; then
        printf 'MARIADB_DATA_DIR in %s must be %s\n' "$ENV_FILE" "$DATA_DIR" >&2
        exit 1
    fi
else
    printf 'MARIADB_DATA_DIR=%s\n' "$DATA_DIR" >> "$ENV_FILE"
fi

chmod 0600 "$ENV_FILE"
export MARIADB_DATA_DIR="$DATA_DIR"

docker compose \
    --project-name thaix \
    --env-file "$ENV_FILE" \
    --file "$COMPOSE_FILE" \
    up -d mariadb

printf '\nMariaDB container status:\n'
docker compose \
    --project-name thaix \
    --env-file "$ENV_FILE" \
    --file "$COMPOSE_FILE" \
    ps mariadb

printf '\nDatabase files: %s\n' "$DATA_DIR"
printf 'Credentials file: %s (permissions 0600)\n' "$ENV_FILE"
printf 'To start the API with the same database credentials, run:\n'
printf '  docker compose --project-name thaix --env-file %q --file %q up -d\n' "$ENV_FILE" "$COMPOSE_FILE"

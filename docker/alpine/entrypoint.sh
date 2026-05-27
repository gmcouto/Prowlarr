#!/bin/sh
set -e

PUID=${PUID:-1000}
PGID=${PGID:-1000}
UMASK=${UMASK:-002}

if [ "$PUID" = "0" ]; then
  umask "$UMASK"
  cd /app/prowlarr
  exec ./Prowlarr -nobrowser -data=/config "$@"
fi

CURRENT_UID=$(id -u prowlarr)
CURRENT_GID=$(id -g prowlarr)

if [ "$PGID" != "$CURRENT_GID" ]; then
  groupmod -o -g "$PGID" prowlarr
fi
if [ "$PUID" != "$CURRENT_UID" ]; then
  usermod -o -u "$PUID" prowlarr
fi

mkdir -p /config /run/prowlarr-temp
chown -R prowlarr:prowlarr /config /run/prowlarr-temp

umask "$UMASK"
cd /app/prowlarr
exec su-exec prowlarr:prowlarr ./Prowlarr -nobrowser -data=/config "$@"

#!/bin/sh
set -e

cd /app/prowlarr
exec ./Prowlarr -nobrowser -data=/config "$@"

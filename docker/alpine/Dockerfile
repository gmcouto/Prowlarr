# syntax=docker/dockerfile:1

FROM alpine:3.21

ARG TARGETARCH

LABEL org.opencontainers.image.source="https://github.com/Prowlarr/Prowlarr"
LABEL org.opencontainers.image.description="Prowlarr on Alpine (musl), built from gmcouto-release"

ENV XDG_CONFIG_HOME="/config/xdg" \
    COMPlus_EnableDiagnostics=0 \
    TMPDIR="/run/prowlarr-temp"

RUN apk add --no-cache \
      ca-certificates \
      icu-libs \
      sqlite-libs \
      tzdata \
    && mkdir -p /app/prowlarr /config /run/prowlarr-temp

COPY docker/alpine/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

COPY _artifacts/linux-musl-x64/net8.0/Prowlarr /tmp/prowlarr-amd64
COPY _artifacts/linux-musl-arm64/net8.0/Prowlarr /tmp/prowlarr-arm64

RUN case "${TARGETARCH}" in \
      amd64) SRC=/tmp/prowlarr-amd64 ;; \
      arm64) SRC=/tmp/prowlarr-arm64 ;; \
      *) echo "Unsupported architecture: ${TARGETARCH}" && exit 1 ;; \
    esac \
    && cp -a "${SRC}/." /app/prowlarr/ \
    && rm -rf /tmp/prowlarr-amd64 /tmp/prowlarr-arm64 \
    && rm -rf /app/prowlarr/Prowlarr.Update \
    && chmod +x /app/prowlarr/Prowlarr

WORKDIR /app/prowlarr

EXPOSE 9696
VOLUME /config

ENTRYPOINT ["/entrypoint.sh"]

# Prowlarr Gaucho

This is a **community fork** of [Prowlarr](https://github.com/Prowlarr/Prowlarr) that tracks upstream stable releases and adds custom changes on top.

It is **not** the official Prowlarr project. For upstream support, use the [Servarr wiki](https://wiki.servarr.com/prowlarr) and [Prowlarr/Prowlarr](https://github.com/Prowlarr/Prowlarr).

## What this fork adds

### Cardigann language & subtitle metadata

The [`cardigann/langsubs`](../../tree/cardigann/langsubs) branch adds support for parsing `languages` and `subs` fields from Cardigann indexer definitions into release metadata.

### Alpine (musl) Docker images

Multi-arch Docker images built for **linux/amd64** and **linux/arm64**, suitable for Alpine and other musl-based systems.

## Releases

| Version | Release |
|---------|---------|
| Latest  | [Releases](../../releases/latest) |

Current stable release: **2.3.5.5327**

## Docker

Replace the version tag with the [latest release](../../releases/latest):

```bash
docker pull ghcr.io/gmcouto/prowlarr:2.3.5.5327
```

```yaml
services:
  prowlarr:
    image: ghcr.io/gmcouto/prowlarr:2.3.5.5327
    container_name: prowlarr
    ports:
      - "9696:9696"
    volumes:
      - ./config:/config
    restart: unless-stopped
```

### Image tags

| Tag | Description |
|-----|-------------|
| `2.3.5.5327` | Pinned upstream version (recommended) |
| `latest` | Latest stable release from this fork |
| `alpine` | Alias for the current Alpine musl build |

Registry: `ghcr.io/gmcouto/prowlarr`

## Repository branches

| Branch | Purpose |
|--------|---------|
| `disclaimer` | This page — fork documentation and release automation |
| [`gmcouto-release`](../../tree/gmcouto-release) | Shippable code (upstream release + customizations) |
| [`gmcouto/infra`](../../tree/gmcouto/infra) | Docker and build workflow overlay |
| [`cardigann/langsubs`](../../tree/cardigann/langsubs) | Cardigann language/subtitle feature |

## How releases are built

1. A scheduled workflow checks [Prowlarr/Prowlarr](https://github.com/Prowlarr/Prowlarr) for new **stable** releases.
2. When a new version is found, upstream code is merged with `gmcouto/infra` and `cardigann/langsubs` on `gmcouto-release`.
3. A version tag (e.g. `v2.3.5.5327`) triggers the Alpine build and publishes the Docker image to GHCR.

## Upstream

- **Source:** https://github.com/Prowlarr/Prowlarr
- **Docs:** https://wiki.servarr.com/prowlarr

#!/bin/bash
# Build the celeste-wasm self-hosted server package via Docker
#
# This script:
#   1. Builds the frontend inside Docker (dotnet + emsdk + pnpm)
#   2. Assembles a server package with the built frontend + game assets
#   3. Either runs it as a Docker container OR extracts it for native Node.js use
#
# Prerequisites:
#   - Docker installed
#   - celeste-wasm.tar at the project root (game assets)
#   - Optionally: docker-assets/Celeste.exe for automatic patching
#
# Usage:
#   ./scripts/docker-build-server.sh              # Build + run as Docker container
#   ./scripts/docker-build-server.sh --extract     # Build + extract package for native use
#   ./scripts/docker-build-server.sh --build-only  # Just build the Docker image
#   ./scripts/docker-build-server.sh --help        # Show this help

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

IMAGE_NAME="celeste-wasm-server"
CONTAINER_PORT=8080
HOST_PORT=8080
EXTRACT_DIR="$PROJECT_ROOT/celeste-wasm-server"

# Parse arguments
ACTION="run"
for arg in "$@"; do
    case $arg in
        --extract)
            ACTION="extract"
            ;;
        --build-only)
            ACTION="build-only"
            ;;
        --port=*)
            HOST_PORT="${arg#*=}"
            ;;
        --help|-h)
            echo ""
            echo "  celeste-wasm server builder"
            echo "  ---------------------------"
            echo ""
            echo "  Usage: $0 [options]"
            echo ""
            echo "  Options:"
            echo "    --extract       Build and extract package to celeste-wasm-server/"
            echo "                    (for running natively with 'node server.mjs')"
            echo "    --build-only    Just build the Docker image, don't run or extract"
            echo "    --port=PORT     Host port to bind when running (default: 8080)"
            echo "    --help, -h      Show this help"
            echo ""
            echo "  Default (no flags): Build the image and run the container on port $HOST_PORT"
            echo ""
            echo "  Prerequisites:"
            echo "    - celeste-wasm.tar must exist at the project root"
            echo "    - Optionally place Celeste.exe in docker-assets/ for auto-patching"
            echo ""
            exit 0
            ;;
    esac
done

cd "$PROJECT_ROOT"

# ---------------------------------------------------------------------------
# Validate prerequisites
# ---------------------------------------------------------------------------
echo ""
echo "  celeste-wasm server builder"
echo "  ---------------------------"
echo ""

if [ ! -f "celeste-wasm.tar" ]; then
    echo "  Error: celeste-wasm.tar not found at project root."
    echo "  This file contains the game assets (Content/, CustomCeleste.dll, etc.)"
    exit 1
fi

# Ensure docker-assets directory exists (even if empty — Dockerfile COPY needs it)
if [ ! -d "docker-assets" ]; then
    echo "  Note: docker-assets/ not found, creating empty directory."
    echo "  (Place Celeste.exe there for automatic patching)"
    mkdir -p docker-assets
fi

# ---------------------------------------------------------------------------
# Build the Docker image
# ---------------------------------------------------------------------------
echo "  Building Docker image (this may take a while on first run)..."
echo "  Platform: linux/amd64 (required for emsdk)"
echo ""

docker build \
    --platform linux/amd64 \
    -f Dockerfile.server \
    -t "$IMAGE_NAME:latest" \
    .

echo ""
echo "  Docker image built successfully: $IMAGE_NAME:latest"
echo ""

# ---------------------------------------------------------------------------
# Action: run, extract, or build-only
# ---------------------------------------------------------------------------
case $ACTION in
    build-only)
        echo "  Image ready. You can run it with:"
        echo "    docker run -p $HOST_PORT:8080 $IMAGE_NAME"
        echo ""
        echo "  Or extract the package with:"
        echo "    $0 --extract"
        ;;

    extract)
        echo "  Extracting server package to $EXTRACT_DIR..."
        rm -rf "$EXTRACT_DIR"

        # Create a temporary container and copy files out
        docker create --name celeste-wasm-tmp "$IMAGE_NAME:latest" > /dev/null 2>&1
        docker cp celeste-wasm-tmp:/app "$EXTRACT_DIR"
        docker rm celeste-wasm-tmp > /dev/null 2>&1

        echo ""
        echo "  Package extracted to: celeste-wasm-server/"
        echo ""
        echo "  To run (requires Node.js):"
        echo "    cd celeste-wasm-server"
        echo "    node server.mjs"
        echo ""
        echo "  Options:"
        echo "    node server.mjs --port 9000       # custom port"
        echo "    node server.mjs --no-open         # don't auto-open browser"
        echo "    node server.mjs --help            # all options"
        echo ""
        ;;

    run)
        echo "  Starting server on port $HOST_PORT..."
        echo ""
        echo "  Local:   http://localhost:$HOST_PORT"
        echo ""
        echo "  Press Ctrl+C to stop."
        echo ""

        docker run --rm \
            -p "$HOST_PORT:8080" \
            --name celeste-wasm-server \
            "$IMAGE_NAME:latest"
        ;;
esac

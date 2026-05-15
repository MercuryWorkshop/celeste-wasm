#!/bin/bash
# Build the celeste-wasm self-hosted server package
#
# Prerequisites:
#   - frontend/dist/ must exist (run "make publish" first)
#   - celeste-wasm.tar must exist in the project root
#
# Output: celeste-wasm-server/ directory ready to distribute
#   Run with: cd celeste-wasm-server && node server.mjs

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DIST_DIR="$PROJECT_ROOT/frontend/dist"
TAR_FILE="$PROJECT_ROOT/celeste-wasm.tar"
SERVER_SCRIPT="$PROJECT_ROOT/server/server.mjs"
OUTPUT_DIR="$PROJECT_ROOT/celeste-wasm-server"

echo ""
echo "  Building celeste-wasm server package"
echo "  -------------------------------------"
echo ""

# Validate prerequisites
if [ ! -d "$DIST_DIR" ] || [ ! -f "$DIST_DIR/index.html" ]; then
    echo "  Error: frontend/dist/ not found or empty."
    echo "  Run 'make publish' first to build the frontend."
    exit 1
fi

if [ ! -f "$TAR_FILE" ]; then
    echo "  Error: celeste-wasm.tar not found at project root."
    echo "  Ensure the game assets tar file exists."
    exit 1
fi

if [ ! -f "$SERVER_SCRIPT" ]; then
    echo "  Error: server/server.mjs not found."
    exit 1
fi

# Clean and create output directory
echo "  Cleaning output directory..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Copy built frontend
echo "  Copying frontend dist/..."
cp -r "$DIST_DIR" "$OUTPUT_DIR/dist"

# Copy game assets tar
echo "  Copying celeste-wasm.tar (~1.3GB, this may take a moment)..."
cp "$TAR_FILE" "$OUTPUT_DIR/celeste-wasm.tar"

# Copy server script
echo "  Copying server.mjs..."
cp "$SERVER_SCRIPT" "$OUTPUT_DIR/server.mjs"
chmod +x "$OUTPUT_DIR/server.mjs"

echo ""
echo "  Package ready at: celeste-wasm-server/"
echo ""
echo "  To run:"
echo "    cd celeste-wasm-server"
echo "    node server.mjs"
echo ""
echo "  Options:"
echo "    node server.mjs --port 9000       # custom port"
echo "    node server.mjs --no-open         # don't auto-open browser"
echo "    node server.mjs --help            # all options"
echo ""

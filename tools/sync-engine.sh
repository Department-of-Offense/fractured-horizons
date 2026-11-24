#!/usr/bin/env bash
# Vendor addons/command_engine from the engine repo into this game.
#
#   tools/sync-engine.sh v0.2.0            # a tagged release from GitHub (normal use)
#   tools/sync-engine.sh main              # latest engine main (try before tagging)
#   ENGINE_PATH=../command-engine tools/sync-engine.sh   # a local engine checkout (engine dev)
#
# Writes ENGINE_VERSION so everyone can see which engine build the game is on.
# Commit the result: the game repo must build with no extra setup.
set -euo pipefail

REPO_URL="${ENGINE_REPO:-https://github.com/Department-of-Offense/command-engine.git}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DEST="$ROOT/addons/command_engine"

if [[ -n "${ENGINE_PATH:-}" ]]; then
  SRC="$(cd "$ENGINE_PATH" && pwd)"
  REF="local:$(git -C "$SRC" describe --tags --always --dirty 2>/dev/null || echo unknown)"
  SHA="$(git -C "$SRC" rev-parse HEAD 2>/dev/null || echo unknown)"
else
  REF="${1:?usage: tools/sync-engine.sh <tag-or-branch>   (or set ENGINE_PATH)}"
  TMP="$(mktemp -d)"
  trap 'rm -rf "$TMP"' EXIT
  git -c advice.detachedHead=false clone --quiet --depth 1 --branch "$REF" "$REPO_URL" "$TMP/engine"
  SRC="$TMP/engine"
  SHA="$(git -C "$SRC" rev-parse HEAD)"
fi

if [[ ! -d "$SRC/addons/command_engine" ]]; then
  echo "error: $SRC has no addons/command_engine" >&2
  exit 1
fi

rm -rf "$DEST"
mkdir -p "$(dirname "$DEST")"
cp -R "$SRC/addons/command_engine" "$DEST"
printf '%s\n%s\n' "$REF" "$SHA" > "$ROOT/ENGINE_VERSION"

echo "Engine synced: $REF ($SHA)"
echo "Next: dotnet build, run the tests, then commit addons/command_engine and ENGINE_VERSION."

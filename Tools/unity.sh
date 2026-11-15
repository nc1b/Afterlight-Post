#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
method="${1:?Usage: unity.sh <editor-method> [Unity arguments]}"
shift
mkdir -p "$project_root/Logs"
"${UNITY_EDITOR:-Unity.exe}" -batchmode -nographics -quit -buildTarget Win64 \
  -projectPath "$project_root" -executeMethod "$method" \
  -logFile "$project_root/Logs/unity-$method.log" "$@"

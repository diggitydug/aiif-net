#!/usr/bin/env bash
set -euo pipefail

# Build and pack Aiif.Net into a local feed directory.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
PROJECT_PATH="${REPO_ROOT}/src/Aiif.Net/Aiif.Net.csproj"
DEFAULT_OUTPUT="${REPO_ROOT}/artifacts/local-packages"
OUTPUT_DIR="${1:-${DEFAULT_OUTPUT}}"

echo "Packing Aiif.Net from: ${PROJECT_PATH}"
echo "Local NuGet feed output: ${OUTPUT_DIR}"

mkdir -p "${OUTPUT_DIR}"

dotnet restore "${PROJECT_PATH}"
dotnet build "${PROJECT_PATH}" -c Release --no-restore
dotnet pack "${PROJECT_PATH}" -c Release -o "${OUTPUT_DIR}" --no-build -p:GeneratePackageOnBuild=false

echo "Done. Packages available in: ${OUTPUT_DIR}"
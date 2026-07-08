#!/usr/bin/env bash
set -euo pipefail

# Bash equivalent of BuildModule.ps1 build flow.
# This script only builds and generates the module zip in the base directory.

MSBUILD_ARG=""
FRAMEWORK_ARG=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --msbuild)
      MSBUILD_ARG="${2:-}"
      shift 2
      ;;
    --framework)
      FRAMEWORK_ARG="${2:-}"
      shift 2
      ;;
    -h|--help)
      cat <<'EOF'
Usage: ./BuildModule.sh [--msbuild <path-or-command>] [--framework <value>]

Options:
  --msbuild    Path/command to MSBuild (optional)
  --framework  Accepted for parity with BuildModule.ps1 (currently unused)
EOF
      exit 0
      ;;
    *)
      echo "Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

MSBUILD_CMD=()

find_msbuild() {
  local candidates=(
    "/c/Program Files (x86)/Microsoft Visual Studio/2019/BuildTools/MSBuild/Current/Bin/msbuild.exe"
    "/c/Program Files (x86)/Microsoft Visual Studio/2019/Enterprise/MSBuild/Current/Bin/msbuild.exe"
    "/c/Program Files (x86)/Microsoft Visual Studio/2019/Professional/MSBuild/Current/Bin/msbuild.exe"
    "/c/Program Files (x86)/Microsoft Visual Studio/2017/Professional/MSBuild/15.0/Bin/msbuild.exe"
    "/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/msbuild.exe"
    "/c/Program Files (x86)/Microsoft Visual Studio/2022/BuildTools/MSBuild/Current/Bin/msbuild.exe"
    "/mnt/c/Program Files (x86)/Microsoft Visual Studio/2019/BuildTools/MSBuild/Current/Bin/msbuild.exe"
    "/mnt/c/Program Files (x86)/Microsoft Visual Studio/2019/Enterprise/MSBuild/Current/Bin/msbuild.exe"
    "/mnt/c/Program Files (x86)/Microsoft Visual Studio/2019/Professional/MSBuild/Current/Bin/msbuild.exe"
    "/mnt/c/Program Files (x86)/Microsoft Visual Studio/2017/Professional/MSBuild/15.0/Bin/msbuild.exe"
    "/mnt/c/Program Files (x86)/Microsoft Visual Studio/2019/Community/MSBuild/Current/Bin/msbuild.exe"
    "/mnt/c/Program Files (x86)/Microsoft Visual Studio/2022/BuildTools/MSBuild/Current/Bin/msbuild.exe"
  )

  for guess in "${candidates[@]}"; do
    if [[ -f "$guess" ]]; then
      MSBUILD_CMD=("$guess")
      return
    fi
  done

  if command -v msbuild >/dev/null 2>&1; then
    MSBUILD_CMD=("msbuild")
    return
  fi

  if command -v dotnet >/dev/null 2>&1; then
    MSBUILD_CMD=("dotnet" "msbuild")
    return
  fi

  echo "Unable to find MSBuild. Pass --msbuild <path-or-command>." >&2
  exit 1
}

resolve_msbuild() {
  if [[ -n "$MSBUILD_ARG" ]]; then
    echo "Using $MSBUILD_ARG"
    MSBUILD_CMD=("$MSBUILD_ARG")
    return
  fi

  echo "Looking for MSBuild"
  find_msbuild
  echo "Found and trying: ${MSBUILD_CMD[*]}"
}

get_compile_target() {
  local base_path="$1"
  local guess="$base_path/build.proj"

  if [[ -f "$guess" ]]; then
    echo "$guess"
    return
  fi

  echo "Could not find a build.proj file, please create one." >&2
  exit 1
}

find_solution_file() {
  local base_path="$1"
  local sln

  shopt -s nullglob
  local matches=("$base_path"/*.sln)
  shopt -u nullglob

  if [[ ${#matches[@]} -eq 0 ]]; then
    echo "Can not find *.sln file" >&2
    exit 1
  fi

  sln="${matches[0]}"
  echo "$sln"
}

find_module_name() {
  local build_proj="$1"

  local cmdline
  cmdline=$(tr '\n' ' ' < "$build_proj" | sed -n 's/.*<Target Name="build_module">.*<Exec Command="\([^"]*\)".*/\1/p')
  cmdline=${cmdline//&quot;/\"}

  if [[ -z "$cmdline" ]]; then
    echo "Could not locate build_module Exec command in $build_proj" >&2
    exit 1
  fi

  read -r -a parts <<< "$cmdline"
  local i
  for ((i = 0; i < ${#parts[@]}; i++)); do
    if [[ "${parts[$i]}" == "-buildmodule" ]]; then
      if (( i + 1 < ${#parts[@]} )); then
        echo "${parts[$((i + 1))]}"
        return
      fi
    fi
  done

  echo "Could not parse module name from build_module command in $build_proj" >&2
  exit 1
}

resolve_msbuild

base_path="$(pwd)"
echo "Using basePath = $base_path"

if [[ -n "$FRAMEWORK_ARG" ]]; then
  echo "Framework argument provided (not used): $FRAMEWORK_ARG"
fi

echo "Compiling project by build.proj, or by .sln file."
compile_target="$(get_compile_target "$base_path")"
solution="$(find_solution_file "$base_path")"
module_name="$(find_module_name "$compile_target")"

"${MSBUILD_CMD[@]}" -t:restore "$solution"

if [[ "${MSBUILD_CMD[*]}" == "dotnet msbuild" ]]; then
  # build.proj uses Windows-style paths in Exec commands; run equivalent cross-platform steps.
  dotnet publish "./$module_name/$module_name.csproj" --runtime win-x64 --self-contained false --output "./$module_name/obj" -c Debug
  dotnet tool uninstall --global CreateDecisionsModule-GlobalTool >/dev/null 2>&1 || true
  dotnet tool update --global CreateDecisionsModule-GlobalTool
  CreateDecisionsModule -buildmodule "$module_name" -output "." -buildfile "Module.Build.json"
else
  "${MSBUILD_CMD[@]}" "$compile_target"
fi

module_zip="$base_path/$module_name.zip"

if [[ ! -f "$module_zip" ]]; then
  echo "Build completed but module zip was not found: $module_zip" >&2
  exit 1
fi

echo "Module generated: $module_zip"

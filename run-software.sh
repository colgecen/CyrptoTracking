#!/usr/bin/env bash
set -e
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
export LIBGL_ALWAYS_SOFTWARE=1
export GALLIUM_DRIVER=llvmpipe
export SKIA_USE_VULKAN=0
export AVALONIA_USE_VULKAN=0
cd "$(dirname "$0")"
dotnet run --project AvaloniaAppCrypto.csproj "$@"

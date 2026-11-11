#!/usr/bin/env bash
set -e
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
export AVALONIA_USE_X11=1
export AVALONIA_USE_WAYLAND=0
cd "$(dirname "$0")"
dotnet run --project AvaloniaAppCrypto.csproj "$@"
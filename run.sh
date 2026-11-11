#!/usr/bin/env bash
set -e
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
cd "$(dirname "$0")"
dotnet run --project AvaloniaAppCrypto.csproj "$@"
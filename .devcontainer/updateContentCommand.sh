#!/usr/bin/env sh
set -eu

# Don't fail container creation on .NET restore errors
dotnet restore || exit 0

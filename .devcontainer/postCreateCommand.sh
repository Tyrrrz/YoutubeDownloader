#!/usr/bin/env sh
set -eu

# CSharpier needs to be manually installed as a .NET tool
# for its VS Code extension to work.
dotnet tool update -g csharpier || dotnet tool install -g csharpier

#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../src/ZawkMapper.Benchmarks"
dotnet restore
dotnet run -- seed --customers 100000 --orders 100000 --items 200000
dotnet run -- compare --take 100000

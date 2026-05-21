Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Push-Location "$PSScriptRoot/../src/ZawkMapper.Benchmarks"
try {
    dotnet restore
    dotnet run -- seed --customers 100000 --orders 100000 --items 200000
    dotnet run -- compare --take 100000
}
finally {
    Pop-Location
}

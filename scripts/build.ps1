$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
dotnet restore "$root\DesktopPetDog.sln"
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }
dotnet test "$root\DesktopPetDog.sln" -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet test failed with exit code $LASTEXITCODE" }
dotnet publish "$root\src\DesktopPetDog\DesktopPetDog.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "$root\publish"
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }
Write-Host "EXE created at $root\publish\DesktopPetDog.exe"

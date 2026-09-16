$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
dotnet restore "$root\DesktopPetDog.sln"
dotnet test "$root\DesktopPetDog.sln" -c Release --no-restore
dotnet publish "$root\src\DesktopPetDog\DesktopPetDog.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "$root\publish"
Write-Host "EXE created at $root\publish\DesktopPetDog.exe"

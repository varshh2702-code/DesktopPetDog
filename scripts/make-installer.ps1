$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
& "$PSScriptRoot\build.ps1"
$iscc = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (!(Test-Path $iscc)) { throw "Install Inno Setup 6, then run this script again." }
& $iscc "$root\installer\DesktopPetDog.iss"
Write-Host "Installer created at $root\artifacts\DesktopPetDog-Setup.exe"

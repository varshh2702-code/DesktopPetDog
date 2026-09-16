$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$forbidden = 'GetWindowText|ProcessName|Clipboard|GetAsyncKeyState|CopyFromScreen|WebClient|HttpClient|MediaCapture|SpeechRecognition|KeyboardHook'
$hits = Get-ChildItem "$root\src" -Recurse -File | Select-String -Pattern $forbidden
if ($hits) { $hits | Format-Table; throw 'Privacy-sensitive API token found; review required.' }
Write-Host 'PASS: no prohibited monitoring/network API tokens found.'

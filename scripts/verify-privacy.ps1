$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$forbidden = 'GetWindowText\s*\(|\.ProcessName\b|Clipboard\s*\.|GetAsyncKeyState\s*\(|CopyFromScreen\s*\(|new\s+WebClient\b|new\s+HttpClient\b|MediaCapture\b|SpeechRecognition\b|KeyboardHook\b'
$hits = Get-ChildItem "$root\src" -Recurse -File | Select-String -Pattern $forbidden
if ($hits) { $hits | Format-Table; throw 'Privacy-sensitive API token found; review required.' }
Write-Host 'PASS: no prohibited monitoring/network API tokens found.'

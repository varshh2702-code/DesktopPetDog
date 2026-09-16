# Desktop Pet Dog

A privacy-first animated dog for Windows 10 and Windows 11. It is a standalone WPF desktop program: it remains available above the desktop, browsers, folders, and study apps, and it does not depend on a browser or internet connection.

## Why C# and WPF

WPF is the best fit here because its transparent, borderless windows and hardware-accelerated vector animations are mature on Windows. C# also provides direct access to the system tray, per-monitor DPI, hotkey registration, startup registry entry, and foreground-window geometry without extra runtime services. A self-contained single-file publish does not require Python or .NET to be preinstalled on the user's computer.

## Project structure

```text
DesktopPetDog/
├── DesktopPetDog.sln
├── README.md
├── LICENSE
├── .github/workflows/
│   └── build-windows-installer.yml
├── assets/
│   └── REPLACING_ASSETS.md
├── installer/
│   └── DesktopPetDog.iss
├── scripts/
│   ├── build.ps1
│   ├── make-installer.ps1
│   └── verify-privacy.ps1
├── src/DesktopPetDog/
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── DesktopPetDog.csproj
│   ├── app.manifest
│   ├── Dialogs.cs
│   ├── DogView.cs
│   ├── MainWindow.cs
│   ├── MessageLibrary.cs
│   ├── Models.cs
│   └── Services.cs
└── tests/DesktopPetDog.Tests/
    ├── CoreTests.cs
    └── DesktopPetDog.Tests.csproj
```

The main dog is an original transparent cream-golden retriever puppy illustration inspired by the supplied breed reference. `DogView.cs` also contains a programmatic vector fallback, so a missing or damaged optional image cannot crash the pet.

## Beginner installation (run from source)

1. On a Windows 10/11 x64 computer, install the **.NET 8 SDK** from Microsoft's official .NET site.
2. Download and unzip this project.
3. Open PowerShell in the `DesktopPetDog` folder.
4. Run:

```powershell
dotnet restore .\DesktopPetDog.sln
dotnet run --project .\src\DesktopPetDog\DesktopPetDog.csproj
```

No administrator permission is needed. Windows may show a first-run reputation warning for an unsigned locally built `.exe`; code-signing is optional for personal use and recommended before public distribution.

## Build, test, and package as an `.exe`

From the project root:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\scripts\verify-privacy.ps1
.\scripts\build.ps1
```

The self-contained Windows executable is written to `publish\DesktopPetDog.exe`. It works offline and does not require a separately installed .NET runtime.

Manual equivalents:

```powershell
dotnet restore .\DesktopPetDog.sln
dotnet test .\DesktopPetDog.sln -c Release
dotnet publish .\src\DesktopPetDog\DesktopPetDog.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

## Create the installer and uninstaller

1. Install **Inno Setup 6** from its official site.
2. Run:

```powershell
.\scripts\make-installer.ps1
```

The installer appears at `artifacts\DesktopPetDog-Setup.exe`. It installs per-user under `%LOCALAPPDATA%\Programs\DesktopPetDog`, so elevation is not required.

Alternatively, push the project to GitHub and open **Actions → Build Windows Installer → Run workflow**. The Windows runner tests the project, creates the installer, and publishes a downloadable artifact named `DesktopPetDog-Windows-Installer`.

The uninstaller:

- stops `DesktopPetDog.exe` and waits for it to end;
- removes the application and installer files;
- removes Start-menu and optional desktop shortcuts;
- removes the `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\DesktopPetDog` startup value;
- asks whether to keep settings;
- if **No** is chosen, recursively deletes `%LOCALAPPDATA%\DesktopPetDog`, including preferences, position, optional mood answer, shuffle history, cache, and error log;
- installs no service, scheduled task, driver, browser helper, or separate updater, so no hidden background component remains.

If the user chooses **Yes**, only the clearly requested settings/history folder remains for a future reinstall. Choosing **No** performs complete removal.

## Controls

- Single-click: tail wag and speech bubble.
- Double-click: playful jump.
- Drag: move the pet and save its position.
- Right-click: full pet menu.
- Default hotkey: `Ctrl+Alt+D` toggles visibility using Windows `RegisterHotKey`; it does not log keys.
- Tray icon: show/hide, Quiet, Focus, Pause, Settings, and Exit.

Use **Exit** in the pet or tray menu to close the process completely. Closing Settings does not exit the pet.

## Local storage

Only these local files may be created in `%LOCALAPPDATA%\DesktopPetDog`:

| Path | Purpose |
|---|---|
| `settings.json` | Name, colour, accessory, size, frequencies, modes, hotkey, position, and optional last mood |
| `history.json` | Per-category shuffle indexes and recent indexes, to prevent repetition |
| `Cache\` | Reserved for replaceable animation caching; empty in the built-in vector version |
| `error.log` | Exception type/message if an unexpected local error occurs |

Settings → **View Exactly What Is Stored** displays settings and shuffle history. **Delete All Pet Data** removes the entire folder. Turning off **Save preferences locally** deletes existing saved pet data and prevents further settings/history writes for that session. A mood answer is saved only after a separate Yes/No confirmation.

## Permissions used

| Permission/capability | Why | Scope |
|---|---|---|
| Ordinary per-user window | Display the dog and speech bubble | No elevation |
| `%LOCALAPPDATA%` file access | Optional preferences/history/cache/log | This app's own folder only |
| HKCU Run registry value | Optional start with Windows | Current user only; user-controlled |
| `RegisterHotKey` | One selected show/hide shortcut | Selected combination only |
| Foreground window handle and rectangles | Compare dimensions for full-screen mode | No title/process/content is queried |
| Monitor/work-area geometry | Keep pet visible and away from taskbars | Geometry only |

It requests no camera, microphone, location, contacts, clipboard, accessibility, screen capture, network, or administrator permission.

## Privacy checklist

- [x] No browsing history, tabs, titles, URLs, or website contents.
- [x] No application/process names or window titles.
- [x] No file/document, email, message, password, or clipboard access.
- [x] No camera, microphone, contacts, or location access.
- [x] No screenshots, screen recording, OCR, or screen analysis.
- [x] No keylogging; only the chosen `RegisterHotKey` combination.
- [x] No mouse monitoring outside the pet window.
- [x] No telemetry, advertising, analytics, trackers, server, or network calls.
- [x] No control of other applications or browser windows.
- [x] Mood is adapted only from voluntary answers.
- [x] Full-screen detection reads dimensions only.
- [x] No service, driver, updater, or hidden helper process.

## Test plan

Run automated tests with `dotnet test .\DesktopPetDog.sln -c Release`. The tests verify every message category contains at least 100 unique messages and the shuffle bag avoids an immediate repeat.

Perform this Windows acceptance checklist on at least one Windows 10 and one Windows 11 machine:

1. Start, Exit, and reopen; confirm only one pet instance can run.
2. Confirm the dog appears on the desktop and above Chrome/Edge/Explorer, then remains after all browsers close.
3. Drag to each monitor and each edge; reconnect/disconnect a monitor and change display scaling; confirm the pet returns inside a work area and avoids the taskbar.
4. Test click, double-click, right-click, bubble dismissal, tray commands, and the selected hotkey.
5. Test Quiet, Focus with a one-minute duration, Do Not Disturb, Pause, Stay Here, Hide, and Exit.
6. Open a full-screen video/presentation; confirm the selected quiet/hide behavior and automatic return. Review source or use a Windows API monitor to confirm only geometry APIs are invoked.
7. Draw 100+ messages per category; confirm no immediate repeat and a reshuffle after a cycle.
8. Save settings, restart, and confirm restoration. Disable saving and confirm the data folder disappears.
9. Save a mood answer with Yes, then answer with No and confirm the second answer is not written.
10. Disconnect the network and confirm every feature continues to work.
11. Run the installer without administrator rights. Enable startup, verify it, then uninstall twice: once keeping settings and once deleting settings. Confirm the process, install directory, shortcut, startup value, and—when deletion is chosen—data folder are absent.
12. Run `Get-Process DesktopPetDog -ErrorAction SilentlyContinue` after Exit/uninstall; it should return nothing.

## Common errors

- **`dotnet` is not recognized:** install the .NET 8 SDK (not only the runtime), reopen PowerShell, and run `dotnet --info`.
- **Windows blocks the unsigned build:** choose “More info → Run anyway” only if you built it yourself or trust the distributor. For distribution, sign both installer and `.exe` with a trusted code-signing certificate.
- **Hotkey does nothing:** another application may own that combination. Choose a different combination in Settings.
- **Pet is missing after monitor changes:** use the tray icon → Show Pet. The next geometry check clamps it into a visible work area.
- **Inno Setup compiler not found:** install Inno Setup 6 in its default location or edit `$iscc` in `scripts\make-installer.ps1`.
- **Damaged JSON settings:** the app falls back to safe defaults. Use Delete All Pet Data to remove the damaged file.
- **Animation artwork is missing:** the default artwork is compiled vector geometry and cannot be missing. Replacement assets should keep the vector fallback enabled.

## Current visual scope

The included pet is a polished, original cream-golden retriever puppy with floppy ears, soft pale fur, dark eyes, black nose, large paws and a gentle head tilt. It supports wag/body-rock, jump, stretch/look, blink, sleep/breathing, click, drag, entry and mood-driven behaviour. A code-drawn dog remains as the automatic fallback. For richer frame-by-frame walk/run/yawn/play sequences, follow `assets\REPLACING_ASSETS.md`.

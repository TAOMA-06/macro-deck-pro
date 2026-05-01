# Macro Deck Pro

![Platform](https://img.shields.io/badge/platform-Windows-blue?style=for-the-badge)
![License](https://img.shields.io/github/license/TAOMA-06/macro-deck-pro?style=for-the-badge)
![.NET](https://img.shields.io/badge/.NET-8.0–Windows-blue?style=for-the-badge)

**Macro Deck Pro** is an enhanced, enterprise-grade fork of the open-source [Macro Deck 2](https://github.com/Macro-Deck-org/Macro-Deck), a Windows macro pad application that works with companion apps on Android, iOS, and web.

---

## Companion App

<a href="https://play.google.com/store/apps/details?id=com.suchbyte.macrodeck"><img src="https://play.google.com/intl/en_us/badges/static/images/badges/en_badge_web_generic.png" width="180"/></a>

**Repository**: [SuchByte/Macro-Deck-Client](https://github.com/SuchByte/Macro-Deck-Client)

---

## What is Macro Deck Pro?

Macro Deck Pro is a **greatly enhanced version** of the original Macro Deck 2 open-source project. While the original Macro Deck supports Wi-Fi connectivity only, Macro Deck Pro introduces **USB ADB tethering**, **SSL/TLS encryption**, **full backup & restore**, **system notifications**, **QR-code quick pairing**, and many other enterprise-grade features.

Macro Deck Pro maintains **full compatibility** with all existing Macro Deck plugins, extensions, web clients, and companion apps.

---

## Features

### 🔌 ADB USB Tethering *(Not in original)*
- Full ADB server integration — plug in your Android device via USB and Macro Deck Pro auto-detects it
- Automatic reverse port forwarding (`tcp:8191`) on USB connect
- Optional auto-launch of the Macro Deck Android app on device connection
- Sends `KEYCODE_WAKEUP` on connect, `KEYCODE_SLEEP` + force-stop on disconnect
- Bundled Android Debug Bridge (platform-tools 33.0.1)
- All managed through Settings with **Enable ADB** and **Auto-start app on USB** toggles

### 🔐 SSL/TLS Certificate Support *(Not in original)*
- Enterprise-grade HTTPS support via Kestrel with X509Certificate2
- PFX certificate files with optional password protection
- HTTPS redirection middleware
- Full Settings UI: enable checkbox, certificate browser, password field, apply button

### 💾 Full Backup & Restore System *(Not in original)*
- Creates compressed ZIP archives containing:
  - Configuration, Profiles, Variables
  - Plugin configurations & credentials
  - Installed icon packs
  - Device registry
- **Granular restore** — choose exactly which components to restore via 8 individual checkboxes
- Auto-restore detection on startup
- Backup list in Settings with date, size, and delete options

### 🔔 Notification System *(Not in original)*
- Plugin notifications via `NotificationManager.Notify(plugin, ...)`
- System notifications for Macro Deck itself (e.g., update alerts, ADB errors, backup completion)
- Windows system tray balloon tip integration
- Notifications can include **custom action buttons**
- Per-sender limit of 5 notifications to avoid spam

### 📱 Multi-Device Management *(Not in original)*
- Persistent **known devices** registry saved to `devices.json`
- Device types: `Unknown`, `Web`, `Android`, `iOS`, `StreamDeck`
- Per-device configuration: Brightness, Auto-Connect, WakeLock method
- Connection policies: Allow all / Ask on new / Block all
- Device rename, block/allow individual devices, profile assignment
- Real-time connection status tracking

### 🌐 Modern Angular Web Client *(Not in original)*
- **Self-hosted** PWA web client in `MacroDeck/wwwroot/client/` (compiled Angular SPA)
- Service Worker + web manifest for installable offline-capable app
- English + Simplified Chinese (`client.js`) built-in
- Automatic reconnection with retry logic
- Dark/Light theme toggle, fullscreen mode
- Recent connection history storage
- Includes `noSleep.js` to prevent device sleep during use

### 🔄 Auto-Update Service *(Not in original)*
- Periodic update checks every 30 minutes
- SHA-256 hash verification of downloaded installers
- Silent installation via Inno Setup `/SILENT` mode
- Optional **Beta channel** for pre-release versions
- System notification when updates are available
- Manual "Check for Updates Now" button in Settings
- Update API via `update.api.macro-deck.app/v2`

### 📷 QR Code Quick Setup *(Not in original)*
- One-click QR code generation via `QrCodeService.cs` (QRCoder library)
- Encodes: computer name, network interfaces, port, SSL status, auto-accept token
- Displayed directly on the deck view with toggle
- Quick connect data via `QuickConnectQrCodeData.cs`

### 🔥 Additional Features
| Feature | Description |
|---------|-------------|
| **Named Pipe IPC** | Single-instance enforcement; second instance sends "show" to existing instance |
| **Global Hotkeys** | System-wide hotkey registration with modifier key support |
| **Internal Device Actions** | `SetProfileAction`, `SetBrightnessAction` built into DevicePlugin |
| **Enhanced Extension Store** | "Update All" button, icon pack update checks, REST API v2 |
| **WakeLock Control** | Always / Connected / Never screen sleep strategies |
| **Custom Cottle Functions** | `getdatetime`, `gettimestamp`, `gettimerend`, `_trimblank_` |
| **Debug Console** | `--debug-console` flag opens dedicated debug console |
| **Professional Installer** | Inno Setup with 25+ languages, VC++ redist, firewall rules, ADB bundle |
| **Extended CLI Options** | `--port`, `--portable`, `--log-level`, `--disable-file-logging`, `--force-update`, `--test-channel` |

---

## Tech Stack

| Component | Technology |
|-----------|------------|
| Runtime | .NET 8.0 (Windows, `net8.0-windows10.0.22000.0`) |
| UI Framework | Windows Forms (WinForms) |
| Web Server | Kestrel |
| ADB Client | [AdvancedSharpAdbClient](https://www.nuget.org/packages/AdvancedSharpAdbClient) v3.3.12 |
| QR Codes | [QRCoder](https://www.nuget.org/packages/QRCoder) v1.5.1 |
| Template Engine | [Cottle](https://www.nuget.org/packages/Cottle) v2.0.10 |
| Image Processing | Magick.NET Q16-x64 |
| Database | sqlite-net-pcl |
| JSON | Newtonsoft.Json v13.0.3 |
| Installer | Inno Setup 6 |

---

## Project Structure

```
MacroDeck.sln
├── MacroDeck/                    # Main Windows Forms application
│   ├── Server/                   # AdbServerHelper, MacroDeckServer
│   ├── Configuration/             # MainConfiguration, config models
│   ├── Backup/                    # BackupManager, restore models
│   ├── Notifications/             # NotificationManager
│   ├── Device/                    # DeviceManager, MacroDeckDevice, DeviceType
│   ├── Services/                  # UpdateService, QrCodeService
│   ├── Language/                  # Strings, LanguageManager
│   ├── ExtensionStore/            # ExtensionStoreHelper
│   ├── GUI/                       # Views, dialogs, custom controls
│   ├── CottleIntegration/         # TemplateManager
│   ├── InternalPlugins/           # DevicePlugin (SetBrightness, SetProfile)
│   ├── wwwroot/client/            # Compiled Angular PWA web client
│   └── Resources/                 # Icons, images, language JSON files
├── MacroDeck.Server/              # Server library (Kestrel, WebSocket)
├── MacroDeck.Setup/               # Inno Setup installer + ADB binaries
│   └── Android Debug Bridge/      # adb.exe + supporting DLLs
├── MacroDeck.Tests/               # NUnit test project
├── Macro-Deck-Web-Client-main/   # Web client Angular source
└── Macro-Deck-Extensions-main/   # Extensions repository (submodule)
```

---

## Getting Started

### Prerequisites
- Windows 10 (build 22000+) or Windows 11
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- Android device with Macro Deck Companion App installed

### Build from Source

```powershell
dotnet build MacroDeck.sln
```

Output binaries are located in each project's `bin/` directory.

### Run

```powershell
dotnet run --project MacroDeck/MacroDeck.csproj
```

Or with debug console:
```powershell
dotnet run --project MacroDeck/MacroDeck.csproj -- --debug-console
```

### Build Installer

You need [Inno Setup 6](https://jrsoftware.org/isinfo.php) to compile the installer:

```
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "MacroDeck.Setup\Macro Deck.iss"
```

---

## Command Line Arguments

| Argument | Description |
|----------|-------------|
| `--port <n>` | Override the default host port (8191) |
| `--show` | Show main window on startup |
| `--debug-console` | Open the debug console window |
| `--log-level <0-5>` | Set log verbosity (0=Trace, 5=Error) |
| `--disable-file-logging` | Disable file logging |
| `--portable` | Enable portable mode |
| `--test-channel` | Use test update channel |
| `--force-update` | Force an update check |
| `--ignore-pid-check` | Ignore process ID check (for restart) |

---

## Supported Languages

Macro Deck Pro ships with **17 languages**:

| # | Language | Code |
|---|----------|------|
| 1 | English | `en` |
| 2 | Chinese (简体中文) | `zh` |
| 3 | German (Deutsch) | `de` |
| 4 | Spanish (Español) | `es` |
| 5 | French (Français) | `fr` |
| 6 | Italian (Italiano) | `it` |
| 7 | Japanese (日本語) | `ja` |
| 8 | Korean (한국어) | `ko` |
| 9 | Polish (Polski) | `pl` |
| 10 | Portuguese (Português) | `pt` |
| 11 | Russian (Русский) | `ru` |
| 12 | Turkish (Türkçe) | `tr` |
| 13 | Bulgarian (Български) | `bg` |
| 14 | Dutch (Nederlands) | `nl` |
| 15 | Finnish (Suomi) | `fi` |
| 16 | Hungarian (Magyar) | `hu` |
| 17 | Filipino | `fil` |

ADB-related strings are translated in all 17 languages.

---

## vs. Original Macro Deck 2

| Feature | Macro Deck 2 (Original) | Macro Deck Pro |
|---------|------------------------|---------------|
| Wi-Fi connection | ✅ | ✅ |
| **USB ADB connection** | ❌ | ✅ |
| **SSL/TLS certificates** | ❌ | ✅ |
| **Backup & restore** | ❌ | ✅ |
| **System notifications** | ❌ | ✅ |
| **Multi-device registry** | ❌ | ✅ |
| **QR code quick setup** | ❌ | ✅ |
| **Angular PWA web client** | ❌ | ✅ (self-hosted) |
| **Auto-update service** | ❌ | ✅ |
| Global hotkeys | Limited | ✅ Full |
| Named pipe IPC | ❌ | ✅ |
| Custom Cottle functions | ❌ | ✅ |
| Debug console | ❌ | ✅ |
| Beta update channel | ❌ | ✅ |
| Portable mode | ❌ | ✅ |
| Professional Inno Setup installer | ❌ | ✅ 25+ languages |

---

## Original Macro Deck

- **Original Repository**: [Macro-Deck-org/Macro-Deck](https://github.com/Macro-Deck-org/Macro-Deck)
- **Original Author**: SuchByte
- **Website**: [macrodeck.org](https://macrodeck.org)
- **Discord**: [discord.gg/yr7TRaXum8](https://discord.gg/yr7TRaXum8)
- **Wiki**: [github.com/SuchByte/Macro-Deck/wiki](https://github.com/SuchByte/Macro-Deck/wiki)

---

## License

This project is licensed under the same terms as the original Macro Deck 2 project. See the [LICENSE](./LICENSE) file for details.

> **Note**: Macro Deck Pro is an independent fork. It is not affiliated with or endorsed by the original Macro Deck project or SuchByte. However, it maintains full compatibility with the Macro Deck ecosystem.

# Fingerprint Browser Phase 1 Design

## Goal

Phase 1 delivers a runnable Windows 10 x64 C# desktop application for profile management, fingerprint generation, and Chromium launching. It intentionally avoids Chromium source patches. The launcher and generator communicate with the future patched Chromium only through profile JSON files and command-line arguments.

## Confirmed Stack

- Runtime: .NET 8
- UI framework: WPF
- Language: C#
- Target OS: Windows 10 x64
- Storage: local JSON files under `profiles/`
- Build verification: GitHub Actions, not local test execution
- Delivery: source project plus published `win-x64` artifact from GitHub Actions

## Repository Layout

The first implementation should add the desktop application without disturbing the Chromium patch pipeline.

```text
src/
  FpBrowserLauncher/
    FpBrowserLauncher.csproj
    App.xaml
    MainWindow.xaml
    Models/
    Services/
    ViewModels/
profiles/
  p001/
    fingerprint.json
    proxy.json
    user_data/
    launch-snapshots/
.github/
  workflows/
    fp-launcher-build.yml
```

`profiles/` is runtime data and should be kept out of committed sample secrets. If sample files are needed, they should use non-sensitive placeholder proxy values.

## Application Scope

### Profile Management

The app shows a list of profiles with profile ID, display name, proxy summary, fingerprint summary, and running state. Phase 1 supports creating, editing, deleting, launching, and stopping profiles.

### Fingerprint Generator

The generator creates a stable `fingerprint.json` for each profile. `profile_seed` is deterministically derived from `profile_id`, so regenerating the same profile does not cause fingerprint drift.

The first generator implementation should use a small built-in Windows desktop sample pool. It should keep fields internally consistent, especially OS, User-Agent, fonts, WebGL vendor/renderer, resolution, CPU cores, memory, timezone, and language.

### Proxy Configuration

Each profile stores SOCKS5 proxy settings in `profiles/{profileId}/proxy.json`:

```json
{
  "type": "socks5",
  "host": "127.0.0.1",
  "port": 1080,
  "username": "",
  "password": ""
}
```

The launcher must fail closed. Before starting Chromium, it must open a TCP connection to the proxy and complete a SOCKS5 handshake, including username/password authentication when configured. If the proxy check fails, Chromium must not be started.

### Chromium Launcher

The launcher reads the selected profile and starts the configured Chromium executable with profile-specific arguments:

```text
--user-data-dir="profiles/{profileId}/user_data"
--fp-config="profiles/{profileId}/fingerprint.json"
--no-first-run
--disable-background-networking
```

User-supplied extra flags are allowed, but flags that conflict with profile isolation or fingerprint configuration must be filtered or overridden by protected flags.

### Process Monitoring

The app tracks launched Chromium processes by profile ID and PID. It updates running state when the process exits and supports force-stopping a running profile.

### Launch Snapshots

Every launch writes a snapshot JSON file under `profiles/{profileId}/launch-snapshots/`. The snapshot records the effective Chromium path, arguments, selected fingerprint fields, proxy summary, timestamp, and process ID when available. This helps verify that profile values remain stable between launches.

## Fingerprint JSON Shape

Phase 1 uses the schema from `指纹浏览器实施提示词.md` section 6 as the canonical target, even though Chromium does not yet consume most fields.

Required initial fields:

```json
{
  "profile_id": "p001",
  "profile_seed": "a1b2c3d4e5f6",
  "webrtc": { "mode": "proxy_udp" },
  "timezone": { "mode": "based_on_ip", "value": "Asia/Shanghai" },
  "geolocation": { "mode": "based_on_ip", "prompt_policy": "ask_every_time" },
  "language": { "mode": "based_on_ip", "value": "zh-CN" },
  "ui_language": { "mode": "based_on_language", "value": "zh-CN" },
  "resolution": { "mode": "based_on_ua", "width": 1920, "height": 1080 },
  "fonts": { "mode": "custom", "list": ["Arial", "Microsoft YaHei", "SimSun", "Calibri"] },
  "noise_toggles": {
    "canvas": false,
    "webgl_image": false,
    "audio_context": true,
    "media_devices": true,
    "client_rects": true,
    "speech_voices": true
  },
  "webgl": {
    "mode": "custom",
    "vendor": "Google Inc. (NVIDIA)",
    "renderer": "ANGLE (NVIDIA, NVIDIA GeForce GTX 1660 Direct3D11 vs_5_0 ps_5_0)"
  },
  "webgpu": { "mode": "based_on_webgl" },
  "cpu_cores": 8,
  "device_memory_gb": 16,
  "device_name": "DESKTOP-PROFILE",
  "mac_address": "00-50-43-3C-49-4B",
  "do_not_track": "default",
  "port_scan_protection": { "enabled": true, "allowed_ports": [] },
  "hardware_acceleration": "default",
  "tls_fingerprint": { "mode": "chrome_default" },
  "extra_flags": ["--disable-notifications"]
}
```

## Main Components

- `ProfileStore`: loads, saves, creates, and deletes profile folders and JSON files.
- `FingerprintGenerator`: creates deterministic, internally consistent fingerprint configs.
- `Socks5ProxyTester`: performs fail-closed SOCKS5 connectivity checks.
- `ChromiumLauncher`: builds protected command-line arguments and starts Chromium.
- `ProcessTracker`: tracks running processes and exit events.
- `LaunchSnapshotWriter`: writes launch audit records.
- `MainWindowViewModel`: coordinates UI commands and state.

## Error Handling

- Invalid Chromium path blocks launch and shows a clear UI error.
- Missing or malformed profile JSON blocks launch until regenerated or fixed.
- Failed SOCKS5 handshake blocks launch.
- Process start failures are reported and logged without marking the profile as running.
- Snapshot write failures should not crash the app, but should be shown in the launch result.

## GitHub Actions Verification

Local test execution is not required for this phase. The repository should include a workflow that runs on push and pull request:

```text
dotnet restore src/FpBrowserLauncher/FpBrowserLauncher.csproj
dotnet build src/FpBrowserLauncher/FpBrowserLauncher.csproj -c Release --no-restore
dotnet test src/FpBrowserLauncher.Tests/FpBrowserLauncher.Tests.csproj -c Release --no-build
dotnet publish src/FpBrowserLauncher/FpBrowserLauncher.csproj -c Release -r win-x64 --self-contained false
```

If the test project is not added in the first implementation slice, the workflow should skip `dotnet test` until tests exist. The publish output should be uploaded as a GitHub Actions artifact.

## Phase 1 Acceptance Criteria

- GitHub Actions can build the WPF app for `win-x64`.
- The app can create a profile folder with `fingerprint.json` and proxy settings.
- Regenerating the same profile keeps the same `profile_seed` and stable deterministic values.
- Launching with an unavailable proxy is refused.
- Launching with an available proxy starts Chromium with `--user-data-dir` and `--fp-config`.
- Running state changes when the Chromium process exits.
- Each launch creates a snapshot log.

## Out Of Scope For Phase 1

- Chromium patch files under `patches/extra/fp-browser/`.
- Renderer or browser-process fingerprint hooks.
- SOCKS5 UDP tunneling inside Chromium.
- Real GeoIP service integration.
- Cookie and storage backup/restore.
- Large external fingerprint sample datasets.

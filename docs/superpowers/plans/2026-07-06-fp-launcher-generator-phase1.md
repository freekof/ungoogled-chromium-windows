# FP Launcher Generator Phase 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Windows 10 x64 .NET 8 WPF launcher and fingerprint generator that can be built and published from GitHub Actions.

**Architecture:** Add a standalone WPF app under `src/FpBrowserLauncher` and unit tests under `tests/FpBrowserLauncher.Tests`. Keep Chromium patch work out of scope; all integration uses profile JSON files and Chromium command-line arguments.

**Tech Stack:** C# 12, .NET 8, WPF, xUnit, GitHub Actions `workflow_dispatch`.

---

## File Map

- `src/FpBrowserLauncher/FpBrowserLauncher.csproj`: WPF project targeting `net8.0-windows`.
- `src/FpBrowserLauncher/App.xaml` and `App.xaml.cs`: application entry point.
- `src/FpBrowserLauncher/MainWindow.xaml` and `MainWindow.xaml.cs`: minimal desktop UI for settings, profile list, and launch controls.
- `src/FpBrowserLauncher/Models/*.cs`: JSON models and UI summaries.
- `src/FpBrowserLauncher/Services/*.cs`: profile storage, fingerprint generation, SOCKS5 testing, launch argument building, process tracking, settings, and snapshots.
- `src/FpBrowserLauncher/ViewModels/*.cs`: WPF view model and commands.
- `tests/FpBrowserLauncher.Tests/FpBrowserLauncher.Tests.csproj`: xUnit tests.
- `tests/FpBrowserLauncher.Tests/*.cs`: tests for deterministic generation, protected launch arguments, profile storage, and SOCKS5 handshake behavior.
- `.github/workflows/fp-launcher-build.yml`: manually triggered GitHub Actions build/test/publish workflow.

## Tasks

### Task 1: Project Skeleton

- [ ] Create `src/FpBrowserLauncher/FpBrowserLauncher.csproj` with WPF enabled and nullable reference types.
- [ ] Create WPF `App` and `MainWindow` files.
- [ ] Create `tests/FpBrowserLauncher.Tests/FpBrowserLauncher.Tests.csproj` referencing the app project.
- [ ] Add `.gitignore` entries for `profiles/`, `bin/`, and `obj/`.

### Task 2: Models And JSON Storage

- [ ] Add profile, fingerprint, proxy, app settings, and launch result models with `System.Text.Json` attributes matching the target JSON schema.
- [ ] Add `JsonFile` helper for indented UTF-8 JSON read/write.
- [ ] Add `ProfileStore` for creating, listing, loading, saving, and deleting profiles.
- [ ] Add tests that create temporary profiles and verify `fingerprint.json` and `proxy.json` paths.

### Task 3: Fingerprint Generator

- [ ] Add deterministic seed generation from `profile_id` using SHA-256.
- [ ] Add a small built-in Windows desktop sample pool.
- [ ] Generate internally consistent Windows 10 fingerprint fields.
- [ ] Add tests proving the same profile ID produces the same seed and stable generated values.

### Task 4: SOCKS5 And Launcher Services

- [ ] Add SOCKS5 handshake tester supporting no-auth and username/password auth.
- [ ] Add `ChromiumLauncher` using `ProcessStartInfo.ArgumentList` instead of manual string quoting.
- [ ] Filter protected conflicting flags such as `--user-data-dir`, `--fp-config`, and `--proxy-server`.
- [ ] Add tests for protected arguments and fail-closed proxy behavior.

### Task 5: WPF UI

- [ ] Add `MainWindowViewModel` with refresh, create/update, delete, launch, and stop commands.
- [ ] Bind UI fields for Chromium path, profile ID, display name, proxy host, port, username, and password.
- [ ] Show profile list and current status messages.

### Task 6: GitHub Actions

- [ ] Add `.github/workflows/fp-launcher-build.yml` with `workflow_dispatch` so the workflow can be started manually.
- [ ] Run restore, build, test, and publish on `windows-latest`.
- [ ] Upload the `win-x64` published output as an artifact.

## Verification

Local build and test commands are intentionally not run for this phase per user instruction. Verification is performed by manually starting the GitHub Actions workflow after push.

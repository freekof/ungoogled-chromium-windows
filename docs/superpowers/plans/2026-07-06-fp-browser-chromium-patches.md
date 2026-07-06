# FP Browser Chromium Patches Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the complete 11 Chromium patch files required for the browser to consume `fingerprint.json` and apply fingerprint/network overrides.

**Architecture:** Keep all Chromium source modifications as patch files in `patches/extra/fp-browser/`, registered from `patches/series`. Patch 01 creates the shared config/utility layer; patches 02-11 hook individual Chromium subsystems and depend on patch 01.

**Tech Stack:** Chromium C++, Chromium patch series, GitHub Actions Windows build validation.

---

## File Map

- Create `patches/extra/fp-browser/01-config-injector.patch`: config loader, command-line switch, renderer switch propagation, build registration.
- Create `patches/extra/fp-browser/02-webrtc-modes.patch`: WebRTC mode gates and ICE candidate replacement hooks.
- Create `patches/extra/fp-browser/03-timezone-geolocation-language.patch`: timezone, geolocation, language, and Accept-Language hooks.
- Create `patches/extra/fp-browser/04-resolution-fonts.patch`: screen size and Windows font filtering hooks.
- Create `patches/extra/fp-browser/05-noise-injection-framework.patch`: deterministic noise injector and hooks for canvas/audio/media/client rects/speech voices.
- Create `patches/extra/fp-browser/06-webgl-webgpu-metadata.patch`: WebGL and WebGPU metadata overrides.
- Create `patches/extra/fp-browser/07-cpu-ram-navigator.patch`: `hardwareConcurrency` and `deviceMemory` overrides.
- Create `patches/extra/fp-browser/08-mac-device-name.patch`: network interface MAC and device-name overrides.
- Create `patches/extra/fp-browser/09-port-scan-protection.patch`: URL loader throttle for local/private port scan blocking.
- Create `patches/extra/fp-browser/10-tls-fingerprint.patch`: TLS fingerprint mode plumbing and fixed JA3 guard rails.
- Create `patches/extra/fp-browser/11-udp-over-socks5.patch`: SOCKS5 TCP/UDP proxy enforcement, UDP ASSOCIATE support, QUIC disable, and fail-closed direct UDP blocking.
- Modify `patches/series`: append the 11 new patch paths after existing Windows patches.
- Modify docs only if needed to document manual GitHub validation.

## Tasks

### Task 1: Create Patch Directory And Register Series

- [ ] Create `patches/extra/fp-browser/`.
- [ ] Add all 11 patch files with stable names.
- [ ] Append all 11 paths to `patches/series` in numeric order.
- [ ] Run `git diff --check` to catch whitespace errors.

### Task 2: Config Injector Patch

- [ ] Add a Chromium common `fp_config` helper that reads `--fp-config` JSON lazily.
- [ ] Add typed getters for profile seed, language, timezone, resolution, WebGL, WebGPU, CPU, RAM, fonts, proxy, WebRTC, noise toggles, and port scan settings.
- [ ] Add renderer command-line propagation for `--fp-config`.
- [ ] Add build file entries required by the new helper.

### Task 3: Browser Fingerprint Hook Patches

- [ ] Add WebRTC mode and ICE candidate replacement hooks.
- [ ] Add timezone, geolocation, language, and Accept-Language hooks.
- [ ] Add screen size and Windows font filtering hooks.
- [ ] Add deterministic noise framework and wire seed-based noise to canvas, WebGL readback, AudioContext, media devices, ClientRects, and speech voices.
- [ ] Add WebGL/WebGPU metadata hooks.
- [ ] Add CPU/RAM navigator hooks.
- [ ] Add MAC/device name hooks.

### Task 4: Network Protection Patches

- [ ] Add URL loader throttle for local/private port scan protection.
- [ ] Add TLS fingerprint mode plumbing with `chrome_default` as the default behavior.
- [ ] Add SOCKS5 proxy enforcement for TCP.
- [ ] Add SOCKS5 UDP ASSOCIATE support for UDP sends.
- [ ] Disable QUIC when proxy UDP protection is active.
- [ ] Block UDP if SOCKS5 UDP setup fails or proxy UDP support is missing.

### Task 5: Verification And Push

- [ ] Run `git diff --check`.
- [ ] Inspect `git diff --stat` and `git status --short`.
- [ ] Commit the patch set.
- [ ] Push to `origin master`.
- [ ] Confirm the existing GitHub `build-x64` workflow is still manually triggerable.
- [ ] Tell the user to manually run `build-x64`; use failed patch/build logs for the next iteration if Chromium APIs changed.

## Verification Limits

Local Chromium build verification is unavailable because this workspace has no Chromium source checkout. The first reliable verification is GitHub Actions applying these patches during the Windows x64 build.

# Fingerprint Browser Chromium Patches Design

## Goal

Phase 2 adds Chromium patch files so the launcher-generated `fingerprint.json` can affect the browser runtime. All Chromium changes are delivered as patch files under `patches/extra/fp-browser/` and registered in `patches/series`.

## Scope

This phase implements the complete patch set requested by `指纹浏览器实施提示词.md` section 8:

- `01-config-injector.patch`
- `02-webrtc-modes.patch`
- `03-timezone-geolocation-language.patch`
- `04-resolution-fonts.patch`
- `05-noise-injection-framework.patch`
- `06-webgl-webgpu-metadata.patch`
- `07-cpu-ram-navigator.patch`
- `08-mac-device-name.patch`
- `09-port-scan-protection.patch`
- `10-tls-fingerprint.patch`
- `11-udp-over-socks5.patch`

The patches are intended to be validated by GitHub Actions because the local workspace does not contain a Chromium source checkout.

## Patch Strategy

### Configuration Bus

Add a shared `fp_config` component that lazily reads `--fp-config`, parses JSON, and exposes typed accessors. The browser process keeps the path and forwards it to renderer processes with `ChromeContentBrowserClient::AppendExtraCommandLineSwitches` so Blink-side hooks can read the same file.

The config loader must fail closed for security-sensitive networking fields. If the profile requests proxy UDP but the required proxy information is missing, WebRTC UDP and generic UDP paths should block instead of falling back to direct traffic.

### Browser-Visible Fingerprint Hooks

Renderer-side hooks read the same config and override JavaScript-visible values:

- `navigator.language` and `navigator.languages`
- `Intl` and timezone offset related values where Chromium exposes a hookable timezone source
- `screen.width`, `screen.height`, `availWidth`, and `availHeight`
- WebGL unmasked vendor and renderer
- WebGPU adapter info aligned with WebGL
- `navigator.hardwareConcurrency`
- `navigator.deviceMemory`
- canvas, WebGL readback, AudioContext, ClientRects, media devices, and speech voices using deterministic seed-based noise

### Network And Leak Protection

TCP traffic should use Chromium's proxy stack with a SOCKS5 proxy derived from `fingerprint.json` or launcher flags.

UDP is separate. Chromium does not automatically route all UDP through a SOCKS5 proxy. Phase 2 therefore uses these rules:

- Disable QUIC to avoid HTTP/3 direct UDP leakage.
- Set WebRTC IP handling policy to `disable_non_proxied_udp`.
- Rewrite ICE candidates so reported public IP matches the configured proxy/fake public IP when supplied.
- Implement SOCKS5 UDP ASSOCIATE for Chromium UDP socket sends where possible.
- If UDP proxy setup fails or the SOCKS5 server does not support UDP, block UDP instead of sending direct packets.

This guarantees fail-closed behavior for the leak paths under Chromium control.

### Patch Ordering

`patches/series` should register the new patches after the existing Windows patches so the Windows build fixes are applied first. Each patch should be independently reviewable, but later patches may depend on the config component from patch 01.

## Testing Model

Local Chromium compilation is not available in this workspace. Validation is done by pushing patches to GitHub and manually running the existing Chromium build workflow.

Expected browser-level checks after a successful build:

- `chrome://version` contains launcher-provided flags including `--fp-config`.
- BrowserLeaks/CreepJS shows configured language, screen, CPU, RAM, WebGL, WebGPU, and deterministic noise values.
- `ipleak.net` WebRTC result does not show the host public IP.
- With proxy disabled or SOCKS5 UDP unsupported, UDP paths fail closed instead of leaking direct IP traffic.

## Known Risk

The complete 11-patch set touches fast-moving Chromium internals. The first GitHub build may expose context or API drift. Fixes should be applied as follow-up commits by reading the failed patch/build logs and adjusting the affected patch files.

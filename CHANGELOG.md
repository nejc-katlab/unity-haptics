# Changelog

All notable changes to this package are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-04-29

### Added
- **Configurable logging.** New `Haptics.LogLevel` (`HapticsLogLevel.None`/`Error`/`Warning`/`Info`/`Debug`,
  default `Warning`) plus `Haptics.SetLogLevel(int)` convenience. Setting the level propagates to the
  native iOS and Android bridges automatically.
  - C# layer routes through Unity's Debug.Log family with a `[katlab.Haptics]` prefix.
  - iOS bridge logs via `NSLog` with prefix `[katlab.Haptics][native]` (visible in Xcode Console / Console.app).
  - Android bridge logs via `android.util.Log` under tag `katlab.Haptics` (visible via `adb logcat -s katlab.Haptics`).
  - Debug level dumps every event of a played pattern (time/duration/intensity/sharpness/type), every
    throttle suppression with the threshold and elapsed delta, and every native call.
  - Error level surfaces engine init/start failures, pattern construction errors, and
    vibrator-service unavailability — previously all silent.
- New entry points: `_Haptics_SetLogLevel` (iOS native) and `HapticsBridge.setLogLevel(int)` (Android native).
- Sample scene: log level picker row at the top of the GUI.

### Changed
- README: rewrote the Installation section to use the actual git-URL form
  (`"dev.katlab.haptics": "https://github.com/nejc-katlab/unity-haptics.git#v1.2.0"`),
  added a Logging section, and added `LogLevel` / `SetLogLevel` to the API reference table.
- `IHapticsService` and `HapticsService` gained a `SetLogLevel(HapticsLogLevel)` method
  (default no-op; iOS/Android override to forward to native).

## [1.1.0] - 2026-04-29

### Added
- `HapticEvent` struct and `HapticEventType` enum for rich Core Haptics-style events with
  per-event time, duration, intensity, sharpness, and type (transient / continuous).
- `HapticPattern.FromEvents(HapticEvent[])` factory for rich patterns.
- `HapticPatternAsset` ScriptableObject with `intensityScale` / `timeScale` multipliers, plus a
  custom inspector that includes a ▶ Play preview button. Create via
  **Assets > Create > katlab > Haptics > Pattern**.
- iOS native `_Haptics_PlayEvents` entry point — full per-event intensity + sharpness, mixing
  `CHHapticEventTypeHapticTransient` and `CHHapticEventTypeHapticContinuous` events.
- Android best-effort translation: rich events → `VibrationEffect.createWaveform` waveform.
  Sharpness is dropped (no Android analog).
- `Haptics.ThrottleIntervalMs` / `Haptics.SetThrottle(int)` — minimum interval gate to suppress
  high-frequency haptic spam (e.g. collision-heavy gameplay). Per-kind, per-sub-key slots so
  unrelated calls don't suppress each other. Default `0` is opt-in.

### Fixed
- iOS Rigid / Soft impact styles were swapped: passing `(int)style` straight to
  `UIImpactFeedbackStyle` produced Soft for Rigid and vice versa. Native bridge now uses an
  explicit lookup table; the public C# enum is unchanged.
- iOS `CHHapticEngine` now has `stoppedHandler` and `resetHandler` wired up. Previously the
  engine could die silently after backgrounding / route changes / hardware reset and never
  restart; rich and legacy patterns will now lazily restart on next play.
- README: clarified that the `Amplitudes` array length matches `Timings` (values at vibrate
  positions are used), matching both the iOS and Android implementations.

### Changed
- **Package rebrand** — `com.mythicstudio.haptics` → `dev.katlab.haptics`. C# namespace
  `MythicStudio.Haptics.*` → `Katlab.Haptics.*`. Java package `com.mythicstudio.haptics` →
  `dev.katlab.haptics`. asmdefs renamed to `Katlab.Haptics` and `Katlab.Haptics.Editor`.
  Author / license / repo metadata in `package.json` filled in.
- Sample scene (`HapticsSample`) extended with a continuous-event "rumble" button, a
  `HapticPatternAsset` slot, and a throttle slider.

## [1.0.0] - 2026-03-12

Initial release.

### Features
- `Impact(HapticImpactStyle)` for Light / Medium / Heavy / Rigid / Soft.
- `Notification(HapticNotificationType)` for Success / Warning / Error.
- `Vibrate(long)` (Android only).
- `PlayPattern(HapticPattern)` for legacy timing/amplitude waveforms.
- `IsSupported` capability check.
- iOS native bridge (`UIImpactFeedbackGenerator`, `UINotificationFeedbackGenerator`,
  CoreHaptics transient-event waveforms).
- Android native bridge (`VibrationEffect` predefined effects on API 29+, fallbacks down to
  legacy `Vibrator.vibrate`).
- Editor `HapticsBuildProcessor` automatically links `CoreHaptics.framework` on iOS builds.
- Generator caching and predefined-effect caching for high-frequency callers.

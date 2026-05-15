# Changelog

All notable changes to this package are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.1] - 2026-05-15

### Fixed
- **Xcode 26 / iOS 26.4 SDK compatibility.** `Runtime/Haptics/Plugins/iOS/HapticsBridge.mm`
  used four CoreHaptics APIs that aren't declared in the iOS 26.4 SDK public headers,
  producing 9 build errors against Xcode 26:
  - `+[CHHapticEngine supportsHardware]` → `+[CHHapticEngine capabilitiesForHardware].supportsHaptics`.
  - `+[CHHapticEventParameter parameterWithParameterID:value:]` → `-[CHHapticEventParameter initWithParameterID:value:]`.
  - `+[CHHapticEvent eventWithEventType:parameters:relativeTime:]` → `-[CHHapticEvent initWithEventType:parameters:relativeTime:]`.
  - `CHHapticEngine.running` (never a public property) → file-scope `s_engineRunning` flag
    maintained by `stoppedHandler` / `resetHandler` / `_Haptics_EnsureEngineRunning`.

  No behaviour change; pure build-compat fix. The package still targets iOS 13.0+.

## [1.5.0] - 2026-04-29

### Added
- **Engine-agnostic native bridges.** `HapticsBridge.java` no longer imports
  `com.unity3d.player.UnityPlayer` and compiles standalone in any Android project.
  Initialisation moves to a mandatory `HapticsBridge.init(Context)` call; the Unity
  C# wrapper does the JNI handoff to `UnityPlayer.currentActivity` so existing Unity
  callers see no behaviour change. Native iOS already had no Unity dependency — it
  remains a pure Obj-C++ file.
- **Inline pattern construction API on both platforms.**
  - **iOS:** `_Haptics_PlayEvents` ABI changed from five parallel float/int arrays to
    a single `KatlabHapticEvent` struct array. Native iOS users can author patterns
    inline with C99 designated initializers.
  - **iOS:** new `KatlabHapticPattern` Obj-C class (`KatlabHapticPattern.h` / `.mm`)
    — fluent builder with `tapAt:intensity:sharpness:` and
    `holdAt:duration:intensity:sharpness:` chaining. Bridges cleanly to Swift.
  - **Android:** new `HapticsPatternBuilder` Java class with `tap(time, intensity, sharpness)` and
    `hold(time, duration, intensity, sharpness)` chaining. Bridges cleanly to Kotlin.
- **README "Using outside Unity" section** with concrete native iOS (Obj-C / Swift)
  and native Android (Java / Kotlin) examples for both the direct ABI and the
  fluent builders, plus runtime pattern construction reference.

### Changed
- **Preset names renamed to neutral / sensory terms** so the package reads idiomatically
  in any context (games, business apps, productivity tools). Old names removed
  (no `[Obsolete]` aliases — nothing live).

  | New (v1.5) | Old (v1.4) |
  |---|---|
  | `Click` | `GunshotPistol` |
  | `Snap` | `GunshotRifle` |
  | `Boom` | `GunshotShotgun` |
  | `BoomDeep` | `GunshotSniper` |
  | `Burst` | `ExplosionSmall` |
  | `BurstHeavy` | `ExplosionMedium` |
  | `BurstHuge` | `ExplosionLarge` |
  | `Rumble` | `ExplosionDistant` |
  | `Thud` | `ImpactHeavy` |
  | `ThudSharp` | `CriticalHit` |
  | `TripleTap` | `DamageTaken` |
  | `DoubleTap` | `Reload` |
  | `Heartbeat` | (unchanged) |

- README "Game-grade presets" section retitled "Presets" with the new feel-based
  description column.
- Sample button labels updated to the new preset names.

### Removed
- Old game-centric preset names (`GunshotPistol`, `ExplosionLarge`, etc.) and their
  underlying private fields. Migration: textual rename per the table above.

## [1.4.0] - 2026-04-29

### Added
- **Capability tiers and tier-aware preset playback.** A given `HapticPattern` doesn't feel
  the same on every phone — Galaxy A-class ERM motors physically can't reproduce sub-30 ms
  transients. The package now detects the device's tier on first call and plays a preset
  variant tuned to it.
  - New `HapticCapability` enum: `None` / `Minimal` / `Basic` / `Rich`.
  - New `Haptics.Capability` property — auto-detected, settable to force a tier for
    testing (and `Haptics.ResetCapability()` to clear the override).
  - Detection: iOS 13+ with `[CHHapticEngine supportsHardware]` → Rich; iOS 10–12 → Basic.
    Android 11+ with `areAllPrimitivesSupported(PRIMITIVE_CLICK + PRIMITIVE_TICK)` → Rich;
    API 26+ with `hasAmplitudeControl()` → Basic; otherwise Minimal.
  - New native entry points: iOS `_Haptics_GetCapability`, Android `getCapability()`.
- **`HapticPreset` enum** with the 13 named events and **`Haptics.PlayPreset(HapticPreset)`**
  that picks the variant matching the device's current `Capability`.
- **Basic and Minimal variants** for every preset built on `HapticPattern.CreateWaveform(...)`:
  - **Basic** uses pulses ≥30 ms with amplitude curves so ERM motors have time to spin up
    and the perceptual "weight" still comes through.
  - **Minimal** uses on/off pulses only (slightly longer durations to convey strength
    without amplitude).
- New lookup methods: `HapticPresets.Get(HapticPreset)` and
  `HapticPresets.Get(HapticPreset, HapticCapability)`.
- Sample scene shows the detected `Capability` and adds a force-tier toggle so you can A/B
  Rich-vs-Basic on the same hardware. The preset row now uses `PlayPreset` instead of
  `PlayPattern` so it follows the active tier.

### Changed
- `IHapticsService` / `HapticsService` gained `Capability { get; }` (default `None`;
  iOS / Android override).
- README: new "Capability tiers and tier-aware playback" section explaining LRA-vs-ERM
  behaviour and which tier each device falls into.

## [1.3.0] - 2026-04-29

### Added
- **`HapticPresets`** — curated game-grade haptic pattern library built on
  `HapticPattern.FromEvents(...)`. Significantly punchier than `Haptics.Impact(...)`,
  which maps to Apple's UI-grade `UIImpactFeedbackGenerator` and is bounded by
  design. Patterns:
  - **Gunshots:** `GunshotPistol`, `GunshotRifle`, `GunshotShotgun`, `GunshotSniper`
  - **Explosions:** `ExplosionSmall`, `ExplosionMedium`, `ExplosionLarge`, `ExplosionDistant`
  - **Combat:** `ImpactHeavy`, `CriticalHit`, `DamageTaken`
  - **Misc:** `Reload`, `Heartbeat`
- Each preset is a `static readonly HapticPattern` built once at class load
  (allocation-free). Use as `Haptics.PlayPattern(HapticPresets.ExplosionLarge);`.
- Sample scene gains a "Game-grade presets" section with one button per preset
  so you can A/B against the UI-grade `Impact(Heavy)` on device.

### Changed
- README: new "Game-grade presets" section explaining why `Impact(Heavy)` is a
  UI tap rather than a game thump, and what real game studios actually use
  (`CHHapticPattern` / `VibrationEffect.createWaveform`).

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

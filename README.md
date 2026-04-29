# katlab Haptics

Cross-platform haptics package for Unity targeting iOS and Android. Provides simple presets, rich Core Haptics patterns (intensity + sharpness, transient + continuous events), ScriptableObject pattern assets for designer authoring, and built-in throttling for high-frequency callers.

## Requirements

- Unity 6 (6000.x)
- iOS 10+ (device; simulator has no haptics) — Core Haptics features require iOS 13+
- Android API 26+ (VibrationEffect); legacy vibrate for older devices

## Installation

**Via Git URL (Package Manager):** open **Window → Package Manager → + → Add package from git URL** and paste:

```
https://github.com/nejc-katlab/unity-haptics.git#v1.4.0
```

Or add directly to `Packages/manifest.json`:

```json
"dev.katlab.haptics": "https://github.com/nejc-katlab/unity-haptics.git#v1.4.0"
```

Drop the `#v1.4.0` suffix to track `main` instead of pinning a release.

**Embedded:** clone or copy the repo into your project's `Packages/` directory.

## Quick Start

```csharp
using Katlab.Haptics;
using Katlab.Haptics.Domain;

// Simple presets
Haptics.Impact(HapticImpactStyle.Light);
Haptics.Impact(HapticImpactStyle.Medium);
Haptics.Impact(HapticImpactStyle.Heavy);
Haptics.Notification(HapticNotificationType.Success);
Haptics.Notification(HapticNotificationType.Warning);
Haptics.Notification(HapticNotificationType.Error);

// Android: vibrate for duration
Haptics.Vibrate(100);

// Legacy waveform pattern (timings: vibrate, pause, vibrate, pause...)
var doubleTap = HapticPattern.CreateWaveform(new long[] { 0, 50, 50, 50 }, null);
Haptics.PlayPattern(doubleTap);

// One-shot
var oneShot = HapticPattern.CreateOneShot(50, 128);
Haptics.PlayPattern(oneShot);

// Rich Core Haptics events (per-event intensity + sharpness, transient + continuous)
var explosion = HapticPattern.FromEvents(new[]
{
    HapticEvent.Transient(time: 0f,   intensity: 1.0f, sharpness: 1.0f),  // sharp peak
    HapticEvent.Continuous(time: 0.02f, duration: 0.35f, intensity: 0.7f, sharpness: 0.2f),  // rumble
    HapticEvent.Transient(time: 0.4f, intensity: 0.4f, sharpness: 0.5f),  // tail
});
Haptics.PlayPattern(explosion);

// Throttling — drop calls within a min interval (useful for collision-heavy gameplay)
Haptics.ThrottleIntervalMs = 30;

if (Haptics.IsSupported) { /* ... */ }
```

## API Reference

### Static Class: `Haptics`

| Member | Signature | Description |
|--------|-----------|-------------|
| `IsSupported` | `bool` | Whether haptics are supported on the current device. `false` in Editor, iOS Simulator, and unsupported platforms. |
| `Capability` | `HapticCapability` | Detected hardware tier (`None` / `Minimal` / `Basic` / `Rich`). Settable to force a tier for testing. See [Capability tiers](#capability-tiers-and-tier-aware-playback). |
| `ResetCapability` | `void ResetCapability()` | Clears any explicit override and returns to auto-detection. |
| `PlayPreset` | `void PlayPreset(HapticPreset preset)` | Plays the variant of `preset` matching the current `Capability`. |
| `ThrottleIntervalMs` | `int` | Minimum interval between haptic calls of the same kind. `0` (default) disables throttling. |
| `SetThrottle` | `void SetThrottle(int milliseconds)` | Convenience setter for `ThrottleIntervalMs`. |
| `LogLevel` | `HapticsLogLevel` | Logging verbosity. Default `Warning`. Setting it propagates to native bridges. See [Logging](#logging). |
| `SetLogLevel` | `void SetLogLevel(HapticsLogLevel level)` | Convenience setter for `LogLevel`. |
| `Impact` | `void Impact(HapticImpactStyle style)` | Triggers an impact haptic. Styles: Light, Medium, Heavy, Rigid, Soft. |
| `Notification` | `void Notification(HapticNotificationType type)` | Triggers a notification haptic. Types: Success, Warning, Error. |
| `Vibrate` | `void Vibrate(long milliseconds)` | Vibrates for the given duration. **Android only**; no-op on iOS. |
| `PlayPattern` | `void PlayPattern(HapticPattern pattern)` | Plays a custom haptic pattern (legacy waveform or rich events). |

### Enum: `HapticImpactStyle`

| Value | iOS | Android |
|-------|-----|---------|
| `Light` | UIImpactFeedbackStyleLight | EFFECT_TICK |
| `Medium` | UIImpactFeedbackStyleMedium | EFFECT_CLICK |
| `Heavy` | UIImpactFeedbackStyleHeavy | EFFECT_HEAVY_CLICK |
| `Rigid` | UIImpactFeedbackStyleRigid (iOS 13+) | Approximated as Heavy |
| `Soft` | UIImpactFeedbackStyleSoft (iOS 13+) | Approximated as Light |

### Enum: `HapticNotificationType`

| Value | iOS | Android |
|-------|-----|---------|
| `Success` | UINotificationFeedbackTypeSuccess | EFFECT_DOUBLE_CLICK |
| `Warning` | UINotificationFeedbackTypeWarning | EFFECT_HEAVY_CLICK |
| `Error` | UINotificationFeedbackTypeError | EFFECT_HEAVY_CLICK |

### Struct: `HapticPattern`

Immutable pattern. Two flavors:

1. **Legacy waveform** — `Timings` (alternating vibrate/pause durations in milliseconds) plus optional `Amplitudes` (length should match `Timings`; values at vibrate positions are used; 0–255).
2. **Rich events** — `Events` array; on iOS this drives Core Haptics directly with per-event intensity + sharpness; on Android it's translated to a best-effort waveform.

| Static Method | Signature | Description |
|---------------|------------|-------------|
| `CreateOneShot` | `HapticPattern CreateOneShot(long durationMs, int amplitude = -1)` | Single vibration. `amplitude` 0–255 or -1 for default. |
| `CreateWaveform` | `HapticPattern CreateWaveform(long[] timings, int[] amplitudes = null)` | Waveform pattern. |
| `FromEvents` | `HapticPattern FromEvents(HapticEvent[] events)` | Rich event pattern. |

### Struct: `HapticEvent`

| Field | Type | Description |
|-------|------|-------------|
| `Time` | `float` | Start time in seconds (relative to pattern start). |
| `Duration` | `float` | Duration in seconds. Only used for `Continuous` events. |
| `Intensity` | `float` | Strength 0..1. |
| `Sharpness` | `float` | Perceived sharpness 0..1 (iOS only; ignored on Android). |
| `Type` | `HapticEventType` | `Transient` (a tap) or `Continuous` (a sustained vibration). |

| Static Factory | Signature |
|---------------|-----------|
| `Transient` | `HapticEvent.Transient(float time, float intensity = 1, float sharpness = 0.5f)` |
| `Continuous` | `HapticEvent.Continuous(float time, float duration, float intensity = 1, float sharpness = 0.5f)` |

### ScriptableObject: `HapticPatternAsset`

Author patterns in the Unity inspector and reference them from MonoBehaviours.

```csharp
[SerializeField] HapticPatternAsset explosionPattern;

void OnExplode() => explosionPattern.Play();
```

Create one via **Assets > Create > katlab > Haptics > Pattern**. The custom inspector shows a ▶ Play button (silent in the Editor — build to device to feel it) and exposes per-asset `intensityScale` and `timeScale` multipliers.

### Capability tiers and tier-aware playback

A given `HapticPattern` doesn't feel the same on every phone. iPhone 11+ and recent flagship Androids (Pixel 6+, Galaxy S22+) ship with **Linear Resonant Actuators** that ramp in <5 ms and reproduce layered events crisply. Mid-range Androids (Galaxy A-series, etc.) and older devices use **Eccentric Rotating Mass** motors that take 30–50 ms to spin up and can't reproduce sub-30 ms transients at all — a `GunshotPistol` rich pattern simply isn't felt.

The package detects this on first call and exposes it as `Haptics.Capability`:

| `HapticCapability` | What it means | Devices |
|---|---|---|
| `Rich` | Core Haptics (iOS 13+) **or** Android `VibrationEffect.Composition` primitives supported | iPhone 7+, Pixel 6+, Galaxy S22+, OnePlus 9+ |
| `Basic` | Amplitude control without primitives (`hasAmplitudeControl()` true, API 26+) | Galaxy A-series, mid-range Androids, iOS 10–12 |
| `Minimal` | Plain on/off vibrate, no amplitude | Older Androids (pre-API 26), low-end devices |
| `None` | No haptic hardware | Simulator, non-mobile |

Detection is automatic. You can also force a tier for testing on a higher-end device:

```csharp
Haptics.Capability = HapticCapability.Basic;   // simulate ERM behaviour on an iPhone
// ...
Haptics.ResetCapability();                     // back to auto-detection
```

### Game-grade presets: `HapticPresets`

`Haptics.Impact(HapticImpactStyle.Heavy)` maps to Apple's `UIImpactFeedbackGenerator` (and the equivalent `VibrationEffect.createPredefined` on Android). Apple's HIG explicitly designs these for **UI feedback only** — button taps, switches, pickers — and they're intentionally short and bounded so they don't fatigue the user during heavy UI use. That's why Heavy still feels like a button click under a virtual gunshot: it's a UI tap, not a game thump.

Real game studios (Apex Legends Mobile, PUBG Mobile, Pokémon GO, every AAA title that ships haptics on iPhone) use **`CHHapticPattern`** — exactly what `HapticPattern.FromEvents(...)` exposes. The "punch" comes from layering a sharp transient (intensity=1, sharpness=1) with a continuous decay body (low sharpness, ~50–800ms), which the system feedback layer deliberately can't do.

`HapticPresets` ships a curated library of those patterns:

| Preset | Best for |
|--------|----------|
| `GunshotPistol` | Sidearms, SMGs — sharp single click |
| `GunshotRifle` | Assault rifles — sharp click + brief crisp tail |
| `GunshotShotgun` | Shotguns — punchy boom + low decay + ejection click |
| `GunshotSniper` | Bolt-action — sharp report + long resonant low tail |
| `ExplosionSmall` | Grenades, small explosives |
| `ExplosionMedium` | RPGs, mortars, demo charges |
| `ExplosionLarge` | Bombs, nearby airstrikes — full cinematic blast |
| `ExplosionDistant` | Far blasts — deep low rumble, no sharp peak |
| `ImpactHeavy` | Game-grade body slam — significantly punchier than `Impact(Heavy)` |
| `CriticalHit` | Crits, weak-point shots |
| `DamageTaken` | "You took damage" — quick triple-tap with decay |
| `Reload` | Two-stage mechanical click-clack |
| `Heartbeat` | Low-HP UI, tension cues |

Each preset has **three variants** — Rich, Basic, Minimal — tuned to the corresponding `HapticCapability`. Use the tier-aware playback for cross-device parity:

```csharp
Haptics.PlayPreset(HapticPreset.ExplosionLarge);   // picks the variant for Haptics.Capability
Haptics.PlayPreset(HapticPreset.GunshotShotgun);
```

…or grab a specific variant if you want to manage selection yourself:

```csharp
HapticPattern p = HapticPresets.Get(HapticPreset.ExplosionLarge);                       // current capability
HapticPattern p = HapticPresets.Get(HapticPreset.ExplosionLarge, HapticCapability.Basic); // explicit tier
HapticPattern p = HapticPresets.ExplosionLarge;                                          // always Rich
```

The Rich variant uses Core Haptics-style layered events. Basic uses a longer waveform with an amplitude curve (pulses ≥30 ms so ERM motors have time to ramp up). Minimal uses on/off pulses only (longer durations to convey "strength" without amplitude). All three are built once at class load — calls are allocation-free.

> Sharpness is iOS-only (no analog on Android). `Haptics.Impact(...)` still maps to system UI feedback (`UIImpactFeedbackGenerator` / `VibrationEffect.createPredefined`) regardless of tier — for game events use the presets above instead.

## Platform Support

| Platform | Impact | Notification | Vibrate | PlayPattern (legacy) | PlayPattern (rich events) |
|----------|--------|---------------|---------|----------------------|---------------------------|
| iOS (device, iOS 13+) | UIImpactFeedbackGenerator | UINotificationFeedbackGenerator | No-op | CoreHaptics transient events | CoreHaptics with intensity+sharpness, transient+continuous |
| iOS (device, iOS 10–12) | UIImpactFeedbackGenerator | UINotificationFeedbackGenerator | No-op | No-op | No-op |
| iOS (simulator) | No-op | No-op | No-op | No-op | No-op |
| Android (API 29+) | VibrationEffect predefined | VibrationEffect predefined | VibrationEffect.createOneShot | VibrationEffect.createWaveform | Translated to waveform |
| Android (API 26–28) | VibrationEffect one-shot fallback | VibrationEffect waveform fallback | VibrationEffect.createOneShot | VibrationEffect.createWaveform | Translated to waveform |
| Android (API < 26) | Legacy vibrate | Legacy pattern | Legacy vibrate | Legacy pattern (no amplitude) | Legacy pattern (no amplitude) |
| Editor | No-op | No-op | No-op | No-op | No-op |
| Other (standalone, etc.) | No-op | No-op | No-op | No-op | No-op |

## Limitations

### General

- **Editor:** All calls are no-op. Test on device.
- **iOS Simulator:** All calls are no-op. No haptic hardware.
- **Non-mobile platforms:** `NullHapticsService`; all calls are no-op.

### iOS

- **Vibrate(long):** No-op. iOS has no duration-based vibration API.
- **PlayPattern with rich events:** Requires iOS 13+ (CoreHaptics). No-op on iOS < 13.
- **CHHapticEngine reset:** The engine can stop on app backgrounding, audio-route changes, or hardware reset; the package wires up `stoppedHandler` and `resetHandler` and lazily restarts on next play.

### Android

- **Impact Rigid / Soft:** Approximated (Rigid ≈ Heavy, Soft ≈ Light). No native equivalents.
- **API < 26:** Uses legacy `Vibrator.vibrate(long)` and `vibrate(long[], int)`. No amplitude control for patterns.
- **API 26–28:** Uses `VibrationEffect` but not predefined effects; impact/notification use one-shot or waveform fallbacks.
- **API 29+:** Full predefined effects for impact and notification.
- **Sharpness:** Ignored. Android has no perceptual-sharpness analog to Core Haptics.
- **Continuous events:** Translated to a single sustained slot at `intensity * 255`; effect is coarser than iOS.

### HapticPattern

- **Timings:** Must be non-null and non-empty. Empty patterns are ignored.
- **Amplitudes:** Optional. When provided, length should match `Timings`; values at vibrate positions (even indices) are used.
- **Units (legacy):** All timings in milliseconds, amplitudes 0–255.
- **Units (rich events):** Time and Duration in seconds, Intensity and Sharpness 0..1.

### Throttling

`Haptics.ThrottleIntervalMs` applies a minimum interval per kind+sub-key (e.g. each impact style has its own slot, so spamming Light won't suppress an unrelated Heavy). Default `0` disables throttling.

## Logging

The package emits diagnostic logs through `UnityEngine.Debug.Log*` (C# side) and `NSLog` / `android.util.Log` (native side). Default verbosity is **Warning** — out of the box you get nothing unless something actually goes wrong (engine init failure, empty pattern, simulator detection).

```csharp
using Katlab.Haptics;

Haptics.LogLevel = HapticsLogLevel.Debug;   // verbose tracing
// ...
Haptics.LogLevel = HapticsLogLevel.None;    // fully silent (production)
```

Levels (each includes everything above it):

| Level | What you see |
|-------|--------------|
| `None` | Nothing — even errors suppressed. |
| `Error` | Native-bridge failures (CHHapticEngine init, vibrator unavailable, etc.). |
| `Warning` *(default)* | Errors + recoverable issues (empty patterns, simulator, route changes). |
| `Info` | + service selection, log-level changes, throttle setter, engine lifecycle. |
| `Debug` | + per-call tracing with full pattern event dumps and throttle decisions. |

C#-side messages appear in the Unity Console (prefixed `[katlab.Haptics]`). Native-side messages appear in **Xcode Console** / **Console.app** for iOS and **`adb logcat -s katlab.Haptics`** for Android — they do **not** appear in the Unity Console. Setting `Haptics.LogLevel` propagates the level to the native bridges automatically.

## Samples

Import via Package Manager: **Samples > Basic Usage > Import**. The sample provides a UI to trigger every haptic kind, including a continuous-event "rumble" demo and a throttle slider.

## Contributing / License

MIT License. See [LICENSE](LICENSE) for details. PRs and issues welcome at the repository linked in `package.json`.

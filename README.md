# Haptics

Cross-platform haptics plugin for Unity targeting iOS and Android. Provides a simple API for common presets and advanced custom pattern playback.

## Requirements

- Unity 6 (6000.x)
- iOS 10+ (device; simulator has no haptics)
- Android API 26+ (VibrationEffect); legacy vibrate for older devices

## Installation

**Via Git (UPM):**
```
com.mythicstudio.haptics
```

**Embedded:** Copy the package folder into your project's `Packages/` directory.

## Quick Start

```csharp
using MythicStudio.Haptics;
using MythicStudio.Haptics.Domain;

// Simple presets
Haptics.Impact(HapticImpactStyle.Light);
Haptics.Impact(HapticImpactStyle.Medium);
Haptics.Impact(HapticImpactStyle.Heavy);
Haptics.Notification(HapticNotificationType.Success);
Haptics.Notification(HapticNotificationType.Warning);
Haptics.Notification(HapticNotificationType.Error);

// Android: vibrate for duration
Haptics.Vibrate(100);

// Custom pattern (timings: vibrate, pause, vibrate, pause...)
var pattern = HapticPattern.CreateWaveform(new long[] { 0, 50, 50, 50 }, null);
Haptics.PlayPattern(pattern);

// One-shot
var oneShot = HapticPattern.CreateOneShot(50, 128);
Haptics.PlayPattern(oneShot);

if (Haptics.IsSupported) { /* ... */ }
```

## API Reference

### Static Class: `Haptics`

| Member | Signature | Description |
|--------|-----------|-------------|
| `IsSupported` | `bool` | Whether haptics are supported on the current device. `false` in Editor, iOS Simulator, and unsupported platforms. |
| `Impact` | `void Impact(HapticImpactStyle style)` | Triggers an impact haptic. Styles: Light, Medium, Heavy, Rigid, Soft. |
| `Notification` | `void Notification(HapticNotificationType type)` | Triggers a notification haptic. Types: Success, Warning, Error. |
| `Vibrate` | `void Vibrate(long milliseconds)` | Vibrates for the given duration. **Android only**; no-op on iOS. |
| `PlayPattern` | `void PlayPattern(HapticPattern pattern)` | Plays a custom haptic pattern. |

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

Immutable pattern with timing and optional amplitude data. Timings alternate: vibrate, pause, vibrate, pause... (even indices = vibrate duration, odd = pause).

| Member | Type | Description |
|--------|------|-------------|
| `Timings` | `long[]` | Alternating durations in milliseconds. |
| `Amplitudes` | `int[]` | Optional. Intensity 0–255 per vibrate segment; `null` for default. |

| Static Method | Signature | Description |
|---------------|------------|-------------|
| `CreateOneShot` | `HapticPattern CreateOneShot(long durationMs, int amplitude = -1)` | Single vibration. `amplitude` 0–255 or -1 for default. |
| `CreateWaveform` | `HapticPattern CreateWaveform(long[] timings, int[] amplitudes = null)` | Waveform pattern. `amplitudes` optional. |

**Example:**
```csharp
// Double-tap: vibrate 50ms, pause 50ms, vibrate 50ms
var pattern = HapticPattern.CreateWaveform(new long[] { 50, 50, 50 }, null);
Haptics.PlayPattern(pattern);
```

## Platform Support

| Platform | Impact | Notification | Vibrate | PlayPattern |
|----------|--------|---------------|---------|-------------|
| iOS (device) | UIImpactFeedbackGenerator | UINotificationFeedbackGenerator | No-op | CoreHaptics (iOS 13+) |
| iOS (simulator) | No-op | No-op | No-op | No-op |
| Android (API 26+) | VibrationEffect predefined | VibrationEffect predefined | VibrationEffect.createOneShot | VibrationEffect.createWaveform |
| Android (API &lt; 26) | Legacy vibrate | Legacy pattern | Legacy vibrate | Legacy pattern (no amplitude) |
| Editor | No-op | No-op | No-op | No-op |
| Other (standalone, etc.) | No-op | No-op | No-op | No-op |

## Limitations

### General

- **Editor:** All calls are no-op. Test on device.
- **iOS Simulator:** All calls are no-op. No haptic hardware.
- **Non-mobile platforms:** `NullHapticsService`; all calls are no-op.

### iOS

- **Vibrate(long):** No-op. iOS has no duration-based vibration API.
- **Rigid / Soft:** Require iOS 13+. On older iOS, behavior is undefined.
- **PlayPattern:** Requires iOS 13+ (CoreHaptics). No-op on iOS &lt; 13.
- **PlayPattern:** Creates a new `CHHapticEngine` per call; no engine reuse or caching.

### Android

- **Impact Rigid / Soft:** Approximated (Rigid ≈ Heavy, Soft ≈ Light). No native equivalents.
- **API &lt; 26:** Uses legacy `Vibrator.vibrate(long)` and `vibrate(long[], int)`. No amplitude control for patterns.
- **API 26–28:** Uses `VibrationEffect` but not predefined effects; impact/notification use one-shot or waveform fallbacks.
- **API 29+:** Full predefined effects for impact and notification.
- **Amplitudes in PlayPattern:** Ignored on API &lt; 26 (legacy API).

### HapticPattern

- **Timings:** Must be non-null and non-empty. Empty patterns are ignored.
- **Amplitudes:** Optional. When provided, length can differ from timings; missing entries use default.
- **Units:** All timings in milliseconds.
- **Amplitude range:** 0–255. Values outside range are clamped on iOS.

## Samples

Import via Package Manager: **Samples > Basic Usage > Import**. The sample provides a simple UI to trigger all haptic types. Add the `HapticsSample` component to a GameObject and run on a device.

## License

See your company's licensing terms.

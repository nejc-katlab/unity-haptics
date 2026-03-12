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

| Method | Description |
|--------|-------------|
| `Haptics.Impact(HapticImpactStyle)` | Light, Medium, Heavy, Rigid, Soft |
| `Haptics.Notification(HapticNotificationType)` | Success, Warning, Error |
| `Haptics.Vibrate(long ms)` | Duration in ms (Android) |
| `Haptics.PlayPattern(HapticPattern)` | Custom pattern |
| `Haptics.IsSupported` | Device supports haptics |

## Platform Support

| Platform | Impact/Notification | Vibrate | PlayPattern |
|----------|---------------------|---------|-------------|
| iOS (device) | UIImpactFeedbackGenerator | No-op | CoreHaptics (iOS 13+) |
| iOS (simulator) | No-op | No-op | No-op |
| Android | VibrationEffect / Vibrator | Yes | VibrationEffect |
| Editor | No-op | No-op | No-op |

**Impact styles (Light, Medium, Heavy, Rigid, Soft):** All work on both platforms. On Android, Rigid and Soft are approximated (Rigid≈Heavy, Soft≈Light) due to limited predefined effects.

**Notification types (Success, Warning, Error):** All work on both platforms.

## License

See your company's licensing terms.

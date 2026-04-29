# Basic Usage Sample

1. Import this sample via Package Manager (Samples > Basic Usage > Import).
2. Create a new scene or use an existing one.
3. Create an empty GameObject and add the `HapticsSample` component (uses `KatLab.Haptics` namespace).
4. (Optional) Create a `HapticPatternAsset` via **Assets > Create > KatLab > Haptics > Pattern**, author a few events, and drop it into the `Pattern Asset` slot on the component.
5. Build for iOS or Android and run on a device.

The sample exposes:

- All five impact styles (Light / Medium / Heavy / Rigid / Soft)
- All three notification types (Success / Warning / Error)
- `Vibrate(100)` (Android only)
- A legacy waveform double-tap
- A continuous rumble (Core Haptics, iOS 13+)
- An "Explosion" pattern combining transient peak + continuous rumble + transient tail
- A throttle slider plus a "Spam Light x10" button for testing `ThrottleIntervalMs`
- An optional ScriptableObject pattern asset slot

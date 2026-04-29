namespace Katlab.Haptics
{
    /// <summary>
    /// Hardware/OS haptic capability tier of the current device. Detected once per process and
    /// available via <see cref="Haptics.Capability"/>; can also be set explicitly to force a tier
    /// for testing on a higher-end device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The tier dictates which variant of a preset is selected by
    /// <see cref="Haptics.PlayPreset(HapticPreset)"/> and
    /// <see cref="HapticPresets.Get(HapticPreset)"/>. Higher tiers can play lower-tier patterns
    /// without trouble; lower tiers cannot reproduce the layered events of Rich patterns
    /// faithfully (Galaxy A15-class ERM motors physically can't ramp fast enough).
    /// </para>
    /// </remarks>
    public enum HapticCapability
    {
        /// <summary>No haptic hardware available (iOS Simulator, non-mobile, missing vibrator).</summary>
        None = 0,

        /// <summary>Plain on/off vibrate, no amplitude control. Older Androids (pre-API 26) and basic ERM.</summary>
        Minimal = 1,

        /// <summary>
        /// Amplitude control, no Composition primitives. Mid-range Androids (Galaxy A-series, etc.)
        /// and iOS 10–12. ERM motors usually live here — amplitude is honoured but ramp time is slow,
        /// so Rich patterns with sub-30 ms transients won't feel right.
        /// </summary>
        Basic = 2,

        /// <summary>
        /// Full fidelity. iOS 13+ with Core Haptics, or Android API 30+ with
        /// <c>VibrationEffect.Composition</c> primitives supported. iPhone 7+, Pixel 6+,
        /// Galaxy S22+, OnePlus 9+, etc.
        /// </summary>
        Rich = 3
    }
}

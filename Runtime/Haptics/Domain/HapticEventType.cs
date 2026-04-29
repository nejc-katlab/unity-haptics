namespace KatLab.Haptics.Domain
{
    /// <summary>
    /// Type of a single <see cref="HapticEvent"/>.
    /// On iOS these map directly to Core Haptics event types (iOS 13+); on Android both are
    /// translated to a best-effort waveform (continuous events become a single sustained slot).
    /// </summary>
    public enum HapticEventType
    {
        /// <summary>A short, instantaneous tap (CHHapticEventTypeHapticTransient).</summary>
        Transient = 0,

        /// <summary>A sustained vibration with explicit duration (CHHapticEventTypeHapticContinuous).</summary>
        Continuous = 1
    }
}

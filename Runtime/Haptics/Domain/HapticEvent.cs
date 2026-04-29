namespace KatLab.Haptics.Domain
{
    /// <summary>
    /// A single rich haptic event. Time and Duration are in seconds; Intensity and Sharpness are 0..1.
    /// </summary>
    /// <remarks>
    /// <para><b>Intensity</b> controls strength (0 = nothing, 1 = full).</para>
    /// <para><b>Sharpness</b> controls perceived "crispness" of the feel — lower values feel rounder/duller,
    /// higher values feel sharper/clickier. Sharpness has no effect on Android.</para>
    /// <para><b>Duration</b> is only used for <see cref="HapticEventType.Continuous"/> events.</para>
    /// </remarks>
    public readonly struct HapticEvent
    {
        public readonly float Time;
        public readonly float Duration;
        public readonly float Intensity;
        public readonly float Sharpness;
        public readonly HapticEventType Type;

        public HapticEvent(float time, float duration, float intensity, float sharpness, HapticEventType type)
        {
            Time = time < 0f ? 0f : time;
            Duration = duration < 0f ? 0f : duration;
            Intensity = intensity < 0f ? 0f : (intensity > 1f ? 1f : intensity);
            Sharpness = sharpness < 0f ? 0f : (sharpness > 1f ? 1f : sharpness);
            Type = type;
        }

        /// <summary>
        /// A short tap at <paramref name="time"/> seconds with the given intensity (0..1) and sharpness (0..1).
        /// </summary>
        public static HapticEvent Transient(float time, float intensity = 1f, float sharpness = 0.5f)
            => new HapticEvent(time, 0f, intensity, sharpness, HapticEventType.Transient);

        /// <summary>
        /// A sustained vibration starting at <paramref name="time"/> seconds for <paramref name="duration"/>
        /// seconds with the given intensity (0..1) and sharpness (0..1).
        /// </summary>
        public static HapticEvent Continuous(float time, float duration, float intensity = 1f, float sharpness = 0.5f)
            => new HapticEvent(time, duration, intensity, sharpness, HapticEventType.Continuous);
    }
}

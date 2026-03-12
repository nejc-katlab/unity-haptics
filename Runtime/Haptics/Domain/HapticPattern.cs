namespace MythicStudio.Haptics.Domain
{
    /// <summary>
    /// Immutable haptic pattern with timing and optional amplitude data.
    /// Timings alternate: vibrate, pause, vibrate, pause... (indices 0, 2, 4... = vibrate).
    /// </summary>
    public readonly struct HapticPattern
    {
        public readonly long[] Timings;
        public readonly int[] Amplitudes;

        public HapticPattern(long[] timings, int[] amplitudes = null)
        {
            Timings = timings ?? System.Array.Empty<long>();
            Amplitudes = amplitudes;
        }

        /// <summary>
        /// Creates a single vibration of the given duration. Amplitude 0-255 or -1 for default.
        /// </summary>
        public static HapticPattern CreateOneShot(long durationMs, int amplitude = -1)
        {
            return new HapticPattern(new[] { durationMs }, amplitude >= 0 ? new[] { amplitude } : null);
        }

        /// <summary>
        /// Creates a waveform pattern. Timings: vibrate, pause, vibrate, pause...
        /// Amplitudes optional; use null for default.
        /// </summary>
        public static HapticPattern CreateWaveform(long[] timings, int[] amplitudes = null)
        {
            return new HapticPattern(timings, amplitudes);
        }
    }
}

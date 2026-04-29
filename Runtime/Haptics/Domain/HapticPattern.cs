namespace KatLab.Haptics.Domain
{
    /// <summary>
    /// Immutable haptic pattern. Two flavors are supported:
    /// <list type="bullet">
    ///   <item>
    ///     <description><b>Legacy waveform</b> (<see cref="Timings"/> + optional <see cref="Amplitudes"/>):
    ///     timings alternate vibrate/pause (indices 0, 2, 4… = vibrate). Amplitudes are sampled at
    ///     vibrate positions; the array length should match <see cref="Timings"/>.</description>
    ///   </item>
    ///   <item>
    ///     <description><b>Rich events</b> (<see cref="Events"/>): per-event time, duration, intensity,
    ///     sharpness, and type (transient/continuous). On iOS this drives Core Haptics directly; on
    ///     Android it is translated to a best-effort waveform.</description>
    ///   </item>
    /// </list>
    /// </summary>
    public readonly struct HapticPattern
    {
        public readonly long[] Timings;
        public readonly int[] Amplitudes;
        public readonly HapticEvent[] Events;

        public HapticPattern(long[] timings, int[] amplitudes = null)
        {
            Timings = timings ?? System.Array.Empty<long>();
            Amplitudes = amplitudes;
            Events = null;
        }

        private HapticPattern(HapticEvent[] events)
        {
            Timings = System.Array.Empty<long>();
            Amplitudes = null;
            Events = events ?? System.Array.Empty<HapticEvent>();
        }

        /// <summary>True when the pattern carries a non-empty rich event sequence.</summary>
        public bool HasEvents => Events != null && Events.Length > 0;

        /// <summary>
        /// Creates a single vibration of the given duration. Amplitude 0-255 or -1 for default.
        /// </summary>
        public static HapticPattern CreateOneShot(long durationMs, int amplitude = -1)
        {
            return new HapticPattern(new[] { durationMs }, amplitude >= 0 ? new[] { amplitude } : null);
        }

        /// <summary>
        /// Creates a waveform pattern. Timings: vibrate, pause, vibrate, pause...
        /// Amplitudes optional; when provided, length should match <paramref name="timings"/> and amplitude
        /// values at vibrate positions (even indices) are used.
        /// </summary>
        public static HapticPattern CreateWaveform(long[] timings, int[] amplitudes = null)
        {
            return new HapticPattern(timings, amplitudes);
        }

        /// <summary>
        /// Creates a rich pattern from a sequence of <see cref="HapticEvent"/>s.
        /// On iOS 13+ each event is delivered to Core Haptics with full intensity + sharpness control
        /// (transient or continuous). On other platforms the events are translated to a best-effort
        /// vibration waveform.
        /// </summary>
        public static HapticPattern FromEvents(HapticEvent[] events)
        {
            return new HapticPattern(events);
        }
    }
}

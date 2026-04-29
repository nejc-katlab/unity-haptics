using Katlab.Haptics.Domain;

namespace Katlab.Haptics
{
    /// <summary>
    /// Curated haptic patterns built on <see cref="HapticPattern.FromEvents"/> (Rich tier)
    /// and <see cref="HapticPattern.CreateWaveform"/> (Basic / Minimal fallbacks).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each preset has three variants tuned to <see cref="HapticCapability"/>:
    /// </para>
    /// <list type="bullet">
    ///   <item><b>Rich</b> — multi-event Core Haptics-style pattern with intensity + sharpness.
    ///   Used on iOS 13+ and Android with Composition primitive support.</item>
    ///   <item><b>Basic</b> — longer waveform with amplitude curve. ERM motors live here; pulses
    ///   are ≥30 ms so the motor has time to spin up and be felt.</item>
    ///   <item><b>Minimal</b> — on/off only, no amplitude. Pulses are even longer to convey
    ///   "strength" through duration alone.</item>
    /// </list>
    /// <para>
    /// Use <see cref="Haptics.PlayPreset"/> to play the variant matching the device's
    /// <see cref="Haptics.Capability"/>. The public <c>static readonly</c> fields below
    /// (e.g. <see cref="Click"/>) expose the <b>Rich</b> variant directly for callers that
    /// want to manage tier selection themselves or pass the pattern around.
    /// </para>
    /// <para>
    /// Each preset is built once at class load (allocation-free per call).
    /// </para>
    /// </remarks>
    public static class HapticPresets
    {
        // ─────────────────────────────────────────────────────────────────────
        //  Public Rich variants (also returned by HapticPresets.Get when
        //  Capability == Rich).
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Sharp single click. Sidearms, SMGs.</summary>
        public static readonly HapticPattern Click = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, intensity: 1.0f, sharpness: 1.0f),
        });

        /// <summary>Sharp click + brief crisp tail. Assault rifles.</summary>
        public static readonly HapticPattern Snap = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.005f, duration: 0.04f, intensity: 0.5f, sharpness: 0.7f),
        });

        /// <summary>Punchy boom + low decay tail + ejection click. Shotguns.</summary>
        public static readonly HapticPattern Boom = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.01f, duration: 0.12f, intensity: 0.75f, sharpness: 0.3f),
            HapticEvent.Transient(0.135f, intensity: 0.4f, sharpness: 0.5f),
        });

        /// <summary>Sharp report with long resonant low tail. Bolt-action / sniper rifles.</summary>
        public static readonly HapticPattern BoomDeep = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.02f, duration: 0.25f, intensity: 0.5f, sharpness: 0.2f),
        });

        /// <summary>Quick punch + decay rumble. Grenade, small explosive.</summary>
        public static readonly HapticPattern Burst = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.02f, 0.18f, 0.7f, 0.2f),
            HapticEvent.Transient(0.22f, 0.4f, 0.4f),
        });

        /// <summary>Layered punch + body + debris. RPG, mortar, demo charge.</summary>
        public static readonly HapticPattern BurstHeavy = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.02f, 0.35f, 0.85f, 0.15f),
            HapticEvent.Transient(0.15f, 0.5f, 0.3f),
            HapticEvent.Transient(0.30f, 0.4f, 0.5f),
            HapticEvent.Transient(0.42f, 0.3f, 0.4f),
        });

        /// <summary>Full cinematic blast: peak, body rumble, debris. Bomb, nearby airstrike.</summary>
        public static readonly HapticPattern BurstHuge = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.01f, 0.12f, 1.0f, 0.4f),
            HapticEvent.Continuous(0.13f, 0.55f, 0.85f, 0.1f),
            HapticEvent.Transient(0.20f, 0.6f, 0.3f),
            HapticEvent.Transient(0.40f, 0.5f, 0.4f),
            HapticEvent.Transient(0.55f, 0.4f, 0.3f),
            HapticEvent.Transient(0.70f, 0.3f, 0.2f),
        });

        /// <summary>Deep low rumble, no sharp peak. Far blast, distant artillery.</summary>
        public static readonly HapticPattern Rumble = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Continuous(0f, 0.7f, 0.5f, 0.05f),
            HapticEvent.Transient(0.10f, 0.3f, 0.2f),
            HapticEvent.Transient(0.35f, 0.4f, 0.15f),
        });

        /// <summary>
        /// Game-grade heavy impact. Significantly punchier than <see cref="HapticImpactStyle.Heavy"/>
        /// (which is a UI-grade tap). Body slams, melee hits, heavy landings.
        /// </summary>
        public static readonly HapticPattern Thud = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 0.9f),
            HapticEvent.Continuous(0.01f, 0.05f, 0.6f, 0.4f),
        });

        /// <summary>Extra-punchy hit with brief body. Critical hits, weak-point shots.</summary>
        public static readonly HapticPattern ThudSharp = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.01f, 0.06f, 0.9f, 0.6f),
            HapticEvent.Transient(0.08f, 0.5f, 0.5f),
        });

        /// <summary>Quick triple-tap with decay. "You took damage" feedback.</summary>
        public static readonly HapticPattern TripleTap = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Transient(0.03f, 0.6f, 0.7f),
            HapticEvent.Transient(0.06f, 0.3f, 0.4f),
        });

        /// <summary>Two-stage mechanical click-clack. Reloads, double-tap interactions.</summary>
        public static readonly HapticPattern DoubleTap = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 0.7f, 0.6f),
            HapticEvent.Transient(0.15f, 0.9f, 0.8f),
        });

        /// <summary>Lub-dub cardiac pattern. Low-HP UI, tension cues.</summary>
        public static readonly HapticPattern Heartbeat = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Continuous(0f, 0.08f, 0.7f, 0.3f),
            HapticEvent.Continuous(0.12f, 0.05f, 0.5f, 0.3f),
        });

        // ─────────────────────────────────────────────────────────────────────
        //  Basic-tier variants (ERM with amplitude control). Pulses ≥30 ms so
        //  the motor has time to ramp up; amplitude curves convey "weight".
        //  Format: HapticPattern.CreateWaveform(timings_ms, amplitudes_0_to_255).
        //  Even-indexed timings are vibrate; odd-indexed are pause. Amplitude
        //  array length matches timings; entries at vibrate slots set RPM target.
        // ─────────────────────────────────────────────────────────────────────

        private static readonly HapticPattern Click_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 40 },
            new int[]  { 0, 255 });

        private static readonly HapticPattern Snap_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 30, 5,  40 },
            new int[]  { 0, 255, 0, 130 });

        private static readonly HapticPattern Boom_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 50, 10, 80,  30, 30 },
            new int[]  { 0, 255, 0, 170, 0, 110 });

        private static readonly HapticPattern BoomDeep_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 40, 5,  220 },
            new int[]  { 0, 255, 0, 100 });

        private static readonly HapticPattern Burst_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 50, 10, 180, 30, 40 },
            new int[]  { 0, 255, 0, 180, 0, 100 });

        private static readonly HapticPattern BurstHeavy_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 60, 10, 350, 40, 50, 40, 40 },
            new int[]  { 0, 255, 0, 220, 0, 130, 0, 80 });

        private static readonly HapticPattern BurstHuge_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 80, 10, 600, 50, 50, 50, 30 },
            new int[]  { 0, 255, 0, 200, 0, 130, 0, 80 });

        private static readonly HapticPattern Rumble_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 350, 0,  350 },
            new int[]  { 0, 110, 0,  130 });

        private static readonly HapticPattern Thud_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 30, 5,  50 },
            new int[]  { 0, 255, 0, 150 });

        private static readonly HapticPattern ThudSharp_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 30, 5,  60,  30, 30 },
            new int[]  { 0, 255, 0, 200, 0, 130 });

        private static readonly HapticPattern TripleTap_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 30, 20, 30,  20, 30 },
            new int[]  { 0, 255, 0, 150, 0, 80 });

        private static readonly HapticPattern DoubleTap_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 30,  130, 40 },
            new int[]  { 0, 180, 0,   255 });

        private static readonly HapticPattern Heartbeat_Basic = HapticPattern.CreateWaveform(
            new long[] { 0, 80,  40, 50 },
            new int[]  { 0, 200, 0,  140 });

        // ─────────────────────────────────────────────────────────────────────
        //  Minimal-tier variants (no amplitude control). On/off only; pulse
        //  durations carry the only available "strength" signal. Slightly
        //  longer than Basic since amplitude can't be varied.
        // ─────────────────────────────────────────────────────────────────────

        private static readonly HapticPattern Click_Minimal   = HapticPattern.CreateWaveform(new long[] { 0, 50 });
        private static readonly HapticPattern Snap_Minimal    = HapticPattern.CreateWaveform(new long[] { 0, 40, 10, 30 });
        private static readonly HapticPattern Boom_Minimal  = HapticPattern.CreateWaveform(new long[] { 0, 60, 30, 80, 50, 40 });
        private static readonly HapticPattern BoomDeep_Minimal   = HapticPattern.CreateWaveform(new long[] { 0, 50, 10, 200 });
        private static readonly HapticPattern Burst_Minimal  = HapticPattern.CreateWaveform(new long[] { 0, 60, 20, 180, 50, 40 });
        private static readonly HapticPattern BurstHeavy_Minimal = HapticPattern.CreateWaveform(new long[] { 0, 70, 20, 350, 50, 50, 50, 40 });
        private static readonly HapticPattern BurstHuge_Minimal  = HapticPattern.CreateWaveform(new long[] { 0, 80, 30, 600, 60, 60, 60, 40 });
        private static readonly HapticPattern Rumble_Minimal= HapticPattern.CreateWaveform(new long[] { 0, 700 });
        private static readonly HapticPattern Thud_Minimal     = HapticPattern.CreateWaveform(new long[] { 0, 40, 10, 50 });
        private static readonly HapticPattern ThudSharp_Minimal     = HapticPattern.CreateWaveform(new long[] { 0, 40, 10, 60, 30, 30 });
        private static readonly HapticPattern TripleTap_Minimal     = HapticPattern.CreateWaveform(new long[] { 0, 40, 30, 40, 30, 40 });
        private static readonly HapticPattern DoubleTap_Minimal          = HapticPattern.CreateWaveform(new long[] { 0, 40, 130, 40 });
        private static readonly HapticPattern Heartbeat_Minimal       = HapticPattern.CreateWaveform(new long[] { 0, 80, 40, 50 });

        // ─────────────────────────────────────────────────────────────────────
        //  Lookup
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Returns the variant of <paramref name="preset"/> matching the current
        /// <see cref="Haptics.Capability"/>.</summary>
        public static HapticPattern Get(HapticPreset preset) => Get(preset, Haptics.Capability);

        /// <summary>Returns the variant of <paramref name="preset"/> tuned for the given
        /// <paramref name="capability"/>. None and Minimal share the Minimal variant.</summary>
        public static HapticPattern Get(HapticPreset preset, HapticCapability capability)
        {
            return capability switch
            {
                HapticCapability.Rich    => Rich(preset),
                HapticCapability.Basic   => Basic(preset),
                _                        => Minimal(preset),
            };
        }

        private static HapticPattern Rich(HapticPreset p) => p switch
        {
            HapticPreset.Click    => Click,
            HapticPreset.Snap     => Snap,
            HapticPreset.Boom   => Boom,
            HapticPreset.BoomDeep    => BoomDeep,
            HapticPreset.Burst   => Burst,
            HapticPreset.BurstHeavy  => BurstHeavy,
            HapticPreset.BurstHuge   => BurstHuge,
            HapticPreset.Rumble => Rumble,
            HapticPreset.Thud      => Thud,
            HapticPreset.ThudSharp      => ThudSharp,
            HapticPreset.TripleTap      => TripleTap,
            HapticPreset.DoubleTap    => DoubleTap,
            HapticPreset.Heartbeat        => Heartbeat,
            _                             => Click,
        };

        private static HapticPattern Basic(HapticPreset p) => p switch
        {
            HapticPreset.Click    => Click_Basic,
            HapticPreset.Snap     => Snap_Basic,
            HapticPreset.Boom   => Boom_Basic,
            HapticPreset.BoomDeep    => BoomDeep_Basic,
            HapticPreset.Burst   => Burst_Basic,
            HapticPreset.BurstHeavy  => BurstHeavy_Basic,
            HapticPreset.BurstHuge   => BurstHuge_Basic,
            HapticPreset.Rumble => Rumble_Basic,
            HapticPreset.Thud      => Thud_Basic,
            HapticPreset.ThudSharp      => ThudSharp_Basic,
            HapticPreset.TripleTap      => TripleTap_Basic,
            HapticPreset.DoubleTap           => DoubleTap_Basic,
            HapticPreset.Heartbeat        => Heartbeat_Basic,
            _                             => Click_Basic,
        };

        private static HapticPattern Minimal(HapticPreset p) => p switch
        {
            HapticPreset.Click    => Click_Minimal,
            HapticPreset.Snap     => Snap_Minimal,
            HapticPreset.Boom   => Boom_Minimal,
            HapticPreset.BoomDeep    => BoomDeep_Minimal,
            HapticPreset.Burst   => Burst_Minimal,
            HapticPreset.BurstHeavy  => BurstHeavy_Minimal,
            HapticPreset.BurstHuge   => BurstHuge_Minimal,
            HapticPreset.Rumble => Rumble_Minimal,
            HapticPreset.Thud      => Thud_Minimal,
            HapticPreset.ThudSharp      => ThudSharp_Minimal,
            HapticPreset.TripleTap      => TripleTap_Minimal,
            HapticPreset.DoubleTap           => DoubleTap_Minimal,
            HapticPreset.Heartbeat        => Heartbeat_Minimal,
            _                             => Click_Minimal,
        };
    }
}

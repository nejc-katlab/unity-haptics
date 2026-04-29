namespace Katlab.Haptics
{
    /// <summary>
    /// Named haptic events. Use with <see cref="Haptics.PlayPreset(HapticPreset)"/>
    /// or <see cref="HapticPresets.Get(HapticPreset)"/> to get a pattern variant tuned to the
    /// device's <see cref="HapticCapability"/>.
    /// </summary>
    /// <remarks>
    /// Names describe the <i>feel</i> rather than a specific use case so they read idiomatically
    /// in any context — games, business apps, productivity tools, etc.
    /// </remarks>
    public enum HapticPreset
    {
        /// <summary>Sharp single tap. Sidearms, button confirms, simple alerts.</summary>
        Click = 0,

        /// <summary>Sharp tap with a brief crisp tail. Assault rifles, snappy confirms.</summary>
        Snap = 1,

        /// <summary>Punchy hit + body + ejection click. Shotguns, dramatic confirms.</summary>
        Boom = 2,

        /// <summary>Sharp report with a long resonant low tail. Bolt-action / sniper rifles, thunderous accents.</summary>
        BoomDeep = 3,

        /// <summary>Quick punch with a decay rumble. Grenades, small bursts.</summary>
        Burst = 4,

        /// <summary>Layered punch + body + debris. RPGs, mortars, big celebratory moments.</summary>
        BurstHeavy = 5,

        /// <summary>Full cinematic blast: peak, body rumble, multiple debris transients. Bombs, dramatic finales.</summary>
        BurstHuge = 6,

        /// <summary>Deep low rumble with no sharp peak. Distant blasts, ambient tension, earthquakes.</summary>
        Rumble = 7,

        /// <summary>Sharp + brief body. Body slams, melee hits, heavy landings.</summary>
        Thud = 8,

        /// <summary>Sharp + body + tail tap. Critical hits, weak-point shots, "extra-punchy" moments.</summary>
        ThudSharp = 9,

        /// <summary>Three quick taps decreasing in intensity. Damage taken, escalating warnings.</summary>
        TripleTap = 10,

        /// <summary>Two taps with a gap. Reload click-clack, double-tap interactions.</summary>
        DoubleTap = 11,

        /// <summary>Lub-dub cardiac rhythm. Tension, low-HP UI, waiting indicators.</summary>
        Heartbeat = 12
    }
}

using Katlab.Haptics.Domain;

namespace Katlab.Haptics
{
    /// <summary>
    /// Curated game-grade haptic patterns built on <see cref="HapticPattern.FromEvents"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These patterns are intentionally heavier and more layered than <see cref="HapticImpactStyle"/>,
    /// which maps to UI-grade system feedback (Apple's <c>UIImpactFeedbackGenerator</c> /
    /// Android's <c>VibrationEffect.createPredefined</c>). The system layer is designed for button
    /// taps and switches; for explosions, gunshots, and combat impacts you want custom Core Haptics
    /// events — which is exactly what these presets provide.
    /// </para>
    /// <para>
    /// Each preset is a <c>static readonly</c> <see cref="HapticPattern"/> built once at class load
    /// time, so calls are allocation-free.
    /// </para>
    /// <para>
    /// Full fidelity (intensity + sharpness via Core Haptics) requires iOS 13+. On Android,
    /// patterns translate to a best-effort <c>VibrationEffect.createWaveform</c>; sharpness is
    /// dropped.
    /// </para>
    /// <para>
    /// Usage:
    /// <code>
    /// Haptics.PlayPattern(HapticPresets.ExplosionLarge);
    /// Haptics.PlayPattern(HapticPresets.GunshotShotgun);
    /// </code>
    /// </para>
    /// </remarks>
    public static class HapticPresets
    {
        // ── Gunshots ───────────────────────────────────────────────────────────

        /// <summary>Sharp single click. Sidearms, SMGs.</summary>
        public static readonly HapticPattern GunshotPistol = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, intensity: 1.0f, sharpness: 1.0f),
        });

        /// <summary>Sharp click + brief crisp tail. Assault rifles.</summary>
        public static readonly HapticPattern GunshotRifle = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.005f, duration: 0.04f, intensity: 0.5f, sharpness: 0.7f),
        });

        /// <summary>Punchy boom + low decay tail + ejection click. Shotguns.</summary>
        public static readonly HapticPattern GunshotShotgun = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.01f, duration: 0.12f, intensity: 0.75f, sharpness: 0.3f),
            HapticEvent.Transient(0.135f, intensity: 0.4f, sharpness: 0.5f),
        });

        /// <summary>Sharp report with long resonant low tail. Bolt-action / sniper rifles.</summary>
        public static readonly HapticPattern GunshotSniper = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.02f, duration: 0.25f, intensity: 0.5f, sharpness: 0.2f),
        });

        // ── Explosions ─────────────────────────────────────────────────────────

        /// <summary>Quick punch + decay rumble. Grenade, small explosive.</summary>
        public static readonly HapticPattern ExplosionSmall = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.02f, 0.18f, 0.7f, 0.2f),
            HapticEvent.Transient(0.22f, 0.4f, 0.4f),
        });

        /// <summary>Layered punch + body + debris. RPG, mortar, demo charge.</summary>
        public static readonly HapticPattern ExplosionMedium = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.02f, 0.35f, 0.85f, 0.15f),
            HapticEvent.Transient(0.15f, 0.5f, 0.3f),
            HapticEvent.Transient(0.30f, 0.4f, 0.5f),
            HapticEvent.Transient(0.42f, 0.3f, 0.4f),
        });

        /// <summary>Full cinematic blast: peak, body rumble, debris. Bomb, nearby airstrike.</summary>
        public static readonly HapticPattern ExplosionLarge = HapticPattern.FromEvents(new[]
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
        public static readonly HapticPattern ExplosionDistant = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Continuous(0f, 0.7f, 0.5f, 0.05f),
            HapticEvent.Transient(0.10f, 0.3f, 0.2f),
            HapticEvent.Transient(0.35f, 0.4f, 0.15f),
        });

        // ── Impacts / combat ───────────────────────────────────────────────────

        /// <summary>
        /// Game-grade heavy impact. Significantly punchier than <see cref="HapticImpactStyle.Heavy"/>
        /// (which is a UI-grade tap). Body slams, melee hits, heavy landings.
        /// </summary>
        public static readonly HapticPattern ImpactHeavy = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 0.9f),
            HapticEvent.Continuous(0.01f, 0.05f, 0.6f, 0.4f),
        });

        /// <summary>Extra-punchy hit with brief body. Critical hits, weak-point shots.</summary>
        public static readonly HapticPattern CriticalHit = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Continuous(0.01f, 0.06f, 0.9f, 0.6f),
            HapticEvent.Transient(0.08f, 0.5f, 0.5f),
        });

        /// <summary>Quick triple-tap with decay. "You took damage" feedback.</summary>
        public static readonly HapticPattern DamageTaken = HapticPattern.FromEvents(new[]
        {
            HapticEvent.Transient(0f, 1.0f, 1.0f),
            HapticEvent.Transient(0.03f, 0.6f, 0.7f),
            HapticEvent.Transient(0.06f, 0.3f, 0.4f),
        });

        // ── Misc ───────────────────────────────────────────────────────────────

        /// <summary>Two-stage mechanical click-clack. Magazine in, slide racked.</summary>
        public static readonly HapticPattern Reload = HapticPattern.FromEvents(new[]
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
    }
}

namespace Katlab.Haptics
{
    /// <summary>
    /// Named game-grade haptic events. Use with <see cref="Haptics.PlayPreset(HapticPreset)"/>
    /// or <see cref="HapticPresets.Get(HapticPreset)"/> to get a pattern variant tuned to the
    /// device's <see cref="HapticCapability"/>.
    /// </summary>
    public enum HapticPreset
    {
        // Gunshots
        GunshotPistol = 0,
        GunshotRifle = 1,
        GunshotShotgun = 2,
        GunshotSniper = 3,

        // Explosions
        ExplosionSmall = 4,
        ExplosionMedium = 5,
        ExplosionLarge = 6,
        ExplosionDistant = 7,

        // Combat
        ImpactHeavy = 8,
        CriticalHit = 9,
        DamageTaken = 10,

        // Misc
        Reload = 11,
        Heartbeat = 12
    }
}

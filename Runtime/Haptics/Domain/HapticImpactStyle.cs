namespace KatLab.Haptics.Domain
{
    /// <summary>
    /// Impact haptic styles. Supported on both iOS and Android.
    /// On iOS: native UIImpactFeedbackGenerator (all styles distinct; Rigid/Soft require iOS 13+).
    /// On Android: mapped to VibrationEffect (Rigid approximates Heavy, Soft approximates Light).
    /// </summary>
    public enum HapticImpactStyle
    {
        Light,
        Medium,
        Heavy,
        Rigid,
        Soft
    }
}

namespace KatLab.Haptics.Domain
{
    /// <summary>
    /// Notification haptic types. Supported on both iOS and Android.
    /// On iOS: native UINotificationFeedbackGenerator.
    /// On Android: mapped to VibrationEffect predefined patterns.
    /// </summary>
    public enum HapticNotificationType
    {
        Success,
        Warning,
        Error
    }
}

using MythicStudio.Haptics.Application;
using MythicStudio.Haptics.Domain;
using MythicStudio.Haptics.Infrastructure;

namespace MythicStudio.Haptics
{
    /// <summary>
    /// Cross-platform haptics API for iOS and Android.
    /// </summary>
    public static class Haptics
    {
        /// <summary>
        /// Whether haptics are supported on the current device.
        /// </summary>
        public static bool IsSupported => HapticsServiceFactory.Get().IsSupported;

        /// <summary>
        /// Triggers an impact haptic with the given style.
        /// All styles work on both iOS and Android; Android approximates Rigid as Heavy and Soft as Light.
        /// </summary>
        public static void Impact(HapticImpactStyle style)
        {
            HapticsServiceFactory.Get().Impact(style);
        }

        /// <summary>
        /// Triggers a notification haptic with the given type.
        /// All types work on both iOS and Android.
        /// </summary>
        public static void Notification(HapticNotificationType type)
        {
            HapticsServiceFactory.Get().Notification(type);
        }

        /// <summary>
        /// Vibrates for the specified duration in milliseconds.
        /// Primarily for Android; iOS has no equivalent.
        /// </summary>
        public static void Vibrate(long milliseconds)
        {
            HapticsServiceFactory.Get().Vibrate(milliseconds);
        }

        /// <summary>
        /// Plays a custom haptic pattern.
        /// On Android uses VibrationEffect; on iOS no-op until CoreHaptics is integrated.
        /// </summary>
        public static void PlayPattern(HapticPattern pattern)
        {
            HapticsServiceFactory.Get().PlayPattern(pattern);
        }
    }
}

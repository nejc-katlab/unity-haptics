using Katlab.Haptics.Application;
using Katlab.Haptics.Domain;
using Katlab.Haptics.Infrastructure;

namespace Katlab.Haptics
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
        /// Minimum interval (milliseconds) between haptic events. <c>0</c> disables throttling (default).
        /// When set, repeated calls within this window for the same call-site key are silently dropped.
        /// </summary>
        public static int ThrottleIntervalMs
        {
            get => HapticsThrottle.IntervalMs;
            set => HapticsThrottle.IntervalMs = value;
        }

        /// <summary>
        /// Convenience for <see cref="ThrottleIntervalMs"/>.
        /// </summary>
        public static void SetThrottle(int milliseconds) => HapticsThrottle.IntervalMs = milliseconds;

        /// <summary>
        /// Triggers an impact haptic with the given style.
        /// All styles work on both iOS and Android; Android approximates Rigid as Heavy and Soft as Light.
        /// </summary>
        public static void Impact(HapticImpactStyle style)
        {
            if (!HapticsThrottle.ShouldFire(ThrottleKey.Impact, (int)style)) return;
            HapticsServiceFactory.Get().Impact(style);
        }

        /// <summary>
        /// Triggers a notification haptic with the given type.
        /// All types work on both iOS and Android.
        /// </summary>
        public static void Notification(HapticNotificationType type)
        {
            if (!HapticsThrottle.ShouldFire(ThrottleKey.Notification, (int)type)) return;
            HapticsServiceFactory.Get().Notification(type);
        }

        /// <summary>
        /// Vibrates for the specified duration in milliseconds.
        /// Primarily for Android; iOS has no equivalent.
        /// </summary>
        public static void Vibrate(long milliseconds)
        {
            if (!HapticsThrottle.ShouldFire(ThrottleKey.Vibrate, 0)) return;
            HapticsServiceFactory.Get().Vibrate(milliseconds);
        }

        /// <summary>
        /// Plays a custom haptic pattern.
        /// On iOS uses Core Haptics (iOS 13+) including per-event intensity and sharpness when the pattern carries
        /// rich events (see <see cref="HapticPattern.FromEvents"/>); legacy timing/amplitude patterns also work.
        /// On Android uses VibrationEffect; rich events are translated to a best-effort waveform.
        /// </summary>
        public static void PlayPattern(HapticPattern pattern)
        {
            if (!HapticsThrottle.ShouldFire(ThrottleKey.PlayPattern, 0)) return;
            HapticsServiceFactory.Get().PlayPattern(pattern);
        }
    }
}

using Katlab.Haptics.Domain;

namespace Katlab.Haptics.Application
{
    public interface IHapticsService
    {
        bool IsSupported { get; }

        void Impact(HapticImpactStyle style);

        void Notification(HapticNotificationType type);

        void Vibrate(long milliseconds);

        void PlayPattern(HapticPattern pattern);

        /// <summary>
        /// Propagates the current log level to any platform-specific layers (e.g. native iOS / Android
        /// bridges). Default implementation is a no-op for services that have no native side.
        /// </summary>
        void SetLogLevel(HapticsLogLevel level);
    }
}

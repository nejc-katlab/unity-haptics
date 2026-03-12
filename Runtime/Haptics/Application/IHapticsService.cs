using MythicStudio.Haptics.Domain;

namespace MythicStudio.Haptics.Application
{
    public interface IHapticsService
    {
        bool IsSupported { get; }

        void Impact(HapticImpactStyle style);

        void Notification(HapticNotificationType type);

        void Vibrate(long milliseconds);

        void PlayPattern(HapticPattern pattern);
    }
}

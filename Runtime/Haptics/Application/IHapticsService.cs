using KatLab.Haptics.Domain;

namespace KatLab.Haptics.Application
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

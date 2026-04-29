using KatLab.Haptics.Domain;

namespace KatLab.Haptics.Application
{
    public abstract class HapticsService : IHapticsService
    {
        public abstract bool IsSupported { get; }

        public abstract void Impact(HapticImpactStyle style);

        public abstract void Notification(HapticNotificationType type);

        public abstract void Vibrate(long milliseconds);

        public abstract void PlayPattern(HapticPattern pattern);
    }
}

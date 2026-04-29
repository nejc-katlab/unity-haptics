using Katlab.Haptics.Domain;

namespace Katlab.Haptics.Application
{
    public abstract class HapticsService : IHapticsService
    {
        public abstract bool IsSupported { get; }

        /// <summary>Default returns None; iOS / Android override with platform-specific detection.</summary>
        public virtual HapticCapability Capability => HapticCapability.None;

        public abstract void Impact(HapticImpactStyle style);

        public abstract void Notification(HapticNotificationType type);

        public abstract void Vibrate(long milliseconds);

        public abstract void PlayPattern(HapticPattern pattern);

        /// <summary>Default no-op; override on services that have a native side to forward to.</summary>
        public virtual void SetLogLevel(HapticsLogLevel level) { }
    }
}

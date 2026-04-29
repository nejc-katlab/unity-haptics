using Katlab.Haptics.Application;
using Katlab.Haptics.Domain;

namespace Katlab.Haptics.Infrastructure
{
    internal sealed class NullHapticsService : HapticsService
    {
        private bool _firstCallNoticed;

        public override bool IsSupported => false;

        public override void Impact(HapticImpactStyle style) => NoticeOnce();

        public override void Notification(HapticNotificationType type) => NoticeOnce();

        public override void Vibrate(long milliseconds) => NoticeOnce();

        public override void PlayPattern(HapticPattern pattern) => NoticeOnce();

        private void NoticeOnce()
        {
            if (_firstCallNoticed) return;
            _firstCallNoticed = true;
            HapticsLog.Info("haptics not supported on this platform; calls are silent");
        }
    }
}

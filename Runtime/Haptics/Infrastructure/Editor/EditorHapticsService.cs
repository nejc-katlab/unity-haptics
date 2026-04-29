#if UNITY_EDITOR
using Katlab.Haptics.Application;
using Katlab.Haptics.Domain;

namespace Katlab.Haptics.Infrastructure.Editor
{
    public sealed class EditorHapticsService : HapticsService
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
            HapticsLog.Info("editor: haptics calls are silent on this platform; build to a device to feel them");
        }
    }
}
#endif

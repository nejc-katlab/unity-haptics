#if UNITY_EDITOR
using MythicStudio.Haptics.Application;
using MythicStudio.Haptics.Domain;

namespace MythicStudio.Haptics.Infrastructure.Editor
{
    public sealed class EditorHapticsService : HapticsService
    {
        public override bool IsSupported => false;

        public override void Impact(HapticImpactStyle style) { }

        public override void Notification(HapticNotificationType type) { }

        public override void Vibrate(long milliseconds) { }

        public override void PlayPattern(HapticPattern pattern) { }
    }
}
#endif

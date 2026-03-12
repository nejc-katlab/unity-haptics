#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using MythicStudio.Haptics.Application;
using MythicStudio.Haptics.Domain;

namespace MythicStudio.Haptics.Infrastructure.Android
{
    public sealed class AndroidHapticsService : HapticsService
    {
        private static readonly AndroidJavaClass BridgeClass = new AndroidJavaClass("com.mythicstudio.haptics.HapticsBridge");
        private static bool? _isSupported;

        public override bool IsSupported
        {
            get
            {
                if (!_isSupported.HasValue)
                {
                    _isSupported = BridgeClass.CallStatic<bool>("isSupported");
                }
                return _isSupported.Value;
            }
        }

        public override void Impact(HapticImpactStyle style)
        {
            if (!IsSupported) return;
            BridgeClass.CallStatic("impact", (int)style);
        }

        public override void Notification(HapticNotificationType type)
        {
            if (!IsSupported) return;
            BridgeClass.CallStatic("notification", (int)type);
        }

        public override void Vibrate(long milliseconds)
        {
            if (!IsSupported) return;
            BridgeClass.CallStatic("vibrate", milliseconds);
        }

        public override void PlayPattern(HapticPattern pattern)
        {
            if (!IsSupported) return;
            if (pattern.Timings == null || pattern.Timings.Length == 0) return;
            BridgeClass.CallStatic("vibratePattern", pattern.Timings, pattern.Amplitudes);
        }
    }
}
#endif

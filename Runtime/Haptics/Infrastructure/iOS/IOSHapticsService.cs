#if UNITY_IOS && !UNITY_EDITOR
using MythicStudio.Haptics.Application;
using MythicStudio.Haptics.Domain;

namespace MythicStudio.Haptics.Infrastructure.iOS
{
    public sealed class IOSHapticsService : HapticsService
    {
        private static int? _isSupported;

        public override bool IsSupported
        {
            get
            {
                if (!_isSupported.HasValue)
                {
                    _isSupported = IOSHapticsNative.IsSupported();
                }
                return _isSupported.Value != 0;
            }
        }

        public override void Impact(HapticImpactStyle style)
        {
            if (!IsSupported) return;
            IOSHapticsNative.Impact((int)style);
        }

        public override void Notification(HapticNotificationType type)
        {
            if (!IsSupported) return;
            IOSHapticsNative.Notification((int)type);
        }

        public override void Vibrate(long milliseconds)
        {
        }

        public override void PlayPattern(HapticPattern pattern)
        {
            if (!IsSupported) return;
            if (pattern.Timings == null || pattern.Timings.Length == 0) return;

            unsafe
            {
                fixed (long* timingsPtr = pattern.Timings)
                {
                    System.IntPtr amplitudesPtr = System.IntPtr.Zero;
                    int amplitudeCount = 0;
                    if (pattern.Amplitudes != null && pattern.Amplitudes.Length > 0)
                    {
                        fixed (int* ampPtr = pattern.Amplitudes)
                        {
                            amplitudesPtr = (System.IntPtr)ampPtr;
                            amplitudeCount = pattern.Amplitudes.Length;
                            IOSHapticsNative.PlayPattern((System.IntPtr)timingsPtr, pattern.Timings.Length, amplitudesPtr, amplitudeCount);
                        }
                    }
                    else
                    {
                        IOSHapticsNative.PlayPattern((System.IntPtr)timingsPtr, pattern.Timings.Length, System.IntPtr.Zero, 0);
                    }
                }
            }
        }
    }
}
#endif

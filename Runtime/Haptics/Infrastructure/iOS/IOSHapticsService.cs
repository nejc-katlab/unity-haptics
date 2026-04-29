#if UNITY_IOS && !UNITY_EDITOR
using Katlab.Haptics.Application;
using Katlab.Haptics.Domain;

namespace Katlab.Haptics.Infrastructure.iOS
{
    public sealed class IOSHapticsService : HapticsService
    {
        private static int? _isSupported;
        private static bool _unsupportedWarned;

        public override bool IsSupported
        {
            get
            {
                if (!_isSupported.HasValue)
                {
                    _isSupported = IOSHapticsNative.IsSupported();
                    if (_isSupported.Value == 0 && !_unsupportedWarned)
                    {
                        _unsupportedWarned = true;
                        HapticsLog.Warning("iOS haptics not supported on this device (likely simulator)");
                    }
                }
                return _isSupported.Value != 0;
            }
        }

        public override void SetLogLevel(HapticsLogLevel level)
        {
            IOSHapticsNative.SetLogLevel((int)level);
        }

        public override void Impact(HapticImpactStyle style)
        {
            if (!IsSupported) return;
            if (HapticsLog.IsEnabled(HapticsLogLevel.Debug)) HapticsLog.Debug($"native Impact(style={(int)style})");
            IOSHapticsNative.Impact((int)style);
        }

        public override void Notification(HapticNotificationType type)
        {
            if (!IsSupported) return;
            if (HapticsLog.IsEnabled(HapticsLogLevel.Debug)) HapticsLog.Debug($"native Notification(type={(int)type})");
            IOSHapticsNative.Notification((int)type);
        }

        public override void Vibrate(long milliseconds)
        {
            // iOS has no duration-based vibration API; intentional no-op.
        }

        public override void PlayPattern(HapticPattern pattern)
        {
            if (!IsSupported) return;

            if (pattern.HasEvents)
            {
                PlayRichEvents(pattern.Events);
                return;
            }

            if (pattern.Timings == null || pattern.Timings.Length == 0)
            {
                HapticsLog.Warning("PlayPattern called with empty pattern (no timings, no events) — ignored");
                return;
            }

            if (HapticsLog.IsEnabled(HapticsLogLevel.Debug))
                HapticsLog.Debug($"native PlayPattern(timings={pattern.Timings.Length}, amplitudes={(pattern.Amplitudes?.Length ?? 0)})");

            unsafe
            {
                fixed (long* timingsPtr = pattern.Timings)
                {
                    if (pattern.Amplitudes != null && pattern.Amplitudes.Length > 0)
                    {
                        fixed (int* ampPtr = pattern.Amplitudes)
                        {
                            IOSHapticsNative.PlayPattern(
                                (System.IntPtr)timingsPtr, pattern.Timings.Length,
                                (System.IntPtr)ampPtr, pattern.Amplitudes.Length);
                        }
                    }
                    else
                    {
                        IOSHapticsNative.PlayPattern(
                            (System.IntPtr)timingsPtr, pattern.Timings.Length,
                            System.IntPtr.Zero, 0);
                    }
                }
            }
        }

        private static void PlayRichEvents(HapticEvent[] events)
        {
            int count = events.Length;
            if (count == 0) return;

            if (HapticsLog.IsEnabled(HapticsLogLevel.Debug))
                HapticsLog.Debug($"native PlayEvents(count={count})");

            // Unpack the struct array into parallel primitive arrays so we can pin them for the native call.
            float[] times = new float[count];
            float[] durations = new float[count];
            float[] intensities = new float[count];
            float[] sharpnesses = new float[count];
            int[] types = new int[count];
            for (int i = 0; i < count; i++)
            {
                HapticEvent e = events[i];
                times[i] = e.Time;
                durations[i] = e.Duration;
                intensities[i] = e.Intensity;
                sharpnesses[i] = e.Sharpness;
                types[i] = (int)e.Type;
            }

            unsafe
            {
                fixed (float* tPtr = times)
                fixed (float* dPtr = durations)
                fixed (float* iPtr = intensities)
                fixed (float* sPtr = sharpnesses)
                fixed (int* tyPtr = types)
                {
                    IOSHapticsNative.PlayEvents(
                        (System.IntPtr)tPtr,
                        (System.IntPtr)dPtr,
                        (System.IntPtr)iPtr,
                        (System.IntPtr)sPtr,
                        (System.IntPtr)tyPtr,
                        count);
                }
            }
        }
    }
}
#endif

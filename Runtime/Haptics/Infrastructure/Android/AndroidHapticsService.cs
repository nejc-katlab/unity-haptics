#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using Katlab.Haptics.Application;
using Katlab.Haptics.Domain;

namespace Katlab.Haptics.Infrastructure.Android
{
    public sealed class AndroidHapticsService : HapticsService
    {
        private static readonly AndroidJavaClass BridgeClass = new AndroidJavaClass("dev.katlab.haptics.HapticsBridge");
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

            if (pattern.HasEvents)
            {
                if (TryEventsToWaveform(pattern.Events, out long[] timings, out int[] amplitudes))
                {
                    BridgeClass.CallStatic("vibratePattern", timings, amplitudes);
                }
                return;
            }

            if (pattern.Timings == null || pattern.Timings.Length == 0) return;
            BridgeClass.CallStatic("vibratePattern", pattern.Timings, pattern.Amplitudes);
        }

        // Best-effort translation from rich Core Haptics-style events to an Android waveform.
        // Transient events become a short ~10ms slot; continuous events become a slot of their declared
        // duration. Sharpness is dropped (no Android analog). Intensity 0..1 maps to amplitude 0..255.
        private static bool TryEventsToWaveform(HapticEvent[] events, out long[] timings, out int[] amplitudes)
        {
            timings = null;
            amplitudes = null;
            if (events == null || events.Length == 0) return false;

            const long TransientDurationMs = 10;
            // (vibrate, pause) pairs per event => up to 2*N entries. We always start with a leading 0
            // pause so even-indexed slots are vibrate slots.
            var t = new System.Collections.Generic.List<long>(events.Length * 2 + 1);
            var a = new System.Collections.Generic.List<int>(events.Length * 2 + 1);
            t.Add(0);
            a.Add(0);

            float cursorSeconds = 0f;
            for (int i = 0; i < events.Length; i++)
            {
                HapticEvent e = events[i];
                float startSeconds = e.Time < cursorSeconds ? cursorSeconds : e.Time;
                long pauseMs = (long)System.Math.Round((startSeconds - cursorSeconds) * 1000.0);
                if (pauseMs > 0)
                {
                    t.Add(pauseMs);
                    a.Add(0);
                }

                long durationMs = e.Type == HapticEventType.Continuous
                    ? (long)System.Math.Round(e.Duration * 1000.0)
                    : TransientDurationMs;
                if (durationMs <= 0) durationMs = 1;

                int amp = (int)System.Math.Round(e.Intensity * 255f);
                if (amp < 1) amp = 1;
                if (amp > 255) amp = 255;

                t.Add(durationMs);
                a.Add(amp);

                cursorSeconds = startSeconds + (durationMs / 1000f);
            }

            timings = t.ToArray();
            amplitudes = a.ToArray();
            return true;
        }
    }
}
#endif

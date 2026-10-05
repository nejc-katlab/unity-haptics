#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using Katlab.Haptics.Application;
using Katlab.Haptics.Domain;

namespace Katlab.Haptics.Infrastructure.Android
{
    public sealed class AndroidHapticsService : HapticsService
    {
        private static readonly AndroidJavaClass BridgeClass = new AndroidJavaClass("dev.katlab.haptics.HapticsBridge");
        private static bool _initialized;
        private static bool? _isSupported;
        private static bool _unsupportedWarned;
        private static HapticCapability? _capability;
        private static bool _initFailureLogged;

        // The Java bridge is engine-agnostic and requires init(Context) before any other call.
        // Unity is the one consumer that must do that wiring; we fetch the activity via JNI and
        // hand it over once. Idempotent and cheap after the first call.
        private static bool EnsureInit()
        {
            if (_initialized) return true;
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null)
                    {
                        LogInitFailureOnce("AndroidHapticsService: UnityPlayer.currentActivity returned null — haptics no-op until it is available");
                        return false;
                    }
                    BridgeClass.CallStatic("init", activity);
                    _initialized = true;
                    return true;
                }
            }
            catch (System.Exception e)
            {
                LogInitFailureOnce($"AndroidHapticsService init failed: {e.Message}");
                return false;
            }
        }

        private static void LogInitFailureOnce(string message)
        {
            if (_initFailureLogged) return;
            _initFailureLogged = true;
            HapticsLog.Error(message);
        }

        public override HapticCapability Capability
        {
            get
            {
                if (!EnsureInit()) return HapticCapability.None;
                if (!_capability.HasValue)
                {
                    int raw = BridgeClass.CallStatic<int>("getCapability");
                    _capability = (HapticCapability)raw;
                    HapticsLog.Info($"detected capability: {_capability.Value}");
                }
                return _capability.Value;
            }
        }

        public override bool IsSupported
        {
            get
            {
                if (!EnsureInit()) return false;
                if (!_isSupported.HasValue)
                {
                    _isSupported = BridgeClass.CallStatic<bool>("isSupported");
                    if (!_isSupported.Value && !_unsupportedWarned)
                    {
                        _unsupportedWarned = true;
                        HapticsLog.Warning("Android Vibrator service unavailable on this device");
                    }
                }
                return _isSupported.Value;
            }
        }

        public override void SetLogLevel(HapticsLogLevel level)
        {
            EnsureInit();
            BridgeClass.CallStatic("setLogLevel", (int)level);
        }

        public override void SetCapabilityOverride(HapticCapability? capability)
        {
            BridgeClass.CallStatic("setCapabilityOverride", capability.HasValue ? (int)capability.Value : -1);
        }

        public override void Impact(HapticImpactStyle style)
        {
            if (!IsSupported)
            {
                HapticsLog.Warning($"Impact({style}) skipped — vibrator unsupported on this device");
                return;
            }
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info)) HapticsLog.Info($"native impact(style={(int)style})");
            BridgeClass.CallStatic("impact", (int)style);
        }

        public override void Notification(HapticNotificationType type)
        {
            if (!IsSupported)
            {
                HapticsLog.Warning($"Notification({type}) skipped — vibrator unsupported on this device");
                return;
            }
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info)) HapticsLog.Info($"native notification(type={(int)type})");
            BridgeClass.CallStatic("notification", (int)type);
        }

        public override void Vibrate(long milliseconds)
        {
            if (!IsSupported)
            {
                HapticsLog.Warning($"Vibrate({milliseconds}ms) skipped — vibrator unsupported on this device");
                return;
            }
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info)) HapticsLog.Info($"native vibrate({milliseconds}ms)");
            BridgeClass.CallStatic("vibrate", milliseconds);
        }

        public override void PlayPattern(HapticPattern pattern)
        {
            if (!IsSupported) return;

            if (pattern.HasEvents)
            {
                if (TryEventsToWaveform(pattern.Events, out long[] timings, out int[] amplitudes))
                {
                    if (HapticsLog.IsEnabled(HapticsLogLevel.Info))
                        HapticsLog.Info($"native vibratePattern (from {pattern.Events.Length} events) " +
                                        $"timings=[{string.Join(",", timings)}] amplitudes=[{string.Join(",", amplitudes)}]");
                    BridgeClass.CallStatic("vibratePattern", timings, amplitudes);
                }
                return;
            }

            if (!TryNormalizeWaveform(pattern.Timings, pattern.Amplitudes, out long[] waveTimings, out int[] waveAmplitudes))
                return;

            if (HapticsLog.IsEnabled(HapticsLogLevel.Info))
                HapticsLog.Info($"native vibratePattern timings=[{string.Join(",", waveTimings)}] " +
                                $"amplitudes={(waveAmplitudes == null ? "null" : "[" + string.Join(",", waveAmplitudes) + "]")}");
            BridgeClass.CallStatic("vibratePattern", waveTimings, waveAmplitudes);
        }

        private static bool TryNormalizeWaveform(long[] timings, int[] amplitudes, out long[] outTimings, out int[] outAmplitudes)
        {
            outTimings = timings;
            outAmplitudes = null;
            if (timings == null || timings.Length == 0)
            {
                HapticsLog.Warning("PlayPattern called with empty pattern (no timings, no events) — ignored");
                return false;
            }

            bool anyNonZero = false;
            for (int i = 0; i < timings.Length; i++)
            {
                if (timings[i] < 0)
                {
                    HapticsLog.Warning($"PlayPattern: negative timing {timings[i]} at index {i} — ignored");
                    return false;
                }
                if (timings[i] > 0) anyNonZero = true;
            }
            if (!anyNonZero)
            {
                HapticsLog.Warning("PlayPattern: all timings are zero — ignored");
                return false;
            }

            if (amplitudes == null || amplitudes.Length == 0) return true;

            bool lengthMismatch = amplitudes.Length != timings.Length;
            bool needsCopy = lengthMismatch;
            for (int i = 0; !needsCopy && i < amplitudes.Length; i++)
            {
                if (amplitudes[i] < -1 || amplitudes[i] > 255) needsCopy = true;
            }
            if (!needsCopy)
            {
                outAmplitudes = amplitudes;
                return true;
            }

            if (lengthMismatch)
                HapticsLog.Warning($"PlayPattern: amplitudes length {amplitudes.Length} does not match timings length {timings.Length} — padding with 0 / truncating");

            int[] normalized = new int[timings.Length];
            for (int i = 0; i < normalized.Length; i++)
            {
                int amp = i < amplitudes.Length ? amplitudes[i] : 0;
                if (amp < -1) amp = -1;
                if (amp > 255) amp = 255;
                normalized[i] = amp;
            }
            outAmplitudes = normalized;
            return true;
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

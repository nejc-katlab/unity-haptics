using System.Text;
using Katlab.Haptics.Application;
using Katlab.Haptics.Domain;
using Katlab.Haptics.Infrastructure;

namespace Katlab.Haptics
{
    /// <summary>
    /// Cross-platform haptics API for iOS and Android.
    /// </summary>
    public static class Haptics
    {
        /// <summary>
        /// Whether haptics are supported on the current device.
        /// </summary>
        public static bool IsSupported => HapticsServiceFactory.Get().IsSupported;

        private static HapticCapability? _capabilityOverride;

        /// <summary>
        /// The hardware/OS haptic capability tier of the current device, used to pick preset
        /// variants in <see cref="PlayPreset"/> and <see cref="HapticPresets.Get(HapticPreset)"/>.
        /// Auto-detected on first read; settable to force a tier for testing on a higher-end device.
        /// </summary>
        public static HapticCapability Capability
        {
            get => _capabilityOverride ?? HapticsServiceFactory.Get().Capability;
            set
            {
                _capabilityOverride = value;
                HapticsLog.Info($"capability override set to {value}");
            }
        }

        /// <summary>Clears any explicit capability override and returns to auto-detection.</summary>
        public static void ResetCapability()
        {
            _capabilityOverride = null;
            HapticsLog.Info("capability override cleared");
        }

        /// <summary>
        /// Minimum interval (milliseconds) between haptic events. <c>0</c> disables throttling (default).
        /// When set, repeated calls within this window for the same call-site key are silently dropped.
        /// </summary>
        public static int ThrottleIntervalMs
        {
            get => HapticsThrottle.IntervalMs;
            set => HapticsThrottle.IntervalMs = value;
        }

        /// <summary>
        /// Convenience for <see cref="ThrottleIntervalMs"/>.
        /// </summary>
        public static void SetThrottle(int milliseconds) => HapticsThrottle.IntervalMs = milliseconds;

        /// <summary>
        /// Logging verbosity. Default is <see cref="HapticsLogLevel.Warning"/>.
        /// Setting this also propagates the level to the native iOS / Android bridges; their messages
        /// appear in Xcode Console / <c>adb logcat</c>, not in the Unity Console.
        /// </summary>
        public static HapticsLogLevel LogLevel
        {
            get => HapticsLog.Level;
            set
            {
                if (HapticsLog.Level == value) return;
                HapticsLog.Level = value;
                HapticsLog.Info($"log level set to {value}");
                HapticsServiceFactory.Get().SetLogLevel(value);
            }
        }

        /// <summary>Convenience for <see cref="LogLevel"/>.</summary>
        public static void SetLogLevel(HapticsLogLevel level) => LogLevel = level;

        /// <summary>
        /// Triggers an impact haptic with the given style.
        /// All styles work on both iOS and Android; Android approximates Rigid as Heavy and Soft as Light.
        /// </summary>
        public static void Impact(HapticImpactStyle style)
        {
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info)) HapticsLog.Info($"Impact({style})");
            if (!HapticsThrottle.ShouldFire(ThrottleKey.Impact, (int)style)) return;
            HapticsServiceFactory.Get().Impact(style);
        }

        /// <summary>
        /// Triggers a notification haptic with the given type.
        /// All types work on both iOS and Android.
        /// </summary>
        public static void Notification(HapticNotificationType type)
        {
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info)) HapticsLog.Info($"Notification({type})");
            if (!HapticsThrottle.ShouldFire(ThrottleKey.Notification, (int)type)) return;
            HapticsServiceFactory.Get().Notification(type);
        }

        /// <summary>
        /// Vibrates for the specified duration in milliseconds.
        /// Primarily for Android; iOS has no equivalent.
        /// </summary>
        public static void Vibrate(long milliseconds)
        {
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info)) HapticsLog.Info($"Vibrate({milliseconds}ms)");
            if (!HapticsThrottle.ShouldFire(ThrottleKey.Vibrate, 0)) return;
            HapticsServiceFactory.Get().Vibrate(milliseconds);
        }

        /// <summary>
        /// Plays a custom haptic pattern.
        /// On iOS uses Core Haptics (iOS 13+) including per-event intensity and sharpness when the pattern carries
        /// rich events (see <see cref="HapticPattern.FromEvents"/>); legacy timing/amplitude patterns also work.
        /// On Android uses VibrationEffect; rich events are translated to a best-effort waveform.
        /// </summary>
        public static void PlayPattern(HapticPattern pattern)
        {
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info)) HapticsLog.Info(DescribePattern(pattern));
            if (!HapticsThrottle.ShouldFire(ThrottleKey.PlayPattern, 0)) return;
            HapticsServiceFactory.Get().PlayPattern(pattern);
        }

        /// <summary>
        /// Plays a named preset, automatically picking the variant tuned to the current
        /// <see cref="Capability"/>. The Rich variant uses Core Haptics-style layered events;
        /// the Basic variant uses a longer waveform with amplitude curve (for ERM-class motors);
        /// the Minimal variant uses on/off pulses only.
        /// </summary>
        public static void PlayPreset(HapticPreset preset)
        {
            if (HapticsLog.IsEnabled(HapticsLogLevel.Info))
                HapticsLog.Info($"PlayPreset({preset}) on capability {Capability}");
            PlayPattern(HapticPresets.Get(preset, Capability));
        }

        // Multi-line description for Debug-level logging. Only invoked inside an IsEnabled gate.
        private static string DescribePattern(HapticPattern pattern)
        {
            if (pattern.HasEvents)
            {
                var sb = new StringBuilder(64 + pattern.Events.Length * 80);
                sb.Append("PlayPattern: ").Append(pattern.Events.Length).Append(" events");
                for (int i = 0; i < pattern.Events.Length; i++)
                {
                    HapticEvent e = pattern.Events[i];
                    sb.Append("\n  [").Append(i).Append("] t=").AppendFormat("{0:0.000}", e.Time).Append("s ");
                    if (e.Type == HapticEventType.Continuous)
                    {
                        sb.Append("continuous intensity=").AppendFormat("{0:0.00}", e.Intensity)
                          .Append(" sharpness=").AppendFormat("{0:0.00}", e.Sharpness)
                          .Append(" duration=").AppendFormat("{0:0.000}", e.Duration).Append('s');
                    }
                    else
                    {
                        sb.Append("transient  intensity=").AppendFormat("{0:0.00}", e.Intensity)
                          .Append(" sharpness=").AppendFormat("{0:0.00}", e.Sharpness);
                    }
                }
                return sb.ToString();
            }

            string timings = pattern.Timings != null ? "[" + string.Join(",", pattern.Timings) + "]" : "null";
            string amps = pattern.Amplitudes != null ? "[" + string.Join(",", pattern.Amplitudes) + "]" : "null";
            return $"PlayPattern: legacy waveform timings={timings} amplitudes={amps}";
        }
    }
}

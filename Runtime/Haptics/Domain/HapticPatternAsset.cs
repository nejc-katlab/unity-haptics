using System;
using System.Collections.Generic;
using UnityEngine;

namespace Katlab.Haptics.Domain
{
    /// <summary>
    /// Designer-friendly haptic pattern stored as a Unity asset. Drop this into a serialized field and
    /// call <see cref="Play"/> at runtime; or trigger preview from the inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "katlab/Haptics/Pattern", fileName = "NewHapticPattern", order = 100)]
    public sealed class HapticPatternAsset : ScriptableObject
    {
        [Tooltip("Sequence of events. Time + Duration in seconds; Intensity + Sharpness in 0..1.")]
        [SerializeField] private List<SerializableHapticEvent> events = new List<SerializableHapticEvent>();

        [Tooltip("Multiplier applied to each event's Intensity at playback (0 = silent, 1 = unchanged).")]
        [SerializeField, Range(0f, 2f)] private float intensityScale = 1f;

        [Tooltip("Multiplier applied to each event's Time and Duration at playback (0.5 = half as long).")]
        [SerializeField, Range(0.05f, 4f)] private float timeScale = 1f;

        public IReadOnlyList<SerializableHapticEvent> Events => events;
        public float IntensityScale => intensityScale;
        public float TimeScale => timeScale;

        /// <summary>Builds a runtime <see cref="HapticPattern"/> from this asset, applying the scales.</summary>
        public HapticPattern ToPattern()
        {
            int count = events?.Count ?? 0;
            if (count == 0) return HapticPattern.FromEvents(Array.Empty<HapticEvent>());

            HapticEvent[] runtime = new HapticEvent[count];
            for (int i = 0; i < count; i++)
            {
                SerializableHapticEvent e = events[i];
                float intensity = e.intensity * intensityScale;
                if (intensity > 1f) intensity = 1f;
                runtime[i] = new HapticEvent(
                    e.time * timeScale,
                    e.duration * timeScale,
                    intensity,
                    e.sharpness,
                    e.type);
            }
            return HapticPattern.FromEvents(runtime);
        }

        /// <summary>Convenience wrapper that calls <see cref="Haptics.PlayPattern"/> with this asset.</summary>
        public void Play() => Haptics.PlayPattern(ToPattern());
    }

    /// <summary>
    /// Inspector-serialisable mirror of <see cref="HapticEvent"/>. Unity cannot serialise readonly
    /// structs, so this is the form used in <see cref="HapticPatternAsset"/>.
    /// </summary>
    [Serializable]
    public struct SerializableHapticEvent
    {
        [Tooltip("Start time in seconds (relative to the start of the pattern).")]
        public float time;

        [Tooltip("Duration in seconds — only used for Continuous events.")]
        public float duration;

        [Range(0f, 1f)]
        [Tooltip("Strength of the event (0 = nothing, 1 = full).")]
        public float intensity;

        [Range(0f, 1f)]
        [Tooltip("Perceived 'sharpness' on iOS (lower = duller, higher = clickier). Ignored on Android.")]
        public float sharpness;

        [Tooltip("Transient = a short tap. Continuous = a sustained vibration of the given duration.")]
        public HapticEventType type;
    }
}

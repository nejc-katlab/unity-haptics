using UnityEngine;
using Katlab.Haptics;
using Katlab.Haptics.Domain;

public class HapticsSample : MonoBehaviour
{
    [Tooltip("Optional ScriptableObject pattern asset. If assigned, a 'Play Asset' button is shown.")]
    [SerializeField] private HapticPatternAsset patternAsset;

    private int _throttleMs;
    private Vector2 _scroll;

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
        _scroll = GUILayout.BeginScrollView(_scroll);

        GUILayout.Label($"Haptics supported: {Haptics.IsSupported}");
        GUILayout.Space(8);

        // --- Impacts ---
        GUILayout.Label("Impact");
        if (GUILayout.Button("Light", GUILayout.Height(40))) Haptics.Impact(HapticImpactStyle.Light);
        if (GUILayout.Button("Medium", GUILayout.Height(40))) Haptics.Impact(HapticImpactStyle.Medium);
        if (GUILayout.Button("Heavy", GUILayout.Height(40))) Haptics.Impact(HapticImpactStyle.Heavy);
        if (GUILayout.Button("Rigid", GUILayout.Height(40))) Haptics.Impact(HapticImpactStyle.Rigid);
        if (GUILayout.Button("Soft", GUILayout.Height(40))) Haptics.Impact(HapticImpactStyle.Soft);

        GUILayout.Space(8);

        // --- Notifications ---
        GUILayout.Label("Notification");
        if (GUILayout.Button("Success", GUILayout.Height(40))) Haptics.Notification(HapticNotificationType.Success);
        if (GUILayout.Button("Warning", GUILayout.Height(40))) Haptics.Notification(HapticNotificationType.Warning);
        if (GUILayout.Button("Error", GUILayout.Height(40))) Haptics.Notification(HapticNotificationType.Error);

        GUILayout.Space(8);

        // --- Vibrate (Android only) ---
        if (GUILayout.Button("Vibrate 100ms (Android)", GUILayout.Height(40))) Haptics.Vibrate(100);

        GUILayout.Space(8);

        // --- Legacy waveform ---
        GUILayout.Label("Legacy waveform");
        if (GUILayout.Button("Double tap", GUILayout.Height(40)))
        {
            var pattern = HapticPattern.CreateWaveform(new long[] { 0, 50, 50, 50 }, null);
            Haptics.PlayPattern(pattern);
        }

        GUILayout.Space(8);

        // --- Rich Core Haptics events ---
        GUILayout.Label("Rich events (Core Haptics on iOS 13+)");
        if (GUILayout.Button("Continuous rumble (0.5s)", GUILayout.Height(40)))
        {
            var rumble = HapticPattern.FromEvents(new[]
            {
                HapticEvent.Continuous(0f, 0.5f, intensity: 0.7f, sharpness: 0.2f),
            });
            Haptics.PlayPattern(rumble);
        }
        if (GUILayout.Button("Explosion (peak + rumble + tail)", GUILayout.Height(40)))
        {
            var explosion = HapticPattern.FromEvents(new[]
            {
                HapticEvent.Transient(0f, 1.0f, 1.0f),
                HapticEvent.Continuous(0.02f, 0.35f, 0.7f, 0.2f),
                HapticEvent.Transient(0.4f, 0.4f, 0.5f),
            });
            Haptics.PlayPattern(explosion);
        }

        GUILayout.Space(8);

        // --- Pattern asset ---
        if (patternAsset != null)
        {
            GUILayout.Label($"Asset: {patternAsset.name}");
            if (GUILayout.Button("Play Asset", GUILayout.Height(40))) patternAsset.Play();
            GUILayout.Space(8);
        }

        // --- Throttle ---
        GUILayout.Label($"Throttle: {_throttleMs} ms");
        int newThrottle = (int)GUILayout.HorizontalSlider(_throttleMs, 0, 250, GUILayout.Height(30));
        if (newThrottle != _throttleMs)
        {
            _throttleMs = newThrottle;
            Haptics.ThrottleIntervalMs = _throttleMs;
        }
        if (GUILayout.Button("Spam Light x10 (test throttle)", GUILayout.Height(40)))
        {
            for (int i = 0; i < 10; i++) Haptics.Impact(HapticImpactStyle.Light);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}

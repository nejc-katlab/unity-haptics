using UnityEngine;
using MythicStudio.Haptics;
using MythicStudio.Haptics.Domain;

public class HapticsSample : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
        GUILayout.Label($"Haptics supported: {Haptics.IsSupported}");

        if (GUILayout.Button("Impact Light", GUILayout.Height(50)))
        {
            Haptics.Impact(HapticImpactStyle.Light);
        }
        if (GUILayout.Button("Impact Medium", GUILayout.Height(50)))
        {
            Haptics.Impact(HapticImpactStyle.Medium);
        }
        if (GUILayout.Button("Impact Heavy", GUILayout.Height(50)))
        {
            Haptics.Impact(HapticImpactStyle.Heavy);
        }
        if (GUILayout.Button("Notification Success", GUILayout.Height(50)))
        {
            Haptics.Notification(HapticNotificationType.Success);
        }
        if (GUILayout.Button("Notification Warning", GUILayout.Height(50)))
        {
            Haptics.Notification(HapticNotificationType.Warning);
        }
        if (GUILayout.Button("Notification Error", GUILayout.Height(50)))
        {
            Haptics.Notification(HapticNotificationType.Error);
        }
        if (GUILayout.Button("Vibrate 100ms", GUILayout.Height(50)))
        {
            Haptics.Vibrate(100);
        }
        if (GUILayout.Button("Play Pattern (double tap)", GUILayout.Height(50)))
        {
            var pattern = HapticPattern.CreateWaveform(new long[] { 0, 50, 50, 50 }, null);
            Haptics.PlayPattern(pattern);
        }
        GUILayout.EndArea();
    }
}

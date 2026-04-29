#if UNITY_EDITOR
using Katlab.Haptics.Domain;
using UnityEditor;
using UnityEngine;

namespace Katlab.Haptics.Editor
{
    [CustomEditor(typeof(HapticPatternAsset))]
    internal sealed class HapticPatternAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _events;
        private SerializedProperty _intensityScale;
        private SerializedProperty _timeScale;

        private void OnEnable()
        {
            _events = serializedObject.FindProperty("events");
            _intensityScale = serializedObject.FindProperty("intensityScale");
            _timeScale = serializedObject.FindProperty("timeScale");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_intensityScale);
            EditorGUILayout.PropertyField(_timeScale);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_events, includeChildren: true);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Editor preview is silent — Unity Editor has no haptic hardware. " +
                "Build to a device to feel the pattern.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(_events == null || _events.arraySize == 0))
            {
                if (GUILayout.Button("▶ Play (calls Haptics.PlayPattern on runtime device)", GUILayout.Height(28)))
                {
                    var asset = (HapticPatternAsset)target;
                    asset.Play();
                }
            }

            DrawPatternSummary();
        }

        private void DrawPatternSummary()
        {
            var asset = (HapticPatternAsset)target;
            int eventCount = asset.Events?.Count ?? 0;
            if (eventCount == 0) return;

            float maxEnd = 0f;
            int continuousCount = 0;
            for (int i = 0; i < eventCount; i++)
            {
                var e = asset.Events[i];
                float end = e.time + (e.type == HapticEventType.Continuous ? e.duration : 0f);
                if (end > maxEnd) maxEnd = end;
                if (e.type == HapticEventType.Continuous) continuousCount++;
            }

            float totalDuration = maxEnd * asset.TimeScale;
            EditorGUILayout.LabelField(
                $"{eventCount} event(s) • {continuousCount} continuous • ~{totalDuration:0.00}s after timeScale",
                EditorStyles.miniLabel);
        }
    }
}
#endif

#if UNITY_EDITOR && UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace KatLab.Haptics.Editor
{
    public static class HapticsBuildProcessor
    {
        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;

            string projectPath = PBXProject.GetPBXProjectPath(path);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);

            string targetGuid = project.GetUnityFrameworkTargetGuid();
            project.AddFrameworkToProject(targetGuid, "CoreHaptics.framework", false);

            project.WriteToFile(projectPath);
        }
    }
}
#endif

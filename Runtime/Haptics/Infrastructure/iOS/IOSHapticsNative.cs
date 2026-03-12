using System.Runtime.InteropServices;

namespace MythicStudio.Haptics.Infrastructure.iOS
{
    internal static class IOSHapticsNative
    {
#if UNITY_IOS && !UNITY_EDITOR
        private const string LibraryName = "__Internal";

        [DllImport(LibraryName, EntryPoint = "_Haptics_Impact")]
        public static extern void Impact(int style);

        [DllImport(LibraryName, EntryPoint = "_Haptics_Notification")]
        public static extern void Notification(int type);

        [DllImport(LibraryName, EntryPoint = "_Haptics_IsSupported")]
        public static extern int IsSupported();

        [DllImport(LibraryName, EntryPoint = "_Haptics_PlayPattern")]
        public static extern void PlayPattern(System.IntPtr timings, int timingCount, System.IntPtr amplitudes, int amplitudeCount);
#endif
    }
}

using System.Runtime.InteropServices;

namespace Katlab.Haptics.Infrastructure.iOS
{
    /// <summary>
    /// Mirrors the C struct <c>KatlabHapticEvent</c> defined in <c>HapticsBridge.mm</c>. Sequential
    /// layout so we can pin a managed array and pass its address straight to the native side
    /// without per-field marshalling.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeHapticEvent
    {
        public float time;
        public float duration;
        public float intensity;
        public float sharpness;
        public int type;     // 0 = transient, 1 = continuous
    }

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

        [DllImport(LibraryName, EntryPoint = "_Haptics_GetCapability")]
        public static extern int GetCapability();

        [DllImport(LibraryName, EntryPoint = "_Haptics_PlayPattern")]
        public static extern void PlayPattern(System.IntPtr timings, int timingCount, System.IntPtr amplitudes, int amplitudeCount);

        [DllImport(LibraryName, EntryPoint = "_Haptics_PlayEvents")]
        public static extern void PlayEvents(System.IntPtr events, int count);

        [DllImport(LibraryName, EntryPoint = "_Haptics_SetLogLevel")]
        public static extern void SetLogLevel(int level);
#endif
    }
}

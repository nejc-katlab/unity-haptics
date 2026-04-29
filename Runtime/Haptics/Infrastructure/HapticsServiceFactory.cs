using Katlab.Haptics.Application;

namespace Katlab.Haptics.Infrastructure
{
    internal static class HapticsServiceFactory
    {
        private static IHapticsService _instance;

        public static IHapticsService Get()
        {
            if (_instance != null) return _instance;

#if UNITY_EDITOR
            _instance = new Editor.EditorHapticsService();
#elif UNITY_IOS
            _instance = new iOS.IOSHapticsService();
#elif UNITY_ANDROID
            _instance = new Android.AndroidHapticsService();
#else
            _instance = new NullHapticsService();
#endif
            HapticsLog.Info($"selected service: {_instance.GetType().Name}");
            return _instance;
        }
    }
}

namespace Katlab.Haptics.Application
{
    /// <summary>
    /// Internal logging helper. Routes messages through Unity's Debug.Log family with a
    /// "[katlab.Haptics]" prefix and filters by <see cref="Level"/>.
    /// Hot-path callers should gate with <see cref="IsEnabled"/> before building the message
    /// string so the interpolation cost is paid only when the level is enabled.
    /// </summary>
    internal static class HapticsLog
    {
        private const string Prefix = "[katlab.Haptics] ";
        private const string DebugPrefix = "[katlab.Haptics] [debug] ";

        public static HapticsLogLevel Level = HapticsLogLevel.Warning;

        public static bool IsEnabled(HapticsLogLevel level) => Level >= level;

        public static void Error(string message)
        {
            if (Level >= HapticsLogLevel.Error) UnityEngine.Debug.LogError(Prefix + message);
        }

        public static void Warning(string message)
        {
            if (Level >= HapticsLogLevel.Warning) UnityEngine.Debug.LogWarning(Prefix + message);
        }

        public static void Info(string message)
        {
            if (Level >= HapticsLogLevel.Info) UnityEngine.Debug.Log(Prefix + message);
        }

        public static void Debug(string message)
        {
            if (Level >= HapticsLogLevel.Debug) UnityEngine.Debug.Log(DebugPrefix + message);
        }
    }
}

namespace Katlab.Haptics
{
    /// <summary>
    /// Logging verbosity for the haptics package. Default is <see cref="Warning"/>.
    /// Setting <see cref="Haptics.LogLevel"/> propagates to the native iOS and Android bridges
    /// (their messages appear in Xcode Console / <c>adb logcat</c>, not in the Unity Console).
    /// </summary>
    public enum HapticsLogLevel
    {
        /// <summary>No logs at all — even errors are suppressed.</summary>
        None = 0,

        /// <summary>Only errors (engine init failure, native call failures, etc.).</summary>
        Error = 1,

        /// <summary>Errors and warnings (default). Recoverable issues like empty patterns or simulator detection.</summary>
        Warning = 2,

        /// <summary>Lifecycle events: service selection, engine init, level changes.</summary>
        Info = 3,

        /// <summary>Verbose per-call tracing including full pattern event dumps and throttle decisions.</summary>
        Debug = 4
    }
}

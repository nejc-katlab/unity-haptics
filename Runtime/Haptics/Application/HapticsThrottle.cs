using System.Diagnostics;

namespace KatLab.Haptics.Application
{
    /// <summary>
    /// Lightweight throttle gate used by the public <see cref="Haptics"/> facade to suppress repeated
    /// haptic calls within a configurable window. Independent of Unity's clock — uses
    /// <see cref="Stopwatch"/> so it remains accurate during pauses, time-scale changes, or
    /// background-foreground transitions.
    /// </summary>
    internal static class HapticsThrottle
    {
        // One slot per (kind, sub-key) pair. The sub-key lets us throttle different impact styles
        // independently (e.g. Light spam doesn't suppress an unrelated Heavy hit).
        private const int KindCount = 4;        // Impact, Notification, Vibrate, PlayPattern
        private const int SubKeyCount = 8;      // enough for 5 impact styles / 3 notification types
        private static readonly long[] LastFireTicks = new long[KindCount * SubKeyCount];
        private static readonly Stopwatch Clock = Stopwatch.StartNew();

        public static int IntervalMs;

        public static bool ShouldFire(ThrottleKey kind, int subKey)
        {
            int interval = IntervalMs;
            if (interval <= 0) return true;

            int kindIndex = (int)kind;
            if ((uint)kindIndex >= KindCount) return true;

            int sub = subKey;
            if ((uint)sub >= SubKeyCount) sub = 0;

            int slot = kindIndex * SubKeyCount + sub;
            long now = Clock.ElapsedMilliseconds;
            long last = LastFireTicks[slot];
            if (last != 0 && now - last < interval) return false;

            LastFireTicks[slot] = now;
            return true;
        }
    }

    internal enum ThrottleKey
    {
        Impact = 0,
        Notification = 1,
        Vibrate = 2,
        PlayPattern = 3
    }
}

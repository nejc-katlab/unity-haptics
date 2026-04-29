package dev.katlab.haptics;

import java.util.ArrayList;

/**
 * Fluent builder for constructing rich haptic patterns at runtime, mirroring the iOS
 * {@code KatlabHapticPattern} Obj-C class.
 *
 * <p>Each method appends an event and returns {@code this} so calls can be chained inline.
 * On {@link #play()} the accumulated events are translated to a {@link android.os.VibrationEffect}-style
 * waveform and submitted via {@link HapticsBridge#vibratePattern(long[], int[])}.
 *
 * <p>Times and durations are in seconds. Intensity and sharpness are clamped to 0..1.
 * <b>Sharpness is iOS-only</b> (it has no Android-side hardware analog and is silently ignored
 * here), but the field is kept for API symmetry across platforms.
 *
 * <p>{@link HapticsBridge#init(android.content.Context)} must have been called before {@link #play()}.
 *
 * <p>Usage in Java:
 * <pre>{@code
 * HapticsPatternBuilder.pattern()
 *     .tap(0f, 1f, 1f)
 *     .hold(0.02f, 0.35f, 0.7f, 0.2f)
 *     .tap(0.4f, 0.4f, 0.5f)
 *     .play();
 * }</pre>
 *
 * <p>Usage in Kotlin:
 * <pre>{@code
 * HapticsPatternBuilder.pattern()
 *     .tap(0f, 1f, 1f)
 *     .hold(0.02f, 0.35f, 0.7f, 0.2f)
 *     .tap(0.4f, 0.4f, 0.5f)
 *     .play()
 * }</pre>
 */
public final class HapticsPatternBuilder {

    /** 0 = transient, 1 = continuous. Mirrors the iOS struct. */
    private static final int TYPE_TRANSIENT = 0;
    private static final int TYPE_CONTINUOUS = 1;

    private static final long TRANSIENT_DURATION_MS = 10L;

    private static final class Event {
        float time;
        float duration;
        float intensity;
        float sharpness;
        int type;
    }

    private final ArrayList<Event> events = new ArrayList<>();

    private HapticsPatternBuilder() {}

    public static HapticsPatternBuilder pattern() {
        return new HapticsPatternBuilder();
    }

    /** Append a sharp transient event. */
    public HapticsPatternBuilder tap(float time, float intensity, float sharpness) {
        Event e = new Event();
        e.time = Math.max(0f, time);
        e.duration = 0f;
        e.intensity = clamp01(intensity);
        e.sharpness = clamp01(sharpness);
        e.type = TYPE_TRANSIENT;
        events.add(e);
        return this;
    }

    /** Append a sustained continuous event of the given duration. */
    public HapticsPatternBuilder hold(float time, float duration, float intensity, float sharpness) {
        Event e = new Event();
        e.time = Math.max(0f, time);
        e.duration = Math.max(0f, duration);
        e.intensity = clamp01(intensity);
        e.sharpness = clamp01(sharpness);
        e.type = TYPE_CONTINUOUS;
        events.add(e);
        return this;
    }

    /**
     * Translate the accumulated events to a waveform and submit it via
     * {@link HapticsBridge#vibratePattern(long[], int[])}. No-op if the pattern is empty.
     */
    public void play() {
        if (events.isEmpty()) return;

        // (vibrate, pause) pairs per event => up to 2*N + 1 entries. Always start with a leading 0
        // pause so even-indexed slots are vibrate slots (matches Android's createWaveform expectation).
        ArrayList<Long> timings = new ArrayList<>(events.size() * 2 + 1);
        ArrayList<Integer> amplitudes = new ArrayList<>(events.size() * 2 + 1);
        timings.add(0L);
        amplitudes.add(0);

        float cursorSeconds = 0f;
        for (Event e : events) {
            float startSeconds = e.time < cursorSeconds ? cursorSeconds : e.time;
            long pauseMs = Math.round((startSeconds - cursorSeconds) * 1000.0);
            if (pauseMs > 0) {
                timings.add(pauseMs);
                amplitudes.add(0);
            }

            long durationMs = e.type == TYPE_CONTINUOUS
                ? Math.round(e.duration * 1000.0)
                : TRANSIENT_DURATION_MS;
            if (durationMs <= 0) durationMs = 1;

            int amp = Math.round(e.intensity * 255f);
            if (amp < 1) amp = 1;
            if (amp > 255) amp = 255;

            timings.add(durationMs);
            amplitudes.add(amp);

            cursorSeconds = startSeconds + (durationMs / 1000f);
        }

        long[] tArr = new long[timings.size()];
        int[]  aArr = new int[amplitudes.size()];
        for (int i = 0; i < timings.size(); i++) tArr[i] = timings.get(i);
        for (int i = 0; i < amplitudes.size(); i++) aArr[i] = amplitudes.get(i);

        HapticsBridge.vibratePattern(tArr, aArr);
    }

    private static float clamp01(float v) {
        if (v < 0f) return 0f;
        if (v > 1f) return 1f;
        return v;
    }
}

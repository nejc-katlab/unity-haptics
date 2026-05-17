package dev.katlab.haptics;

import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.os.VibratorManager;

/**
 * Engine-agnostic Android haptics bridge.
 *
 * <p>Call {@link #init(Context)} once during app startup, then call any of the static helpers.
 * If init was never called, methods log an error and silently no-op rather than crashing.
 *
 * <p>This file has no Unity dependency. To use it from a Unity build, the C# wrapper
 * (AndroidHapticsService) calls init via JNI from {@code UnityPlayer.currentActivity};
 * the wiring lives on the C# side.
 */
public class HapticsBridge {
    private static Vibrator vibrator;
    private static Context context;
    private static boolean initWarned;

    private static final long[] NOTIFICATION_SUCCESS_PATTERN = new long[]{0, 30};
    private static final long[] NOTIFICATION_WARNING_ERROR_PATTERN = new long[]{0, 50, 50, 50};

    private static VibrationEffect[] impactEffects;
    private static VibrationEffect[] notificationEffects;
    private static int cachedCapability = -1;

    // Logging — mirrors Katlab.Haptics.HapticsLogLevel: 0=None, 1=Error, 2=Warning (default), 3=Info, 4=Debug.
    // Output goes to logcat under tag "katlab.Haptics".
    private static final String TAG = "katlab.Haptics";
    private static int sLogLevel = 2;

    public static void setLogLevel(int level) {
        sLogLevel = level;
        logI("log level set to " + level);
    }

    private static void logE(String m) { if (sLogLevel >= 1) android.util.Log.e(TAG, m); }
    private static void logW(String m) { if (sLogLevel >= 2) android.util.Log.w(TAG, m); }
    private static void logI(String m) { if (sLogLevel >= 3) android.util.Log.i(TAG, m); }
    private static void logD(String m) { if (sLogLevel >= 4) android.util.Log.d(TAG, m); }

    /**
     * Idempotent init. Must be called once before any other method.
     *
     * <p>Pass any {@link Context} (Activity, Application, Service); the bridge holds onto the
     * application context internally so it survives Activity lifecycle.
     */
    public static void init(Context ctx) {
        if (context != null) return;
        if (ctx == null) {
            logE("init: context is null");
            return;
        }
        context = ctx.getApplicationContext();
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            VibratorManager vm = (VibratorManager) context.getSystemService(Context.VIBRATOR_MANAGER_SERVICE);
            vibrator = vm != null ? vm.getDefaultVibrator() : null;
        } else {
            vibrator = (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);
        }
        if (vibrator == null) {
            logW("Vibrator service unavailable on this device (API " + Build.VERSION.SDK_INT + ")");
        } else {
            logI("Vibrator initialised (API " + Build.VERSION.SDK_INT + ")");
        }
    }

    private static boolean ensureInit() {
        if (context != null) return true;
        if (!initWarned) {
            initWarned = true;
            logE("HapticsBridge.init(Context) was never called — haptic calls will silently no-op. " +
                 "Call HapticsBridge.init(applicationContext) during app startup.");
        }
        return false;
    }

    private static void ensurePredefinedEffects() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.Q) return;
        if (impactEffects != null) return;
        impactEffects = new VibrationEffect[]{
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_TICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_CLICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_HEAVY_CLICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_HEAVY_CLICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_TICK)
        };
        notificationEffects = new VibrationEffect[]{
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_DOUBLE_CLICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_HEAVY_CLICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_HEAVY_CLICK)
        };
        logD("predefined effect cache built");
    }

    public static boolean isSupported() {
        if (!ensureInit()) return false;
        return vibrator != null && vibrator.hasVibrator();
    }

    /**
     * Returns the device's capability tier. Mirrors Katlab.Haptics.HapticCapability:
     *   0 = None, 1 = Minimal, 2 = Basic, 3 = Rich.
     *
     * Rich   = API 30+ AND VibrationEffect.Composition primitives (PRIMITIVE_CLICK + PRIMITIVE_TICK)
     *          are supported by the OEM HAL. Pixel 6+, Galaxy S22+, OnePlus 9+, etc.
     * Basic  = API 26+ with hasAmplitudeControl(). Mid-range Androids (Galaxy A-series, etc.). ERM
     *          motors usually live here — amplitude is honoured but ramp time is slow.
     * Minimal= Plain on/off vibrate. Pre-API 26, or API 26+ without amplitude control.
     * None   = No vibrator hardware at all.
     */
    public static int getCapability() {
        if (cachedCapability >= 0) return cachedCapability;
        if (!ensureInit()) return 0;
        if (vibrator == null || !vibrator.hasVibrator()) {
            logI("capability: None (no vibrator)");
            cachedCapability = 0;
            return 0;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            try {
                int[] required = new int[] {
                    VibrationEffect.Composition.PRIMITIVE_CLICK,
                    VibrationEffect.Composition.PRIMITIVE_TICK
                };
                if (vibrator.areAllPrimitivesSupported(required)) {
                    logI("capability: Rich (Composition primitives supported, API " + Build.VERSION.SDK_INT + ")");
                    cachedCapability = 3;
                    return 3;
                }
            } catch (Throwable t) {
                logW("capability: areAllPrimitivesSupported threw: " + t.getMessage());
            }
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O && vibrator.hasAmplitudeControl()) {
            logI("capability: Basic (amplitude control, API " + Build.VERSION.SDK_INT + ")");
            cachedCapability = 2;
            return 2;
        }
        logI("capability: Minimal (no amplitude control or API " + Build.VERSION.SDK_INT + ")");
        cachedCapability = 1;
        return 1;
    }

    public static void impact(int style) {
        if (!ensureInit()) return;
        if (vibrator == null) {
            logW("impact: vibrator unavailable");
            return;
        }
        logD("impact(style=" + style + ") API=" + Build.VERSION.SDK_INT);

        // Predefined effects (EFFECT_TICK / EFFECT_CLICK / EFFECT_HEAVY_CLICK) are only well-tuned on
        // Rich-tier hardware where the OEM HAL exposes Composition primitives. On Basic/Minimal motors
        // — especially tablet ERMs (Galaxy Tab) — EFFECT_TICK in particular is rendered so subtly that
        // it's effectively silent. Use explicit createOneShot waveforms there so Light/Medium/Heavy
        // remain perceptible.
        int cap = getCapability();
        if (cap >= 3 && Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ensurePredefinedEffects();
            int index = (style >= 0 && style < impactEffects.length) ? style : 1;
            vibrator.vibrate(impactEffects[index]);
            return;
        }

        // Fallback waveforms. Durations are ≥30ms so ERM motors have time to spin up; amplitudes
        // for Basic-tier are set high enough to be felt on weak tablet motors.
        // Style order: Light(0), Medium(1), Heavy(2), Rigid(3), Soft(4).
        long durationMs;
        int amplitude;
        switch (style) {
            case 0: durationMs = 30; amplitude = 130; break;
            case 1: durationMs = 40; amplitude = 200; break;
            case 2: durationMs = 60; amplitude = 255; break;
            case 3: durationMs = 50; amplitude = 255; break;
            case 4: durationMs = 35; amplitude = 110; break;
            default: durationMs = 40; amplitude = 200; break;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            int amp = cap >= 2 ? amplitude : VibrationEffect.DEFAULT_AMPLITUDE;
            vibrator.vibrate(VibrationEffect.createOneShot(durationMs, amp));
        } else {
            vibrator.vibrate(durationMs);
        }
    }

    public static void notification(int type) {
        if (!ensureInit()) return;
        if (vibrator == null) {
            logW("notification: vibrator unavailable");
            return;
        }
        logD("notification(type=" + type + ") API=" + Build.VERSION.SDK_INT);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ensurePredefinedEffects();
            int index = (type >= 0 && type < notificationEffects.length) ? type : 0;
            vibrator.vibrate(notificationEffects[index]);
        } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            long[] pattern = type == 0 ? NOTIFICATION_SUCCESS_PATTERN : NOTIFICATION_WARNING_ERROR_PATTERN;
            vibrator.vibrate(VibrationEffect.createWaveform(pattern, -1));
        } else {
            long[] pattern = type == 0 ? NOTIFICATION_SUCCESS_PATTERN : NOTIFICATION_WARNING_ERROR_PATTERN;
            vibrator.vibrate(pattern, -1);
        }
    }

    public static void vibrate(long milliseconds) {
        if (!ensureInit()) return;
        if (vibrator == null) {
            logW("vibrate: vibrator unavailable");
            return;
        }
        logD("vibrate(" + milliseconds + "ms)");
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            vibrator.vibrate(VibrationEffect.createOneShot(milliseconds, VibrationEffect.DEFAULT_AMPLITUDE));
        } else {
            vibrator.vibrate(milliseconds);
        }
    }

    public static void vibratePattern(long[] timings, int[] amplitudes) {
        if (!ensureInit()) return;
        if (vibrator == null) {
            logW("vibratePattern: vibrator unavailable");
            return;
        }
        if (sLogLevel >= 4) {
            logD("vibratePattern timings.length=" + (timings == null ? 0 : timings.length)
                + " amplitudes.length=" + (amplitudes == null ? 0 : amplitudes.length));
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            if (amplitudes == null || amplitudes.length == 0) {
                vibrator.vibrate(VibrationEffect.createWaveform(timings, -1));
            } else {
                vibrator.vibrate(VibrationEffect.createWaveform(timings, amplitudes, -1));
            }
        } else {
            vibrator.vibrate(timings, -1);
        }
    }
}

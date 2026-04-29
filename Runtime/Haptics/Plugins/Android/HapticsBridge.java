package dev.katlab.haptics;

import android.content.Context;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;
import android.os.VibratorManager;
import com.unity3d.player.UnityPlayer;

public class HapticsBridge {
    private static Vibrator vibrator;
    private static Context context;

    private static final long[] NOTIFICATION_SUCCESS_PATTERN = new long[]{0, 30};
    private static final long[] NOTIFICATION_WARNING_ERROR_PATTERN = new long[]{0, 50, 50, 50};

    private static VibrationEffect[] impactEffects;
    private static VibrationEffect[] notificationEffects;

    // Logging — mirrors Katlab.Haptics.HapticsLogLevel: 0=None, 1=Error, 2=Warning (default), 3=Info, 4=Debug.
    // Set from C# via setLogLevel; output goes to logcat under tag "katlab.Haptics", not Unity's Console.
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

    private static void ensureInit() {
        if (context != null) return;
        try {
            context = UnityPlayer.currentActivity.getApplicationContext();
        } catch (Exception e) {
            logE("ensureInit: UnityPlayer.currentActivity unavailable: " + e.getMessage());
            return;
        }
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

    public static void init(Context ctx) {
        if (context != null) return;
        context = ctx.getApplicationContext();
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            VibratorManager vm = (VibratorManager) context.getSystemService(Context.VIBRATOR_MANAGER_SERVICE);
            vibrator = vm != null ? vm.getDefaultVibrator() : null;
        } else {
            vibrator = (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);
        }
    }

    public static boolean isSupported() {
        ensureInit();
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
        ensureInit();
        if (vibrator == null || !vibrator.hasVibrator()) {
            logI("capability: None (no vibrator)");
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
                    return 3;
                }
            } catch (Throwable t) {
                logW("capability: areAllPrimitivesSupported threw: " + t.getMessage());
            }
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O && vibrator.hasAmplitudeControl()) {
            logI("capability: Basic (amplitude control, API " + Build.VERSION.SDK_INT + ")");
            return 2;
        }
        logI("capability: Minimal (no amplitude control or API " + Build.VERSION.SDK_INT + ")");
        return 1;
    }

    public static void impact(int style) {
        ensureInit();
        if (vibrator == null) {
            logW("impact: vibrator unavailable");
            return;
        }
        logD("impact(style=" + style + ") API=" + Build.VERSION.SDK_INT);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ensurePredefinedEffects();
            int index = (style >= 0 && style < impactEffects.length) ? style : 1;
            vibrator.vibrate(impactEffects[index]);
        } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            long duration = style == 2 ? 50 : 20;
            vibrator.vibrate(VibrationEffect.createOneShot(duration, VibrationEffect.DEFAULT_AMPLITUDE));
        } else {
            vibrator.vibrate(style == 2 ? 50 : 20);
        }
    }

    public static void notification(int type) {
        ensureInit();
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
        ensureInit();
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
        ensureInit();
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

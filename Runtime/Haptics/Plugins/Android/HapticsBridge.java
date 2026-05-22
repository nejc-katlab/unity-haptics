package dev.katlab.haptics;

import android.os.Build;
import android.view.View;
import android.os.Vibrator;
import android.app.Activity;
import android.content.Context;
import android.os.VibrationEffect;
import android.os.VibratorManager;
import android.view.HapticFeedbackConstants;

/**
 * Engine-agnostic Android haptics bridge.
 *
 * <p>Call {@link #init(Context)} once during app startup with an Activity, then call any of the
 * static helpers. If init was never called, methods log an error and silently no-op.
 *
 * <p>This file has no Unity dependency. To use it from a Unity build, the C# wrapper
 * (AndroidHapticsService) calls init via JNI from {@code UnityPlayer.currentActivity};
 * the wiring lives on the C# side.
 */
public class HapticsBridge {
    private static Vibrator vibrator;
    private static Context context;
    private static Activity activity;
    private static View decorView;
    private static boolean initWarned;

    private static final long[] NOTIFICATION_SUCCESS_PATTERN = new long[]{0, 30};
    private static final long[] NOTIFICATION_WARNING_ERROR_PATTERN = new long[]{0, 50, 50, 50};

    private static VibrationEffect[] notificationEffects;
    private static int cachedCapability = -1;
    private static int cachedLightPrimitive = -1;

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
     * Idempotent init. Must be called once before any other method. Prefer passing an Activity
     * so the bridge can use {@link View#performHapticFeedback(int)} (the OS-preferred UI-haptic
     * path); a non-Activity Context still works for the Vibrator fallback ladder.
     */
    public static void init(Context ctx) {
        if (context != null) return;
        if (ctx == null) {
            logE("init: context is null");
            return;
        }
        context = ctx.getApplicationContext();
        if (ctx instanceof Activity) {
            activity = (Activity) ctx;
            try {
                decorView = activity.getWindow().getDecorView();
                logI("init: Activity available — performHapticFeedback path enabled");
            } catch (Throwable t) {
                logW("init: getDecorView threw: " + t.getMessage() + " — performHapticFeedback path disabled");
                decorView = null;
            }
        } else {
            logI("init: ctx is not an Activity — performHapticFeedback path disabled, using Vibrator only");
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

    private static boolean ensureInit() {
        if (context != null) return true;
        if (!initWarned) {
            initWarned = true;
            logE("HapticsBridge.init(Context) was never called — haptic calls will silently no-op. " +
                 "Call HapticsBridge.init(applicationContext) during app startup.");
        }
        return false;
    }

    private static void ensureNotificationEffects() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.Q) return;
        if (notificationEffects != null) return;
        notificationEffects = new VibrationEffect[]{
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_DOUBLE_CLICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_HEAVY_CLICK),
            VibrationEffect.createPredefined(VibrationEffect.EFFECT_HEAVY_CLICK)
        };
        logD("notification effect cache built");
    }

    private static int lightPrimitive() {
        if (cachedLightPrimitive >= 0) return cachedLightPrimitive;
        int picked = VibrationEffect.Composition.PRIMITIVE_TICK;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            try {
                if (vibrator.areAllPrimitivesSupported(VibrationEffect.Composition.PRIMITIVE_LOW_TICK)) {
                    picked = VibrationEffect.Composition.PRIMITIVE_LOW_TICK;
                    logI("impact: using PRIMITIVE_LOW_TICK for Light/Soft");
                }
            } catch (Throwable t) {
                logW("impact: PRIMITIVE_LOW_TICK probe threw: " + t.getMessage());
            }
        }
        cachedLightPrimitive = picked;
        return picked;
    }

    private static boolean playComposition(int style) {
        try {
            VibrationEffect.Composition c = VibrationEffect.startComposition();
            switch (style) {
                case 0:
                    c.addPrimitive(lightPrimitive(), 1.0f);
                    break;
                case 1:
                    c.addPrimitive(VibrationEffect.Composition.PRIMITIVE_CLICK, 0.7f);
                    break;
                case 2:
                    c.addPrimitive(VibrationEffect.Composition.PRIMITIVE_CLICK, 1.0f);
                    break;
                case 3:
                    c.addPrimitive(VibrationEffect.Composition.PRIMITIVE_CLICK, 1.0f);
                    c.addPrimitive(VibrationEffect.Composition.PRIMITIVE_TICK, 0.4f, 20);
                    break;
                case 4: {
                    int p = lightPrimitive();
                    float scale = (p == VibrationEffect.Composition.PRIMITIVE_LOW_TICK) ? 0.6f : 0.3f;
                    c.addPrimitive(p, scale);
                    break;
                }
                default:
                    c.addPrimitive(VibrationEffect.Composition.PRIMITIVE_CLICK, 0.7f);
                    break;
            }
            vibrator.vibrate(c.compose());
            return true;
        } catch (Throwable t) {
            logW("impact: composition failed (style=" + style + "): " + t.getMessage());
            return false;
        }
    }

    public static boolean isSupported() {
        if (!ensureInit()) return false;
        return vibrator != null && vibrator.hasVibrator();
    }

    /**
     * Returns the device's capability tier. Mirrors Katlab.Haptics.HapticCapability:
     *   0 = None, 1 = Minimal, 2 = Basic, 3 = Rich.
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

    private static int feedbackConstantForStyle(int style) {
        switch (style) {
            case 0:
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
                    return HapticFeedbackConstants.CONTEXT_CLICK;
                return HapticFeedbackConstants.KEYBOARD_TAP;
            case 1:
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R)
                    return HapticFeedbackConstants.CONFIRM;
                return HapticFeedbackConstants.LONG_PRESS;
            case 2:
                return HapticFeedbackConstants.LONG_PRESS;
            case 3:
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R)
                    return HapticFeedbackConstants.REJECT;
                return HapticFeedbackConstants.LONG_PRESS;
            case 4:
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
                    return HapticFeedbackConstants.CLOCK_TICK;
                return HapticFeedbackConstants.KEYBOARD_TAP;
            default:
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
                    return HapticFeedbackConstants.CONTEXT_CLICK;
                return HapticFeedbackConstants.KEYBOARD_TAP;
        }
    }

    private static boolean playPerformHapticFeedback(int style) {
        final Activity act = activity;
        final View view = decorView;
        if (act == null || view == null) {
            logD("impact: no Activity/View — skipping performHapticFeedback path");
            return false;
        }
        final int constant = feedbackConstantForStyle(style);
        final int styleCopy = style;
        try {
            act.runOnUiThread(new Runnable() {
                @Override public void run() {
                    try {
                        boolean ok = view.performHapticFeedback(constant);
                        if (ok) {
                            logI("impact: performHapticFeedback fired (style=" + styleCopy + ", constant=" + constant + ")");
                        } else {
                            logW("impact: performHapticFeedback returned false (style=" + styleCopy + ", constant=" + constant + ") — user may have haptics disabled, or constant unsupported");
                        }
                    } catch (Throwable t) {
                        logW("impact: performHapticFeedback threw on UI thread: " + t.getMessage());
                    }
                }
            });
            return true;
        } catch (Throwable t) {
            logW("impact: runOnUiThread threw: " + t.getMessage());
            return false;
        }
    }

    public static void impact(int style) {
        if (!ensureInit()) return;
        if (vibrator == null) {
            logW("impact: vibrator unavailable (style=" + style + ")");
            return;
        }
        logI("impact(style=" + style + ") API=" + Build.VERSION.SDK_INT);

        int cap = getCapability();

        if (cap >= 3 && playPerformHapticFeedback(style)) {
            return;
        }

        if (cap >= 3 && Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
            if (playComposition(style)) {
                logI("impact: composition path (style=" + style + ")");
                return;
            }
        }

        long durationMs;
        int amplitude;
        switch (style) {
            case 0: durationMs = 40; amplitude = 180; break;
            case 1: durationMs = 55; amplitude = 220; break;
            case 2: durationMs = 70; amplitude = 255; break;
            case 3: durationMs = 60; amplitude = 255; break;
            case 4: durationMs = 45; amplitude = 140; break;
            default: durationMs = 55; amplitude = 220; break;
        }
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                int amp = cap >= 2 ? amplitude : VibrationEffect.DEFAULT_AMPLITUDE;
                vibrator.vibrate(VibrationEffect.createOneShot(durationMs, amp));
                logI("impact: createOneShot path (style=" + style + ", " + durationMs + "ms, amp=" + amp + ")");
            } else {
                vibrator.vibrate(durationMs);
                logI("impact: legacy vibrate path (style=" + style + ", " + durationMs + "ms)");
            }
        } catch (Throwable t) {
            logW("impact: vibrate failed (style=" + style + "): " + t.getMessage());
        }
    }

    public static void notification(int type) {
        if (!ensureInit()) return;
        if (vibrator == null) {
            logW("notification: vibrator unavailable");
            return;
        }
        logI("notification(type=" + type + ") API=" + Build.VERSION.SDK_INT);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            ensureNotificationEffects();
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
        logI("vibrate(" + milliseconds + "ms)");
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
        if (sLogLevel >= 3) {
            logI("vibratePattern timings.length=" + (timings == null ? 0 : timings.length)
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

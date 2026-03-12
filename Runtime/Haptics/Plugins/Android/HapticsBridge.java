package com.mythicstudio.haptics;

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

    private static void ensureInit() {
        if (context != null) return;
        try {
            context = UnityPlayer.currentActivity.getApplicationContext();
        } catch (Exception e) {
            return;
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            VibratorManager vm = (VibratorManager) context.getSystemService(Context.VIBRATOR_MANAGER_SERVICE);
            vibrator = vm != null ? vm.getDefaultVibrator() : null;
        } else {
            vibrator = (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);
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

    public static void impact(int style) {
        ensureInit();
        if (vibrator == null) return;
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
        if (vibrator == null) return;
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
        if (vibrator == null) return;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            vibrator.vibrate(VibrationEffect.createOneShot(milliseconds, VibrationEffect.DEFAULT_AMPLITUDE));
        } else {
            vibrator.vibrate(milliseconds);
        }
    }

    public static void vibratePattern(long[] timings, int[] amplitudes) {
        ensureInit();
        if (vibrator == null) return;
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

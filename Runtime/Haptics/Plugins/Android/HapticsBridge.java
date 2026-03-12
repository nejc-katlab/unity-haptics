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
            int effect;
            switch (style) {
                case 0: effect = VibrationEffect.EFFECT_TICK; break;
                case 1: effect = VibrationEffect.EFFECT_CLICK; break;
                case 2: effect = VibrationEffect.EFFECT_HEAVY_CLICK; break;
                case 3: effect = VibrationEffect.EFFECT_HEAVY_CLICK; break;
                case 4: effect = VibrationEffect.EFFECT_TICK; break;
                default: effect = VibrationEffect.EFFECT_CLICK;
            }
            vibrator.vibrate(VibrationEffect.createPredefined(effect));
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
            int effect;
            switch (type) {
                case 0: effect = VibrationEffect.EFFECT_DOUBLE_CLICK; break;
                case 1: effect = VibrationEffect.EFFECT_HEAVY_CLICK; break;
                case 2: effect = VibrationEffect.EFFECT_HEAVY_CLICK; break;
                default: effect = VibrationEffect.EFFECT_DOUBLE_CLICK;
            }
            vibrator.vibrate(VibrationEffect.createPredefined(effect));
        } else if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            long[] pattern = type == 0 ? new long[]{0, 30} : new long[]{0, 50, 50, 50};
            vibrator.vibrate(VibrationEffect.createWaveform(pattern, -1));
        } else {
            long[] pattern = type == 0 ? new long[]{0, 30} : new long[]{0, 50, 50, 50};
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

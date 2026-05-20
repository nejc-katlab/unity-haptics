#import <UIKit/UIKit.h>
#import <CoreHaptics/CoreHaptics.h>

#if __IPHONE_OS_VERSION_MIN_REQUIRED >= __IPHONE_10_0

extern "C" {

/// Single haptic event in a pattern. Pass an array of these to ``_Haptics_PlayEvents``.
/// All times and durations are seconds; intensity/sharpness are 0..1; type is 0=transient, 1=continuous.
typedef struct {
    float time;
    float duration;
    float intensity;
    float sharpness;
    int   type;
} KatlabHapticEvent;

// Logging — mirrors Katlab.Haptics.HapticsLogLevel: None=0, Error=1, Warning=2 (default), Info=3, Debug=4.
// Set from C# via _Haptics_SetLogLevel; output goes to NSLog (Xcode Console / Console.app), not Unity's Console.
static int s_logLevel = 2;

#define KATLAB_LOG_ERROR(FMT, ...)   do { if (s_logLevel >= 1) NSLog(@"[katlab.Haptics][native] [error] "   FMT, ##__VA_ARGS__); } while (0)
#define KATLAB_LOG_WARNING(FMT, ...) do { if (s_logLevel >= 2) NSLog(@"[katlab.Haptics][native] [warning] " FMT, ##__VA_ARGS__); } while (0)
#define KATLAB_LOG_INFO(FMT, ...)    do { if (s_logLevel >= 3) NSLog(@"[katlab.Haptics][native] "          FMT, ##__VA_ARGS__); } while (0)
#define KATLAB_LOG_DEBUG(FMT, ...)   do { if (s_logLevel >= 4) NSLog(@"[katlab.Haptics][native] [debug] "  FMT, ##__VA_ARGS__); } while (0)

void _Haptics_SetLogLevel(int level) {
    s_logLevel = level;
    KATLAB_LOG_INFO(@"log level set to %d", level);
}

// Katlab.Haptics.Domain.HapticImpactStyle order is: Light(0), Medium(1), Heavy(2), Rigid(3), Soft(4).
// UIImpactFeedbackStyle native order is: Light(0), Medium(1), Heavy(2), Soft(3), Rigid(4).
// We map by index so the public C# enum stays stable while the native call uses the correct value.
static const NSInteger kStyleMap[5] = {
    0, // Light  -> UIImpactFeedbackStyleLight
    1, // Medium -> UIImpactFeedbackStyleMedium
    2, // Heavy  -> UIImpactFeedbackStyleHeavy
    4, // Rigid  -> UIImpactFeedbackStyleRigid (iOS 13+)
    3  // Soft   -> UIImpactFeedbackStyleSoft  (iOS 13+)
};
static const int kImpactStyleCount = 5;

// Cache keyed by our enum index (not the iOS native value), so the lookup stays correct
// regardless of platform-style reordering.
static UIImpactFeedbackGenerator* _impactGenerators[kImpactStyleCount];
static UINotificationFeedbackGenerator* _notificationGenerator;

static UIImpactFeedbackGenerator* getImpactGenerator(int ourStyle) {
    int index = ourStyle;
    if (index < 0 || index >= kImpactStyleCount) index = 1;
    if (!_impactGenerators[index]) {
        UIImpactFeedbackStyle nativeStyle = (UIImpactFeedbackStyle)kStyleMap[index];
        _impactGenerators[index] = [[UIImpactFeedbackGenerator alloc] initWithStyle:nativeStyle];
        KATLAB_LOG_DEBUG(@"created UIImpactFeedbackGenerator for our-style=%d (native=%ld)", ourStyle, (long)nativeStyle);
    }
    return _impactGenerators[index];
}

static UINotificationFeedbackGenerator* getNotificationGenerator(void) {
    if (!_notificationGenerator) {
        _notificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
        KATLAB_LOG_DEBUG(@"created UINotificationFeedbackGenerator");
    }
    return _notificationGenerator;
}

void _Haptics_Impact(int style) {
#if TARGET_OS_SIMULATOR
    static BOOL warned = NO;
    if (!warned) { warned = YES; KATLAB_LOG_WARNING(@"simulator: Impact requested but haptics are silent in the iOS simulator"); }
    return;
#else
    KATLAB_LOG_INFO(@"Impact(style=%d)", style);
    UIImpactFeedbackGenerator* generator = getImpactGenerator(style);
    [generator prepare];
    [generator impactOccurred];
#endif
}

void _Haptics_Notification(int type) {
#if TARGET_OS_SIMULATOR
    static BOOL warned = NO;
    if (!warned) { warned = YES; KATLAB_LOG_WARNING(@"simulator: Notification requested but haptics are silent in the iOS simulator"); }
    return;
#else
    KATLAB_LOG_INFO(@"Notification(type=%d)", type);
    UINotificationFeedbackGenerator* generator = getNotificationGenerator();
    [generator prepare];
    [generator notificationOccurred:(UINotificationFeedbackType)type];
#endif
}

int _Haptics_IsSupported(void) {
#if TARGET_OS_SIMULATOR
    return 0;
#else
    return 1;
#endif
}

// 0=None, 1=Minimal, 2=Basic, 3=Rich. Mirrors Katlab.Haptics.HapticCapability.
int _Haptics_GetCapability(void) {
#if TARGET_OS_SIMULATOR
    return 0; // None
#else
    if (@available(iOS 13.0, *)) {
        if ([CHHapticEngine capabilitiesForHardware].supportsHaptics) {
            KATLAB_LOG_INFO(@"capability: Rich (Core Haptics + supportsHaptics)");
            return 3; // Rich
        }
    }
    // iOS 10–12 with Taptic Engine — UIImpact / UINotification work, but rich patterns can't be played.
    KATLAB_LOG_INFO(@"capability: Basic (no Core Haptics support)");
    return 2; // Basic
#endif
}

static BOOL s_engineRunning = NO;

API_AVAILABLE(ios(13.0))
static CHHapticEngine* _Haptics_GetEngine(void) {
    static CHHapticEngine* s_engine = nil;
    static dispatch_once_t s_once;
    dispatch_once(&s_once, ^{
        if (![CHHapticEngine capabilitiesForHardware].supportsHaptics) {
            KATLAB_LOG_ERROR(@"CHHapticEngine: hardware does not support Core Haptics");
            return;
        }
        NSError* error = nil;
        s_engine = [[CHHapticEngine alloc] initAndReturnError:&error];
        if (error || !s_engine) {
            KATLAB_LOG_ERROR(@"CHHapticEngine init failed: %@", error.localizedDescription ?: @"(no description)");
            s_engine = nil;
            return;
        }
        KATLAB_LOG_INFO(@"CHHapticEngine initialised");

        // Engine can stop on app background, AVAudioSession route changes, hardware reset, etc.
        // The stoppedHandler clears the running flag; the next play call will lazily restart.
        s_engine.stoppedHandler = ^(CHHapticEngineStoppedReason reason) {
            KATLAB_LOG_WARNING(@"CHHapticEngine stopped (reason=%ld) — will restart on next play", (long)reason);
            s_engineRunning = NO;
        };
        s_engine.resetHandler = ^{
            KATLAB_LOG_INFO(@"CHHapticEngine reset — restarting");
            s_engineRunning = NO;
            NSError* startError = nil;
            [s_engine startAndReturnError:&startError];
            if (startError) {
                KATLAB_LOG_ERROR(@"CHHapticEngine restart after reset failed: %@", startError.localizedDescription ?: @"(no description)");
            } else {
                s_engineRunning = YES;
            }
        };
    });
    return s_engine;
}

API_AVAILABLE(ios(13.0))
static BOOL _Haptics_EnsureEngineRunning(CHHapticEngine* engine) {
    if (!engine) return NO;
    if (s_engineRunning) return YES;
    NSError* error = nil;
    [engine startAndReturnError:&error];
    if (error) {
        KATLAB_LOG_ERROR(@"CHHapticEngine start failed: %@", error.localizedDescription ?: @"(no description)");
        return NO;
    }
    s_engineRunning = YES;
    return YES;
}

void _Haptics_PlayPattern(const long* timings, int timingCount, const int* amplitudes, int amplitudeCount) {
#if TARGET_OS_SIMULATOR
    return;
#else
    if (@available(iOS 13.0, *)) {
        if (!timings || timingCount < 1) {
            KATLAB_LOG_WARNING(@"PlayPattern: empty timings, ignoring");
            return;
        }
        KATLAB_LOG_INFO(@"PlayPattern: timingCount=%d amplitudeCount=%d", timingCount, amplitudeCount);

        CHHapticEngine* engine = _Haptics_GetEngine();
        if (!engine) return;

        NSMutableArray<CHHapticEvent*>* events = [NSMutableArray array];
        double time = 0;
        for (int i = 0; i < timingCount; i++) {
            long t = timings[i];
            if (i % 2 == 0) {
                // Vibrate slot. Amplitudes follow the same-length-as-timings convention,
                // so we sample amplitudes[i] (not amplitudes[i/2]).
                float intensity = 1.0f;
                if (amplitudes && i < amplitudeCount && amplitudes[i] >= 0) {
                    intensity = (float)amplitudes[i] / 255.0f;
                    if (intensity > 1.0f) intensity = 1.0f;
                }
                CHHapticEventParameter* intensityParam = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensity];
                CHHapticEventParameter* sharpnessParam = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:0.5f];
                CHHapticEvent* event = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticTransient parameters:@[intensityParam, sharpnessParam] relativeTime:time / 1000.0];
                [events addObject:event];
            }
            time += t;
        }
        if (events.count == 0) {
            KATLAB_LOG_WARNING(@"PlayPattern: no vibrate slots in timings, nothing to play");
            return;
        }

        NSError* error = nil;
        CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithEvents:events parameters:@[] error:&error];
        if (error || !pattern) {
            KATLAB_LOG_ERROR(@"CHHapticPattern init failed: %@", error.localizedDescription ?: @"(no description)");
            return;
        }

        if (!_Haptics_EnsureEngineRunning(engine)) return;

        id<CHHapticPatternPlayer> player = [engine createPlayerWithPattern:pattern error:&error];
        if (error || !player) {
            KATLAB_LOG_ERROR(@"createPlayerWithPattern failed: %@", error.localizedDescription ?: @"(no description)");
            return;
        }
        [player startAtTime:0 error:&error];
        if (error) {
            KATLAB_LOG_ERROR(@"player startAtTime failed: %@", error.localizedDescription ?: @"(no description)");
        }
    }
#endif
}

void _Haptics_PlayEvents(const KatlabHapticEvent* events_in, int count) {
#if TARGET_OS_SIMULATOR
    return;
#else
    if (@available(iOS 13.0, *)) {
        if (!events_in || count < 1) {
            KATLAB_LOG_WARNING(@"PlayEvents: invalid arguments, ignoring");
            return;
        }
        KATLAB_LOG_INFO(@"PlayEvents: count=%d", count);

        CHHapticEngine* engine = _Haptics_GetEngine();
        if (!engine) return;

        NSMutableArray<CHHapticEvent*>* events = [NSMutableArray arrayWithCapacity:count];
        for (int i = 0; i < count; i++) {
            KatlabHapticEvent ev_in = events_in[i];
            float intensity = ev_in.intensity;
            if (intensity < 0.0f) intensity = 0.0f;
            if (intensity > 1.0f) intensity = 1.0f;
            float sharpness = ev_in.sharpness;
            if (sharpness < 0.0f) sharpness = 0.0f;
            if (sharpness > 1.0f) sharpness = 1.0f;

            CHHapticEventParameter* intensityParam = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensity];
            CHHapticEventParameter* sharpnessParam = [[CHHapticEventParameter alloc] initWithParameterID:CHHapticEventParameterIDHapticSharpness value:sharpness];

            CHHapticEventType eventType = (ev_in.type == 1) ? CHHapticEventTypeHapticContinuous : CHHapticEventTypeHapticTransient;
            double relTime = (double)ev_in.time;
            double duration = (eventType == CHHapticEventTypeHapticContinuous) ? (double)ev_in.duration : 0.0;

            CHHapticEvent* ev;
            if (eventType == CHHapticEventTypeHapticContinuous) {
                ev = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticContinuous
                                                   parameters:@[intensityParam, sharpnessParam]
                                                 relativeTime:relTime
                                                     duration:duration];
                KATLAB_LOG_DEBUG(@"  [%d] continuous t=%.3fs dur=%.3fs intensity=%.2f sharpness=%.2f",
                                 i, relTime, duration, intensity, sharpness);
            } else {
                ev = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticTransient
                                                   parameters:@[intensityParam, sharpnessParam]
                                                 relativeTime:relTime];
                KATLAB_LOG_DEBUG(@"  [%d] transient  t=%.3fs intensity=%.2f sharpness=%.2f",
                                 i, relTime, intensity, sharpness);
            }
            [events addObject:ev];
        }
        if (events.count == 0) {
            KATLAB_LOG_WARNING(@"PlayEvents: zero events after filtering, nothing to play");
            return;
        }

        NSError* error = nil;
        CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithEvents:events parameters:@[] error:&error];
        if (error || !pattern) {
            KATLAB_LOG_ERROR(@"CHHapticPattern init failed: %@", error.localizedDescription ?: @"(no description)");
            return;
        }

        if (!_Haptics_EnsureEngineRunning(engine)) return;

        id<CHHapticPatternPlayer> player = [engine createPlayerWithPattern:pattern error:&error];
        if (error || !player) {
            KATLAB_LOG_ERROR(@"createPlayerWithPattern failed: %@", error.localizedDescription ?: @"(no description)");
            return;
        }
        [player startAtTime:0 error:&error];
        if (error) {
            KATLAB_LOG_ERROR(@"player startAtTime failed: %@", error.localizedDescription ?: @"(no description)");
        }
    }
#endif
}

}

#endif

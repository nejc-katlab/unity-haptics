#import <UIKit/UIKit.h>
#import <CoreHaptics/CoreHaptics.h>

#if __IPHONE_OS_VERSION_MIN_REQUIRED >= __IPHONE_10_0

extern "C" {

// KatLab.Haptics.Domain.HapticImpactStyle order is: Light(0), Medium(1), Heavy(2), Rigid(3), Soft(4).
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
    }
    return _impactGenerators[index];
}

static UINotificationFeedbackGenerator* getNotificationGenerator(void) {
    if (!_notificationGenerator) {
        _notificationGenerator = [[UINotificationFeedbackGenerator alloc] init];
    }
    return _notificationGenerator;
}

void _Haptics_Impact(int style) {
#if TARGET_OS_SIMULATOR
    return;
#else
    UIImpactFeedbackGenerator* generator = getImpactGenerator(style);
    [generator prepare];
    [generator impactOccurred];
#endif
}

void _Haptics_Notification(int type) {
#if TARGET_OS_SIMULATOR
    return;
#else
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

API_AVAILABLE(ios(13.0))
static CHHapticEngine* _Haptics_GetEngine(void) {
    static CHHapticEngine* s_engine = nil;
    static dispatch_once_t s_once;
    dispatch_once(&s_once, ^{
        if (![CHHapticEngine supportsHardware]) return;
        NSError* error = nil;
        s_engine = [[CHHapticEngine alloc] initAndReturnError:&error];
        if (error || !s_engine) {
            s_engine = nil;
            return;
        }
        // Engine can stop on app background, AVAudioSession route changes, hardware reset, etc.
        // The stoppedHandler clears the running flag; the next play call will lazily restart.
        s_engine.stoppedHandler = ^(CHHapticEngineStoppedReason reason) {
            // Intentionally empty: engine.running becomes false; we re-start on next play.
            (void)reason;
        };
        s_engine.resetHandler = ^{
            NSError* startError = nil;
            [s_engine startAndReturnError:&startError];
        };
    });
    return s_engine;
}

API_AVAILABLE(ios(13.0))
static BOOL _Haptics_EnsureEngineRunning(CHHapticEngine* engine) {
    if (!engine) return NO;
    NSError* error = nil;
    [engine startAndReturnError:&error];
    return error == nil;
}

void _Haptics_PlayPattern(const long* timings, int timingCount, const int* amplitudes, int amplitudeCount) {
#if TARGET_OS_SIMULATOR
    return;
#else
    if (@available(iOS 13.0, *)) {
        if (!timings || timingCount < 1) return;

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
                CHHapticEventParameter* intensityParam = [CHHapticEventParameter parameterWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensity];
                CHHapticEventParameter* sharpnessParam = [CHHapticEventParameter parameterWithParameterID:CHHapticEventParameterIDHapticSharpness value:0.5f];
                CHHapticEvent* event = [CHHapticEvent eventWithEventType:CHHapticEventTypeHapticTransient parameters:@[intensityParam, sharpnessParam] relativeTime:time / 1000.0];
                [events addObject:event];
            }
            time += t;
        }
        if (events.count == 0) return;

        NSError* error = nil;
        CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithEvents:events parameters:@[] error:&error];
        if (error || !pattern) return;

        if (!engine.running && !_Haptics_EnsureEngineRunning(engine)) return;

        id<CHHapticPatternPlayer> player = [engine createPlayerWithPattern:pattern error:&error];
        if (error || !player) return;
        [player startAtTime:0 error:&error];
    }
#endif
}

void _Haptics_PlayEvents(const float* times,
                         const float* durations,
                         const float* intensities,
                         const float* sharpnesses,
                         const int* types,
                         int count) {
#if TARGET_OS_SIMULATOR
    return;
#else
    if (@available(iOS 13.0, *)) {
        if (!times || !intensities || !sharpnesses || !types || count < 1) return;

        CHHapticEngine* engine = _Haptics_GetEngine();
        if (!engine) return;

        NSMutableArray<CHHapticEvent*>* events = [NSMutableArray arrayWithCapacity:count];
        for (int i = 0; i < count; i++) {
            float intensity = intensities[i];
            if (intensity < 0.0f) intensity = 0.0f;
            if (intensity > 1.0f) intensity = 1.0f;
            float sharpness = sharpnesses[i];
            if (sharpness < 0.0f) sharpness = 0.0f;
            if (sharpness > 1.0f) sharpness = 1.0f;

            CHHapticEventParameter* intensityParam = [CHHapticEventParameter parameterWithParameterID:CHHapticEventParameterIDHapticIntensity value:intensity];
            CHHapticEventParameter* sharpnessParam = [CHHapticEventParameter parameterWithParameterID:CHHapticEventParameterIDHapticSharpness value:sharpness];

            CHHapticEventType eventType = (types[i] == 1) ? CHHapticEventTypeHapticContinuous : CHHapticEventTypeHapticTransient;
            double relTime = (double)times[i];
            double duration = (durations && eventType == CHHapticEventTypeHapticContinuous) ? (double)durations[i] : 0.0;

            CHHapticEvent* ev;
            if (eventType == CHHapticEventTypeHapticContinuous) {
                ev = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticContinuous
                                                   parameters:@[intensityParam, sharpnessParam]
                                                 relativeTime:relTime
                                                     duration:duration];
            } else {
                ev = [[CHHapticEvent alloc] initWithEventType:CHHapticEventTypeHapticTransient
                                                   parameters:@[intensityParam, sharpnessParam]
                                                 relativeTime:relTime];
            }
            [events addObject:ev];
        }
        if (events.count == 0) return;

        NSError* error = nil;
        CHHapticPattern* pattern = [[CHHapticPattern alloc] initWithEvents:events parameters:@[] error:&error];
        if (error || !pattern) return;

        if (!engine.running && !_Haptics_EnsureEngineRunning(engine)) return;

        id<CHHapticPatternPlayer> player = [engine createPlayerWithPattern:pattern error:&error];
        if (error || !player) return;
        [player startAtTime:0 error:&error];
    }
#endif
}

}

#endif

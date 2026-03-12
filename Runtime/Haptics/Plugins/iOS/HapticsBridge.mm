#import <UIKit/UIKit.h>
#import <CoreHaptics/CoreHaptics.h>

#if __IPHONE_OS_VERSION_MIN_REQUIRED >= __IPHONE_10_0

extern "C" {

static UIImpactFeedbackGenerator* _impactGenerators[5];
static UINotificationFeedbackGenerator* _notificationGenerator;
static const int kImpactStyleCount = 5;

static UIImpactFeedbackGenerator* getImpactGenerator(UIImpactFeedbackStyle style) {
    int index = (int)style;
    if (index < 0 || index >= kImpactStyleCount) index = 1;
    if (!_impactGenerators[index]) {
        _impactGenerators[index] = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
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
    UIImpactFeedbackGenerator* generator = getImpactGenerator((UIImpactFeedbackStyle)style);
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

void _Haptics_PlayPattern(const long* timings, int timingCount, const int* amplitudes, int amplitudeCount) {
#if TARGET_OS_SIMULATOR
    return;
#else
    if (@available(iOS 13.0, *)) {
        if (![CHHapticEngine supportsHardware]) return;
        if (!timings || timingCount < 1) return;

        static CHHapticEngine* s_engine = nil;
        static dispatch_once_t s_once;
        dispatch_once(&s_once, ^{
            NSError* error = nil;
            s_engine = [[CHHapticEngine alloc] initAndReturnError:&error];
        });
        if (!s_engine) return;

        NSMutableArray<CHHapticEvent*>* events = [NSMutableArray array];
        double time = 0;
        for (int i = 0; i < timingCount; i++) {
            long t = timings[i];
            if (i % 2 == 0) {
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

        if (!s_engine.running) {
            [s_engine startAndReturnError:&error];
            if (error) return;
        }

        id<CHHapticPatternPlayer> player = [s_engine createPlayerWithPattern:pattern error:&error];
        if (error || !player) return;
        [player startAtTime:0 error:&error];
    }
#endif
}

}

#endif

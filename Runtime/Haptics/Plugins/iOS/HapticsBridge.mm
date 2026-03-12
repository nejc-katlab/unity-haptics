#import <UIKit/UIKit.h>
#import <CoreHaptics/CoreHaptics.h>

#if __IPHONE_OS_VERSION_MIN_REQUIRED >= __IPHONE_10_0

extern "C" {

void _Haptics_Impact(int style) {
#if TARGET_OS_SIMULATOR
    return;
#else
    UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:(UIImpactFeedbackStyle)style];
    [generator prepare];
    [generator impactOccurred];
#endif
}

void _Haptics_Notification(int type) {
#if TARGET_OS_SIMULATOR
    return;
#else
    UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
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

        CHHapticEngine* engine = [[CHHapticEngine alloc] initAndReturnError:&error];
        if (error || !engine) return;
        [engine startAndReturnError:&error];
        if (error) return;

        id<CHHapticPatternPlayer> player = [engine createPlayerWithPattern:pattern error:&error];
        if (error || !player) return;
        [player startAtTime:0 error:&error];
    }
#endif
}

}

#endif

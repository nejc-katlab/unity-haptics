#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

/// Fluent builder for constructing rich haptic patterns at runtime.
///
/// Each call appends an event and returns ``self`` so calls can be chained inline.
/// On ``play``, the accumulated events are submitted to the native bridge.
///
/// Times and durations are in seconds. Intensity and sharpness are clamped to 0..1.
/// Sharpness is iOS-only (ignored on Android, where this builder is mirrored as a Java class).
///
/// Usage in Obj-C:
/// ```objc
/// [[[[KatlabHapticPattern pattern]
///     tapAt:0.0  intensity:1.0 sharpness:1.0]
///     holdAt:0.02 duration:0.35 intensity:0.7 sharpness:0.2]
///     play];
/// ```
///
/// Usage in Swift:
/// ```swift
/// KatlabHapticPattern.pattern()
///     .tap(at: 0,    intensity: 1,   sharpness: 1)
///     .hold(at: 0.02, duration: 0.35, intensity: 0.7, sharpness: 0.2)
///     .play()
/// ```
@interface KatlabHapticPattern : NSObject

+ (instancetype)pattern;

/// Append a sharp transient event. Use for clicks, taps, sharp peaks.
- (KatlabHapticPattern*)tapAt:(float)time
                    intensity:(float)intensity
                    sharpness:(float)sharpness;

/// Append a sustained continuous event. Use for rumbles, decay tails, body of an explosion.
- (KatlabHapticPattern*)holdAt:(float)time
                      duration:(float)duration
                     intensity:(float)intensity
                     sharpness:(float)sharpness;

/// Submit the accumulated events to the haptic engine. No-op if the pattern is empty.
- (void)play;

@end

NS_ASSUME_NONNULL_END

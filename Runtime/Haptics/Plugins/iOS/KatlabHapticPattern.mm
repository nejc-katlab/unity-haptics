#import "KatlabHapticPattern.h"

// Forward-declare the C struct + entry point from HapticsBridge.mm so this file can be dropped
// into a project alongside HapticsBridge.mm with no extra setup.
typedef struct {
    float time;
    float duration;
    float intensity;
    float sharpness;
    int   type;
} KatlabHapticEvent;

#ifdef __cplusplus
extern "C" {
#endif
extern void _Haptics_PlayEvents(const KatlabHapticEvent* events, int count);
#ifdef __cplusplus
}
#endif

@implementation KatlabHapticPattern
{
    NSMutableData* _buffer;   // backing storage of KatlabHapticEvent structs
    NSUInteger _count;
}

+ (instancetype)pattern
{
    return [[self alloc] init];
}

- (instancetype)init
{
    if ((self = [super init])) {
        _buffer = [NSMutableData dataWithCapacity:sizeof(KatlabHapticEvent) * 8];
        _count = 0;
    }
    return self;
}

- (void)appendEvent:(KatlabHapticEvent)event
{
    [_buffer appendBytes:&event length:sizeof(KatlabHapticEvent)];
    _count++;
}

- (KatlabHapticPattern*)tapAt:(float)time
                    intensity:(float)intensity
                    sharpness:(float)sharpness
{
    KatlabHapticEvent ev = {
        .time = time,
        .duration = 0,
        .intensity = intensity,
        .sharpness = sharpness,
        .type = 0  // transient
    };
    [self appendEvent:ev];
    return self;
}

- (KatlabHapticPattern*)holdAt:(float)time
                      duration:(float)duration
                     intensity:(float)intensity
                     sharpness:(float)sharpness
{
    KatlabHapticEvent ev = {
        .time = time,
        .duration = duration,
        .intensity = intensity,
        .sharpness = sharpness,
        .type = 1  // continuous
    };
    [self appendEvent:ev];
    return self;
}

- (void)play
{
    if (_count == 0) return;
    const KatlabHapticEvent* events = (const KatlabHapticEvent*)[_buffer bytes];
    _Haptics_PlayEvents(events, (int)_count);
}

@end

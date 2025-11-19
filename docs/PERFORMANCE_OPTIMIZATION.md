# Performance Optimization Guide

This document provides guidelines for optimizing the Knockout game for stable 60fps performance.

## Target Performance

- **Frame Rate:** 60fps (16.67ms per frame)
- **Platform:** Desktop (PC/Mac/Linux)
- **Resolution:** 1920x1080

## Optimization Tools

### Performance Optimization Helper

Access via: `Knockout > Performance Optimization Helper`

This editor tool provides automated optimizations:
- Animator culling mode optimization
- Physics settings verification
- URP settings check
- Batch optimization runner

## Optimization Areas

### 1. Animation Optimization

**Animator Settings:**
- Culling Mode: `Cull Update Transforms` (best performance)
- Disable Animator components when characters are off-screen or dead
- Use Animation Compression to reduce memory usage

**Animation Clips:**
- Remove unnecessary keyframes
- Use animation compression (Keyframe Reduction)
- Optimize animation events (minimal use)

### 2. Physics Optimization

**Collision Detection:**
- Use `Discrete` for most objects (characters, static objects)
- Use `Continuous` only for fast-moving projectiles
- Use `Continuous Dynamic` sparingly (performance cost)

**Rigidbody Settings:**
- Minimize active Rigidbody components
- Use `Kinematic` for animated characters
- Set `Sleep Threshold` appropriately (default: 0.005)

**Collision Matrix:**
- Configure layer collision matrix in Physics Settings
- Disable unnecessary layer-to-layer collisions
- Example: UI layer should not collide with anything

### 3. Rendering Optimization

**URP Settings:**
```
Recommended Settings:
- Shadow Quality: Medium
- Shadow Distance: 50-100
- Cascade Count: 2
- Anti-aliasing: FXAA or disabled
- HDR: Disabled (unless needed)
- Render Scale: 1.0
```

**Texture Optimization:**
- Use texture compression (DXT5 for PC, ASTC for mobile)
- Max texture size: 2048x2048 for characters, 1024x1024 for UI
- Mipmaps: Enabled for all textures except UI
- Aniso Level: 2-4 (higher for important textures)

**Mesh Optimization:**
- Reduce polygon count where possible
- Use LOD (Level of Detail) for complex models
- Combine meshes to reduce draw calls

### 4. Code Optimization

**Component Caching:**
```csharp
// BAD - GetComponent in Update
private void Update()
{
    GetComponent<CharacterHealth>().CurrentHealth; // Expensive!
}

// GOOD - Cache in Awake
private CharacterHealth _health;

private void Awake()
{
    _health = GetComponent<CharacterHealth>();
}

private void Update()
{
    _health.CurrentHealth; // Fast!
}
```

**Object Pooling:**
- Pool frequently instantiated objects (VFX, damage numbers, audio sources)
- See `AudioManager.cs` for pooling example
- Avoid `Instantiate` and `Destroy` in Update loops

**Garbage Collection:**
- Minimize allocations in Update/FixedUpdate
- Avoid `foreach` loops with LINQ
- Use `StringBuilder` for string concatenation
- Cache delegates and lambdas

**Event Optimization:**
- Unsubscribe from events in OnDestroy
- Use weak event pattern for long-lived subscribers
- Avoid excessive event invocations per frame

### 5. UI Optimization

**Canvas Settings:**
- Use multiple canvases (static vs dynamic content)
- Set Canvas to `Screen Space - Overlay` for UI
- Disable `Raycast Target` on non-interactive UI elements

**Text Optimization:**
- Use TextMeshPro instead of UI.Text
- Minimize dynamic text updates
- Cache text references

### 6. Audio Optimization

**Audio Source Pooling:**
- AudioManager already implements pooling (default: 10 sources)
- Adjust pool size based on concurrent sound needs
- Use 2D audio for UI/music, 3D for positional sounds

**Audio Clip Settings:**
- Compression: Vorbis for music, ADPCM for SFX
- Load Type: Streaming for music, Decompress On Load for short SFX
- Sample Rate: 44.1kHz or lower

## Profiling

### Unity Profiler

Access via: `Window > Analysis > Profiler`

**Key Metrics to Monitor:**
- CPU Usage: Should be under 16.67ms per frame
- Rendering: Check draw calls and batching
- Scripts: Identify expensive Update loops
- Physics: Monitor FixedUpdate time
- GC Alloc: Minimize per-frame allocations

**Profiling Workflow:**
1. Build and run in Development Build mode
2. Open Profiler and connect to player
3. Record during intense combat
4. Identify spikes and bottlenecks
5. Optimize hot paths

### Frame Debugger

Access via: `Window > Analysis > Frame Debugger`

Use to:
- Analyze draw call order
- Identify overdraw
- Check batching effectiveness

## Performance Tests

Run automated performance tests via Unity Test Runner:

```
Assets/Knockout/Tests/PlayMode/Performance/PerformanceTests.cs
```

Tests include:
- Damage calculation efficiency
- Multi-character frame rate
- Memory leak detection
- Event subscription performance
- State transition efficiency

## Common Performance Issues

### Issue: Frame rate drops during combat

**Possible Causes:**
- Too many GetComponent calls
- Animator not optimized
- Excessive GC allocations
- Physics collision matrix not configured

**Solutions:**
- Cache component references
- Set Animator culling mode
- Use object pooling
- Configure layer collision matrix

### Issue: Memory usage grows over time

**Possible Causes:**
- Event subscriptions not cleaned up
- Object pooling not implemented
- Texture/mesh leaks

**Solutions:**
- Unsubscribe in OnDestroy
- Implement pooling for VFX
- Use Profiler Memory module to find leaks

### Issue: Physics jitter

**Possible Causes:**
- Animator and Rigidbody conflicting
- Wrong collision detection mode
- FixedUpdate timestep incorrect

**Solutions:**
- Set Animator Update Mode to "Animate Physics"
- Use appropriate collision detection
- Verify FixedUpdate timestep (default: 0.02)

## Target Metrics

| Metric | Target | Critical |
|--------|--------|----------|
| Frame Time | < 16.67ms | < 20ms |
| CPU Time | < 12ms | < 16ms |
| Rendering | < 4ms | < 6ms |
| Scripts | < 2ms | < 4ms |
| Physics | < 1ms | < 2ms |
| GC Alloc/frame | < 1KB | < 10KB |
| Draw Calls | < 50 | < 100 |

## Optimization Checklist

Before release, verify:
- [ ] Animator culling mode set to `Cull Update Transforms`
- [ ] All textures compressed appropriately
- [ ] No GetComponent calls in Update loops
- [ ] Object pooling implemented for VFX/audio
- [ ] Layer collision matrix configured
- [ ] URP settings optimized for target platform
- [ ] No memory leaks (verified with Profiler)
- [ ] Performance tests passing
- [ ] 60fps achieved during intense combat
- [ ] No GC spikes during gameplay

## Additional Resources

- [Unity Manual: Optimizing Graphics Performance](https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html)
- [Unity Manual: URP Optimization](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Unity Profiler Documentation](https://docs.unity3d.com/Manual/Profiler.html)

---

Last Updated: Phase 5 Implementation

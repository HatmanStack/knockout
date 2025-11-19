# Unity 6.0 Upgrade Impact Analysis for Physics-Based AI Implementation

## Executive Summary

The Knockout project is being upgraded from Unity 2021.3.45f2 to Unity 6.0 (see `upgrade-unity-6` branch). This upgrade significantly impacts the Physics-Based AI implementation plan, requiring updates to ML-Agents versions, Python requirements, API compatibility, and testing strategies.

**Critical Changes Required:**
- Unity ML-Agents: Release 20 (v0.30.0) → ML-Agents 2.0.x (Unity 6 default)
- Unity Package: com.unity.ml-agents 2.3.0 → 2.0.1/2.0.2
- **Inference Engine**: Barracuda → Sentis 2.1 (**MAJOR CHANGE**)
- Python: 3.9-3.10 → **Python 3.10.12** (locked, NO support for 3.11/3.12)
- Unity Editor: 2021.3.8f1 → Unity 6.0 (6000.0.x)
- PyTorch: 1.13.0+ → 2.1.1/2.2.1 (depending on ML-Agents version)
- **New Dependency**: Sentis package required for ML-Agents inference

## Detailed Impact Analysis

### 1. ML-Agents Framework Compatibility

#### Issue
**Original Plan**: Unity ML-Agents Release 20
- Python package: `mlagents==0.30.0`
- Unity package: `com.unity.ml-agents@2.3.0`
- Minimum Unity: 2021.3 LTS
- Inference: Barracuda

**Unity 6 Reality**: ML-Agents 2.0.x
- Python package: From Release 21 (`mlagents` from PyPI)
- Unity package: `com.unity.ml-agents@2.0.1` or `2.0.2` (default in Unity 6 Package Manager)
- Minimum Unity: 2022.3 (compatible with Unity 6)
- Inference: **Sentis 2.1** (Barracuda is deprecated)

#### Impact
- **VERSION DOWNGRADE**: Release 20 (2.3.0) → Release 21 (2.0.x) appears to be a downgrade but is Unity 6's default
- **CONFUSION**: ML-Agents 3.0.0 and 4.0.0 exist but have Unity 6 compatibility issues
- **BREAKING**: Barracuda → Sentis migration required for inference
- **DEPENDENCY**: Sentis package must be installed separately
- **API CHANGES**: Inference code uses Sentis API, not Barracuda API

#### Resolution
- Use ML-Agents 2.0.x (Unity 6 default) NOT 3.0 or 4.0
- Install Sentis 2.1 package as dependency
- Update all inference code references from Barracuda to Sentis
- Test model export produces ONNX compatible with Sentis
- Document Barracuda deprecation throughout

### 2. Python Version Requirements

#### Issue
**Original Plan**: Python 3.9-3.10 (broad range)

**Unity 6 Reality**: Python 3.10.12 (LOCKED)
- Minimum: Python 3.10.1
- Maximum: Python 3.10.12
- **Python 3.11/3.12**: NOT SUPPORTED (PyTorch dependency constraints)

#### Impact
- **LOCKED VERSION**: ML-Agents Python packages explicitly require Python <=3.10.12
- **NO UPGRADE PATH**: Python 3.11 and 3.12 are blocked by PyTorch version requirements
- **ENVIRONMENT**: Must use Python 3.10.12 regardless of system Python version
- **CONDA RECOMMENDED**: Virtual environment with conda/mamba strongly recommended

#### Resolution
- Document Python 3.10.12 as strict requirement
- Use conda to create Python 3.10.12 environment: `conda create -n mlagents python=3.10.12`
- Warn against using Python 3.11+ (will fail)
- Document that this is ML-Agents limitation, not Unity limitation

### 3. Unity Package Version Updates

#### Issue
**Original Plan (Unity 2021.3)**:
- com.unity.render-pipelines.universal: 12.1.15
- com.unity.inputsystem: 1.7.0
- com.unity.cinemachine: 2.10.1
- com.unity.test-framework: 1.1.33

**Unity 6 Upgraded Versions**:
- com.unity.render-pipelines.universal: 17.x or 18.x
- com.unity.inputsystem: 1.8.x+
- com.unity.cinemachine: 3.x (MAJOR version bump)
- com.unity.test-framework: Latest

#### Impact
- **INPUT SYSTEM**: May have API changes affecting character control integration
- **CINEMACHINE**: Major version bump (2.x → 3.x) may affect camera observation
- **URP**: Rendering pipeline changes (unlikely to affect AI directly)
- **TEST FRAMEWORK**: Test syntax may need updates

#### Resolution
- Review Input System 1.8.x API changes for character movement integration
- Check Cinemachine 3.x API if using camera-based observations
- Update test framework syntax in test examples
- Document package version compatibility matrix

### 4. Unity Physics System Changes

#### Issue
Unity 6 may include physics engine updates (PhysX improvements) that affect:
- Collision detection accuracy
- Rigidbody behavior
- Force application
- Joint constraints

#### Impact
- **TUNING**: Physics force magnitudes may need re-tuning
- **BEHAVIOR**: Subtle changes in balance/momentum behavior
- **HITBOXES**: More accurate collision may affect combat hit detection
- **TRAINING**: Reward functions may need adjustment if physics behavior differs

#### Resolution
- Establish physics baselines after Unity 6 upgrade (Phase 1 of main upgrade)
- Compare physics behavior between 2021.3 and Unity 6 in test scenarios
- Adjust force magnitudes in physics controllers if needed
- Re-train models after physics tuning is complete

### 5. Character System Integration

#### Issue
The Physics-Based AI integrates with existing character systems:
- CharacterMovement.cs
- CharacterCombat.cs
- CharacterHealth.cs
- CharacterController component

These systems may be affected by Unity 6 API changes during the upgrade.

#### Impact
- **API CHANGES**: Character systems may use updated Unity APIs
- **INTEGRATION POINTS**: AIArbiter must adapt to API changes
- **TESTING**: Integration tests need to pass on Unity 6
- **TIMING**: Can't implement AI until after Unity 6 upgrade completes

#### Resolution
- **SEQUENCING**: Begin Physics-Based AI implementation ONLY AFTER Unity 6 upgrade is complete and validated (all tests passing)
- Review upgraded character systems for API changes
- Update integration points in Phase 0 documentation
- Add Unity 6 API verification step in Phase 1

### 6. Barracuda to Sentis Migration (CRITICAL)

#### Issue
**Original Plan**: Assumed Barracuda for ML model inference

**Unity 6 Reality**: Barracuda is deprecated, Sentis 2.1 is the replacement

#### What is Sentis?
- Unity's new neural network inference engine
- Replaces Barracuda completely
- Runs ONNX models using compute shaders
- Provides local, on-device AI inference
- Eliminates cloud infrastructure and network latency

#### Impact
**Phase 1 (Training)**:
- ML-Agents exports to ONNX (unchanged)
- Training process unchanged
- Models must be ONNX opset 7-15 for Sentis compatibility

**Phase 5 (Inference/Deployment)**:
- **CANNOT use Barracuda API** - deprecated
- **MUST use Sentis API** for model loading and inference
- **NEW PATTERN**: Sentis Worker-based inference (not Barracuda Model.Load)
- **COMPUTE SHADERS**: Sentis uses GPU compute shaders for acceleration
- **DEPENDENCY**: Sentis package must be installed alongside ML-Agents

#### Sentis Inference Pattern (NEW)
```csharp
using Unity.Sentis;

// Load model
Model runtimeModel = ModelLoader.Load(onnxAsset);

// Create worker (inference engine)
IWorker worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);

// Create input tensor
TensorFloat inputTensor = new TensorFloat(shape, data);

// Execute inference
worker.Execute(inputTensor);

// Get output
TensorFloat outputTensor = worker.PeekOutput() as TensorFloat;

// Cleanup
worker.Dispose();
```

**vs Old Barracuda Pattern**:
```csharp
using Unity.Barracuda;

// OLD - Don't use
Model model = ModelLoader.Load(nnModel);
IWorker worker = WorkerFactory.Create(model, WorkerFactory.Type.ComputePrecompiled);
```

#### Resolution
- Add Sentis package installation to Phase 1
- Update Phase 5 inference code to use Sentis API
- Document Sentis Worker lifecycle (create, execute, dispose)
- Test ONNX model compatibility with Sentis early
- Profile Sentis inference performance vs expected Barracuda performance
- Update all code examples to show Sentis patterns

### 7. Test Framework Changes

#### Issue
Unity Test Framework upgraded from 1.1.33 → Latest (likely 1.3.x or 1.4.x)

#### Impact
- **TEST SYNTAX**: Possible API changes in test assertions
- **PLAYMODE TESTS**: ML-Agents integration tests may need updates
- **EDITMODE TESTS**: Unit tests for observation/action logic
- **PERFORMANCE TESTS**: Timing assertions may need adjustment

#### Resolution
- Use Unity 6-compatible test syntax from start
- Review Test Framework changelog for breaking changes
- Update test examples in Phase 0 documentation
- Run test validation after each phase implementation

### 8. WebGL Build Considerations

#### Issue
Unity 6 includes improved WebGL support:
- Better WASM optimization
- Improved threading (potential)
- Smaller build sizes
- Better performance

#### Impact
- **POSITIVE**: ML-Agents inference may be faster in WebGL
- **MODEL SIZE**: .onnx model included in WebGL build - size matters
- **PERFORMANCE**: Need to validate 60fps with RL agent inference
- **MEMORY**: WebGL memory constraints may affect model size limits

#### Resolution
- Keep neural network small (2 layers, 256 units as planned)
- Profile inference performance in Unity 6 WebGL builds
- Monitor WebGL build size with .onnx model included
- Optimize model if build size or performance issues arise

### 9. Training Performance

#### Issue
Unity 6 Editor may have different performance characteristics:
- Rendering performance (GPU Resident Drawer, etc.)
- Physics performance
- Memory usage
- Parallel environment handling

#### Impact
- **TRAINING SPEED**: May train faster or slower than expected
- **PARALLELISM**: Adjust number of parallel environments based on performance
- **MEMORY**: Monitor RAM usage with 8-16 parallel environments
- **THERMAL**: Watch CPU/GPU temperatures during long training runs

#### Resolution
- Start with 4-8 parallel environments, scale up if performance allows
- Monitor training performance metrics (FPS, step rate)
- Adjust training scene graphics quality for optimal speed
- Document optimal training settings for Unity 6

### 10. Cinemachine 3.x Integration

#### Issue
Cinemachine upgraded from 2.10.1 → 3.x (major version)
If Physics-Based AI uses camera observations or camera-based spatial awareness

#### Impact
- **CAMERA API**: Cinemachine API changes may affect camera access
- **OBSERVATIONS**: If using camera sensors for visual observations
- **SPATIAL**: Likely not affected (using position/velocity observations)

#### Resolution
- Review Phase 0 observation space design - uses physics state, not camera
- If adding camera observations later, use Cinemachine 3.x API
- Document Cinemachine 3.x compatibility in Phase 0
- No immediate changes needed (physics-based observations planned)

## Implementation Timeline Impact

### Original Timeline
Phases 0-5 implementing physics-based AI on Unity 2021.3

### Revised Timeline
**CRITICAL PREREQUISITE**: Unity 6 upgrade must complete first

```
Unity 6 Upgrade (upgrade-unity-6 branch, Phases 0-7)
├─ Phase 0-1: Preparation [PLANNED]
├─ Phase 2-4: Upgrade, packages, tests [PLANNED]
├─ Phase 5-7: Assets, modernization, WebGL [PLANNED]
└─ VALIDATION: All 52+ tests passing ✓

          ↓ THEN ↓

Physics-Based AI Implementation (upgrade-physics-ai branch, Phases 0-5)
├─ Phase 0: Architecture [READ FIRST]
├─ Phase 1: ML-Agents 2.0 + Movement
├─ Phase 2: Attack Execution
├─ Phase 3: Hit Reactions & Balance
├─ Phase 4: Defensive Positioning
└─ Phase 5: Arbiter Integration & Polish
```

**CANNOT BEGIN** Physics-Based AI implementation until:
- [ ] Unity 6 upgrade complete (all phases 0-7)
- [ ] All character systems tested and working
- [ ] All integration tests passing
- [ ] WebGL builds successfully
- [ ] Performance baselines established

**Estimated Delay**: 2-3 weeks for Unity 6 upgrade completion

## Updated Dependency Matrix

| Component | Unity 2021.3 Plan | Unity 6.0 Reality | Status |
|-----------|-------------------|-------------------|--------|
| Unity Editor | 2021.3.8f1 | Unity 6.0 (6000.0.x) | ✓ Update Required |
| ML-Agents (Python) | 0.30.0 (Release 20) | 2.0.x (Release 21) | ✓ **Version Confusion** |
| ML-Agents (Unity) | 2.3.0 | 2.0.1/2.0.2 | ✓ Use Unity 6 default |
| **Inference Engine** | Barracuda | **Sentis 2.1** | 🔥 **CRITICAL CHANGE** |
| Python | 3.9-3.10 | **3.10.12 ONLY** | ✓ Locked version |
| PyTorch | 1.13.0+ | 2.1.1/2.2.1 | ✓ Specify version |
| Sentis Package | N/A | 2.1.1 | 🆕 **NEW DEPENDENCY** |
| Input System | 1.7.0 | 1.8.x+ | ⚠️ API Review |
| Cinemachine | 2.10.1 | 3.x | ℹ️ Monitor (low impact) |
| URP | 12.1.15 | 17.x/18.x | ℹ️ Monitor (low impact) |
| Test Framework | 1.1.33 | Latest | ⚠️ Syntax Review |

## Recommended Actions

### Immediate (Before Starting AI Implementation)
1. ✅ Wait for Unity 6 upgrade completion
2. ✅ Update all AI plan documentation with Unity 6 requirements
3. ✅ Research ML-Agents 4.0.0 API changes
4. ✅ Update Python/package version specifications
5. ✅ Review Unity 6 physics behavior changes

### Phase 1 (During ML-Agents Setup)
1. Install ML-Agents 4.0.0 (not Release 20)
2. Use Python 3.10.12 specifically
3. Verify PyTorch 2.2.1 installation
4. Test basic training pipeline before full implementation
5. Establish physics baselines in Unity 6

### Ongoing (During Implementation)
1. Monitor Unity 6 API compatibility
2. Adjust physics parameters if Unity 6 behavior differs
3. Use Unity 6-compatible test syntax
4. Profile performance in Unity 6 Editor
5. Validate WebGL builds regularly

## Documentation Updates Checklist

- [x] Create UNITY_6_UPGRADE_IMPACT.md (this document)
- [ ] Update README.md prerequisites section
- [ ] Update Phase-0.md ADRs with Unity 6 considerations
- [ ] Update Phase-1.md Task 1 (ML-Agents installation)
- [ ] Update Phase-1.md Task 2-8 (Unity 6 API references)
- [ ] Update Phase-2.md through Phase-5.md (version references)
- [ ] Add Unity 6 validation step at start of Phase 1
- [ ] Update training configuration templates (YAML)
- [ ] Update commit message examples with migration context

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| ML-Agents 4.0 API incompatibility | Low | High | Test setup thoroughly in Phase 1 |
| Physics behavior changes | Medium | Medium | Re-tune parameters, compare baselines |
| Training performance degradation | Low | Medium | Adjust parallel env count, optimize scenes |
| Character system integration breaks | Low | High | Wait for Unity 6 validation, integration tests |
| WebGL build size increase | Medium | Low | Keep model small, monitor builds |
| Python 3.10.12 unavailable | Low | Low | Fall back to 3.10.x, document version |

## Conclusion

The Unity 6 upgrade is a **prerequisite dependency** for the Physics-Based AI implementation. All AI plan phases must be updated to reflect Unity 6 and ML-Agents 4.0 requirements. The core architecture and approach remain valid, but version specifications and API compatibility must be updated throughout the documentation.

**Next Steps:**
1. Update all phase documentation with Unity 6 specifications
2. Wait for Unity 6 upgrade completion
3. Begin Physics-Based AI implementation with updated requirements
4. Validate compatibility at each phase milestone

---

**Document Version**: 1.0
**Created**: 2025-11-19
**Last Updated**: 2025-11-19
**Author**: Claude Code (Unity 6 upgrade impact analysis)

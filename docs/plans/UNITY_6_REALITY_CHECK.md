# Unity 6 Reality Check: Actual Tooling vs Original Plan

**Date**: 2025-11-19
**Purpose**: Document the ACTUAL Unity 6 ML-Agents ecosystem vs what was originally assumed

## Summary

After researching the real Unity 6 development environment, several critical differences emerged from the original plan assumptions. Most notably: **Python is locked to 3.10.12** (not 3.12 as might be expected), **ML-Agents 2.0.x is Unity 6's default** (not 3.0 or 4.0), and **Sentis 2.1 replaces Barracuda** for inference.

## Key Findings

### 1. Python Version: LOCKED to 3.10.12

**Original Assumption**: Python would upgrade to 3.11 or 3.12 with Unity 6
**Reality**: Python 3.10.12 is the MAXIMUM supported version

- ML-Agents explicitly requires: `Python >=3.10.1, <=3.10.12`
- **Python 3.11 and 3.12 are NOT supported**
- Blocked by PyTorch version dependencies
- Will not change until PyTorch adds Python 3.11+ support
- Must use conda/mamba to lock Python to 3.10.12

**Impact**: NO Python upgrade path. Locked to 3.10.12 indefinitely.

### 2. ML-Agents Version: 2.0.x (Not 3.0 or 4.0)

**Original Assumption**: Unity 6 would use latest ML-Agents (4.0.0)
**Reality**: Unity 6 defaults to ML-Agents 2.0.1/2.0.2

- Unity 6 Package Manager offers ML-Agents 2.0.1 or 2.0.2
- ML-Agents 3.0.0 requires Unity 2023.2+ and has Unity 6 compatibility issues
- ML-Agents 4.0.0 exists but is not well-supported in Unity 6
- Unity forums show confusion about this versioning
- ML-Agents appears as "deprecated" in Package Manager (misleading - it's Barracuda that's deprecated)

**Impact**: Use ML-Agents 2.0.x, NOT 3.0 or 4.0, despite higher version numbers existing.

### 3. Inference Engine: Sentis 2.1 (Not Barracuda)

**Original Assumption**: ML-Agents would use Barracuda for model inference
**Reality**: Barracuda is DEPRECATED, Sentis 2.1 is the replacement

- **Sentis 2.1** ships with Unity 6 and replaces Barracuda completely
- Barracuda is officially deprecated and removed
- Sentis uses compute shaders (GPU accelerated)
- ONNX opset 7-15 support
- Completely different API from Barracuda
- Sentis package must be installed as separate dependency

**Impact**: All inference code must use Sentis API. Major architectural change.

### 4. PyTorch Version: 2.1.1 or 2.2.1

**Original Assumption**: PyTorch would auto-install latest
**Reality**: Specific PyTorch versions based on ML-Agents release

- ML-Agents 2.0.x → PyTorch 2.1.1 or 2.2.1
- Windows GPU users must install PyTorch separately with CUDA support
- `pip install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121`

**Impact**: Document specific PyTorch installation instructions.

## What Changed in the AI Plan

### Documentation Updated

1. **README.md**
   - Dependencies: ML-Agents 2.0.x (not 4.0), Sentis 2.1 added
   - Python: Locked to 3.10.12, explicitly warn about 3.11/3.12
   - Environment setup: conda recommended over uv for Python version control
   - Added Sentis package installation instructions

2. **UNITY_6_UPGRADE_IMPACT.md**
   - Section 1: Corrected ML-Agents versions (2.0.x is reality)
   - Section 2: Documented Python 3.10.12 lock (no 3.11/3.12)
   - NEW Section 6: Barracuda to Sentis migration (critical)
   - Dependency matrix: Updated with actual versions
   - Added Sentis API code examples

3. **Phase-0.md**
   - ADR-003: Updated to ML-Agents 2.0.x + Sentis 2.1
   - Consequences: Python 3.10.12 locked, Sentis required
   - Removed references to ML-Agents 4.0

4. **Phase-1.md Task 1**
   - Installation: ML-Agents 2.0.x (not 4.0)
   - Python: 3.10.12 via conda (recommended over uv)
   - **New Step**: Install Sentis 2.1 package
   - Verification: Check both ML-Agents AND Sentis
   - Warnings: Do NOT use ML-Agents 3.0/4.0

5. **Phase-2.md through Phase-5.md**
   - Added Unity 6 compatibility notes (already done)
   - Phase 5 will need Sentis inference patterns (future work)

## Critical Developer Notes

### For Python Environment Setup

```bash
# CORRECT: Use conda to lock Python version
conda create -n mlagents python=3.10.12
conda activate mlagents
pip install mlagents

# INCORRECT: Do not use Python 3.11 or 3.12
# python 3.11+  # This will FAIL
```

### For Unity Package Installation

```
Unity Package Manager:
✓ com.unity.ml-agents@2.0.1 or 2.0.2  # Unity 6 default, use this
✓ com.unity.sentis@2.1.1+              # REQUIRED for inference
✗ com.unity.ml-agents@3.0.0            # Has Unity 6 issues, avoid
✗ com.unity.ml-agents@4.0.0            # Not well-supported, avoid
```

### For Inference Code (Phase 5)

**DO NOT USE** Barracuda API:
```csharp
using Unity.Barracuda;  // DEPRECATED - Don't use
```

**USE** Sentis API instead:
```csharp
using Unity.Sentis;

Model runtimeModel = ModelLoader.Load(onnxAsset);
IWorker worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);
```

## Why These Versions?

### Why Python 3.10.12 Max?

PyTorch (ML-Agents dependency) doesn't support Python 3.11+ in the versions required by ML-Agents. This is an upstream dependency constraint, not a Unity limitation.

### Why ML-Agents 2.0.x and not 3.0/4.0?

- ML-Agents 3.0.0 was released targeting Unity 2023.2+
- Unity 6 (6000.0.x) predates full ML-Agents 3.0/4.0 integration
- Unity 6 Package Manager defaults to 2.0.x for stability
- Community reports compatibility issues with 3.0/4.0 on Unity 6

### Why Sentis?

Unity is moving all neural network inference to Sentis as part of their "Unity AI" initiative. Barracuda was the old system and is no longer maintained. Sentis provides:
- Better performance (compute shaders)
- Better ONNX compatibility
- Integration with Unity's roadmap
- Local, on-device inference (no cloud needed)

## Verification Checklist

Before starting AI implementation, verify:

- [ ] Python version is 3.10.x (check: `python --version`)
- [ ] ML-Agents installed via pip (check: `mlagents-learn --help`)
- [ ] Unity shows ML-Agents 2.0.1 or 2.0.2 in Package Manager
- [ ] Unity shows Sentis 2.1.1+ in Package Manager
- [ ] Both `Unity.MLAgents` and `Unity.Sentis` namespaces compile
- [ ] No console errors related to ML-Agents or Sentis

## References

### Official Documentation
- ML-Agents Release 21: https://github.com/Unity-Technologies/ml-agents/releases
- Unity Sentis 2.1: https://docs.unity3d.com/Packages/com.unity.sentis@2.1/
- ML-Agents Installation: https://unity-technologies.github.io/ml-agents/Installation/
- Sentis Migration: https://discussions.unity.com/t/upgrading-from-barracuda-to-sentis-with-mlagents/268633

### Community Discussions
- "Does Unity 6 support ML-Agents 3.0.0?": https://discussions.unity.com/t/does-unity-6-support-ml-agents-3-0-0/1680366
- "Is ML-Agents being deprecated?": https://discussions.unity.com/t/is-ml-agents-being-deprecated/1643681
- Sentis performance discussions: Unity forums

### Python/PyTorch Constraints
- ML-Agents requires: Python <=3.10.12, >=3.10.1
- PyTorch 2.1.1/2.2.1 does not support Python 3.11+
- This is unlikely to change soon (PyTorch upstream issue)

## Bottom Line

**Unity 6 ML-Agents Development in 2024-2025:**
- Python: **3.10.12** (locked, use conda)
- ML-Agents: **2.0.x** (Unity 6 default, not 3.0/4.0)
- Inference: **Sentis 2.1** (Barracuda deprecated)
- PyTorch: **2.1.1 or 2.2.1**
- Training: Unchanged (still works the same)
- Inference API: Completely different (Sentis API)

The plan has been updated to reflect this reality.

---

**Document Version**: 1.0
**Research Date**: 2025-11-19
**Sources**: Official Unity docs, GitHub releases, Unity forums, PyPI

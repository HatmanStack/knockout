# Physics-Based AI Opponent Implementation Plan

## Feature Overview

This implementation plan details the development of a physics-based AI opponent system for the Knockout fighting game, inspired by the [Facebook Research paper on control strategies for physically simulated characters](https://research.facebook.com/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/). The system uses reinforcement learning (RL) via Unity ML-Agents to train AI opponents that exhibit realistic physics-based movement, attacks, balance recovery, and defensive positioning.

Unlike the current behavioral state machine AI, this physics-based approach creates emergent, adaptive behavior through self-play training. The implementation follows a hybrid architecture where the existing behavioral AI provides high-level strategic decisions while the RL agent controls low-level physics execution, with an arbiter system blending the two approaches. This creates AI opponents that move naturally, transfer weight realistically during attacks, recover from hits with physical authenticity, and maintain proper fighting stances.

The system is designed for incremental rollout across six phases, allowing validation at each stage before adding complexity. Training occurs locally using Unity ML-Agents with parallel environments for efficiency. The RL agent uses a continuous action space (target poses + parameters) and comprehensive observations (physics state, opponent state, spatial relationships, game context) to learn through self-play with multi-objective rewards balancing combat effectiveness, physical realism, strategic depth, and player entertainment.

## Prerequisites

### CRITICAL: Unity 6.0 Upgrade Dependency

**⚠️ IMPORTANT**: This implementation plan requires the completion of the Unity 6.0 upgrade. DO NOT begin implementation until:
- Unity 6.0 upgrade is complete (see `upgrade-unity-6` branch, all phases 0-7)
- All character system tests are passing
- WebGL builds are validated
- Performance baselines are established

See `UNITY_6_UPGRADE_IMPACT.md` for detailed impact analysis.

### Dependencies

- **Unity 6.0 (6000.0.x)** (upgraded from 2021.3)
- **Unity ML-Agents 2.0.x** (Unity 6 default version)
  - Install: `pip install mlagents` (from PyPI, Release 21)
  - Unity Package: `com.unity.ml-agents` version 2.0.1 or 2.0.2
  - **Note**: ML-Agents 3.0/4.0 exist but have Unity 6 compatibility issues
- **Unity Sentis 2.1** (**NEW** - replaces Barracuda)
  - Unity Package: `com.unity.sentis` version 2.1.1+
  - Required for ML model inference (Barracuda is deprecated)
  - Uses compute shaders for neural network execution
- **Python 3.10.12** (REQUIRED - locked version)
  - ML-Agents requires Python >=3.10.1, <=3.10.12
  - **Python 3.11/3.12 NOT supported** (PyTorch dependency constraints)
  - Use conda: `conda create -n mlagents python=3.10.12`
- **PyTorch 2.1.1/2.2.1** (ML-Agents training backend)
  - Auto-installed with mlagents package
  - CPU version sufficient for local training
  - GPU version optional: `pip install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121`

### Environment Setup

1. **Verify Unity 6.0 Upgrade Complete**:
   - Confirm Unity 6.0 is installed and project opens without errors
   - Verify all 52+ tests are passing
   - Check that character systems work correctly

2. **Python Environment**: Use conda/mamba (recommended) or `uv`
   ```bash
   # Recommended: Use conda for Python 3.10.12
   conda create -n mlagents python=3.10.12
   conda activate mlagents
   pip install mlagents

   # Alternative: Use uv
   uv venv ml-agents-env --python 3.10.12
   source ml-agents-env/bin/activate  # Linux/Mac
   uv pip install mlagents
   ```

3. **Unity Package Manager**: Add ML-Agents 2.0.x and Sentis packages
   - Window > Package Manager
   - Add `com.unity.ml-agents` (version 2.0.1/2.0.2 will show)
   - Add `com.unity.sentis` (version 2.1.1+)
   - **Important**: Sentis is required for ML-Agents to work

4. **Verify Installation**:
   ```bash
   mlagents-learn --help
   python --version  # MUST show 3.10.x
   python -c "import torch; print(torch.__version__)"  # Check PyTorch
   ```

### Knowledge Requirements

- Basic familiarity with Unity ML-Agents (have completed examples)
- Understanding of Unity physics system (Rigidbody, forces, joints)
- C# proficiency with Unity component patterns
- Basic RL concepts (observations, actions, rewards, training)

## Phase Summary

| Phase | Goal | Estimated Tokens | Status |
|-------|------|-----------------|--------|
| [Phase 0](Phase-0.md) | Architecture & Foundation - Design decisions, ML-Agents setup, shared patterns | ~15,000 | Not Started |
| [Phase 1](Phase-1.md) | Training Infrastructure & Basic Movement - ML-Agents integration, training pipeline, physics-enhanced footwork | ~95,000 | Not Started |
| [Phase 2](Phase-2.md) | Attack Execution - Physics-based punching with weight transfer and force application | ~85,000 | Not Started |
| [Phase 3](Phase-3.md) | Hit Reactions & Balance - Stumbling, recovery, knockdowns driven by physics | ~90,000 | Not Started |
| [Phase 4](Phase-4.md) | Defensive Positioning - Blocking stance, dodging, center of mass control | ~85,000 | Not Started |
| [Phase 5](Phase-5.md) | Arbiter Integration & Polish - Blend behavioral AI with RL, full system integration, optimization | ~95,000 | Not Started |

**Total Estimated Tokens**: ~465,000 across all phases

## Navigation

### Foundation
- [Phase 0: Architecture & Design Decisions](Phase-0.md)

### Implementation Phases
- [Phase 1: Training Infrastructure & Movement](Phase-1.md)
- [Phase 2: Attack Execution](Phase-2.md)
- [Phase 3: Hit Reactions & Balance](Phase-3.md)
- [Phase 4: Defensive Positioning](Phase-4.md)
- [Phase 5: Arbiter Integration & Polish](Phase-5.md)

## Development Workflow

### Before Starting Each Phase

1. Read Phase 0 (Architecture & Foundation) completely
2. Review prerequisites for the specific phase
3. Ensure previous phases are complete and verified
4. Check that all dependencies are installed
5. Create a feature branch for the phase

### During Phase Implementation

1. Follow tasks sequentially unless otherwise specified
2. Write tests BEFORE implementation (TDD)
3. Commit after each task completion (atomic commits)
4. Use conventional commit format (see Phase 0)
5. Verify task completion checklist before moving on

### After Each Phase

1. Complete phase verification checklist
2. Run full test suite
3. Commit any remaining changes
4. Optionally create a pull request for review
5. Merge to main branch before starting next phase

## Training Workflow

Each phase involving RL training follows this pattern:

1. **Design** observation/action space for the behavior
2. **Implement** Agent component with observations/actions
3. **Configure** training hyperparameters in YAML
4. **Create** parallel training environment
5. **Train** agent locally with self-play
6. **Evaluate** performance and adjust rewards
7. **Iterate** until behavior meets success criteria
8. **Export** trained model (.onnx file)
9. **Integrate** into gameplay

## Common Pitfalls

- **Training too long initially**: Start with short training runs (5-10 minutes) to verify setup
- **Observation space too large**: Keep observations minimal and normalized
- **Reward function conflicts**: Balance competing objectives carefully
- **Forgetting to normalize**: Always normalize observations to [-1, 1] or [0, 1]
- **Not using parallel environments**: Use at least 4-8 parallel envs for local training
- **Skipping curriculum**: Complex behaviors may need staged training

## Support Resources

- [Unity ML-Agents Documentation](https://unity-technologies.github.io/ml-agents/)
- [Facebook Research Paper](https://research.facebook.com/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/)
- [Unity Physics Documentation](https://docs.unity3d.com/Manual/PhysicsSection.html)
- Project-specific: `docs/PERFORMANCE_OPTIMIZATION.md`, `docs/EXTENSION_GUIDE.md`

## Current Project State

### Unity 6.0 Upgraded Systems

The project has been upgraded to Unity 6.0 with the following modernized packages:
- **URP**: 17.x/18.x (upgraded from 12.1.15)
- **Input System**: 1.8.x+ (upgraded from 1.7.0)
- **Cinemachine**: 3.x (upgraded from 2.10.1)
- **Test Framework**: Latest (upgraded from 1.1.33)

### Existing Systems to Leverage

- **Character Components**: CharacterController, CharacterCombat, CharacterMovement, CharacterHealth (Unity 6 compatible)
- **Behavioral AI**: State machine with Observe/Approach/Attack/Defend/Retreat states
- **Combat System**: Attack data, hit detection, blocking mechanics
- **Round System**: RoundManager for game flow
- **Test Infrastructure**: 52+ test files with Unity Test Framework (Unity 6 validated)

### Integration Points

The physics-based AI will integrate with Unity 6-upgraded systems:
- `CharacterAI.cs` - Main AI coordinator (will work alongside new physics agent)
- `CharacterCombat.cs` - Attack execution system (uses Unity 6 APIs)
- `CharacterMovement.cs` - Movement system (Input System 1.8.x)
- `CharacterHealth.cs` - Damage and health tracking
- `AIStateMachine.cs` - Behavioral decision-making

See Phase 0 for detailed architecture decisions on integration strategy.

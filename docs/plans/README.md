# Physics-Based AI Opponent Implementation Plan

## Feature Overview

This implementation plan details the development of a physics-based AI opponent system for the Knockout fighting game, inspired by the [Facebook Research paper on control strategies for physically simulated characters](https://research.facebook.com/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/). The system uses reinforcement learning (RL) via Unity ML-Agents to train AI opponents that exhibit realistic physics-based movement, attacks, balance recovery, and defensive positioning.

Unlike the current behavioral state machine AI, this physics-based approach creates emergent, adaptive behavior through self-play training. The implementation follows a hybrid architecture where the existing behavioral AI provides high-level strategic decisions while the RL agent controls low-level physics execution, with an arbiter system blending the two approaches. This creates AI opponents that move naturally, transfer weight realistically during attacks, recover from hits with physical authenticity, and maintain proper fighting stances.

The system is designed for incremental rollout across six phases, allowing validation at each stage before adding complexity. Training occurs locally using Unity ML-Agents with parallel environments for efficiency. The RL agent uses a continuous action space (target poses + parameters) and comprehensive observations (physics state, opponent state, spatial relationships, game context) to learn through self-play with multi-objective rewards balancing combat effectiveness, physical realism, strategic depth, and player entertainment.

## Prerequisites

### Dependencies

- **Unity 2021.3.8f1 LTS** (current project version)
- **Unity ML-Agents Release 20** (latest stable release)
  - Install: `python -m pip install mlagents==0.30.0`
  - Unity Package: `com.unity.ml-agents` version 2.3.0
- **Python 3.9-3.10** (required for ML-Agents)
  - Not compatible with Python 3.11+ yet
- **PyTorch 1.13.0+** (ML-Agents training backend)
  - CPU version sufficient for local training
  - GPU version optional but recommended for faster training

### Environment Setup

1. **Python Environment**: Use `uv` (already installed) or virtualenv
   ```bash
   uv venv ml-agents-env
   source ml-agents-env/bin/activate  # Linux/Mac
   uv pip install mlagents==0.30.0
   ```

2. **Unity Package Manager**: Add ML-Agents package
   - Window > Package Manager > Add package from git URL
   - `https://github.com/Unity-Technologies/ml-agents.git?path=com.unity.ml-agents#release_20`

3. **Verify Installation**:
   ```bash
   mlagents-learn --help
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

### Existing Systems to Leverage

- **Character Components**: CharacterController, CharacterCombat, CharacterMovement, CharacterHealth
- **Behavioral AI**: State machine with Observe/Approach/Attack/Defend/Retreat states
- **Combat System**: Attack data, hit detection, blocking mechanics
- **Round System**: RoundManager for game flow
- **Test Infrastructure**: 80%+ test coverage with Unity Test Framework

### Integration Points

The physics-based AI will integrate with:
- `CharacterAI.cs` - Main AI coordinator (will work alongside new physics agent)
- `CharacterCombat.cs` - Attack execution system
- `CharacterMovement.cs` - Movement system
- `CharacterHealth.cs` - Damage and health tracking
- `AIStateMachine.cs` - Behavioral decision-making

See Phase 0 for detailed architecture decisions on integration strategy.

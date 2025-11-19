# Phase 0: Architecture & Foundation

## Phase Goal

Establish architectural decisions, design patterns, and technical foundations for the physics-based AI system. This phase documents **decisions, not implementations** - it serves as a reference for all subsequent phases. By completing this phase, implementers will understand the hybrid AI architecture, ML-Agents integration strategy, observation/action space design, reward structure, and testing patterns.

**Success Criteria:**
- All architecture decision records (ADRs) documented
- Integration strategy with existing behavioral AI defined
- Observation and action spaces designed
- Reward function structure defined
- Testing strategy established
- Shared code patterns documented

**Estimated tokens:** ~15,000

## Prerequisites

- **Unity 6.0 upgrade MUST be complete** (see `upgrade-unity-6` branch and `UNITY_6_UPGRADE_IMPACT.md`)
- All character system tests passing in Unity 6
- Familiarity with existing Knockout codebase (see README.md)
- Understanding of current behavioral AI state machine
- Basic knowledge of Unity ML-Agents concepts
- Review of Facebook Research paper (recommended but not required)

---

## Architecture Decision Records (ADRs)

### ADR-001: Hybrid AI Architecture (Parallel Systems with Arbiter)

**Context**: Need to integrate physics-based RL agent with existing behavioral state machine AI.

**Decision**: Implement parallel systems where both behavioral AI and RL agent generate inputs, with an arbiter component that blends or selects between them based on context.

**Rationale**:
- Allows gradual rollout - behavioral AI continues working while RL is trained
- Provides graceful fallback if RL agent behaves unexpectedly
- Enables context-specific routing (e.g., use behavioral AI for long-range spacing, RL for close combat)
- Supports A/B testing and performance comparison
- Lower risk than complete replacement

**Architecture**:
```
CharacterAI (main coordinator)
    ├─ AIStateMachine (existing behavioral AI)
    │   └─ States: Observe, Approach, Attack, Defend, Retreat
    ├─ PhysicsAgent (new RL agent, inherits Unity.MLAgents.Agent)
    │   └─ Observations: physics state, opponent, spatial, game context
    │   └─ Actions: target poses + parameters
    └─ AIArbiter (new blending/selection component)
        └─ Decides which system controls character each frame
```

**Consequences**:
- Additional complexity of arbiter logic
- Need to design arbiter decision rules
- Both systems must output compatible action format
- Requires abstraction layer for actions

**Alternatives Considered**:
- State machine decides, RL executes: Simpler but limits RL autonomy
- RL handles everything: Higher risk, loses behavioral AI advantages
- Layered decision making: More complex, harder to debug

---

### ADR-002: Physics-Enhanced Animation Approach

**Context**: Need to choose level of physics simulation for character control.

**Decision**: Use physics-enhanced animations - keep existing animation system but augment with physics forces for realistic momentum, weight transfer, and reactions.

**Rationale**:
- Maintains visual polish of hand-authored animations
- Easier to train than full ragdoll physics
- Better performance than real-time physics simulation
- Preserves existing animation assets and systems
- Allows gradual transition from pure animation to physics-driven

**Implementation Approach**:
- Animations play normally via Animator
- Physics forces applied to Rigidbody during key moments (punches, movement, hits)
- Blend between animation-driven and physics-driven based on context
- Use AddForce/AddTorque for physics enhancement
- ConfigurableJoint or similar for maintaining poses while allowing physics deviation

**Consequences**:
- Need careful tuning of physics force magnitudes
- Must synchronize animation state with physics application
- Potential visual artifacts if forces conflict with animations
- Requires Rigidbody configuration on character

**Alternatives Considered**:
- Full ragdoll: Too computationally expensive, hard to train, loses animation polish
- Pure procedural animation: Would require rebuilding entire animation system
- High-level actions only: Wouldn't achieve physical realism goal

---

### ADR-003: Unity ML-Agents as RL Framework

**Context**: Need to choose reinforcement learning framework for training physics-based AI.

**Decision**: Use Unity ML-Agents 2.0.x (Unity 6 default) with PPO (Proximal Policy Optimization) algorithm.

**Rationale**:
- Native Unity integration - no need for external simulators
- Well-documented, actively maintained by Unity Technologies
- PPO is stable and sample-efficient for continuous control
- Supports parallel environments for faster local training
- Good community support and examples
- Handles observation/action normalization automatically
- Built-in TensorBoard integration for monitoring
- Unity 6.0 compatible (ML-Agents 2.0.x is Unity 6 default)
- Sentis 2.1 integration for inference (replaces deprecated Barracuda)

**Configuration**:
- Algorithm: PPO (default for continuous control)
- Parallel environments: 8-16 (adjust based on local hardware)
- Training batch size: 2048-4096
- Buffer size: 20480
- Learning rate: 3e-4 (standard PPO learning rate)
- Discount factor (gamma): 0.99 (long-term reward consideration)

**Consequences**:
- Python 3.10.12 required (LOCKED - no support for 3.11/3.12)
- PyTorch 2.1.1/2.2.1 dependency
- Training tied to Unity Editor (can't train headless easily on Linux)
- Model exported as .onnx format (Sentis compatible)
- Sentis 2.1 package required as dependency for inference
- Barracuda is deprecated - must use Sentis API for inference

**Alternatives Considered**:
- Custom RL with PyTorch: More control but significantly more work
- DeepMind DM Control: Requires porting game to MuJoCo, overkill
- OpenAI Gym: Doesn't integrate well with Unity

---

### ADR-004: Self-Play Training Strategy

**Context**: Need to determine what opponents the RL agent trains against.

**Decision**: Use self-play training where two copies of the RL agent fight each other, continuously improving through competition.

**Rationale**:
- Proven effective for competitive two-player scenarios (AlphaGo, OpenAI Five)
- Creates emergent strategies and counter-strategies
- No need for hand-crafted opponent behaviors
- Agent improves indefinitely as it plays against stronger versions of itself
- Naturally creates diverse experiences

**Implementation**:
- Training scene with two identical PhysicsAgent instances
- Each agent controls one character
- Rewards structured as zero-sum (one wins, other loses)
- Periodically save checkpoints to track improvement
- Use symmetric rewards to avoid exploitation

**Consequences**:
- Initial training may be slow (random agents learning from scratch)
- Risk of converging to local optima or degenerate strategies
- Need monitoring to detect training collapse
- Should validate against behavioral AI periodically

**Alternatives Considered**:
- Train vs behavioral AI: Limited by behavioral AI skill ceiling
- Curriculum learning: More complex setup, may not be necessary
- Hybrid training: Could be added later if self-play issues arise

---

### ADR-005: Unity 6.0 Compatibility and Integration

**Context**: Project is being upgraded from Unity 2021.3 to Unity 6.0, affecting all ML-Agents implementation.

**Decision**: Implement physics-based AI ONLY AFTER Unity 6 upgrade is complete and validated. Use Unity 6-compatible APIs, packages, and workflows throughout.

**Rationale**:
- Character systems will use Unity 6 APIs (Input System 1.8.x, Physics updates)
- ML-Agents 4.0.0 requires Unity 6.0 minimum
- Physics behavior may differ between 2021.3 and Unity 6 (affects training)
- Integration tests must pass on Unity 6 before AI implementation
- Starting on correct Unity version avoids double migration work

**Unity 6 Specific Considerations**:
- **Physics**: Unity 6 may have PhysX improvements affecting force tuning
- **Input System**: Input System 1.8.x+ API changes for character control
- **Cinemachine**: Cinemachine 3.x if using camera observations (low impact, using physics)
- **Test Framework**: Use latest Unity Test Framework syntax
- **Performance**: Unity 6 Editor performance affects training speed
- **WebGL**: Unity 6 WebGL improvements may benefit inference performance

**Integration Verification Requirements**:
- CharacterMovement works with Unity 6 Input System
- CharacterCombat hit detection validated in Unity 6 physics
- CharacterHealth damage calculations tested
- All 52+ existing tests passing
- WebGL builds successfully

**Consequences**:
- Implementation blocked until Unity 6 upgrade complete (estimated 2-3 weeks)
- Must use Unity 6 APIs from start (no backward compatibility needed)
- Physics parameters may need different tuning than 2021.3
- Training baseline comparisons must use Unity 6 metrics
- All documentation references Unity 6, not 2021.3

**Migration Notes**:
- ML-Agents Release 20 (0.30.0) → ML-Agents 4.0.0
- Python 3.9-3.10 → Python 3.10.12 recommended
- Unity 2021.3.8f1 → Unity 6.0 (6000.0.x)
- Package consolidation: extensions merged into main package

**Alternatives Considered**:
- Implement on 2021.3, migrate later: Double work, incompatible physics baselines
- Parallel development: Risk of integration conflicts, wasted effort
- Wait for Unity 6 validation: Chosen approach, safest and most efficient

---

### ADR-006: Observation Space Design

**Context**: Need to define what information the RL agent observes each step.

**Decision**: Comprehensive observation space including self physics state, opponent state, spatial relationships, and game context. All observations normalized to [-1, 1].

**Observation Vector Structure** (approx. 50-80 dimensions):

**Self Physics State** (~20 dimensions):
- Self position (x, y, z) - normalized to arena bounds
- Self velocity (x, y, z) - normalized to max speed
- Self facing direction (forward vector x, z) - unit vector
- Self angular velocity - normalized
- Ground contact (boolean as 0/1)
- Center of mass position relative to feet (x, z) - normalized
- Current animation state (one-hot encoded: idle, moving, attacking, blocking, hit-stunned, knocked-down)
- Self momentum magnitude - normalized

**Opponent State** (~15 dimensions):
- Opponent position (x, y, z) - normalized to arena bounds
- Opponent velocity (x, y, z) - normalized
- Opponent facing direction (forward x, z)
- Opponent animation state (one-hot encoded)
- Opponent current attack type (one-hot: none, jab, hook, uppercut)

**Spatial Relationships** (~10 dimensions):
- Distance to opponent - normalized to max arena distance
- Relative position to opponent (x, y, z) - normalized
- Angle to opponent (cos, sin of angle)
- Is opponent in attack range (boolean)
- Is in opponent's attack range (boolean)
- Distance to arena boundaries (closest boundary) - normalized

**Game Context** (~10 dimensions):
- Self health percentage (0-1)
- Opponent health percentage (0-1)
- Round number (normalized)
- Round time remaining (normalized to max round time)
- Rounds won by self (normalized)
- Rounds won by opponent (normalized)
- Time since last hit received (normalized, capped)
- Time since last hit dealt (normalized, capped)

**Rationale**:
- Comprehensive enough for strategic decision-making
- Not too large (keeps training stable)
- All continuous values for PPO compatibility
- Normalized for training stability
- Includes immediate state and context

**Consequences**:
- ~50-80 dimensional observation vector
- Need careful normalization implementation
- Must update observations every decision step
- Should add observation debugging visualization

---

### ADR-007: Action Space Design (Target Poses + Parameters)

**Context**: Need to define what actions the RL agent can take each step.

**Decision**: Continuous action space with target poses selection and physics parameters. Actions output every 0.1-0.2 seconds (5-10 Hz decision frequency).

**Action Vector Structure** (approx. 10-15 dimensions):

**Movement Actions** (~4 dimensions):
- Movement direction X (continuous -1 to 1: left/right)
- Movement direction Z (continuous -1 to 1: back/forward)
- Movement speed multiplier (continuous 0 to 1)
- Turn rate (continuous -1 to 1: rotate left/right)

**Attack Actions** (~5 dimensions):
- Attack type (continuous 0-1, discretized to: none, jab, hook, uppercut)
- Attack hand (continuous 0-1, discretized to: left, right)
- Attack force multiplier (continuous 0.5 to 1.5)
- Attack timing offset (continuous -0.1 to 0.1 seconds)
- Follow-through intensity (continuous 0 to 1)

**Defensive Actions** (~3 dimensions):
- Block activation (continuous 0-1, threshold at 0.5)
- Block height (continuous 0-1: body/head)
- Evasion direction (continuous -1 to 1: duck/lean/neutral)

**Physics Parameters** (~3 dimensions):
- Weight shift (continuous -1 to 1: back foot to front foot)
- Stance width (continuous 0.5 to 1.2: narrow to wide)
- Center of mass height (continuous 0.7 to 1.0: crouched to upright)

**Rationale**:
- Continuous actions work well with PPO
- Parameters give RL control over execution style
- Target poses maintain animation polish
- Physics parameters enable realistic weight transfer
- Enough freedom for emergent behavior

**Consequences**:
- Need action interpretation layer
- Must clamp/normalize action outputs
- Require physics controllers to apply parameters
- Some actions may conflict (need resolution rules)

---

### ADR-008: Multi-Objective Reward Function

**Context**: Need to define reward signals that encourage desired AI behavior.

**Decision**: Weighted multi-objective reward combining combat effectiveness, physical realism, strategic depth, and player entertainment.

**Reward Components**:

**Combat Effectiveness** (weight: 0.4):
- +1.0 per hit landed on opponent
- +0.5 per hit blocked successfully
- -0.5 per hit received
- +10.0 for winning round
- -10.0 for losing round
- +0.1 per frame in optimal attack range (1.5-2.5 units)

**Physical Realism** (weight: 0.2):
- +0.01 per frame maintaining balance (center of mass over support base)
- -0.05 per excessive jerky movement (high acceleration changes)
- +0.05 for smooth weight transfer during attacks (gradual shift)
- -0.1 for unnatural poses (IK constraints violated)
- +0.02 per frame with feet on ground (no floating)

**Strategic Depth** (weight: 0.2):
- +0.1 for attack variety (using different punch types)
- +0.05 for spacing control (adjusting distance)
- +0.1 for defensive reactions (blocking when opponent attacks)
- -0.05 for spam behavior (same action repeatedly)
- +0.05 for footwork patterns (circling, strafing)

**Player Entertainment** (weight: 0.2):
- +0.5 for close-range exchanges (both fighters attacking)
- +0.1 per dramatic moment (narrow health difference)
- -0.2 for overly passive play (no actions for extended time)
- +0.3 for comebacks (dealing damage when low health)
- +0.1 for round going to time (competitive match)

**Sparse Rewards** (milestone-based):
- +50.0 for winning match (2 rounds)
- +5.0 for landing first hit in round
- +20.0 for knockout victory

**Rationale**:
- Multiple objectives prevent exploitation of single metric
- Weights balance competing goals
- Dense rewards (per-frame) guide learning
- Sparse rewards provide clear objectives
- Entertainment rewards ensure fun gameplay

**Consequences**:
- Complex reward calculation each step
- Need careful weight tuning through experimentation
- Risk of reward hacking (need monitoring)
- Should log individual reward components for debugging

**Implementation Notes**:
- Calculate reward components separately
- Log to TensorBoard for analysis
- Allow runtime weight adjustment for experimentation
- Consider reward normalization if magnitudes vary greatly

---

### ADR-009: Incremental Phase Rollout

**Context**: System is complex with multiple behaviors to implement and train.

**Decision**: Implement and train behaviors incrementally across phases, with each phase building on previous trained models.

**Phase Sequence**:
1. **Movement & Footwork**: Train agent to move naturally with physics
2. **Attack Execution**: Add attacking with physics-based weight transfer
3. **Hit Reactions & Balance**: Add balance recovery and hit reactions
4. **Defensive Positioning**: Add blocking and defensive movement
5. **Integration**: Combine all behaviors with arbiter system

**Transfer Learning Approach**:
- Each phase starts with previous phase's trained model
- Freeze learned behaviors while training new ones (optional)
- Expand observation/action space incrementally
- Adjust reward weights as capabilities expand

**Rationale**:
- Easier to debug individual behaviors in isolation
- Can validate each capability before adding complexity
- Reduces training time (don't retrain everything each phase)
- Lower risk of training collapse
- Provides incremental value

**Consequences**:
- Need to manage multiple model checkpoints
- Must carefully design observation/action space to be expandable
- Each phase requires training time
- Risk of catastrophic forgetting (new training degrades old behaviors)
- Should test backward compatibility between phases

---

## Design Patterns and Conventions

### Component Organization

**PhysicsAgent Component** (inherits Unity.MLAgents.Agent):
```
Assets/Knockout/Scripts/AI/PhysicsAgent/
    ├─ PhysicsAgent.cs                    // Main ML-Agents Agent component
    ├─ PhysicsAgentObservations.cs        // Observation collection logic
    ├─ PhysicsAgentActions.cs             // Action interpretation logic
    ├─ PhysicsAgentRewards.cs             // Reward calculation
    └─ PhysicsControllers/
        ├─ MovementController.cs          // Physics-based movement
        ├─ AttackController.cs            // Physics-based attacks
        ├─ BalanceController.cs           // Balance and recovery
        └─ DefenseController.cs           // Blocking and dodging
```

**Arbiter Component**:
```
Assets/Knockout/Scripts/AI/
    └─ AIArbiter.cs                       // Blends behavioral AI + RL agent
```

**Training Infrastructure**:
```
Assets/Knockout/Scenes/
    └─ Training/
        ├─ TrainingArena.unity            // Training scene with parallel envs
        └─ SelfPlayArena.unity            // Self-play specific setup

Assets/Knockout/Training/
    └─ Config/
        ├─ movement_training.yaml         // ML-Agents config for Phase 1
        ├─ attack_training.yaml           // Phase 2
        ├─ balance_training.yaml          // Phase 3
        └─ defense_training.yaml          // Phase 4
```

### Naming Conventions

- **Classes**: PascalCase (e.g., `PhysicsAgent`, `AIArbiter`)
- **Private fields**: camelCase with underscore prefix (e.g., `_currentReward`)
- **Public properties**: PascalCase (e.g., `CurrentState`)
- **Methods**: PascalCase (e.g., `CollectObservations`, `OnActionReceived`)
- **Configuration files**: snake_case (e.g., `movement_training.yaml`)
- **Scene names**: PascalCase (e.g., `TrainingArena`)

### ML-Agents Override Methods

All PhysicsAgent implementations must override:
- `Initialize()`: Setup component references, one-time initialization
- `CollectObservations(VectorSensor sensor)`: Gather observation vector
- `OnActionReceived(ActionBuffers actions)`: Interpret and execute actions
- `OnEpisodeBegin()`: Reset environment for new training episode
- `Heuristic(in ActionBuffers actionsOut)`: Manual control for testing

Optional overrides:
- `OnEpisodeEnd()`: Cleanup, logging
- `OnCollisionEnter(Collision)`: Physics event handling

### Testing Patterns

**Unit Tests** (EditMode):
- Test observation normalization logic
- Test action interpretation
- Test reward calculation components
- Test physics controller math (force calculations, etc.)

**Integration Tests** (PlayMode):
- Test full agent training loop (short run)
- Test agent inference with trained model
- Test arbiter blending logic
- Test interaction with existing character components

**Training Validation**:
- TensorBoard monitoring during training
- Checkpoint evaluation scripts
- Performance benchmarks against behavioral AI
- Visual debugging tools for observations/actions

---

## Shared Utilities

### Observation Normalization Helper

Implement reusable normalization utilities:
- `NormalizePosition(Vector3 pos, Bounds areaBounds)`: Normalize to [-1, 1]
- `NormalizeVelocity(Vector3 vel, float maxSpeed)`: Normalize to [-1, 1]
- `NormalizeAngle(float angle)`: Convert to cos/sin pair
- `NormalizeHealth(float health, float maxHealth)`: Normalize to [0, 1]
- `NormalizeTime(float time, float maxTime)`: Normalize to [0, 1]

### Action Interpretation Helpers

- `DiscretizeAction(float continuous, int numOptions)`: Convert continuous to discrete
- `ClampAction(float action, float min, float max)`: Ensure valid range
- `ThresholdAction(float action, float threshold)`: Boolean decision
- `BlendActions(ActionBuffers ai, ActionBuffers rl, float weight)`: Arbiter blending

### Physics Controller Utilities

- `ApplyWeightShift(Rigidbody rb, float shift)`: Shift center of mass
- `ApplyAttackForce(Rigidbody rb, AttackData attack, float multiplier)`: Add punch force
- `MaintainBalance(Rigidbody rb, Vector3 targetCoM)`: PD controller for balance
- `CalculateCenterOfMass(GameObject character)`: Get current CoM

---

## Training Configuration Template

Standard YAML structure for ML-Agents training configs:

```yaml
behaviors:
  PhysicsAgent:
    trainer_type: ppo

    hyperparameters:
      batch_size: 2048
      buffer_size: 20480
      learning_rate: 3e-4
      beta: 5.0e-3
      epsilon: 0.2
      lambd: 0.95
      num_epoch: 3
      learning_rate_schedule: linear

    network_settings:
      normalize: true
      hidden_units: 256
      num_layers: 2
      vis_encode_type: simple

    reward_signals:
      extrinsic:
        gamma: 0.99
        strength: 1.0

    keep_checkpoints: 5
    max_steps: 5000000
    time_horizon: 1000
    summary_freq: 10000

    self_play:
      save_steps: 50000
      team_change: 200000
      swap_steps: 10000
      window: 10
      play_against_latest_model_ratio: 0.5
      initial_elo: 1200.0
```

Adjust parameters per phase based on complexity.

---

## Common Pitfalls to Avoid

### ML-Agents Specific

1. **Forgetting to normalize observations**: Always normalize to [-1, 1] or [0, 1]
2. **Observation count mismatch**: Ensure `AddObservation()` calls match declared space size
3. **Action space confusion**: Continuous vs discrete - be consistent
4. **Not resetting environment**: Always reset positions, health, etc. in `OnEpisodeBegin()`
5. **Reward scale issues**: Keep rewards roughly in -1 to +1 per step range
6. **Training too long initially**: Start with short runs to validate setup

### Physics Integration

1. **Force magnitude too high**: Start small, tune upward
2. **Conflicting forces**: Animation vs physics - need careful blending
3. **Rigidbody constraints wrong**: May need to freeze rotation axes
4. **Time.fixedDeltaTime misalignment**: Use FixedUpdate for physics
5. **Collision layer issues**: Ensure proper layer configuration

### Architecture

1. **Tight coupling**: Keep PhysicsAgent independent of CharacterAI initially
2. **Premature optimization**: Get it working before optimizing
3. **Ignoring existing systems**: Leverage CharacterCombat, CharacterMovement patterns
4. **Not logging enough**: Add extensive debug logging during development

---

## Testing Strategy

### Phase 1: Unit Tests (Before Implementation)

- Test observation normalization math
- Test action interpretation logic
- Test reward component calculations
- Mock ML-Agents components for testing

### Phase 2: Integration Tests (After Implementation)

- Test agent initialization in scene
- Test observation collection returns correct dimension
- Test action execution integrates with character systems
- Test episode reset properly

### Phase 3: Training Validation

- Run 5-minute training to verify no errors
- Check TensorBoard for reasonable reward trends
- Manually test agent behavior at checkpoints
- Compare against behavioral AI baseline

### Phase 4: Performance Testing

- Measure FPS with 8 parallel training environments
- Measure inference time per decision step
- Profile memory allocation during training
- Ensure 60fps in gameplay with trained agent

---

## Commit Message Format

All commits should follow conventional commits format:

```
<type>(<scope>): <brief description>

- Detailed change 1
- Detailed change 2
- Detailed change 3

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Types**:
- `feat`: New feature implementation
- `fix`: Bug fixes
- `test`: Adding or updating tests
- `refactor`: Code restructuring without behavior change
- `docs`: Documentation updates
- `chore`: Build, dependencies, tooling
- `perf`: Performance improvements

**Scopes** for this project:
- `ml-agents`: ML-Agents setup, training config
- `physics-agent`: PhysicsAgent component
- `arbiter`: AIArbiter component
- `observations`: Observation collection
- `actions`: Action interpretation
- `rewards`: Reward function
- `controllers`: Physics controllers
- `training`: Training infrastructure

**Example**:
```
feat(physics-agent): implement base PhysicsAgent component

- Created PhysicsAgent.cs inheriting from Unity.MLAgents.Agent
- Implemented CollectObservations with self position and velocity
- Implemented OnActionReceived with basic movement actions
- Added OnEpisodeBegin to reset character position
- Configured behavior spec for continuous action space

🤖 Generated with [Claude Code](https://claude.com/claude.code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Integration with Existing Codebase

### CharacterAI Integration

The new PhysicsAgent will work **alongside** the existing CharacterAI component:

**Existing CharacterAI**:
- Keeps AIStateMachine with behavioral states
- Continues making high-level strategic decisions
- Outputs desired actions (move, attack, block)

**New PhysicsAgent**:
- Runs in parallel with AIStateMachine
- Makes physics-aware control decisions
- Outputs physics-enhanced actions

**AIArbiter**:
- Receives inputs from both systems
- Decides which to use or blends them
- Outputs final commands to CharacterMovement, CharacterCombat

### Data Flow

```
CharacterAI (coordinator)
    ↓
[AIStateMachine] → behavioral actions → [AIArbiter] → final actions
                                              ↑
[PhysicsAgent]   → RL actions        →  [AIArbiter]
                                              ↓
                                      CharacterMovement
                                      CharacterCombat
```

### Abstraction Layer

Create `AIAction` data structure to standardize output from both systems:

```csharp
public struct AIAction
{
    public Vector2 movementInput;      // -1 to 1
    public float turnRate;             // -1 to 1
    public AttackType attackType;      // None, Jab, Hook, Uppercut
    public bool shouldBlock;
    public float weightShift;          // -1 to 1
    public Dictionary<string, float> physicsParams;
}
```

Both AIStateMachine and PhysicsAgent output `AIAction`, arbiter blends/selects, character components consume.

---

## Performance Considerations

### Training Performance

- **Target**: Train locally with 8-16 parallel environments at 5-10 FPS
- **Optimization**: Use simple graphics in training scenes (no shadows, low-poly)
- **Monitoring**: Check CPU/GPU usage, reduce parallelism if overloaded
- **Checkpointing**: Save every 50k-100k steps for safety

### Inference Performance

- **Target**: Inference adds <1ms overhead per decision step (5-10 Hz)
- **Model Size**: Keep network small (2 hidden layers, 256 units each)
- **Optimization**: Use ONNX Runtime's CPU optimizations
- **Batch Inference**: If multiple agents, batch observations (future optimization)

### Memory Management

- **Training**: Expect ~4-8GB RAM usage with 8 parallel environments
- **Inference**: Trained model ~1-5MB
- **Observation Buffer**: Pre-allocate observation arrays, avoid GC
- **Action Buffer**: Reuse ActionBuffers, don't allocate each frame

---

## Next Steps

After reading Phase 0, proceed to:
- **[Phase 1: Training Infrastructure & Movement](Phase-1.md)** to begin implementation

Refer back to Phase 0 throughout development for architecture decisions, patterns, and conventions.

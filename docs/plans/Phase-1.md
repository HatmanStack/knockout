# Phase 1: Training Infrastructure & Basic Movement

## Phase Goal

Establish Unity ML-Agents training infrastructure and implement physics-enhanced movement and footwork. By the end of this phase, you will have a functioning RL training pipeline with a PhysicsAgent that learns natural, physics-based locomotion through self-play. The agent will move realistically with proper weight transfer, momentum, and balance while navigating the arena.

**Success Criteria:**
- ML-Agents installed and configured correctly
- Training scene with 8+ parallel environments functional
- PhysicsAgent component implements movement observations and actions
- Physics controllers apply forces for realistic movement
- Agent trains successfully via self-play (reward increases over time)
- Trained agent demonstrates natural-looking footwork
- Model exported and loads for inference

**Estimated tokens:** ~95,000

## Prerequisites

- Phase 0 completed (architecture and design decisions read)
- Python 3.9 or 3.10 installed (verify with `python --version`)
- Unity 2021.3.8f1 LTS project opened
- Existing codebase familiarized (CharacterMovement, CharacterAI, etc.)
- At least 8GB RAM available for local training

---

## Task 1: Install and Configure Unity ML-Agents

**Goal:** Set up the Python ML-Agents package and Unity package for reinforcement learning training.

**Files to Modify/Create:**
- Python environment in project root
- `Packages/manifest.json` - Add ML-Agents Unity package
- `ProjectSettings/ProjectSettings.asset` - Verify scripting runtime

**Prerequisites:**
- Clean Python 3.9 or 3.10 installation
- `uv` package manager available (already installed per CLAUDE.md)

**Implementation Steps:**

1. **Create Python virtual environment** for ML-Agents:
   - Navigate to project root directory
   - Create virtual environment using `uv venv` or standard Python venv
   - Activate the virtual environment
   - Document activation command for future use

2. **Install ML-Agents Python package**:
   - Install `mlagents==0.30.0` (Release 20) using `uv pip install` or pip
   - Verify installation by running `mlagents-learn --help`
   - Check that PyTorch is installed as dependency (should auto-install)
   - Document any installation errors and resolutions

3. **Add ML-Agents Unity package**:
   - Open Unity Package Manager (Window > Package Manager)
   - Click "+" dropdown > "Add package from git URL"
   - Enter: `https://github.com/Unity-Technologies/ml-agents.git?path=com.unity.ml-agents#release_20`
   - Wait for package import (may take several minutes)
   - Verify package appears in Package Manager as "ML Agents" version 2.3.0-exp.3

4. **Verify setup**:
   - Check that `Unity.MLAgents` namespace is available in C# scripts
   - Open Unity > Edit > Project Settings > Player
   - Under "Other Settings" > "Configuration" > "Scripting Backend", ensure it's set to "Mono" (IL2CPP may cause issues with ML-Agents)
   - Verify API Compatibility Level is ".NET Standard 2.1"

5. **Create documentation file** `docs/ML_AGENTS_SETUP.md`:
   - Document Python environment activation steps
   - List exact package versions installed
   - Include troubleshooting notes for common issues
   - Provide quick start command for training

**Verification Checklist:**
- [ ] Python environment activates without errors
- [ ] `mlagents-learn --help` displays help text
- [ ] Unity Package Manager shows ML Agents package installed
- [ ] Can import `using Unity.MLAgents;` in C# script without errors
- [ ] `docs/ML_AGENTS_SETUP.md` created with setup instructions

**Testing Instructions:**
- Create minimal test script that inherits from `Unity.MLAgents.Agent`
- Verify script compiles without errors
- Delete test script after verification

**Commit Message Template:**
```
chore(ml-agents): install and configure ML-Agents framework

- Created Python virtual environment with mlagents==0.30.0
- Added Unity ML-Agents package via Package Manager
- Verified scripting backend and API compatibility settings
- Created ML_AGENTS_SETUP.md with installation instructions
- Confirmed Unity.MLAgents namespace available in C#

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~4,000

---

## Task 2: Create Base PhysicsAgent Component

**Goal:** Implement the foundational PhysicsAgent component that inherits from Unity's Agent class and provides the structure for all future RL behaviors.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (new)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs.meta` (Unity auto-generated)

**Prerequisites:**
- Task 1 completed (ML-Agents installed)
- Familiarity with ML-Agents Agent class API
- Understanding of Phase 0 ADR-005 (Observation Space) and ADR-006 (Action Space)

**Implementation Steps:**

1. **Create folder structure**:
   - Create `Assets/Knockout/Scripts/AI/PhysicsAgent/` directory
   - This will house all physics agent related components

2. **Implement PhysicsAgent.cs skeleton**:
   - Create class inheriting from `Unity.MLAgents.Agent`
   - Add component requirements: `RequireComponent(typeof(CharacterController))`
   - Add serialized fields for tuning:
     - Decision interval (default 0.1f seconds for 10Hz)
     - Max step count per episode (default 5000 steps)
     - Arena bounds (for normalization)
   - Add private component references:
     - CharacterController (coordinator component)
     - CharacterMovement
     - CharacterHealth
     - CharacterCombat
     - Rigidbody (for physics forces)

3. **Implement Initialize() override**:
   - Cache all component references using GetComponent
   - Validate that all required components exist (log errors if missing)
   - Set MaxStep property for episode length
   - Initialize any data structures needed

4. **Implement OnEpisodeBegin() override**:
   - Reset character position to random starting position in arena
   - Reset character rotation to face opponent
   - Reset health to full (via CharacterHealth.ResetHealth())
   - Reset velocity to zero (Rigidbody.velocity = Vector3.zero)
   - Clear any applied forces
   - If in training mode, randomize starting conditions slightly for variety

5. **Implement CollectObservations() stub**:
   - For now, just add placeholder observations to meet minimum requirement
   - Add comment: "Movement observations to be implemented in Task 4"
   - Add basic observations to avoid errors:
     - Self position (x, y, z)
     - Self velocity (x, y, z)
   - Use `sensor.AddObservation()` for each value after normalization

6. **Implement OnActionReceived() stub**:
   - For now, just extract action values to verify action space works
   - Add comment: "Movement actions to be implemented in Task 5"
   - Extract continuous actions from `actions.ContinuousActions`
   - Log action values for debugging (can remove later)
   - Don't apply any actual movement yet

7. **Implement Heuristic() override**:
   - Map keyboard input to action space for manual testing
   - WASD for movement
   - Arrow keys for turning
   - This allows testing agent without training
   - Use Input.GetAxis() and normalize to [-1, 1]

8. **Add helper methods**:
   - `private Vector3 GetArenaCenter()`: Returns center point of fighting arena
   - `private Bounds GetArenaBounds()`: Returns bounds of valid movement area
   - `private void ResetEpisode()`: Helper called from OnEpisodeBegin

9. **Add debug visualization** (optional but recommended):
   - In OnDrawGizmos, visualize arena bounds
   - Draw ray showing agent's facing direction
   - Helps with debugging positioning issues

**Verification Checklist:**
- [ ] PhysicsAgent.cs compiles without errors
- [ ] Component can be added to character GameObject in Unity
- [ ] Inspector shows all serialized fields
- [ ] No errors when entering Play mode with component attached
- [ ] Heuristic mode allows manual control via keyboard
- [ ] Episode resets character position and health

**Testing Instructions:**
- Unit tests not required yet (no complex logic to test)
- Manual testing:
  - Add PhysicsAgent to player character GameObject
  - Enter Play mode
  - Verify component initializes without errors
  - Test Heuristic control with keyboard
  - Observe episode reset after MaxStep reached

**Commit Message Template:**
```
feat(physics-agent): implement base PhysicsAgent component

- Created PhysicsAgent.cs inheriting from Unity.MLAgents.Agent
- Implemented Initialize with component reference caching
- Implemented OnEpisodeBegin with position/health reset logic
- Added placeholder CollectObservations and OnActionReceived
- Implemented Heuristic for keyboard-based manual testing
- Added arena bounds helper methods for normalization
- Included debug visualization with OnDrawGizmos

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~6,000

---

## Task 3: Create Training Scene with Parallel Environments

**Goal:** Set up a dedicated training scene with multiple parallel environments to accelerate self-play RL training.

**Files to Modify/Create:**
- `Assets/Knockout/Scenes/Training/TrainingArena.unity` (new scene)
- `Assets/Knockout/Prefabs/Training/TrainingCharacter.prefab` (new prefab)
- `Assets/Knockout/Scripts/Training/TrainingEnvironment.cs` (new component)
- `Assets/Knockout/Scripts/Training/EpisodeManager.cs` (new component)

**Prerequisites:**
- Task 2 completed (PhysicsAgent component exists)
- Existing character prefabs with all required components
- Understanding of ML-Agents environment setup

**Implementation Steps:**

1. **Create scene folder structure**:
   - Create `Assets/Knockout/Scenes/Training/` directory
   - Create new scene: `TrainingArena.unity`
   - Set up basic lighting (directional light only, no shadows for performance)
   - Add simple ground plane for fighting arena

2. **Create TrainingCharacter prefab**:
   - Duplicate existing player/AI character prefab
   - Name it `TrainingCharacter.prefab`
   - Ensure it has all required components:
     - CharacterController (coordinator)
     - CharacterMovement
     - CharacterCombat
     - CharacterHealth
     - CharacterAnimator
     - Rigidbody (add if not present, set constraints as needed)
     - PhysicsAgent (add, set Behavior Name = "PhysicsAgent")
   - Remove CharacterInput if present (not needed for RL)
   - Remove CharacterAI if present (we're training RL only initially)
   - Configure Rigidbody: mass = 70kg, drag = 0.5, angular drag = 5
   - Set Rigidbody constraints: freeze rotation on X and Z axes (prevent tipping over)

3. **Create TrainingEnvironment component**:
   - Purpose: Manages a single self-play environment (2 characters fighting)
   - Serialized fields:
     - Two Transform references for character spawn points
     - Arena bounds (Vector3 center, Vector3 size)
     - Max episode length in seconds
     - Reward scaling factor
   - Responsibilities:
     - Instantiate two TrainingCharacter prefabs at spawn points
     - Detect episode end conditions (knockout, time limit, out of bounds)
     - Assign opponents to each agent (they need to know who to observe)
     - Call EndEpisode() on both agents when episode completes
   - Implement episode end detection:
     - Check if either character health reaches zero
     - Check if episode duration exceeds max length
     - Check if either character exits arena bounds
   - When episode ends, log basic statistics (duration, winner, cause)

4. **Create EpisodeManager component**:
   - Purpose: Coordinates multiple parallel training environments
   - Serialized fields:
     - Number of parallel environments (default 8)
     - TrainingEnvironment prefab reference
     - Global statistics tracking
   - Responsibilities:
     - Instantiate N TrainingEnvironment instances in scene
     - Arrange environments in grid pattern so they don't overlap
     - Aggregate statistics across all environments
     - Provide UI for monitoring training (optional but helpful)

5. **Set up TrainingArena scene**:
   - Add EpisodeManager GameObject to scene
   - Configure EpisodeManager:
     - Set parallel environments to 8 (adjust based on your hardware)
     - Assign TrainingEnvironment prefab
   - Create TrainingEnvironment prefab:
     - Empty GameObject with TrainingEnvironment component
     - Two child empty GameObjects as spawn points (5 units apart)
     - Simple arena ground (10x10 plane)
   - Let EpisodeManager instantiate environments at runtime

6. **Configure PhysicsAgent behavior settings**:
   - Open TrainingCharacter prefab
   - Select PhysicsAgent component
   - Set "Behavior Name" to "PhysicsAgent" (must match training config)
   - Set "Decision Requester" component (add if not present):
     - Decision Period: 2 (makes decision every 2 FixedUpdate steps, ~0.04s at 50Hz physics)
     - Take Actions Between Decisions: true
   - Set MaxStep on PhysicsAgent to 5000 (episode ends after this many steps)

7. **Handle opponent detection**:
   - In TrainingEnvironment, after instantiating characters:
     - Get PhysicsAgent from each character
     - Set opponent reference on each agent (may need to add public property to PhysicsAgent)
     - This allows agents to observe each other's state
   - Update PhysicsAgent to store opponent reference

8. **Optimize training scene for performance**:
   - Disable shadows on all lights
   - Use low-poly character models if available
   - Disable any VFX or particle systems
   - Set camera to static position (or disable if not needed)
   - Quality Settings: Set to "Fastest" for training
   - Target framerate: Don't limit (Time.captureFr amerate = 0)

9. **Add training UI overlay** (optional but recommended):
   - TextMeshPro or UI Text showing:
     - Current episode count
     - Average episode length
     - Recent rewards
     - Training FPS
   - Updated by EpisodeManager each episode

**Verification Checklist:**
- [ ] TrainingArena scene opens without errors
- [ ] Entering Play mode spawns 8 parallel environments
- [ ] Each environment has 2 characters facing each other
- [ ] Characters have PhysicsAgent component with correct behavior name
- [ ] Episodes automatically reset when conditions met
- [ ] No visual overlap between parallel environments
- [ ] Scene runs at acceptable FPS (>5 FPS for 8 environments)

**Testing Instructions:**
- Manual testing in Unity Editor:
  - Open TrainingArena scene
  - Enter Play mode
  - Observe that environments spawn correctly
  - Wait for episodes to timeout and reset
  - Check Console for any errors
  - Verify FPS is acceptable
- No automated tests required for scene setup

**Commit Message Template:**
```
feat(training): create parallel training scene infrastructure

- Created TrainingArena.unity scene with parallel environments
- Implemented TrainingCharacter prefab with PhysicsAgent
- Created TrainingEnvironment component for single env management
- Created EpisodeManager for coordinating parallel envs
- Configured 8 parallel environments in grid layout
- Added episode end detection (knockout, timeout, out-of-bounds)
- Optimized scene for training performance (disabled shadows, low quality)
- Added optional training UI for monitoring progress

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~8,000

---

## Task 4: Implement Movement Observations

**Goal:** Collect comprehensive observations about character physics state, opponent state, spatial relationships, and game context to enable informed movement decisions.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentObservations.cs` (new)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (modify)

**Prerequisites:**
- Task 2 completed (base PhysicsAgent exists)
- Task 3 completed (training scene with opponents set up)
- Review Phase 0 ADR-005 for observation space design

**Implementation Steps:**

1. **Create PhysicsAgentObservations helper class**:
   - Purpose: Encapsulates observation collection logic, keeps PhysicsAgent clean
   - Make it a static class with utility methods for observation normalization
   - Methods to implement:
     - `NormalizePosition(Vector3 pos, Bounds bounds)` → Vector3 normalized to [-1, 1]
     - `NormalizeVelocity(Vector3 vel, float maxSpeed)` → Vector3 normalized to [-1, 1]
     - `NormalizeAngle(float angle)` → (cos, sin) pair
     - `NormalizeScalar(float value, float min, float max)` → float in [-1, 1] or [0, 1]
     - `EncodeAnimationState(CombatState state)` → one-hot vector as float array
   - Add clear XML documentation for each method

2. **Implement observation space configuration**:
   - In PhysicsAgent, add constant for observation size
   - Calculate based on Phase 0 design (~50-80 dimensions):
     - Self physics: 20 observations
     - Opponent: 15 observations
     - Spatial: 10 observations
     - Game context: 10 observations
   - Override `CollectObservations(VectorSensor sensor)` in PhysicsAgent
   - Observation order must be consistent across all episodes

3. **Collect self physics state observations**:
   - Self position (3): Normalize to arena bounds
     - `sensor.AddObservation(PhysicsAgentObservations.NormalizePosition(transform.position, arenaBounds))`
   - Self velocity (3): Normalize to max movement speed
     - Get from Rigidbody or CharacterMovement component
   - Self facing direction (2): Forward vector (x, z only, normalized)
   - Self angular velocity (1): How fast turning, normalized
   - Ground contact (1): Boolean as 0 or 1 (check Physics.Raycast down)
   - Center of mass offset (2): CoM position relative to feet (x, z), normalized
   - Animation state (6): One-hot encoding of combat state (idle, moving, attacking, blocking, hit-stunned, knocked-down)
   - Momentum magnitude (1): Speed, normalized
   - **Total: ~19 observations**

4. **Collect opponent state observations**:
   - Get opponent reference (should be set by TrainingEnvironment)
   - Opponent position (3): Normalized to arena bounds
   - Opponent velocity (3): Normalized
   - Opponent facing direction (2): Forward (x, z)
   - Opponent animation state (6): One-hot encoding
   - Opponent current attack (4): One-hot (none, jab, hook, uppercut)
     - Get from opponent's CharacterCombat component
   - **Total: ~18 observations**

5. **Collect spatial relationship observations**:
   - Distance to opponent (1): Normalized to max arena diagonal
   - Relative position to opponent (3): Vector from self to opponent, normalized
   - Angle to opponent (2): cos/sin of angle between forward and opponent direction
   - Is opponent in attack range (1): Boolean, check distance < max attack range
   - Is self in opponent's attack range (1): Approximate using distance and opponent facing
   - Distance to nearest arena boundary (1): Normalized
   - **Total: ~9 observations**

6. **Collect game context observations**:
   - Self health percentage (1): Already in [0, 1] range
   - Opponent health percentage (1): From opponent's CharacterHealth
   - Round number (1): Normalized to max rounds (e.g., 3)
   - Round time remaining (1): Normalized to max round time
   - Rounds won by self (1): Normalized to max rounds
   - Rounds won by opponent (1): Normalized
   - Time since last hit received (1): Normalized, cap at some max (e.g., 10 seconds)
   - Time since last hit dealt (1): Normalized, cap at max
   - **Total: ~8 observations**

7. **Handle missing opponent**:
   - If opponent reference is null, add zero observations for opponent state
   - Log warning in Editor but don't error in build
   - This allows testing agent in non-training scenarios

8. **Add observation validation**:
   - In Editor mode, validate observation count matches expected
   - Add Debug.Assert to catch mismatches during development
   - Ensure all observations are in valid range (not NaN, not Infinity)
   - Log any out-of-range values for debugging

9. **Update PhysicsAgent.CollectObservations()**:
   - Call helper methods from PhysicsAgentObservations
   - Keep code clean and readable with clear sections for each observation category
   - Add comments labeling each observation group
   - Total observations should be ~50-60 (adjust as needed for your specific implementation)

10. **Configure behavior parameters**:
    - If using BehaviorParameters component, ensure:
      - Vector Observation Space Size matches total observations
      - Stacked Vectors = 1 (no frame stacking initially)
      - Vector Action Space Type = Continuous
      - Vector Action Space Size = will be set in Task 5

**Verification Checklist:**
- [ ] PhysicsAgentObservations.cs compiles without errors
- [ ] All normalization methods tested (unit tests)
- [ ] PhysicsAgent.CollectObservations() adds correct number of observations
- [ ] No NaN or Infinity values in observations
- [ ] Observation count matches BehaviorParameters configuration
- [ ] Observations update correctly each decision step (verify in debugger)

**Testing Instructions:**
- Write unit tests in `Assets/Knockout/Tests/EditMode/PhysicsAgentObservationsTests.cs`:
  - Test position normalization with various arena bounds
  - Test velocity normalization at max speed
  - Test angle normalization at 0°, 90°, 180°, 270°
  - Test scalar normalization at min, max, middle values
  - Test animation state encoding produces correct one-hot vector
- Run tests to ensure all pass before committing

**Commit Message Template:**
```
feat(observations): implement comprehensive movement observation collection

- Created PhysicsAgentObservations utility class for normalization
- Implemented self physics state observations (position, velocity, CoM, etc.)
- Implemented opponent state observations (position, velocity, attack state)
- Implemented spatial relationship observations (distance, relative position, angle)
- Implemented game context observations (health, round state, time)
- Added observation validation to catch NaN/Infinity values
- Configured BehaviorParameters with correct observation space size
- Added unit tests for all normalization methods

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 5: Implement Movement Actions

**Goal:** Define and interpret continuous actions for physics-based movement control including direction, speed, and turning.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentActions.cs` (new)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (modify)

**Prerequisites:**
- Task 4 completed (observations implemented)
- Review Phase 0 ADR-006 for action space design
- Understanding of continuous action space

**Implementation Steps:**

1. **Create PhysicsAgentActions helper class**:
   - Purpose: Interpret raw continuous actions from neural network
   - Make it a static class with action interpretation methods
   - Methods to implement:
     - `ClampAction(float action, float min, float max)` → clamped float
     - `ThresholdAction(float action, float threshold)` → boolean
     - `MapAction(float action, float sourceMin, float sourceMax, float targetMin, float targetMax)` → mapped float
   - Add XML documentation

2. **Design movement action space**:
   - For Phase 1, focus only on movement actions (attack/defense in later phases)
   - Continuous action vector (8 dimensions):
     - **Movement X** (index 0): Left/right strafe, range [-1, 1]
     - **Movement Z** (index 1): Back/forward, range [-1, 1]
     - **Speed multiplier** (index 2): Movement speed, range [0, 1]
     - **Turn rate** (index 3): Rotation speed, range [-1, 1]
     - **Weight shift** (index 4): CoM shift forward/back, range [-1, 1]
     - **Stance width** (index 5): Narrow to wide stance, range [0.5, 1.2]
     - **CoM height** (index 6): Crouch to upright, range [0.7, 1.0]
     - **Unused** (index 7): Reserved for future use, currently ignored
   - Configure BehaviorParameters Vector Action Space Size = 8

3. **Implement OnActionReceived override in PhysicsAgent**:
   - Extract continuous actions from ActionBuffers parameter
   - Store actions in private fields for use in FixedUpdate
   - Don't apply actions immediately (will be applied in physics update)
   - Example structure:
     ```
     var continuousActions = actions.ContinuousActions;
     _movementX = continuousActions[0];
     _movementZ = continuousActions[1];
     // ... etc
     ```

4. **Create movement action interpretation logic**:
   - Create private method `InterpretMovementActions()` in PhysicsAgent
   - Clamp and process each action value:
     - Clamp movement X and Z to [-1, 1]
     - Clamp speed multiplier to [0, 1]
     - Clamp turn rate to [-1, 1]
     - Clamp physics parameters to valid ranges
   - Create Vector2 for movement input from X and Z components
   - Calculate actual turn angle from turn rate (-180 to 180 degrees/sec)
   - Normalize movement vector if magnitude > 1 (prevents faster diagonal movement)

5. **Apply movement via CharacterMovement component**:
   - In FixedUpdate (physics timing):
     - Call InterpretMovementActions()
     - Pass movement Vector2 to CharacterMovement.SetMovementInput()
     - Apply rotation via CharacterMovement.RotateToward() or direct transform.Rotate()
     - Multiply movement by speed multiplier before applying
   - Let existing CharacterMovement handle actual locomotion (leverage existing system)
   - CharacterMovement should use Rigidbody for physics-based movement

6. **Store actions in PhysicsAgent for physics controller use**:
   - Add public properties to expose interpreted actions:
     - `public Vector2 MovementInput { get; private set; }`
     - `public float SpeedMultiplier { get; private set; }`
     - `public float TurnRate { get; private set; }`
     - `public float WeightShift { get; private set; }`
     - `public float StanceWidth { get; private set; }`
     - `public float CenterOfMassHeight { get; private set; }`
   - Physics controllers (Task 6) will read these

7. **Update Heuristic method**:
   - Map keyboard input to action space for manual testing
   - WASD → movement X and Z (-1 to 1)
   - Left/Right arrows → turn rate
   - Shift → speed boost (speed multiplier to 1.0, default 0.7)
   - Q/E → weight shift forward/back
   - This allows testing movement system without training

8. **Add action visualization for debugging**:
   - In OnDrawGizmos:
     - Draw movement vector as arrow from character
     - Draw turn direction indicator
     - Color code based on speed multiplier
   - Helps visualize what actions agent is taking

9. **Configure action space in BehaviorParameters**:
   - Set Vector Action Space Type: Continuous
   - Set Vector Action Space Size: 8 (for our 8 action dimensions)
   - Ensure Continuous Actions size matches

**Verification Checklist:**
- [ ] PhysicsAgentActions.cs compiles without errors
- [ ] BehaviorParameters action space configured correctly (8 continuous actions)
- [ ] OnActionReceived extracts and stores actions without errors
- [ ] Heuristic control works with keyboard input
- [ ] Character moves when actions applied
- [ ] Movement respects physics (momentum, collision)
- [ ] No jittery or unstable movement

**Testing Instructions:**
- Unit tests in `Assets/Knockout/Tests/EditMode/PhysicsAgentActionsTests.cs`:
  - Test ClampAction with values outside range
  - Test ThresholdAction at boundary values
  - Test MapAction with various ranges
- Manual testing:
  - Open TrainingArena scene
  - Set PhysicsAgent to Heuristic mode (Behavior Type: Heuristic Only in BehaviorParameters)
  - Enter Play mode and test keyboard controls
  - Verify movement feels responsive
  - Verify turning works correctly
  - Verify speed multiplier affects movement speed

**Commit Message Template:**
```
feat(actions): implement movement action interpretation and execution

- Created PhysicsAgentActions utility class for action processing
- Defined 8-dimensional continuous action space for movement
- Implemented OnActionReceived to extract and store actions
- Created InterpretMovementActions for action processing
- Integrated with CharacterMovement for physics-based locomotion
- Updated Heuristic method with full keyboard control mapping
- Added action visualization in OnDrawGizmos for debugging
- Configured BehaviorParameters with correct action space size
- Added unit tests for action interpretation methods

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 6: Implement Movement Physics Controllers

**Goal:** Create physics controllers that apply forces and adjustments based on RL actions to achieve realistic weight transfer, momentum, and balance during movement.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsControllers/MovementController.cs` (new)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (modify to use controller)

**Prerequisites:**
- Task 5 completed (actions interpreted)
- Understanding of Unity physics (Rigidbody, forces, center of mass)
- Review Phase 0 ADR-002 on physics-enhanced animation approach

**Implementation Steps:**

1. **Create MovementController class**:
   - Purpose: Applies physics forces to enhance animated movement with realistic weight and momentum
   - Make it a separate component or embedded class in PhysicsAgent
   - Serialized parameters for tuning:
     - Movement force multiplier (how much force to apply for movement)
     - Turn torque multiplier (how much torque for rotation)
     - Weight shift magnitude (how much to offset center of mass)
     - Balance restoration force (PD controller gains for maintaining balance)
     - Friction coefficients (ground friction)

2. **Implement weight shifting**:
   - Method: `ApplyWeightShift(Rigidbody rb, float shift)`
   - Shift parameter comes from action space (-1 to 1)
   - Shift center of mass forward/backward:
     - Get current Rigidbody center of mass
     - Calculate offset based on shift parameter (e.g., shift * 0.3 units forward)
     - Apply offset to Rigidbody.centerOfMass
   - This simulates leaning forward (aggressive) or back (defensive)
   - Reset center of mass to default when shift is 0

3. **Implement stance width adjustment**:
   - Method: `ApplyStanceWidth(float width)`
   - Width parameter from action space (0.5 to 1.2)
   - Adjust CharacterController capsule radius or use IK to widen legs
   - Wider stance = more stable but slower
   - Narrower stance = less stable but more mobile
   - Could be visual-only or affect physics stability threshold

4. **Implement center of mass height control**:
   - Method: `ApplyCenterOfMassHeight(float height)`
   - Height parameter from action space (0.7 to 1.0)
   - Lower CoM = more stable, better for defensive posture
   - Higher CoM = less stable, better for quick movement
   - Adjust Rigidbody.centerOfMass.y based on parameter

5. **Implement movement force application**:
   - Method: `ApplyMovementForce(Rigidbody rb, Vector2 movement Input, float speedMult)`
   - Convert Vector2 input to world-space Vector3 direction
   - Apply force to Rigidbody:
     - `rb.AddForce(direction * movementForceMultiplier * speedMult, ForceMode.Force)`
   - Add drag to simulate friction when not moving
   - Cap maximum velocity to prevent unrealistic speeds

6. **Implement turning torque**:
   - Method: `ApplyTurningTorque(Rigidbody rb, float turnRate)`
   - Calculate desired angular velocity from turn rate
   - Apply torque around Y axis:
     - `rb.AddTorque(Vector3.up * turnRate * turnTorqueMultiplier, ForceMode.Force)`
   - Add angular drag to prevent spinning indefinitely
   - Clamp angular velocity to max turn speed

7. **Implement balance maintenance**:
   - Method: `MaintainBalance(Rigidbody rb, Vector3 targetCoM)`
   - PD (Proportional-Derivative) controller to keep center of mass over support base
   - Calculate error: current CoM projection vs support polygon center
   - Apply corrective force proportional to error and its derivative
   - Prevents tipping over during movement
   - Use Physics.Raycast to detect ground contact points

8. **Integrate with PhysicsAgent**:
   - In PhysicsAgent FixedUpdate:
     - Create or get MovementController instance
     - Pass interpreted actions to controller methods
     - Apply weight shift, stance, CoM height
     - Apply movement forces and turning torque
     - Run balance maintenance
   - Ensure this happens after action interpretation but during physics step

9. **Tune physics parameters**:
   - Start with conservative values and increase gradually:
     - Movement force: 500-1000 Newtons
     - Turn torque: 100-300 Nm
     - Weight shift offset: 0.2-0.5 units
     - Balance PD gains: Kp=100, Kd=10 (tune experimentally)
   - Test in Heuristic mode to feel responsiveness
   - Adjust Rigidbody mass (default 70kg) and drag values

10. **Add safety constraints**:
    - Clamp forces to maximum values to prevent physics explosions
    - Detect if character tipped over (up vector angle > 45°) and reset
    - Handle edge cases like null Rigidbody or disabled components
    - Log warnings for invalid physics states

**Verification Checklist:**
- [ ] MovementController.cs compiles without errors
- [ ] Weight shift visibly affects character lean
- [ ] Stance width adjustment visible (if implemented visually)
- [ ] Movement feels natural with momentum
- [ ] Turning has appropriate acceleration/deceleration
- [ ] Character maintains balance during movement
- [ ] No physics explosions or jittery behavior
- [ ] Forces tuned for realistic movement speed

**Testing Instructions:**
- Manual testing in TrainingArena with Heuristic mode:
  - Move in all directions (forward, back, strafe left/right)
  - Verify momentum carries character slightly after releasing input
  - Test turning while moving (should feel smooth)
  - Test weight shifting (character should lean visibly)
  - Test rapid direction changes (should not be instant)
  - Verify character doesn't tip over during normal movement
- Play multiple episodes and observe movement naturalness
- Compare to existing CharacterMovement behavior (should be similar or better)

**Commit Message Template:**
```
feat(physics-controllers): implement movement physics controllers

- Created MovementController for physics-enhanced movement
- Implemented weight shifting via center of mass manipulation
- Implemented stance width and CoM height adjustments
- Applied movement forces with momentum simulation
- Applied turning torque with angular velocity control
- Implemented PD controller for balance maintenance
- Integrated controllers with PhysicsAgent FixedUpdate
- Tuned initial physics parameters for natural movement feel
- Added safety constraints to prevent physics instabilities

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~12,000

---

## Task 7: Implement Movement Reward Function

**Goal:** Design and implement reward signals that encourage natural, effective, physics-realistic movement behavior during RL training.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentRewards.cs` (new)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (modify to calculate rewards)

**Prerequisites:**
- Task 6 completed (physics controllers working)
- Review Phase 0 ADR-007 on multi-objective reward design
- Understanding of reward shaping for RL

**Implementation Steps:**

1. **Create PhysicsAgentRewards helper class**:
   - Purpose: Encapsulates all reward calculation logic
   - Instance class (not static, holds state between calls)
   - Serialized parameters for reward weights:
     - Combat effectiveness weight (default 0.4)
     - Physical realism weight (default 0.2)
     - Strategic depth weight (default 0.2)
     - Player entertainment weight (default 0.2)
   - Private fields to track previous state for derivative calculations

2. **Implement reward component: Combat Effectiveness (for movement)**:
   - For Phase 1, combat is limited (no attacks yet), focus on positioning:
     - **Optimal Range Reward** (+0.05 per frame): Agent maintains 1.5-2.5 units from opponent
       - Check distance to opponent
       - If in range, add small positive reward
       - Encourages staying in effective fighting range
     - **Approach Reward** (+0.02 per frame): Agent closes distance if too far (>3.5 units)
     - **Space Reward** (+0.02 per frame): Agent creates distance if too close (<1.0 units)
   - Method: `CalculateCombatEffectivenessReward()` → float

3. **Implement reward component: Physical Realism**:
   - **Balance Reward** (+0.01 per frame):
     - Check if center of mass is over support base (feet position)
     - Calculate support polygon from foot positions
     - Reward if CoM projection is within polygon
     - Method: `IsBalanced()` → bool
   - **Smooth Movement Reward** (+0.02 per frame):
     - Penalize excessive acceleration changes (jerk)
     - Calculate acceleration derivative between frames
     - Reward if jerk magnitude is below threshold
     - Encourages smooth, natural movement
   - **Grounded Reward** (+0.01 per frame):
     - Check if feet are in contact with ground (Physics.Raycast)
     - Reward for maintaining ground contact (no floating)
   - **Weight Transfer Reward** (+0.03 per frame):
     - During movement, weight should shift appropriately
     - Check if weight shift action aligns with movement direction
     - Reward correlation between forward movement and forward weight shift
   - Method: `CalculatePhysicalRealismReward()` → float

4. **Implement reward component: Strategic Depth (for movement)**:
   - **Spacing Control Reward** (+0.05 per frame):
     - Reward for actively adjusting distance to opponent
     - Detect intentional approach or retreat (movement toward/away from opponent)
     - Penalize standing still (-0.02 per frame of no movement)
   - **Footwork Variety Reward** (+0.1 per 2 seconds):
     - Track movement patterns (circling, strafing, advancing, retreating)
     - Reward if multiple patterns used within time window
     - Prevents repetitive movement
   - **Defensive Positioning** (+0.05 per frame):
     - Reward for facing opponent while moving
     - Penalize if facing away from opponent
   - Method: `CalculateStrategicDepthReward()` → float

5. **Implement reward component: Player Entertainment (for movement)**:
   - **Movement Activity Reward** (+0.05 per frame):
     - Reward for dynamic movement (not standing still)
     - Encourage active footwork and engagement
   - **Arena Utilization Reward** (+0.02 per frame):
     - Reward for using different areas of arena
     - Penalize if staying in one spot too long
     - Track position history and calculate variance
   - **Engagement Reward** (+0.1 per frame):
     - Reward for staying in close-mid range of opponent
     - Creates more exciting fights than long-range dancing
   - Method: `CalculatePlayerEntertainmentReward()` → float

6. **Implement sparse milestone rewards**:
   - **Episode Length Bonus**:
     - Reward longer episodes (survived without going out of bounds)
     - +1.0 if episode lasts > 30 seconds
     - Discourages self-terminating behaviors
   - **Exploration Bonus**:
     - One-time reward for discovering new movement patterns
     - Track visited state-action pairs
     - +0.5 for novel states (encourages diverse movement during early training)

7. **Combine reward components**:
   - In PhysicsAgent, add method `CalculateReward()`:
     - Get individual reward components from PhysicsAgentRewards
     - Apply weights to each component
     - Sum weighted components
     - Clamp total reward to reasonable range (e.g., -1.0 to +1.0 per step)
   - Call AddReward() with calculated reward value each decision step
   - For debugging, log individual components to track which rewards are active

8. **Handle episode end rewards**:
   - In OnEpisodeBegin or when episode ends naturally:
     - Add sparse milestone rewards
     - Reset reward tracking state in PhysicsAgentRewards
   - If character goes out of bounds: large negative reward (-5.0) and EndEpisode()
   - If episode times out naturally: small positive reward (+0.5) for survival

9. **Implement reward normalization**:
   - Track running mean and std dev of rewards (optional but recommended)
   - Normalize rewards to have roughly zero mean and unit variance
   - Helps with training stability
   - Use exponential moving average for statistics

10. **Add reward debugging tools**:
    - In Editor, add gizmos showing:
      - Optimal range visualization (draw sphere around opponent)
      - Balance polygon (support base)
      - Recent reward magnitude (color-coded)
    - Add debug UI text showing current reward breakdown
    - Log reward components to CSV for analysis (optional)

**Verification Checklist:**
- [ ] PhysicsAgentRewards.cs compiles without errors
- [ ] All reward components calculate without NaN or Infinity
- [ ] Reward values are in reasonable range (-1 to 1 per step typically)
- [ ] Agent receives positive reward for desired behaviors in Heuristic mode
- [ ] Reward weights sum to meaningful values (don't need to sum to 1.0)
- [ ] Sparse rewards trigger correctly at episode boundaries
- [ ] Debug visualization shows reward components clearly

**Testing Instructions:**
- Unit tests in `Assets/Knockout/Tests/EditMode/PhysicsAgentRewardsTests.cs`:
  - Test IsBalanced() with various CoM configurations
  - Test reward calculation with mock states
  - Test reward clamping at extremes
  - Test weight application math
- Manual testing in Heuristic mode:
  - Move to optimal range and observe positive rewards
  - Stand still and observe negative rewards (passive penalty)
  - Perform smooth movement and observe physical realism rewards
  - Check that total cumulative reward increases with good play

**Commit Message Template:**
```
feat(rewards): implement multi-objective movement reward function

- Created PhysicsAgentRewards class for reward calculation
- Implemented combat effectiveness rewards (optimal range, positioning)
- Implemented physical realism rewards (balance, smooth movement, grounding)
- Implemented strategic depth rewards (spacing control, footwork variety)
- Implemented player entertainment rewards (activity, arena utilization)
- Added sparse milestone rewards for episode completion
- Integrated reward calculation into PhysicsAgent decision loop
- Added reward component weights for tuning
- Implemented reward debugging visualization and logging
- Added unit tests for reward calculation logic

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~14,000

---

## Task 8: Create Training Configuration

**Goal:** Configure ML-Agents training hyperparameters in YAML file to train the movement agent effectively via self-play.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Config/movement_training.yaml` (new)
- `Assets/Knockout/Training/Config/README.md` (new, explains config parameters)

**Prerequisites:**
- Task 7 completed (reward function implemented)
- All previous tasks completed
- Familiarity with ML-Agents configuration format

**Implementation Steps:**

1. **Create training config directory**:
   - Create `Assets/Knockout/Training/Config/` folder
   - This will store all training configurations for each phase

2. **Create movement_training.yaml**:
   - Reference Phase 0 training configuration template
   - Configure for movement-only training
   - Structure:
     ```yaml
     behaviors:
       PhysicsAgent:
         trainer_type: ppo
         hyperparameters:
           # PPO hyperparameters
         network_settings:
           # Neural network architecture
         reward_signals:
           # Reward configuration
         self_play:
           # Self-play specific settings
         checkpoint_settings:
           # Checkpoint saving
         # Other settings
     ```

3. **Configure PPO hyperparameters**:
   - `batch_size: 2048` - Number of experiences per gradient descent update
     - Larger = more stable but slower
     - Good starting point for continuous control
   - `buffer_size: 20480` - Total experiences before updating (should be multiple of batch_size)
     - buffer_size / batch_size = number of epochs
   - `learning_rate: 3e-4` - Step size for gradient descent
     - Standard for PPO, can decrease if training unstable
     - Use linear schedule to decrease over time
   - `beta: 5e-3` - Entropy bonus coefficient
     - Encourages exploration
     - Higher = more exploration, lower = more exploitation
   - `epsilon: 0.2` - PPO clip range
     - Standard value, controls policy update magnitude
   - `lambd: 0.95` - GAE (Generalized Advantage Estimation) lambda
     - Balance between bias and variance in advantage estimation
   - `num_epoch: 3` - Number of passes through experience buffer
     - More epochs = more learning per experience but slower
   - `learning_rate_schedule: linear` - Decay learning rate over time

4. **Configure network architecture**:
   - `normalize: true` - Automatically normalize observations (use built-in normalization)
   - `hidden_units: 256` - Size of hidden layers
     - Larger = more capacity but slower and harder to train
     - 256 is good balance for this task
   - `num_layers: 2` - Number of hidden layers
     - 2 layers sufficient for most tasks
     - Deeper networks may overfit on limited data
   - `vis_encode_type: simple` - Not using visual observations, but required field
   - `activation: tanh` - Activation function (tanh good for continuous control)

5. **Configure reward signals**:
   - Extrinsic reward (our custom reward function):
     ```yaml
     extrinsic:
       gamma: 0.99      # Discount factor (how much to value future rewards)
       strength: 1.0    # Scaling factor for extrinsic rewards
     ```
   - Optionally add curiosity (intrinsic reward) if agent doesn't explore:
     ```yaml
     curiosity:
       gamma: 0.99
       strength: 0.01   # Small contribution from curiosity
       encoding_size: 256
     ```

6. **Configure self-play settings**:
   - `save_steps: 50000` - Save opponent snapshot every N steps
     - Saves previous policy versions for agent to play against
   - `team_change: 200000` - Change opponent every N steps
     - Ensures agent faces variety of opponent skill levels
   - `swap_steps: 10000` - Swap sides every N steps
     - Prevents positional bias
   - `window: 10` - Keep last N opponent snapshots
     - Larger window = more diverse opponents but more memory
   - `play_against_latest_model_ratio: 0.5` - 50% vs latest, 50% vs historical
     - Balance between facing current skill level and maintaining past skills
   - `initial_elo: 1200.0` - Starting ELO rating for skill tracking

7. **Configure checkpoint and training settings**:
   - `keep_checkpoints: 5` - Number of checkpoints to retain
   - `max_steps: 5000000` - Total training steps (5 million)
     - Adjust based on how long you can train (5M might take several hours)
     - Start smaller for testing (e.g., 500k)
   - `time_horizon: 1000` - Steps before forcing end of episode for advantage calculation
     - Set to max episode length or longer
   - `summary_freq: 10000` - How often to write TensorBoard summaries
     - More frequent = finer grain monitoring but slower
   - `checkpoint_interval: 500000` - Save checkpoint every N steps

8. **Set training command line arguments** (document in README):
   - Base command:
     ```bash
     mlagents-learn Assets/Knockout/Training/Config/movement_training.yaml --run-id=movement_phase1 --time-scale=20
     ```
   - Flags:
     - `--run-id`: Unique identifier for this training run (creates folder in results/)
     - `--time-scale`: Speed up Unity time (20x faster, adjust for your hardware)
     - `--num-envs`: Number of parallel environments (auto-detected from scene)
     - `--resume`: Continue from previous checkpoint if training interrupted
     - `--force`: Overwrite existing run with same ID

9. **Create training config README**:
   - Explain each hyperparameter and when to adjust it
   - Provide troubleshooting guide:
     - If reward doesn't increase: decrease learning rate, check reward function
     - If training unstable: decrease batch size, learning rate
     - If agent doesn't explore: increase entropy beta, add curiosity
     - If training too slow: increase time-scale, reduce environments, simplify network
   - Include expected training time estimates
   - Link to ML-Agents documentation for deeper dive

10. **Validate YAML syntax**:
    - Use YAML linter to check for syntax errors
    - Ensure indentation is correct (spaces, not tabs)
    - Verify all required fields present
    - Test load config with: `mlagents-learn <config_path> --help`

**Verification Checklist:**
- [ ] movement_training.yaml has valid YAML syntax
- [ ] All required ML-Agents fields present
- [ ] Hyperparameters are reasonable starting values
- [ ] Self-play configuration included
- [ ] README.md documents all parameters clearly
- [ ] Training command documented and tested

**Testing Instructions:**
- Run config validation:
  ```bash
  mlagents-learn Assets/Knockout/Training/Config/movement_training.yaml --help
  ```
  - Should display help text without errors
  - If errors, check YAML syntax and field names
- Don't start full training yet (will do in Task 9)

**Commit Message Template:**
```
feat(training): create movement training configuration

- Created movement_training.yaml with PPO hyperparameters
- Configured network architecture (2 layers, 256 units)
- Set up self-play settings for competitive learning
- Configured reward signals (extrinsic with gamma=0.99)
- Set checkpoint and summary frequencies
- Created Config/README.md with parameter explanations
- Documented training command and common troubleshooting
- Validated YAML syntax and required fields

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 9: Execute Training and Evaluate Movement Agent

**Goal:** Run the ML-Agents training process, monitor progress, and evaluate the trained agent's movement behavior to ensure it learns natural footwork.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Results/` - Training outputs directory (auto-created)
- `Assets/Knockout/Training/Models/movement_phase1.onnx` - Exported trained model
- `docs/TRAINING_LOG.md` - Training session notes and results

**Prerequisites:**
- Tasks 1-8 completed (entire training pipeline ready)
- Python environment activated with mlagents installed
- TrainingArena scene configured and tested
- Sufficient time for training (plan for 2-4 hours minimum)

**Implementation Steps:**

1. **Pre-training validation**:
   - Open TrainingArena scene in Unity
   - Verify all environments spawn correctly
   - Check PhysicsAgent Behavior Name = "PhysicsAgent" (must match YAML)
   - Ensure BehaviorParameters set to "Default" (not Heuristic Only)
   - Save scene

2. **Start training session**:
   - Activate Python environment: `source ml-agents-env/bin/activate` (or your env name)
   - Navigate to project root in terminal
   - Run training command:
     ```bash
     mlagents-learn Assets/Knockout/Training/Config/movement_training.yaml --run-id=movement_phase1 --time-scale=20
     ```
   - When prompted "Start training by pressing the Play button in the Unity Editor", click Play in Unity
   - Training should begin - you'll see log output in terminal

3. **Monitor training progress**:
   - **Watch terminal output**:
     - Check that environments initialize correctly
     - Look for "Connected new brain" message
     - Monitor step count increasing
     - Watch for any errors or warnings
   - **Open TensorBoard** (in new terminal):
     ```bash
     tensorboard --logdir=results/movement_phase1
     ```
     - Navigate to `http://localhost:6006` in browser
     - Key metrics to watch:
       - **Environment/Cumulative Reward**: Should trend upward (agent improving)
       - **Environment/Episode Length**: May increase as agent learns to survive longer
       - **Losses/Policy Loss**: Should decrease over time
       - **Policy/Entropy**: Should decrease (agent becoming more confident)
       - **Policy/Learning Rate**: Should decrease linearly if using schedule

4. **Early training validation** (first 100k steps):
   - After ~15-30 minutes, check if learning is happening:
     - Cumulative reward should be increasing (even if slowly)
     - Agents should show some intentional behavior (not purely random)
     - No NaN or Inf values in losses
   - If no improvement after 100k steps:
     - Check reward function (are agents getting any positive rewards?)
     - Verify observations are normalized correctly
     - Consider decreasing learning rate or increasing exploration (beta)
   - You can stop training (Ctrl+C) and resume later with `--resume` flag

5. **Mid-training checkpoints** (every 500k steps):
   - Training saves checkpoints automatically to `results/movement_phase1/PhysicsAgent/`
   - Load checkpoint in Unity to test:
     - Copy `.onnx` file to `Assets/Knockout/Training/Models/`
     - In PhysicsAgent BehaviorParameters, set Model to the .onnx file
     - Set Behavior Type to "Inference Only"
     - Enter Play mode and observe agent behavior
     - Evaluate: Does movement look natural? Is agent approaching opponent? Maintaining balance?
   - If behavior looks good, consider stopping training early
   - If behavior poor, continue training or adjust hyperparameters

6. **Training duration decisions**:
   - **Minimum**: 1-2 million steps (~1-2 hours) for basic movement
   - **Recommended**: 3-5 million steps (~2-4 hours) for polished movement
   - **Stop early if**:
     - Cumulative reward plateaus for 500k+ steps
     - Agent demonstrates desired behavior consistently
     - You're satisfied with movement quality
   - **Train longer if**:
     - Reward still increasing steadily
     - Agent behavior still improving noticeably
     - You want more robust/diverse movement patterns

7. **Complete training run**:
   - When ready to finish, stop Unity and press Ctrl+C in terminal
   - Final model saved to `results/movement_phase1/PhysicsAgent/PhysicsAgent.onnx`
   - Copy final model to `Assets/Knockout/Training/Models/movement_phase1.onnx`
   - Git track the .onnx file (it's binary but worth keeping in repo)

8. **Evaluation and testing**:
   - **Quantitative evaluation**:
     - Final cumulative reward value
     - Average episode length
     - Compare to random policy baseline (if you collected)
   - **Qualitative evaluation**:
     - Load model in TrainingArena
     - Watch several episodes
     - Evaluate against criteria:
       - [ ] Agents move naturally with visible momentum
       - [ ] Agents approach opponent when far away
       - [ ] Agents maintain fighting range (1.5-2.5 units)
       - [ ] Agents show footwork variety (circling, strafing, etc.)
       - [ ] Agents maintain balance (no tipping over)
       - [ ] Movement looks realistic (weight transfer visible)
       - [ ] Agents don't exhibit degenerate strategies (spinning, running away, etc.)
   - **Document findings**:
     - Create `docs/TRAINING_LOG.md`
     - Record training parameters, duration, final metrics
     - Note any issues encountered and solutions
     - Include qualitative observations of behavior
     - Add screenshots or video if helpful

9. **Model integration test**:
   - Test trained model in main gameplay scene (not training scene):
     - Open main GameplayTest scene
     - Create character with PhysicsAgent component
     - Assign movement_phase1.onnx model
     - Set Behavior Type to Inference Only
     - Verify agent works in gameplay context
     - Check performance (FPS should remain high)

10. **Troubleshooting common issues**:
    - **Reward not increasing**:
      - Check reward function in Heuristic mode (are you getting rewards for good behavior?)
      - Verify observations are normalized correctly (check for NaN)
      - Try decreasing learning rate or increasing batch size
    - **Training unstable (NaN losses)**:
      - Check for NaN in observations or actions
      - Decrease learning rate significantly
      - Check physics for explosions (forces too high)
    - **Agents not exploring**:
      - Increase entropy beta (e.g., from 0.005 to 0.01)
      - Add curiosity reward signal
    - **Training too slow**:
      - Increase time-scale (but not above 100, causes instability)
      - Reduce number of parallel environments
      - Simplify neural network (fewer layers or units)
    - **Unity crashes during training**:
      - Reduce time-scale
      - Reduce parallel environments
      - Check for physics explosions or errors in code

**Verification Checklist:**
- [ ] Training runs without errors
- [ ] TensorBoard shows increasing cumulative reward
- [ ] No NaN or Inf values in training metrics
- [ ] Trained model loads correctly in Unity
- [ ] Agent exhibits natural-looking movement
- [ ] Agent approaches and maintains range to opponent
- [ ] Agent maintains balance and doesn't tip over
- [ ] Movement shows physics-based momentum
- [ ] Model performs well in both training and gameplay scenes
- [ ] Training results documented in TRAINING_LOG.md

**Testing Instructions:**
- Manual evaluation of trained model:
  - Load model in TrainingArena scene
  - Run 10+ episodes and observe behavior
  - Rate movement naturalness on scale of 1-10
  - Note any undesirable behaviors
  - Compare to behavioral AI movement (should be similar or better)
- Performance testing:
  - Measure FPS in gameplay scene with trained agent
  - Should maintain 60fps with single agent
  - Inference time per decision should be <1ms

**Commit Message Template:**
```
feat(training): train and validate movement agent via self-play

- Executed training run for 3M steps over 3 hours
- Monitored training progress via TensorBoard
- Achieved final cumulative reward of X.X
- Exported trained model as movement_phase1.onnx
- Validated agent demonstrates natural physics-based movement
- Verified agent maintains optimal fighting range
- Confirmed balance and momentum behaviors learned
- Documented training process and results in TRAINING_LOG.md
- Integrated model into main gameplay scene successfully
- Tested inference performance (60fps maintained)

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~15,000

---

## Phase 1 Verification

Complete this checklist before proceeding to Phase 2:

### Training Infrastructure
- [ ] ML-Agents Python package installed and functional
- [ ] Unity ML-Agents package integrated
- [ ] TrainingArena scene with 8+ parallel environments operational
- [ ] Self-play configuration tested (two agents fight each other)
- [ ] Training runs without errors for at least 30 minutes

### PhysicsAgent Implementation
- [ ] PhysicsAgent component inherits from Unity.MLAgents.Agent correctly
- [ ] Observation collection gathers ~50-60 normalized observations
- [ ] Action space configured for 8 continuous movement actions
- [ ] Heuristic mode allows keyboard control for testing
- [ ] Episode reset functionality working (position, health, velocity reset)

### Physics Controllers
- [ ] Movement forces applied realistically with momentum
- [ ] Weight shifting affects center of mass visibly
- [ ] Stance width and CoM height adjustments functional
- [ ] Turning uses torque with smooth acceleration
- [ ] Balance maintenance prevents tipping over
- [ ] No physics explosions or jittery movement

### Reward Function
- [ ] Multi-objective reward calculation implemented
- [ ] Combat effectiveness rewards (optimal range positioning)
- [ ] Physical realism rewards (balance, smooth movement, grounding)
- [ ] Strategic depth rewards (spacing control, footwork variety)
- [ ] Player entertainment rewards (active movement, engagement)
- [ ] Sparse milestone rewards for episode completion
- [ ] Reward values in reasonable range (-1 to 1 per step)

### Training Results
- [ ] Training completed for at least 1M steps
- [ ] Cumulative reward increased over training
- [ ] Trained model exported as .onnx file
- [ ] Agent demonstrates natural-looking movement
- [ ] Agent approaches opponent and maintains fighting range
- [ ] Agent shows footwork variety (not repetitive)
- [ ] Agent maintains balance during movement
- [ ] No degenerate strategies exhibited

### Integration and Performance
- [ ] Trained model loads in gameplay scene correctly
- [ ] Inference maintains 60fps in gameplay
- [ ] Agent integrates with existing character systems
- [ ] No errors in console during agent operation
- [ ] Training process documented in TRAINING_LOG.md

### Known Limitations / Technical Debt
Document any issues to address in future phases:
- Movement may be overly cautious (can tune aggression in Phase 2)
- Limited opponent reactivity (no attacks yet to respond to)
- Physics parameters may need fine-tuning for different character models
- Self-play may have converged to local optima (consider curriculum or opponent diversity later)

---

## Next Steps

After Phase 1 completion:
- **[Phase 2: Attack Execution](Phase-2.md)** - Add physics-based punching with force application and weight transfer
- Ensure you have the trained movement model saved before proceeding
- Phase 2 will build on this model using transfer learning

---

## Appendix: Quick Reference

### Key Files Created
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs`
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentObservations.cs`
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentActions.cs`
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentRewards.cs`
- `Assets/Knockout/Scripts/AI/PhysicsControllers/MovementController.cs`
- `Assets/Knockout/Scripts/Training/TrainingEnvironment.cs`
- `Assets/Knockout/Scripts/Training/EpisodeManager.cs`
- `Assets/Knockout/Scenes/Training/TrainingArena.unity`
- `Assets/Knockout/Training/Config/movement_training.yaml`
- `Assets/Knockout/Training/Models/movement_phase1.onnx`
- `docs/ML_AGENTS_SETUP.md`
- `docs/TRAINING_LOG.md`

### Training Command
```bash
mlagents-learn Assets/Knockout/Training/Config/movement_training.yaml --run-id=movement_phase1 --time-scale=20
```

### TensorBoard Command
```bash
tensorboard --logdir=results/movement_phase1
```

### Resume Training Command
```bash
mlagents-learn Assets/Knockout/Training/Config/movement_training.yaml --run-id=movement_phase1 --resume
```

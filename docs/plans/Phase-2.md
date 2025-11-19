# Phase 2: Attack Execution

## Phase Goal

Extend the trained movement agent with physics-based attack execution. The agent will learn to throw jabs, hooks, and uppercuts with realistic weight transfer, force application, and timing. By the end of this phase, agents will engage in dynamic striking exchanges with physically accurate punch mechanics.

**Success Criteria:**
- Attack actions added to PhysicsAgent action space
- Attack observations include opponent vulnerability and attack opportunities
- Physics controllers apply forces during punches for weight transfer
- Attack reward function encourages effective striking
- Agent trains via self-play and learns when/how to attack
- Trained agent lands hits, varies punch types, and shows realistic striking form

**Estimated tokens:** ~85,000

## Prerequisites

- Phase 0 completed (architecture foundation)
- Phase 1 completed (movement agent trained and working)
- `movement_phase1.onnx` model available for transfer learning
- Existing CharacterCombat system understood
- AttackData ScriptableObjects exist for jab, hook, uppercut

---

## Task 1: Expand Observation Space for Attacks

**Goal:** Add observations that help the agent decide when and how to attack.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentObservations.cs` (modify)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (modify)

**Prerequisites:**
- Phase 1 observations working correctly
- Understanding of attack timing and ranges

**Implementation Steps:**

1. **Add attack opportunity observations** (~8 new observations):
   - Opponent vulnerability (boolean): Is opponent in recovery/hit-stun/attacking?
   - Opponent guard state (boolean): Is opponent blocking?
   - Attack range for jab (boolean): Within jab range (shorter)
   - Attack range for hook (boolean): Within hook range (medium)
   - Attack range for uppercut (boolean): Within uppercut range (close)
   - Time since self's last attack (normalized): Prevents spam, encourages timing
   - Opponent's recent movement direction (2D vector): Predict where opponent is moving
   - Relative velocity to opponent (normalized): Closing or separating?

2. **Add attack cooldown observations** (~3 observations):
   - Jab cooldown remaining (normalized to attack duration)
   - Hook cooldown remaining
   - Uppercut cooldown remaining
   - Get from CharacterCombat component

3. **Add opponent attack prediction observations** (~4 observations):
   - Opponent's attack windup detection (boolean): Is opponent starting attack animation?
   - Opponent's attack type being thrown (one-hot 3D: jab/hook/uppercut)
   - Time until opponent attack lands (normalized): Helps with defensive reactions (Phase 4)

4. **Update total observation count**:
   - Previous Phase 1 observations: ~54
   - New attack observations: ~15
   - New total: ~69 observations
   - Update BehaviorParameters Vector Observation Space Size to 69

5. **Integrate new observations in CollectObservations()**:
   - Add new observation section clearly labeled "Attack Opportunity Observations"
   - Maintain observation order consistency
   - Normalize all continuous values properly

**Verification Checklist:**
- [ ] Observation space size updated to ~69 in BehaviorParameters
- [ ] New observations calculated correctly without errors
- [ ] Opponent vulnerability detection working (test in Heuristic mode)
- [ ] Attack range booleans accurate (verify distances)
- [ ] No NaN values in new observations

**Testing Instructions:**
- Unit tests for attack range calculations
- Manual testing: verify attack opportunity observations update correctly as characters move
- Verify observations in TensorBoard during training (check ranges)

**Commit Message Template:**
```
feat(observations): add attack opportunity observations

- Added opponent vulnerability and guard state observations
- Added attack range booleans for jab/hook/uppercut
- Added attack cooldown tracking observations
- Added opponent attack prediction observations
- Updated observation space size to 69 dimensions
- Integrated new observations in CollectObservations
- Added unit tests for attack range detection

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~7,000

---

## Task 2: Expand Action Space for Attacks

**Goal:** Add continuous attack actions to control punch selection, timing, and force.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentActions.cs` (modify)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (modify)

**Prerequisites:**
- Task 1 completed (attack observations added)
- Phase 1 movement actions working

**Implementation Steps:**

1. **Expand action space** from 8 to 15 continuous actions:
   - **Movement actions** (indices 0-7): Unchanged from Phase 1
   - **Attack type** (index 8): Continuous [0, 1] discretized to 4 options:
     - 0.00-0.25: No attack
     - 0.25-0.50: Jab
     - 0.50-0.75: Hook
     - 0.75-1.00: Uppercut
   - **Attack hand** (index 9): Continuous [0, 1] discretized to left (0-0.5) or right (0.5-1.0)
   - **Attack force multiplier** (index 10): Continuous [0.5, 1.5] - intensity of punch
   - **Attack timing offset** (index 11): Continuous [-0.1, 0.1] seconds - early/late attack
   - **Follow-through intensity** (index 12): Continuous [0, 1] - how much to commit to punch
   - **Unused** (indices 13-14): Reserved for future phases

2. **Update BehaviorParameters**:
   - Set Vector Action Space Size to 15

3. **Implement attack action interpretation**:
   - In OnActionReceived, extract attack actions (indices 8-12)
   - Store in private fields for execution in FixedUpdate
   - Discretize attack type using threshold logic
   - Map force multiplier to valid range
   - Clamp all values appropriately

4. **Create attack execution logic**:
   - In FixedUpdate, check if attack action requested
   - Verify attack can execute (not in cooldown, not already attacking)
   - Call CharacterCombat methods:
     - ExecuteJab(isLeftHand, forceMultiplier)
     - ExecuteHook(isLeftHand, forceMultiplier)
     - ExecuteUppercut(isLeftHand, forceMultiplier)
   - May need to extend CharacterCombat to accept force multiplier parameter

5. **Handle attack state**:
   - Track if attack is currently executing
   - Prevent movement during attack (set movement input to zero or reduced)
   - Allow agent to queue next action during attack recovery

6. **Update Heuristic method**:
   - Map keyboard to attack actions:
     - Q/E: Jab left/right
     - 1/2: Hook left/right
     - Z/C: Uppercut left/right
   - Shift held: Increase force multiplier
   - Allows manual testing of attack system

**Verification Checklist:**
- [ ] Action space size updated to 15 in BehaviorParameters
- [ ] Attack type discretization works correctly
- [ ] Attacks execute via CharacterCombat
- [ ] Force multiplier affects attack damage/force
- [ ] Heuristic control allows manual attack testing
- [ ] Attack cooldowns respected

**Testing Instructions:**
- Manual testing in Heuristic mode:
  - Execute each attack type with keyboard
  - Verify attacks trigger animations
  - Verify hits register on opponent
  - Test force multiplier affects hit impact
- Unit tests for action discretization logic

**Commit Message Template:**
```
feat(actions): add attack execution actions

- Expanded action space from 8 to 15 continuous actions
- Added attack type selection (none/jab/hook/uppercut)
- Added attack hand selection (left/right)
- Added attack force multiplier and timing offset
- Added follow-through intensity control
- Implemented attack action interpretation and execution
- Integrated with CharacterCombat system
- Updated Heuristic method with attack key bindings
- Extended CharacterCombat to accept force parameters

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~8,000

---

## Task 3: Implement Attack Physics Controllers

**Goal:** Create physics controllers that apply forces during attacks for realistic weight transfer, punch velocity, and follow-through.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsControllers/AttackController.cs` (new)
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgent.cs` (modify to use controller)

**Prerequisites:**
- Task 2 completed (attack actions added)
- Understanding of punch biomechanics (weight shift, rotation, arm extension)

**Implementation Steps:**

1. **Create AttackController class**:
   - Serialized parameters:
     - Punch force magnitude (base force for different punch types)
     - Weight transfer speed (how fast to shift CoM during punch)
     - Torso rotation amount (body twist during hook/uppercut)
     - Follow-through distance
     - Recovery force (return to neutral stance)

2. **Implement weight transfer during attacks**:
   - Method: `ApplyAttackWeightTransfer(AttackType type, float intensity)`
   - For jab: Quick weight shift forward (CoM moves toward front foot)
   - For hook: Lateral weight shift + torso rotation
   - For uppercut: Dip then rise (lower CoM then raise during punch)
   - Use Rigidbody.centerOfMass to shift weight
   - Apply over duration of attack animation

3. **Implement punch force application**:
   - Method: `ApplyPunchForce(Rigidbody rb, AttackData attack, float multiplier)`
   - Apply forward force to Rigidbody during active frames of attack
   - Force direction based on punch type:
     - Jab: Straight forward
     - Hook: Arc (lateral component)
     - Uppercut: Upward angle
   - Magnitude based on attack damage * force multiplier
   - Add slight forward momentum to character (step into punch)

4. **Implement torso rotation for hooks**:
   - Method: `ApplyTorsoRotation(float angle)`
   - Rotate torso/spine bone (if using humanoid rig) during hook
   - Or apply torque to Rigidbody
   - Adds rotational power to hook punches
   - Reset rotation during recovery

5. **Implement follow-through mechanics**:
   - Method: `ApplyFollowThrough(float intensity)`
   - After hit/miss, arm continues forward briefly
   - Higher intensity = more committed, longer recovery
   - Lower intensity = safer, quicker recovery
   - Affects recovery time before next action

6. **Implement recovery physics**:
   - Method: `ApplyRecoveryForce()`
   - Return weight to center after attack
   - Reset CoM to neutral position
   - Apply restoring force using PD controller
   - Smooth transition back to idle/movement state

7. **Synchronize with animations**:
   - Use animation events or track animation progress
   - Apply physics forces at key moments:
     - Weight shift: Start of attack animation
     - Punch force: Active frames (when hitbox active)
     - Follow-through: After contact/miss
     - Recovery: After attack animation completes
   - Blend physics forces with animation motion

8. **Integrate with PhysicsAgent**:
   - In FixedUpdate, when attack executing:
     - Get attack type and parameters from interpreted actions
     - Call AttackController methods at appropriate times
     - Track attack state (windup, active, follow-through, recovery)

9. **Tune force magnitudes**:
   - Start conservative and increase:
     - Jab force: 300-500N forward
     - Hook force: 400-600N with rotation
     - Uppercut force: 500-800N with upward component
   - Weight shift: 0.3-0.5 units forward
   - Test in Heuristic mode for feel

**Verification Checklist:**
- [ ] AttackController.cs compiles without errors
- [ ] Weight transfer visible during attacks
- [ ] Punch forces applied during active frames
- [ ] Torso rotation visible during hooks
- [ ] Follow-through affects recovery timing
- [ ] Recovery returns character to neutral
- [ ] Physics forces blend smoothly with animations
- [ ] No physics explosions or instability

**Testing Instructions:**
- Manual testing in Heuristic mode:
  - Throw each punch type, observe weight shift
  - Verify visible lean/rotation during hooks
  - Check that character recovers to neutral after attack
  - Test varying force multiplier (shift key in Heuristic)
  - Ensure physics feels natural and responsive

**Commit Message Template:**
```
feat(physics-controllers): implement attack physics controllers

- Created AttackController for physics-based striking
- Implemented weight transfer during attacks (forward shift for jabs, rotation for hooks)
- Applied punch forces to Rigidbody during active frames
- Added torso rotation mechanics for hooks
- Implemented follow-through and recovery physics
- Synchronized physics forces with attack animations
- Integrated AttackController with PhysicsAgent FixedUpdate
- Tuned force magnitudes for realistic punch feel

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 4: Implement Attack Reward Function

**Goal:** Design rewards that encourage effective, varied, and realistic attacking behavior.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentRewards.cs` (modify)

**Prerequisites:**
- Task 3 completed (attack physics working)
- Phase 1 movement rewards understood

**Implementation Steps:**

1. **Add combat effectiveness rewards for attacking**:
   - **Hit Landed Reward** (+1.0 per hit):
     - Positive reward when attack connects with opponent
     - Scale by damage dealt (higher damage = more reward)
     - Bonus for critical hits or knockdowns
   - **Attack Accuracy Reward** (+0.2 per successful hit):
     - Ratio of hits landed to attacks thrown
     - Encourages attacking when high probability of success
   - **Damage Dealt Reward** (+damage/max_health):
     - Reward proportional to health removed
     - Encourages high-value attacks (uppercuts > jabs)
   - **Hit Received Penalty** (-0.5 per hit taken):
     - Negative reward when opponent hits agent
     - Encourages defense (Phase 4) and avoiding hits

2. **Add physical realism rewards for attacks**:
   - **Weight Transfer Reward** (+0.05 per attack):
     - Check if weight shifted correctly for punch type
     - Jab should shift forward, hook should rotate, etc.
     - Reward if CoM motion matches attack type
   - **Attack Timing Reward** (+0.03 per well-timed attack):
     - Reward for attacking when opponent vulnerable
     - Penalize attacking when opponent is blocking or out of range
   - **Follow-Through Reward** (+0.02 per attack):
     - Reward for appropriate follow-through intensity
     - Too much = long recovery (bad)
     - Too little = weak punch (bad)
     - Optimal around 0.6-0.8 intensity

3. **Add strategic depth rewards for attacks**:
   - **Attack Variety Reward** (+0.1 per 5 seconds):
     - Track recent attack types used
     - Reward if multiple types used in window
     - Prevents spamming single attack type
   - **Combo Potential Reward** (+0.2 per combo):
     - Reward for landing multiple hits in quick succession
     - Encourages pressure and aggressive play
   - **Punish Opponent's Mistakes** (+0.3):
     - Extra reward for hitting opponent during their attack recovery
     - Teaches counterattacking concept

4. **Add player entertainment rewards for attacks**:
   - **Close Combat Reward** (+0.1 per frame in striking range):
     - Reward both fighters for engaging in close-range combat
     - Encourages exciting exchanges over passive play
   - **Back-and-Forth Reward** (+0.5):
     - Reward when both fighters land hits in same episode
     - Creates dynamic, competitive fights
   - **Aggressive Play Reward** (+0.05 per attack thrown):
     - Small reward for throwing attacks (even if miss)
     - Prevents overly passive AI
     - Balance with accuracy to avoid mindless spam

5. **Add sparse milestone rewards**:
   - **First Blood** (+2.0): Land first hit of the round
   - **Knockout Bonus** (+20.0): Reduce opponent health to zero
   - **Perfect Round** (+10.0): Win round without taking damage
   - **Comeback Victory** (+5.0): Win from low health (<30%)

6. **Update reward combination logic**:
   - Adjust weights to balance movement and attack rewards:
     - Combat effectiveness: 0.5 (increased from 0.4)
     - Physical realism: 0.2 (same)
     - Strategic depth: 0.2 (same)
     - Player entertainment: 0.1 (decreased from 0.2)
   - Movement is less critical now, combat more important

7. **Handle opponent defeated scenario**:
   - When opponent health reaches zero:
     - Large positive reward to winner (+50.0)
     - Large negative reward to loser (-50.0)
     - End episode for both agents
   - Reset environment for new episode

8. **Add reward debugging**:
   - Log individual reward components for attack behaviors
   - Visualize attack reward contributions separately from movement
   - Track hit accuracy, combo counts, attack variety metrics

**Verification Checklist:**
- [ ] Hit landed rewards trigger correctly
- [ ] Hit received penalties apply properly
- [ ] Attack variety tracking works over time windows
- [ ] Combo detection counts sequential hits
- [ ] Sparse milestone rewards fire at correct times
- [ ] Reward balance feels appropriate (not dominated by one component)
- [ ] Total reward values remain in reasonable range

**Testing Instructions:**
- Manual testing in Heuristic mode:
  - Land hits and observe positive rewards
  - Get hit and observe negative rewards
  - Vary attack types and verify variety reward
  - Land combos and check combo rewards
- Verify reward logging in TensorBoard during training

**Commit Message Template:**
```
feat(rewards): implement attack execution reward function

- Added combat effectiveness rewards (hits landed, damage dealt, accuracy)
- Added physical realism rewards (weight transfer, timing, follow-through)
- Added strategic depth rewards (variety, combos, punishing mistakes)
- Added player entertainment rewards (close combat, back-and-forth)
- Added sparse milestone rewards (first blood, knockout, perfect round)
- Updated reward weight balance to emphasize combat
- Implemented opponent defeated scenario handling
- Added attack-specific reward debugging and logging

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 5: Update Training Configuration for Attacks

**Goal:** Modify training config to account for expanded observation/action space and attack-focused rewards.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Config/attack_training.yaml` (new, based on movement_training.yaml)

**Prerequisites:**
- Task 4 completed (attack rewards implemented)
- Phase 1 training config understood

**Implementation Steps:**

1. **Create attack_training.yaml**:
   - Copy `movement_training.yaml` as base
   - Modify for attack training specifics

2. **Update hyperparameters for increased complexity**:
   - `batch_size: 4096` (increased from 2048 due to more complex behavior)
   - `buffer_size: 40960` (10x batch size)
   - `learning_rate: 2e-4` (slightly decreased for stability)
   - `beta: 8e-3` (increased entropy for attack exploration)
   - `max_steps: 7000000` (more steps for attack learning)

3. **Update network architecture**:
   - `hidden_units: 384` (increased from 256 for larger observation/action space)
   - `num_layers: 3` (added layer for attack decision complexity)

4. **Configure curriculum learning** (optional but recommended):
   - Start with movement-only, gradually enable attacks
   - Lesson 1: Movement only (use Phase 1 model)
   - Lesson 2: Enable attacks, increase combat reward weight
   - Lesson 3: Full combat with all rewards
   - Uses ML-Agents curriculum feature

5. **Update self-play parameters**:
   - `save_steps: 100000` (save opponents more frequently)
   - `play_against_latest_model_ratio: 0.7` (higher, focuses on current skill)

6. **Add reward signals**:
   - Keep extrinsic with gamma: 0.99
   - Optionally add GAIL (imitation) if you have demo recordings of good attacks
   - Curiosity can help with attack exploration

7. **Configure for transfer learning**:
   - Initialize training from Phase 1 model:
     - Use `--initialize-from=movement_phase1` flag
     - Allows agent to keep movement skills while learning attacks
     - Network must have compatible architecture (may need to expand carefully)
   - Alternatively, start from scratch (longer but sometimes more stable)

**Verification Checklist:**
- [ ] attack_training.yaml has valid YAML syntax
- [ ] Hyperparameters updated for attack complexity
- [ ] Network architecture expanded appropriately
- [ ] Self-play config adjusted
- [ ] Transfer learning configured (if using)

**Testing Instructions:**
- Validate config: `mlagents-learn <config> --help`
- Ensure no errors before starting training

**Commit Message Template:**
```
feat(training): create attack training configuration

- Created attack_training.yaml based on movement config
- Increased batch size and buffer size for complexity
- Expanded network to 3 layers with 384 hidden units
- Configured transfer learning from Phase 1 model
- Increased max training steps to 7M
- Adjusted entropy and learning rate for attack exploration
- Updated self-play parameters for faster opponent updates

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~7,000

---

## Task 6: Train Attack Agent and Evaluate

**Goal:** Execute training with attack capabilities and evaluate the resulting striking behavior.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Models/attack_phase2.onnx` (trained model)
- `docs/TRAINING_LOG.md` (update with Phase 2 results)

**Prerequisites:**
- Tasks 1-5 completed (full attack system ready)
- Phase 1 model available for transfer learning

**Implementation Steps:**

1. **Prepare for training**:
   - Verify all components updated (observations, actions, rewards, physics controllers)
   - Test in Heuristic mode that attacks work correctly
   - Save TrainingArena scene

2. **Start training with transfer learning**:
   ```bash
   mlagents-learn Assets/Knockout/Training/Config/attack_training.yaml --run-id=attack_phase2 --initialize-from=movement_phase1 --time-scale=20
   ```
   - If initialization fails (architecture mismatch), train from scratch:
   ```bash
   mlagents-learn Assets/Knockout/Training/Config/attack_training.yaml --run-id=attack_phase2 --time-scale=20
   ```

3. **Monitor training closely**:
   - Watch TensorBoard for:
     - Cumulative reward should increase
     - Hit accuracy metric (custom scalar if logged)
     - Damage dealt increasing over time
     - Attack variety (track via custom scalars)
   - Early training (first 500k steps):
     - Agents may swing wildly, missing often
     - Reward will be volatile as agents explore
     - Look for ANY hits landing as progress sign

4. **Mid-training checkpoints** (every 1M steps):
   - Load checkpoint and observe:
     - Do agents hit each other with attacks?
     - Is attack timing improving (hitting vulnerable opponents)?
     - Are multiple attack types used?
     - Is movement still functional (not degraded)?
   - If movement degraded catastrophically, consider:
     - Freezing movement policy layers (advanced)
     - Increasing movement reward weights
     - Starting from scratch instead of transfer learning

5. **Training duration**:
   - Minimum: 3M steps (~2-3 hours)
   - Recommended: 5-7M steps (~4-6 hours)
   - Attacks are harder to learn than movement, expect longer training
   - Stop when:
     - Hit accuracy plateaus at reasonable level (>30%)
     - Agents consistently land combos
     - Attack variety is demonstrated

6. **Evaluate trained model**:
   - Quantitative metrics:
     - Final cumulative reward
     - Hit accuracy percentage
     - Average damage per episode
     - Knockout rate
   - Qualitative evaluation:
     - [ ] Agents throw punches during fights
     - [ ] Attacks land on opponent (not just swinging wildly)
     - [ ] Multiple attack types used (jabs, hooks, uppercuts)
     - [ ] Weight transfer visible during punches
     - [ ] Agents attack when opponent vulnerable
     - [ ] Movement still natural (not degraded from Phase 1)
     - [ ] Close-range combat occurs frequently
     - [ ] Fights look dynamic and engaging

7. **Compare to Phase 1**:
   - Agent should retain movement skills from Phase 1
   - Plus now actively attack opponent
   - Ideal: seamless integration of movement + attacks

8. **Export and integrate model**:
   - Copy final model to `Assets/Knockout/Training/Models/attack_phase2.onnx`
   - Test in gameplay scene
   - Verify performance acceptable

9. **Document results**:
   - Update `docs/TRAINING_LOG.md` with Phase 2 details
   - Include metrics, observations, issues encountered
   - Note any hyperparameter adjustments made

10. **Troubleshooting**:
    - **Agents don't attack**:
      - Increase attack reward magnitudes
      - Decrease movement rewards (may be exploiting movement-only)
      - Reduce attack execution cost (make attacking easier)
    - **Agents spam attacks wildly**:
      - Increase attack accuracy rewards
      - Add larger penalty for missed attacks
      - Increase attack variety rewards
    - **Movement degraded**:
      - Use transfer learning from Phase 1
      - Increase movement reward weights
      - Consider freezing movement layers
    - **Training unstable**:
      - Decrease learning rate
      - Decrease batch size
      - Check for NaN in observations/rewards

**Verification Checklist:**
- [ ] Training completes without errors
- [ ] Cumulative reward increases over training
- [ ] Agents demonstrate attacking behavior
- [ ] Hit accuracy improves over training
- [ ] Multiple attack types observed
- [ ] Movement quality maintained from Phase 1
- [ ] Model performs well in gameplay scene
- [ ] Training results documented

**Testing Instructions:**
- Load trained model and run 20+ episodes
- Manually observe and rate:
  - Hit accuracy (% of attacks that land)
  - Attack variety (number of different attacks per episode)
  - Movement quality compared to Phase 1
  - Overall fight entertainment value

**Commit Message Template:**
```
feat(training): train attack execution agent via self-play

- Executed training with transfer learning from Phase 1
- Trained for 5M steps over 5 hours
- Achieved final cumulative reward of X.X
- Agents demonstrate varied attack execution (jabs, hooks, uppercuts)
- Hit accuracy improved to ~35% by end of training
- Agents exhibit weight transfer and physics-based striking
- Movement quality maintained from Phase 1
- Exported attack_phase2.onnx model
- Documented training process and results in TRAINING_LOG.md

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~12,000

---

## Phase 2 Verification

Complete checklist before proceeding to Phase 3:

### Observations
- [ ] Attack opportunity observations added (~15 new observations)
- [ ] Total observation space ~69 dimensions
- [ ] Attack range detection accurate
- [ ] Opponent vulnerability detection working

### Actions
- [ ] Attack actions added to action space (total 15 actions)
- [ ] Attack type discretization working correctly
- [ ] Force multiplier and timing parameters functional
- [ ] Attacks execute via CharacterCombat system

### Physics Controllers
- [ ] AttackController implemented for strike physics
- [ ] Weight transfer visible during attacks
- [ ] Punch forces applied during active frames
- [ ] Follow-through and recovery mechanics working
- [ ] Physics-enhanced attacks blend with animations

### Reward Function
- [ ] Combat effectiveness rewards for hits/damage
- [ ] Physical realism rewards for technique
- [ ] Strategic rewards for variety and combos
- [ ] Sparse milestone rewards for knockouts
- [ ] Reward balance appropriate

### Training Results
- [ ] Training completed for at least 3M steps
- [ ] Agents attack opponents during fights
- [ ] Hit accuracy >25% by training end
- [ ] Multiple attack types demonstrated
- [ ] Movement quality maintained
- [ ] Model exported as attack_phase2.onnx

### Known Limitations
- Agents may favor certain attack types over others
- Hit accuracy still below human-level (30-40% is good for RL)
- No defensive reactions yet (agents take hits without blocking)
- Weight transfer may be exaggerated or subtle (needs tuning)

---

## Next Steps

After Phase 2 completion:
- **[Phase 3: Hit Reactions & Balance](Phase-3.md)** - Add physics-based hit reactions, stumbling, and recovery
- Ensure attack_phase2.onnx model saved before proceeding

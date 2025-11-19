# Phase 3: Hit Reactions & Balance Recovery

## Unity 6.0 Compatibility Note

**This phase assumes Unity 6.0 upgrade is complete.** Physics-based hit reactions and balance recovery use Unity 6 physics engine. Force calculations and Rigidbody behavior are tuned for Unity 6. See `UNITY_6_UPGRADE_IMPACT.md` for details.

## Phase Goal

Implement physics-based hit reactions, balance control, and recovery systems. The agent will learn to respond realistically to incoming hits, stumble authentically based on force and location, maintain balance through center-of-mass control, and recover gracefully to continue fighting. This creates dramatic, physically believable combat exchanges.

**Success Criteria:**
- Hit reaction observations track balance state and impact forces
- Balance recovery actions allow agent to regain stability
- Physics controllers simulate stumbling, reeling, and knockdowns
- Reward function encourages maintaining balance and quick recovery
- Agent trains to recover from hits and continue fighting
- Trained agent exhibits realistic hit reactions and balance behavior

**Estimated tokens:** ~90,000

## Prerequisites

- Phase 0 completed (architecture foundation)
- Phase 1 completed (movement trained)
- Phase 2 completed (attack execution trained)
- `attack_phase2.onnx` model available
- Understanding of character balance and physics

---

## Task 1: Add Balance and Hit Reaction Observations

**Goal:** Provide agent with information about balance state, incoming forces, and recovery status.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentObservations.cs` (modify)

**Prerequisites:**
- Phase 2 observations working (69 dimensions currently)

**Implementation Steps:**

1. **Add balance state observations** (~8 observations):
   - Center of mass offset from support base (2D x,z normalized)
   - Balance stability metric (0-1, how close to tipping)
   - Current tilt angle from vertical (normalized)
   - Angular velocity around balance point (normalized)
   - Ground contact status per foot (2 booleans)
   - Is currently stumbling/staggering (boolean)
   - Time since last hit received (normalized, capped at 5 seconds)

2. **Add hit impact observations** (~6 observations):
   - Last hit impact force magnitude (normalized)
   - Last hit direction (2D vector x,z)
   - Last hit height (normalized: body/head)
   - Is currently in hit-stun state (boolean)
   - Hit-stun time remaining (normalized)

3. **Add recovery status observations** (~4 observations):
   - Recovery progress (0-1, from hit-stun to normal)
   - Current animation state (one-hot: normal/hit-stunned/stumbling/knocked-down)
   - Distance fallen from impact (normalized, if stumbling back)
   - Can perform recovery action (boolean)

4. **Update total observation count**:
   - Previous: ~69
   - New balance observations: ~18
   - New total: ~87 observations
   - Update BehaviorParameters Vector Observation Space Size to 87

5. **Implement observation collection**:
   - Calculate center of mass projection onto ground plane
   - Determine support polygon from foot positions (use Physics.Raycast)
   - Calculate stability metric (distance from CoM to polygon edge)
   - Get last hit data from CharacterHealth or CharacterCombat component
   - Track hit-stun duration and recovery progress

6. **Handle knockdown observations**:
   - If character knocked down, add specific observations:
     - Ground contact (full body boolean)
     - Orientation (angle from upright)
     - Time knocked down (normalized)

**Verification Checklist:**
- [ ] Observation space updated to ~87 dimensions
- [ ] Balance stability calculates correctly
- [ ] Hit impact data captured accurately
- [ ] Recovery progress tracks over time
- [ ] No NaN values in new observations

**Testing Instructions:**
- Unit tests for CoM and support polygon calculations
- Manual testing: get hit and verify hit impact observations update
- Check balance stability metric as character leans

**Commit Message Template:**
```
feat(observations): add balance and hit reaction observations

- Added balance state observations (CoM, stability, tilt, contact)
- Added hit impact observations (force, direction, height, stun)
- Added recovery status observations (progress, state, actions)
- Implemented center of mass and support polygon calculations
- Updated observation space to 87 dimensions
- Added unit tests for balance calculations

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~8,000

---

## Task 2: Add Balance Recovery Actions

**Goal:** Expand action space to allow agent to control balance recovery movements.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentActions.cs` (modify)

**Prerequisites:**
- Task 1 completed (balance observations added)
- Phase 2 actions working (15 dimensions)

**Implementation Steps:**

1. **Expand action space** from 15 to 20 continuous actions:
   - Movement actions (0-7): Unchanged
   - Attack actions (8-12): Unchanged
   - **Recovery step direction** (index 13): Continuous [-1, 1] for lateral step to regain balance
   - **Recovery step distance** (index 14): Continuous [0, 1] for step magnitude
   - **Brace action** (index 15): Continuous [0, 1] threshold for bracing against impact
   - **Counter-rotation** (index 16): Continuous [-1, 1] for torso twist to counter spin
   - **Crouch for stability** (index 17): Continuous [0, 1] for lowering CoM
   - **Unused** (indices 18-19): Reserved for Phase 4 (defense)

2. **Update BehaviorParameters**: Set Vector Action Space Size to 20

3. **Interpret recovery actions in OnActionReceived**:
   - Extract recovery actions (indices 13-17)
   - Store in fields for FixedUpdate execution
   - Clamp values to valid ranges

4. **Implement recovery action execution logic**:
   - **Recovery step**: If unstable, take quick step in specified direction
     - Apply lateral force to Rigidbody
     - Animate foot placement (IK or animation trigger)
   - **Brace action**: Stiffen joints/increase drag when hit imminent
     - Increase Rigidbody drag temporarily
     - Widen stance for stability
   - **Counter-rotation**: Apply torque opposite to spin from hit
     - Counteract angular velocity from impact
   - **Crouch**: Lower center of mass for stability
     - Reduce CoM height (already implemented in Phase 1, reuse)

5. **Enable recovery actions only in appropriate states**:
   - Recovery actions only execute when character in hit-stun or stumbling
   - Disable during normal combat (use movement/attack instead)
   - Check combat state before executing

6. **Update Heuristic for testing**:
   - Map keys to recovery actions:
     - Arrow keys: Recovery step direction
     - Space: Brace
     - Ctrl: Crouch for stability
   - Allows manual testing of recovery system

**Verification Checklist:**
- [ ] Action space updated to 20 dimensions
- [ ] Recovery actions interpreted correctly
- [ ] Recovery step applies lateral force
- [ ] Brace increases stability when hit
- [ ] Counter-rotation reduces spin
- [ ] Actions only execute in appropriate states
- [ ] Heuristic control works for recovery testing

**Testing Instructions:**
- Manual testing in Heuristic mode:
  - Get hit and use recovery keys
  - Verify recovery step prevents falling
  - Test brace reduces knockback
  - Test counter-rotation stops spinning
- Verify recovery actions don't interfere with normal combat

**Commit Message Template:**
```
feat(actions): add balance recovery actions

- Expanded action space from 15 to 20 continuous actions
- Added recovery step direction and distance actions
- Added brace action for impact absorption
- Added counter-rotation action for spin recovery
- Added crouch action for stability
- Implemented recovery action execution in FixedUpdate
- Enabled recovery actions only during hit-stun/stumbling
- Updated Heuristic with recovery key mappings

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~8,000

---

## Task 3: Implement Balance Physics Controllers

**Goal:** Create physics systems for realistic stumbling, balance recovery, and knockdown mechanics.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsControllers/BalanceController.cs` (new)
- `Assets/Knockout/Scripts/Combat/HitReactionSystem.cs` (new or modify existing)

**Prerequisites:**
- Task 2 completed (recovery actions added)
- Understanding of balance physics and PD controllers

**Implementation Steps:**

1. **Create BalanceController class**:
   - Serialized parameters:
     - Balance threshold (how far CoM can be from support before unstable)
     - Recovery force magnitude
     - Stumble duration curve
     - Knockdown force threshold
     - Brace effectiveness multiplier

2. **Implement balance monitoring**:
   - Method: `CheckBalanceStability()` → BalanceState enum
   - Calculate center of mass projection onto ground
   - Determine support polygon from foot contacts
   - Return state: Balanced, Unstable, Stumbling, FallingDown
   - Use Physics.Raycast to detect foot ground contact

3. **Implement hit reaction force application**:
   - Method: `ApplyHitReactionForce(Vector3 hitForce, Vector3 hitPoint, bool bracing)`
   - When hit received (listen to CharacterHealth.OnDamageReceived event):
     - Apply force to Rigidbody at hit location
     - If bracing: reduce force magnitude by brace effectiveness
     - Calculate resulting CoM offset from impact
     - Determine if knockdown threshold exceeded
     - Trigger stumble or knockdown state

4. **Implement stumbling mechanics**:
   - Method: `SimulateStumble(Vector3 impactDirection, float magnitude)`
   - Apply backwards momentum in hit direction
   - Reduce movement control (multiply movement input by 0.3)
   - Gradually restore balance using PD controller
   - Play stumble animation (backward step, catch balance)
   - Duration based on hit force and agent's recovery actions

5. **Implement balance recovery PD controller**:
   - Method: `ApplyBalanceRecoveryForce(Vector3 targetCoM, float recoveryIntensity)`
   - Calculate error: current CoM vs target (center of support polygon)
   - Calculate derivative: CoM velocity
   - Apply corrective force: `F = Kp * error + Kd * derivative`
   - Tune gains: Kp=200, Kd=20 (adjust experimentally)
   - Agent's recovery actions modify target CoM

6. **Implement recovery step execution**:
   - Method: `ExecuteRecoveryStep(Vector2 stepDirection, float stepDistance)`
   - Apply impulse force in step direction
   - Expand support polygon by moving foot
   - Use IK to place foot at target position
   - Immediately stabilize after step completes

7. **Implement brace mechanics**:
   - Method: `ApplyBraceEffect(float intensity)`
   - Increase Rigidbody drag temporarily (simulate tensing muscles)
   - Widen stance slightly (more stable base)
   - Reduce hit force impact by brace effectiveness (e.g., 50% reduction at full brace)

8. **Implement counter-rotation**:
   - Method: `ApplyCounterRotation(float intensity)`
   - Measure current angular velocity
   - Apply torque in opposite direction scaled by intensity
   - Helps recover from spinning hits (hooks)

9. **Implement knockdown physics**:
   - Method: `TriggerKnockdown(Vector3 force)`
   - If hit force exceeds threshold and balance critical:
     - Disable balance controller temporarily
     - Let physics simulate fall (ragdoll-lite)
     - Play knockdown animation
     - After delay, allow recovery attempt
   - Recovery: gradually restore kinematic control, stand up

10. **Integrate with PhysicsAgent**:
    - In FixedUpdate:
      - Run CheckBalanceStability()
      - If unstable/stumbling, apply balance recovery forces
      - Execute recovery actions from agent (step, brace, counter-rotate)
    - Listen to damage events and apply hit reaction forces

11. **Tune parameters**:
    - Balance threshold: CoM must be within 0.8x support polygon radius
    - Stumble duration: 0.5-2.0 seconds based on hit force
    - Knockdown threshold: 1500N impact force
    - Recovery force: 800-1200N
    - Brace reduction: 0.4-0.6 (40-60% damage reduction)

**Verification Checklist:**
- [ ] BalanceController compiles without errors
- [ ] Balance stability monitoring works
- [ ] Hit reactions apply forces realistically
- [ ] Stumbling occurs when hit hard
- [ ] Balance recovery brings character back to stable
- [ ] Recovery step prevents falls
- [ ] Brace reduces knockback when hit
- [ ] Counter-rotation stops spinning
- [ ] Knockdowns occur at appropriate force levels
- [ ] Physics feels natural and responsive

**Testing Instructions:**
- Manual testing with Heuristic control:
  - Get hit and observe stumble behavior
  - Use recovery actions to regain balance
  - Test brace before getting hit (should reduce stumble)
  - Get hit by strong attack and verify knockdown
  - Test recovery from knockdown
- Verify balance controller doesn't interfere with normal movement

**Commit Message Template:**
```
feat(physics-controllers): implement balance and hit reaction physics

- Created BalanceController for physics-based balance management
- Implemented balance stability monitoring with CoM calculations
- Applied hit reaction forces based on impact location and force
- Simulated realistic stumbling with backward momentum
- Implemented PD controller for balance recovery
- Added recovery step execution with IK foot placement
- Implemented brace mechanics to reduce hit impact
- Added counter-rotation to recover from spinning hits
- Implemented knockdown physics for high-force impacts
- Tuned balance parameters for realistic behavior

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~12,000

---

## Task 4: Implement Balance and Recovery Reward Function

**Goal:** Design rewards that encourage maintaining balance, recovering quickly from hits, and continuing to fight.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentRewards.cs` (modify)

**Prerequisites:**
- Task 3 completed (balance physics working)

**Implementation Steps:**

1. **Add balance maintenance rewards**:
   - **Balance Stability Reward** (+0.05 per frame):
     - Reward for maintaining CoM within support polygon
     - Higher reward for more stable balance (closer to center)
     - Encourages proactive balance control
   - **Avoid Stumble Reward** (+0.5 per hit absorbed without stumbling):
     - Reward for taking hit but staying upright via bracing
     - Encourages defensive balance techniques
   - **Grounding Reward** (+0.02 per frame):
     - Reward for both feet on ground
     - Already in Phase 1, but now more critical during hit recovery

2. **Add recovery performance rewards**:
   - **Quick Recovery Reward** (+2.0 inversely proportional to recovery time):
     - Reward for recovering balance quickly after hit
     - Faster recovery = higher reward
     - Formula: `2.0 * (1.0 - time_to_recover / max_recovery_time)`
   - **Recovery Action Effectiveness** (+0.3 per successful recovery step):
     - Reward if recovery step prevented fall
     - Measure stability before and after step
   - **Resilience Reward** (+1.0 for continuing to fight after hit):
     - Reward for attacking or moving purposefully within 2 seconds of being hit
     - Encourages "shaking off" hits and re-engaging

3. **Add physical realism for hit reactions**:
   - **Natural Stumble Reward** (+0.1 per hit reaction):
     - Reward if stumble direction matches hit direction (physics realism)
     - Penalize unnatural reactions (stumbling toward hit)
   - **Appropriate Brace Timing** (+0.2 per well-timed brace):
     - Reward for bracing just before hit lands
     - Requires prediction of incoming attack
   - **Counter-Rotation Usage** (+0.1 per spin recovery):
     - Reward for using counter-rotation when spinning from hook

4. **Add strategic fighting rewards**:
   - **Fight Through Adversity** (+0.5):
     - Reward for winning round despite being knocked down
     - Encourages not giving up
   - **Capitalize on Opponent's Stumble** (+0.3):
     - Reward for attacking opponent who is stumbling/recovering
     - Teaches to press advantage
   - **Defensive Recovery** (+0.2 per defensive action during recovery):
     - Reward for blocking or evading during recovery period
     - Phase 4 will add blocking, placeholder for now

5. **Add penalties for poor balance**:
   - **Falling Penalty** (-1.0 per fall/knockdown):
     - Negative reward for losing balance completely
     - Encourages balance maintenance
   - **Extended Recovery Penalty** (-0.05 per frame stumbling beyond threshold):
     - Penalize prolonged recovery times
     - Encourages efficient recovery actions
   - **Failed Brace Penalty** (-0.1 if bracing but still stumble significantly):
     - Penalize ineffective brace timing

6. **Add sparse milestone rewards**:
   - **Iron Chin Bonus** (+5.0): Take 5+ hits in round without knockdown
   - **Rocky Comeback** (+10.0): Win round after being knocked down
   - **Unshakeable** (+3.0): Win round without stumbling

7. **Update reward weight balance**:
   - Combat effectiveness: 0.4
   - Physical realism: 0.25 (increased to emphasize realistic reactions)
   - Strategic depth: 0.2
   - Player entertainment: 0.15

8. **Handle knockdown scenario**:
   - During knockdown, minimal rewards (only recovery rewards)
   - Large penalty for staying down too long
   - Large reward for successful stand-up

9. **Add reward debugging for balance**:
   - Log balance stability metric
   - Track stumble duration statistics
   - Monitor recovery effectiveness
   - Visualize balance rewards separately

**Verification Checklist:**
- [ ] Balance stability reward calculated correctly
- [ ] Recovery time tracked and rewarded
- [ ] Stumble direction realism checked
- [ ] Brace timing detection works
- [ ] Knockdown penalties applied
- [ ] Recovery action effectiveness measured
- [ ] Milestone rewards trigger appropriately

**Testing Instructions:**
- Manual Heuristic testing:
  - Maintain balance and observe rewards
  - Get hit, recover quickly, check recovery rewards
  - Use brace effectively and verify timing rewards
  - Get knocked down and observe penalties
- Verify reward balance doesn't dominate other components

**Commit Message Template:**
```
feat(rewards): implement balance and recovery reward function

- Added balance maintenance rewards (stability, avoid stumble, grounding)
- Added recovery performance rewards (quick recovery, action effectiveness)
- Added physical realism rewards (natural stumble, brace timing, counter-rotation)
- Added strategic fighting rewards (capitalize on stumbles, fight through adversity)
- Added penalties for poor balance and extended recovery
- Added sparse milestone rewards (iron chin, rocky comeback)
- Updated reward weights to emphasize physical realism
- Implemented knockdown scenario reward handling
- Added balance-specific reward debugging and logging

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 5: Create Balance Training Configuration

**Goal:** Configure ML-Agents training for balance and hit reaction learning.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Config/balance_training.yaml` (new)

**Prerequisites:**
- Task 4 completed (balance rewards implemented)

**Implementation Steps:**

1. **Create balance_training.yaml** based on attack_training.yaml

2. **Update hyperparameters**:
   - `batch_size: 4096` (maintain complexity handling)
   - `buffer_size: 40960`
   - `learning_rate: 2e-4`
   - `beta: 6e-3` (slightly lower than attack training, less exploration needed)
   - `max_steps: 6000000`

3. **Network architecture**:
   - `hidden_units: 384` (maintain from Phase 2)
   - `num_layers: 3`

4. **Configure transfer learning** from Phase 2:
   - Use `--initialize-from=attack_phase2`
   - Agent keeps movement and attack skills while learning balance

5. **Self-play settings**:
   - `save_steps: 100000`
   - `play_against_latest_model_ratio: 0.6`

6. **Add curriculum for balance difficulty** (optional):
   - Lesson 1: Light hits (low force), easy to recover
   - Lesson 2: Medium hits, require recovery actions
   - Lesson 3: Full force hits, knockdowns possible
   - Use ML-Agents curriculum parameter feature

**Verification Checklist:**
- [ ] balance_training.yaml valid syntax
- [ ] Transfer learning configured
- [ ] Hyperparameters appropriate
- [ ] Curriculum defined (if using)

**Testing Instructions:**
- Validate config before training

**Commit Message Template:**
```
feat(training): create balance training configuration

- Created balance_training.yaml based on attack config
- Configured transfer learning from Phase 2 model
- Maintained network architecture for consistency
- Set max steps to 6M for balance learning
- Configured curriculum for progressive difficulty
- Tuned entropy for balance exploration

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~7,000

---

## Task 6: Train Balance Agent and Evaluate

**Goal:** Execute training and evaluate balance recovery behavior.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Models/balance_phase3.onnx`
- `docs/TRAINING_LOG.md` (update)

**Prerequisites:**
- Tasks 1-5 completed
- Phase 2 model available

**Implementation Steps:**

1. **Start training**:
   ```bash
   mlagents-learn Assets/Knockout/Training/Config/balance_training.yaml --run-id=balance_phase3 --initialize-from=attack_phase2 --time-scale=20
   ```

2. **Monitor balance-specific metrics**:
   - Stumble frequency (should decrease)
   - Knockdown rate (should decrease)
   - Average recovery time (should decrease)
   - Balance stability metric (should increase)

3. **Training duration**: 4-6M steps (~3-5 hours)

4. **Evaluate trained model**:
   - Quantitative:
     - Knockdown rate compared to Phase 2
     - Average time to recover balance
     - Percentage of hits absorbed without stumbling
   - Qualitative:
     - [ ] Agents stumble realistically when hit
     - [ ] Stumble direction matches hit direction
     - [ ] Agents use recovery actions to regain balance
     - [ ] Brace reduces knockback visibly
     - [ ] Quick recovery allows continued fighting
     - [ ] Knockdowns occur only on very hard hits
     - [ ] Agents stand up from knockdowns
     - [ ] Balance maintained during normal movement (Phase 1 quality retained)
     - [ ] Attack execution maintained (Phase 2 quality retained)

5. **Compare to Phase 2**:
   - Should retain attack and movement capabilities
   - Plus now recover from hits effectively

6. **Export model** as `balance_phase3.onnx`

7. **Document results** in `docs/TRAINING_LOG.md`

8. **Troubleshooting**:
   - **Agents ignore recovery actions**: Increase recovery rewards
   - **Too many knockdowns**: Lower knockdown force threshold
   - **Unrealistic stumbles**: Adjust hit reaction force application
   - **Skills degraded**: Use transfer learning, increase reward weights for prior skills

**Verification Checklist:**
- [ ] Training completes successfully
- [ ] Stumble behavior looks realistic
- [ ] Recovery actions functional
- [ ] Knockdown rate reasonable (<20% of hits)
- [ ] Previous skills (movement, attacks) maintained
- [ ] Model exported and tested

**Commit Message Template:**
```
feat(training): train balance recovery agent

- Trained for 5M steps with transfer learning from Phase 2
- Agents demonstrate realistic hit reactions and stumbling
- Recovery actions reduce knockdown rate by 40%
- Balance stability improved significantly
- Movement and attack quality maintained from previous phases
- Exported balance_phase3.onnx model
- Documented training in TRAINING_LOG.md

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Phase 3 Verification

Complete before proceeding to Phase 4:

### Observations
- [ ] Balance state observations added (~18 new)
- [ ] Total observation space ~87 dimensions
- [ ] Hit impact data captured correctly
- [ ] Recovery status tracked

### Actions
- [ ] Balance recovery actions added (total 20 actions)
- [ ] Recovery step, brace, counter-rotation functional
- [ ] Actions execute only in appropriate states

### Physics Controllers
- [ ] BalanceController implemented
- [ ] Hit reactions apply forces realistically
- [ ] Stumbling simulated naturally
- [ ] Balance recovery PD controller working
- [ ] Brace reduces impact effectively
- [ ] Knockdowns occur at appropriate thresholds

### Reward Function
- [ ] Balance maintenance rewards
- [ ] Recovery performance rewards
- [ ] Physical realism rewards for reactions
- [ ] Strategic fighting rewards
- [ ] Penalties for poor balance

### Training Results
- [ ] Training completed for at least 4M steps
- [ ] Knockdown rate decreased from Phase 2
- [ ] Recovery time improved
- [ ] Hit reactions look realistic
- [ ] Previous skills maintained
- [ ] Model exported as balance_phase3.onnx

### Known Limitations
- Recovery may be slower than desired (tune PD controller)
- Some knockdowns may look unnatural (physics tuning)
- Agents may "tank" hits rather than evade (Phase 4 will add defense)

---

## Next Steps

After Phase 3 completion:
- **[Phase 4: Defensive Positioning](Phase-4.md)** - Add blocking, dodging, and defensive movement
- Ensure balance_phase3.onnx saved before proceeding

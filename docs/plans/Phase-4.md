# Phase 4: Defensive Positioning & Blocking

## Unity 6.0 Compatibility Note

**This phase assumes Unity 6.0 upgrade is complete.** Defensive systems integrate with Unity 6 physics and character controllers. Input System 1.8.x+ APIs used for control integration. See `UNITY_6_UPGRADE_IMPACT.md` for details.

## Phase Goal

Implement defensive behaviors including blocking, evasive movement, and defensive positioning. The agent will learn to recognize incoming attacks, activate blocking at appropriate times, use defensive footwork to avoid hits, and maintain a defensive fighting stance. This completes the core combat loop: movement, attacking, hit recovery, and defense.

**Success Criteria:**
- Defensive observations detect incoming threats
- Blocking and evasion actions added to action space
- Physics controllers handle blocking stance and defensive movement
- Reward function encourages effective defense and damage mitigation
- Agent learns to block attacks and evade strategically
- Trained agent demonstrates defensive skills alongside offensive abilities

**Estimated tokens:** ~85,000

## Prerequisites

- Phase 0 completed
- Phase 1 completed (movement)
- Phase 2 completed (attacks)
- Phase 3 completed (balance/recovery)
- `balance_phase3.onnx` model available
- Existing blocking system in CharacterCombat understood

---

## Task 1: Add Defensive Observations

**Goal:** Provide agent with information to recognize attack threats and evaluate defensive opportunities.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentObservations.cs` (modify)

**Prerequisites:**
- Phase 3 observations working (87 dimensions)

**Implementation Steps:**

1. **Add incoming attack detection observations** (~8 observations):
   - Opponent attack windup detected (boolean)
   - Opponent attack type (one-hot 3D: jab/hook/uppercut)
   - Time until opponent attack lands (normalized 0-1, based on animation frame)
   - Attack trajectory intersects self position (boolean, ray cast prediction)
   - Attack expected impact height (normalized: body/head)
   - Attack expected impact force (normalized)
   - Can block in time (boolean, based on block startup time vs attack time)

2. **Add defensive state observations** (~5 observations):
   - Is currently blocking (boolean)
   - Block stamina/endurance (normalized 0-1, if implementing block stamina system)
   - Current guard position (normalized: low/mid/high)
   - Time in defensive stance (normalized, capped at 3 seconds)
   - Consecutive blocks count (normalized, tracks blocking streak)

3. **Add positioning safety observations** (~4 observations):
   - Distance to safe zone (away from opponent range, normalized)
   - Is in opponent's preferred attack range (boolean, based on opponent's recent attack patterns)
   - Escape route availability (boolean, can back away without hitting arena boundary)
   - Cornered status (boolean, near arena boundary with limited movement)

4. **Update total observation count**:
   - Previous: ~87
   - New defensive observations: ~17
   - New total: ~104 observations
   - Update BehaviorParameters Vector Observation Space Size to 104

5. **Implement attack prediction logic**:
   - Detect opponent animation state (windup)
   - Estimate time to impact based on animation progress
   - Ray cast from opponent's attacking limb to predict hit location
   - Compare timing with agent's block activation time

6. **Implement positioning safety calculations**:
   - Determine safe distance (outside opponent's attack range)
   - Check proximity to arena boundaries
   - Evaluate escape routes (can move away without cornering self)

**Verification Checklist:**
- [ ] Observation space updated to ~104 dimensions
- [ ] Attack prediction detects opponent attacks accurately
- [ ] Time-to-impact estimates reasonable
- [ ] Blocking state observations track correctly
- [ ] Positioning safety calculated properly
- [ ] No NaN values

**Testing Instructions:**
- Manual testing: have opponent attack and verify observations update
- Check attack prediction timing accuracy
- Verify cornered detection at arena edges

**Commit Message Template:**
```
feat(observations): add defensive and threat detection observations

- Added incoming attack detection observations
- Implemented attack prediction with time-to-impact estimation
- Added defensive state observations (blocking, guard, stamina)
- Added positioning safety observations (escape routes, cornered status)
- Updated observation space to 104 dimensions
- Implemented ray cast for attack trajectory prediction

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~8,000

---

## Task 2: Add Defensive Actions

**Goal:** Expand action space to include blocking activation, guard positioning, and evasive movements.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentActions.cs` (modify)

**Prerequisites:**
- Task 1 completed (defensive observations added)
- Phase 3 actions working (20 dimensions)

**Implementation Steps:**

1. **Utilize reserved action indices** from Phase 3 (indices 18-19) and expand:
   - Movement actions (0-7): Unchanged
   - Attack actions (8-12): Unchanged
   - Recovery actions (13-17): Unchanged
   - **Block activation** (index 18): Continuous [0, 1], threshold at 0.5 to activate
   - **Guard height** (index 19): Continuous [0, 1], maps to low/mid/high guard
   - **Evasion direction** (index 20): Continuous [-1, 1], lean/slip direction
   - **Evasion intensity** (index 21): Continuous [0, 1], how much to evade
   - **Defensive footwork** (index 22): Continuous [-1, 1], defensive circling direction
   - **Total: 23 continuous actions**

2. **Update BehaviorParameters**: Set Vector Action Space Size to 23

3. **Interpret defensive actions in OnActionReceived**:
   - Extract defensive actions (indices 18-22)
   - Store in fields for execution in FixedUpdate
   - Apply thresholding and mapping as needed

4. **Implement blocking execution**:
   - If block activation > 0.5 threshold:
     - Call CharacterCombat.StartBlocking()
     - Set guard height (low/mid/high) based on guard height action
     - Maintain block until action falls below threshold
   - Else if currently blocking:
     - Call CharacterCombat.StopBlocking()

5. **Implement evasion execution**:
   - If evasion intensity > threshold (e.g., 0.3):
     - Apply evasion: lean upper body in evasion direction
     - Use Rigidbody rotation or IK to bend at waist
     - "Slip" punches by moving head position
     - Reduce during frames around opponent attack
   - Subtle movements, not large dodges (more like head movement)

6. **Implement defensive footwork**:
   - Use defensive footwork action for circling while in defensive stance
   - Combines with base movement but prioritizes safety
   - Keeps opponent at optimal defensive distance

7. **Coordinate blocking with other actions**:
   - Blocking reduces movement speed (multiply movement by 0.5)
   - Cannot attack while blocking
   - Can still perform recovery actions while blocking

8. **Update Heuristic for testing**:
   - Map defensive controls:
     - Left Shift: Block (already used in Phase 1, now activates defensive block)
     - Num pad 8/2: Guard height (up/down)
     - Num pad 4/6: Evasion direction (left/right)
     - R: Toggle defensive footwork mode

**Verification Checklist:**
- [ ] Action space updated to 23 dimensions
- [ ] Block activation toggles blocking state
- [ ] Guard height affects block coverage
- [ ] Evasion creates visible lean/slip
- [ ] Defensive footwork maintains safe distance
- [ ] Actions coordinate properly (no blocking while attacking)
- [ ] Heuristic controls work for testing

**Testing Instructions:**
- Manual Heuristic testing:
  - Activate block and verify animation/state changes
  - Test guard heights against incoming attacks
  - Use evasion to slip punches
  - Test defensive footwork circling
- Verify blocking reduces damage (check CharacterCombat damage reduction)

**Commit Message Template:**
```
feat(actions): add defensive blocking and evasion actions

- Expanded action space to 23 continuous actions
- Added block activation and guard height actions
- Added evasion direction and intensity actions
- Added defensive footwork action
- Implemented blocking execution via CharacterCombat
- Implemented evasion with upper body lean/slip
- Implemented defensive footwork for safe circling
- Coordinated blocking with movement and attack limitations
- Updated Heuristic with defensive control mappings

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~8,000

---

## Task 3: Implement Defensive Physics Controllers

**Goal:** Create physics systems for blocking stance, defensive posture, and evasive movements.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsControllers/DefenseController.cs` (new)

**Prerequisites:**
- Task 2 completed (defensive actions added)

**Implementation Steps:**

1. **Create DefenseController class**:
   - Serialized parameters:
     - Block effectiveness by guard height
     - Defensive stance CoM offset (lower, more stable)
     - Evasion angle limits
     - Defensive footwork speed multiplier
     - Block stamina drain rate (optional feature)

2. **Implement blocking stance physics**:
   - Method: `ApplyBlockingStance(float guardHeight)`
   - Lower center of mass for stability
   - Tighten defensive posture (arms up, compact)
   - Increase Rigidbody drag (harder to push around)
   - Position guard at specified height:
     - Low (0-0.33): Protect body/ribs
     - Mid (0.33-0.67): General protection
     - High (0.67-1.0): Protect head
   - Visualize guard position with gizmos or debug objects

3. **Implement damage mitigation**:
   - Method: `CalculateBlockedDamage(AttackData attack, float guardHeight, Vector3 attackHeight)`
   - If attack height matches guard height: 75% damage reduction (existing system)
   - If mismatch: 25% damage reduction or none
   - Apply additional physics: blocking absorbs force (less knockback)
   - Integrate with CharacterHealth damage calculation

4. **Implement evasion lean physics**:
   - Method: `ApplyEvasionLean(float direction, float intensity)`
   - Rotate upper body (spine/torso bones) in direction
   - Use ConfigurableJoint or direct IK
   - Shift head position laterally (slip punches)
   - Amount based on intensity parameter
   - Limit maximum lean angle (don't tip over)
   - Return to neutral when evasion ends

5. **Implement defensive footwork**:
   - Method: `ApplyDefensiveFootwork(float circleDirection, Vector3 threatPosition)`
   - Circle around opponent at safe distance
   - Prioritize staying outside attack range
   - Combine with base movement but override if too close
   - Maintain facing toward threat
   - Smooth, controlled movement (not panicked retreat)

6. **Implement defensive posture**:
   - Method: `ApplyDefensivePosture()`
   - When in defensive mode:
     - Crouch slightly (lower CoM from Phase 1)
     - Weight on back foot (ready to retreat)
     - Tuck chin (protect head)
   - Affects balance and movement characteristics

7. **Implement block stamina system** (optional but adds depth):
   - Track block stamina (starts at 100)
   - Drains while blocking (e.g., -5 per second)
   - Drains faster when hit while blocking (-20 per hit)
   - Regenerates when not blocking (+10 per second)
   - If stamina depleted, block effectiveness reduced
   - Observation space already includes stamina (Task 1)

8. **Coordinate with attack system**:
   - Blocking state should prevent attacks from executing
   - Guard position affects attack availability
   - Transition smoothly between offensive and defensive states

9. **Integrate with PhysicsAgent**:
   - In FixedUpdate:
     - If blocking action active, apply blocking stance
     - If evasion requested, apply lean
     - Execute defensive footwork if defensive mode
     - Update block stamina

10. **Tune defensive parameters**:
    - Block damage reduction: 75% (existing)
    - Defensive CoM lowering: -0.2 units
    - Max evasion lean angle: 25 degrees
    - Defensive footwork speed: 0.7x normal
    - Block stamina drain: -5/sec idle, -20 per hit

**Verification Checklist:**
- [ ] DefenseController compiles without errors
- [ ] Blocking stance lowers CoM and increases stability
- [ ] Damage reduction works correctly with guard height matching
- [ ] Evasion lean visible and natural-looking
- [ ] Defensive footwork maintains safe distance
- [ ] Block stamina system functions (if implemented)
- [ ] Defensive actions coordinate with combat system
- [ ] No physics instability from defensive postures

**Testing Instructions:**
- Manual Heuristic testing:
  - Block incoming attacks, verify damage reduction
  - Test guard height matching (high guard vs head punch)
  - Use evasion to slip punches
  - Observe defensive footwork keeping distance
  - Monitor block stamina depletion and regen
- Verify defensive stance doesn't break movement or balance

**Commit Message Template:**
```
feat(physics-controllers): implement defensive physics controllers

- Created DefenseController for defensive mechanics
- Implemented blocking stance with lowered CoM and increased stability
- Implemented guard height-based damage mitigation
- Applied evasion lean with upper body rotation
- Implemented defensive footwork for safe circling
- Added defensive posture physics (crouch, weight shift)
- Implemented block stamina system with drain and regen
- Coordinated defensive actions with combat system
- Tuned defensive parameters for balance and realism

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 4: Implement Defensive Reward Function

**Goal:** Reward effective defense, damage mitigation, and smart defensive positioning.

**Files to Modify/Create:**
- `Assets/Knockout/Scripts/AI/PhysicsAgent/PhysicsAgentRewards.cs` (modify)

**Prerequisites:**
- Task 3 completed (defensive physics working)

**Implementation Steps:**

1. **Add damage mitigation rewards**:
   - **Successful Block Reward** (+0.5 per blocked hit):
     - Reward for activating block before hit lands
     - Scale by damage prevented
   - **Perfect Block Reward** (+1.0 per perfect block):
     - Bonus for matching guard height to attack height
     - Full damage mitigation
   - **Damage Prevented Reward** (+damage_prevented / max_health):
     - Proportional to damage blocked
     - Emphasizes defense effectiveness

2. **Add defensive timing rewards**:
   - **Anticipation Reward** (+0.3 per predicted block):
     - Reward for blocking before opponent attack starts
     - Requires attack prediction based on observations
   - **Reaction Block Reward** (+0.2 per reactive block):
     - Reward for blocking during opponent's windup
     - Less than anticipation but still good
   - **Block Efficiency Reward** (+0.1 per frame blocking only when needed):
     - Penalize constant blocking (-0.05 per frame blocking with no threat)
     - Encourages smart blocking, not turtling

3. **Add evasion rewards**:
   - **Successful Evasion Reward** (+0.8 per evaded attack):
     - Reward if opponent attack misses due to evasion
     - Detect by comparing attack trajectory to actual position
   - **Evasion Timing Reward** (+0.2 per well-timed evasion):
     - Reward for evading at last moment (high risk, high reward)
     - Measure timing relative to attack active frames

4. **Add positioning rewards**:
   - **Safe Distance Reward** (+0.05 per frame at safe range):
     - Reward for maintaining defensive spacing when threatened
     - Safe range: outside opponent's optimal attack range
   - **Escape Route Reward** (+0.02 per frame with escape available):
     - Reward for keeping options open (not cornered)
   - **Defensive Footwork Reward** (+0.1 per effective circle):
     - Reward for using footwork to maintain safe distance

5. **Add strategic defense rewards**:
   - **Defensive Counter Reward** (+1.5 per counter-attack after block):
     - Large reward for attacking immediately after successful block
     - Teaches block-then-counter strategy
   - **Stamina Management Reward** (+0.1 per frame with >50% block stamina):
     - Reward for maintaining stamina reserves
     - Encourages not over-blocking
   - **Adaptive Defense Reward** (+0.3 per guard height change matching opponent):
     - Reward for adjusting guard based on opponent's attack patterns
     - Requires pattern recognition

6. **Add combat balance rewards**:
   - **Balanced Aggression Reward** (+0.2 per exchange):
     - Reward for mixing offense and defense
     - Penalize pure turtling (-0.1 per extended defensive period)
     - Penalize pure aggression without defense
   - **Damage Ratio Reward** (+1.0 if damage dealt > damage taken):
     - Reward for winning damage trades
     - Encourages effective combat overall

7. **Add sparse milestone rewards**:
   - **Defensive Masterclass** (+5.0): Win round taking <20% damage
   - **Rope-a-Dope** (+3.0): Block 10+ attacks in a round
   - **Counterpuncher** (+4.0): Land 3+ counter-attacks after blocks

8. **Penalties for poor defense**:
   - **Failed Block Penalty** (-0.3 per hit while blocking incorrectly):
     - Penalize blocking with wrong guard height
   - **Over-Blocking Penalty** (-0.05 per frame blocking unnecessarily):
     - Prevent passive turtling strategy
   - **Cornered Penalty** (-0.1 per frame cornered):
     - Penalize poor positioning

9. **Update reward weight balance**:
   - Combat effectiveness: 0.35 (decreased, defense now part of combat)
   - Physical realism: 0.25
   - Strategic depth: 0.25 (increased, defense is strategic)
   - Player entertainment: 0.15

10. **Add defensive reward debugging**:
    - Log block success rate
    - Track evasion effectiveness
    - Monitor defensive vs offensive time ratio
    - Visualize defensive rewards separately

**Verification Checklist:**
- [ ] Block rewards trigger on successful blocks
- [ ] Guard height matching rewarded
- [ ] Evasion rewards for dodged attacks
- [ ] Positioning rewards for safe spacing
- [ ] Counter-attack rewards fire after blocks
- [ ] Over-blocking penalties prevent turtling
- [ ] Reward balance encourages mixed offense/defense

**Testing Instructions:**
- Manual Heuristic testing:
  - Block attacks and observe rewards
  - Match guard height and check perfect block bonus
  - Evade attacks and verify evasion rewards
  - Counter-attack after block, check bonus
  - Turtle (block constantly) and verify penalties
- Ensure defensive rewards don't dominate offensive play

**Commit Message Template:**
```
feat(rewards): implement defensive and mitigation reward function

- Added damage mitigation rewards (blocks, perfect blocks, damage prevented)
- Added defensive timing rewards (anticipation, reaction, efficiency)
- Added evasion rewards (successful evasion, timing)
- Added positioning rewards (safe distance, escape routes, footwork)
- Added strategic defense rewards (counters, stamina management, adaptive guard)
- Added combat balance rewards (mixed aggression, damage ratio)
- Added sparse milestone rewards (defensive masterclass, rope-a-dope)
- Added penalties for poor defense (failed blocks, over-blocking, cornered)
- Updated reward weights to emphasize strategic depth
- Implemented defensive reward debugging and logging

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Task 5: Create Defensive Training Configuration

**Goal:** Configure ML-Agents training for defensive behavior learning.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Config/defense_training.yaml` (new)

**Prerequisites:**
- Task 4 completed (defensive rewards implemented)

**Implementation Steps:**

1. **Create defense_training.yaml** based on balance_training.yaml

2. **Update hyperparameters**:
   - `batch_size: 4096`
   - `buffer_size: 40960`
   - `learning_rate: 1.5e-4` (slightly lower for stable defensive learning)
   - `beta: 5e-3` (moderate exploration)
   - `max_steps: 6000000`

3. **Network architecture**:
   - `hidden_units: 512` (increased for complex decision making)
   - `num_layers: 3`

4. **Configure transfer learning** from Phase 3:
   - Use `--initialize-from=balance_phase3`

5. **Self-play settings**:
   - `save_steps: 100000`
   - `play_against_latest_model_ratio: 0.7` (face strong opponents to learn defense)

6. **Optional: Asymmetric self-play**:
   - Configure one agent to be more aggressive (attack-focused)
   - Other agent learns to defend against aggression
   - Use ML-Agents team settings if implementing

**Verification Checklist:**
- [ ] defense_training.yaml valid syntax
- [ ] Transfer learning configured
- [ ] Network expanded for decision complexity
- [ ] Hyperparameters tuned for defensive learning

**Testing Instructions:**
- Validate config before training

**Commit Message Template:**
```
feat(training): create defensive training configuration

- Created defense_training.yaml based on balance config
- Configured transfer learning from Phase 3 model
- Expanded network to 512 hidden units for complex decisions
- Set max steps to 6M for defensive behavior learning
- Configured self-play to emphasize challenging opponents
- Tuned hyperparameters for defensive stability

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~7,000

---

## Task 6: Train Defensive Agent and Evaluate

**Goal:** Execute training and evaluate defensive capabilities.

**Files to Modify/Create:**
- `Assets/Knockout/Training/Models/defense_phase4.onnx`
- `docs/TRAINING_LOG.md` (update)

**Prerequisites:**
- Tasks 1-5 completed
- Phase 3 model available

**Implementation Steps:**

1. **Start training**:
   ```bash
   mlagents-learn Assets/Knockout/Training/Config/defense_training.yaml --run-id=defense_phase4 --initialize-from=balance_phase3 --time-scale=20
   ```

2. **Monitor defensive metrics**:
   - Block success rate (should increase)
   - Damage taken per episode (should decrease)
   - Time spent blocking (should stabilize, not constant)
   - Counter-attack rate (should increase)

3. **Training duration**: 4-6M steps (~3-5 hours)

4. **Evaluate trained model**:
   - Quantitative:
     - Block success rate (target >40%)
     - Damage reduction via blocking (target >30%)
     - Damage taken compared to Phase 3 (should be lower)
     - Counter-attack frequency
   - Qualitative:
     - [ ] Agents block incoming attacks
     - [ ] Guard height adjusts to attack type
     - [ ] Blocks timed appropriately (not constant)
     - [ ] Evasion used to slip punches
     - [ ] Defensive footwork maintains safe distance
     - [ ] Counter-attacks after successful blocks
     - [ ] Mix of offense and defense (not pure turtling)
     - [ ] Previous skills maintained (movement, attacks, balance)

5. **Compare to Phase 3**:
   - Should take significantly less damage
   - Fights should last longer (both agents defending)
   - More tactical, less brawling

6. **Export model** as `defense_phase4.onnx`

7. **Document results** in `docs/TRAINING_LOG.md`

8. **Troubleshooting**:
   - **Agents turtle constantly**: Increase over-blocking penalties, increase offensive rewards
   - **Agents don't block**: Increase block rewards, ensure observations detect attacks
   - **Guard height wrong**: Increase perfect block bonus, ensure height matching implemented
   - **Skills degraded**: Use transfer learning, verify network expansion compatible

**Verification Checklist:**
- [ ] Training completes successfully
- [ ] Block success rate improved
- [ ] Damage taken decreased
- [ ] Defensive timing appropriate
- [ ] Mix of offense and defense observed
- [ ] Previous skills maintained
- [ ] Model exported and tested

**Commit Message Template:**
```
feat(training): train defensive agent with blocking and evasion

- Trained for 5M steps with transfer learning from Phase 3
- Block success rate improved to 45%
- Damage taken reduced by 35% compared to Phase 3
- Agents demonstrate guard height adaptation
- Counter-attacking after blocks observed frequently
- Balanced offense/defense maintained (not turtling)
- All previous skills (movement, attacks, balance) retained
- Exported defense_phase4.onnx model
- Documented training in TRAINING_LOG.md

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

**Estimated tokens:** ~10,000

---

## Phase 4 Verification

Complete before proceeding to Phase 5:

### Observations
- [ ] Defensive observations added (~17 new)
- [ ] Total observation space ~104 dimensions
- [ ] Attack prediction working
- [ ] Blocking state tracked
- [ ] Positioning safety calculated

### Actions
- [ ] Defensive actions added (total 23 actions)
- [ ] Block activation functional
- [ ] Guard height adjustable
- [ ] Evasion and defensive footwork working

### Physics Controllers
- [ ] DefenseController implemented
- [ ] Blocking stance physics correct
- [ ] Damage mitigation functional
- [ ] Evasion lean natural-looking
- [ ] Defensive footwork maintains distance
- [ ] Block stamina system working (if implemented)

### Reward Function
- [ ] Damage mitigation rewards
- [ ] Defensive timing rewards
- [ ] Evasion rewards
- [ ] Positioning rewards
- [ ] Strategic defense rewards
- [ ] Over-blocking penalties prevent turtling

### Training Results
- [ ] Training completed for at least 4M steps
- [ ] Block success rate >35%
- [ ] Damage taken reduced from Phase 3
- [ ] Defensive behaviors demonstrated
- [ ] Balanced offense/defense
- [ ] Previous skills maintained
- [ ] Model exported as defense_phase4.onnx

### Known Limitations
- Block timing may not be perfect (human-level blocking difficult)
- Agents may over-rely on blocking vs evasion (tuning needed)
- Defensive footwork may be cautious (can adjust aggression in Phase 5)

---

## Next Steps

After Phase 4 completion:
- **[Phase 5: Arbiter Integration & Polish](Phase-5.md)** - Integrate all systems with behavioral AI, add arbiter, polish and optimize
- Ensure defense_phase4.onnx saved before proceeding

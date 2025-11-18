# Phase 4: AI Opponent Foundation

## Phase Goal

Implement a basic but functional AI opponent using a Finite State Machine that can make decisions based on distance, health, and player behavior. The AI will use the same character systems (CharacterCombat, CharacterMovement) as the player, providing a challenging single-player experience.

**Success Criteria:**
- AI state machine implemented with observe, approach, retreat, attack, and defend states
- AI makes decisions based on distance to player and health status
- AI uses CharacterCombat and CharacterMovement to execute actions
- CharacterAI component replaces CharacterInput for AI-controlled characters
- AI opponent provides engaging gameplay challenge
- All tests passing

**Estimated Tokens:** ~90,000

---

## Prerequisites

- Phase 3 complete (all combat systems functional)
- Player can control character and fight stationary opponent
- CharacterCombat and CharacterMovement components work correctly

---

## Tasks

### Task 1: Create AI State Machine Foundation

**Goal:** Create the abstract AI state classes and state machine manager.

**Files to Create:**
- `Assets/Knockout/Scripts/AI/States/AIState.cs` (abstract)
- `Assets/Knockout/Scripts/AI/AIStateMachine.cs`
- `Assets/Knockout/Scripts/AI/AIContext.cs` (data structure)

**Implementation Steps:**

1. **Create AIContext struct** - holds data for AI decision-making:
   - Reference to player character
   - Distance to player
   - Own health percentage
   - Player health percentage
   - Player is attacking (bool)
   - Time since last state change

2. **Create AIState abstract class:**
   - Methods: Enter(), Update(AIContext), Exit(), CanTransitionTo(AIState)
   - Each state returns next state or null (stay in current state)

3. **Create AIStateMachine:**
   - Manages current state, handles transitions
   - Updates context each frame
   - Calls currentState.Update(), handles state changes

**Estimated Tokens:** ~12,000

---

### Task 2: Implement AI States

**Goal:** Create concrete implementations of each AI state with decision-making logic.

**Files to Create:**
- `Assets/Knockout/Scripts/AI/States/ObserveState.cs`
- `Assets/Knockout/Scripts/AI/States/ApproachState.cs`
- `Assets/Knockout/Scripts/AI/States/RetreatState.cs`
- `Assets/Knockout/Scripts/AI/States/AttackState.cs`
- `Assets/Knockout/Scripts/AI/States/DefendState.cs`

**Implementation Summary:**

**ObserveState** (default/idle state):
- Maintain distance around 2.5-3.5 units
- Transition to Approach if player too far (> 4.0)
- Transition to Retreat if player too close (< 1.5)
- Transition to Attack if player in range and not blocking
- Transition to Defend if player is attacking

**ApproachState:**
- Move toward player
- Transition to Observe when in optimal range (2.0-3.0)
- Transition to Attack if close enough (< 2.5)
- Transition to Defend if player attacks during approach

**RetreatState:**
- Move away from player
- Transition to Observe when at safe distance (> 3.5)
- Transition to Defend if player pursues and attacks
- Triggered when health < 30%

**AttackState:**
- Choose attack based on distance:
  - Close (< 1.5): Uppercut (high damage)
  - Medium (1.5-2.5): Hook or Jab (randomized 50/50)
  - Far (> 2.5): Approach instead
- Add randomization (30% chance to choose different attack)
- Execute attack via CharacterCombat component
- Transition to Observe after attack completes
- Transition to Defend if hit during attack

**DefendState:**
- Activate blocking via CharacterCombat
- Hold block for random duration (0.5-1.5 seconds)
- Transition to Observe when threat passes
- Transition to Retreat if health critical (< 20%)

**Estimated Tokens:** ~25,000

---

### Task 3: Create CharacterAI Component

**Goal:** Create the AI component that drives AI characters using the state machine and character subsystems.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterAI.cs`

**Implementation Steps:**

1. **Component structure:**
   - Reference to target (player character)
   - AIStateMachine instance
   - References to CharacterCombat, CharacterMovement, CharacterHealth
   - Decision-making update interval (default 0.1s, not every frame)

2. **Initialize AI:**
   - Find player character in scene (tag-based or reference)
   - Create and initialize AIStateMachine with ObserveState
   - Cache component references

3. **Update loop:**
   - Every 0.1s, update AIContext with current game state
   - Call AIStateMachine.Update(context)
   - State machine handles state transitions
   - States command CharacterCombat and CharacterMovement

4. **AI actions:**
   - MoveToward(Vector3 target) - calls CharacterMovement
   - MoveAwayFrom(Vector3 target) - calls CharacterMovement
   - ExecuteAttack(int attackType) - calls CharacterCombat
   - StartDefending() / StopDefending() - calls CharacterCombat

5. **Difficulty tuning parameters:**
   - Reaction time (decision update interval)
   - Attack accuracy (randomization percentage)
   - Aggression level (affects state transition thresholds)

**Estimated Tokens:** ~20,000

---

### Task 4: Configure AI Character Prefab

**Goal:** Set up the AICharacter prefab to use CharacterAI instead of CharacterInput.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Implementation Steps:**

1. Open AICharacter prefab
2. Remove CharacterInput component (if present)
3. Add CharacterAI component
4. Assign target reference to player (can be set at runtime via tag)
5. Configure AI parameters (reaction time, aggression)
6. Save prefab

**Estimated Tokens:** ~5,000

---

### Task 5: Create AI Target Detection System

**Goal:** Allow AI to dynamically find and track the player character.

**Files to Create:**
- `Assets/Knockout/Scripts/AI/AITargetDetector.cs` (helper class)

**Implementation:**

- Use Physics.OverlapSphere to detect nearby characters
- Filter for player tag
- Cache target reference
- Update target if player changes (for future multi-opponent support)
- Fallback: Find player by tag on Start()

**Estimated Tokens:** ~8,000

---

### Task 6: Implement AI Movement and Rotation

**Goal:** Make AI smoothly move and rotate toward/away from player.

**Implementation in CharacterAI:**

- Calculate direction vector to/from player
- Convert to movement input (Vector2) for CharacterMovement
- Smooth rotation toward player using Quaternion.Slerp
- Maintain optimal distance (strafe around player when in range)
- Stop movement when executing attacks

**Estimated Tokens:** ~10,000

---

### Task 7: Add AI Behavior Variation

**Goal:** Prevent AI from being too predictable with timing and pattern variation.

**Implementation:**

- Randomize state duration (min/max ranges)
- Random attack selection within distance constraints
- Occasional "mistakes" (suboptimal choices 20% of the time)
- Variable reaction time (slight delay before responding to player actions)
- Add personality parameters (aggressive vs defensive)

**Estimated Tokens:** ~10,000

---

## Phase Verification

**All Tasks Complete:**
- [ ] AI state machine foundation created
- [ ] All five AI states implemented (Observe, Approach, Retreat, Attack, Defend)
- [ ] CharacterAI component created and added to AICharacter prefab
- [ ] AI can find and track player target
- [ ] AI moves toward/away from player appropriately
- [ ] AI executes attacks based on distance
- [ ] AI defends when player attacks
- [ ] AI behavior includes variation/randomization

**Gameplay Testing:**
- [ ] AI maintains appropriate distance from player
- [ ] AI attacks when player in range
- [ ] AI defends when player attacks
- [ ] AI retreats when low health
- [ ] AI movement looks natural (not robotic)
- [ ] AI provides reasonable challenge (not too easy/hard)

**Integration:**
- [ ] AI uses same combat/movement systems as player
- [ ] No code duplication between player and AI control
- [ ] State transitions feel responsive (not laggy)

**Tests Passing:**
- [ ] AI state machine unit tests pass
- [ ] AI behavior integration tests pass
- [ ] Manual gameplay test: Player vs AI is playable and fun

**Known Limitations:**
- AI uses simple FSM (not physics-based control from research paper)
- AI doesn't learn or adapt during match
- Single opponent only (no multi-AI support yet)

---

**Phase 4 Complete!** Proceed to [Phase 5: Polish, Testing & Integration](Phase-5.md).

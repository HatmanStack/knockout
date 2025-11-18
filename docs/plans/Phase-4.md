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

**Verification Checklist:**
- [ ] AIContext struct created with all necessary data
- [ ] AIState abstract class defines proper interface
- [ ] AIStateMachine manages state transitions
- [ ] All files compile without errors

**Testing Instructions:**

```csharp
// File: Assets/Knockout/Tests/EditMode/AI/AIStateMachineTests.cs
[Test]
public void AIStateMachine_UpdateContext_CalculatesDistanceToPlayer()
{
    var context = new AIContext();
    var aiPos = Vector3.zero;
    var playerPos = new Vector3(5f, 0f, 0f);

    context.UpdateFrom(aiPos, playerPos, 100f, 100f, false);

    Assert.AreEqual(5f, context.DistanceToPlayer, 0.1f);
}
```

**Commit Message Template:**
```
feat(ai): create AI state machine foundation

- Add AIContext struct for decision-making data
- Create AIState abstract base class
- Implement AIStateMachine for state management
- Add edit mode tests for state machine logic

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

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

**Verification Checklist:**
- [ ] All five AI states created (Observe, Approach, Retreat, Attack, Defend)
- [ ] Each state implements Enter/Update/Exit/CanTransitionTo
- [ ] Distance-based decision logic works correctly
- [ ] Attack selection based on distance
- [ ] States integrate with CharacterCombat and CharacterMovement

**Testing Instructions:**

```csharp
// File: Assets/Knockout/Tests/PlayMode/AI/AIStatesTests.cs
[Test]
public void AttackState_ChoosesUppercut_WhenCloseRange()
{
    var attackState = new AttackState();
    var context = new AIContext { DistanceToPlayer = 1.0f };

    int selectedAttack = attackState.ChooseAttack(context);

    Assert.AreEqual(AttackType.Uppercut, selectedAttack);
}
```

**Commit Message Template:**
```
feat(ai): implement all AI behavioral states

- Create ObserveState for maintaining optimal distance
- Create ApproachState for closing distance to player
- Create RetreatState for escaping when low health
- Create AttackState with distance-based attack selection
- Create DefendState for blocking incoming attacks
- Add play mode tests for state decision logic

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

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

**Verification Checklist:**
- [ ] CharacterAI component created and compiles
- [ ] AI finds player target on Start
- [ ] AI state machine updates correctly
- [ ] AI actions call appropriate character components
- [ ] Decision-making runs at configured interval (not every frame)
- [ ] Difficulty parameters adjustable via Inspector

**Testing Instructions:**

```csharp
// File: Assets/Knockout/Tests/PlayMode/Characters/CharacterAITests.cs
[UnityTest]
public IEnumerator CharacterAI_FindsPlayerTarget_OnStart()
{
    var player = CreatePlayerCharacter();
    player.tag = "Player";
    var aiCharacter = CreateAICharacter();
    var ai = aiCharacter.GetComponent<CharacterAI>();

    yield return null; // Wait for Start()

    Assert.IsNotNull(ai.Target);
    Assert.AreEqual(player, ai.Target);
}
```

**Commit Message Template:**
```
feat(ai): create CharacterAI component

- Implement CharacterAI with state machine integration
- Add player target detection and tracking
- Create decision-making update loop (10Hz)
- Add AI action methods (move, attack, defend)
- Implement difficulty tuning parameters
- Add play mode tests for AI behavior

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 4: Configure AI Character Prefab

**Goal:** Set up the AICharacter prefab to use CharacterAI instead of CharacterInput.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Prerequisites:**
- Task 3 complete (CharacterAI component exists)

**Implementation Steps:**

1. Open AICharacter prefab in Unity
2. Remove CharacterInput component (if present from Phase 3)
3. Add CharacterAI component
4. Assign target reference to player (or leave null for runtime detection)
5. Configure AI parameters:
   - Reaction Time: 0.1s (default)
   - Attack Accuracy: 70% (default)
   - Aggression Level: Medium
6. Save prefab

**Verification Checklist:**
- [ ] CharacterInput removed from AICharacter prefab
- [ ] CharacterAI added and configured
- [ ] AI parameters set to reasonable defaults
- [ ] Prefab saved without errors

**Testing Instructions:**
Manual verification - open AICharacter prefab in Inspector and verify CharacterAI component is present with correct settings.

**Commit Message Template:**
```
refactor(ai): configure AICharacter prefab with AI component

- Remove CharacterInput from AICharacter
- Add CharacterAI component
- Configure default AI parameters
- AI ready for gameplay testing

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 5: Create AI Target Detection System

**Goal:** Allow AI to dynamically find and track the player character.

**Files to Create:**
- `Assets/Knockout/Scripts/AI/AITargetDetector.cs` (helper class)

**Prerequisites:**
- None (standalone utility class)

**Implementation Steps:**

1. **Create AITargetDetector class:**
   - Static utility methods for finding targets
   - `FindPlayerCharacter()`: Uses GameObject.FindWithTag("Player")
   - `FindNearestCharacter(Vector3 position, float radius)`: Uses Physics.OverlapSphere
   - Optional: Caching to avoid repeated searches

2. **Target detection patterns:**
   - Primary: Tag-based search (fastest for single player)
   - Fallback: Physics overlap (for future multi-opponent)
   - Cache player reference after first find
   - Validate target still exists before using

3. **Integration with CharacterAI:**
   - CharacterAI calls AITargetDetector.FindPlayerCharacter() in Start()
   - Stores reference to target
   - Optional: Re-check target validity each frame (target could be destroyed)

**Verification Checklist:**
- [ ] AITargetDetector class created
- [ ] FindPlayerCharacter method works
- [ ] FindNearestCharacter method works
- [ ] CharacterAI successfully finds target

**Testing Instructions:**

```csharp
[Test]
public void AITargetDetector_FindsPlayerByTag()
{
    var player = new GameObject("Player");
    player.tag = "Player";

    GameObject found = AITargetDetector.FindPlayerCharacter();

    Assert.AreEqual(player, found);
    Object.DestroyImmediate(player);
}
```

**Commit Message Template:**
```
feat(ai): add AI target detection system

- Create AITargetDetector utility class
- Implement tag-based player finding
- Implement physics-based nearest character finding
- Add target caching for performance
- Add unit tests for target detection

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 6: Implement AI Movement and Rotation

**Goal:** Make AI smoothly move and rotate toward/away from player.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterAI.cs`

**Prerequisites:**
- Tasks 1-5 complete
- CharacterMovement component exists

**Implementation Steps:**

1. **Add movement methods to CharacterAI:**
   - `MoveToward(Vector3 target)`:
     - Calculate direction from AI to target
     - Convert 3D direction to 2D input (X/Z plane)
     - Call `characterMovement.SetMovementInput(input2D)`
   - `MoveAwayFrom(Vector3 target)`:
     - Calculate direction from target to AI (inverse)
     - Call CharacterMovement with negative direction

2. **Rotation toward player:**
   - In Update: Always face player character
   - `characterMovement.RotateToward(playerTarget.position)`
   - Keeps AI facing player even while strafing

3. **Distance maintenance (for ObserveState):**
   - Calculate current distance to player
   - If distance > 3.5: Move toward
   - If distance < 2.5: Move away
   - Else: Strafe (random left/right movement)
   - This creates natural "circling" behavior

4. **Stop movement during attacks:**
   - Check `characterCombat.CurrentState` before moving
   - Only allow movement in: Idle, Blocking states
   - Set movement input to Vector2.zero during attacks

**Verification Checklist:**
- [ ] AI moves toward player when far away
- [ ] AI moves away when too close
- [ ] AI rotates to face player
- [ ] AI strafes at optimal distance
- [ ] Movement stops during attacks

**Testing Instructions:**
Manual gameplay test - AI should smoothly approach, retreat, and circle around player.

**Commit Message Template:**
```
feat(ai): implement AI movement and rotation

- Add MoveToward and MoveAwayFrom methods
- Implement rotation to face player
- Add distance maintenance logic
- Prevent movement during attack animations
- AI now moves naturally during combat

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 7: Add AI Behavior Variation

**Goal:** Prevent AI from being too predictable with timing and pattern variation.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterAI.cs`
- All AI state files (add randomization)

**Prerequisites:**
- Tasks 1-6 complete

**Implementation Steps:**

1. **State duration randomization:**
   - Each state has min/max duration (e.g., Observe: 1-3 seconds)
   - Random.Range to pick actual duration on Enter()
   - Prevents AI from transitioning at exact same time intervals

2. **Attack selection randomization:**
   - In AttackState.ChooseAttack():
     - 70% of time: Choose optimal attack for distance
     - 30% of time: Choose random valid attack
   - Makes AI less predictable while still being smart

3. **Reaction delay:**
   - Add small random delay (0.1-0.3s) before responding to player actions
   - E.g., when player attacks, wait random delay before transitioning to DefendState
   - Makes AI feel less robotic

4. **Occasional mistakes:**
   - 20% chance to make suboptimal choice:
     - Attack when should defend
     - Approach when should retreat (if health not critical)
     - Use wrong attack for distance
   - Creates openings for player, makes AI beatable

5. **Personality parameters (optional):**
   - Aggression slider (0-1):
     - High: Attack more, defend less, approach aggressively
     - Low: Defend more, retreat more, cautious approach
   - Affects state transition thresholds
   - Can create different AI difficulty levels

**Verification Checklist:**
- [ ] AI behavior varies between matches
- [ ] State transitions not perfectly timed
- [ ] Attack selection includes randomness
- [ ] AI makes occasional mistakes
- [ ] AI feels less robotic and more human-like

**Testing Instructions:**
Play multiple matches - AI should behave differently each time, with varied timing and decisions.

**Commit Message Template:**
```
feat(ai): add behavior variation and personality

- Randomize AI state duration for unpredictability
- Add attack selection randomization (70/30 split)
- Implement reaction delay to player actions
- Add occasional mistakes for balanced difficulty
- Add optional aggression parameter for tuning
- AI now feels more human-like and engaging

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

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

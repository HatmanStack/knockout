# Phase 3: Combat Mechanics & Hit Detection

## Phase Goal

Implement the complete combat system including hit detection with hitboxes/hurtboxes, damage calculation, combat state machine, character health system, and player input integration. By the end of this phase, you will have a fully playable single-player experience where the player can control a character to fight (though the opponent will be stationary until Phase 4).

**Success Criteria:**
- Hit detection system functional with hitbox and hurtbox colliders
- Combat state machine enforces valid state transitions
- CharacterCombat component controls attacks and defense
- CharacterHealth component manages damage and death
- CharacterInput component connects Unity Input System to character
- Player can move, attack, block, and damage opponent
- All tests passing


---

## Prerequisites

### Must Complete First
- Phase 2 complete (animation system fully functional)
- Animator Controller with all layers working
- CharacterAnimator component created

### Verify Before Starting
1. Character prefabs animate correctly
2. AnimationTester works for manual testing
3. Animation events fire correctly

---

## Tasks

### Task 1: Create Hit Detection Data Structures

**Goal:** Create the data structures and ScriptableObjects that define hit properties, hurt regions, and damage calculations.

**Files to Create:**
- `Assets/Knockout/Scripts/Combat/HitDetection/HitData.cs` (struct)
- `Assets/Knockout/Scripts/Combat/HitDetection/HurtboxData.cs` (component)
- `Assets/Knockout/Scripts/Combat/HitDetection/HitboxData.cs` (component)

**Prerequisites:**
- Phase 1 Task 7 complete (AttackData ScriptableObject exists)

**Implementation Steps:**

1. **Create HitData struct:**
   - Create new C# file: `Assets/Knockout/Scripts/Combat/HitDetection/HitData.cs`
   - Define a public struct in the `Knockout.Combat.HitDetection` namespace
   - Add public fields to represent a hit event:
     - Attacker (GameObject), Damage (float), Knockback (float)
     - HitPoint (Vector3), HitDirection (Vector3)
     - HitType (int, 0=light 1=medium 2=heavy), AttackName (string)
   - Create a constructor that accepts all fields and initializes them
   - Purpose: Passed from attacker's hitbox to target's CharacterHealth when collision occurs

2. **Create HurtboxData component:**
   - Create new C# file: `Assets/Knockout/Scripts/Combat/HitDetection/HurtboxData.cs`
   - Inherit from `MonoBehaviour` in the `Knockout.Combat.HitDetection` namespace
   - Add `[RequireComponent(typeof(Collider))]` attribute
   - Define serialized fields:
     - damageMultiplier (float, default 1.0) - e.g., head = 1.5x, body = 1.0x
     - hitTypeOverride (int, default -1) - allows specific body part to force hit type, -1 means use attack's type
     - ownerCharacter (GameObject reference to character root)
   - Expose fields as read-only public properties
   - Implement Awake(): Verify attached collider is set to isTrigger, log warning and fix if not
   - Implement OnValidate(): Auto-assign ownerCharacter by finding CharacterController in parent if not already set
   - Follow Phase-0 code conventions

3. **Create HitboxData component:**
   - Create new C# file: `Assets/Knockout/Scripts/Combat/HitDetection/HitboxData.cs`
   - Inherit from `MonoBehaviour` in the `Knockout.Combat.HitDetection` namespace
   - Add `[RequireComponent(typeof(Collider))]` attribute
   - Import `System.Collections.Generic` for HashSet
   - Define serialized field: ownerCharacter (GameObject)
   - Define private fields:
     - _currentAttack (AttackData) - stores current attack's data while active
     - _hitTargets (HashSet<GameObject>) - prevents multiple hits on same target per attack
     - _collider (Collider) - cached collider reference
   - Cache collider in Awake(), disable it by default, verify isTrigger is true
   - Implement `ActivateHitbox(AttackData attackData)` method:
     - Store attackData in _currentAttack
     - Clear _hitTargets HashSet
     - Enable collider
   - Implement `DeactivateHitbox()` method:
     - Disable collider
     - Clear _currentAttack and _hitTargets
   - Implement `OnTriggerEnter(Collider other)` method:
     - Check if other has HurtboxData component, return if not
     - Check if hurtbox belongs to self (ownerCharacter), return if yes (no self-damage)
     - Check if target already in _hitTargets, return if yes (no multi-hit)
     - Add target to _hitTargets
     - Call CalculateHitData() to create HitData struct
     - Call SendHitToTarget() to deliver hit
   - Implement `CalculateHitData(HurtboxData hurtbox, Vector3 hitPoint)` private method:
     - Get base damage from _currentAttack
     - Multiply by hurtbox.DamageMultiplier for final damage
     - Calculate hit direction (from hitbox to hurtbox, normalized)
     - Determine hit type (use hurtbox override if set, otherwise categorize by damage thresholds: <15=light, 15-30=medium, >30=heavy)
     - Return new HitData struct with all calculated values
   - Implement `SendHitToTarget(GameObject target, HitData hitData)` private method:
     - Find CharacterHealth component on target using GetComponent
     - Call CharacterHealth.TakeDamage(hitData)
     - Log warning if CharacterHealth not found
   - Implement OnValidate(): Auto-assign ownerCharacter from parent CharacterController
   - Follow Phase-0 code organization and naming conventions

**Verification Checklist:**
- [ ] All three scripts created and compile without errors
- [ ] HitData struct has all required fields
- [ ] HurtboxData and HitboxData components have RequireComponent attributes
- [ ] Auto-validation in OnValidate() works

**Testing Instructions:**
Create edit mode tests for data structures:

```csharp
// File: Assets/Knockout/Tests/EditMode/Combat/HitDataTests.cs
[Test]
public void HitData_Constructor_SetsAllFields()
{
    // Arrange & Act
    GameObject attacker = new GameObject();
    HitData hitData = new HitData(
        attacker: attacker,
        damage: 20f,
        knockback: 1.5f,
        hitPoint: Vector3.zero,
        hitDirection: Vector3.forward,
        hitType: 1,
        attackName: "TestAttack"
    );

    // Assert
    Assert.AreEqual(attacker, hitData.Attacker);
    Assert.AreEqual(20f, hitData.Damage);
    Assert.AreEqual(1.5f, hitData.Knockback);
    Assert.AreEqual(1, hitData.HitType);
    Assert.AreEqual("TestAttack", hitData.AttackName);

    // Cleanup
    Object.DestroyImmediate(attacker);
}
```

**Commit Message Template:**
```
feat(combat): create hit detection data structures

- Add HitData struct for hit event information
- Add HurtboxData component for damage regions
- Add HitboxData component for attack hitboxes
- Implement hit detection logic in OnTriggerEnter
- Calculate damage with hurtbox multipliers
- Prevent multiple hits per attack on same target
- Add edit mode tests for hit data
```


---

### Task 2: Add Hitboxes and Hurtboxes to Character Prefab

**Goal:** Add collider GameObjects with HitboxData and HurtboxData components to the character prefab.

**Files to Modify:**
- `Assets/Knockout/Prefabs/Characters/PlayerCharacter.prefab`
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Prerequisites:**
- Task 1 complete (HitboxData and HurtboxData components exist)

**Implementation Steps:**

1. **Open PlayerCharacter prefab.**

2. **Create hurtbox colliders:**
   - Expand "Hurtboxes" child GameObject (created in Phase 1)
   - Create child GameObject under Hurtboxes: "Hurtbox_Head"
     - Add Sphere Collider
       - Is Trigger: True
       - Center: (0, 1.6, 0) - approximate head position
       - Radius: 0.15
     - Add HurtboxData component
       - Damage Multiplier: 1.5 (headshots do more damage)
       - Hit Type Override: -1 (use attack's type)
       - Owner Character: (auto-assigned to prefab root)

   - Create child GameObject: "Hurtbox_Body"
     - Add Capsule Collider
       - Is Trigger: True
       - Center: (0, 1.0, 0)
       - Radius: 0.25
       - Height: 0.8
     - Add HurtboxData component
       - Damage Multiplier: 1.0
       - Hit Type Override: -1
       - Owner Character: (auto-assigned)

3. **Create hitbox colliders:**
   - Expand "Hitboxes" child GameObject
   - Create child GameObject: "Hitbox_LeftHand"
     - Add Sphere Collider
       - Is Trigger: True
       - Center: (0, 0, 0) - will be positioned by animation rig
       - Radius: 0.1
       - **Enabled: False** (starts disabled)
     - Add HitboxData component
       - Owner Character: (auto-assigned)

   - Create child GameObject: "Hitbox_RightHand"
     - Same as LeftHand

4. **Position hitboxes on hands:**
   - **Option A: Manual positioning** (if no animation rigging):
     - Position Hitbox_LeftHand at approximate left hand position during idle
     - Position Hitbox_RightHand at right hand position
     - Hitboxes will move with character but not track hands precisely

   - **Option B: Constraint-based positioning** (recommended):
     - Select Hitbox_LeftHand GameObject
     - Add component: Parent Constraint
       - Add Source: Find "LeftHand" bone in character rig hierarchy
       - Weight: 1.0
       - Activate constraint
       - Now hitbox follows left hand bone
     - Repeat for Hitbox_RightHand with "RightHand" bone

5. **Verify setup:**
   - In Scene view, hurtbox colliders should be visible as green wireframes (when GameObject selected)
   - Hitbox colliders visible as yellow/blue wireframes
   - Colliders should roughly match character proportions

6. **Test collider placement:**
   - Enter Play mode
   - Select character in Hierarchy
   - In Scene view, verify hurtbox positions match head and body
   - Manually enable a hitbox collider (via Inspector)
   - Verify it follows hand position

7. **Save prefab changes.**

8. **Apply to AICharacter prefab:**
   - Open AICharacter prefab
   - Repeat steps 2-7 (or overrides should inherit from PlayerCharacter if it's a prefab variant)

**Verification Checklist:**
- [ ] Hurtbox_Head and Hurtbox_Body created with correct colliders
- [ ] Hitbox_LeftHand and Hitbox_RightHand created
- [ ] All colliders are triggers
- [ ] Hitbox colliders start disabled
- [ ] HurtboxData components have correct damage multipliers
- [ ] HitboxData components reference owner character
- [ ] Hitboxes constrained to hand bones (if using Parent Constraint)
- [ ] Changes saved to both prefabs

**Testing Instructions:**
Manual verification in Scene view and Play mode. Visual inspection of collider gizmos is sufficient.

**Commit Message Template:**
```
feat(combat): add hitbox and hurtbox colliders to character prefabs

- Create head and body hurtbox colliders with HurtboxData
- Create left and right hand hitbox colliders with HitboxData
- Set head hurtbox to 1.5x damage multiplier
- Constrain hitboxes to hand bones for accurate tracking
- Configure all colliders as triggers
- Apply setup to both player and AI prefabs
```


---

### Task 3: Create Combat State Machine

**Goal:** Implement a finite state machine to manage combat states and enforce valid transitions between idle, attacking, blocking, hit reactions, and knockouts.

**Files to Create:**
- `Assets/Knockout/Scripts/Combat/States/CombatState.cs` (abstract base class)
- `Assets/Knockout/Scripts/Combat/States/IdleState.cs`
- `Assets/Knockout/Scripts/Combat/States/AttackingState.cs`
- `Assets/Knockout/Scripts/Combat/States/BlockingState.cs`
- `Assets/Knockout/Scripts/Combat/States/HitStunnedState.cs`
- `Assets/Knockout/Scripts/Combat/States/KnockedDownState.cs`
- `Assets/Knockout/Scripts/Combat/States/KnockedOutState.cs`
- `Assets/Knockout/Scripts/Combat/CombatStateMachine.cs`

**Prerequisites:**
- Task 2 complete (hitboxes/hurtboxes added)

**Implementation Steps:**

1. **Create CombatState abstract class:**
   - Define interface: `Enter(CharacterCombat)`, `Update(CharacterCombat)`, `Exit(CharacterCombat)`, `CanTransitionTo(CombatState)`
   - Each state knows which transitions are valid
   - States can access CharacterCombat to trigger animations, check conditions

2. **Implement IdleState:**
   - Can transition to: AttackingState, BlockingState, HitStunnedState, KnockedDownState, KnockedOutState
   - Cannot transition to: (already idle)
   - Update: Check for input to attack or block

3. **Implement AttackingState:**
   - Can transition to: IdleState (after attack completes), HitStunnedState, KnockedDownState, KnockedOutState
   - Cannot transition to: AttackingState (no attack canceling), BlockingState (committed to attack)
   - Enter: Store current attack data, trigger attack animation
   - Exit: Clear attack data

4. **Implement BlockingState:**
   - Can transition to: IdleState (release block), HitStunnedState (block broken), KnockedDownState, KnockedOutState
   - Cannot transition to: AttackingState (can't attack while blocking)
   - Enter: Trigger block animation, enable damage reduction
   - Exit: Disable damage reduction

5. **Implement HitStunnedState:**
   - Can transition to: IdleState (after stun duration), KnockedDownState, KnockedOutState
   - Cannot transition to: AttackingState, BlockingState (stunned can't act)
   - Enter: Trigger hit reaction animation
   - Exit: Reset to idle after animation completes

6. **Implement KnockedDownState:**
   - Can transition to: IdleState (after get-up animation), KnockedOutState
   - Cannot transition to: Others (must get up first)
   - Enter: Trigger knockdown animation
   - Update: Wait for get-up animation to complete

7. **Implement KnockedOutState:**
   - Can transition to: Nothing (terminal state)
   - Enter: Trigger knockout animation, disable character

8. **Create CombatStateMachine:**
   - Manages current state reference
   - `ChangeState(CombatState newState)` method validates transition with `CanTransitionTo`
   - Calls `Exit()` on old state, `Enter()` on new state
   - `Update()` delegates to current state
   - Logs warnings for invalid transition attempts

**Verification Checklist:**
- [ ] All state classes created and compile
- [ ] State transitions enforce valid combat rules
- [ ] CombatStateMachine properly manages state changes
- [ ] Invalid transitions are rejected with warnings
- [ ] Each state has clear entry/exit behavior

**Testing Instructions:**
Create edit mode tests for state machine logic:

```csharp
// File: Assets/Knockout/Tests/EditMode/Combat/CombatStateMachineTests.cs
[Test]
public void IdleState_CanTransitionTo_AttackingState()
{
    var idleState = new IdleState();
    var attackingState = new AttackingState();
    Assert.IsTrue(idleState.CanTransitionTo(attackingState));
}

[Test]
public void AttackingState_CannotTransitionTo_BlockingState()
{
    var attackingState = new AttackingState();
    var blockingState = new BlockingState();
    Assert.IsFalse(attackingState.CanTransitionTo(blockingState));
}
```

**Commit Message Template:**
```
feat(combat): implement combat state machine

- Create CombatState abstract base class
- Implement all combat states (Idle, Attacking, Blocking, HitStunned, KnockedDown, KnockedOut)
- Create CombatStateMachine to manage transitions
- Enforce valid state transition rules
- Add edit mode tests for state transitions

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 4: Create CharacterCombat Component

**Goal:** Create the component that manages combat actions, owns the combat state machine, and controls hitbox activation during attacks.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterCombat.cs`

**Prerequisites:**
- Task 3 complete (combat state machine exists)
- Phase 1 Task 7 complete (AttackData ScriptableObjects exist)

**Implementation Steps:**

1. **Component structure:**
   - Reference to CharacterAnimator (for triggering animations)
   - Reference to CombatStateMachine instance
   - References to hitbox GameObjects (left hand, right hand)
   - Current AttackData being executed
   - Events: OnAttackExecuted, OnBlockStarted, OnBlockEnded

2. **Initialize combat system:**
   - Awake: Cache component references, find hitboxes in children
   - Start: Initialize state machine with IdleState
   - Subscribe to CharacterAnimator animation events

3. **Attack execution:**
   - `ExecuteAttack(AttackData attackData)` method:
     - Validate can attack (check state machine)
     - Store current attack data
     - Transition to AttackingState
     - Trigger attack animation via CharacterAnimator
   - Convenience methods: `ExecuteJab()`, `ExecuteHook()`, `ExecuteUppercut()` that load appropriate AttackData

4. **Hitbox management:**
   - Subscribe to CharacterAnimator.OnHitboxActivate event:
     - Find appropriate hitbox (based on attack animation)
     - Call `HitboxData.ActivateHitbox(currentAttackData)`
   - Subscribe to CharacterAnimator.OnHitboxDeactivate event:
     - Call `HitboxData.DeactivateHitbox()`

5. **Defense actions:**
   - `StartBlocking()`: Transition to BlockingState, trigger block animation
   - `StopBlocking()`: Transition back to IdleState, stop block animation

6. **Handle animation event callbacks:**
   - OnAttackEnd: Transition back to IdleState, clear current attack data
   - OnHitReactionEnd: Called by CharacterHealth when hit, updates state machine

7. **State machine integration:**
   - Update: Call `stateMachine.Update()` each frame
   - Provide public CurrentState property for other components to check

**Verification Checklist:**
- [ ] CharacterCombat component created and compiles
- [ ] Combat state machine properly integrated
- [ ] Attack execution triggers correct animations
- [ ] Hitboxes activate/deactivate at correct times
- [ ] Blocking transitions state correctly
- [ ] Component added to character prefabs

**Testing Instructions:**
Create play mode test for combat actions:

```csharp
// File: Assets/Knockout/Tests/PlayMode/Characters/CharacterCombatTests.cs
[UnityTest]
public IEnumerator CharacterCombat_ExecuteAttack_TriggersAnimation()
{
    // Setup character with combat component
    GameObject character = InstantiateCharacterPrefab();
    CharacterCombat combat = character.GetComponent<CharacterCombat>();
    AttackData jabData = LoadAttackData("AttackData_Jab");

    yield return null;

    // Execute attack
    bool attacked = false;
    combat.OnAttackExecuted += () => attacked = true;
    combat.ExecuteAttack(jabData);

    yield return new WaitForSeconds(0.1f);

    Assert.IsTrue(attacked);
    Assert.AreEqual("Attacking", combat.CurrentState.GetType().Name);
}
```

**Commit Message Template:**
```
feat(combat): create CharacterCombat component

- Implement CharacterCombat component with state machine integration
- Add attack execution methods (Jab, Hook, Uppercut)
- Implement hitbox activation control via animation events
- Add blocking state management
- Subscribe to CharacterAnimator events for combat flow
- Add play mode tests for combat actions

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 5: Create CharacterHealth Component

**Goal:** Implement health management, damage calculation with multipliers, and hit reaction triggering.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterHealth.cs`

**Prerequisites:**
- Task 4 complete (CharacterCombat exists)
- Phase 1 Task 7 complete (CharacterStats ScriptableObject exists)

**Implementation Steps:**

1. **Component properties:**
   - Reference to CharacterStats (for max health)
   - Current health value (private, expose via property)
   - Reference to CharacterAnimator (for hit reactions)
   - Reference to CharacterCombat (to check if blocking)
   - Events: OnHealthChanged(float current, float max), OnDeath, OnHitTaken(HitData)

2. **Initialize health:**
   - Start: Set current health to CharacterStats.MaxHealth
   - Cache component references

3. **Damage application - TakeDamage(HitData) method:**
   - Calculate damage multipliers:
     - Base damage from HitData
     - Character damage taken multiplier from CharacterStats
     - Blocking reduction: if CharacterCombat.IsBlocking, multiply by 0.25
   - Subtract final damage from current health
   - Clamp health to 0-max range
   - Raise OnHealthChanged event
   - Raise OnHitTaken event
   - Trigger hit reaction or death

4. **Hit reaction selection:**
   - Based on final damage amount:
     - < 15 damage: Light hit reaction (HitType = 0)
     - 15-30 damage: Medium hit reaction (HitType = 1)
     - > 30 damage: Heavy hit reaction (HitType = 2)
   - If health reaches 0: Trigger knockout instead of hit reaction
   - If health < 30 and heavy hit: Consider knockdown
   - Call CharacterAnimator.TriggerHitReaction(hitType) or TriggerKnockout()

5. **Death handling:**
   - When health reaches 0:
     - Trigger knockout animation via CharacterAnimator
     - Transition CharacterCombat to KnockedOutState
     - Raise OnDeath event
     - Disable character input/controls

6. **Public API:**
   - `float CurrentHealth { get; }` property
   - `float HealthPercentage { get; }` property (for UI)
   - `bool IsDead { get; }` property
   - `void Heal(float amount)` method (for future use)

**Verification Checklist:**
- [ ] CharacterHealth component created
- [ ] Damage calculation accounts for all multipliers
- [ ] Blocking reduces damage correctly (75% reduction)
- [ ] Hit reactions trigger at appropriate damage thresholds
- [ ] Death triggers knockout and disables character
- [ ] Health events fire correctly

**Testing Instructions:**

```csharp
// File: Assets/Knockout/Tests/PlayMode/Characters/CharacterHealthTests.cs
[Test]
public void TakeDamage_WithBlocking_ReducesDamageBy75Percent()
{
    var character = CreateTestCharacter();
    var health = character.GetComponent<CharacterHealth>();
    var combat = character.GetComponent<CharacterCombat>();

    combat.StartBlocking();
    health.TakeDamage(new HitData { Damage = 20f });

    Assert.AreEqual(95f, health.CurrentHealth); // 20 * 0.25 = 5 damage
}
```

**Commit Message Template:**
```
feat(combat): create CharacterHealth component

- Implement health tracking and damage application
- Calculate damage with blocking and multiplier modifiers
- Trigger appropriate hit reactions based on damage amount
- Handle knockout when health reaches zero
- Add health changed and death events for UI integration
- Add play mode tests for damage calculation

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 6: Create CharacterMovement Component

**Goal:** Implement locomotion system with movement input processing, rotation, and selective root motion application.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterMovement.cs`

**Prerequisites:**
- Phase 2 complete (animations set up)
- CharacterAnimator exists

**Implementation Steps:**

1. **Component structure:**
   - Reference to CharacterAnimator
   - Reference to Rigidbody (for physics-based movement)
   - Movement speed from CharacterStats
   - Current movement input (Vector2)
   - Facing direction

2. **Movement input processing:**
   - `SetMovementInput(Vector2 input)` method:
     - Store input vector
     - Calculate world-space movement direction
     - Update CharacterAnimator with movement direction and speed

3. **Apply movement:**
   - In FixedUpdate (for physics consistency):
     - Calculate velocity from input and move speed
     - Apply to Rigidbody: `rigidbody.velocity = new Vector3(moveVelocity.x, rigidbody.velocity.y, moveVelocity.z)`
     - Alternative: `transform.position += movement * Time.fixedDeltaTime` for non-physics movement

4. **Character rotation:**
   - `RotateToward(Vector3 target)` method for facing opponent:
     - Calculate direction to target
     - Smooth rotation using Quaternion.Slerp
     - Rotation speed from CharacterStats
   - Auto-rotate toward opponent during idle/combat

5. **Root motion handling:**
   - Implement `OnAnimatorMove()` callback:
     - If CharacterCombat is in AttackingState: Apply animator root motion (for attack momentum)
     - If in other states: Ignore animator root motion, use transform-based movement
   - This gives attacks realistic forward movement while keeping locomotion responsive

6. **Movement constraints:**
   - Check CharacterCombat state before allowing movement
   - Can move in: IdleState, BlockingState (reduced speed)
   - Cannot move in: AttackingState, HitStunnedState, KnockedDownState, KnockedOutState

**Verification Checklist:**
- [ ] CharacterMovement component created
- [ ] Movement input properly processed
- [ ] Character moves in world space correctly
- [ ] Rotation toward opponent works smoothly
- [ ] Root motion applied during attacks only
- [ ] Movement disabled during appropriate states

**Testing Instructions:**

```csharp
[UnityTest]
public IEnumerator CharacterMovement_ProcessesInput_MovesCharacter()
{
    var character = CreateTestCharacter();
    var movement = character.GetComponent<CharacterMovement>();
    Vector3 startPos = character.transform.position;

    movement.SetMovementInput(Vector2.up); // Move forward
    yield return new WaitForSeconds(0.5f);

    Assert.Greater(Vector3.Distance(startPos, character.transform.position), 0.1f);
}
```

**Commit Message Template:**
```
feat(characters): create CharacterMovement component

- Implement movement input processing and world-space movement
- Add character rotation toward opponent
- Implement selective root motion (attacks only)
- Integrate movement state checks with CharacterCombat
- Add FixedUpdate physics-based movement
- Add play mode tests for movement

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 7: Create CharacterInput Component

**Goal:** Connect Unity Input System to character components via event-driven architecture.

**Files to Create:**
- `Assets/Knockout/Scripts/Characters/Components/CharacterInput.cs`

**Prerequisites:**
- Phase 1 Task 5 complete (Input System configured)
- CharacterMovement and CharacterCombat exist

**Implementation Steps:**

1. **Component structure:**
   - Reference to KnockoutInputActions instance
   - Events: OnMoveInput(Vector2), OnJabPressed, OnHookPressed, OnUppercutPressed, OnBlockPressed, OnBlockReleased
   - Input enabled flag (for disabling during certain states)

2. **Initialize Input System:**
   - Awake: Create KnockoutInputActions instance
   - OnEnable: Enable input actions, subscribe to all input events
   - OnDisable: Unsubscribe from events, disable input actions

3. **Subscribe to input actions:**
   - Movement: `inputActions.Gameplay.Movement.performed += OnMovementInput`
   - Jab: `inputActions.Gameplay.Jab.performed += OnJabInput`
   - Hook: `inputActions.Gameplay.Hook.performed += OnHookInput`
   - Uppercut: `inputActions.Gameplay.Uppercut.performed += OnUppercutInput`
   - Block: `inputActions.Gameplay.Block.started/canceled += OnBlockInput`

4. **Raise component events:**
   - Input callbacks convert Input System events to component events
   - Example: `OnMovementInput(InputAction.CallbackContext ctx) => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>())`
   - Other components subscribe to these events, not directly to Input System

5. **Input enable/disable:**
   - `EnableInput()` / `DisableInput()` methods
   - Call DisableInput when character is knocked out or in certain states
   - Prevents input processing without unsubscribing from Input System

6. **Input validation:**
   - Check if input enabled before raising events
   - Log warnings if attempting to process input while disabled

**Verification Checklist:**
- [ ] CharacterInput component created
- [ ] All input actions properly subscribed
- [ ] Component events fire when inputs received
- [ ] Input can be enabled/disabled
- [ ] No direct dependencies on Input System in other components

**Testing Instructions:**

```csharp
[UnityTest]
public IEnumerator CharacterInput_JabPressed_RaisesEvent()
{
    var character = CreateTestCharacter();
    var input = character.GetComponent<CharacterInput>();

    bool jabPressed = false;
    input.OnJabPressed += () => jabPressed = true;

    // Simulate jab input (requires Input System test helpers)
    SimulateInput(input.InputActions.Gameplay.Jab);
    yield return null;

    Assert.IsTrue(jabPressed);
}
```

**Commit Message Template:**
```
feat(input): create CharacterInput component

- Implement Input System integration via event-driven architecture
- Subscribe to all gameplay inputs (movement, attacks, defense)
- Raise component events for other components to consume
- Add input enable/disable functionality
- Decouple Input System from combat/movement components
- Add play mode tests for input event flow

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

### Task 8: Integrate All Components

**Goal:** Wire all components together, configure character prefabs, and test the complete combat system.

**Files to Modify:**
- `Assets/Knockout/Scripts/Characters/CharacterController.cs`
- `Assets/Knockout/Prefabs/Characters/PlayerCharacter.prefab`
- `Assets/Knockout/Prefabs/Characters/AICharacter.prefab`

**Prerequisites:**
- Tasks 3-7 complete (all components exist)

**Implementation Steps:**

1. **Update CharacterController:**
   - Add private fields for all components:
     - `_characterAnimator`, `_characterInput`, `_characterMovement`, `_characterCombat`, `_characterHealth`
   - Cache all components in Awake via `GetComponent<T>()`
   - Add public properties to expose components if needed
   - Remove any component caching already done in Phase 1

2. **Wire up event connections:**

   **CharacterInput → CharacterMovement:**
   ```csharp
   _characterInput.OnMoveInput += _characterMovement.SetMovementInput;
   ```

   **CharacterInput → CharacterCombat:**
   ```csharp
   _characterInput.OnJabPressed += () => _characterCombat.ExecuteJab();
   _characterInput.OnHookPressed += () => _characterCombat.ExecuteHook();
   _characterInput.OnUppercutPressed += () => _characterCombat.ExecuteUppercut();
   _characterInput.OnBlockPressed += _characterCombat.StartBlocking;
   _characterInput.OnBlockReleased += _characterCombat.StopBlocking;
   ```

   **CharacterAnimator ← CharacterCombat:**
   - Already wired in CharacterCombat component

   **CharacterHealth → CharacterInput:**
   ```csharp
   _characterHealth.OnDeath += _characterInput.DisableInput;
   ```

3. **Add components to PlayerCharacter prefab:**
   - Open prefab in Unity
   - Add CharacterInput component (player-controlled)
   - Add CharacterMovement component
   - Add CharacterCombat component
   - Add CharacterHealth component
   - Assign CharacterStats reference to CharacterHealth
   - Save prefab

4. **Add components to AICharacter prefab:**
   - Open prefab in Unity
   - Do NOT add CharacterInput (AI will use CharacterAI in Phase 4)
   - Add CharacterMovement component
   - Add CharacterCombat component
   - Add CharacterHealth component
   - Assign CharacterStats reference
   - Save prefab

5. **Test full integration:**
   - Open GameplayTest scene
   - Place PlayerCharacter at PlayerSpawnPoint
   - Place AICharacter at AISpawnPoint (stationary for now)
   - Enter Play mode
   - Test: WASD movement, mouse clicks for attacks, Shift for block
   - Test: Attacks trigger animations, hitboxes activate, damage applies
   - Test: AI character takes damage and plays hit reactions
   - Test: Health reaches zero triggers knockout

**Verification Checklist:**
- [ ] All components wired together in CharacterController
- [ ] Event connections properly established
- [ ] PlayerCharacter prefab has all components
- [ ] AICharacter prefab has all components (except Input)
- [ ] Player can control character with keyboard/mouse
- [ ] Attacks damage opponent and trigger hit reactions
- [ ] Blocking reduces damage
- [ ] Knockout occurs at zero health
- [ ] No console errors during gameplay

**Testing Instructions:**
Full integration test - manual gameplay testing required. Create test scene with both characters and verify all systems work together.

**Commit Message Template:**
```
feat(integration): wire all character components together

- Update CharacterController to cache all component references
- Wire event connections between Input, Movement, Combat, Health
- Add all components to PlayerCharacter prefab
- Add components to AICharacter prefab (no Input for AI)
- Test full combat integration in GameplayTest scene
- Player can now fight stationary AI opponent

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## Phase Verification

**All Tasks Complete:**
- [ ] Hit detection data structures created
- [ ] Hitboxes and hurtboxes added to prefabs
- [ ] Combat state machine implemented
- [ ] CharacterCombat, CharacterHealth, CharacterMovement, CharacterInput components created
- [ ] All components integrated and wired up
- [ ] Player can move, attack, block using keyboard/mouse input
- [ ] Attacks trigger hitboxes, deal damage to opponent
- [ ] Damage reduces health, triggers hit reactions

**Integration Points:**
- [ ] Input System → CharacterInput → CharacterMovement/CharacterCombat
- [ ] CharacterCombat → CharacterAnimator → Animation system
- [ ] Hit detection → CharacterHealth → Hit reactions
- [ ] All components communicate via events (loose coupling)

**Tests Passing:**
- [ ] All EditMode and PlayMode tests pass
- [ ] Manual gameplay test: Player can fight stationary AI dummy

**Known Limitations:**
- AI opponent is stationary (Phase 4 will add AI)
- No camera follow (can add in Phase 5)
- Basic combat feel (Phase 5 will polish)

---

**Phase 3 Complete!** Proceed to [Phase 4: AI Opponent Foundation](Phase-4.md).

# Phase 0: Architecture & Design Foundation

**Purpose:** This document establishes architectural decisions, design patterns, and conventions that apply to all implementation phases. Read this thoroughly before starting any implementation work.

---

## Table of Contents

1. [Architecture Decisions](#architecture-decisions)
2. [Project Structure](#project-structure)
3. [Design Patterns](#design-patterns)
4. [Animation System Architecture](#animation-system-architecture)
5. [Combat System Design](#combat-system-design)
6. [AI System Architecture](#ai-system-architecture)
7. [Testing Strategy](#testing-strategy)
8. [Code Conventions](#code-conventions)
9. [Common Pitfalls](#common-pitfalls)

---

## Architecture Decisions

### ADR-001: Component-Based Character Controller

**Decision:** Use Unity's component-based architecture with single-responsibility components rather than monolithic character controller.

**Rationale:**
- Separation of concerns makes code testable and maintainable
- Components can be enabled/disabled for AI vs player characters
- Easier to extend with new behaviors (physics-based control later)
- Follows Unity best practices and Entity Component System principles

**Structure:**
```
CharacterController (coordinator, minimal logic)
├── CharacterAnimator (animation state management)
├── CharacterInput (player input handling)
├── CharacterMovement (locomotion logic)
├── CharacterCombat (attack/defense execution)
├── CharacterHealth (damage, knockouts)
└── CharacterAI (AI decision-making, replaces CharacterInput)
```

**Implications:**
- Each component communicates via events or direct references (held by coordinator)
- Components should not directly reference each other (loose coupling)
- Character state is managed by coordinator, accessed via properties

---

### ADR-002: Animation-Driven vs Root Motion

**Decision:** Use animation-driven movement with root motion for attacks, traditional transform movement for locomotion.

**Rationale:**
- **Root motion for attacks:** Ensures punches have realistic weight and forward momentum
- **Transform movement for locomotion:** Gives precise control for gameplay feel and AI pathfinding
- Hybrid approach balances realism with gameplay control
- Easier to tune gameplay parameters (speed, acceleration) separately from animations

**Implementation:**
- Animator root motion enabled, but selectively applied via `OnAnimatorMove()`
- Movement component decides when to apply root motion vs transform movement
- Attack animations include root motion, locomotion animations do not

---

### ADR-003: Animator State Machine Structure

**Decision:** Use layered Animator with separate layers for locomotion, upper body, and full-body actions.

**Rationale:**
- Allows simultaneous animations (move while blocking)
- Upper body layer can override for attacks/blocks while legs continue moving
- Full-body layer for knockdowns/knockouts that override everything
- Standard fighting game animation architecture

**Layers:**
1. **Base Layer (Locomotion):** Idle, walk, run, strafe
2. **Upper Body Layer (Attacks/Defense):** Punches, blocks - affects spine and above
3. **Full Body Override Layer (Reactions):** Hit reactions, knockdowns, knockouts - overrides all

**Layer Weights:**
- Base: Always 1.0
- Upper Body: 0.0 to 1.0 (controlled by combat state)
- Full Body Override: 0.0 or 1.0 (binary, takes full control when active)

---

### ADR-004: Input System Architecture

**Decision:** Use Unity's new Input System with Action Maps and binding separation.

**Rationale:**
- Already installed in project (manifest.json shows 1.4.4)
- Supports both player input and AI input via same interface
- Easy to rebind controls, add gamepad support later
- Event-driven callbacks reduce polling overhead

**Action Maps:**
- **Gameplay:** Movement (Vector2), Jab, Hook, Uppercut, Block, Dodge
- **UI:** Pause, Menu navigation (for future menus)

**Implementation Pattern:**
- CharacterInput component subscribes to Input Actions
- Raises events that CharacterCombat and CharacterMovement subscribe to
- AI can trigger same events, making character code input-source agnostic

---

### ADR-005: Hit Detection System

**Decision:** Use trigger colliders on hitboxes (hands during punch animations) + hurtboxes (head, body on characters).

**Rationale:**
- Precise control over hit timing and spacing
- Animations can enable/disable hitboxes at specific frames via Animation Events
- No raycasting needed, cleaner and more performant
- Standard fighting game approach

**Structure:**
- **Hitbox:** Trigger collider on attacking limb (disabled by default)
  - Enabled during attack's "active frames" via Animation Event
  - Contains HitboxData component (damage, knockback, hit type)
- **Hurtbox:** Trigger colliders on character (head, body, always active)
  - Contains HurtboxData component (damage multiplier, hit reaction type)
- **Hit Registration:** Hitbox OnTriggerEnter detects Hurtbox collision
  - Sends damage event to target CharacterHealth component
  - Triggers appropriate hit reaction animation

**Preventing Multiple Hits:**
- Each hitbox tracks hit targets in current attack (HashSet)
- Cleared when attack animation ends
- Prevents same punch hitting same opponent multiple times

---

### ADR-006: Combat State Management

**Decision:** Use explicit finite state machine for combat states, integrated with Animator.

**Rationale:**
- Combat has clear, mutually exclusive states (idle, attacking, blocking, hit-stunned, knocked down)
- State machine enforces valid transitions and prevents impossible actions
- Mirrors Animator state machine structure for consistency
- Easy to debug and visualize current state

**States:**
- **Idle:** Can move, attack, block, dodge
- **Attacking:** Cannot move, cannot attack again, vulnerable to hits (no active block)
- **Blocking:** Cannot attack, reduced movement, mitigates damage
- **Dodging:** Cannot attack, invulnerable for dodge duration
- **Hit Stunned:** Cannot act, brief recovery time
- **Knocked Down:** Cannot act, longer recovery
- **Knocked Out:** Cannot act, match over

**Transitions managed by:**
- CharacterCombat component (owns the state machine)
- Animator state machine mirrors this (via parameters set by CharacterCombat)

---

### ADR-007: AI Architecture Foundation

**Decision:** Start with simple Finite State Machine (FSM) for AI, design for future Behavior Tree or physics-based control.

**Rationale:**
- FSM is simple, predictable, sufficient for basic AI opponent
- Physics-based AI from Facebook paper is complex; needs research phase first
- Architecture allows swapping AI decision-making without changing character controller
- AI uses same CharacterCombat/Movement interfaces as player

**Initial AI States:**
- **Observe:** Watch player, maintain distance
- **Approach:** Close distance when player is far
- **Retreat:** Back away when player is too close or AI is low health
- **Attack:** Choose attack type based on range and pattern
- **Defend:** Block when player attacks or AI is low health

**Decision-Making:**
- Timer-based behavior switching (prevents predictable patterns)
- Range-based decisions (distance to player)
- Health-based aggression (more defensive when low health)
- Simple pattern variation (randomize attack types)

**Future Extension:**
- Behavior Tree library can replace FSM without changing character controller
- Physics-based control can replace scripted movement by swapping components

---

## Project Structure

```
Assets/
├── Knockout/                          # All game code and assets
│   ├── Animations/                    # Animation clips and controllers
│   │   ├── Characters/
│   │   │   └── BaseCharacter/
│   │   │       ├── AnimationClips/    # FBX animation clips
│   │   │       ├── AnimatorController.controller
│   │   │       └── AnimatorMasks/     # Avatar masks for layers
│   │   └── ...
│   │
│   ├── Materials/                     # Character and environment materials
│   │   └── Characters/
│   │
│   ├── Models/                        # Character models
│   │   └── Characters/
│   │       └── BaseCharacter/
│   │           ├── BaseCharacter.fbx
│   │           └── Textures/
│   │
│   ├── Prefabs/                       # Prefabs for characters and systems
│   │   ├── Characters/
│   │   │   ├── PlayerCharacter.prefab
│   │   │   └── AICharacter.prefab
│   │   └── Systems/
│   │       └── CombatSystem.prefab
│   │
│   ├── Scenes/                        # Unity scenes
│   │   ├── MainMenu.unity
│   │   ├── GameplayTest.unity         # Test scene for character development
│   │   └── ...
│   │
│   ├── Scripts/                       # All C# code
│   │   ├── Characters/                # Character-specific code
│   │   │   ├── CharacterController.cs
│   │   │   ├── Components/            # Character components
│   │   │   │   ├── CharacterAnimator.cs
│   │   │   │   ├── CharacterInput.cs
│   │   │   │   ├── CharacterMovement.cs
│   │   │   │   ├── CharacterCombat.cs
│   │   │   │   ├── CharacterHealth.cs
│   │   │   │   └── CharacterAI.cs
│   │   │   └── Data/                  # ScriptableObjects for character data
│   │   │       ├── CharacterStats.cs
│   │   │       └── AttackData.cs
│   │   │
│   │   ├── Combat/                    # Combat system code
│   │   │   ├── HitDetection/
│   │   │   │   ├── Hitbox.cs
│   │   │   │   ├── Hurtbox.cs
│   │   │   │   └── HitData.cs
│   │   │   ├── States/                # Combat state machine
│   │   │   │   ├── CombatState.cs (abstract)
│   │   │   │   ├── IdleState.cs
│   │   │   │   ├── AttackingState.cs
│   │   │   │   └── ...
│   │   │   └── CombatStateMachine.cs
│   │   │
│   │   ├── AI/                        # AI decision-making
│   │   │   ├── States/
│   │   │   │   ├── AIState.cs (abstract)
│   │   │   │   ├── ObserveState.cs
│   │   │   │   ├── AttackState.cs
│   │   │   │   └── ...
│   │   │   └── AIStateMachine.cs
│   │   │
│   │   ├── Input/                     # Input handling
│   │   │   └── InputActions.inputactions  # Input Action Asset
│   │   │
│   │   └── Utilities/                 # Helper classes
│   │       ├── Extensions/
│   │       └── Helpers/
│   │
│   └── Tests/                         # Unit and integration tests
│       ├── EditMode/                  # Edit mode tests (no runtime)
│       │   ├── Combat/
│       │   └── Characters/
│       └── PlayMode/                  # Play mode tests (runtime)
│           ├── Combat/
│           └── Characters/
│
└── Packages/                          # Unity packages (already configured)
```

**Naming Conventions:**
- **Folders:** PascalCase
- **Files:** Match class name exactly (PascalCase)
- **Unity Assets:** Descriptive names, PascalCase
- **Test Files:** `[ClassName]Tests.cs`

---

## Design Patterns

### 1. Component Pattern
Each character component handles one responsibility:
- **CharacterController:** Coordinator, holds references, minimal logic
- **CharacterAnimator:** Owns Animator, exposes animation methods
- **CharacterInput:** Subscribes to Input System, raises input events
- **CharacterMovement:** Handles locomotion, rotation, positioning
- **CharacterCombat:** Executes attacks/defense, manages combat state
- **CharacterHealth:** Tracks health, applies damage, triggers death
- **CharacterAI:** AI decision-making (replaces CharacterInput for AI)

### 2. State Pattern
Used for combat states and AI states:
```csharp
public abstract class CombatState
{
    public abstract void Enter(CharacterCombat combat);
    public abstract void Update(CharacterCombat combat);
    public abstract void Exit(CharacterCombat combat);
    public abstract bool CanTransitionTo(CombatState newState);
}
```

### 3. Event-Driven Communication
Components communicate via C# events to maintain loose coupling:
```csharp
// CharacterInput raises events
public event Action<Vector2> OnMoveInput;
public event Action OnJabPressed;

// CharacterCombat subscribes
void Start() {
    characterInput.OnJabPressed += HandleJab;
}
```

### 4. ScriptableObject Data
Combat and character data stored in ScriptableObjects:
- **CharacterStats:** Health, speed, damage multipliers
- **AttackData:** Damage, knockback, animation trigger, active frames

**Benefits:**
- Data changes don't require code changes
- Easy to create character variants
- Designer-friendly
- Testable (can create test data in tests)

### 5. Object Pooling (Future)
Not implemented in initial phases, but designed for:
- Hit effects, damage numbers (frequent instantiation)
- Architecture supports adding pooling later without refactoring

---

## Animation System Architecture

### Animator Parameters

**Parameters for Base Layer (Locomotion):**
- `MoveSpeed` (float): 0 = idle, 0.1-0.5 = walk, 0.5-1.0 = run
- `MoveDirectionX` (float): -1 = left, 1 = right (strafe)
- `MoveDirectionY` (float): -1 = back, 1 = forward

**Parameters for Upper Body Layer (Attacks/Defense):**
- `AttackTrigger` (trigger): Initiates attack
- `AttackType` (int): 0 = jab, 1 = hook, 2 = uppercut
- `IsBlocking` (bool): True when blocking
- `UpperBodyWeight` (float): Blend weight for upper body layer

**Parameters for Full Body Override Layer:**
- `HitReaction` (trigger): Triggers hit reaction
- `HitType` (int): 0 = light (head), 1 = medium (body), 2 = heavy
- `KnockedDown` (bool): True when knocked down
- `KnockedOut` (bool): True when knocked out
- `OverrideWeight` (float): Blend weight for full body override

### Animation Events

Used to synchronize gameplay with animations:

**Attack Animations:**
- `OnAttackStart()`: Called at first frame of attack
- `OnHitboxActivate()`: Enable hitbox collider (active frames begin)
- `OnHitboxDeactivate()`: Disable hitbox collider (active frames end)
- `OnAttackRecoveryStart()`: Vulnerable but can't act yet
- `OnAttackEnd()`: Attack complete, can act again

**Hit Reaction Animations:**
- `OnHitReactionEnd()`: Hit stun over, can act again

**Knockdown Animations:**
- `OnKnockedDownComplete()`: Knockdown animation done, can get up
- `OnGetUpComplete()`: Back to idle state

**Implementation:**
- CharacterAnimator component receives animation events
- Raises C# events that CharacterCombat subscribes to
- CharacterCombat updates combat state machine accordingly

### State Machine Structure

**Base Layer (Locomotion):**
```
Idle
├─> Walk (blend tree: forward, back, left, right)
├─> Run (blend tree: forward, back, left, right)
└─> Idle
```

**Upper Body Layer (Additive/Override from spine up):**
```
Empty (idle upper body)
├─> JabLeft ──> Recovery ──> Empty
├─> JabRight ──> Recovery ──> Empty
├─> HookLeft ──> Recovery ──> Empty
├─> HookRight ──> Recovery ──> Empty
├─> UppercutLeft ──> Recovery ──> Empty
├─> UppercutRight ──> Recovery ──> Empty
└─> Block ──> Empty
```

**Full Body Override Layer (Override all when active):**
```
Empty
├─> HitReactionLight ──> Empty
├─> HitReactionMedium ──> Empty
├─> HitReactionHeavy ──> Empty
├─> KnockedDown ──> GetUp ──> Empty
└─> KnockedOut (no exit)
```

---

## Combat System Design

### Damage Calculation

```
BaseDamage = AttackData.Damage
CharacterModifier = CharacterStats.DamageMultiplier
HurtboxModifier = HurtboxData.DamageMultiplier (e.g., head = 1.5x, body = 1.0x)
BlockModifier = IsBlocking ? 0.25 : 1.0

FinalDamage = BaseDamage * CharacterModifier * HurtboxModifier * BlockModifier
```

### Hit Reaction Selection

Based on `FinalDamage` and `HitType`:
- **Light:** Damage < 15, brief head snap or body flinch
- **Medium:** Damage 15-30, stagger back, longer stun
- **Heavy:** Damage > 30, knockback, potential knockdown
- **Knockdown:** Heavy + low health, or specific attack property
- **Knockout:** Health reaches 0

### Combat Frame Data (Timing)

Each attack has frame timings (at 60fps):
- **Startup:** Frames before hitbox activates (cannot cancel, vulnerable)
- **Active:** Frames where hitbox is active (can hit opponent)
- **Recovery:** Frames after hitbox deactivates (vulnerable, cannot act)
- **Total Frames:** Startup + Active + Recovery

**Example:**
- Jab: 6 startup, 3 active, 6 recovery = 15 frames total (~0.25s)
- Hook: 10 startup, 4 active, 12 recovery = 26 frames total (~0.43s)
- Uppercut: 15 startup, 5 active, 18 recovery = 38 frames total (~0.63s)

**Frame data stored in AttackData ScriptableObject.**

---

## AI System Architecture

### AI Decision-Making Loop

```
1. Sense environment (distance to player, player state, own health)
2. Evaluate current state (should I continue or transition?)
3. Execute state behavior (move, attack, defend)
4. Repeat at fixed interval (e.g., 0.1s)
```

### AI State Transitions (Initial FSM)

```
Observe
├─> Approach (if player distance > 3.0)
├─> Retreat (if own health < 30% OR player attacking)
├─> Attack (if player distance < 2.5 AND not blocking)
└─> Defend (if player is attacking AND distance < 3.0)

Approach
├─> Observe (if reached optimal distance ~2.0)
├─> Attack (if close enough < 2.5)
└─> Defend (if player attacks)

Attack
├─> Observe (after attack recovery)
├─> Retreat (if hit during attack)
└─> Defend (if player counter-attacks)

Defend
├─> Observe (after threat passes)
├─> Retreat (if health critical < 20%)
└─> Attack (if player vulnerable)

Retreat
├─> Observe (if reached safe distance > 4.0)
└─> Defend (if player pursues)
```

### AI Attack Selection

Based on distance:
- **Close range (< 1.5):** Uppercut (high damage, slow)
- **Medium range (1.5-2.5):** Hook or Jab (mix randomly)
- **Far range (> 2.5):** Approach (no attacks)

Add randomization to prevent predictability (70% optimal choice, 30% random).

---

## Testing Strategy

### Test Pyramid

1. **Unit Tests (70%):** Test individual components in isolation
   - Character components (CharacterMovement, CharacterCombat, etc.)
   - Combat state machine transitions
   - AI state machine logic
   - Damage calculation
   - Hit detection logic

2. **Integration Tests (25%):** Test component interactions
   - Input → Movement → Animation
   - Attack → Hit Detection → Damage → Health
   - AI → Combat → Animation

3. **Manual/Playtesting (5%):** Full gameplay testing
   - Feel, balance, responsiveness
   - Edge cases discovered during play

### Test Structure

**Edit Mode Tests (No Unity runtime needed):**
- State machine logic
- Damage calculations
- Data validation
- Utility functions

**Play Mode Tests (Require Unity runtime):**
- Component interactions
- Animation state changes
- Physics/collision
- Input handling

### Test Naming Convention

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

**Example:**
```csharp
[Test]
public void ApplyDamage_WhenBlocking_ReducesDamageBySeventyFivePercent()
{
    // Arrange
    var health = CreateCharacterHealth(100);
    var attackData = CreateAttackData(damage: 20);
    health.SetBlocking(true);

    // Act
    health.ApplyDamage(attackData);

    // Assert
    Assert.AreEqual(95, health.CurrentHealth); // 20 * 0.25 = 5 damage
}
```

### Test Coverage Goals

- **Critical paths:** 100% (damage, hit detection, state transitions)
- **Component logic:** 80%+
- **UI/Visual feedback:** Manual testing (not unit tested)

### Mocking Strategy

- Use interfaces for dependencies (ICharacterInput, ICharacterAnimator)
- Mock Unity components in edit mode tests
- Use test doubles for ScriptableObject data

---

## Code Conventions

### Naming

**Classes and Structs:** PascalCase
```csharp
public class CharacterController { }
public struct HitData { }
```

**Methods:** PascalCase
```csharp
public void ApplyDamage(float damage) { }
```

**Properties:** PascalCase
```csharp
public float CurrentHealth { get; private set; }
```

**Fields (private):** camelCase with underscore prefix
```csharp
private float _currentHealth;
private CharacterAnimator _animator;
```

**Fields (public/serialized):** camelCase (Unity convention)
```csharp
[SerializeField] private float moveSpeed;
public float attackDamage;
```

**Constants:** SCREAMING_SNAKE_CASE
```csharp
private const float MAX_HEALTH = 100f;
```

**Events:** PascalCase with "On" prefix
```csharp
public event Action OnDeath;
public event Action<float> OnHealthChanged;
```

### File Organization

```csharp
// 1. Using statements (grouped: System, Unity, Project)
using System;
using System.Collections.Generic;
using UnityEngine;
using Knockout.Combat;

// 2. Namespace
namespace Knockout.Characters
{
    // 3. Class documentation
    /// <summary>
    /// Manages character health, damage application, and death.
    /// </summary>
    public class CharacterHealth : MonoBehaviour
    {
        // 4. Serialized fields
        [SerializeField] private float maxHealth = 100f;

        // 5. Public properties
        public float CurrentHealth { get; private set; }

        // 6. Events
        public event Action OnDeath;

        // 7. Private fields
        private bool _isDead;

        // 8. Unity lifecycle methods
        private void Awake() { }
        private void Start() { }

        // 9. Public methods
        public void ApplyDamage(float damage) { }

        // 10. Private methods
        private void Die() { }
    }
}
```

### Comments

- **XML documentation** for public classes, methods, properties
- **Inline comments** for complex logic only (code should be self-documenting)
- **TODO comments** with issue tracking reference

```csharp
/// <summary>
/// Applies damage to the character, accounting for blocking and damage multipliers.
/// </summary>
/// <param name="damage">Base damage amount.</param>
/// <param name="isBlocking">Whether the character is currently blocking.</param>
public void ApplyDamage(float damage, bool isBlocking)
{
    // Blocking reduces damage by 75%
    float finalDamage = isBlocking ? damage * 0.25f : damage;

    CurrentHealth -= finalDamage;

    // TODO: Add damage number visualization (Issue #23)
}
```

### Serialization

- Prefer `[SerializeField] private` over `public` for inspector fields
- Use `[Header]` to organize inspector sections
- Use `[Tooltip]` for designer guidance
- Use `[Range]` for bounded values

```csharp
[Header("Health Settings")]
[SerializeField] [Tooltip("Maximum health points")]
private float maxHealth = 100f;

[Header("Movement Settings")]
[SerializeField] [Range(1f, 10f)] [Tooltip("Movement speed multiplier")]
private float moveSpeed = 5f;
```

---

## Common Pitfalls

### 1. Animator State Timing Issues

**Problem:** Animation events fire before/after expected, causing state desync.

**Solution:**
- Always use normalized time for animation events (0.0 to 1.0)
- Place hitbox activation at 0.4-0.6 of animation (mid-swing)
- Test events in isolation before integrating with state machine
- Use Animation Event Debugger to verify timing

### 2. Hitbox Persistent Activation

**Problem:** Hitbox remains active after attack ends, hitting multiple times.

**Solution:**
- Always pair `OnHitboxActivate` with `OnHitboxDeactivate` events
- Disable hitboxes in `OnAttackEnd` as failsafe
- Use HashSet to track already-hit targets per attack
- Clear hit tracking when attack state exits

### 3. Combat State Race Conditions

**Problem:** Multiple state transitions in one frame cause undefined behavior.

**Solution:**
- Validate transitions with `CanTransitionTo()` check
- Process state changes at end of frame (coroutine or late update)
- Use state queue if multiple transitions occur
- Log warnings when invalid transitions attempted

### 4. Input Buffer Overflow

**Problem:** Player mashes buttons, queues too many inputs, character is unresponsive.

**Solution:**
- Limit input buffer to 2-3 inputs max
- Clear buffer when hit or knocked down
- Prioritize defensive inputs (block, dodge) over attacks
- Implement input timeout (clear after 0.5s if not used)

### 5. Animation Layer Weight Conflicts

**Problem:** Multiple layers fight for control, animations blend incorrectly.

**Solution:**
- Full Body Override layer should always be 0.0 or 1.0 (binary)
- Upper Body layer weight should transition smoothly (lerp over time)
- Reset layer weights when returning to idle
- Use Animation Layer debugging to visualize weights

### 6. AI Infinite Loop States

**Problem:** AI gets stuck in state loop (e.g., Approach → Retreat → Approach).

**Solution:**
- Add state minimum duration (e.g., stay in state for at least 1 second)
- Add transition cooldown (can't return to previous state for X seconds)
- Use hysteresis for distance checks (different thresholds for enter/exit)
- Log state changes for debugging

### 7. Root Motion Sliding

**Problem:** Character slides on ground during root motion attacks.

**Solution:**
- Ensure attack animations have proper root motion baked
- Use `OnAnimatorMove()` to selectively apply root motion
- Apply only XZ components (horizontal), ignore Y (vertical)
- Blend root motion with character controller movement smoothly

### 8. Physics Jitter with Animator

**Problem:** Character jitters when Animator and Rigidbody interact.

**Rationale:**
- Set Animator Update Mode to "Animate Physics" if using Rigidbody
- Apply movement in `FixedUpdate` for physics-based characters
- Keep trigger colliders on separate GameObjects (not on Rigidbody)
- Use Kinematic Rigidbody for animated characters, Dynamic for physics-driven

### 9. Test Flakiness

**Problem:** Tests pass/fail randomly due to timing or Unity lifecycle issues.

**Solution:**
- Use `yield return null` in PlayMode tests to wait for frame
- Don't rely on exact timing (use ranges or thresholds)
- Reset all Unity state between tests (destroy objects)
- Use `[UnitySetUp]` and `[UnityTearDown]` properly

### 10. ScriptableObject Instance Sharing

**Problem:** Tests modify ScriptableObject data, affecting other tests or runtime.

**Solution:**
- Use `ScriptableObject.CreateInstance<T>()` in tests (not asset references)
- Clone data before modifying in tests
- Reset ScriptableObject data in `[TearDown]`
- Never modify ScriptableObject assets at runtime (copy to instance first)

---

## Future Considerations

### Physics-Based AI Integration

When implementing the Facebook Research paper's physics-based control:

1. **Parallel Development:** Build physics-based character controller alongside animated controller
2. **Shared Interface:** Both controllers implement same interface (ICharacterController)
3. **Character Swap:** CharacterController coordinator can swap components at runtime
4. **Animation Matching:** Physics controller can reference animations for target poses
5. **Hybrid Approach:** Use animated for player, physics-based for AI

**Do not over-engineer for this now.** Current architecture supports adding it later.

### Multiplayer Networking

While not planned initially, architecture supports future networking:

- **Deterministic Combat:** Frame-based combat can be made deterministic
- **Input-Based Networking:** Input events can be serialized and replayed
- **State Synchronization:** Combat states can be networked
- **Prediction:** State machine allows client-side prediction

**Do not add networking hooks now.** YAGNI principle applies.

### Character Roster Expansion

When adding multiple political characters:

1. **Shared Skeleton:** All characters use same rig (humanoid Unity rig)
2. **Shared Animations:** Base animations work on all characters
3. **Character-Specific Animations:** Override specific attacks with unique animations
4. **Stats Variation:** Each character has own CharacterStats ScriptableObject
5. **Visual Customization:** Different materials, models, voice lines

Current architecture supports this through ScriptableObject data-driven design.

---

**Read this document thoroughly before starting Phase 1.** Refer back when making architectural decisions during implementation.
